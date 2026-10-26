# Security Assessment Report

**Generated:** 2026-10-26T04:57:48.083763+00:00

## Summary

| Metric | Count |
|--------|-------|
| TotalFindings | 7 |
| CveCount | 5 |
| CweCount | 2 |
| TotalRulesAssessed | 59 |
| RulesPassed | 57 |

Minimum CVE severity: **medium**. CWE checklist matches are not independent root causes; CWE-434 and CWE-79 describe the same upload flaw. NOT_FOUND means no confirmed match during this review, not a guarantee of safety.

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 4 |
| optional | 3 |
| potential | 0 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Category:** CVE
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w): ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

Severity: HIGH

Affected dependency: SixLabors.ImageSharp:3.1.11 (direct, PhotoAlbum/PhotoAlbum.csproj:16; also inherited by PhotoAlbum.Tests).

Affected range: >= 2.1.0, <= 4.1.1. Patched version: 4.1.2

### CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop
- **Category:** CVE
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106116](https://github.com/advisories/GHSA-wmxv-xphr-5c9g): ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop

Severity: MEDIUM

Affected dependency: SixLabors.ImageSharp:3.1.11 (direct, PhotoAlbum/PhotoAlbum.csproj:16; also inherited by PhotoAlbum.Tests).

Affected range: >= 2.0.0, <= 4.1.1. Patched version: 4.1.2

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Category:** CVE
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q): ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

Severity: HIGH

Affected dependency: SixLabors.ImageSharp:3.1.11 (direct, PhotoAlbum/PhotoAlbum.csproj:16; also inherited by PhotoAlbum.Tests).

Affected range: >= 2.0.0, <= 4.1.1. Patched version: 4.1.2

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Category:** CVE
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4): ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

Severity: HIGH

Affected dependency: SixLabors.ImageSharp:3.1.11 (direct, PhotoAlbum/PhotoAlbum.csproj:16; also inherited by PhotoAlbum.Tests).

Affected range: >= 2.0.0, <= 4.1.1. Patched version: 4.1.2

### CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions
- **Category:** CVE
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106114](https://github.com/advisories/GHSA-gwg2-r3hj-4w44): ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions

Severity: MEDIUM

Affected dependency: SixLabors.ImageSharp:3.1.11 (direct, PhotoAlbum/PhotoAlbum.csproj:16; also inherited by PhotoAlbum.Tests).

Affected range: >= 1.0.0-beta0001, <= 4.1.1. Patched version: 4.1.2

## CWE Findings (Code-Level Vulnerabilities)

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:81-88, PhotoAlbum/Services/PhotoService.cs:109-144, PhotoAlbum/Program.cs:62-70

PhotoService.UploadPhotoAsync trusts the supplied Content-Type, retains the client filename extension, and accepts files even when Image.LoadAsync fails. An anonymous uploader can submit HTML with image/png and an .html extension. Original bytes are saved under wwwroot/uploads and served as HTML by static-file middleware, enabling same-origin stored script execution.

### CWE-79: Improper Neutralization of Input During Web Page Generation ('Cross-site Scripting')
- **Category:** Injection Attacks
- **Severity:** optional
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:81-144, PhotoAlbum/Program.cs:62-70

PhotoService.UploadPhotoAsync permits HTML bytes with an .html extension when Content-Type is image/png and image decoding fails. Static-file middleware serves these uploaded bytes as active HTML under the application origin. A victim following the public upload URL executes attacker-controlled script. This is the same root cause as CWE-434.

## Additional observations outside the requested CWE checklist

- **CWE-1391:** `infra/main.bicep:57-68,79-84` deterministically derives the SQL administrator password from non-secret deployment metadata.
- **CWE-532:** `azure-setup.sh:197-200` prints the SQL administrator password to deployment output. No secret values are reproduced here.
These observations are excluded from the 59-rule checklist statistics.

## Coverage limitations

- Image decoding has no explicit application-level decoded-pixel/frame budget (`PhotoAlbum/Services/PhotoService.cs:125-138`). Internal allocator safeguards were not independently validated.
- This is static assessment, not a penetration test. No exploit payloads were executed.
