# Configuration & Externalized Settings Inventory

PhotoAlbum has two JSON-based configuration sources and one launch settings file; there are no external config servers, Vault integrations, or feature flag frameworks.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| `appsettings.json` | JSON | `PhotoAlbum/appsettings.json` | Base configuration — applies to all environments |
| `appsettings.Development.json` | JSON | `PhotoAlbum/appsettings.Development.json` | Development overrides — detailed logging and error pages |
| `launchSettings.json` | JSON (local only) | `PhotoAlbum/Properties/launchSettings.json` | Dev launch profiles for `dotnet run`; not deployed |
| User Secrets | .NET User Secrets | `UserSecretsId: 28fdd5b1-...` (local dev only) | Provides a local dev secrets store; no secrets present in source code |

No Spring Cloud Config, Azure App Configuration, Consul KV, HashiCorp Vault, AWS Secrets Manager, Kubernetes ConfigMaps, or `.env` files are used.

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default for `dotnet build` without `-c` | Local development and testing build | No optimizations; debug symbols included |
| Release | `-c Release` flag | Production packaging | Compiler optimizations; no debug symbols |

No custom MSBuild profiles, conditional compilation symbols, or build-time feature switches are defined in the `.csproj` files.

## Runtime Profiles

| Profile | Activation Method | Config Files Applied | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` (set in `launchSettings.json`) | `appsettings.json` → `appsettings.Development.json` | `DetailedErrors=true`; `Logging.LogLevel.Default=Debug`; EF Core log level = Information |
| Production | Default when `ASPNETCORE_ENVIRONMENT` is unset or `Production` | `appsettings.json` only | Standard logging; HSTS and exception handler enabled via `app.Environment.IsDevelopment()` check |
| Test | `IsTestEnvironment=true` (set programmatically in test factory) | `appsettings.json` (in-memory DB replaces SQL Server) | Skips EF Core migration step at startup |

## Properties Inventory

### PhotoAlbum

| Property Key | Default Value | Environment Override | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Server=(localdb)\\mssqllocaldb;Database=PhotoAlbumDb;Trusted_Connection=true;MultipleActiveResultSets=true` | None configured | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` (10 MB) | None | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | `["image/jpeg","image/png","image/gif","image/webp"]` | None | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | None | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | None | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | `Debug` (Development) | `appsettings.json` / `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | `Information` (Development) | `appsettings.json` / `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | *(not set)* | `Information` (Development) | `appsettings.Development.json` |
| `DetailedErrors` | *(not set)* | `true` (Development) | `appsettings.Development.json` |
| `AllowedHosts` | `*` | None | `appsettings.json` |
| `IsTestEnvironment` | `false` | `true` (set by test host factory) | Programmatic / `appsettings.json` |

Form options (configured in `Program.cs` via code, not config files):

| Setting | Value |
|---|---|
| `MultipartBodyLengthLimit` | `10485760` (10 MB) |
| `ValueLengthLimit` | `10485760` (10 MB) |
| `MultipartBoundaryLengthLimit` | `128` |

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum Web | None specified — default .NET runtime settings | No explicit limits set | 1 (single-process, no container or orchestration config) |

No JVM heap flags, Kubernetes resource requests/limits, Docker Compose memory constraints, or autoscaling configuration is present.

## Startup Dependency Chain

1. **PhotoAlbum Web** starts and immediately calls `context.Database.MigrateAsync()` to apply any pending EF Core migrations.
   - If SQL Server LocalDB is unavailable, the migration will throw and the application will terminate (`throw` after logging).
   - There is no wait-for-TCP mechanism, readiness probe, or retry policy for the database connection.
   - In test environments (`IsTestEnvironment=true`), the migration step is skipped entirely.

No multi-service startup ordering, Docker Compose `depends_on`, Kubernetes readiness probes, or `dockerize` wait mechanisms are configured.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string (trusted/integrated auth) | `appsettings.json` (no password — uses Windows Integrated Authentication for LocalDB) |
| .NET User Secrets (`UserSecretsId: 28fdd5b1-4b72-4763-98cc-ac5ebb3f280d`) | Local dev secrets store | Local filesystem only; never committed to source control |

No passwords, API keys, tokens, or KeyVault/Vault/AWS Secrets Manager references are present in source code or config files. The LocalDB connection uses Trusted (Windows Integrated) Authentication so no credential is stored.

### Secrets Provisioning Workflow

There is no formal secrets provisioning workflow. The application relies on Windows Integrated Authentication for database access in development (no password required). For production deployments, connection string injection via environment variable (`ConnectionStrings__DefaultConnection`) or a cloud secret store (e.g., Azure Key Vault via managed identity) would need to be configured manually — no such mechanism is currently implemented.

## Feature Flags

No feature flag framework (LaunchDarkly, .NET `Microsoft.FeatureManagement`, custom `@ConditionalOnProperty`, etc.) is used. The only conditional runtime behavior is controlled by:

| Flag | Default | Controlled By | Effect |
|---|---|---|---|
| `IsTestEnvironment` | `false` | Set programmatically in `WebApplicationFactory` during tests | Skips `MigrateAsync()` at startup |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Runtime / SDK | 9.0 | `<TargetFramework>net9.0</TargetFramework>` in `PhotoAlbum.csproj` |
| ASP.NET Core (Razor Pages) | 9.0 | Included via `Microsoft.NET.Sdk.Web` SDK |
| Entity Framework Core (SQL Server) | 9.0.9 | `PhotoAlbum.csproj` `PackageReference` |
| Entity Framework Core Design | 9.0.9 | `PhotoAlbum.csproj` (build-time only) |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum.csproj` `PackageReference` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| Microsoft.NET.Test.Sdk | 17.12.0 | `PhotoAlbum.Tests.csproj` |
| coverlet.collector | 6.0.2 | `PhotoAlbum.Tests.csproj` |
| SQL Server LocalDB | (runtime, version not pinned) | Connection string in `appsettings.json` |
