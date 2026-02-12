# Fabrica.Watch

A pure Microsoft.Extensions.Logging provider for structured logging with rich UI visualization support.

## Installation

```bash
dotnet add package Fabrica.Watch
```

## Quick Start

### Basic Console Logging

```csharp
using Fabrica.Watch;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddWatch(w => w
    .UseConsole()
    .WhenNotMatched(Level.Debug, System.Drawing.Color.LightGray));

var app = builder.Build();
```

### Production (Watch Server)

```csharp
builder.Logging.AddWatch(w => w
    .UseHttpSink("http://watch-server:11000", "MyApp")
    .UseHttpSwitchSource("http://watch-server:11000", "MyApp"));
```

### High Performance (Quiet Mode)

```csharp
builder.Logging.AddWatch(w => w.UseQuiet());
```

## Usage

### Standard Logging

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoWork()
    {
        _logger.LogInformation("Processing started");
        _logger.LogWarning("Resource usage high: {Usage}%", 85);
    }
}
```

### Method Tracing

```csharp
public async Task ProcessOrder(int orderId)
{
    using var scope = _logger.EnterMethod();

    _logger.LogInformation("Processing order {OrderId}", orderId);
    // ... method body ...
}
// Automatically logs entry/exit with timing
```

### Object Serialization

```csharp
_logger.LogObject(myDto);                      // Serialize to JSON
_logger.LogObject("Order Details", order);     // With custom title
```

### Typed Payloads (Syntax Highlighting)

```csharp
_logger.LogJson("API Response", jsonString);
_logger.LogSql("Query", sqlString);
_logger.LogXml("Configuration", xmlString);
_logger.LogYaml("Settings", yamlString);
_logger.LogText("Output", textString);
```

## Configuration Options

### Builder Methods

| Method | Description |
|--------|-------------|
| `UseQuiet()` | Zero overhead, no logging |
| `UseConsole()` | Console output |
| `UseHttpSink(url, domain)` | Send to Watch Server |
| `UseHttpSwitchSource(url, domain)` | Fetch switches from server |
| `UseLocalSwitchSource(config)` | Local switch configuration |
| `WithDomain(name)` | Set batch domain name |
| `WithBatchSize(size)` | Events per batch |
| `WithFlushInterval(span)` | Max time before flush |
| `WhenNotMatched(level, color)` | Default switch |
| `WhenMatched(pattern, level, color)` | Pattern-specific switch |

## Features

- **Version-based switch invalidation**: Cached loggers automatically update when switches change
- **Channel-based batching**: Efficient async batching with configurable size/timing
- **Circuit breaker**: HTTP sink gracefully handles server outages
- **Critical event buffering**: Warning/Error events preserved during outages
- **Zero-allocation quiet mode**: For high-performance production scenarios
- **Rich payload types**: JSON, SQL, XML, YAML syntax highlighting in UI
- **Method tracing**: Automatic entry/exit logging with timing

## Documentation

See [CLAUDE.md](CLAUDE.md) for detailed documentation and AI development guidance.

## License

MIT License - See LICENSE file for details.
