# Configuration & Externalized Settings Inventory

Configuration is sourced from the standard ASP.NET Core JSON files, environment variables, and launch profiles. Development and production do not define separate database profiles; user secrets are enabled by the project but no secret provisioning integration is configured.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Default connection, upload settings, logging, allowed hosts |
| Development settings | JSON | `PhotoAlbum/appsettings.Development.json` | Development logging and detailed errors |
| Launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | HTTP/HTTPS URLs and Development environment |
| User secrets | .NET User Secrets | Project `UserSecretsId` | Supported by project; no secret values referenced in checked-in configuration |
| Environment variables | ASP.NET Core configuration | Runtime environment | Can override JSON settings; `ASPNETCORE_ENVIRONMENT` selects environment |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build` default or `-c Debug` | Development build | None specific |
| Release | `-c Release` | Optimized build | None specific |

No conditional compilation symbols or custom MSBuild profiles were identified.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | Launch profile sets `ASPNETCORE_ENVIRONMENT=Development` | `appsettings.json`, `appsettings.Development.json` | Detailed errors and verbose ASP.NET Core / EF Core logging |
| Other environments | `ASPNETCORE_ENVIRONMENT` or hosting environment | `appsettings.json` plus matching optional environment file | No checked-in production-specific overrides |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB `PhotoAlbumDb`, trusted connection | All | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | All | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | JPEG, PNG, GIF, WebP | All | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | All | `appsettings.json`; not consumed by current upload service |
| `FileUpload:UploadPath` | `wwwroot/uploads` | All | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | All | `appsettings.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | All | `appsettings.json` |
| `DetailedErrors` | Not set | Development | `appsettings.Development.json`: `true` |
| `Logging:LogLevel:Default` | Overrides default with `Debug` | Development | `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | Overrides default with `Information` | Development | `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | `Information` | Development | `appsettings.Development.json` |
| `AllowedHosts` | `*` | All | `appsettings.json` |
| `IsTestEnvironment` | `false` when absent | Test/runtime override | Read by `Program.cs` to skip startup migrations |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | ASP.NET Core runtime; launch profiles use HTTP 5134 and HTTPS 7055 | Not specified | Not specified |

## Startup Dependency Chain

1. ASP.NET Core loads configuration and builds the service container.
2. The application ensures the upload directory exists.
3. Unless `IsTestEnvironment` is set, it connects to SQL Server and applies pending EF Core migrations before serving requests.
4. No health-check wait mechanism or external service startup dependency is configured.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection string | Uses a trusted LocalDB connection; no password is present in the checked-in value |
| Project User Secrets ID | Secret store identifier | Identifier is configured, but no secret values or provisioning workflow are checked in |

### Secrets Provisioning Workflow

No Key Vault, Vault, AWS Secrets Manager, managed identity, or deployment-time secret binding was identified. ASP.NET Core User Secrets is available for local development; production secret provisioning is not documented in the repository.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Configuration; disables startup database migrations when true |

No feature-flag framework or rollout toggles were identified.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---:|---|
| .NET target framework | `net9.0` | `PhotoAlbum/PhotoAlbum.csproj` |
| ASP.NET Core | 9.0 shared framework | `Microsoft.NET.Sdk.Web` project SDK |
| EF Core Design | 9.0.9 | `PhotoAlbum/PhotoAlbum.csproj` |
| EF Core SQL Server | 9.0.9 | `PhotoAlbum/PhotoAlbum.csproj` |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum/PhotoAlbum.csproj` |
| .NET SDK | No `global.json` pin identified | Repository files |
