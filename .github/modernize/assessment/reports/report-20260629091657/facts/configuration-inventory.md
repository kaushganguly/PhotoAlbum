# Configuration & Externalized Settings Inventory

PhotoAlbum uses 3 JSON configuration files (base + one environment override) plus a `launchSettings.json` for local development, with no external config server, no secret store integration, and no feature flag framework.

## Configuration Sources

| Source | Type | Path / Location | Notes |
|---|---|---|---|
| `appsettings.json` | JSON file | `PhotoAlbum/appsettings.json` | Base configuration; always loaded |
| `appsettings.Development.json` | JSON file (env override) | `PhotoAlbum/appsettings.Development.json` | Loaded when `ASPNETCORE_ENVIRONMENT=Development`; overrides logging levels and enables `DetailedErrors` |
| `launchSettings.json` | Dev-only JSON | `PhotoAlbum/Properties/launchSettings.json` | Used by `dotnet run` and Visual Studio only; not deployed; sets `ASPNETCORE_ENVIRONMENT=Development` for local profiles |
| User Secrets | .NET User Secrets | `secrets.json` (local, outside repo) | `UserSecretsId: 28fdd5b1-4b72-4763-98cc-ac5ebb3f280d`; used in Development to override sensitive values without committing them to source control |
| `Dockerfile` | Container image spec | `Dockerfile` | Multi-stage build; sets `EXPOSE 8080`; no environment variable injection at image-build time |

No Spring Cloud Config, Azure App Configuration, AWS AppConfig, Consul KV, HashiCorp Vault, or Azure Key Vault integration is present.

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies / Properties |
|---|---|---|---|
| Debug | Default for `dotnet run` / IDE | Development build; no optimizations; full debug symbols | No conditional compilation symbols defined |
| Release | Manual: `-c Release` (used in `Dockerfile`) | Production/container build; optimizations enabled; `UseAppHost=false` for self-contained publish | `dotnet publish -c Release /p:UseAppHost=false` |

No MSBuild conditional item groups, `Directory.Build.props`, or `Directory.Packages.props` are present.

## Runtime Profiles

| Profile | Activation Method | Config Files Loaded | Key Overrides vs Base |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` (set in `launchSettings.json`) | `appsettings.json` + `appsettings.Development.json` | `DetailedErrors: true`; log levels: Default→Debug, AspNetCore→Information, EF Core→Information |
| Production (default) | `ASPNETCORE_ENVIRONMENT` not set or any non-Development value | `appsettings.json` only | Exception handler page `/Error` enabled; HSTS headers active; no detailed errors |

No `appsettings.Production.json`, `appsettings.Staging.json`, or other environment-specific files exist.

## Properties Inventory

### PhotoAlbum Web

| Property Key | Default Value | Profile | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Server=(localdb)\\mssqllocaldb;Database=PhotoAlbumDb;Trusted_Connection=true;MultipleActiveResultSets=true` | All | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` (10 MB) | All | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | `["image/jpeg","image/png","image/gif","image/webp"]` | All | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | All | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | All | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` (base) / `Debug` (Development) | Base / Development | `appsettings.json` / `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` (base) / `Information` (Development) | Base / Development | `appsettings.json` / `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | _(not set)_ (base) / `Information` (Development) | Development only | `appsettings.Development.json` |
| `AllowedHosts` | `*` | All | `appsettings.json` |
| `DetailedErrors` | _(not set)_ (base) / `true` (Development) | Development only | `appsettings.Development.json` |
| `IsTestEnvironment` | `false` (implicit) | Test only | Set programmatically in test `WebApplicationFactory`; suppresses DB migration on startup |

The form options `MultipartBodyLengthLimit`, `ValueLengthLimit`, and `MultipartBoundaryLengthLimit` are hardcoded in `Program.cs` (10 MB, not read from configuration).

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum Web (local `dotnet run`) | `ASPNETCORE_ENVIRONMENT=Development`; HTTP port 5134, HTTPS port 7055 | No explicit limit | 1 |
| PhotoAlbum Web (Docker / container) | `EXPOSE 8080`; no environment variable defaults in `Dockerfile` | No `mem_limit` or resource constraints configured | 1 |

No JVM heap settings (not applicable — .NET runtime). No Kubernetes manifests or Docker Compose files with resource constraints are present.

## Startup Dependency Chain

1. **Application starts** → creates `wwwroot/uploads/` directory if missing
2. **EF Core migrations** → `context.Database.MigrateAsync()` is called before the HTTP pipeline opens (skipped when `IsTestEnvironment=true`)
3. **HTTP pipeline opens** → application is ready to serve requests

There are no external dependencies (Config Server, Service Bus, discovery registry) that must be available before startup. If the SQL Server LocalDB instance is unavailable, the migration step throws and the process exits (fail-fast). No readiness probes, `dockerize` wait-for-TCP, or health check endpoints are configured.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection string (Windows Integrated Auth — no password in string) | `appsettings.json` (trusted connection, no credentials embedded) |
| User Secrets (`UserSecretsId: 28fdd5b1-4b72-4763-98cc-ac5ebb3f280d`) | Developer override store | Local `%APPDATA%\Microsoft\UserSecrets\` (not committed to repo) |

The connection string uses Windows Trusted Authentication (`Trusted_Connection=true`) — no database password is present in configuration. There are no API keys, OAuth client secrets, or external service credentials configured.

### Secrets Provisioning Workflow

No automated secrets provisioning workflow is in place. The application relies on Windows Integrated Authentication for the SQL Server connection in development. For production cloud deployment, a secrets provisioning strategy (e.g., Azure Key Vault with a managed identity, and the connection string injected as an environment variable or App Service connection string setting) has not yet been implemented. The `.NET User Secrets` mechanism provides local developer isolation during development.

## Feature Flags

No feature flag framework (e.g., `Microsoft.FeatureManagement`, LaunchDarkly, Unleash) is configured. The only conditional runtime behavior is the `IsTestEnvironment` boolean flag that suppresses EF Core migrations during integration testing.

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | `appsettings.json` override in test `WebApplicationFactory` (`builder.UseSetting("IsTestEnvironment", "true")`) |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Runtime / SDK | 9.0 | `PhotoAlbum.csproj` `<TargetFramework>net9.0</TargetFramework>` |
| ASP.NET Core (Razor Pages) | 9.0 | Included in `Microsoft.NET.Sdk.Web` |
| Entity Framework Core | 9.0.9 | `Microsoft.EntityFrameworkCore.SqlServer` package reference |
| EF Core Design (build-time) | 9.0.9 | `Microsoft.EntityFrameworkCore.Design` (private assets) |
| SixLabors.ImageSharp | 3.1.11 | Package reference in `PhotoAlbum.csproj` |
| Docker base image (runtime) | `mcr.microsoft.com/dotnet/aspnet:9.0` | `Dockerfile` |
| Docker base image (build) | `mcr.microsoft.com/dotnet/sdk:9.0` | `Dockerfile` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| Build tool | MSBuild (via `dotnet build`) | `PhotoAlbum.sln` / SDK-style projects |
