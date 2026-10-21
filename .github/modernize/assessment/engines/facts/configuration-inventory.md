# Configuration & Externalized Settings Inventory

PhotoAlbum uses JSON application settings, development launch profiles, and project-level framework/package declarations. No external configuration service or secrets provider is configured; database and upload settings have local defaults.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON runtime configuration | `PhotoAlbum/appsettings.json` | Default connection, upload limits and types, logging, allowed hosts |
| Development settings | JSON runtime configuration | `PhotoAlbum/appsettings.Development.json` | Development diagnostics and logging levels |
| Launch profiles | Local launch configuration | `PhotoAlbum/Properties/launchSettings.json` | HTTP/HTTPS URLs and `ASPNETCORE_ENVIRONMENT=Development` |
| Project files | Build configuration | `PhotoAlbum/PhotoAlbum.csproj`, `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | Target frameworks and package versions |
| IIS hosting | Hosting configuration | `PhotoAlbum/Web.config` | ASP.NET Core Module hosting settings |
| Deployment example | Environment setup template | `.env.example` | Documents Azure resource group input; no secret values |
| User Secrets metadata | .NET project metadata | `PhotoAlbum/PhotoAlbum.csproj` | A User Secrets ID is set; no secret values are present in the repository |

No external config repository, Azure App Configuration, Key Vault, Vault, or AWS Secrets Manager integration was detected.

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | MSBuild configuration (default for local builds) | Development build | No conditional package additions found |
| Release | MSBuild configuration | Optimized build and publish | No conditional package additions found |

Both projects target `net9.0`; no conditional compilation symbols or custom build configurations were identified.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Default | No environment-specific file selected | `PhotoAlbum/appsettings.json` | SQL Server LocalDB, file upload settings, Information logging |
| Development | `ASPNETCORE_ENVIRONMENT=Development` in launch profiles | `appsettings.json`, `appsettings.Development.json` | Detailed errors and more verbose ASP.NET Core and EF Core logs |
| Test | `IsTestEnvironment=true` configuration value set by the test host | Application settings plus test configuration | Skips startup database migrations; test project uses EF Core InMemory |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server LocalDB database `PhotoAlbumDb` with trusted connection | Default and Development unless overridden | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Default and Development unless overridden | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | `image/jpeg`, `image/png`, `image/gif`, `image/webp` | Default and Development unless overridden | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | Default and Development unless overridden | `appsettings.json`; no use was found in application code |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Default and Development unless overridden | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | Development overrides to `Debug` | `appsettings.json`, `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Development overrides to `Information` | `appsettings.json`, `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | Not specified | `Information` in Development | `appsettings.Development.json` |
| `AllowedHosts` | `*` | Default unless overridden | `appsettings.json` |
| `IsTestEnvironment` | `false` when absent | Test host may set `true` | Read from configuration in `Program.cs` |

Environment variables can override .NET configuration values using the standard configuration provider naming convention, but no explicit environment-variable placeholders are declared.

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | ASP.NET Core runtime; development launch uses `http://localhost:5134` and `https://localhost:7055` | Not specified | Not specified |

No JVM settings, container memory limits, CPU allocations, or replica counts were found in the repository.

## Startup Dependency Chain

1. ASP.NET Core builds configuration and registers Razor Pages, `PhotoAlbumContext`, and scoped `PhotoService`.
2. Startup creates `wwwroot/uploads` if it is missing.
3. Outside test mode, startup creates a service scope and applies EF Core migrations; SQL Server must be reachable for this step.
4. The app configures exception handling/HSTS outside Development, HTTPS redirection, static files, routing, and Razor Pages before serving requests.

No explicit readiness probe, database wait loop, or startup retry policy was found.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection configuration | Trusted SQL Server LocalDB connection; no password is stored in the checked-in value |
| .NET User Secrets ID | Local secret-store identifier | ID is project metadata; no secret value is present |

### Secrets Provisioning Workflow

No production secret-provisioning workflow or external secret store is configured in the repository. .NET User Secrets metadata is present, but no secret values or documented secret binding flow were found. Deployments must provide any non-local connection details through environment-specific configuration or an external deployment secret mechanism.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | .NET configuration; when true, startup skips EF migrations |
| `ASPNETCORE_ENVIRONMENT` | Framework default when not set | Launch profile/environment; selects Development settings and middleware behavior |

No dedicated feature-flag framework was found.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET target framework | .NET 9 (`net9.0`) | Both project files |
| ASP.NET Core | 9.x shared framework | `Microsoft.NET.Sdk.Web` project |
| Entity Framework Core | 9.0.9 | Application project |
| EF Core SQL Server provider | 9.0.9 | Application project |
| SixLabors.ImageSharp | 3.1.11 | Application project |
| xUnit | 2.9.2 | Test project |
| Microsoft.NET.Test.Sdk | 17.12.0 | Test project |
