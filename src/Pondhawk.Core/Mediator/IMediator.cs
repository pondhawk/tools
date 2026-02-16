namespace Pondhawk.Mediator;

/// <summary>
/// Mediator interface for sending requests through the pipeline to handlers.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request through the pipeline to its handler.
    /// </summary>
    Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
