# Architecture Diagram

This document summarizes the PhotoAlbum application's runtime architecture and the main component interactions that support photo upload, retrieval, and deletion.

## Application Architecture

```mermaid
flowchart TD
    subgraph Client["Client Layer"]
        Browser["Web Browser"]
    end
    subgraph App["Application Layer - ASP.NET Core 9 Razor Pages"]
        Pages["Razor Pages UI"]
        UploadJs["Upload JavaScript"]
        Service["PhotoService"]
    end
    subgraph Data["Data Layer"]
        DbContext["PhotoAlbumContext"]
        DB[("SQL Server LocalDB")]
        Files[("wwwroot/uploads")]
    end
    subgraph External["External Dependencies"]
        ImageSharp["ImageSharp"]
    end

    Browser -->|"GET pages and assets"| Pages
    Browser -->|"POST multipart upload"| UploadJs
    UploadJs -->|"Upload request"| Pages
    Pages -->|"Photo operations"| Service
    Service -->|"EF Core CRUD"| DbContext
    DbContext -->|"SQL queries"| DB
    Service -->|"Save and delete files"| Files
    Service -->|"Read image dimensions"| ImageSharp
    Pages -->|"Serve image stream"| Browser
```

### Technology Stack Summary

| Layer | Technology | Version | Purpose |
|---|---|---:|---|
| Presentation | ASP.NET Core Razor Pages | 9.0 | Server-rendered UI for gallery, detail, and file endpoints |
| Client Interaction | Vanilla JavaScript + Fetch | n/a | Drag-and-drop uploads and gallery updates |
| Business Logic | PhotoService | Application code | Validates uploads and coordinates file/database persistence |
| Data Access | Entity Framework Core SQL Server | 9.0.9 | Maps the `Photo` entity and persists metadata |
| Storage | SQL Server LocalDB | Configured connection string | Stores photo metadata |
| File Storage | Local file system (`wwwroot/uploads`) | n/a | Stores uploaded image binaries |
| Image Processing | SixLabors.ImageSharp | 3.1.11 | Extracts image width and height metadata |

### Data Storage & External Services

The application uses a single SQL Server LocalDB database to store photo metadata and a local `wwwroot/uploads` directory to store the image files themselves. There are no outbound API integrations, caches, or message brokers; the only external library interaction at runtime beyond the framework is ImageSharp for image inspection.

### Key Architectural Decisions

- Uses a service-layer abstraction (`IPhotoService`/`PhotoService`) to keep Razor Page handlers thin and isolate storage logic.
- Splits binary storage from metadata persistence, with rollback logic that removes the saved file if the database write fails.
- Stores uploads on the local file system behind an indirect `/photo/{id}` endpoint instead of exposing raw file paths directly.

## Component Relationships

```mermaid
flowchart LR
    subgraph Presentation["Presentation"]
        IndexPage["IndexModel"]
        DetailPage["DetailModel"]
        PhotoFilePage["PhotoFileModel"]
        UploadScript["upload.js"]
    end
    subgraph Business["Business Logic"]
        PhotoSvc["PhotoService"]
        PhotoContract["IPhotoService"]
    end
    subgraph DataAccess["Data Access"]
        AlbumContext["PhotoAlbumContext"]
        PhotoEntity["Photo"]
    end
    subgraph Infra["Infrastructure"]
        FormConfig["FormOptions"]
        FileStore["Uploads Directory"]
        Migration["Startup Migrations"]
    end

    UploadScript -->|"POST upload"| IndexPage
    IndexPage -->|"uses"| PhotoContract
    DetailPage -->|"uses"| PhotoContract
    PhotoFilePage -->|"uses"| PhotoContract
    PhotoContract -->|"implemented by"| PhotoSvc
    PhotoSvc -->|"queries and saves"| AlbumContext
    AlbumContext -->|"maps"| PhotoEntity
    PhotoSvc -->|"reads and writes"| FileStore
    FormConfig -.->|"upload size limits"| IndexPage
    Migration -.->|"ensures schema"| AlbumContext
```

### Component Inventory

| Component | Layer | Type | Responsibility |
|---|---|---|---|
| `IndexModel` | Presentation | Razor Page model | Loads the gallery and handles upload POST requests |
| `DetailModel` | Presentation | Razor Page model | Shows a single photo, navigation links, and delete workflow |
| `PhotoFileModel` | Presentation | Razor Page model | Streams stored photo binaries by ID |
| `upload.js` | Presentation | Browser script | Performs client-side validation and AJAX upload requests |
| `IPhotoService` | Business Logic | Service contract | Defines photo retrieval, upload, and deletion operations |
| `PhotoService` | Business Logic | Service implementation | Validates files, extracts dimensions, stores files, and persists metadata |
| `PhotoAlbumContext` | Data Access | EF Core DbContext | Exposes the `Photos` DbSet and model configuration |
| `Photo` | Data Access | Entity | Represents persisted photo metadata |
| Form options configuration | Infrastructure | Startup configuration | Enforces multipart form upload limits |
| Startup migrations | Infrastructure | Startup routine | Applies EF Core migrations when not in the test environment |
| `wwwroot/uploads` | Infrastructure | File store | Holds uploaded image content on disk |
