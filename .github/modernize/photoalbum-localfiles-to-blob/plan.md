# Modernization Plan: Migrate Local File Storage to Azure Blob Storage

**Project**: PhotoAlbum

---

## Technical Framework

- **Language**: .NET 9.0 (C#)
- **Framework**: ASP.NET Core 9.0 (Razor Pages)
- **Build Tool**: .NET SDK 9.0
- **Database**: SQL Server (Azure SQL via Entity Framework Core 9.0)
- **Key Dependencies**: Microsoft.EntityFrameworkCore.SqlServer 9.0.9, SixLabors.ImageSharp 3.1.11

---

## Overview

> This migration replaces the local disk-based photo file storage with Azure Blob Storage. The
> application currently saves uploaded images to `wwwroot/uploads/` on the server's local file
> system via `FileStream`, and stores relative paths such as `/uploads/{filename}` in the
> database. The new architecture will:
>
> - Store photo files in Azure Blob Storage for scalable, durable, and geo-redundant cloud
>   storage decoupled from the compute host
> - Use Managed Identity for passwordless, credential-free authentication to Azure Blob Storage
> - Return publicly accessible blob URLs (or SAS URLs) for photo retrieval, replacing local
>   relative paths
>
> The migration is scoped to code changes only: `PhotoService.cs` is updated to upload blobs
> and delete blobs instead of using local file I/O, while all other application behaviour
> (metadata in SQL Server, image validation, error handling) remains unchanged.

---

## Migration Impact Summary

| Application  | Original Service                    | New Azure Service     | Authentication   | Comments                                         |
|--------------|-------------------------------------|-----------------------|------------------|--------------------------------------------------|
| PhotoAlbum   | Local File System (`wwwroot/uploads`)| Azure Blob Storage    | Managed Identity | Replace FileStream save/delete with Blob SDK     |

---

## Migration Tasks

### Task 1 — Migrate Local File Storage to Azure Blob Storage

Replace all local file I/O in `PhotoService` with Azure Blob Storage SDK operations so that
uploaded photos are stored in and served from a Blob container rather than the server's disk.

**Skill**: `migration-azure-storage-blob`

---

### Task 2 — Security & CVE Remediation

Scan all project dependencies for known CVEs and remediate identified vulnerabilities before
the modernised application is deployed.

**Skill**: `validate-cves-and-fix`

---

## Open Questions & Questionnaire

- [x] Q: Should the plan include infrastructure provisioning? → A: No — focus on code migration only; no new infrastructure provisioned
- [x] Q: Should the plan include integration testing? → A: No — integration testing not explicitly requested; skipped
- [x] Q: Should the plan include security/CVE remediation? → A: Yes — default security task included
- [x] Q: Which deployment target should the plan use? → A: No deployment — code migration only
