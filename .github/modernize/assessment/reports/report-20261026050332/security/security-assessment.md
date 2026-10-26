# Security Assessment Report

**Generated:** 2026-10-26T05:09:12.790818+00:00

**Minimum CVE severity:** medium

## Summary

| Metric | Count |
|--------|-------|
| TotalFindings | 11 |
| CveCount | 5 |
| CweCount | 6 |
| TotalRulesAssessed | 59 |
| RulesPassed | 53 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 4 |
| optional | 6 |
| potential | 1 |

## CVE Findings (Dependency Vulnerabilities)

### CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer
- **Category:** CVE
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106115](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w): ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11, directly declared at PhotoAlbum/PhotoAlbum.csproj:16 and transitively resolved by PhotoAlbum.Tests through its application project reference (not a test-project package declaration). Affected range: >= 2.1.0, <= 4.1.1. Both dotnet list package --include-transitive --vulnerable and GitHub REST advisories confirm this version.

Recommended fix: upgrade to 4.1.2 or later, checking major-version compatibility. This identifies vulnerable dependency presence; TIFF encoding and histogram-processing APIs are not used by current application code, so exploitability of those specific advisories is not established.

### CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop
- **Category:** CVE
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106116](https://github.com/advisories/GHSA-wmxv-xphr-5c9g): ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop

Severity: MEDIUM

Affected dependency: SixLabors.ImageSharp 3.1.11, directly declared at PhotoAlbum/PhotoAlbum.csproj:16 and transitively resolved by PhotoAlbum.Tests through its application project reference (not a test-project package declaration). Affected range: >= 2.0.0, <= 4.1.1. Both dotnet list package --include-transitive --vulnerable and GitHub REST advisories confirm this version.

Recommended fix: upgrade to 4.1.2 or later, checking major-version compatibility. This identifies vulnerable dependency presence; TIFF encoding and histogram-processing APIs are not used by current application code, so exploitability of those specific advisories is not established.

### CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer
- **Category:** CVE
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106110](https://github.com/advisories/GHSA-j9gm-c75j-xc9q): ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11, directly declared at PhotoAlbum/PhotoAlbum.csproj:16 and transitively resolved by PhotoAlbum.Tests through its application project reference (not a test-project package declaration). Affected range: >= 2.0.0, <= 4.1.1. Both dotnet list package --include-transitive --vulnerable and GitHub REST advisories confirm this version.

Recommended fix: upgrade to 4.1.2 or later, checking major-version compatibility. This identifies vulnerable dependency presence; TIFF encoding and histogram-processing APIs are not used by current application code, so exploitability of those specific advisories is not established.

### CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index
- **Category:** CVE
- **Severity:** mandatory
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106113](https://github.com/advisories/GHSA-j3p4-wp97-rph4): ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index

Severity: HIGH

Affected dependency: SixLabors.ImageSharp 3.1.11, directly declared at PhotoAlbum/PhotoAlbum.csproj:16 and transitively resolved by PhotoAlbum.Tests through its application project reference (not a test-project package declaration). Affected range: >= 2.0.0, <= 4.1.1. Both dotnet list package --include-transitive --vulnerable and GitHub REST advisories confirm this version.

Recommended fix: upgrade to 4.1.2 or later, checking major-version compatibility. This identifies vulnerable dependency presence; TIFF encoding and histogram-processing APIs are not used by current application code, so exploitability of those specific advisories is not established.

### CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions
- **Category:** CVE
- **Severity:** optional
- **Story Points:** 1
- **Files:** PhotoAlbum/PhotoAlbum.csproj:16

[CVE-2026-106114](https://github.com/advisories/GHSA-gwg2-r3hj-4w44): ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions

Severity: MEDIUM

Affected dependency: SixLabors.ImageSharp 3.1.11, directly declared at PhotoAlbum/PhotoAlbum.csproj:16 and transitively resolved by PhotoAlbum.Tests through its application project reference (not a test-project package declaration). Affected range: >= 1.0.0-beta0001, <= 4.1.1. Both dotnet list package --include-transitive --vulnerable and GitHub REST advisories confirm this version.

Recommended fix: upgrade to 4.1.2 or later, checking major-version compatibility. This identifies vulnerable dependency presence; TIFF encoding and histogram-processing APIs are not used by current application code, so exploitability of those specific advisories is not established.

## CWE Findings (Code-Level Vulnerabilities)

### CWE-789: Memory Allocation with Excessive Size Value
- **Category:** Code Quality
- **Severity:** potential
- **Story Points:** 5
- **Files:** PhotoAlbum/Services/PhotoService.cs:92, PhotoAlbum/Services/PhotoService.cs:127, PhotoAlbum/Pages/Index.cshtml.cs:53

PhotoService.UploadPhotoAsync bounds only the compressed upload byte count (92-99), then Image.LoadAsync (127) fully decodes attacker-supplied image data to extract width/height. There is no application pixel-count, frame-count, decoded-byte budget, or configured image allocator limit before this allocation. Highly compressed large-dimension/multiframe PNG/GIF input can remain below 10MB while its decoded pixel buffers require much more memory. IndexModel.OnPostUploadAsync exposes this path to uploaded files without authentication. Image disposal occurs only after decoding; catching decoder exceptions does not prevent peak memory exhaustion. This is an application-level resource bound omission, independent of the separately reported ImageSharp CVEs. No oversized image was generated or executed against shared resources.

### CWE-259: Use of Hard-coded Password
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep:59, infra/main.bicep:68

The SQL administrator credential is constructed at infra/main.bicep:59 from a fixed source-code password prefix and uniqueString(resourceGroup().id, environmentName), then used as administratorLoginPassword at line 68. uniqueString is a deterministic naming hash of non-secret deployment identifiers, not a cryptographic random secret; the credential is reproducible from source plus those identifiers and has no independent secret input. This is a source-defined deterministic administrator credential, not a recovered live credential. Credential text is intentionally omitted. The alternative azure-setup.sh path uses openssl randomness and is not classified as a hard-coded password.

### CWE-732: Incorrect Permission Assignment for Critical Resource
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** PhotoAlbum/Pages/Detail.cshtml.cs:92, PhotoAlbum/Services/PhotoService.cs:212, PhotoAlbum/Program.cs:8, PhotoAlbum/Program.cs:74

DetailModel.OnPostDeleteAsync accepts an arbitrary photo ID (92-96) and PhotoService.DeletePhotoAsync removes the corresponding file and database row (216-240), without checking identity or ownership. Program registers no authentication and no page authorization convention/policy, and the page model has no authorization attribute. Thus any site visitor, not just a photo owner/administrator, can obtain the publicly rendered form antiforgery token and delete any listed photo. Antiforgery validation does not assign permission to mutate another visitor's resource. This finding concerns destructive access; the public read-only gallery itself is not treated as a vulnerability. Static authorization/dataflow review; no files were deleted.

### CWE-798: Use of Hard-coded Credentials
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep:58, infra/main.bicep:59, infra/main.bicep:67

The deployment source fixes the SQL administrator login at line 58 and determines its entire password using a fixed source prefix plus a deterministic uniqueString of the resource-group ID and environment name (59). The pair is installed as SQL administrator credentials at 67-68. Knowing the public/non-secret deployment identifiers and repository source suffices to reproduce the source-defined credential; no secret parameter, vault value, or random entropy participates in this Bicep path. Credential values are redacted from this report; no deployed account was accessed.

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:82, PhotoAlbum/Services/PhotoService.cs:110, PhotoAlbum/Services/PhotoService.cs:134, PhotoAlbum/Program.cs:63

PhotoService.UploadPhotoAsync trusts the client ContentType (line 82), preserves an unrestricted extension (110-112), ignores image-decoding failures (134-138), and persists the original bytes (143-144). A multipart file named payload.html with Content-Type image/jpeg and HTML/JavaScript content is therefore stored as a GUID.html under wwwroot/uploads. Program.cs:63 serves that directory as static files; the extension causes HTML to be returned as text/html, allowing attacker-supplied active content on the application origin. The upload response reveals the static filePath (Index.cshtml.cs:78). No executable server-side upload behavior is assumed.

### CWE-79: Improper Neutralization of Input During Web Page Generation ('Cross-site Scripting')
- **Category:** Injection Attacks
- **Severity:** optional
- **Story Points:** 8
- **Files:** PhotoAlbum/wwwroot/js/upload.js:166, PhotoAlbum/wwwroot/js/upload.js:193, PhotoAlbum/wwwroot/js/upload.js:224, PhotoAlbum/Pages/Index.cshtml.cs:77

createPhotoCard interpolates the untrusted originalFileName directly into HTML attributes and element text (upload.js:193-197); displayNewPhotos parses that markup with insertAdjacentHTML (166). IndexModel.OnPostUploadAsync returns the original uploaded filename unchanged (77), and PhotoService persists it unchanged (157). showErrors independently inserts filenames from local validation/server failure responses through innerHTML (224-226). A crafted filename containing an HTML element with an event handler is parsed rather than displayed as text when that upload result/error is shown; the DOM finding concerns the uploader/viewer of the crafted file, not all gallery visitors (server-rendered Razor names are encoded). Separately, the confirmed unrestricted .html upload/static-serving path in CWE-434 permits active content to be shared as a same-origin link with other visitors. Static dataflow confirmed; no browser exploit executed.

## Scope and limitations

Static source and dependency analysis only; no live exploitation or deployed-resource verification was performed. NOT_FOUND means no evidence was identified within the reviewed scope, not proof of absence. Several CWE classifications describe the same underlying issue and are not distinct vulnerabilities. Dependency findings do not establish that every affected API is reachable. Findings are recommendations; this assessment does not remediate application vulnerabilities.
