# Configuration & Externalized Settings Inventory

Configuration is concentrated in ASP.NET Core JSON settings, launch profiles, Docker metadata, and Azure Developer CLI deployment files; no external configuration server or dedicated secret store binding was detected in code.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Default connection string, upload limits, allowed MIME types, logging, allowed hosts |
| Development settings | JSON | `PhotoAlbum/appsettings.Development.json` | Development logging verbosity and detailed errors |
| Launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | Local HTTP and HTTPS URLs plus `ASPNETCORE_ENVIRONMENT` |
| Project file | MSBuild XML | `PhotoAlbum/PhotoAlbum.csproj` | Target framework, nullable settings, user secrets ID, package references |
| Test project file | MSBuild XML | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | Test target framework, package references, project reference |
| Dockerfile | Container build | `Dockerfile` | ASP.NET runtime and SDK images, exposed container port, build and publish steps |
| Azure Developer CLI | YAML | `azure.yaml` | Container app service definition and infra hook scripts |
| Infrastructure files | Bicep and scripts | `infra/` | Azure resource provisioning inputs and deployment hooks |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default local `dotnet build` configuration | Development compilation | .NET SDK build targets |
| Release | `dotnet build -c Release` or Docker build stage | Optimized publish output | .NET SDK publish targets |
| Docker build | `docker build` | Container image creation | `mcr.microsoft.com/dotnet/sdk:9.0` and `aspnet:9.0` images |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Default | No explicit environment | `appsettings.json` | SQL Server LocalDB, upload settings, information logging |
| Development | `ASPNETCORE_ENVIRONMENT=Development` from launch profiles | `appsettings.json`, `appsettings.Development.json` | Detailed errors and more verbose logging |
| Container | Docker entry point | Published application files and environment supplied by host | Listens on exposed container port 8080 unless host overrides URLs |
| Test | Test host sets `IsTestEnvironment` | Test configuration | Skips startup migrations and uses test data provider setup |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server LocalDB connection for PhotoAlbumDb | Default | `PhotoAlbum/appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Default | `PhotoAlbum/appsettings.json` |
| `FileUpload:AllowedMimeTypes` | `image/jpeg`, `image/png`, `image/gif`, `image/webp` | Default | `PhotoAlbum/appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | Default | `PhotoAlbum/appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Default | `PhotoAlbum/appsettings.json` |
| `Logging:LogLevel:Default` | `Information`; `Debug` in Development | Default, Development | `appsettings.json`, `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning`; `Information` in Development | Default, Development | `appsettings.json`, `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | Not set; `Information` in Development | Development | `appsettings.Development.json` |
| `AllowedHosts` | `*` | Default | `PhotoAlbum/appsettings.json` |
| `DetailedErrors` | Not set; `true` in Development | Development | `PhotoAlbum/appsettings.Development.json` |
| `ASPNETCORE_ENVIRONMENT` | `Development` in launch profiles | Local run | `PhotoAlbum/Properties/launchSettings.json` |
| `IsTestEnvironment` | `false` when absent | Test | Test host configuration |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | `dotnet PhotoAlbum.dll` in container; local launch URLs `http://localhost:5134` and `https://localhost:7055` | Not specified | Not specified |
| PhotoAlbum Docker image | Exposes port 8080 | Not specified | One container per host instance by default |

## Startup Dependency Chain

1. PhotoAlbum starts and builds the ASP.NET Core service provider.
2. The application ensures the local uploads directory exists.
3. Unless `IsTestEnvironment` is true, the application creates a scoped `PhotoAlbumContext` and applies EF Core migrations.
4. Middleware is configured for exception handling or HSTS, HTTPS redirection, static files, routing, authorization, and Razor Pages.
5. No Docker Compose `depends_on`, Kubernetes readiness probes, or external service wait scripts were detected in the application source.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection string | App settings; no password present in checked-in default value |
| `UserSecretsId` | Local development secret store identifier | Project file identifier only; secret values not present |

### Secrets Provisioning Workflow

No runtime secret provisioning workflow is implemented in application code. Local development can use ASP.NET Core user secrets because the project declares a `UserSecretsId`, and deployment-time values may be supplied by Azure infrastructure hooks or hosting environment variables, but no Key Vault, Vault, or cloud secret binding is configured in the application files reviewed.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` when absent | Test host configuration; controls whether startup migrations run |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---:|---|
| .NET target framework | net9.0 | `PhotoAlbum/PhotoAlbum.csproj`, `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
| ASP.NET Core runtime image | 9.0 | `Dockerfile` |
| .NET SDK image | 9.0 | `Dockerfile` |
| Entity Framework Core SQL Server | 9.0.9 | `PhotoAlbum/PhotoAlbum.csproj` |
| Entity Framework Core Design | 9.0.9 | `PhotoAlbum/PhotoAlbum.csproj` |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum/PhotoAlbum.csproj` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
| Microsoft.NET.Test.Sdk | 17.12.0 | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
