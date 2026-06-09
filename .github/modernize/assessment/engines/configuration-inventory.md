# Configuration & Externalized Settings Inventory

PhotoAlbum uses 3 configuration sources (appsettings JSON files + launchSettings for local dev + Web.config fallback) with two runtime profiles (Development and Production), and relies on environment variables injected by Azure Container Apps for cloud deployment — no external config server or secret store is integrated.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|--------|------|--------------|-------|
| appsettings.json | JSON (ASP.NET Core) | `PhotoAlbum/appsettings.json` | Base configuration; applies to all environments |
| appsettings.Development.json | JSON (ASP.NET Core) | `PhotoAlbum/appsettings.Development.json` | Development overrides; activated by `ASPNETCORE_ENVIRONMENT=Development` |
| launchSettings.json | JSON (dev-time only) | `PhotoAlbum/Properties/launchSettings.json` | Local dev launch profiles (http / https); NOT deployed |
| Web.config | XML | `PhotoAlbum/Web.config` | Legacy IIS/Windows-style connection string fallback; present alongside appsettings.json |
| .env.example | Template | `.env.example` | Documents required environment variables for Azure deployment (not loaded at runtime) |
| Environment variables (Azure Container Apps) | Env vars | Injected at runtime via Bicep IaC | `ConnectionStrings__DefaultConnection`, `AzureStorageBlob__Endpoint`, `AzureStorageBlob__ContainerName`, `ASPNETCORE_ENVIRONMENT` |
| Bicep IaC templates | Infrastructure | `infra/main.bicep`, `infra/main.parameters.json` | Provisions Azure resources and injects environment variables into the Container App |

No Spring Cloud Config, Azure App Configuration, HashiCorp Vault, AWS AppConfig, or Consul KV integration is present.

## Build Profiles

| Profile | Activation | Purpose | Key Settings |
|---------|-----------|---------|-------------|
| Debug | Default in `dotnet run` / Visual Studio | Local development with debug symbols | `<Configuration>Debug</Configuration>`, no publish optimizations |
| Release | `-c Release` flag (CI/CD, `dotnet publish`) | Production-ready optimized binary | Enables AOT-friendly trim settings; used in Dockerfile multi-stage build |

No conditional compilation symbols or MSBuild property overrides are declared in the `.csproj` beyond the standard SDK defaults.

## Runtime Profiles

| Profile | Activation Method | Config Files Loaded | Key Overrides |
|---------|-----------------|--------------------|--------------------|
| Development | `ASPNETCORE_ENVIRONMENT=Development` (set in launchSettings.json) | appsettings.json + appsettings.Development.json | Detailed errors enabled; EF Core logging at `Information`; default log level `Debug` |
| Production | `ASPNETCORE_ENVIRONMENT=Production` (injected by Azure Container Apps via Bicep) | appsettings.json only | HSTS enabled; exception handler page active; standard log level `Information` |

No additional profile-specific files (e.g., `appsettings.Staging.json`) are present. The test project sets `IsTestEnvironment=true` programmatically to skip EF Core migrations.

## Properties Inventory

### PhotoAlbum (main application)

| Property Key | Default Value | Profile Override | Source |
|-------------|--------------|-----------------|--------|
| `ConnectionStrings:DefaultConnection` | `Server=(localdb)\mssqllocaldb;Database=PhotoAlbumDb;Trusted_Connection=true;MultipleActiveResultSets=true` | Production: Azure SQL connection string via env var `ConnectionStrings__DefaultConnection` | appsettings.json / env var |
| `FileUpload:MaxFileSizeBytes` | `10485760` (10 MB) | None | appsettings.json |
| `FileUpload:AllowedMimeTypes` | `["image/jpeg","image/png","image/gif","image/webp"]` | None | appsettings.json |
| `FileUpload:MaxFilesPerUpload` | `10` | None | appsettings.json |
| `FileUpload:UploadPath` | `wwwroot/uploads` | None | appsettings.json |
| `Logging:LogLevel:Default` | `Information` | Development: `Debug` | appsettings.json / appsettings.Development.json |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Development: `Information` | appsettings.json / appsettings.Development.json |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | _(not set)_ | Development: `Information` | appsettings.Development.json |
| `AllowedHosts` | `*` | None | appsettings.json |
| `DetailedErrors` | _(not set)_ | Development: `true` | appsettings.Development.json |
| `IsTestEnvironment` | `false` | Test: `true` (set programmatically) | Code / test configuration |
| `AzureStorageBlob:Endpoint` | _(not set)_ | Production: Azure Blob endpoint via env var | Azure Container Apps env injection |
| `AzureStorageBlob:ContainerName` | _(not set)_ | Production: `photos` via env var | Azure Container Apps env injection |
| `ASPNETCORE_ENVIRONMENT` | _(not set in appsettings)_ | Development: `Development` (launchSettings); Production: `Production` (Bicep) | Environment variable |

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory/CPU | Instance Count |
|---------|----------------|-----------|----------------|
| PhotoAlbum (local dev) | `dotnet run` — no custom JVM/CLR flags; ports 5134 (HTTP), 7055 (HTTPS) | Host system defaults | 1 |
| PhotoAlbum (Docker / Container App) | `dotnet PhotoAlbum.dll`; port 8080 exposed | Not specified in Bicep (Azure Container Apps defaults: 0.5 vCPU, 1 Gi) | 1 (no scaling rules defined) |

