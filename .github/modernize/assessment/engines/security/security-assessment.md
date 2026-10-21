# Security Assessment Report

**Generated:** 2026-10-21T13:11:12.277395Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 5 |
| CVE Vulnerabilities | 3 |
| CWE Vulnerabilities | 2 |
| Total Rules Assessed | 59 |
| Rules Passed | 57 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 4 |
| optional | 1 |
| potential | 0 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q): ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range: >= 2.0.0, <= 4.1.1; first patched: 4.1.2

Recommended fix:
  - Upgrade affected dependencies to the first patched version listed above, or a later supported version.

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4): ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range: >= 2.0.0, <= 4.1.1; first patched: 4.1.2

Recommended fix:
  - Upgrade affected dependencies to the first patched version listed above, or a later supported version.

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w): ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range: >= 2.1.0, <= 4.1.1; first patched: 4.1.2

Recommended fix:
  - Upgrade affected dependencies to the first patched version listed above, or a later supported version.

## CWE Findings (Code-Level Vulnerabilities)

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:81, PhotoAlbum/Services/PhotoService.cs:109, PhotoAlbum/Services/PhotoService.cs:125, PhotoAlbum/Services/PhotoService.cs:140

PhotoService.UploadPhotoAsync trusts the client-controlled ContentType for its allowlist check (lines 81-89) and derives the stored extension from the client-supplied FileName (lines 109-112). ImageSharp decode errors are caught and ignored (lines 125-138), after which the original uploaded bytes are still saved (lines 140-145). Thus a non-image payload with a spoofed allowed ContentType and a dangerous extension can be persisted and served from the uploads area without successful image validation.

### CWE-79: Improper Neutralization of Input During Web Page Generation ('Cross-site Scripting')
- **Category:** Injection Attacks
- **Severity:** optional
- **Story Points:** 8
- **Files:** PhotoAlbum/wwwroot/js/upload.js:193, PhotoAlbum/wwwroot/js/upload.js:196-197, PhotoAlbum/wwwroot/js/upload.js:223-226

The upload flow returns the client-controlled original filename in JSON (PhotoAlbum/Pages/Index.cshtml.cs:76-83). upload.js inserts originalFileName into an HTML template at lines 193 and 196-197, then inserts the template with insertAdjacentHTML at line 166; upload error messages similarly interpolate file names into innerHTML at lines 128-129 and 223-226. A crafted filename can therefore inject markup/script into the gallery or upload feedback.
