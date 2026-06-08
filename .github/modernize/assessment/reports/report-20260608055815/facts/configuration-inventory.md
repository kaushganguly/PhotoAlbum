# Configuration & Externalized Settings Inventory

The configuration model relies on standard ASP.NET Core JSON files, launch profiles, and optional user secrets. Runtime settings primarily control database connectivity, upload limits, and logging verbosity.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| `appsettings.json` | Base runtime config | `PhotoAlbum/appsettings.json` | Connection string, upload options, logging |
| `appsettings.Development.json` | Environment override | `PhotoAlbum/appsettings.Development.json` | Development logging/detail overrides |
| `launchSettings.json` | Local launch profile | `PhotoAlbum/Properties/launchSettings.json` | HTTP/HTTPS URLs, `ASPNETCORE_ENVIRONMENT` |
| User secrets | Local secret store | Referenced by `UserSecretsId` in csproj | Suitable for local secret overrides |
| Test in-memory config | Programmatic config | `PhotoAlbum.Tests/Unit/Services/PhotoServiceTests.cs` | Overrides upload path/mime/size during tests |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build/test` default | Local diagnostics | Standard SDK and package references |
| Release | `dotnet build -c Release` | Optimized deployment build | Standard SDK and package references |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` | `appsettings.json` + `appsettings.Development.json` | Detailed errors and verbose logging |
| Non-Development | Default when env var unset | `appsettings.json` | Production-style exception handler and HSTS |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB connection string | Base | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Base, test override | `appsettings.json`, test in-memory config |
| `FileUpload:AllowedMimeTypes` | jpeg/png/gif/webp | Base, test override | `appsettings.json`, test in-memory config |
| `FileUpload:MaxFilesPerUpload` | `10` | Base | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Base, test override | `appsettings.json`, test in-memory config |
| `Logging:LogLevel:Default` | `Information` | Base, dev override | `appsettings.json`, `appsettings.Development.json` |
| `AllowedHosts` | `*` | Base | `appsettings.json` |
| `IsTestEnvironment` | `false` implicit | Optional external override | runtime configuration lookup in `Program.cs` |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | ASP.NET Core runtime options from host defaults; no explicit custom startup flags checked in | Not explicitly configured in repo | 1 (single web app) |

## Startup Dependency Chain

1. Web host starts and DI container is built.
2. Upload directory is created if missing.
3. Database migrations execute (`Database.MigrateAsync`) unless `IsTestEnvironment=true`.
4. If migrations fail, startup throws and app does not accept requests.
5. On success, middleware pipeline starts serving Razor Pages.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` (when overridden) | Database credential/connection | User secrets or environment (expected) |
| `UserSecretsId` | Local secret indirection | Project metadata only, values outside repo |

### Secrets Provisioning Workflow

Local development can supply secret values via `dotnet user-secrets` tied to `UserSecretsId`; deployment environments are expected to inject secrets via environment variables or managed platform settings. The application reads configuration through standard ASP.NET Core configuration providers and binds values at startup.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Configuration provider value |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET target framework | net9.0 | `PhotoAlbum.csproj` |
| ASP.NET Core SDK | `Microsoft.NET.Sdk.Web` | `PhotoAlbum.csproj` |
| Entity Framework Core SQL Server | 9.0.9 | `PhotoAlbum.csproj` |
| Entity Framework Core Design | 9.0.9 | `PhotoAlbum.csproj` |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum.csproj` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` |
