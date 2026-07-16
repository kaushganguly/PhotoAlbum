# Data Architecture & Persistence Layer

PhotoAlbum uses a single SQL Server LocalDB database with one EF Core entity (`Photo`) and no caching layer. EF Core Migrations manage schema lifecycle.

## Database Configuration

| Service/Module | DB Type | Profile | Driver | Connection | Migration Tool |
|---|---|---|---|---|---|
| PhotoAlbum | SQL Server LocalDB | All (default) | Microsoft.EntityFrameworkCore.SqlServer 9.0.9 | `Server=(localdb)\mssqllocaldb;Database=PhotoAlbumDb;Trusted_Connection=true;MultipleActiveResultSets=true` | EF Core Migrations (auto-applied on startup) |

## Data Ownership per Service

| Service | Tables Owned | ORM Framework | Caching | Notes |
|---|---|---|---|---|
| PhotoAlbum | Photos | EF Core 9.0 | None | Single bounded context; migrations applied automatically at startup (skipped in test environment) |

## Entity Model

```mermaid
erDiagram
    Photo {
        int Id PK
        string OriginalFileName
        string StoredFileName
        string FilePath
        long FileSize
        string MimeType
        datetime UploadedAt
        int Width
        int Height
    }
```

## Key Repository Methods

| Service | Repository / Access Point | Notable Methods | Purpose |
|---|---|---|---|
| PhotoAlbum | `PhotoAlbumContext.Photos` (DbSet) | `OrderByDescending(p => p.UploadedAt).ToListAsync()` | Retrieve all photos newest-first |
| PhotoAlbum | `PhotoAlbumContext.Photos` (DbSet) | `FindAsync(id)` | Look up a single photo by primary key |
| PhotoAlbum | `PhotoAlbumContext.Photos` (DbSet) | `AddAsync(photo)` + `SaveChangesAsync()` | Persist new photo metadata |
| PhotoAlbum | `PhotoAlbumContext.Photos` (DbSet) | `Remove(photo)` + `SaveChangesAsync()` | Delete photo metadata |

No custom query methods, stored procedures, or named queries are defined. All data access goes through `PhotoService`, which uses the `DbContext` directly rather than a repository abstraction.

## Caching Strategy

No caching layer is configured. All photo queries are issued directly against SQL Server on every request. Static files served from `wwwroot/` are cached by ASP.NET Core's static file middleware with a `Cache-Control: public, max-age=3600` header, and photo files served by `PhotoFileModel` include `Cache-Control: public, max-age=31536000` and `ETag` headers for browser-level caching. There is no server-side cache (Redis, MemoryCache, etc.).

## Data Ownership Boundaries

The application has a single data store (SQL Server LocalDB) and a single service (`PhotoAlbum`). There are no cross-service data access patterns, shared databases, or database-per-service boundaries to document. All reads and writes are performed by `PhotoService` through `PhotoAlbumContext`.

Physical file storage in `wwwroot/uploads/` is co-located with the application and is not shared across instances, which creates a data boundary risk if the application is deployed to multiple nodes.

### Data Classification & Sensitivity

| Entity | Sensitive Fields | Classification | Controls in Place |
|---|---|---|---|
| Photo | OriginalFileName | PII (low risk — user-supplied filenames may contain personal identifiers) | None (stored in plain text) |
| Photo | FilePath, StoredFileName, MimeType, FileSize, Width, Height, UploadedAt | Non-sensitive metadata | None required |

No PHI or PCI data is stored. The `OriginalFileName` field preserves the user-supplied filename, which could incidentally contain personal information (e.g., `IMG_20240101_John.jpg`). No encryption-at-rest or data masking is configured for any field.
