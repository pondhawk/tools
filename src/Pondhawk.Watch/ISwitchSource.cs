/*
The MIT License (MIT)

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

using Pondhawk.Watch.Switching;

namespace Pondhawk.Watch;

/// <summary>
/// Provides switch lookup and management with version-based cache invalidation.
/// </summary>
/// <remarks>
/// <para>
/// The Version property enables efficient caching in loggers. Loggers cache their
/// ISwitch reference along with the version number. On each IsEnabled check, they
/// compare the cached version with the current Version - only re-looking up when
/// the version has changed. This allows switch updates to propagate even to statically
/// cached loggers.
/// </para>
/// <para>
/// Thread-safety: All methods must be safe for concurrent access. Update() should
/// increment Version after completing the update to ensure atomic visibility.
/// </para>
/// </remarks>
public interface ISwitchSource
{
    /// <summary>
    /// Gets the current version number. Incremented after each Update() call.
    /// Used by loggers for cache invalidation.
    /// </summary>
    /// <remarks>
    /// Version is incremented atomically after Update() completes, ensuring
    /// loggers see a consistent set of switches when they re-lookup.
    /// </remarks>
    long Version { get; }

    /// <summary>
    /// Starts the switch source. For HTTP sources, this may begin polling for updates.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// Stops the switch source and releases any resources.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopAsync(CancellationToken ct = default);

    /// <summary>
    /// Looks up the switch for a given category using longest-prefix matching.
    /// </summary>
    /// <param name="category">The logger category (typically a fully-qualified type name).</param>
    /// <returns>The matching switch, or DefaultSwitch if no pattern matches.</returns>
    /// <exception cref="ArgumentException">Thrown when category is null or whitespace.</exception>
    ISwitch Lookup(string category);

    /// <summary>
    /// Updates the switch configuration. Increments Version after the update completes.
    /// </summary>
    /// <param name="switches">The new switch definitions to apply.</param>
    /// <remarks>
    /// Switches are sorted by pattern length (longest first) for prefix matching.
    /// Version is incremented after the update is complete to ensure atomic visibility.
    /// </remarks>
    void Update(IEnumerable<SwitchDef> switches);

    /// <summary>
    /// Asynchronously refreshes switches from the underlying source (e.g., HTTP endpoint).
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the default switch used when no pattern matches a category.
    /// </summary>
    ISwitch DefaultSwitch { get; }
}
