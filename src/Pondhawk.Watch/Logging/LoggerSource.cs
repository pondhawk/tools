using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Core;

namespace Pondhawk.Watch;

/// <summary>
/// Default <see cref="ILoggerSource"/> that creates Serilog loggers using the type's concise full name as the source context.
/// </summary>
public class LoggerSource : ILoggerSource
{
    /// <inheritdoc />
    public ILogger GetLogger(object source)
    {
        var category = source.GetType().GetConciseFullName();
        return Log.ForContext(Constants.SourceContextPropertyName, category);
    }

    /// <inheritdoc />
    public ILogger GetLogger(string category)
    {
        return Log.ForContext(Constants.SourceContextPropertyName, category);
    }

#if NET7_0_OR_GREATER
    /// <inheritdoc />
    public MethodLogger EnterMethod(
        object source,
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
    {
        return GetLogger(source).EnterMethod(method, file);
    }
#endif
}
