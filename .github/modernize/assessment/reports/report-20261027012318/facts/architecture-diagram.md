# Architecture Diagram

PhotoAlbum is an ASP.NET Core Razor Pages application that manages photo metadata in SQL Server and image files on the local filesystem.

## Application Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        Browser["Web Browser"]
    end
    subgraph App["Application Layer - ASP.NET Core 9.0"]
        Razor["Razor Pages"]
        Service["PhotoService"]
        Image["ImageSharp"]
    end
    subgraph Data["Data Layer"]
        EF["Entity Framework Core 9.0"]
        SQL[("SQL Server LocalDB")]
        Files[("Local uploads directory")]
    end

    Browser -->|"HTTP requests and responses"| Razor
    Razor -->|"photo operations"| Service
    Service -->|"image dimensions"| Image
    Service -->|"photo metadata"| EF
    EF -->|"SQL queries"| SQL
    Service -->|"image file read and write"| Files
```

### Technology Stack Summary

| Layer | Technology | Version | Purpose |
|---|---|---|---|
| Presentation | ASP.NET Core Razor Pages | 9.0 | Server-rendered gallery, details, and upload endpoints |
| Business logic | PhotoService | Application code | Validates and manages photo uploads, retrieval, and deletion |
| Image processing | SixLabors.ImageSharp | 3.1.11 | Reads image dimensions |
| Data access | Entity Framework Core with SQL Server provider | 9.0.9 | Persists photo metadata |
| Data storage | SQL Server LocalDB | LocalDB | Stores photo metadata |
| File storage | Local filesystem | N/A | Stores uploaded images under `wwwroot/uploads` |

### Data Storage & External Services

Photo metadata is stored in SQL Server LocalDB through Entity Framework Core, while the image binaries are stored in the application's local `wwwroot/uploads` directory. ImageSharp is used locally to inspect image dimensions; no external APIs, caches, or message brokers are configured.

### Key Architectural Decisions

- Razor Pages handle HTTP interactions and delegate photo operations through the `IPhotoService` abstraction.
- `PhotoService` coordinates filesystem operations and EF Core persistence, deleting a newly written file if metadata persistence fails.
- Configuration supplies upload limits, allowed MIME types, storage path, and the database connection string.

## Component Relationships

```mermaid
flowchart LR
    subgraph Presentation["Presentation"]
        Index["IndexModel"]
        Detail["DetailModel"]
        PhotoFile["PhotoFileModel"]
    end
    subgraph Business["Business Logic"]
        Interface["IPhotoService"]
        PhotoService["PhotoService"]
    end
    subgraph DataAccess["Data Access"]
        Context["PhotoAlbumContext"]
        Entity["Photo"]
    end
    subgraph Infrastructure["Infrastructure"]
        Config["Configuration and logging"]
        Disk["Local uploads directory"]
        Database[("SQL Server LocalDB")]
        ImageSharp["ImageSharp"]
    end

    Index -->|"uses"| Interface
    Detail -->|"uses"| Interface
    PhotoFile -->|"uses"| Interface
    Interface -.->|"implemented by"| PhotoService
    PhotoService -->|"queries and persists"| Context
    Context -->|"maps"| Entity
    Context -->|"uses EF Core provider"| Database
    PhotoService -->|"reads and writes"| Disk
    PhotoService -->|"extracts dimensions"| ImageSharp
    PhotoService -->|"reads settings and logs"| Config
    Index -.->|"logs"| Config
    Detail -.->|"logs"| Config
    PhotoFile -.->|"reads settings and logs"| Config
```

### Component Inventory

| Component | Layer | Type | Responsibility |
|---|---|---|---|
| IndexModel | Presentation | Razor Page model | Lists photos and handles multi-file uploads |
| DetailModel | Presentation | Razor Page model | Displays photo details and handles deletion |
| PhotoFileModel | Presentation | Razor Page model | Serves a stored image by photo ID |
| IPhotoService | Business Logic | Service interface | Defines photo retrieval, upload, and deletion operations |
| PhotoService | Business Logic | Service | Validates uploads and coordinates image, file, and metadata operations |
| PhotoAlbumContext | Data Access | EF Core DbContext | Maps and queries photo metadata |
| Photo | Data Access | Entity | Represents photo metadata persisted in the database |
| Program | Infrastructure | Application startup | Registers Razor Pages, services, database, and HTTP pipeline |
