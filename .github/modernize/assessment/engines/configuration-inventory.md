# Configuration & Externalized Settings Inventory

The project uses a small set of .NET configuration sources centered around appsettings files, launch profiles, and optional user secrets. Environment-specific behavior is primarily handled through Development overrides.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| appsettings.json | Application config | `PhotoAlbum/appsettings.json` | Base connection string, upload limits, logging, allowed hosts |
| appsettings.Development.json | Environment override | `PhotoAlbum/appsettings.Development.json` | Development logging verbosity and detailed errors |
| launchSettings.json | Runtime profile launcher | `PhotoAlbum/Properties/launchSettings.json` | Local URL bindings and `ASPNETCORE_ENVIRONMENT` |
| User Secrets | Secret store reference | `PhotoAlbum.csproj` (`UserSecretsId`) | Local developer secret storage supported |
| .env.example | Sample env file | repository root | Example Azure-related environment variables |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build` default local build | Developer diagnostics and local iteration | Standard SDK pipeline |
| Release | `dotnet build -c Release` | Optimized production-ready build | Standard SDK pipeline |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` | `appsettings.json` + `appsettings.Development.json` | Detailed errors and elevated logging |
| Non-Development | Default when env var not Development | `appsettings.json` | HSTS/exception handler branch in startup |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB connection string | Base | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Base | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | jpeg/png/gif/webp | Base | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | Base | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Base | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | Base | `appsettings.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Base | `appsettings.json` |
| `DetailedErrors` | `true` | Development override | `appsettings.Development.json` |
| `Logging:LogLevel:Default` | `Debug` | Development override | `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | `Information` | Development override | `appsettings.Development.json` |
| `AllowedHosts` | `*` | Base | `appsettings.json` |
| `IsTestEnvironment` | `false` (implicit if missing) | Test override by tests | runtime configuration lookup in `Program.cs` |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum Web | .NET runtime; no explicit startup flags detected | Not explicitly configured | 1 (single-process app assumption) |

## Startup Dependency Chain

1. PhotoAlbum Web → waits for local filesystem path creation (`wwwroot/uploads`) during startup.
2. PhotoAlbum Web → applies EF Core database migrations before handling requests.
3. Once migrations succeed, Razor Pages and static file pipeline become available.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Connection string | Appsettings value (no password in default sample) |
| `UserSecretsId` | Secret-store pointer | Project file reference to local user secret store |

### Secrets Provisioning Workflow

Secrets can be supplied via .NET configuration providers (appsettings, environment variables, or user secrets). Development environments may use User Secrets keyed by the configured `UserSecretsId`; production secret flow is not explicitly codified in the repository.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | false when unset | Configuration key consumed at startup |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Target Framework | net9.0 | `PhotoAlbum.csproj`, `PhotoAlbum.Tests.csproj` |
| ASP.NET Core | 9.0 (implicit via SDK) | `Microsoft.NET.Sdk.Web` |
| EF Core SQL Server | 9.0.9 | `PhotoAlbum.csproj` |
| EF Core Design | 9.0.9 | `PhotoAlbum.csproj` |
| ImageSharp | 3.1.11 | `PhotoAlbum.csproj` |
| Test SDK | 17.12.0 | `PhotoAlbum.Tests.csproj` |
