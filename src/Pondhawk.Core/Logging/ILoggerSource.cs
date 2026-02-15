using System.Runtime.CompilerServices;
using Serilog;

namespace Pondhawk.Logging;

/// <summary>
/// Abstraction for creating Serilog loggers and method tracing scopes from a source object or category.
/// </summary>
public interface ILoggerSource
{
    ILogger GetLogger(object source);

    ILogger GetLogger(string category);

    MethodLogger EnterMethod(
        object source,
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "");
}
