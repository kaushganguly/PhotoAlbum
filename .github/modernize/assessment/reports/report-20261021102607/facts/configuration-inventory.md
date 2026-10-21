# Configuration & Externalized Settings Inventory

Configuration is supplied from application JSON, local launch profiles, XML hosting configuration, and deployment templates. No dedicated production `appsettings` file, feature-flag provider, or secret-store client configuration was identified.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application defaults | JSON | `PhotoAlbum/appsettings.json` | Connection, upload, host, and logging settings |
| Development overrides | JSON | `PhotoAlbum/appsettings.Development.json` | Detailed errors and development log levels |
| Local launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | HTTP/HTTPS local ports and Development environment |
| Hosting configuration | XML | `PhotoAlbum/Web.config` | LocalDB connection string for IIS hosting |
| Container build | Dockerfile | `Dockerfile` | .NET 9 multi-stage image, application listens on port 8080 |
| Azure deployment metadata | YAML | `azure.yaml` | Container App service and Bicep infrastructure locations |
| Infrastructure | Bicep | `infra/main.bicep` and related parameters | Azure resources and deployment settings |
| Environment overrides | Environment variables | Deployment configuration | Connection string and `ASPNETCORE_ENVIRONMENT` can be supplied externally |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default MSBuild configuration | Development build | No conditional package changes identified |
| Release | `-c Release` | Optimized/publish build; used by Dockerfile | No conditional package changes identified |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development`; default local launch profiles | `appsettings.json`, `appsettings.Development.json`, `launchSettings.json` | Detailed errors, more verbose logs, local ports |
| Production | `ASPNETCORE_ENVIRONMENT=Production` in deployment | `appsettings.json` plus environment overrides | Connection string supplied at deployment; HSTS enabled and exception handler used |
| Tests | `IsTestEnvironment=true` configuration in test host | Test-provided in-memory configuration | Startup database migrations are skipped; unit tests inject in-memory settings |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB connection | Local; deployment override | `appsettings.json`, `Web.config`, environment |
| `FileUpload:MaxFileSizeBytes` | `10485760` | All | `appsettings.json`; service fallback also 10485760 |
| `FileUpload:AllowedMimeTypes` | JPEG, PNG, GIF, WebP | All | `appsettings.json`; service has the same fallback |
| `FileUpload:MaxFilesPerUpload` | `10` | All | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | All | `appsettings.json`; service fallback |
| `Logging:LogLevel:Default` | `Information` | Development override: `Debug` | JSON config |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Development override: `Information` | JSON config |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | Not specified | Development: `Information` | `appsettings.Development.json` |
| `AllowedHosts` | `*` | All | `appsettings.json` |
| `IsTestEnvironment` | `false` when absent | Tests | Read from configuration in `Program.cs` |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | .NET 9 ASP.NET Core; container port 8080; local profiles use ports 5134 and 7055 | Not specified | Not specified |

## Startup Dependency Chain

1. ASP.NET Core loads configuration and registers Razor Pages, SQL Server `PhotoAlbumContext`, and scoped `IPhotoService`.
2. The application creates the uploads directory if missing.
3. Outside test mode, the application opens a DI scope and applies EF migrations before serving requests; a migration failure is logged and aborts startup.
4. The HTTP pipeline enables HTTPS redirection, static file handling, routing, authorization middleware, and Razor Pages.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection configuration | Local LocalDB uses trusted connection; cloud deployment supplies a connection override, with managed identity described by deployment documentation |

No password or cryptographic key value is present in the application settings examined. Deployment documentation describes using managed identity for Azure SQL access; no application Key Vault client binding was identified.

### Secrets Provisioning Workflow

For cloud deployment, infrastructure provisions the application and database, and deployment configuration supplies the connection string/environment values. Deployment documentation indicates managed identity is used for database access. Local development uses LocalDB trusted authentication; no credentials are required in the committed application settings.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Application configuration; skips startup migrations when true |

No feature-flag framework or remote toggle service was identified.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---:|---|
| Target framework | .NET 9 (`net9.0`) | Both project files |
| ASP.NET Core | 9.0 shared framework | Web SDK project |
| Entity Framework Core | 9.0.9 | Project package references |
| ImageSharp | 3.1.11 | Application package reference |
| .NET SDK/runtime container images | 9.0 | `Dockerfile` |
| Visual Studio solution format | 12.00 / VS 17 metadata | `PhotoAlbum.sln` |
