namespace LairBus.Interfaces;

public interface IBus
{
    Task SendRequest<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;
    Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;
    
    Task SendNotification<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}