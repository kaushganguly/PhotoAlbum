# Configuration & Externalized Settings Inventory

PhotoAlbum uses two `appsettings.json` files (base + Development override) as its only configuration sources, with no external config server, secret store, or environment-variable-driven secrets workflow.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| appsettings.json | JSON config file | `PhotoAlbum/appsettings.json` | Base configuration; always loaded |
| appsettings.Development.json | JSON config file | `PhotoAlbum/appsettings.Development.json` | Loaded when `ASPNETCORE_ENVIRONMENT=Development`; overrides logging levels and enables detailed errors |
| launchSettings.json | IDE/CLI launch profile | `PhotoAlbum/Properties/launchSettings.json` | Local development only; not deployed. Defines `http` (port 5134) and `https` (ports 7055/5134) profiles, both setting `ASPNETCORE_ENVIRONMENT=Development` |
| User Secrets | .NET User Secrets | UserSecretsId: `28fdd5b1-4b72-4763-98cc-ac5ebb3f280d` | Not used in the repository; available for overriding connection strings locally without committing credentials |

No Spring Cloud Config, Azure App Configuration, AWS AppConfig, Consul, Vault, or Key Vault integrations are present.

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Manual (`dotnet build` default) | Local development build; includes debug symbols | None beyond standard SDK |
| Release | Manual (`dotnet build -c Release`) | Optimized production artifact; no debug symbols | None beyond standard SDK |

No custom MSBuild properties, conditional compilation symbols, or multi-target configurations are defined in the project files.

## Runtime Profiles

| Profile | Activation Method | Config Files Loaded | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` (set in launchSettings.json) | `appsettings.json` + `appsettings.Development.json` | `DetailedErrors=true`; log levels: Default→Debug, AspNetCore→Information, EF Core→Information |
| Production (implied) | `ASPNETCORE_ENVIRONMENT` not set or set to `Production` | `appsettings.json` only | No production-specific override file; relies entirely on base settings |

No `appsettings.Production.json`, `appsettings.Staging.json`, or other environment-specific overrides exist in the repository. The application will run with LocalDB and debug-level logging if `ASPNETCORE_ENVIRONMENT` is not set for a production deployment.

## Properties Inventory

### PhotoAlbum

| Property Key | Default Value | Profile | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Server=(localdb)\mssqllocaldb;Database=PhotoAlbumDb;Trusted_Connection=true;MultipleActiveResultSets=true` | All | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` (10 MB) | All | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | `["image/jpeg","image/png","image/gif","image/webp"]` | All | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | All | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | All | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` (base) / `Debug` (Development) | All / Development | `appsettings.json` / `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` (base) / `Information` (Development) | All / Development | `appsettings.json` / `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | _(not set)_ / `Information` (Development) | Development only | `appsettings.Development.json` |
| `AllowedHosts` | `*` | All | `appsettings.json` |
| `DetailedErrors` | _(not set)_ / `true` (Development) | Development only | `appsettings.Development.json` |
| `IsTestEnvironment` | _(not set / false)_ | Test only | Set programmatically in `WebApplicationFactory` test setup to skip EF migrations |

Additionally, `Program.cs` reads `FormOptions` limits directly from code (not from config):
- `MultipartBodyLengthLimit`: 10485760 (hard-coded)
- `ValueLengthLimit`: 10485760 (hard-coded)
- `MultipartBoundaryLengthLimit`: 128 (hard-coded)

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum Web | No JVM/CLR tuning; default .NET GC settings | Not specified (no Docker, K8s, or cloud manifests in repository) | 1 (no scaling config) |

No Dockerfile, docker-compose.yml, Kubernetes manifests, or cloud deployment templates (Bicep, ARM, Terraform) are present in the repository. Resource requirements have not been specified.

## Startup Dependency Chain

1. **ASP.NET Core host** starts and registers services (DI container, EF Core, FormOptions).
2. **`wwwroot/uploads/` directory** is created if it does not exist (synchronous, before middleware pipeline).
3. **EF Core Migrations** (`context.Database.MigrateAsync()`) are applied against SQL Server unless `IsTestEnvironment=true`. If migrations fail, the application throws and exits.
4. **Kestrel** begins listening on configured ports.

No wait-for-TCP mechanisms, readiness probes, health checks, or external dependency checks are in place. If SQL Server is unavailable at startup, the application fails to start with an unhandled exception.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | Plain text in `appsettings.json` — contains no password (uses Windows Integrated Security / Trusted Connection for LocalDB) |

### Secrets Provisioning Workflow

No secrets provisioning workflow is implemented. The only sensitive-adjacent configuration is the database connection string, which relies on Windows Integrated Security (Trusted Connection) for the LocalDB development instance — no password is present in the config file.

For production deployments, the connection string would need to be supplied via an environment variable (`ConnectionStrings__DefaultConnection`), Azure App Service connection string override, Azure Key Vault, or another secret management mechanism. No such mechanism is configured or documented in the repository.

User Secrets (ID `28fdd5b1-4b72-4763-98cc-ac5ebb3f280d`) are registered in the project file but not used.

## Feature Flags

No feature flag framework (LaunchDarkly, .NET `Microsoft.FeatureManagement`, Unleash, or custom toggle) is configured. The only conditional behavior controlled by configuration is:

| Condition | Controlled By | Behavior |
|---|---|---|
| Skip EF migrations | `IsTestEnvironment` (bool, config key) | `true` skips `MigrateAsync()` on startup; used by `WebApplicationFactory` in tests |

No `@ConditionalOnProperty`, `IFeatureManager`, or A/B testing flags are present.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| Target Framework | net9.0 | `PhotoAlbum/PhotoAlbum.csproj` |
| ASP.NET Core | 9.0 (implicit via SDK) | `Microsoft.NET.Sdk.Web` |
| Entity Framework Core | 9.0.9 | `PhotoAlbum.csproj` PackageReference |
| EF Core SQL Server Provider | 9.0.9 | `PhotoAlbum.csproj` PackageReference |
| EF Core Design | 9.0.9 | `PhotoAlbum.csproj` PackageReference (build-time) |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum.csproj` PackageReference |
| .NET SDK (build environment) | 10.0.301 | Installed on build agent |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| EF Core InMemory (test) | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| coverlet.collector | 6.0.2 | `PhotoAlbum.Tests.csproj` |
