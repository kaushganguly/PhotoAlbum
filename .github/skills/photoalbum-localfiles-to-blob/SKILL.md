---
name: photoalbum-localfiles-to-blob
description: Replace local file system photo storage (wwwroot/uploads via PhotoService and IPhotoService) with Azure Blob Storage using Azure.Storage.Blobs and Microsoft Entra Managed Identity (DefaultAzureCredential). Use no storage account keys or connection strings.
---

## Overview

PhotoAlbum persists uploaded images to the local file system under `wwwroot/uploads`. `PhotoService.UploadPhotoAsync`
creates the directory, writes the file with a `FileStream`, and `DeletePhotoAsync` removes it with `File.Delete`.
Local disk does not work for scaled-out or containerized hosting on Azure.

This skill migrates photo storage to **Azure Blob Storage** with **passwordless** authentication via
`DefaultAzureCredential`. It is an organization standard:

- **Never** use storage account keys, SAS, or connection strings — always Microsoft Entra Managed Identity.
- **Preserve** the existing `IPhotoService` contract and the SixLabors.ImageSharp dimension extraction.
- **Keep** the EF Core metadata persistence and the delete-file-on-DB-failure rollback behavior.

## Steps

1. Add the Azure SDK packages `Azure.Storage.Blobs` and `Azure.Identity` to `PhotoAlbum/PhotoAlbum.csproj`.

2. Add configuration for the storage endpoint and container (no secrets):
   - `Storage:BlobEndpoint` = `https://<account>.blob.core.windows.net`
   - `Storage:ContainerName` = `photos`
   Remove reliance on `FileUpload:UploadPath` for persistence (keep `MaxFileSizeBytes` and `AllowedMimeTypes`
   validation — that logic stays).

3. Register a singleton `BlobServiceClient` in `Program.cs` using `DefaultAzureCredential` and the blob endpoint
   URI from configuration. Do not pass a connection string or account key.

4. Reimplement `PhotoService` against `BlobContainerClient` / `BlobClient`:
   - **Upload**: stream `file.OpenReadStream()` directly to a blob named `{Guid}{extension}`; set
     `BlobHttpHeaders.ContentType` from `file.ContentType`. Remove `Directory.CreateDirectory`, `Path.Combine`,
     and `FileStream` disk writes.
   - **Dimensions**: keep extracting width/height with ImageSharp before upload (buffer the stream once so it can
     be read for both dimension extraction and upload).
   - **Persistence**: store the blob name in `Photo.StoredFileName` and the blob URL in `Photo.FilePath`.
   - **Delete**: replace `File.Delete` with `BlobClient.DeleteIfExistsAsync`. Keep the existing rollback that
     deletes the blob if the database save fails.

5. Do not change the `IPhotoService` signatures (`GetAllPhotosAsync`, `GetPhotoByIdAsync`, `UploadPhotoAsync`,
   `DeletePhotoAsync`) or the EF Core `Photo` model / `PhotoAlbumContext`.

6. In `Program.cs`, remove the `wwwroot/uploads` directory creation and any static-file serving that depends on it
   (keep static-file serving for other assets). Serve images via the blob URL stored in `Photo.FilePath`.

7. Infrastructure note: grant the app's Managed Identity the **Storage Blob Data Contributor** role on the storage
   account so `DefaultAzureCredential` can read/write blobs.

## Sample code

### Before (local disk)

```csharp
// PhotoService.UploadPhotoAsync
var fullPath = Path.Combine(_uploadPath, storedFileName);
if (!Directory.Exists(_uploadPath))
{
    Directory.CreateDirectory(_uploadPath);
}
using var stream = new FileStream(fullPath, FileMode.Create);
await file.CopyToAsync(stream);

// PhotoService.DeletePhotoAsync
var fullPath = Path.Combine(_uploadPath, photo.StoredFileName);
if (File.Exists(fullPath))
{
    File.Delete(fullPath);
}
```

### After (Azure Blob Storage, passwordless)

```csharp
// Program.cs — register BlobServiceClient with Managed Identity (no keys/connection strings)
builder.Services.AddSingleton(_ =>
    new BlobServiceClient(
        new Uri(builder.Configuration["Storage:BlobEndpoint"]!),
        new DefaultAzureCredential()));

// PhotoService.UploadPhotoAsync
var container = _blobServiceClient.GetBlobContainerClient(_containerName);
await container.CreateIfNotExistsAsync();

var blob = container.GetBlobClient(storedFileName);
await blob.UploadAsync(
    file.OpenReadStream(),
    new BlobUploadOptions
    {
        HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
    });

photo.StoredFileName = storedFileName;
photo.FilePath = blob.Uri.ToString();

// PhotoService.DeletePhotoAsync
await container.GetBlobClient(photo.StoredFileName).DeleteIfExistsAsync();
```

## Dependency changes

- Add `Azure.Storage.Blobs` (12.x).
- Add `Azure.Identity` (1.x).
- No new secret-handling packages; no storage keys or connection strings introduced.

## Verification checks

- The solution builds and the existing xUnit tests (`PhotoAlbum.Tests/Unit/Services/PhotoServiceTests.cs`) pass.
- Uploaded images appear as blobs in the configured container; **no** files are written to `wwwroot/uploads`.
- Deleting a photo removes the blob via `DeleteIfExistsAsync`; the DB-failure rollback still deletes the blob.
- `Photo.Width` / `Photo.Height` are still populated by ImageSharp after upload.
- No storage account keys, SAS tokens, or connection strings exist in configuration or source; authentication is
  `DefaultAzureCredential` only.
