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
    
    public async Task SendRequest<T>(T request, CancellationToken cancellationToken = default)
        where T : IRequest
    {
        var handler = _serviceProvider.GetService<IRequestHandler<T>>();
        if (handler == null) throw new NullReferenceException(nameof(IRequestHandler<IRequest>));
        await handler.HandleRequest(request, cancellationToken);
    }

    public async Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        var handler = _serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>();
        if (handler == null) throw new NullReferenceException(nameof(IRequestHandler<IRequest<TRequest>, TRequest>));
        return await handler.HandleRequest(request, cancellationToken);
    }

    public async Task SendNotification(INotification notification, CancellationToken cancellationToken = default)
    {
        var handler = _serviceProvider.GetService<INotificationHandler<INotification>>();
        if (handler == null) throw new NullReferenceException(nameof(INotificationHandler<INotification>));
        await handler.HandleNotification(notification, cancellationToken);
    }
}