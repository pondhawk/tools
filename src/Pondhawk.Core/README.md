# Pondhawk.Core

Shared foundation library providing a lightweight mediator, configuration-driven DI modules, pipeline infrastructure, type utilities, and common exception types.

## Mediator

CQRS-style request/response dispatch with pipeline behaviors for cross-cutting concerns.

```csharp
using Pondhawk.Mediator;

// Register mediator and auto-discover handlers from assemblies
services.AddMediator(typeof(CreateOrderHandler).Assembly);

// Register open-generic pipeline behaviors (logging, validation, etc.)
services.AddPipelineBehavior(typeof(LoggingBehavior<,>));

// Define a command
public record CreateOrderCommand(string Customer, decimal Total) : ICommand<int>;

// Define a handler
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, int>
{
    public async Task<int> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // ... create order, return ID
    }
}

// Send through the mediator — returns a Response<T> envelope, never throws on app errors
Response<int> result = await mediator.SendAsync(new CreateOrderCommand("Acme", 500m));

if (result.Ok)
    Console.WriteLine($"Created order {result.Value}");
else
    Console.WriteLine($"Failed ({result.Error!.Kind}): {result.Error.Explanation}");
```

### The `Response<T>` envelope

Handlers still **throw** on error — nothing about authoring changes. The mediator is the single
seam that converts a throw into a `Response<T>`: a thrown `ExternalException` becomes a
`Response.Failure` carrying a structured `ErrorInfo` (with the error `Kind`), while an unexpected
exception is logged at `Error` and enveloped as `ErrorKind.System`. `OperationCanceledException`
and configuration errors (no handler registered) still propagate. This lets non-HTTP callers —
queue consumers and batch — branch on the outcome without catching:

```csharp
var response = await mediator.SendAsync(command);
response.Match(
    onSuccess: value => Ack(value),
    onFailure: error => error.Kind.IsTransient() ? Requeue() : DeadLetter(error));
```

`ErrorKindPolicy.IsTransient(this ErrorKind)` is the canonical retry-vs-dead-letter default
(`System`, `Remote`, `Concurrency` are transient). The HTTP status mapping deliberately lives in
the ASP.NET layer, not here — `Pondhawk.Core` stays transport-agnostic.

### Pipeline Behaviors

Wrap handler execution for cross-cutting concerns:

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next)
    {
        Log.Information("Handling {Request}", typeof(TRequest).Name);
        var response = await next();
        Log.Information("Handled {Request}", typeof(TRequest).Name);
        return response;
    }
}
```

### Batch Support

Track batch operations with `AsyncLocal`-based context:

```csharp
using var scope = BatchExecutionContext.BeginBatch("import-2024");
// All mediator calls within this scope share the batch context
```

## Configuration Modules

Bind DI modules from `IConfiguration` and register services in one step:

```csharp
using Pondhawk.Configuration;

public class DatabaseModule : IServiceModule
{
    public string ConnectionString { get; set; } = "";
    public int PoolSize { get; set; } = 10;

    public void Build(IServiceCollection services)
    {
        services.AddDbContext<AppDb>(o => o.UseSqlServer(ConnectionString));
    }
}

// In Startup — properties bound from config, then Build() called
services.AddServiceModule<DatabaseModule>(configuration.GetSection("Database"));

// With post-binding overrides
services.AddServiceModule<DatabaseModule>(configuration.GetSection("Database"),
    module => module.PoolSize = 20);
```

## Pipeline Infrastructure

Composable step-based execution with Before/After hooks:

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
| `FailedValidationException` | Validation failures with violation details (`Kind = Predicate`) |
| `NotFoundException` | Resource not found (`Kind = NotFound`) |
| `ConflictException` | State conflict (`Kind = Conflict`) |
| `NotAuthorizedException` | Caller not authorized (`Kind = NotAuthorized`) |
| `InternalException` | Internal/system errors |
| `ErrorInfo` | Transport-agnostic error shape shared by exceptions and the `Response<T>` envelope |
| `ErrorKindPolicy` | `IsTransient(ErrorKind)` — canonical retry/dead-letter default |
| `Error` / `NotFoundError` / `NotValidError` / `UnhandledError` | Structured error results |
| `ProblemDetail` | RFC 7807 problem detail for HTTP APIs |

## Utility Types

- **`TypeSource`** -- Collects types from assemblies for discovery-based registration.
- **`TypeExtensions`** -- `GetConciseName()` for human-readable generic type names.
- **`AssemblyExtensions`** -- Query embedded resources and filter types.
- **`DateTimeHelpers`** -- Pre-built date range models and Unix timestamp conversion.
