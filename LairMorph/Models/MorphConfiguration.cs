using System.Reflection;

namespace LairMorph.Models;

public class MorphConfiguration
{
    internal List<Assembly> Assemblies { get; set; } = [];

    public void RegisterServicesFromAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
    }

    public void RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        Assemblies.AddRange(assemblies);
    }
}