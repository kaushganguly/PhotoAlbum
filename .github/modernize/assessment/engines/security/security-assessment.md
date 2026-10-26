# Security Assessment Report

**Generated:** 2026-10-26T23:08:45.301487Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 13 |
| CVE Vulnerabilities | 6 |
| CWE Vulnerabilities | 7 |
| Total Rules Assessed | 59 |
| Rules Passed | 52 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 5 |
| optional | 6 |
| potential | 2 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w) — ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16)

Recommended fix:
  - Upgrade SixLabors.ImageSharp to 3.2.0 or later

### CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106116](https://github.com/advisories/GHSA-wmxv-xphr-5c9g) — ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop

Severity: MEDIUM

Affected dependencies:
  - SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16)

Recommended fix:
  - Upgrade SixLabors.ImageSharp to 3.2.0 or later

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q) — ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16)

Recommended fix:
  - Upgrade SixLabors.ImageSharp to 3.2.0 or later

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4) — ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16)

Recommended fix:
  - Upgrade SixLabors.ImageSharp to 3.2.0 or later

### CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106114](https://github.com/advisories/GHSA-gwg2-r3hj-4w44) — ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions

Severity: MEDIUM

Affected dependencies:
  - SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16)

Recommended fix:
  - Upgrade SixLabors.ImageSharp to 3.2.0 or later

### CVE-2026-106118: ImageSharp: Tiled fax TIFF: tile buffer sized by TileWidth but fax decompressor writes scanlines of ImageWidth — heap OOB write
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106118](https://github.com/advisories/GHSA-v76p-62qx-wwq2) — ImageSharp: Tiled fax TIFF: tile buffer sized by TileWidth but fax decompressor writes scanlines of ImageWidth — heap OOB write

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16)

Recommended fix:
  - Upgrade SixLabors.ImageSharp to 3.2.0 or later

## CWE Findings (Code-Level Vulnerabilities)

### CWE-606: Unchecked Input for Loop Condition
- **Category:** Code Quality
- **Severity:** potential
- **Story Points:** 3
- **Files:** PhotoAlbum/Pages/Index.cshtml.cs

IndexModel.OnPostUploadAsync iterates directly over the caller-supplied multipart file collection. Although a MaxFilesPerUpload value of 10 exists in configuration, the handler does not enforce it. A request can submit many small files, causing repeated validation, image processing, filesystem operations, and database calls in the loop; the overall request-size limit does not impose the configured file-count bound.

### CWE-772: Missing Release of Resource after Effective Lifetime
- **Category:** Code Quality
- **Severity:** potential
- **Story Points:** 3
- **Files:** PhotoAlbum/Services/PhotoService.cs

PhotoService.UploadPhotoAsync opens a stream with file.OpenReadStream() and passes it to Image.LoadAsync without disposing the stream. It then opens a second stream only to reset its position and also leaves that stream undisposed. These caller-created disposable stream resources are not released at their effective lifetime in the method.

### CWE-259: Use of Hard-coded Password
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep

The infrastructure template defines the SQL administrator password from a source-controlled expression containing a fixed password prefix combined with a deterministic uniqueString value. This supplies a credential used for SQL administrator authentication rather than obtaining the password from a secret provider.

### CWE-732: Incorrect Permission Assignment for Critical Resource
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep

The Container Registry is configured with its administrative user enabled even though the Container App is separately configured to pull using its system-assigned managed identity (lines 162-170). The enabled registry admin account provides a broad registry-level credential path in addition to the scoped identity access.

### CWE-798: Use of Hard-coded Credentials
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep

The SQL Server administrator credential is defined in source-controlled infrastructure as a deterministic expression with a fixed password prefix, then passed as administratorLoginPassword. The credential is not retrieved from a dedicated secret store.

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:82, PhotoAlbum/Services/PhotoService.cs:110, PhotoAlbum/Services/PhotoService.cs:125-144

PhotoService.UploadPhotoAsync checks the caller-controlled file.ContentType against an image MIME allowlist (line 82), but preserves the caller-provided filename extension in the generated stored name (line 110). ImageSharp decode failures are caught and ignored, after which the original bytes are still written beneath the public wwwroot/uploads directory (lines 125-144). A multipart request can therefore claim an allowed image MIME type while providing non-image content and a dangerous extension such as .html, which can be served as active content.

### CWE-79: Improper Neutralization of Input During Web Page Generation ('Cross-site Scripting')
- **Category:** Injection Attacks
- **Severity:** optional
- **Story Points:** 8
- **Files:** PhotoAlbum/wwwroot/js/upload.js:189-198, PhotoAlbum/wwwroot/js/upload.js:223-226

createPhotoCard interpolates photo.originalFileName, which originates from the uploaded file name, directly into HTML attributes and text (lines 193, 196-197); the resulting string is inserted with insertAdjacentHTML (lines 164-166). showErrors also interpolates server-returned error strings, including the user-supplied filename, into innerHTML (lines 223-226). These paths do not HTML-escape the untrusted filename before rendering it to other users.
