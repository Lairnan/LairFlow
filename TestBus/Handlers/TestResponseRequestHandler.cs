using LairBus.Interfaces;
using TestBus.Model;
using TestBus.Requests;

namespace TestBus.Handlers;

public class TestResponseRequestHandler : IRequestHandler<TestResponseRequest, TestModel>
{
    public Task<TestModel> HandleRequest(TestResponseRequest request, CancellationToken cancellationToken)
    {
        var testModel = new TestModel
        {
            Id = request.Id,
            Text = request.Input
        };
        return Task.FromResult(testModel);
    }
}