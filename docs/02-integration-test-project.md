# Improvement 2 — Integration & Functional Test Project

**Commit:** `4cc64f1`  
**Scope:** `TUTSportApp.IntegrationTest` (new project)

---

## Problem

The repository had only `TUTSportApp.UnitTest` with a single placeholder smoke test (`Assert.True(true)`). There was no way to verify that the HTTP pipeline, MediatR handlers, repositories, and EF Core all worked together correctly without spinning up the full application against a real SQL Server.

---

## What Was Done

### Project Structure

```
TUTSportApp.IntegrationTest/
├── TUTSportApp.IntegrationTest.csproj
├── Infrastructure/
│   ├── IntegrationTestWebApplicationFactory.cs
│   └── TestDataSeeder.cs
├── Common/
│   └── IntegrationTestBase.cs
├── Auth/
│   ├── LoginEndpointTests.cs          (10 tests)
│   ├── LoginCommandHandlerTests.cs    (5 tests)
│   └── AuthServiceTests.cs            (8 tests)
└── Repositories/
    └── LoginRepositoryTests.cs        (11 tests)
```

**Total: 34 tests**

### Key Design Decisions

#### `IntegrationTestWebApplicationFactory`
Inherits `WebApplicationFactory<Program>`. Replaces the SQL Server `DbContext` registration with an **EF Core InMemory database**, so integration tests run with zero external infrastructure — no Docker, no SQL Server.

```csharp
services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase(_databaseName));
```

Each factory instance gets a unique database name (`Guid.NewGuid()`) so parallel test classes are fully isolated.

#### `IntegrationTestBase`
Implements `IClassFixture<IntegrationTestWebApplicationFactory>` and `IDisposable`. Provides every test class with a scoped DI container, an `HttpClient`, `ApplicationDbContext`, and `IAuthService` without any boilerplate.

#### `TestDataSeeder`
Uses **Bogus** to generate realistic fake data. Centralises seeding so every test starts from a known, repeatable state.

---

## Test Coverage

### `LoginEndpointTests` — HTTP pipeline (10 tests)

| Test | Verifies |
|------|----------|
| Valid credentials → 200 + JWT | Happy path through full stack |
| Wrong password → 401 | Auth failure |
| Unknown username → 401 | Non-existent user |
| Locked account → 401 | Account lockout |
| Empty username → 400 | FluentValidation (ValidationBehavior) |
| Empty password → 400 | FluentValidation (ValidationBehavior) |
| Password too short → 400 | FluentValidation min length rule |
| Null body → 400 | Model binding guard |
| Wrong content-type → 415 | ASP.NET Core media type enforcement |
| Response shape validation | `{ isSuccess, data, error }` contract |
| Token is well-formed JWT | 3 dot-separated segments |

### `LoginCommandHandlerTests` — MediatR layer (5 tests)

Exercises Application + Infrastructure together **without** the HTTP pipeline. Uses `ISender` directly.

### `AuthServiceTests` — Service layer (8 tests)

| Test | Verifies |
|------|----------|
| Hash format | `iterations.saltBase64.hashBase64` |
| Two hashes differ | Random salt per call |
| Correct password verifies | True |
| Wrong password rejected | False |
| Empty password → false | Guard |
| Tampered hash → false | Integrity |
| Malformed hash → false | Format guard |
| `CreateTokenAsync` → valid JWT | 3 segments |

### `LoginRepositoryTests` — Data layer (11 tests)

Covers `GetByUsernameAsync`, `GetByUserIdAsync`, `UpdateFailedAttemptsAsync` (including the 5-attempt lockout threshold), `IsUsernameUniqueAsync`, `GetByIdAsync`, `ExistsAsync`.

---

## Packages Added

```xml
<PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
<PackageVersion Include="Moq" Version="4.20.72" />
<PackageVersion Include="FluentAssertions" Version="6.12.2" />
<PackageVersion Include="Bogus" Version="35.6.1" />
```
