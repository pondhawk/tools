# Pondhawk.Watch.Framework

Pure `Microsoft.Extensions.Logging` provider for structured logging with Watch Server integration. Targets `netstandard2.0` for .NET Framework compatibility.

## Quick Start

### Configure Logging

```csharp
using Microsoft.Extensions.Logging;

// Development -- Console output
builder.Logging.AddWatch(w => w
    .UseConsole()
    .WhenNotMatched(Level.Debug, Color.LightGray));

// Production -- HTTP sink to Watch Server
builder.Logging.AddWatch(w => w
    .UseHttpSink("http://watch-server:11000", "MyApp")
    .UseHttpSwitchSource("http://watch-server:11000", "MyApp"));

// High performance -- Zero overhead
builder.Logging.AddWatch(w => w.UseQuiet());
```

### Use with ILogger

```csharp
public class OrderService
{
    private readonly ILogger _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync(int orderId)
    {
        using var _ = _logger.EnterMethod();

        _logger.LogDebug("Loading order {OrderId}", orderId);
        var order = await _repo.GetOrderAsync(orderId);
        _logger.LogObject(order);
    }
}
```

## Key Features

- **Method Tracing** -- `EnterMethod()` creates collapsible hierarchy with timing.
- **Object Serialization** -- `LogObject()` serializes with `[Sensitive]` masking.
- **Typed Payloads** -- `LogJson()`, `LogSql()`, `LogXml()`, `LogYaml()` for syntax highlighting.
- **Dynamic Switches** -- Runtime log level control per category pattern via `WhenMatched()`.
- **Circuit Breaker** -- HTTP sink buffers critical events during outages.
- **Channel Batching** -- Events queued and batched by size/time.
- **Zero-Allocation Quiet Mode** -- For high-performance production scenarios.

## Configuration Options

| Method | Description |
|--------|-------------|
| `UseQuiet()` | Zero overhead, no logging |
| `UseConsole()` | Console output |
| `UseHttpSink(url, domain)` | Send to Watch Server |
| `UseHttpSwitchSource(url, domain)` | Fetch switches from server |
| `WhenNotMatched(level, color)` | Default switch for unmatched categories |
| `WhenMatched(pattern, level, color)` | Pattern-specific switch |

## Compatibility

Targets `netstandard2.0` for .NET Framework support. Uses `Newtonsoft.Json` for serialization (vs `System.Text.Json` / `MemoryPack` in the modern `Pondhawk.Watch`).
