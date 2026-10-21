# Security Assessment Report

**Generated:** 2026-10-21T10:32:11.847217Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 6 |
| CVE Vulnerabilities | 3 |
| CWE Vulnerabilities | 3 |
| Total Rules Assessed | 59 |
| Rules Passed | 56 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 4 |
| optional | 0 |
| potential | 2 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w): ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct dependency declared in PhotoAlbum/PhotoAlbum.csproj:16; also transitive in PhotoAlbum.Tests); vulnerable range >= 2.1.0, <= 4.1.1; first patched version 4.1.2

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q): ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct dependency declared in PhotoAlbum/PhotoAlbum.csproj:16; also transitive in PhotoAlbum.Tests); vulnerable range >= 2.0.0, <= 4.1.1; first patched version 4.1.2

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4): ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct dependency declared in PhotoAlbum/PhotoAlbum.csproj:16; also transitive in PhotoAlbum.Tests); vulnerable range >= 2.0.0, <= 4.1.1; first patched version 4.1.2

## CWE Findings (Code-Level Vulnerabilities)

### CWE-772: Missing Release of Resource after Effective Lifetime
- **Category:** Code Quality
- **Severity:** potential
- **Story Points:** 3
- **Files:** PhotoAlbum/Services/PhotoService.cs

PhotoService.UploadPhotoAsync opens an additional stream with file.OpenReadStream() and only sets its Position (line 132); the returned Stream is not assigned to a using scope or disposed after use. Repeated uploads can therefore retain stream resources beyond their effective lifetime.

### CWE-820: Missing Synchronization
- **Category:** Concurrency & Synchronization
- **Severity:** potential
- **Story Points:** 8
- **Files:** PhotoAlbum/Pages/PhotoFile.cshtml.cs:67, PhotoAlbum/Services/PhotoService.cs:224-229

PhotoFileModel.OnGetAsync reads a stored photo while PhotoService.DeletePhotoAsync may delete that same file concurrently. The per-request scoped service does not coordinate these accesses, so concurrent requests can race and make an in-progress photo read fail or observe inconsistent file/database state.

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:82, PhotoAlbum/Services/PhotoService.cs:110, PhotoAlbum/Services/PhotoService.cs:127, PhotoAlbum/Services/PhotoService.cs:134

PhotoService.UploadPhotoAsync trusts the client-provided ContentType when checking the allowlist (lines 82-88), then derives the stored extension from the client-provided filename (line 110). ImageSharp decode failures are caught and ignored (lines 125-138), after which the original bytes are still written to the publicly served uploads directory (lines 140-145). A client can therefore label non-image content with an allowed image MIME type and have it stored and served using its supplied extension.