No `-Xms`/`-Xmx` equivalents (CLR heap flags) or custom GC settings are configured.

## Startup Dependency Chain

The application applies EF Core migrations on startup via `context.Database.MigrateAsync()`. This means:

1. **SQL Server** (or LocalDB) must be reachable before the application fully starts.
2. If the database is unavailable, the migration call throws and the process exits.

No explicit health-check probes, `dockerize` wait-for-TCP, Kubernetes readiness probes, or Docker Compose `depends_on` conditions are configured. For Azure Container Apps deployment, the SQL Server firewall rule `AllowAzureServices` is relied upon to permit connectivity. The container app's startup is a single-step deploy with no orchestrated wait mechanism.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage Method |
|-----------------|------|----------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | Dev: `(localdb)` Trusted Connection (no password); Production: plain-text env var injected by Bicep — **no Key Vault** |
| `sqlAdminPassword` (Bicep variable) | SQL Server admin password | Computed in Bicep as `P@ssw0rd${uniqueString(...)}` — deterministic from resource group ID; **not stored in Key Vault** |
| `AzureStorageBlob:Endpoint` | Azure Blob Storage endpoint URL | Plain-text env var injected by Bicep |

### Secrets Provisioning Workflow

Secrets are provisioned entirely via **Azure Developer CLI (`azd`) + Bicep IaC**:

1. The operator runs `azd up` (or `azd provision` + `azd deploy`).
2. Bicep computes the SQL admin password deterministically from the resource group ID and environment name.
3. Bicep injects `ConnectionStrings__DefaultConnection`, `AzureStorageBlob__Endpoint`, and `AzureStorageBlob__ContainerName` directly as Container App environment variables — no Key Vault integration.
4. The Container App uses a **system-assigned managed identity** with the `Storage Blob Data Contributor` role on the storage account, and Azure AD authentication is used for SQL Server (Bicep configures an Active Directory admin).

**Risk note:** The SQL admin password is computed deterministically from predictable inputs (`resourceGroup().id` + `environmentName`), which reduces its effective entropy. The production SQL connection string uses `Authentication=Active Directory Default` (managed identity), which mitigates direct password exposure. However, the admin password remains in Bicep state and is not rotated. No Key Vault is used.

## Feature Flags

No feature flag framework (LaunchDarkly, .NET `Microsoft.FeatureManagement`, Unleash, custom `@ConditionalOnProperty`) is configured. The single conditional behavior is `IsTestEnvironment` (bool config key) which gates EF Core migration execution — this is a test-isolation mechanism, not a runtime feature flag.

| Flag Name | Default | Controlled By |
|-----------|---------|--------------|
| `IsTestEnvironment` | `false` | App configuration / test setup code |

## Framework & Runtime Versions

| Component | Version | Source |
|-----------|---------|--------|
| .NET Runtime | 9.0 | `<TargetFramework>net9.0</TargetFramework>` in PhotoAlbum.csproj |
| ASP.NET Core (Razor Pages) | 9.0 | Implicit via `Microsoft.NET.Sdk.Web` SDK |
| Entity Framework Core | 9.0.9 | `Microsoft.EntityFrameworkCore.SqlServer` PackageReference |
| SixLabors.ImageSharp | 3.1.11 | PackageReference |
| EF Core Design (build-time) | 9.0.9 | PackageReference (PrivateAssets=all) |
| Docker base image (runtime) | `mcr.microsoft.com/dotnet/aspnet:9.0` | Dockerfile `FROM` |
| Docker base image (build) | `mcr.microsoft.com/dotnet/sdk:9.0` | Dockerfile multi-stage build |
| Azure Container Apps | N/A (PaaS) | `azure.yaml` host: containerapp |
| Azure SQL Database | Basic tier | `infra/main.bicep` |
| Azure Blob Storage | StorageV2 | `infra/main.bicep` |
| xUnit | 2.9.2 | Test project PackageReference |
| Microsoft.AspNetCore.Mvc.Testing | 9.0.9 | Test project PackageReference |
| coverlet.collector | 6.0.2 | Test project PackageReference |
