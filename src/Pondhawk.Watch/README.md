# Pondhawk.Watch

A Serilog `ILogEventSink` with Channel-based batching for the Watch structured logging pipeline. Provides rich structured logging with method tracing, object serialization, and multiple sink targets.

## Quick Start

### Configure Serilog for Watch

```csharp
using Pondhawk.Watch;
using Serilog;

// Recommended — Watch Server controls log levels via switches
Log.Logger = new LoggerConfiguration()
    .UseWatch("http://localhost:11000", "MyApp")
    .CreateLogger();

// Advanced — manual MinimumLevel and sink configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Watch("http://localhost:11000", "MyApp")
    .CreateLogger();
```

### Use the Logging API

```csharp
using Pondhawk.Watch;

public class OrderService
{
    public void ProcessOrder(int orderId)
    {
        using var _ = this.EnterMethod();
        var logger = this.GetLogger();

        logger.Debug("Loading order {OrderId}", orderId);
        var order = LoadOrder(orderId);
        logger.LogObject(order);
    }
}
```

### Method Tracing

```csharp
public async Task ProcessAsync(int orderId)
{
    using var _ = this.EnterMethod();
    // Logs "Entering ClassName.ProcessAsync" at Verbose level
    // On dispose: "Exiting ClassName.ProcessAsync (elapsed ms)"
}
```

### Object Serialization

```csharp
logger.LogObject(order);                       // Serialize to JSON payload
logger.LogObject("Fetched Order", order);      // With custom title

// Typed payloads with syntax highlighting hints
logger.LogJson("API Response", jsonString);
logger.LogSql("Query", sqlString);
logger.LogXml("Configuration", xmlString);
logger.LogYaml("Settings", yamlString);
logger.LogText("Output", textString);
```

### Sensitive Data Masking

```csharp
public class Credentials
{
    public string Username { get; set; }

    [Sensitive]
    public string Password { get; set; }  // Logged as "Sensitive - HasValue: true"
}
```

## Key Components

- **WatchSink** -- `ILogEventSink` with unbounded `Channel` batching. Converts Serilog events to Watch `LogEvent` instances.
- **Switching** -- Dynamic log level control via `ISwitch`/`ISwitchSource` with pattern matching (longest prefix wins).
- **Console Sink** -- Colored console output.
- **Monitor Sink** -- Accumulates events for testing.
- **HTTP Sink** -- Posts event batches to Watch Server with circuit breaker and critical event buffering.
- **LogEvent / LogEventBatch** -- MemoryPack-serializable event model with Brotli compression.

## Architecture

Events flow: Serilog `ILogger` -> WatchSink (Channel queue) -> Background batch task -> Event sink (Console / HTTP / Monitor).

Switch-based filtering checks the source context pattern against configured switches. Longest prefix match wins. Version-based invalidation ensures cached loggers see switch updates without recreation.

Fully standalone -- no dependency on Pondhawk.Core. Logging API types (`SerilogExtensions`, `MethodLogger`, `PayloadType`, `SensitiveAttribute`) are included directly.

## Documentation

See [CLAUDE.md](CLAUDE.md) for detailed AI development guidance and logging conventions.
