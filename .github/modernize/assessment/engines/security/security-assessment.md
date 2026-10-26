# Security Assessment Report

**Generated:** 2026-10-26T20:46:39.366465Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 12 |
| CVE Vulnerabilities | 6 |
| CWE Vulnerabilities | 6 |
| Total Rules Assessed | 59 |
| Rules Passed | 53 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 5 |
| optional | 5 |
| potential | 2 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q)

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); affected range >= 2.0.0, < 3.2.0.

Recommended fix: Upgrade to 3.2.0 or later in the supported version line.

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4)

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); affected range >= 2.0.0, < 3.2.0.

Recommended fix: Upgrade to 3.2.0 or later in the supported version line.

### CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106114](https://github.com/advisories/GHSA-gwg2-r3hj-4w44)

Severity: MEDIUM

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); affected range >= 1.0.0-beta0001, < 3.2.0.

Recommended fix: Upgrade to 3.2.0 or later in the supported version line.

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w)

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); affected range >= 2.1.0, < 3.2.0.

Recommended fix: Upgrade to 3.2.0 or later in the supported version line.

### CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106116](https://github.com/advisories/GHSA-wmxv-xphr-5c9g)

Severity: MEDIUM

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); affected range >= 2.0.0, < 3.2.0.

Recommended fix: Upgrade to 3.2.0 or later in the supported version line.

### CVE-2026-106118: ImageSharp: Tiled fax TIFF: tile buffer sized by TileWidth but fax decompressor writes scanlines of ImageWidth — heap OOB write
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106118](https://github.com/advisories/GHSA-v76p-62qx-wwq2)

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11 (declared at PhotoAlbum/PhotoAlbum.csproj:16); affected range >= 3.0.0, < 3.2.0.

Recommended fix: Upgrade to 3.2.0 or later in the supported version line.

## CWE Findings (Code-Level Vulnerabilities)

### CWE-606: Unchecked Input for Loop Condition
- **Category:** Code Quality
- **Severity:** potential
- **Story Points:** 3
- **Files:** PhotoAlbum/Pages/Index.cshtml.cs:53

IndexModel.OnPostUploadAsync iterates over the caller-provided files collection without enforcing the configured MaxFilesPerUpload value. The number of per-file service operations is therefore controlled by the submitted multipart request; the request body has an overall limit but the individual loop count is not checked against the application limit.

### CWE-772: Missing Release of Resource after Effective Lifetime
- **Category:** Code Quality
- **Severity:** potential
- **Story Points:** 3
- **Files:** PhotoAlbum/Services/PhotoService.cs:127

PhotoService.UploadPhotoAsync passes a newly opened IFormFile stream directly to Image.LoadAsync without disposing the stream. The subsequent OpenReadStream call at line 132 is also not disposed, leaving request stream wrappers unreleased after dimension extraction.

### CWE-259: Use of Hard-coded Password
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep:59

The infrastructure template defines the SQL administrator password inline as a deterministic value derived from deployment identifiers and supplies it to the SQL server module. This embeds password construction in source rather than accepting a secret from a protected provisioning input.

### CWE-798: Use of Hard-coded Credentials
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep:59

The infrastructure template embeds construction of the SQL administrator credential in source and passes that value to the SQL server module, making a credential part of the deployed infrastructure definition.

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:82, PhotoAlbum/Services/PhotoService.cs:110, PhotoAlbum/Services/PhotoService.cs:143, PhotoAlbum/Program.cs:63

PhotoService.UploadPhotoAsync accepts uploads based on the client-supplied ContentType allowlist, preserves the extension from the user-supplied filename, and writes bytes under the static web root. ImageSharp load failure is caught and upload continues, so content is not required to be a valid image. Program enables static file serving for the upload directory, allowing mismatched or active content with a permitted claimed MIME type to be stored and publicly served.

### CWE-79: Improper Neutralization of Input During Web Page Generation ('Cross-site Scripting')
- **Category:** Injection Attacks
- **Severity:** optional
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:82, PhotoAlbum/Services/PhotoService.cs:110, PhotoAlbum/Services/PhotoService.cs:127, PhotoAlbum/Program.cs:63

PhotoService.UploadPhotoAsync trusts the client-supplied MIME type, preserves the uploaded filename extension, and continues after ImageSharp fails to decode the content. The file is saved below wwwroot, and Program enables static-file serving there. An attacker can upload HTML bytes while claiming an allowed image MIME type and using an .html extension; the static-file middleware can serve the uploaded content as an active same-origin HTML page.
