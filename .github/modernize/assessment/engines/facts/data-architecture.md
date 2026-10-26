# Data Architecture & Persistence Layer

The data layer contains one EF Core entity, `Photo`, and uses SQL Server LocalDB for photo metadata. Uploaded image bytes are persisted separately on the local filesystem.

## Database Configuration

| Service/Module | DB Type | Profile | Driver | Connection | Migration Tool |
|---|---|---|---|---|---|
| PhotoAlbum | SQL Server LocalDB | Default and Development | EF Core SQL Server provider 9.0.9 | Configured by application connection settings | EF Core Migrations |
| PhotoAlbum.Tests | EF Core InMemory | Tests | EF Core InMemory provider 9.0.9 | Unique in-memory database per test setup | None |

The application applies EF Core migrations at startup except when its test-environment flag is set. An initial schema migration is present; no seed-data files or seed routines were identified.

## Data Ownership per Service

| Service | Tables Owned | ORM Framework | Caching | Notes |
|---|---|---|---|---|
| PhotoAlbum | Photos | EF Core 9 | No data cache | Stores photo metadata; image bytes are kept in local files |

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

`Photo` is defined in `PhotoAlbum/Models/Photo.cs` and mapped by `PhotoAlbum/Data/PhotoAlbumContext.cs`. It has no entity relationships. `PhotoAlbumContext` configures required metadata fields, maximum lengths, and a descending index on the upload timestamp. No explicit transaction scope is configured; EF Core `SaveChangesAsync` is used for persistence.

## Key Repository Methods

| Service | Repository | Notable Methods | Purpose |
|---|---|---|---|
| PhotoAlbum | No repository interface; queries are made through `PhotoAlbumContext.Photos` in `PhotoService` | `GetAllPhotosAsync` orders by upload time; `GetPhotoByIdAsync` uses `FindAsync` | Retrieve ordered photo metadata or one entity by key |

## Caching Strategy

No server-side data cache or distributed cache provider is configured. Photo responses set HTTP cache headers, which affect client/proxy caching rather than EF Core query caching.

## Data Ownership Boundaries

The single application owns its photo metadata database and local uploaded-file directory. There are no separate service data stores or cross-service data access paths. Reads and writes are performed directly through the DbContext and `PhotoService`; no CQRS or shared-database access between services was identified.

### Data Classification & Sensitivity

| Entity | Sensitive Fields | Classification (PII/PHI/PCI/None) | Controls in Place |
|---|---|---|---|
| Photo | User-provided original filename and image content stored outside the entity database row may contain personal information | Potential PII; not explicitly classified | No data masking or field-level access controls found; application configuration does not specify encryption at rest |
