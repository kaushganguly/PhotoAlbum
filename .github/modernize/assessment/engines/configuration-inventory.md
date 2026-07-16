# Configuration & Externalized Settings Inventory

The project uses ASP.NET Core JSON configuration files plus launch profiles and environment variables, with a relatively small set of externalized settings focused on database and file upload behavior.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| appsettings.json | Application config | `PhotoAlbum/appsettings.json` | Base settings: connection string, upload limits, logging, hosts |
| appsettings.Development.json | Environment override | `PhotoAlbum/appsettings.Development.json` | Development logging/detail overrides |
| launchSettings.json | Launch profile config | `PhotoAlbum/Properties/launchSettings.json` | Local URLs and `ASPNETCORE_ENVIRONMENT=Development` |
| User Secrets | Secret reference | `UserSecretsId` in `PhotoAlbum.csproj` | Development-time secret storage hook |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build -c Debug` (default local) | Developer build configuration | Standard SDK toolchain |
| Release | `dotnet build -c Release` | Optimized production build | Standard SDK toolchain |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` via launch profile | `appsettings.json` + `appsettings.Development.json` | Detailed errors and verbose logging |
| Non-Development | Environment name not Development | `appsettings.json` | Production exception handler + HSTS path in Program |
| Test flag | `IsTestEnvironment=true` config value | Any config source that sets it | Skips automatic DB migrations on startup |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB connection string | Base | appsettings.json |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Base | appsettings.json |
| `FileUpload:AllowedMimeTypes` | jpeg/png/gif/webp | Base | appsettings.json |
| `FileUpload:MaxFilesPerUpload` | `10` | Base | appsettings.json |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Base | appsettings.json |
| `Logging:LogLevel:Default` | `Information` (base), `Debug` (dev) | Base + Development override | appsettings.json, appsettings.Development.json |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` (base), `Information` (dev) | Base + Development override | appsettings.json, appsettings.Development.json |
| `AllowedHosts` | `*` | Base | appsettings.json |
| `IsTestEnvironment` | `false` when absent | Optional override | Configuration providers/environment |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | ASP.NET Core runtime, launch profile URLs (`http://localhost:5134`, `https://localhost:7055`) | Not explicitly configured | 1 instance (local process) |

## Startup Dependency Chain

1. Web host starts and loads configuration.
2. Upload directory is created if missing.
3. Database migration runs unless `IsTestEnvironment=true`.
4. Razor Pages endpoints become available.

No external service readiness dependency chain is configured.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection string | appsettings/User Secrets (value masked in docs) |
| `UserSecretsId` | Secret store pointer | Project file reference |

### Secrets Provisioning Workflow

For local development, secrets can be injected through ASP.NET Core user secrets or environment-based configuration overrides. The runtime resolves configuration through ASP.NET Core providers and binds values into service setup (DbContext, upload settings).

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | false | Configuration value / environment override |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET target framework | net9.0 | `PhotoAlbum.csproj` |
| ASP.NET Core Web SDK | net9.0 SDK web project | `PhotoAlbum.csproj` |
| Entity Framework Core SqlServer | 9.0.9 | `PhotoAlbum.csproj` |
| Entity Framework Core Design | 9.0.9 | `PhotoAlbum.csproj` |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum.csproj` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` |
