namespace Pondhawk.Utilities.Process
{



    /// <summary>
    /// Default implementation of <see cref="ILaunchRequest"/> for configuring and launching external processes.
    /// </summary>
    public class LaunchRequest : ILaunchRequest
    {

        /// <inheritdoc />
        public bool CaptureOutput { get; set; }

        /// <inheritdoc />
        public bool WaitForExit { get; set; }

        /// <inheritdoc />
        public string WorkingDirectory { get; set; } = "";

        /// <inheritdoc />
        public string ExecutablePath { get; set; } = "";

        /// <inheritdoc />
        public string Arguments { get; set; } = "";

        /// <inheritdoc />
        public bool UseShellExecute { get; set; }

        /// <inheritdoc />
        public bool ShowWindow { get; set; }


        /// <summary>
        /// Sets the working directory using a format string and arguments.
        /// </summary>
        /// <param name="template">The format string for the working directory path.</param>
        /// <param name="args">The format arguments.</param>
        public void SetWorkingDirectory(string template, params object[] args)
        {

            var working = string.Format(System.Globalization.CultureInfo.InvariantCulture, template, args);

            WorkingDirectory = working;

        }

        /// <summary>
        /// Sets the executable path, shell execution, and window visibility using a format string and arguments.
        /// </summary>
        /// <param name="useShell">Whether to use the operating system shell to start the process.</param>
        /// <param name="showWindow">Whether the process window should be visible.</param>
        /// <param name="template">The format string for the executable path.</param>
        /// <param name="args">The format arguments.</param>
        public void SetExecutable(bool useShell, bool showWindow, string template, params object[] args)
        {

            var executable = string.Format(System.Globalization.CultureInfo.InvariantCulture, template, args);

            ExecutablePath = executable;
            UseShellExecute = useShell;
            ShowWindow = showWindow;

        }

        /// <summary>
        /// Sets the command-line arguments using a format string and arguments.
        /// </summary>
        /// <param name="template">The format string for the arguments.</param>
        /// <param name="args">The format arguments.</param>
        public void SetArguments(string template, params object[] args)
        {

            var arguments = string.Format(System.Globalization.CultureInfo.InvariantCulture, template, args);

            Arguments = arguments;

        }


    }



}
