namespace Pondhawk.Hosting;

internal sealed class ServiceStartDescriptor
{
    public required Type ServiceType { get; init; }
    public required Func<object, CancellationToken, Task> StartAction { get; init; }
    public required Func<object, CancellationToken, Task> StopAction { get; init; }
}
