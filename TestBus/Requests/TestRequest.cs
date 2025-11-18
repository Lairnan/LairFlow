using LairBus.Interfaces;

namespace TestBus.Requests;

public class TestRequest : IRequest
{
    public int Id { get; set; }
}