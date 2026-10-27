# Configuration & Externalized Settings Inventory

Application configuration is provided by JSON settings, ASP.NET Core environment variables, launch profiles, and deployment files. The repository has Development and Production runtime configurations but no external configuration server or secret-store integration was identified.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Default application and upload settings |
| Development settings | JSON | `PhotoAlbum/appsettings.Development.json` | Development logging and detailed errors |
| Launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | Local HTTP and HTTPS profiles; sets `ASPNETCORE_ENVIRONMENT=Development` |
| Project files | MSBuild | `PhotoAlbum/PhotoAlbum.csproj`, `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | Target frameworks, build options, package versions, and user-secrets identifier |
| Container build | Dockerfile | `Dockerfile` | .NET 9 runtime and SDK images; container listens on port 8080 |
| Azure deployment | Azure Developer CLI YAML | `azure.yaml` | Defines the web service, container app host, Bicep infrastructure, and lifecycle hooks |
| Azure infrastructure | Bicep and parameters | `infra/main.bicep`, `infra/main.parameters.json` | Provisions Container Apps, SQL, storage, and managed identity settings |
| Assessment settings | YAML | `.github/modernize/assessment/reports/assessment-config.yaml` | Configures assessment domains and the .NET target runtime |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | Default MSBuild configuration | Local builds and debugging | No conditional package groups identified |
| Release | `-c Release` | Optimized container build and publish | No conditional package groups identified |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` in local launch profiles | `appsettings.json`, `appsettings.Development.json` | Detailed errors and more verbose logging |
| Production | `ASPNETCORE_ENVIRONMENT=Production` in Azure Container App deployment | `appsettings.json` | No `appsettings.Production.json` found; deployment injects connection and storage-related environment values |
| Test | `IsTestEnvironment=true` configuration value in test hosting | `appsettings.json` plus test-host configuration | Skips startup database migrations; tests configure an in-memory database |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server LocalDB connection string (no password) | Default | `PhotoAlbum/appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Default | `PhotoAlbum/appsettings.json` |
| `FileUpload:AllowedMimeTypes` | JPEG, PNG, GIF, WebP | Default | `PhotoAlbum/appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | Default | `PhotoAlbum/appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Default | `PhotoAlbum/appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | Default | `PhotoAlbum/appsettings.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Default | `PhotoAlbum/appsettings.json` |
| `DetailedErrors` | Not set | Development: `true` | `PhotoAlbum/appsettings.Development.json` |
| `Logging:LogLevel:Default` | Development: `Debug` | Development | `PhotoAlbum/appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | Development: `Information` | Development | `PhotoAlbum/appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | Development: `Information` | Development | `PhotoAlbum/appsettings.Development.json` |
| `AllowedHosts` | `*` | Default | `PhotoAlbum/appsettings.json` |
| `IsTestEnvironment` | `false` when absent | Test host may set `true` | Read by `Program.cs`; no default-file entry |

### Azure Container App

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings__DefaultConnection` | Azure SQL connection string using Active Directory Default; value supplied by Bicep | Production | `infra/main.bicep` |
| `AzureStorageBlob__Endpoint` | Provisioned storage account endpoint | Production | `infra/main.bicep` |
| `AzureStorageBlob__ContainerName` | `photos` | Production | `infra/main.bicep` |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Production | `infra/main.bicep` |
| `AZURE_ENV_NAME` | Supplied by Azure Developer CLI environment | Deployment | `infra/main.parameters.json` |
| `AZURE_LOCATION` | Supplied by Azure Developer CLI environment | Deployment | `infra/main.parameters.json` |
| `SERVICE_WEB_IMAGE_NAME` | Hello-world container image fallback | Deployment | `infra/main.parameters.json` |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum container | `dotnet PhotoAlbum.dll`; ASP.NET Core listens on port 8080 in container | 1 GiB; 0.5 CPU | 1–3 replicas in Bicep |
| Local development | `dotnet run`; local launch profiles use ports 5134 and 7055 | Not specified | Not specified |

## Startup Dependency Chain

1. Azure resource provisioning creates the Container Apps environment, SQL database, storage account, and Container App with system-assigned identity.
2. Role assignments grant the identity access to the container registry and storage; the post-provision hook waits for role propagation.
3. Before deployment, the predeploy hook configures Container App registry authentication with managed identity.
4. The application starts and applies EF Core migrations unless test mode is enabled. No container health probe or application readiness mechanism was identified.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| SQL administrator password in Bicep | Deployment credential | Generated from deployment context; value intentionally omitted |
| Local SQL connection string | Database connection | Trusted LocalDB connection without password |
| Azure SQL connection string | Database connection | Active Directory Default authentication; no embedded password |
| `.NET UserSecretsId` | Local development secret-store identifier | Identifier only; no secret values checked into the project file |

### Secrets Provisioning Workflow

The Azure SQL administrator password is generated by the Bicep deployment and used to provision the SQL server. The Container App uses a system-assigned managed identity for Azure SQL authentication and receives no SQL password in its environment; role assignments also grant registry and blob-storage access. No Key Vault, external secret manager, or checked-in application secret values were identified. Local developer secrets can use the configured .NET user-secrets mechanism.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` when absent | Application configuration; used to skip startup migrations in tests |

No feature-flag framework or rollout configuration was identified.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| ASP.NET Core / application target | .NET 9.0 | `PhotoAlbum/PhotoAlbum.csproj` |
| Test target | .NET 9.0 | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
| Entity Framework Core | 9.0.9 | Application and test project files |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum/PhotoAlbum.csproj` |
| .NET container runtime image | 9.0 | `Dockerfile` |
| .NET SDK container image | 9.0 | `Dockerfile` |
| Azure Container Apps target | Container Apps | `azure.yaml`, `infra/main.bicep` |
