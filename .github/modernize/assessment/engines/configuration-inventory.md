# Configuration & Externalized Settings Inventory

This inventory captures the application's primary configuration files, environment-driven behavior, and runtime settings across local development and test execution.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| appsettings.json | Runtime app config | `PhotoAlbum/appsettings.json` | Connection strings, upload options, logging |
| appsettings.Development.json | Runtime override | `PhotoAlbum/appsettings.Development.json` | Development log-level overrides |
| launchSettings.json | Local profile config | `PhotoAlbum/Properties/launchSettings.json` | Local URLs and `ASPNETCORE_ENVIRONMENT` |
| User Secrets | Secret store reference | `UserSecretsId` in `PhotoAlbum.csproj` | Externalized local secrets support |
| Azure Developer CLI config | Deployment config | `azure.yaml` | Deployment metadata |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build` default local | Development builds and symbols | SDK default |
| Release | `dotnet build -c Release` | Optimized production build artifacts | SDK default |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` via launch profile | `appsettings.json` + `appsettings.Development.json` | More verbose logging |
| Non-Development | Default when env variable differs | `appsettings.json` | Enables exception handler + HSTS branch |
| Test execution | `IsTestEnvironment=true` flag | test configuration injection | Skips startup DB migration block |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB connection string | Base | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Base; test override | `appsettings.json`, tests in-memory config |
| `FileUpload:AllowedMimeTypes` | jpeg/png/gif/webp | Base; test override | `appsettings.json`, tests in-memory config |
| `FileUpload:MaxFilesPerUpload` | `10` | Base | `appsettings.json` |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Base; test override | `appsettings.json`, tests in-memory config |
| `Logging:LogLevel:Default` | Information | Base + Development override | `appsettings*.json` |
| `AllowedHosts` | `*` | Base | `appsettings.json` |
| `IsTestEnvironment` | false when absent | Test only | configuration provider at runtime |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | ASP.NET Core defaults; form multipart limits set in Program | Not explicitly constrained in repo | Single instance by default |

## Startup Dependency Chain

1. Application starts and builds DI container.
2. Upload directory existence is validated/created.
3. Database migration executes on startup unless `IsTestEnvironment=true`.
4. HTTP pipeline starts accepting requests.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection string | Config file value (can be overridden by secrets/env) |
| `UserSecretsId` reference | Local secret store key | `[MASKED]` |

### Secrets Provisioning Workflow

Local development uses appsettings defaults, with optional secret overrides through the ASP.NET Core User Secrets mechanism and standard environment-variable configuration providers. No Key Vault or external secret manager wiring is explicitly configured in the current codebase.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | false | Runtime configuration value |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET target framework | net9.0 | `PhotoAlbum.csproj`, `PhotoAlbum.Tests.csproj` |
| ASP.NET Core Web SDK | net9.0 SDK stack | `PhotoAlbum.csproj` |
| EF Core SqlServer | 9.0.9 | `PhotoAlbum.csproj` |
| EF Core Design | 9.0.9 | `PhotoAlbum.csproj` |
| ImageSharp | 3.1.11 | `PhotoAlbum.csproj` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests.csproj` |
