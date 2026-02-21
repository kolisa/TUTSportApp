## 🧪 How to Write and Run Tests


### Unit Tests

- **Location:** `TUTSportApp.UnitTest/`
- **Purpose:** Test business logic in isolation (no database, HTTP, or infrastructure).
- **How to Add:**
  1. Create or update test classes in the appropriate feature folder (e.g., `Features/Auth/Commands`).
  2. Use xUnit for test methods (`[Fact]` or `[Theory]`).
  3. Mock all dependencies using Moq or similar libraries.
  4. Use FluentAssertions for expressive assertions.
  5. Focus each test on a single behavior or rule.

**Example Unit Test:**
```csharp
using Xunit;
using Moq;
using FluentAssertions;
using TUTSportApp.Application.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ReturnsInvalidCredentials()
    {
        var authService = new Mock<IAuthService>();
        var loginRepo = new Mock<ILoginRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new LoginCommandHandler(authService.Object, loginRepo.Object, unitOfWork.Object);

        loginRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((Login)null);

        var result = await handler.Handle(new LoginCommand { Username = "nouser", Password = "pass" }, default);
        result.IsSuccess.Should().BeFalse();
    }
}
```

- **How to Run:**
  ```bash
  dotnet test TUTSportApp.UnitTest/TUTSportApp.UnitTest.csproj
  ```


### Integration Tests

- **Location:** `TUTSportApp.IntegrationTest/`
- **Purpose:** Test the full stack (API, MediatR, repositories, EF Core) using in-memory or test infrastructure.
- **How to Add:**
  1. Create or update test classes in the relevant folder (e.g., `Auth/`, `Repositories/`).
  2. Use the provided `IntegrationTestWebApplicationFactory` and `IntegrationTestBase` for setup.
  3. Seed test data using `TestDataSeeder` if needed.
  4. Use HttpClient to make real HTTP requests to the in-memory API.
  5. Assert on HTTP responses, database state, and service behavior.

**Example Integration Test:**
```csharp
using Xunit;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TUTSportApp.IntegrationTest.Infrastructure;

public class LoginEndpointTests : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public LoginEndpointTests(IntegrationTestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_ForInvalidCredentials()
    {
        var content = new StringContent("{\"username\":\"baduser\",\"password\":\"badpass\"}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/login", content);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
```

- **How to Run:**
  ```bash
  dotnet test TUTSportApp.IntegrationTest/TUTSportApp.IntegrationTest.csproj
  ```

### Best Practices
- Keep unit and integration tests separate.
- Mock all external dependencies in unit tests.
- Use real infrastructure (in-memory DB, DI) in integration tests.
- Run all tests before submitting a PR.

