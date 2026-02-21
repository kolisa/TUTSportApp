# Improvement 4 — MediatR Pipeline Behaviors & Serilog Logging

**Commit:** `5eaf740`  
**Scope:** `TUTSportApp.Application`, `TUTSportApp.Api`

---

## Problem

The application had no structured logging and no way to observe what was happening inside MediatR handlers. Debugging required attaching a debugger or reading raw ASP.NET Core logs. There was also no automatic performance monitoring — slow handlers went undetected.

---

## MediatR Pipeline Behaviors

Every command and query now passes through three behaviors automatically, in a fixed order, before reaching its handler:

```
Request
  → PerformanceBehavior   (starts stopwatch)
    → LoggingBehavior     (logs request name)
      → ValidationBehavior (runs FluentValidation)
        → Handler
      ← ValidationBehavior
    ← LoggingBehavior     (logs completion)
  ← PerformanceBehavior   (logs warning if > 500ms)
Response
```

All three are registered as **open-generic** behaviors — they apply to every `IRequest<T>` automatically with no per-handler wiring:

```csharp
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### `ValidationBehavior<TRequest, TResponse>`

Runs all registered `IValidator<TRequest>` implementations before the handler executes. If any rule fails, throws `FluentValidation.ValidationException`, which `ExceptionHandlingMiddleware` catches and maps to `400 Bad Request` with a structured `ValidationProblemDetails` body.

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation Failed",
  "status": 400,
  "errors": {
    "Username": ["Username is required"],
    "Password": ["Password must be at least 6 characters long"]
  }
}
```

Before this, FluentValidation validators were registered but **never executed** — requests with invalid data reached handlers silently.

### `LoggingBehavior<TRequest, TResponse>`

Logs request name on entry and exit using `ILogger<T>` (Serilog underneath):

```
[14:32:01 INF] Handling LoginCommand
[14:32:01 INF] Handled LoginCommand
```

Useful for tracing which commands ran during a request, especially in more complex flows with multiple dispatched commands.

### `PerformanceBehavior<TRequest, TResponse>`

Uses `Stopwatch` to measure handler execution time. Logs a warning if the handler exceeds **500 milliseconds**:

```
[14:32:05 WRN] Slow request detected: LoginCommand took 623ms (threshold: 500ms)
```

This makes performance regressions visible in logs without requiring any profiler setup.

---

## Serilog Structured Logging

### Why Serilog Over the Default Logger

The default Microsoft logging outputs plain text. Serilog outputs **structured JSON-compatible logs** with message templates, making logs searchable and parseable by tools like Seq, Elastic, or Datadog.

### Configuration

Serilog is configured entirely in `appsettings.json` — no code changes needed to adjust log levels or add sinks:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/tutsportapp-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithEnvironmentName"]
  }
}
```

### Bootstrap Logger Pattern

`Program.cs` uses a two-stage logger so startup errors are captured even before full Serilog is configured:

```csharp
// Stage 1: captures fatal startup errors
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    // Stage 2: full config from appsettings.json
    builder.Host.UseSerilog((context, services, cfg) =>
        cfg.ReadFrom.Configuration(context.Configuration)
           .ReadFrom.Services(services));
    // ...
}
catch (Exception ex)
{
    Log.Fatal(ex, "TUTSportApp API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
```

### Per-Environment Overrides

| Environment | Level | EF Query Logging |
|-------------|-------|-----------------|
| Production | Information | Warning (query text hidden) |
| Development | Debug | Information (shows SQL) |
| Testing | Warning | Off |

### Request Logging

`app.UseSerilogRequestLogging()` replaces the verbose default ASP.NET Core request logs with a single structured line per request:

```
[14:32:01 INF] HTTP POST /api/login/login responded 200 in 47ms
```

---

## Packages Added

```xml
<PackageVersion Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageVersion Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageVersion Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageVersion Include="Serilog.Enrichers.Environment" Version="3.0.1" />
<PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.0" />
```
