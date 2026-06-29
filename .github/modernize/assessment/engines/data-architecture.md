# Data Architecture & Persistence Layer

The PhotoAlbum application has a single entity (`Photo`) persisted to SQL Server via EF Core 9.0, with schema managed by EF Migrations. There are no inter-service data boundaries or caching layers.

## Database Configuration

| Service/Module | DB Type | Profile | Driver | Connection | Migration Tool |
|---|---|---|---|---|---|
| PhotoAlbum | SQL Server LocalDB | Development (default) | Microsoft.EntityFrameworkCore.SqlServer 9.0.9 | `(localdb)\mssqllocaldb`, database `PhotoAlbumDb` | EF Core Migrations (applied automatically on startup) |
| PhotoAlbum.Tests | In-memory (EF Core) | Test | Microsoft.EntityFrameworkCore.InMemory 9.0.9 | In-memory provider; no persistent store | N/A — schema created from model on each test run |

EF Migrations are applied automatically at startup unless `IsTestEnvironment=true` is set in configuration. The initial migration (`20250930101715_InitialCreate`) creates the `Photos` table with a descending index on `UploadedAt`. No seed data scripts are present.

## Data Ownership per Service

| Service | Tables Owned | ORM Framework | Caching | Notes |
|---|---|---|---|---|
| PhotoAlbum | Photos | EF Core 9.0 | None | Single-table schema; physical files stored in `wwwroot/uploads/` alongside DB metadata |

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
        int Width "nullable"
        int Height "nullable"
    }
```

**Photo** (`PhotoAlbum/Models/Photo.cs`, table `Photos`):

| Field | DB Type | Constraints |
|---|---|---|
| Id | int IDENTITY | PK, auto-increment |
| OriginalFileName | nvarchar(255) | NOT NULL |
| StoredFileName | nvarchar(255) | NOT NULL — GUID-based filename on disk |
| FilePath | nvarchar(500) | NOT NULL — relative path from wwwroot (e.g., `/uploads/abc.jpg`) |
| FileSize | bigint | NOT NULL |
| MimeType | nvarchar(50) | NOT NULL |
| UploadedAt | datetime2 | NOT NULL — indexed descending (IX_Photos_UploadedAt) |
| Width | int | Nullable — populated by ImageSharp on upload |
| Height | int | Nullable — populated by ImageSharp on upload |

No relationships or foreign keys exist — the schema is a single independent table.

## Key Repository Methods

EF Core's `DbContext` is used directly (no repository abstraction layer). The `PhotoAlbumContext.Photos` `DbSet<Photo>` provides all data access.

| Service | Access Point | Notable Methods | Purpose |
|---|---|---|---|
| PhotoService | `PhotoAlbumContext.Photos` | `Photos.OrderByDescending(p => p.UploadedAt).ToListAsync()` | Fetch all photos sorted newest-first for gallery display |
| PhotoService | `PhotoAlbumContext.Photos` | `Photos.FindAsync(id)` | Retrieve a single photo by primary key |
| PhotoService | `PhotoAlbumContext.Photos` | `Photos.AddAsync(photo)` + `SaveChangesAsync()` | Persist newly uploaded photo metadata |
| PhotoService | `PhotoAlbumContext.Photos` | `Photos.Remove(photo)` + `SaveChangesAsync()` | Delete a photo record from the database |

No custom query methods, named queries, stored procedures, or `@Query`-style annotations are present. No transaction scope decorators are used; EF Core's default per-`SaveChanges` transaction is relied upon.

## Caching Strategy

No caching layer is configured in the application. Static file responses served from `wwwroot/uploads/` include a `Cache-Control: public, max-age=3600` HTTP response header, and the `PhotoFile` page endpoint sets `Cache-Control: public, max-age=31536000` with an ETag — these are client-side/proxy HTTP cache hints, not a server-side cache provider.

There is no Redis, IMemoryCache, IDistributedCache, or second-level cache configured. Frequently-read data (e.g., the photo list on every page load) hits the database directly on every request.

## Data Ownership Boundaries

The application has a single deployable unit and a single shared SQL Server database. There are no service boundaries to enforce; all data access originates from `PhotoService` via `PhotoAlbumContext`. Physical image files are stored alongside the database metadata on the local file system — the database holds only the metadata (filename, path, size, MIME type, dimensions) while the actual bytes reside in `wwwroot/uploads/`.

No CQRS patterns, event sourcing, or read replicas are in use. All read and write operations share the same EF Core `DbContext` scoped to the HTTP request lifetime.

### Data Classification & Sensitivity

| Entity | Sensitive Fields | Classification | Controls in Place |
|---|---|---|---|
| Photo | None — metadata only (filenames, file size, MIME type, dimensions, upload timestamp) | None (no PII/PHI/PCI) | N/A |

The uploaded image files themselves may contain embedded EXIF metadata (GPS coordinates, device identifiers) which could constitute PII depending on content, but no EXIF stripping or inspection is performed at upload time. No encryption-at-rest, field-level masking, or access controls are configured.
