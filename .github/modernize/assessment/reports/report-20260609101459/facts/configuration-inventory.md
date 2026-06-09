# Configuration & Externalized Settings Inventory

PhotoAlbum uses a small but varied configuration surface that combines ASP.NET Core JSON settings, launch profiles, user secrets metadata, deployment descriptors, and infrastructure-generated environment variables. Secrets are expected to be externalized rather than hardcoded in committed application settings.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Base connection string, upload limits, allowed MIME types, and logging defaults |
| Development overrides | JSON | `PhotoAlbum/appsettings.Development.json` | Enables detailed errors and more verbose logging |
| Launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | Defines local URLs and `ASPNETCORE_ENVIRONMENT=Development` |
| User secrets metadata | MSBuild property | `PhotoAlbum/PhotoAlbum.csproj` | References a development secrets store via `UserSecretsId` |
| Deployment environment template | ENV template | `.env.example` | Placeholder for Azure deployment resource group values |
| Azure Developer CLI config | YAML | `azure.yaml` | Describes the `web` service, container build, infra path, and deployment hooks |
| Container build | Dockerfile | `Dockerfile` | Defines .NET SDK/runtime images and exposed port 8080 |
| Infrastructure output | Generated ARM JSON | `infra/main.json` | Contains environment variable mappings such as `ConnectionStrings__DefaultConnection` |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| `Debug` | Default local build | Developer-friendly compilation and test execution | Standard .NET SDK toolchain |
| `Release` | `dotnet build/publish -c Release` | Production-oriented build and publish output | Standard .NET SDK toolchain |
| Docker multi-stage publish | `docker build` using `Dockerfile` | Produces a container image for deployment | `mcr.microsoft.com/dotnet/sdk:9.0`, `mcr.microsoft.com/dotnet/aspnet:9.0` |
| Azure deployment workflow | `azd up` / deploy hooks | Packages app plus Bicep infrastructure for Azure | `azure.yaml` + infra hooks |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | Launch profile sets `ASPNETCORE_ENVIRONMENT=Development` | `appsettings.json`, `appsettings.Development.json`, user secrets | Detailed errors, debug logging, local HTTP/HTTPS URLs |
| Non-Development | Default when environment variable is absent or different | `appsettings.json` plus external environment values | Exception handler, HSTS, production connection string override expected |
| Test host | Test configuration sets `IsTestEnvironment=true` in memory when needed | In-memory test configuration | Skips startup migrations in integration-style test scenarios |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `(localdb)\mssqllocaldb` SQL Server connection string | Base; overridden in deployment | `appsettings.json`, environment variable `ConnectionStrings__DefaultConnection` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Base, tests may override | `appsettings.json`, in-memory test config |
| `FileUpload:AllowedMimeTypes` | JPEG, PNG, GIF, WebP | Base, tests may override | `appsettings.json`, in-memory test config |
| `FileUpload:MaxFilesPerUpload` | `10` | Base | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Base, tests override to temp directory | `appsettings.json`, in-memory test config |
| `Logging:LogLevel:Default` | `Information` | Development overrides to `Debug` | `appsettings.json`, `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Development overrides to `Information` | `appsettings.json`, `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | `Information` | Development only | `appsettings.Development.json` |
| `AllowedHosts` | `*` | Base | `appsettings.json` |
| `ASPNETCORE_ENVIRONMENT` | `Development` in launch profile | Development | `launchSettings.json`, deployment environment |
| `IsTestEnvironment` | `false` if absent | Test-specific | In-memory test configuration or external config |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| `PhotoAlbum` | No explicit runtime flags in repo; container exposes port `8080` | Not specified in repo | Not specified in repo |

## Startup Dependency Chain

1. `Program` builds the ASP.NET Core host and registers Razor Pages, EF Core, and `PhotoService`.
2. The app ensures `wwwroot/uploads` exists before handling requests.
3. Unless `IsTestEnvironment` is true, startup opens a DI scope and applies pending EF Core migrations.
4. After migrations succeed, middleware is configured and Razor Pages endpoints become available.

There are no Docker Compose `depends_on`, Kubernetes readiness probes, or inter-service wait scripts in the current runtime path because this is a single-service application.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection string | Base config or environment override `[MASKED]` |
| `UserSecretsId` | Developer secret store reference | ASP.NET Core user secrets metadata `[MASKED]` |
| Deployment resource group variables | Azure deployment values | `.env` / deployment environment `[MASKED]` |

### Secrets Provisioning Workflow

During local development, the project uses ASP.NET Core user secrets metadata to keep sensitive overrides out of source control. In deployed environments, infrastructure and deployment tooling are expected to provide values such as `ConnectionStrings__DefaultConnection` through environment variables or platform settings rather than committed JSON files. The web application consumes those values directly at startup when configuring the EF Core SQL Server connection.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` when unset | Configuration value supplied by tests or hosting environment |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET target framework | `net9.0` | `PhotoAlbum/PhotoAlbum.csproj` |
| ASP.NET Core Web SDK | `net9.0` | `PhotoAlbum/PhotoAlbum.csproj` |
| EF Core SqlServer | `9.0.9` | `PhotoAlbum/PhotoAlbum.csproj` |
| EF Core Design | `9.0.9` | `PhotoAlbum/PhotoAlbum.csproj` |
| SixLabors.ImageSharp | `3.1.11` | `PhotoAlbum/PhotoAlbum.csproj` |
| Test SDK | `17.12.0` | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
| xUnit | `2.9.2` | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
| Docker runtime image | `mcr.microsoft.com/dotnet/aspnet:9.0` | `Dockerfile` |
| Docker build image | `mcr.microsoft.com/dotnet/sdk:9.0` | `Dockerfile` |
