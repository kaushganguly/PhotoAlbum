# Configuration & Externalized Settings Inventory

PhotoAlbum has a small JSON-based application configuration surface, with local development profiles and Azure deployment settings supplied by Bicep. No external configuration server or secret-store integration is configured in the application.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Default database connection, upload controls, logging, and host filtering |
| Development settings | JSON | `PhotoAlbum/appsettings.Development.json` | Development diagnostics and log levels |
| Launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | Local HTTP/HTTPS ports and Development environment |
| Azure deployment | Bicep | `infra/main.bicep` | Creates container app, SQL Server, storage account, and resource assignments |
| Azure Developer CLI | YAML | `azure.yaml` | Selects the web project, container app host, Bicep template, and deployment hooks |
| Container build | Dockerfile | `Dockerfile` | .NET 9 SDK and ASP.NET runtime images; container port 8080 |
| Environment variables | Runtime | Container App configuration | Uses `ConnectionStrings__DefaultConnection`, `AzureStorageBlob__Endpoint`, `AzureStorageBlob__ContainerName`, and `ASPNETCORE_ENVIRONMENT` |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build` default or `-c Debug` | Development build | No conditional package set identified |
| Release | `-c Release` in Docker build/publish | Production container build | No conditional package set identified |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` in launch profiles | `appsettings.json`, `appsettings.Development.json` | Detailed errors and more verbose ASP.NET Core/EF Core logs |
| Production | `ASPNETCORE_ENVIRONMENT=Production` in Azure Container App | `appsettings.json` plus environment-variable overrides | Azure SQL connection string; production exception handling and HSTS |
| Tests | Test configuration and `IsTestEnvironment` flag | In-memory test settings | EF Core InMemory and test upload directory; migrations skipped when the test flag is true |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB trusted connection to `PhotoAlbumDb` | Production overrides via `ConnectionStrings__DefaultConnection` | `appsettings.json`, Bicep |
| `FileUpload:MaxFileSizeBytes` | `10485760` | All | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | JPEG, PNG, GIF, WebP | All | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | All | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | All; tests supply a temporary path | `appsettings.json`, test configuration |
| `Logging:LogLevel:Default` | `Information` | Development overrides to `Debug` | `appsettings.json`, `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Development overrides to `Information` | `appsettings.json`, `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | Not set in base configuration | Development: `Information` | `appsettings.Development.json` |
| `AllowedHosts` | `*` | All | `appsettings.json` |
| `DetailedErrors` | Not set in base configuration | Development: `true` | `appsettings.Development.json` |
| `IsTestEnvironment` | `false` unless overridden | Tests: `true` | Runtime configuration |
| `AzureStorageBlob:Endpoint` | Not set in application settings | Deployment template supplies endpoint | Bicep environment; no consuming application code identified |
| `AzureStorageBlob:ContainerName` | Not set in application settings | Deployment template supplies `photos` | Bicep environment; no consuming application code identified |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum container | `dotnet PhotoAlbum.dll`; ASP.NET Core environment set to Production in Bicep | 1 GiB; 0.5 CPU | 1 minimum, 3 maximum in Container Apps |

## Startup Dependency Chain

1. Azure deployment provisions the logging workspace and Container Apps environment, SQL Server/database, and storage resources.
2. The Container App receives its system-assigned identity and SQL connection string; role assignments grant ACR pull and storage data access.
3. During application startup, EF Core migrations run before requests are served, unless `IsTestEnvironment` is enabled.
4. No explicit readiness probe or retry/wait mechanism was identified in the application or deployment configuration.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| SQL administrator password | Database credential | Bicep-generated expression; value masked |
| Production SQL access | Managed identity authentication | Connection string uses Azure AD Default; no password in the app connection string |
| Local development database | LocalDB trusted connection | No password configured |
| `.env.example` | Deployment input template | Resource group placeholder only; no credential value |

### Secrets Provisioning Workflow

The Bicep template provisions a SQL administrator credential and configures the container app with an Azure AD Default SQL connection string. A system-assigned managed identity is configured as the SQL Active Directory administrator. No Key Vault or other dedicated secret store is wired into the application configuration. The deployment template also provides Blob Storage settings, though the application code does not currently consume them.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| None identified | N/A | N/A |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---:|---|
| Target framework | .NET 9.0 | `PhotoAlbum.csproj`, `PhotoAlbum.Tests.csproj` |
| Entity Framework Core SQL Server | 9.0.9 | `PhotoAlbum.csproj` |
| Entity Framework Core Design | 9.0.9 | `PhotoAlbum.csproj` |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum.csproj` |
| .NET SDK container image | 9.0 | `Dockerfile` |
| ASP.NET Core runtime container image | 9.0 | `Dockerfile` |
| Test framework | xUnit 2.9.2 | `PhotoAlbum.Tests.csproj` |
