# Security Assessment Report

**Generated:** 2026-10-26T13:42:09.396Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 9 |
| CVE Vulnerabilities | 5 |
| CWE Vulnerabilities | 4 |
| Total Rules Assessed | 59 |
| Rules Passed | 55 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 4 |
| optional | 4 |
| potential | 1 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w): ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct, resolved for PhotoAlbum/PhotoAlbum.csproj at line 16)

Recommended fix:
  - Upgrade to SixLabors.ImageSharp: 4.1.2

### CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106116](https://github.com/advisories/GHSA-wmxv-xphr-5c9g): ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop

Severity: MEDIUM

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct, resolved for PhotoAlbum/PhotoAlbum.csproj at line 16)

Recommended fix:
  - Upgrade to SixLabors.ImageSharp: 4.1.2

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q): ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct, resolved for PhotoAlbum/PhotoAlbum.csproj at line 16)

Recommended fix:
  - Upgrade to SixLabors.ImageSharp: 4.1.2

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4): ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

Severity: HIGH

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct, resolved for PhotoAlbum/PhotoAlbum.csproj at line 16)

Recommended fix:
  - Upgrade to SixLabors.ImageSharp: 4.1.2

### CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106114](https://github.com/advisories/GHSA-gwg2-r3hj-4w44): ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions

Severity: MEDIUM

Affected dependencies:
  - SixLabors.ImageSharp:3.1.11 (direct, resolved for PhotoAlbum/PhotoAlbum.csproj at line 16)

Recommended fix:
  - Upgrade to SixLabors.ImageSharp: 4.1.2

## CWE Findings (Code-Level Vulnerabilities)

### CWE-772: Missing Release of Resource after Effective Lifetime
- **Category:** Code Quality
- **Severity:** potential
- **Story Points:** 3
- **Files:** PhotoAlbum/Services/PhotoService.cs:127,132

UploadPhotoAsync opens a stream with file.OpenReadStream() and passes it to Image.LoadAsync without retaining or disposing the stream (line 127). It then opens a second stream to set Position and immediately discards the reference (line 132), leaving these stream resources unreleased by the method.

### CWE-259: Use of Hard-coded Password
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.json:43

The sqlAdminPassword variable creates the SQL administrator credential as a fixed 'P@ssw0rd' prefix plus ARM uniqueString(resourceGroup().id, environmentName). uniqueString is deterministic from deployment identifiers rather than secret randomness, making the credential reproducible/predictable; the value is passed to the SQL server deployment (infra/main.json:6332).

### CWE-798: Use of Hard-coded Credentials
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.json:43

The sqlAdminPassword variable creates the SQL administrator credential as a fixed 'P@ssw0rd' prefix plus ARM uniqueString(resourceGroup().id, environmentName). uniqueString is deterministic from deployment identifiers rather than secret randomness, making the credential reproducible/predictable; the value is passed to the SQL server deployment (infra/main.json:6332).

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:82, PhotoAlbum/Services/PhotoService.cs:110-144

UploadPhotoAsync trusts the client-supplied ContentType when checking allowed types (line 82) and derives the stored extension from the client-supplied filename (line 110). If ImageSharp cannot decode the content, the exception is caught and processing continues (lines 125-138), after which the unverified bytes are written to the upload directory (lines 140-144). The server therefore persists content that was not confirmed to be a supported image type.
