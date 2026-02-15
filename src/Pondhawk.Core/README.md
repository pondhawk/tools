# Pondhawk.Core

Shared foundation library providing a Serilog-based logging API, pipeline infrastructure, type utilities, and common exception types.

## Logging API

### Get a Logger

```csharp
using Pondhawk.Logging;

// From any object -- uses the type's full name as the source context
var logger = this.GetLogger();
logger.Debug("Processing order {OrderId}", orderId);
```

### Method Tracing

```csharp
public void ProcessOrder(int orderId)
{
    using var _ = this.EnterMethod();
    // Logs "Entering ClassName.ProcessOrder" at Verbose level
    // On dispose: "Exiting ClassName.ProcessOrder (elapsed ms)"

    logger.Debug("Loading order from database");
    var order = LoadOrder(orderId);
    logger.Inspect("order.Total", order.Total);
}
```

### Object Serialization

```csharp
// Log a complex object as JSON payload
logger.LogObject(order);
logger.LogObject("Fetched Order", order);

// Typed payloads with syntax highlighting hints
logger.LogJson("Response", jsonString);
logger.LogSql("Query", sqlString);
logger.LogXml("Config", xmlString);
logger.LogYaml("Data", yamlString);
```

### Sensitive Data

```csharp
public class UserCredentials
{
    public string Username { get; set; }

    [Sensitive]
    public string Password { get; set; }  // Masked in LogObject output
}
```

## Pipeline Infrastructure

### Define a Pipeline

```csharp
using Pondhawk.Utilities.Pipeline;

// Register with Autofac
builder.RegisterPipelineFactory();
builder.AddPipeline<OrderContext>(steps => steps
    .Add<ValidateStep>()
    .Add<CalculateTaxStep>()
    .Add<SaveStep>());

// Execute
var pipeline = factory.Create<OrderContext>();
await pipeline.ExecuteAsync(context, async ctx =>
{
    // Main action runs between Before/After hooks
    await ProcessAsync(ctx);
});
```

## Exception Types

| Type | Purpose |
|------|---------|
| `ExternalException` | Base for application-facing errors with kind, code, explanation, details |
| `FluentException<T>` | Fluent builder API: `.WithKind().WithErrorCode().WithExplanation()` |
| `FunctionalException` | Business logic errors |
| `FailedValidationException` | Validation failures with violation details |
| `InternalException` | Internal/system errors |
| `Error` / `NotFoundError` / `NotValidError` / `UnhandledError` | Structured error results |
| `ProblemDetail` | RFC 7807 problem detail for HTTP APIs |

## Utility Types

- **`TypeSource`** -- Collects types from assemblies for discovery-based registration.
- **`TypeExtensions`** -- `GetConciseName()` for human-readable generic type names.
- **`AssemblyExtensions`** -- Query embedded resources and filter types.
- **`DateTimeHelpers`** -- Pre-built date range models and Unix timestamp conversion.
- **`FileSignalController`** -- File-based inter-process lifecycle signaling.
