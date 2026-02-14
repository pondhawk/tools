namespace Pondhawk.Hosting;

internal sealed class ServiceStartDescriptor
{
    public Type ServiceType { get; init; }
    public Func<object, CancellationToken, Task> StartAction { get; init; }
    public Func<object, CancellationToken, Task> StopAction { get; init; }
}
