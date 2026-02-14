# Fabrica.Watch - AI Development Guide

## Overview

Fabrica.Watch is a pure Microsoft.Extensions.Logging provider designed for high-performance, structured logging with rich UI visualization support. It provides seamless integration with the Watch Server ecosystem.

---

## Logging Guidelines

**Logging is the primary debugging tool.** You cannot attach a debugger in production, but you can always read logs. Well-structured logging tells you exactly what happened and why.

### 1. Start Methods with EnterMethod

Most methods should begin with `EnterMethod()`. Only the simplest methods (one-liners, trivial getters) skip this.

```csharp
public class OrderService
{
    private readonly ILogger _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task<Order> ProcessOrderAsync(int orderId)
    {
        using var _ = _logger.EnterMethod();  // Use discard - EnterMethod returns IDisposable, not a logger

        _logger.LogDebug("Loading order from database");
        var order = await _repository.GetOrderAsync(orderId);
        _logger.LogObject(order);

        return order;
    }
}
```

- Inject `ILogger<T>` via constructor (MS DI handles category automatically)
- Store as `_logger` field
- Use discard `_` for `EnterMethod()` return value
- Creates collapsible hierarchy in log viewers with automatic timing

### 2. Logging IS Comments, Comments ARE Logging

**Do not write comments. Write log statements instead.**

The log serves as both runtime documentation AND debugging information. Comments are invisible in production; logs are not.

```csharp
// BAD - Comment invisible in production
// Validate the order before processing
if (!order.IsValid)
    return null;

// GOOD - Log visible in production, serves as documentation
_logger.LogDebug("Validating order before processing");
if (!order.IsValid)
{
    _logger.LogDebug("Order validation failed");
    return null;
}
```

### 3. Log Calculated and Fetched Values

When you calculate a value or fetch it from somewhere (database, API, config), log it.

```csharp
var discount = CalculateDiscount(customer);
_logger.LogDebug("{Name} = {Value}", nameof(discount), discount);

var user = await _repository.GetUserAsync(userId);
_logger.LogDebug("{Name} = {Value}", nameof(user), user?.Email ?? "not found");
```

### 4. Use LogObject for Complex Types

When fetching objects from a database or receiving complex DTOs, use `LogObject` to capture the full state.

```csharp
var order = await _db.Orders.FindAsync(orderId);
_logger.LogObject(order);

var response = await _client.GetAsync<ApiResponse>(url);
_logger.LogObject(response);
```

`LogObject` uses a custom serializer that:
- **Catches exceptions from property getters** - Some objects (e.g., MemoryStream) have properties that throw when accessed. System.Text.Json has no built-in way to handle this. Our serializer catches these and returns defaults.
- **Respects [Sensitive] attribute** - Properties marked with `[Sensitive]` are masked.

### 5. Mark Sensitive Data with [Sensitive]

Never log passwords, API keys, tokens, or PII. Mark sensitive properties with the `[Sensitive]` attribute:

```csharp
public class UserCredentials
{
    public string Username { get; set; }

    [Sensitive]
    public string Password { get; set; }

    [Sensitive]
    public string ApiKey { get; set; }
}

// Logs: { "Username": "jsmith", "Password": "Sensitive - HasValue: true", "ApiKey": "Sensitive - HasValue: true" }
_logger.LogObject(credentials);
```

The serializer replaces sensitive values with `"Sensitive - HasValue: true/false"` so you know if a value was present without exposing it.

### 6. Provide Context for Problem-Solving

Give log viewers all the information they need to solve problems. Include relevant IDs, states, and values. But don't go crazy - log what matters.

```csharp
_logger.LogDebug("Processing payment for Order {OrderId}, Amount {Amount}, Customer {CustomerId}",
    order.Id, order.Total, order.CustomerId);
```

### 7. Exception Context is Critical

When logging exceptions, include whatever context helps understand what was happening when the error occurred.

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to process order {OrderId} for customer {CustomerId} with amount {Amount}",
        orderId, customerId, amount);
    throw;
}
```

### Summary

| Principle | Practice |
|-----------|----------|
| Start methods | `using var _ = _logger.EnterMethod();` |
| Replace comments | `_logger.LogDebug("Explanation of what's happening");` |
| Log values | `_logger.LogDebug("{Name} = {Value}", nameof(x), x);` |
| Log complex objects | `_logger.LogObject(dto);` |
| Mark sensitive data | `[Sensitive]` attribute on properties |
| Provide context | Include IDs, states, relevant values |
| Exception handling | Include context: IDs, values, state at time of failure |

---

## Key Concepts

### Switches (Dynamic Log Levels)

- Switches control logging level and color per category pattern
- Fetched from Watch Server via HTTP, cached with version-based invalidation
- Pattern matching: longest prefix wins ("Fabrica.Data" beats "Fabrica")
- When switch version changes, cached loggers automatically re-lookup their switch

### Color (UI Visualization)

- Color comes from Switch configuration, NOT from application code
- Applied automatically to every LogEvent
- Used in Watch Server UI for visual category grouping
- Never expose color in public API - it's infrastructure

### Nesting (Method Tracing)

- `EnterMethod()` sets Nesting = 1, dispose sets Nesting = -1
- SmartInspect renders as collapsible method hierarchy
- Includes elapsed time measurement

### PayloadType (Syntax Highlighting)

- Json, Sql, Xml, Yaml, Text for UI syntax highlighting
- Use `LogJson()`, `LogSql()`, `LogXml()` etc. for explicit types
- `LogObject()` automatically uses Json type

## Extension Method Reference

### Method Tracing

```csharp
using var scope = logger.EnterMethod();
// Logs entry with Nesting=1, exit with Nesting=-1 and timing
```

### Typed Payloads

```csharp
logger.LogObject(dto);              // Serializes to JSON
logger.LogJson("Title", jsonStr);   // Raw JSON with highlighting
logger.LogSql("Query", sqlStr);     // SQL syntax highlighting
logger.LogXml("Config", xmlStr);    // XML syntax highlighting
logger.LogYaml("Data", yamlStr);    // YAML syntax highlighting
logger.LogText("Output", textStr);  // Plain text
```

## Configuration

### Development (Console)

```csharp
builder.Logging.AddWatch(w => w
    .UseConsole()
    .WhenNotMatched(Level.Debug, Color.LightGray));
