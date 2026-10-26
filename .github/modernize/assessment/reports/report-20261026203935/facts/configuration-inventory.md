# Configuration & Externalized Settings Inventory

PhotoAlbum configuration is supplied by JSON settings, launch profiles, environment variables, and Azure deployment templates. No application-level secret-store integration or feature-flag framework is configured.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Default connection, upload behavior, logging, allowed hosts |
| Development settings | JSON | `PhotoAlbum/appsettings.Development.json` | Development logging levels and detailed errors |
| Local launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | Sets local HTTP/HTTPS URLs and Development environment |
| IIS settings | XML | `PhotoAlbum/Web.config` | Contains a LocalDB connection string |
| Deployment settings | Bicep | `infra/main.bicep`, `infra/main.parameters.json` | Provisions Azure resources and injects runtime settings |
| Azure Developer CLI | YAML | `azure.yaml` | Defines the web service, container, infrastructure, and deployment hooks |
| Container build | Dockerfile | `Dockerfile` | Defines .NET 9 build/runtime images, port, and startup command |
| Deployment environment example | Environment file template | `.env.example` | Documents the `RESOURCE_GROUP` deployment variable |
| User secrets metadata | Project property | `PhotoAlbum/PhotoAlbum.csproj` | A `UserSecretsId` is present; no user-secret values are committed |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default `dotnet build` configuration | Development build | No conditional package references identified |
| Release | `-c Release`; used by Docker build/publish | Optimized deployment build | No conditional package references identified |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Default | No environment-specific file | `appsettings.json` | SQL LocalDB; default upload limits and logging |
| Development | `ASPNETCORE_ENVIRONMENT=Development` in local launch profiles | `appsettings.json`, `appsettings.Development.json` | Detailed errors and more verbose logging |
| Production | Set by the Azure Container App deployment | `appsettings.json` plus environment variables | SQL connection uses Azure-provided setting; ASP.NET Core environment is Production |
| Test | `IsTestEnvironment=true` configuration when enabled; tests configure the InMemory provider | Application settings and test setup | Skips startup migrations in Program; service unit tests use isolated in-memory databases |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server LocalDB connection for `PhotoAlbumDb` | Default/development; Azure deployment injects a SQL connection string using Active Directory Default authentication | `appsettings.json`; Azure `ConnectionStrings__DefaultConnection`; also `Web.config` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Default | `appsettings.json`; service fallback is 10 MiB |
| `FileUpload:AllowedMimeTypes` | `image/jpeg`, `image/png`, `image/gif`, `image/webp` | Default | `appsettings.json`; matching fallback in `PhotoService` |
| `FileUpload:MaxFilesPerUpload` | `10` | Default | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Default | `appsettings.json`; service fallback uses the same path |
| `Logging:LogLevel:Default` | `Information` | Default | `appsettings.json`; Development overrides to `Debug` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Default | `appsettings.json`; Development overrides to `Information` |
| `DetailedErrors` | `false` (framework default) | Development sets `true` | `appsettings.Development.json` |
| `AllowedHosts` | `*` | Default | `appsettings.json` |
| `IsTestEnvironment` | `false` (configuration default) | Test when set to `true` | Application configuration |
| `AzureStorageBlob:Endpoint` | Not set in application settings | Azure deployment sets endpoint | Bicep environment variable `AzureStorageBlob__Endpoint` |
| `AzureStorageBlob:ContainerName` | Not set in application settings | Azure deployment sets `photos` | Bicep environment variable `AzureStorageBlob__ContainerName` |
| `ASPNETCORE_ENVIRONMENT` | Framework default | Development locally; Production in Azure | `launchSettings.json`; Bicep |

The Azure deployment also configures the Container App to use port 8080, 0.5 CPU, 1 GiB memory, and one to three replicas.

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum local | `dotnet PhotoAlbum.dll` (container entry point); no JVM settings | Not specified locally | Not specified locally |
| PhotoAlbum Azure Container App | ASP.NET Core Production environment; ingress target port 8080 | 0.5 CPU and 1 GiB | Minimum 1, maximum 3 |

## Startup Dependency Chain

1. Azure infrastructure provisions the Container Apps environment, SQL Server/database, storage account, and registry.
2. The Container App receives its settings and starts the application container.
3. On application startup, the app creates the local uploads directory and applies EF Core migrations unless the test-environment flag is set.
4. The app begins serving requests after startup initialization; no separate readiness probe or database wait/retry mechanism is declared in the application configuration.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection configuration | LocalDB trusted-connection string in configuration; Azure deployment supplies a SQL connection using managed identity authentication |
| SQL administrator password | Infrastructure provisioning input | Generated by the Bicep template; value intentionally omitted |
| `UserSecretsId` | .NET development secret-store identifier | Identifier only; no secret values included |
| `RESOURCE_GROUP` | Deployment environment variable | Supplied by the operator; `.env.example` contains no value |

### Secrets Provisioning Workflow

For local development, the configured database connection is in application settings and uses LocalDB trusted connection. The project has a .NET user-secrets identifier, but no secret provisioning or secret-store references are wired into application code. Azure infrastructure provisions a SQL administrator credential and configures the Container App with an Active Directory Default SQL connection; a system-assigned managed identity is configured as SQL administrator. The actual generated credential is not included in this report.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Application configuration; when true, startup database migrations are skipped |

No general feature flag framework or rollout toggles were identified.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| Target framework | .NET 9 (`net9.0`) | `PhotoAlbum.csproj`, `PhotoAlbum.Tests.csproj` |
| ASP.NET Core | 9.0 | Target framework and Docker runtime image |
| Entity Framework Core Design | 9.0.9 | `PhotoAlbum.csproj` |
| EF Core SQL Server | 9.0.9 | `PhotoAlbum.csproj` |
| EF Core InMemory | 9.0.9 | `PhotoAlbum.Tests.csproj` |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum.csproj` |
| .NET SDK/container build image | 9.0 | Dockerfile |
| .NET ASP.NET runtime/container image | 9.0 | Dockerfile |
