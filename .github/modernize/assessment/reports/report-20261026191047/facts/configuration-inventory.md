# Configuration & Externalized Settings Inventory

Configuration is held in the standard ASP.NET Core JSON settings, development launch profiles, and project files. The checked-in connection string is for trusted LocalDB authentication and contains no password; no external secret store or feature-flag provider was identified.

## Configuration Sources

| Source | Type | Path/Location | Notes |
|---|---|---|---|
| Application settings | JSON | `PhotoAlbum/appsettings.json` | Base connection string, upload controls, logging, and allowed hosts |
| Development settings | JSON | `PhotoAlbum/appsettings.Development.json` | Development logging levels and detailed errors |
| Launch profiles | JSON | `PhotoAlbum/Properties/launchSettings.json` | Local HTTP/HTTPS URLs and `ASPNETCORE_ENVIRONMENT=Development` |
| Application project | MSBuild XML | `PhotoAlbum/PhotoAlbum.csproj` | Target framework, nullable context, implicit usings, package references, and user-secrets identifier |
| Test project | MSBuild XML | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | Test framework and test dependencies |
| Runtime code defaults | C# | `PhotoAlbum/Program.cs`, `PhotoAlbum/Services/PhotoService.cs` | Upload form limits, default upload path, MIME fallback, and migration behavior |

## Build Profiles

| Profile | Activation | Purpose | Key Dependencies/Plugins |
|---|---|---|---|
| Debug | `dotnet build` default or `-c Debug` | Development build | No conditional package references identified |
| Release | `dotnet build -c Release` | Optimized build | No conditional package references identified |

## Runtime Profiles

| Profile | Activation Method | Config Files | Key Overrides |
|---|---|---|---|
| Development | Set by local launch profiles through `ASPNETCORE_ENVIRONMENT` | `appsettings.json`, `appsettings.Development.json`, `launchSettings.json` | Detailed errors and more verbose logging |
| Non-development | ASP.NET Core environment value at runtime | `appsettings.json` plus any externally supplied environment-specific settings | Uses exception handler and HSTS; no checked-in production settings file |
| Test | Test host configuration | Test code and base settings | `IsTestEnvironment` controls startup migration behavior; tests use EF Core InMemory |

## Properties Inventory

### PhotoAlbum

| Property Key | Default | Profiles | Source |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | LocalDB with trusted connection | Base; externally overrideable | `appsettings.json` |
| `FileUpload:MaxFileSizeBytes` | `10485760` | Base | `appsettings.json`; service fallback is 10 MiB |
| `FileUpload:AllowedMimeTypes` | JPEG, PNG, GIF, WebP | Base | `appsettings.json`; service has the same fallback list |
| `FileUpload:MaxFilesPerUpload` | `10` | Base | `appsettings.json`; no corresponding enforcement was identified in the page handler |
| `FileUpload:UploadPath` | `wwwroot/uploads` | Base | `appsettings.json`; service fallback is the same relative path |
| `AllowedHosts` | `*` | Base | `appsettings.json` |
| `Logging:LogLevel:Default` | `Information` | `Debug` in Development | App settings |
| `Logging:LogLevel:Microsoft.AspNetCore` | `Warning` | `Information` in Development | App settings |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | Not set in base settings | `Information` in Development | `appsettings.Development.json` |
| `DetailedErrors` | Not set in base settings | `true` in Development | `appsettings.Development.json` |
| `IsTestEnvironment` | `false` when absent | Test configuration | Read in `Program.cs` to skip automatic migrations |
| `FormOptions:MultipartBodyLengthLimit` | `10485760` | All runtime environments | `Program.cs` |
| `FormOptions:ValueLengthLimit` | `10485760` | All runtime environments | `Program.cs` |
| `FormOptions:MultipartBoundaryLengthLimit` | `128` | All runtime environments | `Program.cs` |

## Startup Parameters & Resource Requirements

| Service | JVM/Runtime Options | Memory | Instance Count |
|---|---|---|---|
| PhotoAlbum | No custom runtime options identified; .NET 9 application | Not specified | Not specified |

## Startup Dependency Chain

1. ASP.NET Core host builds and registers Razor Pages, EF Core, and `PhotoService`.
2. The application ensures `wwwroot/uploads` exists.
3. Unless `IsTestEnvironment` is true, the application creates a service scope and applies EF Core migrations before handling requests. Migration failure is logged and rethrown, preventing startup from continuing.
4. HTTPS redirection, static files, routing, authorization middleware, and Razor Pages are configured.

No external service readiness wait or health-check dependency was identified.

## Secrets & Sensitive Configuration

| Secret Reference | Type | Storage (masked) |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Database connection | Checked-in LocalDB trusted connection; no password present |
| `UserSecretsId` | Local development secret-store identifier | Identifier in the application project; no credential value |

### Secrets Provisioning Workflow

No production secret provisioning, Key Vault, managed identity, Vault, or external secret-store binding is configured in the checked-in files. ASP.NET Core user secrets are enabled for local development through a project user-secrets identifier; the SQL Server setting uses trusted local authentication.

## Feature Flags

| Flag Name | Default | Controlled By |
|---|---|---|
| `IsTestEnvironment` | `false` | Application configuration; skips automatic migration during tests |

No feature-flag framework or remote toggle service was identified.

## Framework & Runtime Versions

| Component | Version | Source |
|---|---|---|
| Target framework | .NET 9 (`net9.0`) | Application and test project files |
| Entity Framework Core SQL Server | 9.0.9 | `PhotoAlbum/PhotoAlbum.csproj` |
| Entity Framework Core Design | 9.0.9 | `PhotoAlbum/PhotoAlbum.csproj` |
| Entity Framework Core InMemory | 9.0.9 | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
| ASP.NET Core MVC Testing | 9.0.9 | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
| SixLabors.ImageSharp | 3.1.11 | `PhotoAlbum/PhotoAlbum.csproj` |
| xUnit | 2.9.2 | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` |
| .NET SDK used for this assessment | 10.0.401 | Assessment environment |
