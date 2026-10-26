# API & Service Communication Contracts

The application exposes Razor Page routes and handlers for gallery browsing, uploads, photo details, deletion, and photo retrieval. Communication is synchronous and occurs within a single application process; no separate service-to-service API was found.

## Service Catalog

| Service | Port | Category | Purpose |
|---|---|---|---|
| PhotoAlbum | 5134 (HTTP), 7055 (HTTPS) in development | API Layer | Serves the photo gallery and handles photo operations |

## API Endpoints Inventory

| Service | Method | Path | Request Type | Response Type |
|---|---|---|---|---|
| PhotoAlbum | GET | `/` | None | Gallery page |
| PhotoAlbum | POST | `/Index?handler=Upload` | Multipart form with `List<IFormFile>` and antiforgery token | JSON upload summary with uploaded and failed items; bad request if no files |
| PhotoAlbum | GET | `/Detail/{id?}` | Optional photo ID path parameter | Detail page or 404 |
| PhotoAlbum | POST | `/Detail/{id}?handler=Delete` | Photo ID path parameter and antiforgery token | Redirect to gallery or detail page |
| PhotoAlbum | GET | `/PhotoFile?id={id}` | Photo ID query parameter | Image bytes with stored MIME type, 404 if missing |

No API versioning scheme or OpenAPI specification was found. Razor Pages use ASP.NET Core's default JSON serialization for the upload result.

## Management & Observability Endpoints

| Service | Endpoint | Custom Metrics |
|---|---|---|
| PhotoAlbum | None identified | None identified |

## DTOs & Contracts

`UploadResult` reports the outcome of an upload to the service layer. `Photo` is the domain entity used by gallery/detail views and metadata persistence. The upload handler returns an anonymous JSON response containing upload summaries; no dedicated immutable API DTOs, protobuf schemas, GraphQL schemas, or OpenAPI documents were found.

## Communication Patterns

Page handlers call `IPhotoService` synchronously via asynchronous in-process method calls. `PhotoService` accesses EF Core and the filesystem directly; there are no message brokers, service discovery, gateways, retries, or circuit breakers. The service logs and rethrows database retrieval/deletion exceptions, while upload errors are represented in `UploadResult`.

HTTPS redirection and HSTS outside development are configured. Authorization middleware is present, but no authentication scheme, authorization policies, or user identity checks were identified; endpoints are not protected by application authentication.

## Service Technology Matrix

| Service | Web | Data Access | Discovery | Gateway | Actuator | Cache | Metrics |
|---|---|---|---|---|---|---|---|
| PhotoAlbum | Razor Pages | EF Core SQL Server | None | None | None identified | None | None identified |

## Service Communication Sequence

```mermaid
sequenceDiagram
    participant Client as "Browser"
    participant Pages as "Razor Pages"
    participant Service as "PhotoService"
    participant Image as "ImageSharp"
    participant Files as "Upload Directory"
    participant DB as "SQL Server"

    Client->>Pages: POST upload form
    Pages->>Service: UploadPhotoAsync(file)
    Service->>Image: Read image dimensions
    alt Image can be decoded
        Image-->>Service: Width and height
    else Image decode fails
        Image-->>Service: Decode error; upload continues without dimensions
    end
    Service->>Files: Save uploaded bytes
    Service->>DB: Save Photo metadata
    alt Metadata saved
        DB-->>Service: Photo ID
        Service-->>Pages: UploadResult success
        Pages-->>Client: JSON result
    else Metadata save fails
        Service->>Files: Delete saved file
        Service-->>Pages: UploadResult failure
        Pages-->>Client: JSON result
    end
```
