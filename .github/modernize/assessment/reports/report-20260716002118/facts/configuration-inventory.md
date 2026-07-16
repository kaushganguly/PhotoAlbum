# Configuration & Externalized Settings Inventory

PhotoAlbum uses a small set of local configuration files and environment-dependent ASP.NET conventions, with key settings centered on DB connection and file upload policies.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| appsettings.json | JSON config | PhotoAlbum/appsettings.json | Base runtime settings |
| appsettings.Development.json | JSON config | PhotoAlbum/appsettings.Development.json | Development overrides |
| launchSettings.json | ASP.NET launch profile | PhotoAlbum/Properties/launchSettings.json | Local run profiles and URLs |
| User Secrets | Secret store reference | `UserSecretsId` in PhotoAlbum.csproj | Local developer secret injection |
| Environment variables | Runtime source | Process environment | Standard ASP.NET config override source |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build` default local | Development build | .NET SDK default build pipeline |
| Release | `dotnet build -c Release` | Optimized deployment build | .NET SDK default build pipeline |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | `ASPNETCORE_ENVIRONMENT=Development` or launch profile | appsettings.json + appsettings.Development.json | Local diagnostics and dev settings |
| Production | `ASPNETCORE_ENVIRONMENT=Production` | appsettings.json (+ external env) | Production defaults with env overrides |
| Test flag mode | `IsTestEnvironment=true` setting | Effective runtime config composition | Skips startup DB migration |

## Properties Inventory

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| ConnectionStrings:DefaultConnection | LocalDB connection string | Base (override-capable) | appsettings.json |
| FileUpload:MaxFileSizeBytes | 10485760 | Base | appsettings.json |
| FileUpload:AllowedMimeTypes | jpeg/png/gif/webp | Base | appsettings.json |
| FileUpload:MaxFilesPerUpload | 10 | Base | appsettings.json |
| FileUpload:UploadPath | wwwroot/uploads | Base | appsettings.json |
| Logging:LogLevel:Default | Information | Base | appsettings.json |
| Logging:LogLevel:Microsoft.AspNetCore | Warning | Base | appsettings.json |
| AllowedHosts | * | Base | appsettings.json |
| IsTestEnvironment | false when absent | Test runs set true | test/runtime config injection |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | .NET runtime defaults, multipart body limit configured in Program.cs | Not explicitly set in repo config | 1 (single web process assumption) |

## Startup Dependency Chain

1. Web host starts and loads configuration.
2. Upload directory is ensured on disk (`wwwroot/uploads`).
3. Database migration runs at startup unless test flag is set.
4. Middleware pipeline starts serving requests.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| ConnectionStrings:DefaultConnection | Database connection string | appsettings.json (no password in sample) |
| UserSecretsId | Developer secret reference | `.csproj` metadata reference |

### Secrets Provisioning Workflow

Primary configuration is file-based with optional environment override and local user-secrets support. At runtime, ASP.NET configuration providers resolve values from JSON, environment variables, and user secrets, then bind them to service configuration usage.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| IsTestEnvironment | false | Configuration value used in Program.cs |

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| .NET Target Framework | net9.0 | PhotoAlbum.csproj |
| ASP.NET Core | 9.0 (via SDK) | Microsoft.NET.Sdk.Web |
| EF Core SQL Server | 9.0.9 | PackageReference |
| EF Core Design | 9.0.9 | PackageReference |
| SixLabors.ImageSharp | 3.1.11 | PackageReference |
| xUnit | 2.9.2 | PhotoAlbum.Tests.csproj |
