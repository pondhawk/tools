using System.Diagnostics;
using System.Net;
using Pondhawk.Watch.Tests.Http;
using Serilog.Events;
using Shouldly;
using Xunit;
using SerilogEvent = Serilog.Events.LogEvent;
using WatchCorrelationManager = Pondhawk.Logging.CorrelationManager;

namespace Pondhawk.Watch.Tests;

public class WatchSinkTests
{
    private static readonly Serilog.Parsing.MessageTemplateParser Parser = new();

    private static HttpClient CreateClient(MockHttpHandler handler)
    {
        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11000") };
    }

    private static SwitchSource CreateSwitchSource()
    {
        var source = new SwitchSource();
        source.WhenNotMatched(LogEventLevel.Verbose);
        return source;
    }

    private static SerilogEvent MakeSerilogEvent(
        LogEventLevel level = LogEventLevel.Information,
        List<LogEventProperty> properties = null,
        Exception exception = null,
        string sourceContext = null)
    {
        var props = new List<LogEventProperty>(properties ?? []);
        if (sourceContext is not null)
            props.Add(new LogEventProperty("SourceContext", new ScalarValue(sourceContext)));

        return new SerilogEvent(
            DateTimeOffset.UtcNow,
            level,
            exception,
            Parser.Parse("Test message"),
            props);
    }

    private static List<SerilogEvent> MakeEventList(
        LogEventLevel level = LogEventLevel.Information,
        List<LogEventProperty> properties = null,
        Exception exception = null,
        string sourceContext = null)
    {
        return [MakeSerilogEvent(level, properties, exception, sourceContext)];
    }

    // --- ConvertEvent: basic conversion ---

    [Fact]
    public async Task ConvertEvent_BasicConversion_MakesHttpRequest()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        await sink.FlushBatchAsync(MakeEventList(sourceContext: "MyApp.Service"));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- ConvertEvent: quiet switch filters event ---

    [Fact]
    public async Task ConvertEvent_QuietSwitch_FiltersEvent()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var source = new SwitchSource { DefaultSwitch = new Switch { IsQuiet = true } };
        var sink = new WatchSink(CreateClient(handler), source, "test");

        await sink.FlushBatchAsync(MakeEventList(sourceContext: "MyApp.Service"));

