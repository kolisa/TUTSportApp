# Improvement 6 — Developer Experience & Project Setup

**Commits:** `5eaf740`, `e2f0fa5`, `3ffffeb`  
**Scope:** Root-level tooling, scripts, docs, CI

---

## Problem

A developer who cloned the repo faced the following blockers immediately:

- README described a `/src /tests /docs /scripts` folder structure that didn't exist
- No connection string in `appsettings.json` — the app would crash on startup
- No JWT key — the app would crash before handling any request
- No EF Core migrations — the database schema couldn't be created
- No way to run the app locally without manually figuring out SQL Server
- Swagger had no JWT auth — protected endpoints couldn't be tested from the browser
- No CI — PRs had no automated quality gate
- No guide on how to add a new feature

---

## What Was Done

### 1. One-Command Setup Scripts

**`scripts/setup-dev.ps1`** (Windows) and **`scripts/setup-dev.sh`** (Linux/macOS)

Run once after cloning. The script handles everything automatically:

```
Step 1: Check .NET 9 SDK, Docker Desktop, dotnet-ef tool
Step 2: Generate a random 48-char JWT key → store in .NET User Secrets
Step 3: docker compose up -d → wait for SQL Server healthy
Step 4: dotnet ef database update
Step 5: dotnet build → verify zero warnings
```

The JWT key is **never stored in source control**. User Secrets are stored in the OS user profile (`%APPDATA%\Microsoft\UserSecrets\` on Windows).

**`scripts/add-migration.ps1`** / **`scripts/add-migration.sh`**

Thin wrappers around `dotnet ef migrations add` with the correct `--project` and `--startup-project` arguments pre-filled, so developers don't need to memorise the paths:

```bash
bash scripts/add-migration.sh AddSportTable
```

### 2. Docker Compose — Local SQL Server

`docker-compose.yml` provides a SQL Server 2022 Developer Edition container with a health check:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "TUTSportApp@Dev123!"
    ports:
      - "1433:1433"
    healthcheck:
      test: ["/opt/mssql-tools18/bin/sqlcmd", "-Q", "SELECT 1"]
      interval: 10s
      retries: 10
```

Developers without Visual Studio (LocalDB) can use this instead.

### 3. EF Core Initial Migration

`TUTSportApp.Infrastructure/Data/Migrations/20250101000000_InitialCreate.cs`

Creates all three tables with production-quality SQL:

| Table | Key Design Decisions |
|-------|---------------------|
| `Companies` | `NEWSEQUENTIALID()` default — sequential GUIDs avoid index fragmentation |
| `Users` | Filtered unique index on `Email` (`WHERE IsDeleted = 0`) — allows soft-delete |
| `Logins` | Filtered unique index on `Username`, cascade delete from Users |

All `AuditableEntity` columns (`CreatedAt`, `CreatedBy`, `LastModifiedAt`, `LastModifiedBy`, `IsDeleted`) are present with correct defaults.

### 4. Swagger JWT Auth

Before: Swagger had no authentication. Developers had to use Postman or curl to get a JWT, then manually paste it into every request.

After: A JWT bearer `SecurityDefinition` is registered in Swagger. Developers click **Authorize**, paste their token once, and all subsequent requests in the Swagger UI include the `Authorization: Bearer ...` header automatically.

```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT"
});
c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, [] } });
```

### 5. appsettings Restructured

| File | Purpose |
|------|---------|
| `appsettings.json` | Base config — Serilog sinks, JWT placeholder, LocalDB connection |
| `appsettings.Development.json` | Debug logging, EF query logging on |
| `appsettings.Testing.json` | Warning-only logging, InMemory DB connection string |

The `appsettings.Testing.json` is marked `CopyToOutputDirectory: PreserveNewest` in the API csproj so `WebApplicationFactory` picks it up during integration test runs.

### 6. GitHub Actions CI

`.github/workflows/ci.yml` runs on every push and PR to `master`, `main`, and `develop`:

```
Job: build-and-test
  ├── Checkout
  ├── Setup .NET 9
  ├── dotnet restore
  ├── dotnet build --configuration Release       (zero warnings gate)
  ├── dotnet test TUTSportApp.UnitTest           (fast, no DB)
  ├── dotnet test TUTSportApp.IntegrationTest    (InMemory DB, no SQL Server needed)
  └── Upload test results + coverage artifacts
```

A SQL Server service container is defined in the workflow YAML for future smoke tests that require a real database. Integration tests currently use InMemory.

### 7. README Rewritten

The original README described directories (`/src`, `/tests`, `/docs`) that didn't exist in the repository. The new README:

- Has an accurate folder tree with a description for every file
- Includes a Quick Start (one command)
- Documents both LocalDB and Docker connection string variants
- Explains secrets management (User Secrets for dev, env vars for CI/prod)
- Lists all migration commands
- Documents the MediatR pipeline with a table
- Lists the full tech stack

### 8. CONTRIBUTING.md

A complete feature development guide using **Register User** as a worked example, covering all seven steps:

1. Domain — entity or interface changes
2. Application — command, handler, validator
3. Infrastructure — repository implementation
4. Api — controller action with XML docs and `ProducesResponseType`
5. Unit tests — Moq-based, no DB
6. Integration tests — WebApplicationFactory pattern
7. Migration — `add-migration` script

Also covers branch naming, commit message format, code standards checklist, and NuGet package addition instructions.

### 9. `LoginController` Fixed

| Before | After |
|--------|-------|
| `public class` | `public sealed class` |
| No XML doc comments | Full `///` docs on class and action |
| No `ProducesResponseType` | `[ProducesResponseType(200)]`, `[ProducesResponseType(400)]`, `[ProducesResponseType(401)]` |
| `return Unauthorized(result.Error)` — raw string | `return Unauthorized(new ProblemDetails { ... })` — RFC 7807 |

### 10. `.gitignore` Additions

```
# Serilog runtime output
logs/

# User secrets
appsettings.*.local.json

# Environment files
.env
.env.*

# Docker local overrides
docker-compose.override.yml

# Test results and coverage
TestResults/
coverage/
*.opencover.xml
*.cobertura.xml

# JetBrains Rider
.idea/

# OS artefacts
.DS_Store
Thumbs.db
```
