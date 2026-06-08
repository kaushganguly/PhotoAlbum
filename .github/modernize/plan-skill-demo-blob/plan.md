# Modernization Plan: Migrate Local File Storage to Azure Blob Storage

**Project**: PhotoAlbum

---

## Technical Framework

- **Language**: C# / .NET 9.0
- **Framework**: ASP.NET Core 9.0 with Razor Pages
- **Build Tool**: dotnet CLI / MSBuild
- **Database**: SQL Server (LocalDB in development) via Entity Framework Core 9.0
- **Key Dependencies**: Microsoft.EntityFrameworkCore.SqlServer 9.0.9, SixLabors.ImageSharp 3.1.11

---

## Overview

This migration replaces the PhotoAlbum application's local file system storage with Azure Blob Storage. The application currently stores uploaded photos on the local disk under `wwwroot/uploads` via `PhotoService`, which does not work for scaled-out or containerized Azure hosting.

The new architecture will:

- Store all uploaded photos as blobs in an Azure Blob Storage container, enabling durable and scalable cloud storage
- Authenticate to Azure Blob Storage using Microsoft Entra Managed Identity (DefaultAzureCredential) — no storage account keys or connection strings
- Preserve the existing `IPhotoService` contract, EF Core metadata persistence, and ImageSharp dimension extraction without breaking changes

The migration is a focused code-only change: update `PhotoService` and `Program.cs` to use the Azure Blob Storage SDK. No infrastructure provisioning or deployment changes are included in this plan.

---

## Migration Impact Summary

| Application  | Original Service          | New Azure Service           | Authentication       | Comments                                 |
|--------------|---------------------------|-----------------------------|----------------------|------------------------------------------|
| PhotoAlbum   | Local disk (wwwroot/uploads) | Azure Blob Storage       | Managed Identity     | Passwordless via DefaultAzureCredential  |

---

## Open Questions & Questionnaire

- [x] Q: Should the plan include environment/infrastructure provisioning? → A: No — focus on code migration only; no infrastructure provisioning included.
- [x] Q: Should the plan include integration testing? → A: No — integration testing not requested by user; skipped.
- [x] Q: Should the plan include a security scan and CVE remediation task? → A: Yes — default security/CVE remediation task included.
- [x] Q: Which Azure deployment target should the plan use? → A: No deployment — migration only, no cloud deployment.
