# Configuration & Externalized Settings Inventory

Configuration is supplied by two application settings files, launch profiles, project/build files, and Azure deployment artifacts. Runtime environments are Development and Production; the application uses a local connection string by default and deployment infrastructure supplies cloud connection settings and a managed identity.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Default connection, upload settings, logging, allowed hosts |
| Development settings | JSON | `PhotoAlbum/appsettings.Development.json` | Development logging levels and detailed errors |
| Launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | HTTP and HTTPS local profiles; sets Development environment |
| Application project | MSBuild XML | `PhotoAlbum/PhotoAlbum.csproj` | Target framework, package versions, nullable and implicit usings |
| Test project | MSBuild XML | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | Test target framework and test package versions |
| Container build | Dockerfile | `Dockerfile` | .NET 9 SDK build and ASP.NET runtime image; exposes port 8080 |
| Azure Developer CLI | YAML | `azure.yaml` | Service mapping, Bicep infrastructure location, and deployment hooks |
| Azure infrastructure | Bicep and parameters | `infra/main.bicep`, `infra/main.parameters.json` | Azure resources, runtime settings, managed identity, and resource sizing |
| Generated ARM template | JSON | `infra/main.json` | Compiled deployment template corresponding to Bicep |

No external configuration server or secret-store reference was identified.

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default local build configuration | Development builds | No profile-specific packages declared |
| Release | `-c Release` or deployment build | Optimized publish/container build | No profile-specific packages declared |

Both projects target `net9.0`. No conditional compilation symbols or package-specific build profiles were found.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` in launch profiles | `appsettings.json`, `appsettings.Development.json` | Detailed errors; more verbose ASP.NET Core and EF Core logging |
| Production | Set `ASPNETCORE_ENVIRONMENT=Production` by deployment infrastructure | `appsettings.json` plus deployment environment variables | SQL connection is supplied by the container app; standard production exception/HSTS behavior |
| Tests | Test host/configuration; optional `IsTestEnvironment` setting | Base settings plus test configuration | Startup migrations are skipped only when the test flag is true; tests use EF Core InMemory |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server LocalDB connection | Default/Development | `appsettings.json`; cloud deployment overrides with `ConnectionStrings__DefaultConnection` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | All | `appsettings.json`; fallback in `PhotoService` is 10485760 |
| `FileUpload:AllowedMimeTypes` | `image/jpeg`, `image/png`, `image/gif`, `image/webp` | All | `appsettings.json`; same fallback list in `PhotoService` |
| `FileUpload:MaxFilesPerUpload` | `10` | All | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | All | `appsettings.json`; same fallback in `PhotoService` |
| `Logging:LogLevel:Default` | `Information` | Default | `appsettings.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Default | `appsettings.json` |
| `Logging:LogLevel:Default` | `Debug` | Development | `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Information` | Development | `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | `Information` | Development | `appsettings.Development.json` |
| `DetailedErrors` | `true` | Development | `appsettings.Development.json` |
| `AllowedHosts` | `*` | Default | `appsettings.json` |
| `IsTestEnvironment` | `false` when unset | Tests | Read from application configuration by `Program.cs`; suppresses startup migrations when true |
| `ConnectionStrings__DefaultConnection` | Not specified in code deployment | Production | Set in Bicep as an Azure SQL connection string using Active Directory Default authentication |
| `AzureStorageBlob__Endpoint` | Not specified in application settings | Production | Supplied by Bicep; no corresponding application use was identified |
| `AzureStorageBlob__ContainerName` | `photos` in Bicep | Production | Supplied by Bicep; no corresponding application use was identified |
| `ASPNETCORE_ENVIRONMENT` | Not explicitly set by application | Development/Production | Launch profiles use Development; Bicep sets Production |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum local | `dotnet` with ASP.NET Core 9 | Not specified | Not specified |
| PhotoAlbum container | `dotnet PhotoAlbum.dll`; container port 8080 | 0.5 CPU, 1 GiB | 1–3 replicas |

No JVM settings or local process memory limits were found.

## Startup Dependency Chain

1. Azure deployment provisions SQL Server/database, storage, container environment, and the container app; role assignments grant the app managed identity access to the registry and blob storage.
2. The app starts and attempts to apply EF Core migrations unless `IsTestEnvironment` is true.
3. Database availability is therefore required for application startup outside tests. No explicit database readiness wait or retry policy was found.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection string | LocalDB integrated trusted connection in configuration; no password included |
| SQL administrator password | Database bootstrap credential | Generated dynamically in Bicep; value intentionally omitted |
| `ConnectionStrings__DefaultConnection` | Production SQL connection string | Passed to the container app; uses Active Directory Default authentication rather than a password |

### Secrets Provisioning Workflow

The Bicep deployment provisions a system-assigned managed identity for the container app and configures the SQL connection string for Active Directory Default authentication. It also generates an SQL administrator credential during provisioning, but the application connection uses the managed identity. No Key Vault, external secret manager, or user-secrets provisioning workflow was identified.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Application configuration; when true, skips startup database migrations |

No feature-flag framework or rollout configuration was found.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET target framework | .NET 9 (`net9.0`) | Both project files |
| ASP.NET Core runtime image | 9.0 | `Dockerfile` |
| .NET SDK build image | 9.0 | `Dockerfile` |
| Entity Framework Core Design | 9.0.9 | `PhotoAlbum.csproj` |
| Entity Framework Core SQL Server | 9.0.9 | `PhotoAlbum.csproj` |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum.csproj` |
| ASP.NET Core MVC Testing | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| Entity Framework Core InMemory | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` |
| .NET SDK used for assessment | 10.0.401 | Installed environment |
