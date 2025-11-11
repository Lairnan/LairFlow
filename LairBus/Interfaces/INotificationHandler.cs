namespace LairBus.Interfaces;

public interface INotificationHandler<in T> where T : INotification
{
    Task HandleNotification(T notification, CancellationToken cancellationToken = default);
}