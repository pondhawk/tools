using System.Runtime.CompilerServices;
using Serilog;

namespace Pondhawk.Logging;

public interface ILoggerSource
{
    ILogger GetLogger(object source);

    ILogger GetLogger(string category);

    MethodLogger EnterMethod(
        object source,
        [CallerMemberName] string method = "",
        [CallerFilePath] string file = "");
}
