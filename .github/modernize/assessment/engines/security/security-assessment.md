# Security Assessment Report

**Generated:** 2026-10-26T05:31:50Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 7 |
| CVE Vulnerabilities | 5 |
| CWE Vulnerabilities | 2 |
| Total Rules Assessed | 59 |
| Rules Passed | 57 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 4 |
| optional | 3 |
| potential | 0 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Severity:** high
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w): ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

Severity: HIGH

Affected dependencies:
- SixLabors.ImageSharp:3.1.11 (directly declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 2.1.0, <= 4.1.1; first patched 4.1.2)

### CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop
- **Severity:** medium
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106116](https://github.com/advisories/GHSA-wmxv-xphr-5c9g): ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop

Severity: MEDIUM

Affected dependencies:
- SixLabors.ImageSharp:3.1.11 (directly declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 2.0.0, <= 4.1.1; first patched 4.1.2)

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Severity:** high
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q): ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

Severity: HIGH

Affected dependencies:
- SixLabors.ImageSharp:3.1.11 (directly declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 2.0.0, <= 4.1.1; first patched 4.1.2)

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Severity:** high
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4): ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

Severity: HIGH

Affected dependencies:
- SixLabors.ImageSharp:3.1.11 (directly declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 2.0.0, <= 4.1.1; first patched 4.1.2)

### CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions
- **Severity:** medium
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106114](https://github.com/advisories/GHSA-gwg2-r3hj-4w44): ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions

Severity: MEDIUM

Affected dependencies:
- SixLabors.ImageSharp:3.1.11 (directly declared at PhotoAlbum/PhotoAlbum.csproj:16; vulnerable range >= 1.0.0-beta0001, <= 4.1.1; first patched 4.1.2)

## CWE Findings (Code-Level Vulnerabilities)

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:82, PhotoAlbum/Services/PhotoService.cs:109-112, PhotoAlbum/Services/PhotoService.cs:125-138, PhotoAlbum/Services/PhotoService.cs:143-144

PhotoService.UploadPhotoAsync trusts the client-provided ContentType, derives the stored extension from the client-provided FileName, and continues with saving the upload when ImageSharp fails to decode it. An attacker can label arbitrary active content (for example, an HTML file) as an allowed image MIME type and have it stored with an .html extension under wwwroot/uploads, where static-file middleware can serve it.

### CWE-79: Improper Neutralization of Input During Web Page Generation ('Cross-site Scripting')
- **Category:** Injection Attacks
- **Severity:** optional
- **Story Points:** 8
- **Files:** PhotoAlbum/wwwroot/js/upload.js:73-75, PhotoAlbum/wwwroot/js/upload.js:128, PhotoAlbum/wwwroot/js/upload.js:223-226

Client-controlled file names are placed into error strings in handleFiles and passed to showErrors, which concatenates them into HTML and assigns that markup to uploadErrors.innerHTML without escaping. An attacker-controlled file name containing markup can therefore execute script in the gallery page when a file is rejected.
