namespace LairBus.Interfaces;

public interface IBus
{
    Task SendRequest(IRequest request, CancellationToken cancellationToken = default);
    Task<T> SendRequest<T>(IRequest<T> request, CancellationToken cancellationToken = default);
    
    Task SendNotification(INotification notification, CancellationToken cancellationToken = default);
}