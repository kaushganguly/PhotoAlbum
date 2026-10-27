# Security Assessment Report

**Generated:** 2026-10-27T02:20:30.174Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 10 |
| CVE Vulnerabilities | 6 |
| CWE Vulnerabilities | 4 |
| Total Rules Assessed | 59 |
| Rules Passed | 55 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 5 |
| optional | 5 |
| potential | 0 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q): ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 2.0.0, < 3.2.0); first patched version 3.2.0
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 4.0.0, < 4.1.2); first patched version 4.1.2

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4): ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 2.0.0, < 3.2.0); first patched version 3.2.0
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 4.0.0, < 4.1.2); first patched version 4.1.2

### CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106114](https://github.com/advisories/GHSA-gwg2-r3hj-4w44): ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions

Severity: MEDIUM

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 4.0.0, < 4.1.2); first patched version 4.1.2
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 1.0.0-beta0001, < 3.2.0); first patched version 3.2.0

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w): ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 2.1.0, < 3.2.0); first patched version 3.2.0
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 4.0.0, < 4.1.2); first patched version 4.1.2

### CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106116](https://github.com/advisories/GHSA-wmxv-xphr-5c9g): ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop

Severity: MEDIUM

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 2.0.0, < 3.2.0); first patched version 3.2.0
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 4.0.0, < 4.1.2); first patched version 4.1.2

### CVE-2026-106118: ImageSharp: Tiled fax TIFF: tile buffer sized by TileWidth but fax decompressor writes scanlines of ImageWidth — heap OOB write
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106118](https://github.com/advisories/GHSA-v76p-62qx-wwq2): ImageSharp: Tiled fax TIFF: tile buffer sized by TileWidth but fax decompressor writes scanlines of ImageWidth — heap OOB write

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 3.0.0, < 3.2.0); first patched version 3.2.0
  - SixLabors.ImageSharp:3.1.11 (direct dependency, declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 4.0.0, < 4.1.1); first patched version 4.1.1

## CWE Findings (Code-Level Vulnerabilities)

### CWE-259: Use of Hard-coded Password
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep

The SQL server administrator password is defined at infra/main.bicep:58-68 as a literal fixed password prefix [MASKED] plus uniqueString(resourceGroup().id, environmentName). The fixed password prefix is part of the credential sent to the external SQL service, and the suffix is deterministically derived from deployment identifiers rather than a secret source.

### CWE-798: Use of Hard-coded Credentials
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep

The SQL administrator credential is assembled in infra/main.bicep:58-68 using the hard-coded string a fixed password prefix [MASKED] and a deterministic suffix derived from resourceGroup().id and environmentName, then passed as administratorLoginPassword.

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs

PhotoService.UploadPhotoAsync (lines 81-89) accepts a file based on the client-controlled ContentType, and lines 109-120 derive the stored extension from the supplied filename. ImageSharp decode errors are caught without rejecting the upload (lines 125-138), after which the original content is written beneath the web root (lines 140-145). This permits non-image content with an allowed claimed MIME type and a chosen extension to be stored and processed/served as an uploaded photo.

### CWE-79: Improper Neutralization of Input During Web Page Generation ('Cross-site Scripting')
- **Category:** Injection Attacks
- **Severity:** optional
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs, PhotoAlbum/Program.cs, PhotoAlbum/wwwroot/js/upload.js

PhotoService.UploadPhotoAsync (PhotoService.cs:82-89, 109-120, 125-145) trusts the client-supplied MIME type and stores the original filename extension under wwwroot even when ImageSharp cannot decode the content; Program.cs:62-70 enables static file serving. Independently, the upload response includes the untrusted original filename and upload.js:189-199 interpolates it into an HTML template passed to insertAdjacentHTML at line 166, enabling stored script injection when a crafted filename is submitted.
