# Configuration & Externalized Settings Inventory

PhotoAlbum uses application settings, launch profiles, project properties, and deployment templates as configuration sources. Local user secrets are enabled by a project identifier, while deployment configuration includes Azure SQL and container settings.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Connection string, upload limits/types/path, logging, allowed hosts |
| Development settings | JSON | `PhotoAlbum/appsettings.Development.json` | Development logging and detailed errors |
| Launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | Development environment and local HTTP/HTTPS URLs |
| Project settings | MSBuild | `PhotoAlbum/PhotoAlbum.csproj` | Target framework, nullable mode, and UserSecretsId |
| Test configuration | C# test fixture | `PhotoAlbum.Tests/Unit/Services/PhotoServiceTests.cs` | In-memory database and upload settings |
| Container build | Dockerfile | `Dockerfile` | .NET 9 runtime/build images, app port 8080 |
| Azure deployment | Azure Developer CLI | `azure.yaml`, `infra/` | Container Apps deployment and infrastructure configuration |

No external configuration server or dedicated Key Vault reference was found in the application configuration.

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default `dotnet build` configuration | Local development and debugging | Standard .NET SDK |
| Release | `-c Release` in Docker build/publish | Optimized container build | Standard .NET SDK |

No custom conditional compilation symbols or project-specific MSBuild profiles were found.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` in launch profiles | `appsettings.json`, `appsettings.Development.json` | Detailed errors and more verbose logging |
| Production/default | Hosting environment | `appsettings.json` plus environment variables | Exception handler and HSTS are enabled when not Development |
| Tests | Test fixture and `IsTestEnvironment` configuration | In-memory configuration in tests | EF Core InMemory and temporary upload path; suppresses startup migrations when true |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB connection targeting `PhotoAlbumDb` | All; can be overridden by environment configuration | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | All | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | JPEG, PNG, GIF, WebP | All | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | All | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | All | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | Development overrides to `Debug` | Application settings |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Development overrides to `Information` | Application settings |
| `DetailedErrors` | `false` when unspecified | Development sets `true` | `appsettings.Development.json` |
| `IsTestEnvironment` | `false` when unspecified | Test host may set `true` | Read by `Program.cs` |
| `ASPNETCORE_ENVIRONMENT` | Hosting default | Development launch profiles set `Development` | Launch settings/environment |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum container | .NET 9 ASP.NET runtime; listens on container port 8080 | No explicit allocation found | Not specified in the application Dockerfile |

No CPU, memory, JVM, or instance-count limits were identified in the reviewed application configuration.

## Startup Dependency Chain

1. ASP.NET Core starts and creates the `wwwroot/uploads` directory if absent.
2. Unless `IsTestEnvironment` is true, the app creates a DI scope and applies EF Core migrations.
3. Database migration failures are logged to standard output and rethrown, preventing normal startup.
4. After initialization, the app serves requests through HTTPS redirection, static-file middleware, routing, and Razor Pages.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection configuration | LocalDB trusted connection; no password present in checked-in settings |
| `PhotoAlbum.csproj` `UserSecretsId` | .NET user-secrets namespace | ID present; no secret values checked in |
| `infra/main.json` SQL administrator password variable | Deployment credential | `[MASKED: derived by deterministic ARM expression]` |
| Azure SQL connection configuration | Database authentication | Azure deployment uses Microsoft Entra authentication; no password embedded in connection string |

### Secrets Provisioning Workflow

Local development uses .NET user secrets as an available external store, but no application secret values are present in the committed appsettings files. Azure deployment configures database access through Microsoft Entra authentication. The infrastructure template also defines a SQL administrator credential expression; the security assessment records that expression as predictable because it derives from stable deployment identifiers.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Application configuration; when true, startup migrations are skipped |

No feature-flag framework or rollout service was found.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| Target framework | .NET 9.0 | Both project files |
| ASP.NET Core | 9.0 | Web SDK target framework |
| Entity Framework Core | 9.0.9 | Application package references |
| SixLabors.ImageSharp | 3.1.11 | Application package reference |
| .NET SDK/runtime container | 9.0 | Dockerfile |
| Local development endpoints | HTTP 5134, HTTPS 7055 | `launchSettings.json` |
