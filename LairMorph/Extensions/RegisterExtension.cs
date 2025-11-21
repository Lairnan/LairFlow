using System.Reflection;
using LairMorph.Interfaces;
using LairMorph.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LairMorph.Extensions;

internal static class RegisterExtension
{
    private const string AutoMorphTypeName = nameof(IAutoMorphEntity);
    internal static readonly List<Type> AutoMorphEntities = [];
    
    internal static void RegisterServicesFromAssemblies(this IServiceCollection serviceCollection,
        MorphConfiguration configuration)
    {
        var assemblies = configuration.Assemblies.Distinct().ToArray();

        var allTypes = assemblies.SelectMany(s =>
        {
            try
            {
                return s.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
            }
        });

        var concreteTypes = allTypes.Where(s => s is { IsAbstract: false, IsClass: true }).ToArray();

        foreach (var implementationType in concreteTypes)
        {
            var serviceInterface = implementationType!.GetInterfaces().FirstOrDefault(s => s.Name == AutoMorphTypeName);
            if (serviceInterface != null)
                AutoMorphEntities.Add(implementationType);
        }
    }
}