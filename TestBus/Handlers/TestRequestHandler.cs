using LairBus.Interfaces;
using TestBus.Requests;

namespace TestBus.Handlers;

public class TestRequestHandler : IRequestHandler<TestRequest>
{
    public async Task HandleRequest(TestRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Executing test request: {request.Id}");
    }
}