using System.Reflection;
using LairBus;
using LairBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using TestBus.Requests;

var serviceCollection = new ServiceCollection();
serviceCollection.AddBus(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
var serviceProvider = serviceCollection.BuildServiceProvider();

var bus = serviceProvider.GetRequiredService<IBus>();
await bus.SendRequest(new TestRequest
{
    Id = 5
});