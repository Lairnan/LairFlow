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
    
    public async Task SendRequest<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        var handler = _serviceProvider.GetService<IRequestHandler<TRequest>>();
        if (handler == null) throw new NullReferenceException(nameof(IRequestHandler<TRequest>));
        await handler.HandleRequest(request, cancellationToken);
    }

    public async Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        var handler = _serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>();
        if (handler == null) throw new NullReferenceException(nameof(IRequestHandler<TRequest, TResponse>));
        return await handler.HandleRequest(request, cancellationToken);
    }

    public async Task SendNotification<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var handler = _serviceProvider.GetService<INotificationHandler<TNotification>>();
        if (handler == null) throw new NullReferenceException(nameof(INotificationHandler<TNotification>));
        await handler.HandleNotification(notification, cancellationToken);
    }
}