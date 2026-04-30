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

    public async Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null) throw new NullReferenceException($"Handler not found for {handlerType.Name}");

        var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.HandleRequest));
        return await (Task<TResponse>)method!.Invoke(handler, [request, cancellationToken])!;
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
        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        foreach (var handler in handlers)
            await handler.HandleNotification(notification, cancellationToken);
    }
}