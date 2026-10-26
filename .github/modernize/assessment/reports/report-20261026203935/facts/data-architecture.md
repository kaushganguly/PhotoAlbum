# Data Architecture & Persistence Layer

The data layer contains one EF Core entity, `Photo`, persisted to SQL Server in the application and an in-memory provider in service tests. Photo content is stored separately on local disk.

## Database Configuration

| Service/Module | DB Type | Profile | Driver | Connection | Migration Tool |
|---|---|---|---|---|---|
| PhotoAlbum | SQL Server LocalDB | Default application configuration | EF Core SQL Server provider 9.0.9 | LocalDB connection configured for `PhotoAlbumDb` | EF Core Migrations; applied at application startup outside the test environment |
| PhotoAlbum.Tests | EF Core InMemory | Unit tests | EF Core InMemory provider 9.0.9 | Unique in-memory database per test | None |

The initial schema is represented by an EF Core migration. No seed-data scripts or programmatic database seeding were identified.

## Data Ownership per Service

| Service | Tables Owned | ORM Framework | Caching | Notes |
|---|---|---|---|---|
| PhotoAlbum | Photos | Entity Framework Core | No data-layer cache | Metadata is stored in SQL Server; file content is stored on the local filesystem |

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

`PhotoAlbum.Models.Photo` (`PhotoAlbum/Models/Photo.cs`) contains metadata for one stored image. There are no entity relationships or foreign keys. `PhotoAlbumContext` (`PhotoAlbum/Data/PhotoAlbumContext.cs`) defines the entity mapping and a descending index on upload time. No explicit transaction scope is configured; EF Core `SaveChangesAsync` uses its provider transaction behavior, and the service removes a newly written file if metadata persistence fails.

## Key Repository Methods

| Service | Repository | Notable Methods | Purpose |
|---|---|---|---|
| PhotoAlbum | None; `PhotoAlbumContext` is used directly | `Photos.OrderByDescending(...).ToListAsync()`, `Photos.FindAsync(id)`, `Photos.AddAsync(photo)`, `SaveChangesAsync()`, `Photos.Remove(photo)` | Loads, persists, and deletes photo metadata; there are no custom repository interfaces or query methods |

## Caching Strategy

No database query cache or application data cache is configured. HTTP responses for static files and photo-file responses set cache headers; these are transport-level response directives rather than a configured data cache.

## Data Ownership Boundaries

The single application service owns the photo metadata entity and its SQL Server database. The image bytes are kept in the local upload directory, and the database record refers to that file. The test project substitutes an EF Core InMemory store. There is no cross-service data access, shared multi-service database, or CQRS/read-model split.

### Data Classification & Sensitivity

| Entity | Sensitive Fields | Classification (PII/PHI/PCI/None) | Controls in Place |
|---|---|---|---|
| Photo | `OriginalFileName` may contain user-supplied identifying text; remaining fields are file metadata | Potentially identifying metadata; no explicit PII, PHI, or PCI fields | No field-level masking or encryption configuration identified; database and file protection depend on deployment/storage configuration |
