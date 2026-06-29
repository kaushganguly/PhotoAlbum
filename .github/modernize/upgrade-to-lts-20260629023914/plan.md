# .NET Upgrade Plan: net9.0 → net10.0

## Overview

This plan upgrades the PhotoAlbum solution from **.NET 9.0** to **.NET 10.0 LTS**.

.NET 9 reached end of support on **May 12, 2026** and is no longer receiving security or quality updates. Upgrading to .NET 10.0 LTS ensures long-term support (until May 2030), access to the latest runtime improvements, and continued security patch coverage.

## Source Version

- **Current .NET version**: `net9.0`

## Target Version

- **Target .NET version**: `net10.0` (Latest LTS)

## Projects in Solution

| Project | File | Current TFM |
|---------|------|-------------|
| PhotoAlbum (web app) | `PhotoAlbum/PhotoAlbum.csproj` | `net9.0` |
| PhotoAlbum.Tests (test project) | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | `net9.0` |

## Upgrade Scope

1. **Target Framework Moniker (TFM)**: Update `<TargetFramework>` from `net9.0` to `net10.0` in both project files.
2. **NuGet Package Updates**: Upgrade all `9.x` package versions to their compatible `10.x` equivalents:
   - `Microsoft.EntityFrameworkCore.Design`
   - `Microsoft.EntityFrameworkCore.SqlServer`
   - `Microsoft.AspNetCore.Mvc.Testing`
   - `Microsoft.EntityFrameworkCore.InMemory`
   - `Microsoft.NET.Test.Sdk`
3. **API Compatibility**: Review and fix any breaking API changes introduced between .NET 9 and .NET 10.
4. **Build Verification**: Ensure the solution compiles successfully after upgrade.
5. **Test Verification**: Ensure all existing unit/integration tests pass after upgrade.

## Tasks

- `001-upgrade-dotnet-to-net10`: Upgrade PhotoAlbum solution from .NET 9.0 to .NET 10.0 LTS
