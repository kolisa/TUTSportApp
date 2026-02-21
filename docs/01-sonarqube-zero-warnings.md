# Improvement 1 — SonarQube Integration & Zero-Warning Policy

**Commit:** `82ba11f`  
**Scope:** Solution-wide build quality enforcement

---

## Problem

The codebase had no static analysis enforcement. Warnings were silently ignored, meaning code quality issues accumulated undetected. There was no SonarQube integration, no zero-warning gate, and no consistent tooling to catch violations before they reached a PR.

---

## What Was Done

### 1. Central SonarAnalyzer Configuration (`Directory.Packages.props`)

`SonarAnalyzer.CSharp` was added as a `GlobalPackageReference` — meaning it applies automatically to **every project** in the solution without each `.csproj` needing to reference it individually.

```xml
<ItemGroup>
  <GlobalPackageReference Include="SonarAnalyzer.CSharp" PrivateAssets="all" />
</ItemGroup>
```

### 2. Zero-Warning Build Policy

Four MSBuild properties were added to `Directory.Packages.props` so they apply solution-wide:

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<AnalysisLevel>latest</AnalysisLevel>
<AnalysisMode>All</AnalysisMode>
<CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
```

This means **the build fails on any warning** — Roslyn, SonarQube, or EditorConfig style rules.

### 3. Eight Violations Fixed

| File | Rule | Fix |
|------|------|-----|
| `ProfileMapper.cs` | CS8019 | Removed 5 unused `using` directives |
| `AuthService.cs` | CS8019 | Removed unused `System.Buffers.Binary` and `Features.Auth.Commands` usings |
| `AuditableEntity.cs` | CS8019 | Removed redundant `using System;` (covered by implicit usings) |
| `ApplicationDbContext.cs` | S131 | Added `default: break;` to switch statement |
| `LoginRepository.cs` | IDE0150/S6397 | Changed `!= null` to `is not null` pattern matching |
| `LoginCommand.cs` | S4457 | Removed `IValidatableObject` + `DataAnnotations` (duplicated FluentValidation) |
| `LoginCommand.cs` | S125 | Removed commented-out `/* cancellationToken */` blocks |
| `LoginCommand.cs` | IDE0300 | Renamed shadowed `mapper` variable to `loginModel` |
| `Company.cs` | CS8019 | Removed unused `System` and `Collections.Generic` usings |
| `LoginModel.cs` | CS8019 | Removed unused `DataAnnotations` using |

### 4. `sonar-project.properties`

Added root-level SonarQube project configuration for CI scanner integration:

```properties
sonar.projectKey=TUTSportApp
sonar.sources=.
sonar.exclusions=**/obj/**,**/bin/**,**/*.Designer.cs
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml
```

---

## Impact

- Every future PR is automatically checked — any new warning breaks the build
- SonarQube rules run at compile time via the Roslyn analyzer (no separate scan step needed in dev)
- Code style is enforced consistently across every developer's machine via `.editorconfig` + `EnforceCodeStyleInBuild`

---

## Rule Reference

| Rule | Name | Category |
|------|------|----------|
| CS8019 | Unnecessary using directive | Roslyn |
| S131 | Switch statement should have a default case | SonarQube |
| S125 | Sections of code should not be commented out | SonarQube |
| S4457 | Split method with optional params into two | SonarQube |
| S6397 / IDE0150 | Use pattern matching over null check | Roslyn/Sonar |
| IDE0300 | Collection expression simplification | Roslyn |
