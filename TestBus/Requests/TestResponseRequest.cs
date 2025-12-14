using LairBus.Interfaces;
using TestBus.Model;

namespace TestBus.Requests;

public class TestResponseRequest : IRequest<TestModel>
{
    public long Id { get; set; }
    public string Input { get; set; }
}