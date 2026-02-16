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

// Send through the mediator
var orderId = await mediator.SendAsync(new CreateOrderCommand("Acme", 500m));
```

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
| `FailedValidationException` | Validation failures with violation details |
| `InternalException` | Internal/system errors |
| `Error` / `NotFoundError` / `NotValidError` / `UnhandledError` | Structured error results |
| `ProblemDetail` | RFC 7807 problem detail for HTTP APIs |

## Utility Types

- **`TypeSource`** -- Collects types from assemblies for discovery-based registration.
- **`TypeExtensions`** -- `GetConciseName()` for human-readable generic type names.
- **`AssemblyExtensions`** -- Query embedded resources and filter types.
- **`DateTimeHelpers`** -- Pre-built date range models and Unix timestamp conversion.
