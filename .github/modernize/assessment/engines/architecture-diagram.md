# Architecture Diagram

PhotoAlbum is an ASP.NET Core 9.0 Razor Pages application providing a photo gallery with upload, browse, and delete capabilities backed by SQL Server via Entity Framework Core.

## Application Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        Browser["Web Browser"]
    end
    subgraph App["Application Layer - ASP.NET Core 9.0"]
        RazorPages["Razor Pages (Index, Detail, PhotoFile)"]
        PhotoSvc["PhotoService (Business Logic)"]
        EFCore["Entity Framework Core 9.0"]
    end
    subgraph Data["Data Layer"]
        DB[("SQL Server LocalDB\nPhotoAlbumDb")]
    end
    subgraph FileStorage["File Storage"]
        LocalFS["Local File System\nwwwroot/uploads/"]
    end
    subgraph ImgProc["Image Processing"]
        ImageSharp["SixLabors.ImageSharp 3.1"]
    end

    Browser -->|"HTTP/HTTPS requests"| RazorPages
    RazorPages -->|"delegates operations"| PhotoSvc
    PhotoSvc -->|"CRUD operations"| EFCore
    EFCore -->|"SQL queries"| DB
    PhotoSvc -->|"read/write image files"| LocalFS
    PhotoSvc -->|"extract dimensions"| ImageSharp
```

### Technology Stack Summary

| Layer | Technology | Version | Purpose |
|---|---|---|---|
| Presentation | ASP.NET Core Razor Pages | 9.0 | Server-side web UI framework |
| Business Logic | PhotoService | — | Photo upload, validation, retrieval, deletion |
| Data Access | Entity Framework Core (SQL Server) | 9.0.9 | ORM for database persistence |
| Image Processing | SixLabors.ImageSharp | 3.1.11 | Image dimension extraction |
| Runtime | .NET | 9.0 | Application host |
| Database | SQL Server LocalDB | — | Relational data storage |
| Static Files | Local file system (`wwwroot/uploads/`) | — | Uploaded photo storage |

### Data Storage & External Services

The application uses a single SQL Server LocalDB instance (`PhotoAlbumDb`) for photo metadata persistence and a local `wwwroot/uploads/` directory for physical image file storage. There are no external cloud services, message queues, or caches configured in the current state.

### Key Architectural Decisions

- **Service abstraction**: `IPhotoService` interface decouples the business logic from Razor Pages, making the storage backend swappable (e.g., local → Azure Blob Storage).
- **Transactional file consistency**: If a database save fails after a file is written to disk, the service deletes the orphaned file to keep file and database state in sync.
- **Configuration-driven constraints**: File size limits (10 MB) and allowed MIME types are read from `appsettings.json`, avoiding hard-coded values in service logic.

## Component Relationships

```mermaid
flowchart LR
    subgraph Presentation["Presentation (Razor Pages)"]
        IndexPage["IndexModel"]
        DetailPage["DetailModel"]
        PhotoFilePage["PhotoFileModel"]
        ErrorPage["ErrorModel"]
    end
    subgraph Business["Business Logic"]
        IPhotoSvc["IPhotoService (interface)"]
        PhotoSvc["PhotoService"]
    end
    subgraph DataAccess["Data Access"]
        DBContext["PhotoAlbumContext (DbContext)"]
    end
    subgraph Models["Domain Models"]
        PhotoModel["Photo"]
        UploadResult["UploadResult"]
    end

    IndexPage -->|"delegates"| IPhotoSvc
    DetailPage -->|"delegates"| IPhotoSvc
    PhotoFilePage -->|"delegates"| IPhotoSvc
    IPhotoSvc -->|"implemented by"| PhotoSvc
    PhotoSvc -->|"queries/persists"| DBContext
    DBContext -->|"maps to"| PhotoModel
    PhotoSvc -->|"returns"| UploadResult
    PhotoSvc -->|"creates/reads"| PhotoModel
```

### Component Inventory

| Component | Layer | Type | Responsibility |
|---|---|---|---|
| IndexModel | Presentation | Razor Page Model | Gallery view; handles file upload POST and photo list GET |
| DetailModel | Presentation | Razor Page Model | Single photo display with navigation; handles delete POST |
| PhotoFileModel | Presentation | Razor Page Model | Serves physical photo files by ID with cache headers |
| ErrorModel | Presentation | Razor Page Model | Error display page |
| IPhotoService | Business Logic | Interface | Contract for photo operations (upload, retrieve, delete) |
| PhotoService | Business Logic | Service | Implements IPhotoService; validates, stores, and manages photos |
| PhotoAlbumContext | Data Access | EF Core DbContext | Database context exposing `Photos` DbSet; configures indexes |
| Photo | Domain Models | Entity / Model | Photo metadata entity (filenames, size, MIME, dimensions, timestamp) |
| UploadResult | Domain Models | DTO | Upload operation result carrying success flag, photo ID, and error message |
