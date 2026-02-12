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
/// Represents a logging switch that controls log level, color, and tag for a category pattern.
/// Switches are matched against logger categories to determine logging behavior.
/// </summary>
/// <remarks>
/// <para>
/// Switches are immutable after creation and safe to cache by loggers.
/// When switches are updated in the ISwitchSource, the source's Version is incremented,
/// signaling loggers to re-lookup their switch.
/// </para>
/// <para>
/// Thread-safety: ISwitch implementations must be thread-safe for reading.
/// All properties should be immutable once the switch is created.
/// </para>
/// </remarks>
public interface ISwitch
{
    /// <summary>
    /// Gets the pattern used to match this switch against logger categories.
    /// Uses prefix matching (longest match wins).
    /// </summary>
    /// <example>
    /// Pattern "Fabrica.Data" matches categories "Fabrica.Data", "Fabrica.Data.Sql", etc.
    /// </example>
    string Pattern { get; }

    /// <summary>
    /// Gets an optional tag for additional categorization.
    /// Can be used for filtering in UI or analysis.
    /// </summary>
    string Tag { get; }

    /// <summary>
    /// Gets the minimum log level for this switch.
    /// Log events below this level are discarded.
    /// </summary>
    LogEventLevel Level { get; }

    /// <summary>
    /// Gets whether this switch suppresses all logging.
    /// When true, no events are emitted regardless of level.
    /// </summary>
    bool IsQuiet { get; }

    /// <summary>
    /// Gets the color for log events matching this switch.
    /// Used for visual grouping in UI viewers.
    /// </summary>
    /// <remarks>
    /// Color is infrastructure-level configuration, not exposed in application API.
    /// It flows automatically from switch to log events.
    /// </remarks>
    Color Color { get; }
}