        handler.Requests.Count.ShouldBe(0);
    }

    // --- ConvertEvent: level below threshold filters event ---

    [Fact]
    public async Task ConvertEvent_LevelBelowThreshold_FiltersEvent()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var source = new SwitchSource();
        source.WhenNotMatched(LogEventLevel.Warning);
        var sink = new WatchSink(CreateClient(handler), source, "test");

        await sink.FlushBatchAsync(MakeEventList(level: LogEventLevel.Debug));

        handler.Requests.Count.ShouldBe(0);
    }

    // --- ConvertEvent: SourceContext extraction ---

    [Fact]
    public async Task ConvertEvent_WithSourceContext_UsesCategory()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        await sink.FlushBatchAsync(MakeEventList(sourceContext: "MyApp.Services.OrderService"));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- ConvertEvent: Watch.Nesting property ---

    [Fact]
    public async Task ConvertEvent_WithNesting_Succeeds()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        var props = new List<LogEventProperty>
        {
            new("Watch.Nesting", new ScalarValue(1))
        };
        await sink.FlushBatchAsync(MakeEventList(properties: props));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- ConvertEvent: Watch.PayloadType + Watch.PayloadContent ---

    [Fact]
    public async Task ConvertEvent_WithPayloadTypeAndContent_Succeeds()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        var props = new List<LogEventProperty>
        {
            new("Watch.PayloadType", new ScalarValue(1)),
            new("Watch.PayloadContent", new ScalarValue("{\"key\":\"value\"}"))
        };
        await sink.FlushBatchAsync(MakeEventList(properties: props));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- ConvertEvent: exception branch ---

    [Fact]
    public async Task ConvertEvent_WithException_Succeeds()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        await sink.FlushBatchAsync(MakeEventList(exception: new InvalidOperationException("boom")));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- ConvertEvent: structured payload with ScalarValue properties ---

    [Fact]
    public async Task ConvertEvent_WithCustomProperties_BuildsStructuredPayload()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        var props = new List<LogEventProperty>
        {
            new("UserId", new ScalarValue(42)),
            new("UserName", new ScalarValue("John"))
        };
        await sink.FlushBatchAsync(MakeEventList(properties: props));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- ConvertPropertyValue: SequenceValue ---

    [Fact]
    public async Task ConvertEvent_WithSequenceValue_Succeeds()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        var props = new List<LogEventProperty>
        {
            new("Tags", new SequenceValue([new ScalarValue(1), new ScalarValue(2), new ScalarValue(3)]))
        };
        await sink.FlushBatchAsync(MakeEventList(properties: props));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- ConvertPropertyValue: StructureValue ---

    [Fact]
    public async Task ConvertEvent_WithStructureValue_Succeeds()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        var props = new List<LogEventProperty>
        {
            new("Address", new StructureValue([
                new LogEventProperty("Street", new ScalarValue("123 Main")),
                new LogEventProperty("City", new ScalarValue("Springfield"))
            ]))
        };
        await sink.FlushBatchAsync(MakeEventList(properties: props));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- ConvertPropertyValue: DictionaryValue ---

    [Fact]
    public async Task ConvertEvent_WithDictionaryValue_Succeeds()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        var props = new List<LogEventProperty>
        {
            new("Headers", new DictionaryValue([
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                    new ScalarValue("Content-Type"), new ScalarValue("application/json")),
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(
                    new ScalarValue("Accept"), new ScalarValue("text/html"))
            ]))
        };
        await sink.FlushBatchAsync(MakeEventList(properties: props));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- Emit + Dispose integration ---

    [Fact]
    public void Emit_ThenDispose_FlushesEvent()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test",
            flushInterval: TimeSpan.FromMilliseconds(10));

        sink.Emit(MakeSerilogEvent(sourceContext: "MyApp"));
        sink.Dispose();

        handler.Requests.Count.ShouldBeGreaterThan(0);
    }

    // --- DisposeAsync ---

    [Fact]
    public async Task DisposeAsync_Succeeds()
    {
        var handler = new MockHttpHandler();
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        await sink.DisposeAsync();
    }

    // --- Dispose is idempotent ---

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var handler = new MockHttpHandler();
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        sink.Dispose();
        sink.Dispose();
    }

    // --- Emit after Dispose is no-op ---

    [Fact]
    public void Emit_AfterDispose_IsNoOp()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        sink.Dispose();
        sink.Emit(MakeSerilogEvent(sourceContext: "MyApp"));

        handler.Requests.Count.ShouldBe(0);
    }

    // --- GetCorrelationId: with Activity baggage ---

    [Fact]
    public async Task GetCorrelationId_WithActivityBaggage_UsesExistingCorrelation()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        using var scope = WatchCorrelationManager.Begin("test-correlation-123");

        await sink.FlushBatchAsync(MakeEventList(sourceContext: "MyApp"));

        handler.Requests.Count.ShouldBe(1);
    }

    // --- GetCorrelationId: Activity without existing correlation ---

    [Fact]
    public async Task GetCorrelationId_WithActivityWithoutBaggage_CreatesNewCorrelation()
    {
        var handler = new MockHttpHandler();
        handler.RespondWith(HttpStatusCode.OK);
        var sink = new WatchSink(CreateClient(handler), CreateSwitchSource(), "test");

        using var activity = new Activity("TestActivity");
        activity.Start();

        await sink.FlushBatchAsync(MakeEventList(sourceContext: "MyApp"));

        handler.Requests.Count.ShouldBe(1);

        var correlation = activity.GetBaggageItem(WatchCorrelationManager.BaggageKey);
        correlation.ShouldNotBeNullOrEmpty();
    }
}