[![Coverage](https://img.shields.io/badge/coverage-unknown-lightgrey)](./TestResults/CoverageReport/index.html)

# 🏗️ TUTSportApp (.NET 9) – Setup & Development Guide

> A modern sports management application built on **.NET 9**, with a modular structure for clean architecture, scalability, and maintainability.

---

## 📚 Table of Contents

1. [Prerequisites](#-prerequisites)
2. [Repository Structure](#-repository-structure)
3. [Initial Setup](#-initial-setup)
4. [Central NuGet Package Management](#-central-nuget-package-management)
5. [Database Setup](#️-database-setup)
6. [Adding New Projects](#-adding-new-projects)
7. [Common Issues & Fixes](#️-common-issues--fixes)
8. [Best Practices](#-best-practices)
9. [Additional Resources](#-helpful-resources)

11. [How to Add a New Feature](#-how-to-add-a-new-feature)

---

## 📋 Prerequisites

Ensure the following are installed:

- **Visual Studio 2022** (latest update recommended)  
- **.NET 9 SDK**  
- **SQL Server** (LocalDB or Express for development)  
- **Git**

---

## 📁 Repository Structure


```bash
/TUTSportApp
  TUTSportApp.Api/            # ASP.NET Core Web API
  TUTSportApp.Application/    # Application layer
  TUTSportApp.Domain/         # Domain layer
  TUTSportApp.Infrastructure/ # Infrastructure layer
  TUTSportApp.UnitTest/       # Unit tests
  TUTSportApp.IntegrationTest/# Integration tests
  docs/                       # Documentation
  scripts/                    # Dev helper scripts
  docker-compose.yml          # Local SQL Server
  .github/workflows/ci.yml    # CI pipeline
  README.md                   # This file
```

---



## 🛠️ How to Add a New Feature

Follow these steps to add a new feature in line with Clean Architecture and project conventions:

1. **Create a Feature Folder**
  - In `TUTSportApp.Application/Features`, create a new folder for your feature (e.g., `Sport`).

2. **Define Request/Command/Query**
  - Add a new `Command` or `Query` record/class (e.g., `CreateSportCommand.cs`).
  - Implement the corresponding `Handler` (e.g., `CreateSportCommandHandler`).

3. **Add Validation**
  - Create a `Validator` class for your command/query using FluentValidation (e.g., `CreateSportCommandValidator`).

4. **Update Domain/Entities if Needed**
  - If your feature requires new domain models or changes, update `TUTSportApp.Domain/Entities` accordingly.

5. **Add Repository Methods**
  - If persistence is needed, add methods to the appropriate repository interface and implementation in `TUTSportApp.Domain/Common/Interfaces` and `TUTSportApp.Infrastructure/Data/Repositories`.

6. **Register Dependencies**
  - Ensure any new services or repositories are registered in the DI container (see `DependencyInjection.cs`).

7. **Add Controller Endpoint**
  - In `TUTSportApp.Api/Controllers`, add or update a controller to expose your feature via HTTP.

8. **Write Unit Tests**
  - Add or update tests in `TUTSportApp.UnitTest/Features/<YourFeature>` for validators, handlers, and any business logic.

9. **Write Integration Tests**
  - Add or update tests in `TUTSportApp.IntegrationTest/<YourFeature>` to cover end-to-end scenarios.

10. **Update Documentation**
   - Document your feature in the `docs/` folder if needed.

11. **Run All Tests and Lint**
   - Run `dotnet test` and ensure all tests pass.
   - Check code style and static analysis (SonarQube, .editorconfig).

12. **Open a Pull Request**
   - Follow the PR process in CONTRIBUTING.md.

---

### 1. Clone the Repository
```bash
git clone <repo-url>
cd TUTSportApp
```

### 2. One-Command Setup (Windows)
```powershell
scripts/setup-dev.ps1
```

This will:
- Check .NET 9 SDK, Docker, and dotnet-ef
- Generate a JWT key in User Secrets
- Start SQL Server via Docker Compose
- Apply EF Core migrations
- Build the solution (zero warnings)

### 3. Open in Visual Studio
- Launch **Visual Studio 2022**
- Open the `.sln` file

### 4. Run the API
```bash
dotnet run --project TUTSportApp.Api/TUTSportApp.Api.csproj
```

### 5. Run Tests
```bash
dotnet test
```

---

## 🐳 Local SQL Server (Docker)

The solution uses Docker Compose to run SQL Server locally. See `docker-compose.yml` for details.

---

## 🤖 Continuous Integration

All pushes and PRs to `main` run the CI pipeline defined in `.github/workflows/ci.yml`.

### 3. Restore NuGet Packages
- Automatically restored by Visual Studio  
- Or manually:
  ```bash
  dotnet restore
  ```

### 4. Build the Solution
- **In Visual Studio:**  
  `Build → Rebuild Solution`
- **Or via CLI:**
  ```bash
  dotnet build
  ```

---

## 📦 Central NuGet Package Management

All package versions are centrally defined in `Directory.Packages.props`.

### ✅ Rules

- **Do not** include a `Version` attribute in individual `.csproj` files.
- To add a new package:

1. Add it in `Directory.Packages.props`:
   ```xml
   <PackageVersion Include="Package.Name" Version="x.y.z" />
   ```
2. Reference it in your project:
   ```xml
   <PackageReference Include="Package.Name" />
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```

---

## 🗄️ Database Setup

Connection strings are defined in:
```
src/TUTSportApp.API/appsettings.json
```

### Apply Migrations
```bash
dotnet ef database update
```

> ✅ Ensure the `DefaultConnection` string in `appsettings.json` is correct for your local setup.

---

## ➕ Adding New Projects

1. Create the new project under `/src` or `/tests`.  
2. Add necessary project references.  
3. Add NuGet dependencies:
   - Version in `Directory.Packages.props`
   - Reference in `.csproj` without version number

---

## ⚙️ Common Issues & Fixes

| Issue | Solution |
|-------|-----------|
| **Central Package Management errors** | Remove all `Version` attributes from `.csproj` files. |
| **Missing types or namespaces** | Add the correct `using` directives. |
| **Build errors** | Ensure all project references and packages are properly restored. |

---

## 🧭 Best Practices

- Use **Central Package Management** for all dependencies  
- Keep base classes (`BaseEntity`, `AuditableEntity`) in the **Domain** layer  
- Use **CQRS** and **MediatR** for clean application logic separation  
- Use **FluentValidation** for request validation  
- Keep secrets and connection strings **out of source control**  
  - Use **Environment Variables** or **User Secrets** for production

---

## 📚 Helpful Resources

- [🧩 .NET Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management)  
- [🗃️ EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)  
- [🧪 xUnit Testing](https://xunit.net/docs/getting-started/netfx/visual-studio)

---

## 🧩 Troubleshooting Tip

If you encounter issues:
- Check the **Error List** and **Build Output** in Visual Studio  
- Confirm all setup steps were followed  
- Ensure all dependencies are restored and up to date  

---

> Maintained by the **TUTSportApp Development Team** 💻  
> For assistance, please reach out to your team lead or open a GitHub issue.
