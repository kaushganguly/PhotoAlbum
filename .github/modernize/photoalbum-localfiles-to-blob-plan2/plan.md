# Modernization Plan: Migrate Local File Storage to Azure Blob Storage

**Project**: PhotoAlbum

---

## Technical Framework

- **Language**: C# / .NET 9.0
- **Framework**: ASP.NET Core 9.0 (Razor Pages)
- **Build Tool**: dotnet CLI / MSBuild
- **Database**: SQL Server LocalDB (Entity Framework Core 9.0)
- **Key Dependencies**: SixLabors.ImageSharp, Microsoft.EntityFrameworkCore.SqlServer, xUnit (tests)

---

## Overview

> This migration replaces the local file system photo storage with Azure Blob Storage. The application currently
> writes uploaded images to `wwwroot/uploads` on disk using `FileStream` inside `PhotoService`, and deletes them
> via `File.Delete`. This approach does not work for scale-out or containerised hosting on Azure.
>
> The new architecture will:
>
> - Store and retrieve photos from Azure Blob Storage instead of the local file system
> - Use Microsoft Entra Managed Identity (DefaultAzureCredential) for passwordless authentication — no storage
>   keys, SAS tokens, or connection strings
> - Preserve the existing `IPhotoService` contract, EF Core metadata persistence, and the DB-failure rollback
>   behaviour
>
> The migration proceeds in a single focused transform task followed by a security CVE scan.

---

## Migration Impact Summary

| Application  | Original Service          | New Azure Service       | Authentication            | Comments                                  |
|--------------|---------------------------|-------------------------|---------------------------|-------------------------------------------|
| PhotoAlbum   | Local disk (`wwwroot/uploads`) | Azure Blob Storage  | Managed Identity (DefaultAzureCredential) | No keys or connection strings; IPhotoService contract unchanged |

---

## Migration Tasks

### Task 001 — Migrate Local File Storage to Azure Blob Storage

Migrate `PhotoService` from writing/reading files on the local disk (`wwwroot/uploads`) to storing and retrieving
blobs in Azure Blob Storage using the `Azure.Storage.Blobs` SDK and `DefaultAzureCredential` (Managed Identity).
No storage account keys, SAS tokens, or connection strings are permitted.

**Skill**: `photoalbum-localfiles-to-blob` (project)

---

### Task 002 — Security CVE Remediation

Scan all project dependencies for known CVEs and remediate any identified vulnerabilities to ensure the
application is secure before deployment.

**Skill**: `validate-cves-and-fix` (builtin)

---

## Open Questions & Questionnaire

- [x] Q: What authentication method should be used for Azure Storage? → A: Microsoft Entra Managed Identity (DefaultAzureCredential); no keys or connection strings.
- [x] Q: Should the existing IPhotoService contract be preserved? → A: Yes, signatures are unchanged.
- [x] Q: Is a .NET version upgrade required? → A: No — the project targets net9.0 which is in active support and supports the modern Azure SDK.
