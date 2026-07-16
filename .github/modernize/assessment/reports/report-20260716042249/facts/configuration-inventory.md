# Configuration & Externalized Settings Inventory

PhotoAlbum uses two `appsettings` JSON files and a `launchSettings.json` for its configuration, with no external config server, secret store, or feature flag framework.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| appsettings.json | JSON (ASP.NET Core) | `PhotoAlbum/appsettings.json` | Base/production configuration; connection string and file upload limits |
| appsettings.Development.json | JSON (ASP.NET Core) | `PhotoAlbum/appsettings.Development.json` | Development overrides; verbose logging |
| launchSettings.json | JSON (dev-only) | `PhotoAlbum/Properties/launchSettings.json` | Local launch profiles (http/https); not deployed |
| User Secrets | .NET User Secrets | Project UserSecretsId: `28fdd5b1-4b72-4763-98cc-ac5ebb3f280d` | Configured for dev-time secret overrides; no secrets currently defined |

No Spring Cloud Config, Azure App Configuration, Consul, HashiCorp Vault, or AWS Secrets Manager is configured.

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default for `dotnet build` without `-c` | Development build with debug symbols | All packages included |
| Release | `-c Release` flag | Optimized production build | All packages included; compiler optimizations enabled |

No custom MSBuild profiles or conditional compilation symbols are defined in the `.csproj` files.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` (launchSettings.json) | appsettings.json + appsettings.Development.json | `DetailedErrors: true`; log levels set to Debug/Information |
| Production (default) | No `ASPNETCORE_ENVIRONMENT` set (or any non-Development value) | appsettings.json only | Standard Information-level logging; HSTS and exception handler enabled |

## Properties Inventory

**PhotoAlbum — appsettings.json**

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Server=(localdb)\mssqllocaldb;Database=PhotoAlbumDb;...` | All | appsettings.json |
| `FileUpload:MaxFileSizeBytes` | `10485760` (10 MB) | All | appsettings.json |
| `FileUpload:AllowedMimeTypes` | `["image/jpeg","image/png","image/gif","image/webp"]` | All | appsettings.json |
| `FileUpload:MaxFilesPerUpload` | `10` | All | appsettings.json |
| `FileUpload:UploadPath` | `wwwroot/uploads` | All | appsettings.json |
| `Logging:LogLevel:Default` | `Information` | All (overridden in Dev) | appsettings.json / appsettings.Development.json |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | All (overridden in Dev) | appsettings.json / appsettings.Development.json |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | _(not set)_ | Development | appsettings.Development.json |
| `AllowedHosts` | `*` | All | appsettings.json |
| `IsTestEnvironment` | `false` (implicit) | Test only | Set programmatically in test host; skips DB migration |

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | No JVM/heap settings (native .NET); `ASPNETCORE_ENVIRONMENT` set via launchSettings.json | Not specified | 1 (no scaling configuration) |

No Docker `mem_limit`, Kubernetes resource requests/limits, or process-level memory settings are defined.

## Startup Dependency Chain

1. **ASP.NET Core host builds** — services are registered (Razor Pages, DbContext, PhotoService, FormOptions).
2. **`wwwroot/uploads/` directory** is created if missing.
3. **EF Core Migrations** are applied automatically via `context.Database.MigrateAsync()` — requires SQL Server LocalDB to be running before the application starts.
4. **HTTP pipeline configured** — static files, routing, authorization, Razor Pages mapped.
5. **Application begins serving requests.**

If SQL Server LocalDB is unavailable at startup, `MigrateAsync()` throws and the application fails to start (fast-fail pattern). No health-check endpoint or wait-for-dependency mechanism is configured.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection string (Windows auth, no password) | appsettings.json (plain text — uses Windows Trusted Connection) |

No passwords, API keys, or tokens are present in configuration files. The connection string uses Windows Integrated Security (`Trusted_Connection=true`), so no database password is stored.

### Secrets Provisioning Workflow

No secret management workflow is implemented. Configuration is supplied entirely through `appsettings.json` files. The project has a User Secrets ID configured (`28fdd5b1-4b72-4763-98cc-ac5ebb3f280d`), allowing developers to use `dotnet user-secrets` locally, but no secrets are currently defined.

For production deployments, connection strings and any sensitive values should be supplied via environment variables or a secret store (e.g., Azure Key Vault with managed identity).

## Feature Flags

No feature flag framework is configured. No `@ConditionalOnProperty`, `IFeatureManager`, LaunchDarkly, or custom toggle logic is present.

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Configuration key; set to `true` in the test host to skip EF migrations |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Runtime | 9.0 | `<TargetFramework>net9.0</TargetFramework>` in .csproj |
| ASP.NET Core | 9.0 | Included with .NET 9.0 SDK |
| Entity Framework Core | 9.0.9 | `Microsoft.EntityFrameworkCore.SqlServer` package |
| SixLabors.ImageSharp | 3.1.11 | NuGet package reference |
| xUnit | 2.9.2 | Test project NuGet reference |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.9 | Test project NuGet reference |
| Bootstrap | 5.x (bundled) | `wwwroot/lib/bootstrap/` (static file, not NuGet) |
| jQuery | 3.x (bundled) | `wwwroot/lib/jquery/` (static file, not NuGet) |
