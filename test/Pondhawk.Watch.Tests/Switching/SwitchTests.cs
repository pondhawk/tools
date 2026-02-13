using System.Drawing;
using Pondhawk.Watch.Switching;
using Serilog.Events;
using Shouldly;
using Xunit;

namespace Pondhawk.Watch.Tests.Switching;

public class SwitchTests
{

    [Fact]
    public void Create_ReturnsNewInstanceWithDefaults()
    {
        var sw = Switch.Create();

        sw.Pattern.ShouldBe("");
        sw.Level.ShouldBe(LogEventLevel.Error);
        sw.Color.ShouldBe(Color.White);
        sw.Tag.ShouldBe("");
        sw.IsQuiet.ShouldBeFalse();
    }

    [Fact]
    public void WhenMatched_SetsPattern()
    {
        var sw = Switch.Create().WhenMatched("MyApp.Services");

        sw.Pattern.ShouldBe("MyApp.Services");
    }

    [Fact]
    public void UseLevel_SetsLevel()
    {
        var sw = Switch.Create().UseLevel(LogEventLevel.Debug);

        sw.Level.ShouldBe(LogEventLevel.Debug);
    }

    [Fact]
    public void UseColor_SetsColor()
    {
        var sw = Switch.Create().UseColor(Color.Red);

        sw.Color.ShouldBe(Color.Red);
    }

    [Fact]
    public void UseTag_SetsTag()
    {
        var sw = Switch.Create().UseTag("Infrastructure");

        sw.Tag.ShouldBe("Infrastructure");
    }

    [Fact]
    public void FluentChain_SetsAllProperties()
    {
        var sw = Switch.Create()
            .WhenMatched("MyApp")
            .UseLevel(LogEventLevel.Warning)
            .UseColor(Color.Blue)
            .UseTag("Test");

        sw.Pattern.ShouldBe("MyApp");
        sw.Level.ShouldBe(LogEventLevel.Warning);
        sw.Color.ShouldBe(Color.Blue);
        sw.Tag.ShouldBe("Test");
    }

    [Fact]
    public void FluentMethods_ReturnSameInstance()
    {
        var sw = Switch.Create();

        var r1 = sw.WhenMatched("p");
        var r2 = r1.UseLevel(LogEventLevel.Debug);
        var r3 = r2.UseColor(Color.Green);
        var r4 = r3.UseTag("t");

        r1.ShouldBeSameAs(sw);
        r2.ShouldBeSameAs(sw);
        r3.ShouldBeSameAs(sw);
        r4.ShouldBeSameAs(sw);
    }

}
