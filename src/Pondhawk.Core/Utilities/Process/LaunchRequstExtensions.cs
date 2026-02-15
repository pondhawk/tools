using System.Diagnostics;
using System.Text;

namespace Pondhawk.Utilities.Process;

/// <summary>
/// Extension methods for launching external processes from <c>ILaunchRequest</c> instances.
/// </summary>
public static class LaunchRequstExtensions
{

    /// <summary>
    /// Launches an external process using the configuration from this <see cref="ILaunchRequest"/> and returns the result.
    /// </summary>
    /// <param name="request">The launch request containing the process configuration.</param>
    /// <returns>A <see cref="LaunchResult"/> containing the process output, error, and exit information.</returns>
    public static LaunchResult Run(this ILaunchRequest request)
    {

        var capture = request.CaptureOutput && !request.UseShellExecute;
        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            Arguments = request.Arguments,
            WorkingDirectory = request.WorkingDirectory,
            UseShellExecute = request.UseShellExecute,
            CreateNoWindow = !request.ShowWindow,
            RedirectStandardOutput = capture,
            RedirectStandardError = capture
        };

        var process = new System.Diagnostics.Process
        {
            EnableRaisingEvents = capture,
            StartInfo = startInfo
        };

        var result = new LaunchResult(process);
        var output = new StringBuilder();
        var error = new StringBuilder();

        if (capture)
        {
            process.OutputDataReceived += (s, e) => output.AppendLine(e.Data);
            process.ErrorDataReceived += (s, e) => error.AppendLine(e.Data);
        }

        process.Start();

        if (capture)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        if (request.WaitForExit)
        {
            process.WaitForExit();
        }

        result.Output = output.ToString();
        result.Error = error.ToString();

        return result;

    }

}
