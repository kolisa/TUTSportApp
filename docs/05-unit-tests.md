# Improvement 5 — Real Unit Test Coverage

**Commit:** `5eaf740`  
**Scope:** `TUTSportApp.UnitTest`

---

## Problem

`TUTSportApp.UnitTest` contained a single file, `UnitTest1.cs`, with one test:

```csharp
[Fact]
public void Test1()
{
    Assert.True(true);
}
```

This provided zero actual coverage and gave a false sense of a working test suite.

---

## What Was Done

`UnitTest1.cs` was deleted and replaced with three test files covering the Application layer in full isolation — no database, no HTTP, no EF Core.

### Test Project Setup

```xml
<ItemGroup>
  <PackageReference Include="xunit" />
  <PackageReference Include="Moq" />
  <PackageReference Include="FluentAssertions" />
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\TUTSportApp.Application\..." />
  <ProjectReference Include="..\TUTSportApp.Domain\..." />
</ItemGroup>
```

No Infrastructure reference — unit tests must not touch EF Core or any real service.

---

## `LoginCommandValidatorTests` (10 tests)

Tests every validation rule in `LoginCommandValidator` using FluentValidation's `TestValidate` helper.

| Test | Rule Verified |
|------|--------------|
| `Username_Empty_HasValidationError` | `NotEmpty()` |
| `Username_Whitespace_HasValidationError` | `NotEmpty()` (whitespace treated as empty) |
| `Username_ExceedsMaxLength_HasValidationError` | `MaximumLength(50)` |
| `Username_AtMaxLength_NoValidationError` | Boundary: 50 chars is valid |
| `Username_Valid_NoValidationError` | Happy path |
| `Password_Empty_HasValidationError` | `NotEmpty()` |
| `Password_TooShort_HasValidationError` | `MinimumLength(6)` |
| `Password_AtMinLength_NoValidationError` | Boundary: 6 chars is valid |
| `Password_Valid_NoValidationError` | Happy path |
| `BothFieldsEmpty_HasTwoValidationErrors` | Both rules fire independently |

**No external dependencies.** The validator is instantiated directly:

```csharp
private readonly LoginCommandValidator _validator = new();
```

---

## `LoginCommandHandlerTests` (8 tests)

Tests `LoginCommandHandler` with all dependencies mocked via Moq.

```csharp
private readonly Mock<IAuthService>     _authService     = new();
private readonly Mock<ILoginRepository> _loginRepository = new();
private readonly Mock<IUnitOfWork>      _unitOfWork      = new();

private LoginCommandHandler CreateHandler()
    => new(_authService.Object, _loginRepository.Object, _unitOfWork.Object);
```

| Test | What it Verifies |
|------|-----------------|
| `Handle_ValidCredentials_ReturnsSuccessWithToken` | Happy path — `Result.Success` with token |
| `Handle_UserNotFound_ReturnsInvalidCredentials` | Repo returns `null` → failure |
| `Handle_LockedAccount_ReturnsAccountLocked` | `IsLocked=true` → failure before password check |
| `Handle_WrongPassword_ReturnsInvalidCredentials` | `VerifyPasswordHash` returns `false` |
| `Handle_WrongPassword_IncrementsFailedAttempts` | `UpdateFailedAttemptsAsync` called with `currentAttempts + 1` |
| `Handle_WrongPassword_PersistsViaUnitOfWork` | `IUnitOfWork.SaveChangesAsync` called exactly once |
| `Handle_SuccessfulLogin_DoesNotCallSaveChanges` | No DB write on successful read-only login |
| `Handle_NullRequest_ThrowsArgumentNullException` | Null guard at top of `Handle` |

**Key test — verifies the Bug 2 fix:**

```csharp
[Fact]
public async Task Handle_WrongPassword_PersistsViaUnitOfWork()
{
    // ...setup...
    _unitOfWork.Verify(
        u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
        Times.Once);
}
```

This test would have **failed against the original code** because `SaveChangesAsync` was never called. It now serves as a regression guard.

---

## `ValidationBehaviorTests` (4 tests)

Tests `ValidationBehavior<TRequest, TResponse>` in isolation.

```csharp
private sealed record TestRequest : IRequest<string>
{
    public string Value { get; init; } = string.Empty;
}
```

| Test | What it Verifies |
|------|-----------------|
| `Handle_NoValidators_CallsNextDelegate` | No validators = pipeline continues |
| `Handle_AllValidatorsPass_CallsNextDelegate` | Valid request = pipeline continues |
| `Handle_ValidatorFails_ThrowsValidationException` | Invalid request = `ValidationException` thrown |
| `Handle_ValidatorFails_DoesNotCallNextDelegate` | Handler is **not called** when validation fails |

The last test is critical — it verifies that `ValidationBehavior` acts as a true gate, not just a side-effect logger.

---

## Philosophy

Unit tests in this layer follow three strict rules:

1. **No infrastructure.** No EF Core, no database, no HTTP client.
2. **Mock every dependency.** Use Moq to control exactly what repositories and services return.
3. **One assertion focus per test.** Each test verifies one specific behaviour.

This makes tests fast (milliseconds each), deterministic, and pinpointed — a failing test immediately tells you which behaviour broke.
