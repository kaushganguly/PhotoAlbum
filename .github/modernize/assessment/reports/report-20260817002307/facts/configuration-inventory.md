# Configuration & Externalized Settings Inventory

The application uses a straightforward ASP.NET Core configuration model with JSON files, launch profiles, and optional user secrets during local development. Configuration is primarily local-file based with minimal profile sprawl.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| App settings | JSON | `PhotoAlbum/appsettings.json` | Base runtime configuration |
| Development overrides | JSON | `PhotoAlbum/appsettings.Development.json` | Development logging/detail overrides |
| Launch profile | JSON | `PhotoAlbum/Properties/launchSettings.json` | Dev run URLs and `ASPNETCORE_ENVIRONMENT` |
| User secrets | Secret store reference | `UserSecretsId` in `PhotoAlbum.csproj` | Developer-local secret storage linkage |
| Environment variables | Runtime source | Process environment | Standard ASP.NET Core configuration providers |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build` default for local dev | Development build with symbols | SDK defaults |
| Release | `dotnet build -c Release` | Optimized build for deployment | SDK defaults |
| Test project build | `dotnet test` on test csproj | Test execution and host bootstrapping | `Microsoft.NET.Test.Sdk`, `xunit` |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` (launch settings) | `appsettings.json` + `appsettings.Development.json` | Detailed errors and debug/info logging |
| Non-development | Default when env var unset or set otherwise | `appsettings.json` | Baseline connection/file-upload/logging settings |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB connection string | Base | appsettings.json |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Base | appsettings.json |
| `FileUpload:AllowedMimeTypes` | jpeg/png/gif/webp list | Base | appsettings.json |
| `FileUpload:MaxFilesPerUpload` | `10` | Base | appsettings.json |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Base | appsettings.json |
| `Logging:LogLevel:Default` | `Information` | Base | appsettings.json |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Base | appsettings.json |
| `DetailedErrors` | `true` | Development | appsettings.Development.json |
| `Logging:LogLevel:Default` | `Debug` | Development override | appsettings.Development.json |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Information` | Development override | appsettings.Development.json |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | `Information` | Development override | appsettings.Development.json |
| `AllowedHosts` | `*` | Base | appsettings.json |
| `ASPNETCORE_ENVIRONMENT` | `Development` in launch profile | Launch profile | launchSettings.json |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | .NET runtime options not explicitly set in repo | Not explicitly defined | Single process by default |
| PhotoAlbum.Tests | Test host defaults | Not explicitly defined | On-demand during `dotnet test` |

## Startup Dependency Chain

1. PhotoAlbum process starts and loads ASP.NET Core configuration providers.
2. Upload directory is created if missing.
3. Unless `IsTestEnvironment=true`, EF Core migrations run against configured SQL Server.
4. HTTP request pipeline starts and serves Razor Pages/static assets.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `UserSecretsId` | Local development secret indirection | `28fdd5b1-4b72-4763-98cc-ac5ebb3f280d` (identifier only) |
| `ConnectionStrings:DefaultConnection` | Potentially sensitive endpoint data | Stored in config file (no password in current default string) |

### Secrets Provisioning Workflow

Current workflow is local-first: base values come from `appsettings.json`, with optional override through user secrets and environment variables at runtime. No Key Vault, Vault, or cloud secrets manager integration is declared in the repository.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` (implicit when unset) | Configuration value used in `Program.cs` to skip migrations during tests |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET target framework (app) | net9.0 | `PhotoAlbum/PhotoAlbum.csproj` |
| .NET target framework (tests) | net9.0 | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
| ASP.NET Core Web SDK | `Microsoft.NET.Sdk.Web` | `PhotoAlbum/PhotoAlbum.csproj` |
| EF Core SqlServer | 9.0.9 | `PhotoAlbum/PhotoAlbum.csproj` |
| EF Core Design | 9.0.9 | `PhotoAlbum/PhotoAlbum.csproj` |
| ImageSharp | 3.1.11 | `PhotoAlbum/PhotoAlbum.csproj` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
