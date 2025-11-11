using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LairBus.Models;

public class BusConfiguration
{
    internal List<Assembly> Assemblies { get; set; } = [];
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    public void RegisterServicesFromAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
    }

    public void RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        Assemblies.AddRange(assemblies);
    }

    public void SetLifeTimeServices(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}