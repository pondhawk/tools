namespace Pondhawk.Utilities.Process;

/// <summary>
/// File-based inter-process signaling controller that uses flag files to coordinate started/stop/stopped state.
/// </summary>
public class FileSignalController : ISignalController, IDisposable
{

    /// <summary>
    /// Specifies whether the signal controller is owned by the host or the appliance process.
    /// </summary>
    public enum OwnerType
    {
        /// <summary>The signal controller is owned by the host process.</summary>
        Host,

        /// <summary>The signal controller is owned by the appliance (child) process.</summary>
        Appliance
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSignalController"/> class.
    /// </summary>
    /// <param name="owner">Specifies whether this controller is used by the host or appliance process.</param>
    /// <param name="path">The directory path for signal flag files. If empty, uses the application base directory.</param>
    public FileSignalController(OwnerType owner, string path = "")
    {

        Owner = owner;

        if (string.IsNullOrWhiteSpace(path))
        {
            var entry = AppDomain.CurrentDomain.BaseDirectory;
            var fi = new FileInfo(entry);
            path = fi.DirectoryName ?? Path.GetTempPath();
        }


        InstallationRoot = path;

        StartedFlag = Path.Combine(path, "started.flag");
        StartedEvent = new ManualResetEvent(false);

        MustStopFlag = Path.Combine(path, "muststop.flag");
        MustStopEvent = new ManualResetEvent(false);

        StoppedFlag = Path.Combine(path, "stopped.flag");
        StoppedEvent = new ManualResetEvent(false);

        EndWatchEvent = new ManualResetEvent(false);


    }

    private OwnerType Owner { get; }

    private string InstallationRoot { get; }


    private string StartedFlag { get; }
    private ManualResetEvent StartedEvent { get; }
    /// <inheritdoc />
    public bool WaitForStarted(TimeSpan interval) => StartedEvent.WaitOne(interval);

    private string MustStopFlag { get; }
    private ManualResetEvent MustStopEvent { get; }
    /// <inheritdoc />
    public bool WaitForMustStop(TimeSpan interval) => MustStopEvent.WaitOne(interval);

    private string StoppedFlag { get; }
    private ManualResetEvent StoppedEvent { get; }
    /// <inheritdoc />
    public bool WaitForStopped(TimeSpan interval) => StoppedEvent.WaitOne(interval);

    private ManualResetEvent EndWatchEvent { get; }

    private Task _watchTask = Task.CompletedTask;

    /// <summary>
    /// Creates a signal flag file on disk for the specified signal type.
    /// </summary>
    /// <param name="type">The signal type to create.</param>
    protected virtual void CreateSignal(SignalTypes type)
    {

        switch (type)
        {
            case SignalTypes.Started:
                Create(StartedFlag);
                break;
            case SignalTypes.MustStop:
                Create(MustStopFlag);
                break;
            case SignalTypes.Stopped:
                Create(StoppedFlag);
                break;
        }

        void Create(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("ok");
            }
        }


    }


    /// <inheritdoc />
    public void Started()
    {
        CreateSignal(SignalTypes.Started);
        StartedEvent.Set();
    }

    /// <inheritdoc />
    public void RequestStop()
    {
        CreateSignal(SignalTypes.MustStop);
    }

    /// <inheritdoc />
    public void Stopped()
    {
        CreateSignal(SignalTypes.Stopped);
        StoppedEvent.Set();
    }

    /// <inheritdoc />
    public void Reset()
    {

        foreach (var file in Directory.EnumerateFiles(InstallationRoot, "*.flag"))
            File.Delete(file);

        StartedEvent.Reset();
        MustStopEvent.Reset();
        StoppedEvent.Reset();

    }

    private void WatchHost()
    {

        while (true)
        {

            if (!StartedEvent.WaitOne(0) && CheckSignal(SignalTypes.Started))
                StartedEvent.Set();

            if (!StoppedEvent.WaitOne(0) && CheckSignal(SignalTypes.Stopped))
            {
                StoppedEvent.Set();
                break;
            }

            if (EndWatchEvent.WaitOne(TimeSpan.FromMilliseconds(500)))
                break;

        }


    }

    private async Task WatchAppliance()
    {

        while (true)
        {

            if (!MustStopEvent.WaitOne(0) && CheckSignal(SignalTypes.MustStop))
            {
                MustStopEvent.Set();
                break;
            }

            if (EndWatchEvent.WaitOne(0))
                break;

            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        }


    }

    /// <summary>
    /// Checks whether a signal flag file exists on disk for the specified signal type.
    /// </summary>
    /// <param name="type">The signal type to check.</param>
    /// <returns><c>true</c> if the signal flag file exists; otherwise, <c>false</c>.</returns>
    protected virtual bool CheckSignal(SignalTypes type)
    {


        string? path = null;
        switch (type)
        {
            case SignalTypes.Started:
                path = StartedFlag;
                break;
            case SignalTypes.MustStop:
                path = MustStopFlag;
                break;
            case SignalTypes.Stopped:
                path = StoppedFlag;
                break;
        }

        if (path is null)
            return false;

        var exists = File.Exists(path);

        return exists;

    }

    /// <inheritdoc />
    public bool HasStarted => CheckSignal(SignalTypes.Started);

    /// <inheritdoc />
    public bool MustStop => CheckSignal(SignalTypes.MustStop);

    /// <inheritdoc />
    public bool HasStopped => !HasStarted || CheckSignal(SignalTypes.Stopped);


    /// <summary>
    /// Starts the background file-watching task that monitors for signal flag files based on the owner type.
    /// </summary>
    /// <returns>A completed task once the watcher has been started.</returns>
    public Task StartAsync()
    {
        _watchTask = Owner == OwnerType.Host
            ? Task.Run(WatchHost)
            : Task.Run(WatchAppliance);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the background watcher and releases all managed resources.
    /// </summary>
    public void Dispose()
    {
        EndWatchEvent.Set();
        _watchTask.Wait(TimeSpan.FromSeconds(5));

        StartedEvent.Dispose();
        MustStopEvent.Dispose();
        StoppedEvent.Dispose();
        EndWatchEvent.Dispose();

        GC.SuppressFinalize(this);
    }

}
