# Pondhawk.Hosting

Lightweight service lifecycle management for `Microsoft.Extensions.Hosting`. Co-locate service start/stop logic with DI registration. Standalone -- no dependency on other Pondhawk packages.

## Quick Start

### Register Services with Start Logic

```csharp
using Pondhawk.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Sync start
builder.Services.AddSingletonWithStart<MyService>(s => s.Initialize());

// Sync start + stop
builder.Services.AddSingletonWithStart<CacheService>(
    s => s.WarmUp(),
    s => s.Flush());

// Async start + stop with cancellation
builder.Services.AddSingletonWithStart<BackgroundProcessor>(
    (s, ct) => s.StartAsync(ct),
    (s, ct) => s.StopAsync(ct));

var app = builder.Build();
await app.RunAsync();
// Start actions fire on host startup
// Stop actions fire in reverse order on shutdown
```

### With the Rules Engine

```csharp
builder.Services.AddSingletonWithStart<RuleSetFactory>(f => f.Start());
```

## How It Works

- `AddSingletonWithStart<T>` registers your service as a singleton and stores the start/stop lambdas.
- `ServiceStarterHostedService` (auto-registered) implements `IHostedService`.
- On `StartAsync`, it resolves each registered service and calls its start lambda.
- On `StopAsync`, it calls stop lambdas in reverse registration order.
- Falls back to `NullLoggerFactory` when `AddLogging()` hasn't been called (e.g. in tests).
