namespace Pondhawk.Utilities.Process
{


    /// <summary>
    /// Contains the result of a launched external process, including captured output, error, exit state, and exit code.
    /// </summary>
    public class LaunchResult
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchResult"/> class wrapping the specified process.
        /// </summary>
        /// <param name="process">The launched <see cref="System.Diagnostics.Process"/> instance.</param>
        public LaunchResult(System.Diagnostics.Process process)
        {
            TheProcess = process;
        }

        private System.Diagnostics.Process TheProcess { get; }

        /// <summary>
        /// Gets or sets the captured standard output from the process.
        /// </summary>
        public string Output { get; set; } = "";

        /// <summary>
        /// Gets or sets the captured standard error from the process.
        /// </summary>
        public string Error { get; set; } = "";

        /// <summary>
        /// Gets a value indicating whether the process has exited.
        /// </summary>
        public bool Exited => TheProcess?.HasExited ?? true;

        /// <summary>
        /// Gets the exit code of the process, or 0 if the process is unavailable.
        /// </summary>
        public int ExitCode => TheProcess?.ExitCode ?? 0;


    }


}
