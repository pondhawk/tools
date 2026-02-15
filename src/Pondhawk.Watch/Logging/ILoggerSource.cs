using System.Runtime.CompilerServices;
using Serilog;

namespace Pondhawk.Watch;

/// <summary>
/// Abstraction for creating Serilog loggers and method tracing scopes from a source object or category.
/// </summary>
public interface ILoggerSource
{
    /// <summary>
    /// Creates a Serilog <see cref="ILogger"/> with SourceContext set to the source object's type name.
    /// </summary>
    /// <param name="source">The object whose type name becomes the SourceContext.</param>
    /// <returns>A configured Serilog logger.</returns>
    ILogger GetLogger(object source);

    /// <summary>
    /// Creates a Serilog <see cref="ILogger"/> with the specified category as SourceContext.
    /// </summary>
    /// <param name="category">The source context category string.</param>
    /// <returns>A configured Serilog logger.</returns>
    ILogger GetLogger(string category);

#if NET7_0_OR_GREATER
    /// <summary>
    /// Creates a disposable <see cref="MethodLogger"/> that logs method entry and exit with elapsed time.
    /// </summary>
    /// <param name="source">The object whose type provides the logger context.</param>
    /// <param name="method">The calling method name (auto-populated by compiler).</param>
    /// <param name="file">The calling file path (auto-populated by compiler).</param>
    /// <returns>A disposable <see cref="MethodLogger"/> that logs exit on dispose.</returns>
    MethodLogger EnterMethod(
        object source,
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "");
#endif
}
