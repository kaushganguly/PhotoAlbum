# Configuration & Externalized Settings Inventory

The application configuration is defined in JSON settings, launch profiles, project/build files, and container infrastructure. Development and production environments are represented, with no external secret-store integration identified in application configuration.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | Runtime JSON | `PhotoAlbum/appsettings.json` | Default connection, upload, logging, and host settings |
| Development settings | Runtime JSON | `PhotoAlbum/appsettings.Development.json` | Development error detail and logging overrides |
| Launch profiles | Local launch configuration | `PhotoAlbum/Properties/launchSettings.json` | HTTP and HTTPS profiles set Development environment and local URLs |
| Project files | Build and dependency configuration | `PhotoAlbum/PhotoAlbum.csproj`, `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | Target framework and NuGet dependencies |
| Solution | Build configuration | `PhotoAlbum.sln` | Debug and Release configurations |
| Web.config | IIS configuration | `PhotoAlbum/Web.config` | Contains a LocalDB connection string for IIS-hosted configuration |
| Container build | Container configuration | `Dockerfile` | .NET 9.0 runtime and SDK images; exposes port 8080 |
| Azure deployment | Deployment configuration | `azure.yaml`, `infra/main.bicep` | Defines Container Apps service and deployment-time environment settings |
| Environment template | Local deployment input | `.env.example` | Documents a resource group variable; no value is included |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build` default or `-c Debug` | Development and debugging build | None conditionally added |
| Release | `-c Release` in container build | Optimized container build and publish | None conditionally added |

The solution also lists Any CPU, x64, and x86 solution platforms; project mappings use Any CPU configurations.

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` in launch profiles | `appsettings.json`, `appsettings.Development.json` | Detailed errors and more verbose logging |
| Production | `ASPNETCORE_ENVIRONMENT=Production` in Azure Container App configuration | `appsettings.json` | No `appsettings.Production.json` identified; deployment injects connection and storage settings |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB connection string (no credential) | Default; overridden in Azure deployment using a managed-identity connection string | `appsettings.json`, `Web.config`, deployment configuration |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Default | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | `image/jpeg`, `image/png`, `image/gif`, `image/webp` | Default | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | Default | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Default | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | Default | `appsettings.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Default; `Information` in Development | `appsettings.json`, `appsettings.Development.json` |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | Not set | `Information` in Development | `appsettings.Development.json` |
| `DetailedErrors` | Not set | `true` in Development | `appsettings.Development.json` |
| `AllowedHosts` | `*` | Default | `appsettings.json` |
| `IsTestEnvironment` | `false` when absent | Can be set by test configuration | Read by `Program.cs` to skip startup migrations |
| `ConnectionStrings:DefaultConnection` | Deployment-generated Azure SQL connection string with Microsoft Entra default authentication | Production | `infra/main.bicep`, injected into Container App |
| `AzureStorageBlob:Endpoint` | Storage endpoint from deployed storage account | Production | `infra/main.bicep` |
| `AzureStorageBlob:ContainerName` | `photos` | Production | `infra/main.bicep` |
| `ASPNETCORE_ENVIRONMENT` | Framework default when not set | Development in launch profiles; Production in Azure | Launch settings, deployment configuration |
| `FormOptions:MultipartBodyLengthLimit` | `10485760` | All environments | Configured in `Program.cs` |
| `FormOptions:ValueLengthLimit` | `10485760` | All environments | Configured in `Program.cs` |
| `FormOptions:MultipartBoundaryLengthLimit` | `128` | All environments | Configured in `Program.cs` |

Azure Storage environment settings are defined in the deployment template; matching application consumption was not identified in the inspected source.

## Startup Parameters & Resource Requirements

| Service | Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum container | ASP.NET Core listens on the Container Apps ingress target port 8080; no JVM or explicit .NET runtime flags identified | 0.5 CPU and 1 GiB in `infra/main.bicep` | 1 minimum, 3 maximum replicas |

## Startup Dependency Chain

1. The container starts the ASP.NET Core application.
2. Before request handling, application startup applies EF Core migrations unless `IsTestEnvironment` is enabled; this requires the configured database to be available.
3. Container App deployment provisions the SQL database, storage account, and managed identity and injects connection/settings values.

No explicit startup wait script, health probe, or dependency retry configuration was identified.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| SQL administrator password | Database credential | Generated in `infra/main.bicep`; not stored in an external secret store in the inspected configuration |
| `ConnectionStrings:DefaultConnection` | Database connection string | LocalDB by default; Azure production template uses managed identity authentication rather than an embedded password |
| `RESOURCE_GROUP` | Deployment input | Blank placeholder in `.env.example`; no secret value |
| `UserSecretsId` | Development secrets identifier | Present in project metadata; no secret values or user-secret mappings identified |

### Secrets Provisioning Workflow

The Azure deployment template creates a system-assigned managed identity for the Container App and configures SQL authentication using Microsoft Entra default authentication. The template also grants the identity access to the container registry and storage account. No Key Vault, Vault, or other external secret-provider integration was identified; the SQL administrator password is generated by the infrastructure template and should be handled as sensitive deployment data.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Application configuration; when true, skips database migration during startup |

No feature-flag framework or rollout toggles were identified.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET target framework | `net9.0` | Project files |
| ASP.NET Core runtime container | `9.0` | `Dockerfile` |
| .NET SDK build container | `9.0` | `Dockerfile` |
| Entity Framework Core | `9.0.9` | Application and test project files |
| SixLabors.ImageSharp | `3.1.11` | Application project file |
| Visual Studio solution format | Version 12.00; Visual Studio 17 metadata | `PhotoAlbum.sln` |
| Azure Container Apps deployment | Bicep template | `infra/main.bicep` |
