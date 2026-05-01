using LairBus;
using LairBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TestBus.Model;
using TestBus.Requests;

var serviceCollection = new ServiceCollection();
serviceCollection.AddBus(config =>
{
    config.RegisterServicesFromAssemblyContaining<Program>();
});
var serviceProvider = serviceCollection.BuildServiceProvider();

var bus = serviceProvider.GetRequiredService<IBus>();
await bus.SendRequest(new TestRequest
{
    Id = 5
});

var model = await bus.SendRequest<TestModel>(new TestResponseRequest
{
    Id = 5,
    Input = "TEST INPUT"
});
Console.WriteLine($"Id: {model.Id}, text: {model.Text}");