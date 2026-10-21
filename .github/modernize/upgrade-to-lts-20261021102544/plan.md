# .NET Upgrade Plan: PhotoAlbum

## Summary

| | |
|---|---|
| **Source .NET version** | .NET 9.0 (`net9.0`) |
| **Target .NET version** | .NET 10.0 (`net10.0`) — latest LTS |
| **Solution** | `PhotoAlbum.sln` |
| **Projects** | `PhotoAlbum\PhotoAlbum.csproj` (ASP.NET Core Razor Pages, `Microsoft.NET.Sdk.Web`), `PhotoAlbum.Tests\PhotoAlbum.Tests.csproj` (xUnit test project, `Microsoft.NET.Sdk`) |

## Why Upgrade

.NET 9.0 is a Standard Term Support (STS) release that reaches end of support in May 2026, while .NET 10.0 is the current Long Term Support (LTS) release. Standardizing on the latest LTS version ensures continued security patches, long-term support, and access to the newest runtime/framework improvements.

## Scope of Changes

1. **PhotoAlbum\PhotoAlbum.csproj**
   - Update `<TargetFramework>` from `net9.0` to `net10.0`.
   - Update NuGet package references to their .NET 10–compatible versions:
     - `Microsoft.EntityFrameworkCore.Design` (9.0.9 → 10.x)
     - `Microsoft.EntityFrameworkCore.SqlServer` (9.0.9 → 10.x)
     - `SixLabors.ImageSharp` (verify compatibility, update if needed)
   - Review ASP.NET Core / EF Core breaking changes between .NET 9 and .NET 10 and update code as needed.

2. **PhotoAlbum.Tests\PhotoAlbum.Tests.csproj**
   - Update `<TargetFramework>` from `net9.0` to `net10.0`.
   - Update test-related NuGet packages to .NET 10–compatible versions:
     - `Microsoft.AspNetCore.Mvc.Testing` (9.0.9 → 10.x)
     - `Microsoft.EntityFrameworkCore.InMemory` (9.0.9 → 10.x)
     - `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `coverlet.collector` (verify/update to latest compatible versions)

3. **General**
   - Rebuild solution and resolve any compiler warnings/errors introduced by the TFM bump.
   - Run the full test suite (`PhotoAlbum.Tests`) to confirm behavior parity after the upgrade.
   - Verify EF Core migrations still apply cleanly against SQL Server LocalDB.

## Out of Scope

- Feature changes, refactoring unrelated to the upgrade, containerization, or Azure deployment.

## Tasks

See `.metadata\tasks.json` for the structured task list executed by the upgrade agent.
