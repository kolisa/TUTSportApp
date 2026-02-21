# Improvement 3 — Clean Architecture & SOLID Enforcement

**Commit:** `870ccef`  
**Scope:** All four production layers

---

## Overview

A thorough audit identified violations of Clean Architecture dependency rules and all five SOLID principles. Two of these were **critical runtime bugs** — the application silently failed in ways that would never have been caught without the right tests.

---

## Critical Bug Fixes

### Bug 1 — `UseAuthentication()` Missing (Security Vulnerability)

**File:** `TUTSportApp.Api/Program.cs`

`Program.cs` had `app.UseAuthorization()` but no `app.UseAuthentication()`. Without authentication middleware, JWT tokens were **never validated**. Every endpoint decorated with `[Authorize]` was effectively unguarded — any request, authenticated or not, would pass through.

```csharp
// BEFORE (broken)
app.UseAuthorization();

// AFTER (correct order)
app.UseAuthentication();   // ← must come first
app.UseAuthorization();
```

### Bug 2 — Failed Login Attempts Never Persisted (Data Loss)

**Files:** `LoginCommandHandler.cs`, `LoginRepository.cs`

`UpdateFailedAttemptsAsync` staged changes in the EF Core change tracker by calling `UpdateAsync` (which sets `EntityState.Modified`), but `SaveChangesAsync` was **never called**. Every failed login attempt was silently dropped — the database never recorded them, the lockout counter never incremented, and account locking never activated.

**Fix:** Added `IUnitOfWork` to `LoginCommandHandler` and called `SaveChangesAsync` after every failed attempt.

```csharp
await _loginRepository.UpdateFailedAttemptsAsync(login.Id, login.FailedAttempts + 1, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);   // ← was missing
```

---

## Clean Architecture Violations Fixed

### 1. EF Core Leaked into Domain

`IApplicationDbContext` in the Domain layer exposed `DbSet<T>` — forcing a NuGet dependency on `Microsoft.EntityFrameworkCore` directly into the innermost layer. Domain must have **zero external dependencies**.

| Before | After |
|--------|-------|
| `IApplicationDbContext` with `DbSet<User>`, `DbSet<Login>`, `DbSet<Company>` in Domain | Deleted entirely |
| `using Microsoft.EntityFrameworkCore` in Domain.csproj | Removed — Domain.csproj has zero NuGet references |
| `using Microsoft.EntityFrameworkCore` in Application.csproj | Removed |

### 2. `IUnitOfWork` Added to Domain

The only persistence abstraction Domain needs is a way to say "commit everything". Added a minimal interface:

```csharp
// TUTSportApp.Domain/Common/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

Implementation (`UnitOfWork.cs`) lives in Infrastructure and wraps `ApplicationDbContext`.

### 3. Misplaced Files Moved to Correct Layers

| File | Was in | Moved to | Reason |
|------|--------|----------|--------|
| `Result.cs` | Domain folder, Application namespace | `Application/Common/Models/` | Result type belongs in Application |
| `JwtSettings.cs` | Domain folder, Application namespace | `Infrastructure/Models/` | JWT config is Infrastructure concern |
| `LoginModel.cs` | Domain | Deleted | Dead code after `IAuthService` redesign |

### 4. Class Naming Collision Fixed

Both Application and Infrastructure had a class named `ServiceRegistration`. Renamed:
- `ApplicationServiceRegistration` in `TUTSportApp.Application`
- `InfrastructureServiceRegistration` in `TUTSportApp.Infrastructure`

---

## SOLID Violations Fixed

### Single Responsibility Principle (SRP)

**`AuthService.CreateTokenAsync` had two responsibilities:**
The method previously accepted a `LoginModel`, then re-fetched the `Login` entity from the database — even though `LoginCommandHandler` had just fetched it. `AuthService` had both a repository dependency and token-building logic.

**Fix:** Redesigned `IAuthService.CreateTokenAsync` to accept only the identity claims it needs:

```csharp
// BEFORE
Task<string> CreateTokenAsync(LoginModel model);   // re-fetches Login internally

// AFTER — only needs what it uses
Task<string> CreateTokenAsync(Guid userId, string username, CancellationToken ct = default);
```

`ILoginRepository` dependency removed from `AuthService` entirely.

**`ExceptionHandlingMiddleware` added (SRP for controllers):**  
Controllers no longer need try/catch blocks. All error-to-HTTP mapping is centralised in one place. Maps:
- `ValidationException` → `400 Bad Request` (RFC 7807 `ValidationProblemDetails`)
- `NotFoundException` → `404 Not Found` (RFC 7807 `ProblemDetails`)
- `UnauthorizedAccessException` → `401 Unauthorized`
- All others → `500 Internal Server Error` (safe message, no stack trace leak)

### Dependency Inversion / Liskov Substitution (DIP/LSP)

**`IGenericRepository<T>` constraint fixed:**

```csharp
// BEFORE — too weak; the Id property is not guaranteed
public interface IGenericRepository<T> where T : class

// AFTER — Id is always available; correct contract
public interface IGenericRepository<T> where T : BaseEntity
```

**`CancellationToken` added consistently:**  
All async methods in `IGenericRepository<T>`, `ILoginRepository`, and `ICompanyRepository` now accept `CancellationToken`. This was already present on `IUserRepository` but missing from the others — inconsistent APIs violate LSP.

### Open/Closed Principle (OCP)

`ValidationBehavior<TRequest,TResponse>` registered as an open-generic MediatR pipeline behavior:

```csharp
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

Every future command/query automatically benefits from validation without any handler code changes.

### Other Fixes

- `BaseEntity.Id = Guid.NewGuid()` — entities always have a valid Id from the moment of construction, eliminating a category of null-reference and "empty Guid" bugs
- All repository classes marked `sealed` where appropriate (GenericRepository is open for inheritance by design)

---

## Before / After Summary

| Concern | Before | After |
|---------|--------|-------|
| Domain NuGet deps | EF Core, Application namespace refs | Zero |
| Failed login persistence | Silently dropped | Persisted via IUnitOfWork |
| JWT auth middleware | Missing UseAuthentication() | Both in correct order |
| AuthService DB calls | Re-fetched Login from DB | Accepts only identity claims |
| Error handling | Each handler/controller needs try/catch | ExceptionHandlingMiddleware centralises all |
| Generic constraint | `where T : class` | `where T : BaseEntity` |
| CancellationToken | Inconsistent across interfaces | Consistent on all async methods |
| ValidationBehavior | Registered but never wired to pipeline | Open-generic pipeline behavior |
