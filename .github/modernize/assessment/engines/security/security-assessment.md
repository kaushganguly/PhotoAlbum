# Security Assessment Report

**Generated:** 2026-10-26T18:10:07.371653Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 7 |
| CVE Vulnerabilities | 6 |
| CWE Vulnerabilities | 1 |
| Total Rules Assessed | 59 |
| Rules Passed | 58 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 5 |
| optional | 2 |
| potential | 0 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w): ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 2.1.0, < 3.2.0; first patched 3.2.0
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 4.0.0, < 4.1.2; first patched 4.1.2

Recommended fix: upgrade to the listed first patched version when available.

### CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106116](https://github.com/advisories/GHSA-wmxv-xphr-5c9g): ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop

Severity: MEDIUM

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 2.0.0, < 3.2.0; first patched 3.2.0
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 4.0.0, < 4.1.2; first patched 4.1.2

Recommended fix: upgrade to the listed first patched version when available.

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q): ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 2.0.0, < 3.2.0; first patched 3.2.0
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 4.0.0, < 4.1.2; first patched 4.1.2

Recommended fix: upgrade to the listed first patched version when available.

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4): ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 2.0.0, < 3.2.0; first patched 3.2.0
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 4.0.0, < 4.1.2; first patched 4.1.2

Recommended fix: upgrade to the listed first patched version when available.

### CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106114](https://github.com/advisories/GHSA-gwg2-r3hj-4w44): ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions

Severity: MEDIUM

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 4.0.0, < 4.1.2; first patched 4.1.2
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 1.0.0-beta0001, < 3.2.0; first patched 3.2.0

Recommended fix: upgrade to the listed first patched version when available.

### CVE-2026-106118: ImageSharp: Tiled fax TIFF: tile buffer sized by TileWidth but fax decompressor writes scanlines of ImageWidth — heap OOB write
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106118](https://github.com/advisories/GHSA-v76p-62qx-wwq2): ImageSharp: Tiled fax TIFF: tile buffer sized by TileWidth but fax decompressor writes scanlines of ImageWidth — heap OOB write

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 3.0.0, < 3.2.0; first patched 3.2.0
  - SixLabors.ImageSharp:3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); vulnerable range >= 4.0.0, < 4.1.1; first patched 4.1.1

Recommended fix: upgrade to the listed first patched version when available.

## CWE Findings (Code-Level Vulnerabilities)

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs

`PhotoService.UploadPhotoAsync` trusts the client-provided `ContentType` for the allowlist check (PhotoAlbum/Services/PhotoService.cs:81-88). If ImageSharp cannot decode the content, the exception is logged but execution continues (lines 125-138), after which the bytes are saved to disk (lines 140-145). Thus a file with a permitted declared MIME type but arbitrary/dangerous content can be stored and later served.
