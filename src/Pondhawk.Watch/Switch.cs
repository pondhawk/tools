/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.
Copyright (c) 2025 Pond Hawk Technologies Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Drawing;
using Serilog.Events;

namespace Pondhawk.Watch;

/// <summary>
/// Default implementation of ISwitch with fluent configuration API.
/// </summary>
/// <remarks>
/// Switch instances are immutable after construction via the fluent API.
/// They are safe to cache and share across threads.
/// </remarks>
public class Switch
{
    /// <summary>
    /// Creates a new Switch instance with default values.
    /// </summary>
    /// <returns>A new Switch for fluent configuration.</returns>
    public static Switch Create()
    {
        return new Switch();
    }

    /// <summary>
    /// Gets or sets the pattern to match against logger categories.
    /// </summary>
    public string Pattern { get; set; } = "";

    /// <summary>
    /// Gets or sets an optional tag for categorization.
    /// </summary>
    public string Tag { get; set; } = "";

    /// <summary>
    /// Gets or sets the minimum log level.
    /// </summary>
    public LogEventLevel Level { get; set; } = LogEventLevel.Error;

    /// <summary>
    /// Gets or sets whether this switch suppresses all logging.
    /// </summary>
    public bool IsQuiet { get; set; }

    /// <summary>
    /// Gets or sets the color for log events.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Sets the pattern for this switch.
    /// </summary>
    /// <param name="pattern">The pattern to match.</param>
    /// <returns>This switch for fluent chaining.</returns>
    public Switch WhenMatched(string pattern)
    {
        Pattern = pattern;
        return this;
    }

    /// <summary>
    /// Sets the log level for this switch.
    /// </summary>
    /// <param name="level">The minimum log level.</param>
    /// <returns>This switch for fluent chaining.</returns>
    public Switch UseLevel(LogEventLevel level)
    {
        Level = level;
        return this;
    }

    /// <summary>
    /// Sets the color for this switch.
    /// </summary>
    /// <param name="color">The color for log events.</param>
    /// <returns>This switch for fluent chaining.</returns>
    public Switch UseColor(Color color)
    {
        Color = color;
        return this;
    }

    /// <summary>
    /// Sets the tag for this switch.
    /// </summary>
    /// <param name="tag">The tag value.</param>
    /// <returns>This switch for fluent chaining.</returns>
    public Switch UseTag(string tag)
    {
        Tag = tag;
        return this;
    }
}
