namespace Pondhawk.Utilities.Process
{


    /// <summary>
    /// Defines inter-process signaling for started, stop-requested, and stopped lifecycle states.
    /// </summary>
    public interface ISignalController
    {


        /// <summary>
        /// Signals that the process has started.
        /// </summary>
        void Started();

        /// <summary>
        /// Gets a value indicating whether the started signal has been raised.
        /// </summary>
        bool HasStarted { get; }

        /// <summary>
        /// Blocks the calling thread until the started signal is raised or the specified interval elapses.
        /// </summary>
        /// <param name="interval">The maximum time to wait.</param>
        /// <returns><c>true</c> if the started signal was received within the interval; otherwise, <c>false</c>.</returns>
        bool WaitForStarted(TimeSpan interval);

        /// <summary>
        /// Signals that the process should stop.
        /// </summary>
        void RequestStop();

        /// <summary>
        /// Gets a value indicating whether a stop has been requested.
        /// </summary>
        bool MustStop { get; }

        /// <summary>
        /// Blocks the calling thread until a stop request is received or the specified interval elapses.
        /// </summary>
        /// <param name="interval">The maximum time to wait.</param>
        /// <returns><c>true</c> if the stop request was received within the interval; otherwise, <c>false</c>.</returns>
        bool WaitForMustStop(TimeSpan interval);

        /// <summary>
        /// Signals that the process has stopped.
        /// </summary>
        void Stopped();

        /// <summary>
        /// Gets a value indicating whether the process has stopped.
        /// </summary>
        bool HasStopped { get; }

        /// <summary>
        /// Blocks the calling thread until the stopped signal is raised or the specified interval elapses.
        /// </summary>
        /// <param name="interval">The maximum time to wait.</param>
        /// <returns><c>true</c> if the stopped signal was received within the interval; otherwise, <c>false</c>.</returns>
        bool WaitForStopped(TimeSpan interval);

        /// <summary>
        /// Resets all signal states back to their initial (unset) condition.
        /// </summary>
        void Reset();


    }


}
