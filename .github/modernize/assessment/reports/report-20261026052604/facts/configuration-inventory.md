# Configuration & Externalized Settings Inventory

The application uses JSON settings, ASP.NET Core environment variables, launch profiles, and project-file settings. No external configuration service or secret-store integration was identified.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Default connection string, upload settings, logging, allowed hosts |
| Development settings | JSON | `PhotoAlbum/appsettings.Development.json` | Detailed errors and development log levels |
| Launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | Local HTTP/HTTPS URLs and Development environment |
| Web configuration | XML | `PhotoAlbum/Web.config` | IIS hosting configuration |
| Project settings | MSBuild | `PhotoAlbum/PhotoAlbum.csproj`, `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | Target frameworks and package references |
| Runtime environment | Environment variables | ASP.NET Core configuration providers | Standard environment and command-line overrides |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | MSBuild configuration | Development build | No conditional package differences found |
| Release | MSBuild configuration | Production build | No conditional package differences found |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` | `appsettings.json`, `appsettings.Development.json` | Detailed errors; more verbose ASP.NET Core and EF Core logging |
| Other environments | ASP.NET Core environment setting | `appsettings.json` plus matching environment file if supplied | Production exception handler and HSTS are enabled outside Development |
| Tests | `IsTestEnvironment` configuration flag | Test host configuration | Skips startup migrations when enabled |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server LocalDB `PhotoAlbumDb` | Default | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Default | `appsettings.json` |
| `FileUpload:AllowedMimeTypes` | JPEG, PNG, GIF, WebP | Default | `appsettings.json` |
| `FileUpload:MaxFilesPerUpload` | `10` | Default | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Default | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | Default | `appsettings.json` |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | Default | `appsettings.json` |
| `DetailedErrors` | `true` | Development | `appsettings.Development.json` |
| `IsTestEnvironment` | `false` | Tests | Runtime configuration; controls startup migrations |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | .NET runtime; no custom startup options identified | Not specified | Not specified |

## Startup Dependency Chain

1. Create the upload directory if absent.
2. Unless `IsTestEnvironment` is true, apply pending EF Core migrations.
3. Configure and start the HTTP request pipeline.

No explicit readiness probe, database wait/retry policy, or startup timeout was identified. Migration failure is logged and rethrown.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection configuration | LocalDB integrated authentication; no password value present |
| `UserSecretsId` | ASP.NET Core user secrets identifier | Identifier is stored in the project file; no secret value is committed here |

### Secrets Provisioning Workflow

No Key Vault, Vault, AWS Secrets Manager, or deployment secrets workflow was found. ASP.NET Core user secrets and environment-variable configuration are available mechanisms, but no production secret binding or identity/access model is configured in the repository.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Runtime configuration; skips database migration in test hosts |

No feature-flag framework was identified.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET target framework | `net9.0` | Project files |
| ASP.NET Core | 9.0 | Web SDK and shared framework |
| Entity Framework Core | 9.0.9 | Project package references |
| SixLabors.ImageSharp | 3.1.11 | Application project |
| xUnit | 2.9.2 | Test project |
| Microsoft.NET.Test.Sdk | 17.12.0 | Test project |
