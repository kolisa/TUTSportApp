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
10. [Troubleshooting Tip](#-troubleshooting-tip)

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
  /src
    /TUTSportApp.API             # ASP.NET Core Web API
    /TUTSportApp.UI              # Frontend (Blazor, React, etc.)
  /tests
    /TUTSportApp.UnitTests       # Unit tests
    /TUTSportApp.IntegrationTests# Integration tests
  /docs                          # Documentation
  /scripts                       # Dev helper scripts
  .gitignore                     # Git ignore settings
  README.md                      # This file
```

---

## 🚀 Initial Setup

### 1. Clone the Repository
```bash
git clone <repo-url>
```

### 2. Open in Visual Studio
- Launch **Visual Studio 2022**
- Open the `.sln` file

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
