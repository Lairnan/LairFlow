using LairMorph.Extensions;
using LairMorph.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LairMorph;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddMorph(this IServiceCollection serviceCollection, Action<MorphConfiguration>? configure = null)
    {
        var busConfiguration = new MorphConfiguration();
        configure?.Invoke(busConfiguration);
        return serviceCollection.AddMorph(busConfiguration);
    }

    public static IServiceCollection AddMorph(this IServiceCollection serviceCollection, MorphConfiguration configuration)
    {
        serviceCollection.RegisterServicesFromAssemblies(configuration);
        return serviceCollection;
    }
}