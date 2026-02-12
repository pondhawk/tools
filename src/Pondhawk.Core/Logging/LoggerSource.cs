using System.Runtime.CompilerServices;
using Pondhawk.Logging.Utilities;
using Serilog;
using Serilog.Core;

namespace Pondhawk.Logging;

public class LoggerSource : ILoggerSource
{
    public ILogger GetLogger(object source)
    {
        var category = source.GetType().GetConciseFullName();
        return Log.ForContext(Constants.SourceContextPropertyName, category);
    }

    public ILogger GetLogger(string category)
    {
        return Log.ForContext(Constants.SourceContextPropertyName, category);
    }

    public MethodLogger EnterMethod(
        object source,
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "")
    {
        return GetLogger(source).EnterMethod(method, file);
    }
}
