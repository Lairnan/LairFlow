namespace LairBus.Interfaces;

public interface IBus
{
    Task SendRequest<T>(T request, CancellationToken cancellationToken = default)
        where T : IRequest;
    Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;
    
    Task SendNotification(INotification notification, CancellationToken cancellationToken = default);
}