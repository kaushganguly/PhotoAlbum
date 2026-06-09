# .NET Upgrade Plan: net9.0 → net10.0

## Overview

Upgrade the PhotoAlbum solution from **.NET 9.0** to **.NET 10.0 LTS**.

.NET 9 is a Standard-Term Support (STS) release with a shorter support window. .NET 10 is the current Long-Term Support (LTS) release, providing 3 years of mainstream support and improved stability, performance, and security.

## Source Version

- **Current .NET version**: `net9.0`

## Target Version

- **Target .NET version**: `net10.0` (Latest LTS)

## Projects in Solution

| Project | Path | Current TFM |
|---------|------|-------------|
| PhotoAlbum | `PhotoAlbum/PhotoAlbum.csproj` | `net9.0` |
| PhotoAlbum.Tests | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | `net9.0` |

## Upgrade Scope

1. **Target Framework Moniker (TFM)**: Update `<TargetFramework>` from `net9.0` to `net10.0` in both project files.
2. **NuGet Package Updates**: Update all `9.x` packages (Entity Framework Core, ASP.NET Core Mvc Testing, ImageSharp, test SDKs) to their `net10.0`-compatible versions.
3. **API Compatibility**: Address any breaking changes or deprecated APIs introduced between .NET 9 and .NET 10.
4. **Build Verification**: Ensure the solution compiles cleanly and all existing unit tests pass after the upgrade.

## Tasks

- `001-upgrade-dotnet-to-net10`: Upgrade both projects from net9.0 to net10.0
