using Microsoft.EntityFrameworkCore;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.WebP;
using MetadataExtractor.Util;
using PhotoAlbum.Data;
using PhotoAlbum.Models;

namespace PhotoAlbum.Services;

/// <summary>
/// Service for photo operations including upload, retrieval, and deletion
/// </summary>
public class PhotoService : IPhotoService
{
    private readonly PhotoAlbumContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PhotoService> _logger;
    private readonly string _uploadPath;
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedMimeTypes;

    public PhotoService(
        PhotoAlbumContext context,
        IConfiguration configuration,
        ILogger<PhotoService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;

        _uploadPath = _configuration["FileUpload:UploadPath"] ?? "wwwroot/uploads";
        _maxFileSizeBytes = _configuration.GetValue<long>("FileUpload:MaxFileSizeBytes", 10485760);
        _allowedMimeTypes = _configuration.GetSection("FileUpload:AllowedMimeTypes").Get<string[]>()
            ?? new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
    }

    /// <summary>
    /// Get all photos ordered by upload date (newest first)
    /// </summary>
    public async Task<List<Photo>> GetAllPhotosAsync()
    {
        try
        {
            return await _context.Photos
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving photos from database");
            throw;
        }
    }

    /// <summary>
    /// Get a specific photo by ID
    /// </summary>
    public async Task<Photo?> GetPhotoByIdAsync(int id)
    {
        try
        {
            return await _context.Photos.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving photo with ID {PhotoId}", id);
            throw;
        }
    }

    /// <summary>
    /// Upload a photo file
    /// </summary>
    public async Task<UploadResult> UploadPhotoAsync(IFormFile file)
    {
        var result = new UploadResult
        {
            FileName = file.FileName
        };

        try
        {
            // Validate file type
            if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                result.Success = false;
                result.ErrorMessage = $"File type not supported. Please upload JPEG, PNG, GIF, or WebP images.";
                _logger.LogWarning("Upload rejected: Invalid file type {ContentType} for {FileName}",
                    file.ContentType, file.FileName);
                return result;
            }

            // Validate file size
            if (file.Length > _maxFileSizeBytes)
            {
                result.Success = false;
                result.ErrorMessage = $"File size exceeds {_maxFileSizeBytes / 1024 / 1024}MB limit.";
                _logger.LogWarning("Upload rejected: File size {FileSize} exceeds limit for {FileName}",
                    file.Length, file.FileName);
                return result;
            }

            // Validate file length
            if (file.Length <= 0)
            {
                result.Success = false;
                result.ErrorMessage = "File is empty.";
                return result;
            }

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var relativePath = $"/uploads/{storedFileName}";

            // Ensure upload directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }

            var fullPath = Path.Combine(_uploadPath, storedFileName);

            using var imageStream = file.OpenReadStream();
            var fileType = FileType.Unknown;
            try
            {
                fileType = FileTypeDetector.DetectFileType(imageStream);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not detect image format for {FileName}", file.FileName);
            }

            if (fileType != FileType.Unknown
                && !string.Equals(fileType.GetMimeType(), file.ContentType, StringComparison.OrdinalIgnoreCase))
            {
                result.Success = false;
                result.ErrorMessage = "File content does not match the specified image type.";
                return result;
            }

            imageStream.Position = 0;

            // Extract image dimensions from image metadata
            int? width = null;
            int? height = null;
            try
            {
                if (fileType is FileType.Jpeg or FileType.Png or FileType.Gif or FileType.WebP)
                {
                    var dimensions = GetImageDimensions(imageStream);
                    width = dimensions.Width;
                    height = dimensions.Height;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not extract image dimensions for {FileName}", file.FileName);
                // Continue without dimensions - not critical
            }

            // Save file to disk
            try
            {
                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file {FileName} to {FullPath}", file.FileName, fullPath);
                result.Success = false;
                result.ErrorMessage = "Error saving file. Please try again.";
                return result;
            }

            // Create photo entity
            var photo = new Photo
            {
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                FilePath = relativePath,
                FileSize = file.Length,
                MimeType = file.ContentType,
                UploadedAt = DateTime.UtcNow,
                Width = width,
                Height = height
            };

            // Save to database
            try
            {
                await _context.Photos.AddAsync(photo);
                await _context.SaveChangesAsync();

                result.Success = true;
                result.PhotoId = photo.Id;

                _logger.LogInformation("Successfully uploaded photo {FileName} with ID {PhotoId}",
                    file.FileName, photo.Id);
            }
            catch (Exception ex)
            {
                // Rollback: Delete file if database save fails
                try
                {
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
                catch (Exception deleteEx)
                {
                    _logger.LogError(deleteEx, "Error deleting file {FullPath} during rollback", fullPath);
                }

                _logger.LogError(ex, "Error saving photo metadata to database for {FileName}", file.FileName);
                result.Success = false;
                result.ErrorMessage = "Error saving photo information. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during photo upload for {FileName}", file.FileName);
            result.Success = false;
            result.ErrorMessage = "An unexpected error occurred. Please try again.";
        }

        return result;
    }

    private static (int? Width, int? Height) GetImageDimensions(Stream imageStream)
    {
        var directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(imageStream);

        var jpeg = directories.OfType<JpegDirectory>().FirstOrDefault();
        if (jpeg is not null)
        {
            return (jpeg.GetImageWidth(), jpeg.GetImageHeight());
        }

        var png = directories.OfType<PngDirectory>()
            .FirstOrDefault(directory =>
                directory.ContainsTag(PngDirectory.TagImageWidth)
                && directory.ContainsTag(PngDirectory.TagImageHeight));
        if (png is not null)
        {
            return (
                Convert.ToInt32(png.GetObject(PngDirectory.TagImageWidth)),
                Convert.ToInt32(png.GetObject(PngDirectory.TagImageHeight)));
        }

        var gif = directories.OfType<GifHeaderDirectory>().FirstOrDefault();
        if (gif is not null)
        {
            return (
                Convert.ToInt32(gif.GetObject(GifHeaderDirectory.TagImageWidth)),
                Convert.ToInt32(gif.GetObject(GifHeaderDirectory.TagImageHeight)));
        }

        var webP = directories.OfType<WebPDirectory>().FirstOrDefault();
        if (webP is not null)
        {
            return (
                Convert.ToInt32(webP.GetObject(WebPDirectory.TagImageWidth)),
                Convert.ToInt32(webP.GetObject(WebPDirectory.TagImageHeight)));
        }

        return (null, null);
    }

    /// <summary>
    /// Delete a photo by ID
    /// </summary>
    public async Task<bool> DeletePhotoAsync(int id)
    {
        try
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                _logger.LogWarning("Photo with ID {PhotoId} not found for deletion", id);
                return false;
            }

            // Delete file from disk
            var fullPath = Path.Combine(_uploadPath, photo.StoredFileName);
            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FullPath} for photo ID {PhotoId}", fullPath, id);
                // Continue with database deletion even if file deletion fails
            }

            // Delete from database
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted photo ID {PhotoId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo with ID {PhotoId}", id);
            throw;
        }
    }
}
