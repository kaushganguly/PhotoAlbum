# Configuration & Externalized Settings Inventory

PhotoAlbum uses two `appsettings.json` files (base + Development override) as its only configuration sources, with no external config server, secret store, or feature flag framework.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|--------|------|--------------|-------|
| appsettings.json | JSON file | `PhotoAlbum/appsettings.json` | Base configuration; always loaded |
| appsettings.Development.json | JSON file (environment override) | `PhotoAlbum/appsettings.Development.json` | Loaded when `ASPNETCORE_ENVIRONMENT=Development`; overrides log levels and enables detailed errors |
| launchSettings.json | Development-only local settings | `PhotoAlbum/Properties/launchSettings.json` | Sets `ASPNETCORE_ENVIRONMENT=Development`; defines HTTP (port 5134) and HTTPS (port 7055) launch profiles. Never deployed. |
| Environment variables | Runtime environment | Host environment | Standard ASP.NET Core env-var override chain (e.g., `ConnectionStrings__DefaultConnection`); none explicitly documented in code |
| User Secrets | Developer machine only | `secrets.json` (UserSecretsId: `28fdd5b1-4b72-4763-98cc-ac5ebb3f280d`) | Available for local connection-string overrides; not used in CI/CD |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---------|-----------|---------|--------------------------|
| Debug | Default in development (MSBuild) | Includes debug symbols, no optimization | Standard SDK defaults |
| Release | Manual (`dotnet build -c Release` or CI) | Optimized output, no debug symbols | Standard SDK defaults |

No custom MSBuild properties or conditional compilation symbols are defined in the `.csproj` files beyond the standard SDK defaults.

## Runtime Profiles

| Profile | Activation Method | Config Files Loaded | Key Overrides |
|---------|-----------------|-------------------|--------------|
| Development | `ASPNETCORE_ENVIRONMENT=Development` (set via launchSettings.json) | `appsettings.json` + `appsettings.Development.json` | `DetailedErrors=true`; log levels raised to `Debug` (Default) and `Information` (ASP.NET Core, EF Core) |
| Production (default) | `ASPNETCORE_ENVIRONMENT` not set, or set to `Production` | `appsettings.json` only | HSTS and exception handler enabled; log levels at `Information` (Default) and `Warning` (ASP.NET Core) |
| Test | `IsTestEnvironment=true` injected by test host | `appsettings.json` + in-memory overrides | EF Core in-memory provider; auto-migration suppressed |

## Properties Inventory

### PhotoAlbum (main application)

| Property Key | Default Value | Profile Override | Source |
|-------------|--------------|-----------------|--------|
| `ConnectionStrings:DefaultConnection` | `Server=(localdb)\\mssqllocaldb;Database=PhotoAlbumDb;Trusted_Connection=true;MultipleActiveResultSets=true` | None defined | appsettings.json |
| `FileUpload:MaxFileSizeBytes` | `10485760` (10 MB) | None | appsettings.json |
| `FileUpload:AllowedMimeTypes` | `["image/jpeg","image/png","image/gif","image/webp"]` | None | appsettings.json |
| `FileUpload:MaxFilesPerUpload` | `10` | None | appsettings.json |
| `FileUpload:UploadPath` | `wwwroot/uploads` | None | appsettings.json |
| `Logging:LogLevel:Default` | `Information` | Development: `Debug` | appsettings.json / appsettings.Development.json |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Development: `Information` | appsettings.json / appsettings.Development.json |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | _(not set — inherits Default)_ | Development: `Information` | appsettings.Development.json |
| `AllowedHosts` | `*` | None | appsettings.json |
| `DetailedErrors` | _(not set — false)_ | Development: `true` | appsettings.Development.json |
| `IsTestEnvironment` | `false` | Test host: `true` | Injected by test `WebApplicationFactory` |

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory / CPU | Instance Count |
|---------|----------------|-------------|----------------|
| PhotoAlbum Web | No explicit JVM/CLR heap overrides; default .NET GC sizing | Not specified (no Docker or K8s manifests present) | 1 (no scaling configuration) |

No Docker Compose, Kubernetes, or Azure App Service deployment manifests are present in the repository. There are no CPU/memory limit declarations.

## Startup Dependency Chain

1. **PhotoAlbum Web** starts and builds the DI container.
2. On first request (or eagerly at startup), `Program.cs` calls `context.Database.MigrateAsync()` to apply any pending EF Core migrations to SQL Server LocalDB.
3. The uploads directory (`wwwroot/uploads`) is created if it does not exist.
4. The application begins serving requests.

There is no config server, no service discovery server, no message broker, and no external dependency with a health-check or wait mechanism. If the SQL Server LocalDB is unavailable at startup, `MigrateAsync()` throws and the process terminates.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage |
|-----------------|------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string (Windows-auth, LocalDB) | Plain text in `appsettings.json` — no password because Trusted Connection / Windows Auth is used |
| `UserSecretsId: 28fdd5b1-4b72-4763-98cc-ac5ebb3f280d` | .NET User Secrets identifier | Developer machine only (`%APPDATA%\Microsoft\UserSecrets\`); not in source control |

No API keys, OAuth client secrets, or third-party credentials are present. The LocalDB connection uses Windows Integrated Authentication (no password in connection string). No Key Vault, Vault, or AWS Secrets Manager references are configured.

### Secrets Provisioning Workflow

Currently there is no formal secrets provisioning workflow. The application relies on Windows Integrated Authentication for the LocalDB connection, which requires no password. For a production deployment (e.g., Azure SQL), the connection string would need to be supplied via environment variables or Azure Key Vault references — neither is currently configured. Developer overrides can be placed in User Secrets without touching `appsettings.json`.

## Feature Flags

No feature flag framework (e.g., Microsoft.FeatureManagement, LaunchDarkly, Unleash) is configured. No `@ConditionalOnProperty` equivalents or custom toggle mechanisms are present. The `IsTestEnvironment` flag in `appsettings.json` is the only boolean switch used to conditionally alter behavior (suppress DB migrations in the test host).

| Flag Name | Default | Controlled By |
|-----------|---------|--------------|
| `IsTestEnvironment` | `false` | Injected by test `WebApplicationFactory`; read in `Program.cs` |

## Framework & Runtime Versions

| Component | Version | Source |
|-----------|---------|--------|
| ASP.NET Core (Razor Pages) | 9.0 | `<TargetFramework>net9.0</TargetFramework>` in `PhotoAlbum.csproj` |
| .NET Runtime | 9.0 | `<TargetFramework>net9.0</TargetFramework>` |
| Entity Framework Core (SQL Server) | 9.0.9 | `PhotoAlbum.csproj` PackageReference |
| Entity Framework Core (Design) | 9.0.9 | `PhotoAlbum.csproj` PackageReference (build-time only) |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum.csproj` PackageReference |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` PackageReference |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.9 | `PhotoAlbum.Tests.csproj` PackageReference |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.9 | `PhotoAlbum.Tests.csproj` PackageReference |
| coverlet.collector | 6.0.2 | `PhotoAlbum.Tests.csproj` PackageReference |
| Build tool | dotnet SDK 10.0.301 | Host machine; solution targets net9.0 |
