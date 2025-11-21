using LairBus.Extensions;
using LairBus.Implementations;
using LairBus.Interfaces;
using LairBus.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LairBus;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddBus(this IServiceCollection serviceCollection, Action<BusConfiguration>? configure = null)
    {
        var busConfiguration = new BusConfiguration();
        configure?.Invoke(busConfiguration);
        return serviceCollection.AddBus(busConfiguration);
    }

    public static IServiceCollection AddBus(this IServiceCollection serviceCollection, BusConfiguration configuration)
    {
        serviceCollection.RegisterServicesFromAssemblies(configuration);
        serviceCollection.AddTransient<IBus, Bus>();
        return serviceCollection;
    }
}