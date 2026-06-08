# Configuration & Externalized Settings Inventory

PhotoAlbum uses 4 configuration sources (two `appsettings.json` files, a `web.config`, and `launchSettings.json`) with no external config server or secrets store — connection strings and environment variables are injected at deployment time via Azure Container Apps environment variables.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| `appsettings.json` | JSON (base config) | `PhotoAlbum/appsettings.json` | Base configuration for all environments; defines connection string placeholder, file upload limits, and logging defaults |
| `appsettings.Development.json` | JSON (env override) | `PhotoAlbum/appsettings.Development.json` | Overrides for `Development` environment: detailed errors on, debug-level logging |
| `web.config` | XML | `PhotoAlbum/Web.config` | Legacy-style XML; contains a `connectionStrings` entry mirroring the dev connection string for IIS compatibility |
| `launchSettings.json` | JSON (dev-only) | `PhotoAlbum/Properties/launchSettings.json` | Local development launch profiles (`http` and `https`); not deployed |
| `.env.example` | Env template | `.env.example` | Documents Azure deployment environment variables (`RESOURCE_GROUP`); not loaded at runtime |
| `azure.yaml` | Azure Dev CLI (azd) | `azure.yaml` | Declares `photoalbum` as an azd application targeting Azure Container Apps via Bicep |
| `infra/main.bicep` | Bicep IaC | `infra/main.bicep` | Provisions Container Apps, SQL Server, Azure Blob Storage, and Container Registry |
| `infra/main.parameters.json` | Bicep parameters | `infra/main.parameters.json` | Parameterizes `AZURE_ENV_NAME`, `AZURE_LOCATION`, and `SERVICE_WEB_IMAGE_NAME` from environment |

No Spring Cloud Config, Azure App Configuration, HashiCorp Vault, or AWS Secrets Manager references detected.

## Build Profiles

| Profile | Activation | Purpose | Key Additions |
|---|---|---|---|
| Debug | Default in Visual Studio / `dotnet build` | Local development build with debug symbols | Full PDB output, no optimizations |
| Release | `-c Release` (explicit) | Optimized production build | Used in `Dockerfile` (`dotnet build -c Release`, `dotnet publish -c Release /p:UseAppHost=false`) |

No conditional compilation symbols or MSBuild property conditions beyond the standard `Debug`/`Release` pair are defined in the `.csproj` files.

## Runtime Profiles

