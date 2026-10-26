# Security Assessment Report

**Generated:** 2026-10-26T19:48:26.025572Z

## Summary

| Metric | Count |
|--------|-------|
| Total Findings | 3 |
| CVE Vulnerabilities | 0 |
| CWE Vulnerabilities | 3 |
| Total Rules Assessed | 59 |
| Rules Passed | 56 |

### By Severity

| Severity | Count |
|----------|-------|
| mandatory | 1 |
| optional | 2 |
| potential | 0 |

## CVE Findings (Dependency Vulnerabilities)

No CVE findings were recorded. The unauthenticated GitHub Advisory API returned HTTP 403 rate-limit errors for each dependency batch, so the dependency scan is inconclusive.

## CWE Findings (Code-Level Vulnerabilities)

### CWE-259: Use of Hard-coded Password
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep:57-59, infra/main.json:41-44

The SQL administrator password is constructed in the deployment template from a fixed literal prefix and a deterministic value derived from public deployment inputs. This embeds a predictable credential-generation pattern in source-controlled infrastructure.

### CWE-798: Use of Hard-coded Credentials
- **Category:** Credentials & Secrets
- **Severity:** optional
- **Story Points:** 5
- **Files:** infra/main.bicep:57-59, infra/main.json:41-44

The SQL administrator password is constructed in the deployment template from a fixed literal prefix and a deterministic value derived from public deployment inputs. This embeds a predictable credential-generation pattern in source-controlled infrastructure.

### CWE-434: Unrestricted Upload of File with Dangerous Type
- **Category:** File & Path Security
- **Severity:** mandatory
- **Story Points:** 8
- **Files:** PhotoAlbum/Services/PhotoService.cs:81-88, PhotoAlbum/Services/PhotoService.cs:125-138, PhotoAlbum/Pages/PhotoFile.cshtml.cs:67-76

PhotoService.UploadPhotoAsync accepts files based on the client-supplied ContentType allowlist. ImageSharp decoding failures are caught and treated as non-fatal, after which the original contents are saved and can be served by PhotoFileModel using the stored MIME type. Thus a non-image payload can be stored and made available as an image upload by claiming an allowed MIME type.
