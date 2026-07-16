# Configuration & Externalized Settings Inventory

The configuration surface for PhotoAlbum is compact and mostly file-based, with one primary application settings file, development launch profiles, and deployment metadata for container and Azure hosting. Secret handling is minimal and relies on a user secrets identifier plus environment-specific overrides rather than a dedicated secret store in code.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| `appsettings.json` | ASP.NET Core app settings | `PhotoAlbum/appsettings.json` | Primary runtime configuration for connection string, upload limits, logging, and allowed hosts |
| `launchSettings.json` | Development launch profile | `PhotoAlbum/Properties/launchSettings.json` | Defines local HTTP/HTTPS URLs and sets `ASPNETCORE_ENVIRONMENT=Development` |
| Project file | Build/runtime metadata | `PhotoAlbum/PhotoAlbum.csproj` | Declares target framework, package versions, and `UserSecretsId` |
| Environment variables | External overrides | Process environment | Used by ASP.NET Core configuration stack and launch profiles |
| `.env.example` | Example deployment variables | repository root | Provides sample deployment environment values |
| `azure.yaml` | Deployment/service definition | repository root | Describes Azure Developer CLI service and infrastructure wiring |
| `Dockerfile` | Container runtime configuration | repository root | Defines exposed port and .NET base images |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default local build configuration | Developer builds and test execution | Standard .NET SDK toolchain |
| Release | Explicit `-c Release` build/publish | Production/container publish output | Standard .NET SDK toolchain |
| Docker multi-stage build | `docker build` | Restores, builds, and publishes the web application for container deployment | `mcr.microsoft.com/dotnet/sdk:9.0`, `mcr.microsoft.com/dotnet/aspnet:9.0` |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` from launch settings | `appsettings.json`, `launchSettings.json` | Local URLs `http://localhost:5134` and `https://localhost:7055` |
| Default / non-development | Absence of development override | `appsettings.json` | Enables exception handler and HSTS branch in startup |
| Test host | Test configuration flag | In-memory configuration during tests | Sets `IsTestEnvironment=true` to skip startup migrations |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Server=(localdb)\\mssqllocaldb;Database=PhotoAlbumDb;Trusted_Connection=true;MultipleActiveResultSets=true` | Default | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Default | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | `image/jpeg`, `image/png`, `image/gif`, `image/webp` | Default | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | Default | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Default | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | Default | `appsettings.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Default | `appsettings.json` |
| `AllowedHosts` | `*` | Default | `appsettings.json` |
| `ASPNETCORE_ENVIRONMENT` | `Development` in local profiles | Development | `launchSettings.json` / environment |
| `applicationUrl` | `http://localhost:5134` or `https://localhost:7055;http://localhost:5134` | Development | `launchSettings.json` |
| `IsTestEnvironment` | Not set by default | Test host only | In-memory test configuration |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum web app | No custom CLR startup switches detected; launches with ASP.NET Core defaults | Not specified in repository configuration | 1 by default |
| Container image | Exposes port `8080` | Not specified in Dockerfile or `azure.yaml` | Not specified |

## Startup Dependency Chain

1. PhotoAlbum web app → waits for local configuration to load from `appsettings.json` and environment variables.
2. PhotoAlbum web app → creates `wwwroot/uploads` directory before handling requests.
3. PhotoAlbum web app → applies EF Core migrations unless `IsTestEnvironment` is true.
4. PhotoAlbum web app → begins serving Razor Pages and static assets.

No container health checks, explicit readiness probes, or dependent services are defined in repository runtime configuration.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection string | Stored in `appsettings.json` with integrated security and no password |
| `UserSecretsId` | Development secret store binding | `28fdd5b1-4b72-4763-98cc-ac5ebb3f280d` in `PhotoAlbum.csproj` |

### Secrets Provisioning Workflow

At runtime, configuration is loaded from standard ASP.NET Core sources, allowing environment variables or user secrets to override file-based defaults. The repository does not show Key Vault, managed identity, or other centralized secret retrieval code; deployment tooling is expected to provide any production secrets externally if the application is modernized beyond LocalDB.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| None detected | n/a | No feature flag framework or conditional feature properties were found |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---:|---|
| .NET target framework | 9.0 | `PhotoAlbum/PhotoAlbum.csproj` |
| ASP.NET Core web SDK | 9.0 | `Microsoft.NET.Sdk.Web` in `PhotoAlbum.csproj` |
| EF Core SQL Server | 9.0.9 | `PhotoAlbum.csproj` |
| EF Core Design | 9.0.9 | `PhotoAlbum.csproj` |
| ImageSharp | 3.1.11 | `PhotoAlbum.csproj` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` |
| .NET test SDK | 17.12.0 | `PhotoAlbum.Tests.csproj` |
| ASP.NET runtime image | 9.0 | `Dockerfile` |
| .NET SDK image | 9.0 | `Dockerfile` |
