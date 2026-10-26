# Architecture Diagram

PhotoAlbum is a server-rendered photo gallery with a single web application and no separately deployed services. Photo metadata is stored in SQL Server, while image files are stored on the local filesystem.

## Application Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        Browser["Web Browser"]
    end
    subgraph App["Application Layer - ASP.NET Core 9 Razor Pages"]
        Pages["Razor Pages"]
        Service["Photo Service"]
        Images["ImageSharp Processing"]
    end
    subgraph Data["Data and File Storage"]
        EF["Entity Framework Core 9"]
        DB[("SQL Server LocalDB")]
        Files[("wwwroot uploads directory")]
    end

    Browser -->|"HTTP requests"| Pages
    Pages -->|"photo operations"| Service
    Service -->|"image dimensions"| Images
    Service -->|"metadata CRUD"| EF
    EF -->|"SQL queries"| DB
    Service -->|"read and write image files"| Files
```

### Technology Stack Summary

| Layer | Technology | Version | Purpose |
|---|---|---:|---|
| Presentation | ASP.NET Core Razor Pages | 9.0 | Server-rendered gallery and photo pages |
| Business logic | PhotoService / ImageSharp | 3.1.11 | Upload, retrieval, deletion, and image dimension extraction |
| Persistence | Entity Framework Core / SQL Server provider | 9.0.9 | Persist photo metadata |
| File storage | Local filesystem | — | Store image bytes under the configured upload directory |

### Data Storage & External Services

The app uses SQL Server LocalDB for photo metadata and local disk for image content. It does not declare external APIs, caches, or message brokers; ImageSharp processes uploaded images in-process.

### Key Architectural Decisions

- `IPhotoService` separates page handlers from photo operations and storage implementation.
- Upload writes the image file and then persists metadata, deleting the file if the database save fails.
- Database migrations run during startup except when the `IsTestEnvironment` setting is enabled.

## Component Relationships

```mermaid
flowchart LR
    subgraph Presentation["Presentation"]
        Index["Index Razor Page"]
        Detail["Detail Razor Page"]
        PhotoFile["PhotoFile Razor Page"]
    end
    subgraph Business["Business Logic"]
        Contract["IPhotoService"]
        PhotoSvc["PhotoService"]
        Image["ImageSharp"]
    end
    subgraph DataAccess["Data Access"]
        Context["PhotoAlbumContext"]
        Photo["Photo Entity"]
        SQL[("SQL Server")]
        Disk[("Upload Directory")]
    end
    subgraph CrossCutting["Cross-cutting"]
        Logging["ASP.NET Core Logging"]
    end

    Index -->|"uses"| Contract
    Detail -->|"uses"| Contract
    PhotoFile -->|"uses"| Contract
    Contract -->|"implemented by"| PhotoSvc
    PhotoSvc -->|"extracts dimensions"| Image
    PhotoSvc -->|"queries and saves"| Context
    Context -->|"maps"| Photo
    Context -->|"persists metadata"| SQL
    PhotoSvc -->|"stores image bytes"| Disk
    PhotoSvc -.->|"logs errors and operations"| Logging
```

### Component Inventory

| Component | Layer | Type | Responsibility |
|---|---|---|---|
| Index | Presentation | Razor Page | Displays the gallery and handles photo uploads |
| Detail | Presentation | Razor Page | Displays a photo and handles deletion |
| PhotoFile | Presentation | Razor Page | Serves an image file by photo ID |
| IPhotoService | Business Logic | Service interface | Defines photo retrieval, upload, and deletion operations |
| PhotoService | Business Logic | Service | Validates uploads and coordinates image, file, and metadata operations |
| PhotoAlbumContext | Data Access | EF Core DbContext | Maps photo metadata to SQL Server |
| Photo | Data Access | Entity | Represents stored photo metadata |
