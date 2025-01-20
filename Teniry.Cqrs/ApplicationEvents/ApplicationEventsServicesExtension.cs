using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Teniry.Cqrs.ApplicationEvents.EventsChannelHandler;

namespace Teniry.Cqrs.ApplicationEvents;

public static class ApplicationEventsServicesExtension {
    public static void AddApplicationEvents(
        this IServiceCollection services
    ) {
        AddApplicationEvents(services, Assembly.GetCallingAssembly());
    }

    public static void AddApplicationEvents(
        this   IServiceCollection services,
        params Assembly[]         assemblies
    ) {
        services.TryAddScoped<IApplicationEventDispatcher, ApplicationEventDispatcher>();

        services.Scan(
            selector =>
            {
                selector.FromAssemblies(assemblies)
                    .AddClasses(filter => { filter.AssignableTo(typeof(IApplicationEventHandler<>)); })
                    .AsImplementedInterfaces().WithScopedLifetime();
            });

        services.AddSingleton<EventsChannel>();
        services.AddHostedService<EventsChannelHandlerBackgroundService>();
    }
}