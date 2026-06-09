# Modernization Plan: Migrate Local File Storage to Azure Blob Storage

**Project**: PhotoAlbum

---

## Technical Framework

- **Language**: C# / .NET 9.0
- **Framework**: ASP.NET Core 9.0 (Razor Pages)
- **Build Tool**: dotnet CLI / MSBuild
- **Database**: SQL Server (LocalDB for development, Azure SQL for production)
- **Key Dependencies**: Entity Framework Core 9.0, SixLabors.ImageSharp 3.1.11

---

## Overview

This migration replaces local file system photo storage with Azure Blob Storage. The application currently saves uploaded images to `wwwroot/uploads` on the local disk via `PhotoService.UploadPhotoAsync` and removes them with `File.Delete` in `DeletePhotoAsync`. Local disk storage is incompatible with scaled-out or containerized hosting on Azure.

The new architecture will:

- Store uploaded photos as blobs in an Azure Storage account, eliminating local-disk writes entirely
- Authenticate with Azure using Managed Identity (`DefaultAzureCredential`) — no storage account keys, SAS tokens, or connection strings
- Serve images via the blob URL stored in `Photo.FilePath`, replacing the static-file path under `wwwroot/uploads`

The migration is delivered in a single focused task that rewrites `PhotoService` and updates `Program.cs`, while preserving the existing `IPhotoService` contract, EF Core metadata model, and ImageSharp dimension-extraction logic.

---

## Migration Impact Summary

| Application  | Original Service          | New Azure Service          | Authentication     | Comments                                    |
|--------------|---------------------------|----------------------------|--------------------|---------------------------------------------|
| PhotoAlbum   | Local file system (disk)  | Azure Blob Storage         | Managed Identity   | Replace wwwroot/uploads with blob container |

---

## Open Questions & Questionnaire

- [x] Q: What authentication method should be used for Azure Storage? → A: Managed Identity (`DefaultAzureCredential`); no storage account keys or connection strings
- [x] Q: Should the existing `IPhotoService` API surface be preserved? → A: Yes, all method signatures remain unchanged
- [x] Q: Should integration tests be added? → A: Not explicitly requested; skipped
- [x] Q: Should a deployment task be included? → A: Not explicitly requested; skipped
