namespace Pondhawk.Utilities.Process
{


    /// <summary>
    /// Defines inter-process signaling for started, stop-requested, and stopped lifecycle states.
    /// </summary>
    public interface ISignalController
    {


        void Started();
        bool HasStarted { get; }
        bool WaitForStarted( TimeSpan interval );

        void RequestStop();
        bool MustStop { get; }
        bool WaitForMustStop( TimeSpan interval );

        void Stopped();
        bool HasStopped { get; }
        bool WaitForStopped( TimeSpan interval );

        void Reset();


    }


}