| Profile | Activation Method | Config Files Loaded | Key Overrides vs Base |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` (set in `launchSettings.json`) | `appsettings.json` + `appsettings.Development.json` | `DetailedErrors=true`; log levels: Default→Debug, AspNetCore→Information, EF Core→Information |
| Production | `ASPNETCORE_ENVIRONMENT` absent or set to `Production` | `appsettings.json` only | Exception handler at `/Error`, HSTS enabled; EF Core / detailed error logging suppressed |
| Test (runtime flag) | `IsTestEnvironment=true` injected by test host | `appsettings.json` (overridden in-memory) | Skips `MigrateAsync()` on startup; EF Core in-memory provider used instead of SQL Server |

## Properties Inventory

### PhotoAlbum — all environments

| Property Key | Default Value | Profile Override | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Server=(localdb)\mssqllocaldb;Database=PhotoAlbumDb;Trusted_Connection=true;MultipleActiveResultSets=true` | Production: injected via Container Apps env var | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` (10 MB) | None | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | `["image/jpeg","image/png","image/gif","image/webp"]` | None | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | None | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | None | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | Development: `Debug` | `appsettings.json` / `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Development: `Information` | `appsettings.json` / `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | _(not set — inherits Default)_ | Development: `Information` | `appsettings.Development.json` |
| `DetailedErrors` | `false` (implicit) | Development: `true` | `appsettings.Development.json` |
| `AllowedHosts` | `*` | None | `appsettings.json` |
| `IsTestEnvironment` | `false` (implicit) | Test host: `true` | Set programmatically by test `WebApplicationFactory` |

### Azure Deployment (environment variable parameters)

| Variable | Default / Example | Source |
|---|---|---|
| `AZURE_ENV_NAME` | _(required)_ | `main.parameters.json` → azd environment |
| `AZURE_LOCATION` | _(required)_ | `main.parameters.json` → azd environment |
| `SERVICE_WEB_IMAGE_NAME` | `mcr.microsoft.com/azuredocs/containerapps-helloworld:latest` | `main.parameters.json`; overridden by `azd deploy` |

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory | CPU | Instance Count |
|---|---|---|---|---|
| PhotoAlbum (local dev) | None — default `dotnet run` | Not specified | Not specified | 1 |
| PhotoAlbum (Docker) | `ENTRYPOINT ["dotnet", "PhotoAlbum.dll"]`; port 8080 exposed | Not specified in `Dockerfile` | Not specified | 1 |
| PhotoAlbum (Azure Container Apps via Bicep) | `ASPNETCORE_ENVIRONMENT` and `ConnectionStrings__DefaultConnection` injected at provisioning time | Not explicitly set in Bicep; Container Apps defaults apply | Not explicitly set | 1 (default; no auto-scale rules defined) |

No JVM heap settings apply (.NET runtime). No explicit `DOTNET_GCHeapHardLimit` or memory limit environment variables are set.

## Startup Dependency Chain

The application has a single service with no external service registry or config server. The only startup ordering concern is:

1. **SQL Server must be reachable** before `MigrateAsync()` completes during application startup.
   - If SQL Server is unavailable, `MigrateAsync()` throws, the exception is logged to stdout, and the application terminates (fail-fast behavior).
   - No `dockerize`, Kubernetes readiness probe, or `depends_on` health check is configured — the application relies on retry logic at the infrastructure layer (e.g., Azure Container Apps restart policy).
2. **Local `wwwroot/uploads` directory** is created on startup if absent (`Directory.CreateDirectory(uploadsPath)`).

No health check endpoints are registered; there is no `/health` or `/healthz` probe for container orchestrators to use.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string (trusted in dev; password-based in production via Bicep-generated password) | Development: local `appsettings.json` (no password, Windows auth); Production: Azure Container Apps environment variable (value injected by azd / Bicep post-provision hook) |
| SQL admin password (Bicep) | Generated password for Azure SQL Server admin | `infra/main.bicep` — generated at provisioning time as `P@ssw0rd${uniqueString(...)}` and injected into Container Apps environment variable; **not stored in source control as a static value** |

> ⚠️ **Risk**: The SQL admin password generation formula in `main.bicep` (`P@ssw0rd${uniqueString(resourceGroup().id, environmentName)}`) is deterministic and readable in source control. Anyone with knowledge of the resource group ID and environment name can reconstruct the password. A secrets store (Azure Key Vault) should be used instead.

No API keys, OAuth credentials, or other secrets are present in configuration files. No Jasypt encryption or DPAPI-protected entries are used.

### Secrets Provisioning Workflow

1. Developer runs `azd provision` → Bicep deploys Azure SQL Server, Container Apps Environment, Container Registry.
2. Bicep generates the SQL admin password via `uniqueString(resourceGroup().id, environmentName)` and stores it as a Container Apps secret.
3. `azd deploy` builds and pushes the Docker image; the Container App revision is updated with the new image.
4. Post-provision hooks (`infra/hooks/postprovision.sh` / `.ps1`) run to perform any additional wiring (e.g., setting the `ConnectionStrings__DefaultConnection` environment variable on the Container App pointing to the Azure SQL DB).
5. On container startup, ASP.NET Core reads `ConnectionStrings__DefaultConnection` from the environment (double-underscore notation maps to JSON hierarchy), overriding the `appsettings.json` localdb default.

No managed identity or Key Vault integration is configured in the current codebase.

## Feature Flags

No feature flag framework is present. The single conditional startup behavior is:

| Flag | Default | Controlled By | Effect |
|---|---|---|---|
| `IsTestEnvironment` | `false` | Set programmatically in test `WebApplicationFactory` | `true` suppresses EF Core `MigrateAsync()` on startup |

No `Microsoft.FeatureManagement`, LaunchDarkly, Unleash, `@ConditionalOnProperty`, or custom feature toggle patterns are present.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Runtime | 9.0 | `PhotoAlbum.csproj` `<TargetFramework>net9.0</TargetFramework>` |
| ASP.NET Core | 9.0 (implicit, bundled with .NET 9) | `Microsoft.NET.Sdk.Web` SDK |
| Entity Framework Core | 9.0.9 | `PhotoAlbum.csproj` NuGet reference |
| EF Core SQL Server provider | 9.0.9 | `PhotoAlbum.csproj` NuGet reference |
| EF Core Design (tooling) | 9.0.9 | `PhotoAlbum.csproj` NuGet reference (private assets) |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum.csproj` NuGet reference |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| EF Core InMemory | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| Docker base image (runtime) | `mcr.microsoft.com/dotnet/aspnet:9.0` | `Dockerfile` |
| Docker base image (build) | `mcr.microsoft.com/dotnet/sdk:9.0` | `Dockerfile` |
| Azure Dev CLI (azd) schema | v1.0 | `azure.yaml` `$schema` reference |
| Bicep AVM module — Container Apps Env | `0.8.1` | `infra/main.bicep` |
| Bicep AVM module — SQL Server | `0.9.1` | `infra/main.bicep` |
| Bicep AVM module — Container Registry | `0.6.0` | `infra/main.bicep` |
