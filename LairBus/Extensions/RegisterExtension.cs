using LairBus.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LairBus.Extensions;

internal static class RegisterExtension
{
    internal static void RegisterServicesFromAssemblies(this IServiceCollection serviceCollection,
        BusConfiguration configuration)
    {
    }
}