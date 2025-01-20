using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Teniry.Cqrs.Commands;
using Teniry.Cqrs.Queries;

namespace Teniry.Cqrs;

public static class CqrsServicesExtensions {
    public static void AddCqrs(
        this IServiceCollection services
    ) {
        AddCqrs(services, Assembly.GetCallingAssembly());
    }

    public static void AddCqrs(
        this   IServiceCollection services,
        params Assembly[]         assemblies
    ) {
        services.TryAddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        services.Scan(
            selector =>
            {
                selector.FromAssemblies(assemblies)
                    .AddClasses(filter => { filter.AssignableTo(typeof(IQueryHandler<,>)); })
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });

        services.Scan(
            selector =>
            {
                selector.FromAssemblies(assemblies)
                    .AddClasses(filter => { filter.AssignableTo(typeof(ICommandHandler<,>)); })
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });

        services.Scan(
            selector =>
            {
                selector.FromAssemblies(assemblies)
                    .AddClasses(filter => { filter.AssignableTo(typeof(ICommandHandler<>)); })
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });
    }
}