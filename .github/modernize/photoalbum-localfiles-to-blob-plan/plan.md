# Modernization Plan: Migrate PhotoAlbum Local File Storage to Azure Blob Storage

**Project**: PhotoAlbum

---

## Technical Framework

- **Language**: .NET (C#)
- **Framework**: ASP.NET Core 9.0 (Razor Pages)
- **Build Tool**: dotnet CLI
- **Database**: SQL Server LocalDB via Entity Framework Core
- **Key Dependencies**: Entity Framework Core, SixLabors.ImageSharp

---

## Overview

This migration replaces local photo file storage with Azure Blob Storage for the
PhotoAlbum application. The application currently stores uploaded photos on the
local file system under `wwwroot/uploads`. The modernized architecture will:

- Move photo binary storage from local disk to Azure Blob Storage.
- Use passwordless identity-based access for storage operations.
- Preserve existing upload, retrieval, and delete user flows.

The migration follows a focused phased approach: storage migration first, then
security remediation for dependencies impacted by modernization work.

---

## Migration Impact Summary

| Application | Original Service | New Azure Service | Authentication | Comments |
|-------------|------------------|-------------------|----------------|----------|
| PhotoAlbum | Local file system | Azure Blob Storage | Managed Identity | Migrate photo storage from local disk to Blob |

---

## Open Questions & Questionnaire

- [x] Q: What migration scope is included in this plan? → A: Local photo file
  storage to Azure Blob Storage only.
- [x] Q: Should deployment or infrastructure provisioning be included? → A: No,
  not requested in scope.
- [x] Q: Which authentication approach should be used for storage access? → A:
  Managed identity with passwordless access.

---

## Planned Tasks

1. **Transform Task**: Migrate local file storage behavior to Azure Blob
   Storage while preserving existing photo workflows.
2. **Security Task**: Scan and remediate known CVEs in dependencies after
   migration updates.

