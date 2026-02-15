# Pondhawk.Core

Shared foundation library providing pipeline infrastructure, type utilities, and common exception types.

## Pipeline Infrastructure

### Define a Pipeline

```csharp
using Pondhawk.Utilities.Pipeline;

// Register with DI
services.AddPipelineFactory();
services.AddPipeline<OrderContext>(steps => steps
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
