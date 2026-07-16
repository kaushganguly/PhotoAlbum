# .NET Upgrade Plan: net9.0 → net10.0

## Overview

The PhotoAlbum solution currently targets **.NET 9.0**, which reached end of mainstream support in May 2026 and is now EOL. This plan upgrades all projects to **.NET 10.0 LTS**, the current long-term support release, to restore supported status, gain access to modern runtime improvements, and ensure continued compatibility with the latest Azure SDK and NuGet ecosystem.

## Source Version

- **Current .NET version**: `net9.0` (.NET 9 — EOL)

## Target Version

- **Target .NET version**: `net10.0` (.NET 10 LTS)

## Projects in Solution

| Project | Path | Current TFM |
|---------|------|-------------|
| PhotoAlbum | `PhotoAlbum/PhotoAlbum.csproj` | `net9.0` |
| PhotoAlbum.Tests | `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj` | `net9.0` |

## Upgrade Scope

1. **Target Framework Moniker (TFM)** — Update `<TargetFramework>net9.0</TargetFramework>` to `net10.0` in both `.csproj` files.
2. **NuGet packages** — Upgrade all `9.x` packages to their `10.x` equivalents:
   - `Microsoft.EntityFrameworkCore.Design`
   - `Microsoft.EntityFrameworkCore.SqlServer`
   - `Microsoft.AspNetCore.Mvc.Testing`
   - `Microsoft.EntityFrameworkCore.InMemory`
   - Other test SDK packages as needed.
3. **API compatibility** — Review and resolve any breaking changes or deprecated APIs introduced between .NET 9 and .NET 10.
4. **Build validation** — Ensure the solution compiles and all existing xUnit tests pass after the upgrade.

## No SDK-Style Conversion Required

Both projects already use SDK-style project files (`<Project Sdk="Microsoft.NET.Sdk.*">`), so no project file restructuring is needed.
