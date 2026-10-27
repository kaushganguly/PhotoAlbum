# Architecture Diagram

PhotoAlbum is a server-rendered photo gallery with upload, retrieval, and deletion flows. Its application code uses a single web application, a photo service, Entity Framework Core, SQL Server, and local file storage.

## Application Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        Browser["Web Browser"]
    end
    subgraph App["Application Layer - ASP.NET Core 9"]
        Razor["Razor Pages"]
        Service["PhotoService"]
        Imaging["ImageSharp"]
    end
    subgraph Data["Data Layer"]
        EF["Entity Framework Core 9"]
        SQL[("SQL Server")]
        Files[("Local uploads directory")]
    end

    Browser -->|"HTTP page and upload requests"| Razor
    Razor -->|"photo operations"| Service
    Service -->|"image dimension extraction"| Imaging
    Service -->|"photo metadata operations"| EF
    EF -->|"SQL queries"| SQL
    Service -->|"read and write photo files"| Files
    Razor -->|"serve photo bytes"| Browser
```

### Technology Stack Summary

| Layer | Technology | Version | Purpose |
|---|---|---:|---|
| Presentation | ASP.NET Core Razor Pages | 9.0 | Server-rendered gallery and detail pages |
| Business logic | C# PhotoService | .NET 9.0 | Upload validation, photo operations, and file management |
| Image processing | SixLabors.ImageSharp | 3.1.11 | Image dimension extraction |
| Data access | Entity Framework Core SQL Server provider | 9.0.9 | Photo metadata persistence |
| Database | SQL Server | LocalDB in development; Azure SQL in deployment configuration | Store photo metadata |
| File storage | Local filesystem | N/A | Store uploaded photo files under `wwwroot/uploads` |

### Data Storage & External Services

The application persists photo metadata in SQL Server and stores photo content in a local uploads directory. The deployment templates provision Azure SQL and Azure Blob Storage, but the application code currently saves files to the local filesystem and does not use a blob-storage client.

### Key Architectural Decisions

- Razor Page models delegate photo operations through the `IPhotoService` abstraction.
- `PhotoService` coordinates EF Core persistence and local file operations, deleting a saved file if metadata persistence fails.
- EF Core migrations are applied during startup except when the test-environment flag is enabled.

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
        Database[("SQL Server")]
    end
    subgraph Infrastructure["Infrastructure"]
        Disk[("Uploads directory")]
        ImageSharp["ImageSharp"]
        Logging["ASP.NET Core logging"]
    end

    Index -->|"upload and list operations"| Interface
    Detail -->|"retrieve and delete operations"| Interface
    PhotoFile -->|"lookup photo metadata"| Interface
    Interface -.->|"implemented by"| PhotoService
    PhotoService -->|"queries and updates"| Context
    Context -->|"maps"| Entity
    Context -->|"EF Core SQL"| Database
    PhotoService -->|"stores and deletes files"| Disk
    PhotoService -->|"extracts dimensions"| ImageSharp
    Index -.->|"diagnostics"| Logging
    Detail -.->|"diagnostics"| Logging
    PhotoFile -.->|"diagnostics"| Logging
```

### Component Inventory

| Component | Layer | Type | Responsibility |
|---|---|---|---|
| IndexModel | Presentation | Razor Page model | Lists photos and handles multipart uploads |
| DetailModel | Presentation | Razor Page model | Displays one photo and handles deletion/navigation |
| PhotoFileModel | Presentation | Razor Page model | Returns photo bytes by photo ID |
| IPhotoService | Business Logic | Service interface | Defines photo retrieval, upload, and deletion operations |
| PhotoService | Business Logic | Scoped service | Validates uploads and coordinates image, file, and database operations |
| PhotoAlbumContext | Data Access | EF Core DbContext | Maps and persists photo metadata |
| Photo | Data Access | Entity | Represents photo metadata |
| ImageSharp | Infrastructure | Image library | Reads image dimensions during upload |
| Uploads directory | Infrastructure | Local filesystem | Holds uploaded files |
