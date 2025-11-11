using LairBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LairBus.Implementations;

internal class Bus : IBus
{
    private readonly IServiceProvider _serviceProvider;

    public Bus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task SendRequest(IRequest request, CancellationToken cancellationToken = default)
    {
        var handler = _serviceProvider.GetService<IRequestHandler<IRequest>>();
        if (handler == null) throw new NullReferenceException(nameof(IRequestHandler<IRequest>));
        await handler.HandleRequest(request, cancellationToken);
    }

    public async Task<T> SendRequest<T>(IRequest<T> request, CancellationToken cancellationToken = default)
    {
        var handler = _serviceProvider.GetService<IRequestHandler<IRequest<T>, T>>();
        if (handler == null) throw new NullReferenceException(nameof(IRequestHandler<IRequest<T>, T>));
        return await handler.HandleRequest(request, cancellationToken);
    }

    public async Task SendNotification(INotification notification, CancellationToken cancellationToken = default)
    {
        var handler = _serviceProvider.GetService<INotificationHandler<INotification>>();
        if (handler == null) throw new NullReferenceException(nameof(INotificationHandler<INotification>));
        await handler.HandleNotification(notification, cancellationToken);
    }
}