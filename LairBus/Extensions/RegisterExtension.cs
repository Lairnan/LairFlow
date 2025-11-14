using System.Reflection;
using LairBus.Interfaces;
using LairBus.Models;
using Microsoft.Extensions.DependencyInjection;

namespace LairBus.Extensions;

internal static class RegisterExtension
{
    private static readonly Type[] Definitions =
    {
        typeof(INotificationHandler<>),
        typeof(IRequest<>),
        typeof(IRequestHandler<>),
        typeof(IRequestHandler<,>),
    };

    internal static void RegisterServicesFromAssemblies(this IServiceCollection serviceCollection,
        BusConfiguration configuration)
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

        var concreteTypes = allTypes
            .Where(s => s!.IsClass && s is { IsAbstract: false, IsGenericTypeDefinition: false }).ToArray();

        Action<Type, Type> serviceRegister = configuration.Lifetime switch
        {
            ServiceLifetime.Singleton => (service, impl) => serviceCollection.AddSingleton(service, impl),
            ServiceLifetime.Scoped => (service, impl) => serviceCollection.AddScoped(service, impl),
            ServiceLifetime.Transient => (service, impl) => serviceCollection.AddTransient(service, impl),
            _ => throw new ArgumentOutOfRangeException()
        };

        foreach (var implementationType in concreteTypes)
        {
            var serviceInterfaces = implementationType!.GetInterfaces()
                .Where(s => (s.IsGenericType && Definitions.Contains(s.GetGenericTypeDefinition()))
                            || (!s.IsGenericType && (s == typeof(INotification) || s == typeof(IRequest))));

            foreach (var serviceInterface in serviceInterfaces)
                serviceRegister(serviceInterface, implementationType);
        }
    }
}