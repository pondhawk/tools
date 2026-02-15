namespace Pondhawk.Utilities.Process
{

    /// <summary>
    /// Enumerates the inter-process signal types used by <see cref="ISignalController"/>.
    /// </summary>
    public enum SignalTypes
    {
        /// <summary>Indicates the process has started.</summary>
        Started,

        /// <summary>Indicates the process must stop.</summary>
        MustStop,

        /// <summary>Indicates the process has stopped.</summary>
        Stopped
    }


}