```

### Production (HTTP to Watch Server)

```csharp
builder.Logging.AddWatch(w => w
    .UseHttpSink("http://watch-server:11000", "MyApp")
    .UseHttpSwitchSource("http://watch-server:11000", "MyApp"));
```

### High Performance (Zero Overhead)

```csharp
builder.Logging.AddWatch(w => w.UseQuiet());
```

## Architecture Notes

### Version-Based Switch Invalidation

- `ISwitchSource.Version` increments on every `Update()`
- `WatchLogger` caches switch + version
- On `IsEnabled()`, compares version - re-lookups only when changed
- Handles static cached loggers correctly

### Why This Matters

Developers often cache ILogger in static fields for "performance".
MS LoggerFactory also caches loggers. With version-based invalidation,
all cached loggers see switch updates without recreation.

### Channel-Based Batching

- Events queued to unbounded channel
- Background task batches by size/time
- Flushes remaining events on dispose

### Circuit Breaker (HTTP Sink)

- Opens after N consecutive failures
- Critical events (Warning/Error) buffered during outage
- Non-critical events dropped
- Exponential backoff with max delay

## Performance Guidelines

1. **Quiet Mode**: Zero allocation, near-zero CPU overhead
2. **Disabled Levels**: Only version check (long comparison)
3. **Enabled Levels**: LogEvent allocation (no pooling currently)
4. **Batching**: Events queued to channel, batched for sink
5. **Hot Path**: Avoid string interpolation before IsEnabled check

```csharp
// Good - no allocation if disabled
logger.LogInformation("User {UserId} logged in", userId);

// Bad - string allocated even if disabled
logger.LogInformation($"User {userId} logged in");
```

## Common Mistakes

- Don't set color in application code - it comes from Switch
- Don't use string interpolation: `$"User {user}"` allocates even when disabled
- Do use structured logging: `"User {UserId}", userId`
- Do use `EnterMethod()` for method-level tracing
- Do use appropriate PayloadType for syntax highlighting

## Project Structure

```
src/Fabrica.Watch/
  Level.cs                    # Trace, Debug, Info, Warning, Error, Quiet
  PayloadType.cs              # None, Json, Sql, Xml, Text, Yaml
  LogEvent.cs                 # Core event model (MemoryPackable)
  LogEventBatch.cs            # Batch container

  ISwitch.cs                  # Pattern, Tag, Level, Color
  ISwitchSource.cs            # Version, Lookup, Update
  IEventSinkProvider.cs       # Start, Stop, Accept

  WatchLogger.cs              # ILogger with version-based switch caching
  WatchLoggerProvider.cs      # ILoggerProvider with channel batching
  QuietWatchLogger.cs         # Zero-allocation no-op logger

  WatchLoggerExtensions.cs    # EnterMethod, LogObject, LogJson, etc.
  WatchLoggingBuilder.cs      # Fluent configuration builder
  LoggingBuilderExtensions.cs # AddWatch() for MS DI

  Switching/
    Switch.cs                 # ISwitch implementation
    SwitchDef.cs              # Switch definition DTO
    SwitchSource.cs           # Local switch source with Version

  Sink/
    ConsoleEventSink.cs       # Console output
    MonitorSink.cs            # Test accumulator
    LogEventBatchSerializer.cs # MemoryPack + Brotli

  Http/
    HttpEventSinkProvider.cs  # HTTP sink with circuit breaker
    HttpSwitchSource.cs       # Polls server for switch updates
    SwitchDto.cs              # Wire format for switches

  Serializers/
    IObjectSerializer.cs      # Object to payload
    JsonObjectSerializer.cs   # System.Text.Json implementation

  Utilities/
    MessageTemplate.cs        # Structured logging parser
    Ulid.cs                   # Correlation ID generation
```

## Testing

```csharp
// Use MonitorSink to capture events in tests
var sink = new MonitorSink { Accumulate = true };
var provider = new WatchLoggerProvider(switchSource, sink);
var logger = provider.CreateLogger("Test");

logger.LogInformation("Test message");
await Task.Delay(100); // Wait for batch

var events = sink.GetEvents();
events.ShouldContain(e => e.Title == "Test message");
```

## Troubleshooting

### Switch not updating
- Check `Version` increments on `Update()`
- Ensure `WhenMatched()` is called (not just `WhenNotMatched()`)

### Events not appearing
- Check sink is receiving batches (`MonitorSink.Count`)
- Check switch level allows the log level
- Ensure provider is not disposed

### Performance issues
- Use Quiet mode in production if logging not needed
- Check batching settings (BatchSize, FlushInterval)
- Avoid string interpolation in log calls
