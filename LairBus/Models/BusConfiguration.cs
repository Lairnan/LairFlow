using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LairBus.Models;

public class BusConfiguration
{
    internal List<Assembly> Assemblies { get; set; } = [];
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    public BusConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
        return this;
    }

    public BusConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        Assemblies.AddRange(assemblies);
        return this;
    }

    public BusConfiguration RegisterServicesFromAssemblyContaining<TAssembly>() where TAssembly : class
    {
        return RegisterServicesFromAssembly(typeof(TAssembly).Assembly);
    }

    public void SetLifeTimeServices(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}