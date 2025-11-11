namespace LairBus.Interfaces;

public interface IRequestHandler<in T> where T : IRequest
{
    Task HandleRequest(T request, CancellationToken cancellationToken);
}

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleRequest(TRequest request, CancellationToken cancellationToken);
}