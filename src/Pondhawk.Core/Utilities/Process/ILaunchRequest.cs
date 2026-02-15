namespace Pondhawk.Utilities.Process
{
    /// <summary>
    /// Defines the configuration for launching an external process.
    /// </summary>
    public interface ILaunchRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether standard output and error streams should be captured.
        /// </summary>
        bool CaptureOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the caller should block until the process exits.
        /// </summary>
        bool WaitForExit { get; set; }

        /// <summary>
        /// Gets or sets the working directory for the launched process.
        /// </summary>
        string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the path to the executable to launch.
        /// </summary>
        string ExecutablePath { get; set; }

        /// <summary>
        /// Gets or sets the command-line arguments to pass to the executable.
        /// </summary>
        string Arguments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the operating system shell to start the process.
        /// </summary>
        bool UseShellExecute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the process window should be visible.
        /// </summary>
        bool ShowWindow { get; set; }
    }
}
