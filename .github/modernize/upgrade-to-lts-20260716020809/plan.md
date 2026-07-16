# .NET Upgrade Plan: net9.0 → net10.0

## Summary

Upgrade the PhotoAlbum solution from **.NET 9** (`net9.0`) to **.NET 10 LTS** (`net10.0`).

.NET 9 is a Standard Term Support (STS) release that reached end of support on **May 12, 2026**. Upgrading to .NET 10 LTS ensures continued security patches, long-term support, and access to the latest platform improvements.

## Source Version

- **Current framework**: `net9.0` (.NET 9 — end of support May 12, 2026)

## Target Version

- **Target framework**: `net10.0` (.NET 10 LTS)

## Projects in Solution

| Project | Path | Type |
|---------|------|------|
| PhotoAlbum | `PhotoAlbum/PhotoAlbum.csproj` | ASP.NET Core Razor Pages Web App |
| PhotoAlbum.Tests | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | xUnit Test Project |

## Upgrade Scope

1. **Target Framework Moniker (TFM)**: Update `<TargetFramework>` from `net9.0` to `net10.0` in both `.csproj` files.
2. **NuGet Package Updates**: Bump all `9.x` versioned packages (Entity Framework Core, ASP.NET Core MVC Testing, etc.) to their `.NET 10`-compatible `10.x` releases.
3. **API Compatibility**: Review and resolve any breaking changes introduced between .NET 9 and .NET 10.
4. **Build Verification**: Ensure the solution compiles cleanly after the upgrade.
5. **Test Verification**: Confirm all existing xUnit tests pass on the new framework.

## Tasks

See `.metadata/tasks.json` for the structured task definition.
