---
name: photoalbum-sql-managed-identity
description: Replace SQL Server LocalDB and Trusted_Connection authentication with Azure SQL Database and Microsoft Entra Managed Identity (Authentication=Active Directory Default). Remove trusted, integrated, and password credentials from configuration so the app connects passwordless.
---

## Overview

PhotoAlbum uses EF Core (`PhotoAlbumContext`) against SQL Server LocalDB. The `DefaultConnection` string uses
`Server=(localdb)\mssqllocaldb;...;Trusted_Connection=true`, which only works on a developer's Windows machine.

This skill migrates the database to **Azure SQL Database** with **passwordless** Microsoft Entra authentication. It
is an organization standard:

- **Never** use SQL usernames/passwords, `Trusted_Connection`, or integrated security in connection strings.
- Authenticate with **Managed Identity in Azure** and the **developer identity locally**, both via
  `Authentication=Active Directory Default`.
- **Keep** the EF Core `DbContext` registration, the `Photo` model, and the startup `MigrateAsync` behavior intact.

## Steps

1. Update `ConnectionStrings:DefaultConnection` in `PhotoAlbum/appsettings.json` to the Azure SQL + Entra form:
   `Server=tcp:<server>.database.windows.net,1433;Database=PhotoAlbumDb;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true`

2. Remove `(localdb)\mssqllocaldb`, `Trusted_Connection=true`, integrated security, and any `User Id` / `Password`
   from all configuration files (`appsettings.json`, `appsettings.Development.json`, user secrets).

3. Leave `options.UseSqlServer(...)` in `Program.cs` unchanged. EF Core's SQL Server provider uses
   `Microsoft.Data.SqlClient`, which supports `Authentication=Active Directory Default` — no provider swap needed.

4. Rely on `DefaultAzureCredential` semantics: local development authenticates with the signed-in developer identity
   (Azure CLI / Visual Studio), and Azure hosting authenticates with the app's system- or user-assigned Managed
   Identity.

5. Keep `await context.Database.MigrateAsync()` on startup. Ensure the Managed Identity (and the developer identity)
   are created as **contained database users** in Azure SQL with the required role (for example `db_owner` for
   migrations, or least-privilege read/write plus migration rights). The repo's `scripts/configure-sql-user.*`
   scripts handle this provisioning.

6. Move any residual secrets to Azure Key Vault. Do not store passwords in `appsettings`, deployment templates,
   scripts, or source control.

## Sample code

### Before (LocalDB, trusted connection)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PhotoAlbumDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### After (Azure SQL, Microsoft Entra Managed Identity)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:<server>.database.windows.net,1433;Database=PhotoAlbumDb;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true"
  }
}
```

`Program.cs` is unchanged — the provider picks up Entra auth from the connection string:

```csharp
builder.Services.AddDbContext<PhotoAlbumContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Optional — explicit token acquisition

Use this only if the application manages a raw `SqlConnection` directly; the connection-string approach above is
preferred for EF Core.

```csharp
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

var credential = new DefaultAzureCredential();
var token = await credential.GetTokenAsync(
    new TokenRequestContext(new[] { "https://database.windows.net/.default" }));

await using var connection = new SqlConnection(connectionString) { AccessToken = token.Token };
await connection.OpenAsync();
```

## Dependency changes

- Reference `Azure.Identity` (1.x) only if explicit token acquisition is used; `Authentication=Active Directory
  Default` via the EF Core SQL Server provider needs no additional package.
- No new secret-handling packages.

## Verification checks

- The app connects to Azure SQL Database using Microsoft Entra authentication.
- No `Trusted_Connection`, integrated security, usernames, or passwords remain in any configuration file or source.
- EF Core migrations still run on startup (`MigrateAsync`) against Azure SQL.
- Local development works with the developer's Entra identity; Azure hosting works with the Managed Identity.
- The `Photo` entity and `PhotoAlbumContext` (including the `IX_Photos_UploadedAt` index) are unchanged.
