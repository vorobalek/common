using System;
using System.Linq;
using System.Reflection;
using Common.Database.Infrastructure;
using Common.Infrastructure.Extensions;
using Common.Database.Infrastructure.Services;
using Common.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Database.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonDatabaseFeatures<TContext>(
        this IServiceCollection services,
        params Assembly[] assemblies)
        where TContext : DbContext, ICommonDbContext<TContext>
    {
        var count = assemblies.Length;
        Array.Resize(ref assemblies, count + 3);
        assemblies[count] = typeof(IEntity).Assembly;
        assemblies[count + 1] = typeof(EntityChangeListener<>).Assembly;
        assemblies[count + 2] = typeof(TContext).Assembly;

        services
            .AddEntityChangeListeners(assemblies)
            .AddEntityTraitChangeListeners(assemblies)
            .AddEntityHostChangeListeners(assemblies)
            .AddScoped<IEntityChangeListenerService<TContext>, EntityChangeListenerService<TContext>>();
        return services;
    }

    public static IServiceCollection AddEntityChangeListeners(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Distinct()
            .ToArray();

        var listeners = types
            .Where(type => type.HasInterface(typeof(IEntityChangeListener<>)) &&
                           !type.IsAbstract &&
                           !type.IsGenericType)
            .Distinct();

        foreach (var listener in listeners)
        {
            var type = listener.GetGenericTypeArgumentsOfSpecificParent(typeof(EntityChangeListener<>))
                .FirstOrDefault();
            if (type == null) continue;
            services.AddEntityChangeListener(type, listener);
        }

        return services;
    }

    public static IServiceCollection AddEntityTraitChangeListeners(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        AssertIEntityTraitInheritance(assemblies);

        var types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Distinct()
            .ToArray();

        var entityTypes = types
            .Where(type => type.HasInterface(typeof(IEntityTrait)) &&
                           !type.IsAbstract)
            .Distinct()
            .ToArray();

        var listeners = types
            .Where(type => !type.IsAbstract)
            .Where(type => type.HasInterface(typeof(IEntityTraitChangeListener<,>)))
            .Distinct()
            .Select(type => new
            {
                Type = type, Args = type.GetGenericTypeArgumentsOfSpecificParent(typeof(EntityTraitChangeListener<,>))
            })
            .Where(t => t.Args.Length == 2);

        foreach (var listener in listeners)
        {
            var trait = listener.Args[1];
            var traitTypes = entityTypes.Where(type => type.IsAssignableTo(trait)).ToArray();

            if (!listener.Type.ContainsGenericParameters ||
                listener.Type.GetGenericArguments().Length != 1 ||
                listener.Type.GetGenericArguments()[0].GetGenericParameterConstraints().Length < 1 ||
                !listener.Type.GetGenericArguments()[0].GetGenericParameterConstraints()[0]
                    .HasInterface(typeof(IEntityTrait)))
                throw new ArgumentException(
                    $"The {listener.Type.FullName} must be a generic type and have a single generic parameter with type cast to {typeof(IEntityTrait).FullName}");

            foreach (var type in traitTypes)
                services.AddEntityChangeListener(type, listener.Type.MakeGenericType(type));
        }

        return services;
    }

    public static IServiceCollection AddEntityHostChangeListeners(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        AssertIEntityHostInheritance(assemblies);

        var types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Distinct()
            .ToArray();

        var entityTypes = types
            .Where(type => type.HasInterface(typeof(IEntityHost<>)) &&
                           !type.IsAbstract)
            .Distinct()
            .ToArray();

        var listeners = types
            .Where(type => !type.IsAbstract)
            .Where(type => type.HasInterface(typeof(IEntityHostChangeListener<,>)))
            .Distinct()
            .Select(type => new
            {
                Type = type, Args = type.GetGenericTypeArgumentsOfSpecificParent(typeof(EntityHostChangeListener<,>))
            })
            .Where(t => t.Args.Length == 2);

        foreach (var listener in listeners)
        {
            var host = listener.Args[1];
            var hostTypes = entityTypes.Where(type => type.HasInterface(host)).ToArray();

            foreach (var type in hostTypes)
            {
                if (type.GetInterface(host.Name) == null ||
                    type.GetInterface(host.Name)!.GetGenericArguments().Any(a => a.IsGenericParameter))
                    throw new ArgumentException(
                        $"The {type.FullName} cannot contains interfaces with undefined generic arguments");

                if (!listener.Type.ContainsGenericParameters ||
                    listener.Type.GetGenericArguments().Count(a => a.IsGenericParameter) !=
                    type.GetInterface(host.Name)?.GetGenericArguments().Length)
                    throw new ArgumentException(
                        $"The {listener.Type.Name} must be a generic type and have as many generic parameters as the interface {host.Name}");

                services.AddEntityChangeListener(type,
                    listener.Type.MakeGenericType(type.GetInterface(host.Name)!.GetGenericArguments()));
            }
        }

        return services;
    }

    public static IServiceCollection AddEntityChangeListener(
        this IServiceCollection services,
        Type typeOfEntity,
        Type typeOfEntityChangeListener)
    {
        typeOfEntity
            .AssertIsAssignableTo(typeof(IEntity));
        typeOfEntityChangeListener
            .AssertIsAssignableTo(typeof(IEntityChangeListener));
        typeOfEntityChangeListener
            .AssertIsAssignableTo(typeof(IEntityChangeListener<>).MakeGenericType(typeOfEntity));

        if (!services.Any(s =>
                s.ServiceType == typeof(IEntityChangeListener) &&
                s.ImplementationType == typeOfEntityChangeListener))
            services.AddScoped(
                typeof(IEntityChangeListener),
                typeOfEntityChangeListener);

        if (!services.Any(s =>
                s.ServiceType == typeof(IEntityChangeListener<>).MakeGenericType(typeOfEntity) &&
                s.ImplementationType == typeOfEntityChangeListener))
            services.AddScoped(
                typeof(IEntityChangeListener<>).MakeGenericType(typeOfEntity),
                typeOfEntityChangeListener);

        return services;
    }

    private static void AssertIEntityTraitInheritance(params Assembly[] assemblies)
    {
        var types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Distinct()
            .Where(t => t.IsAbstract && t.IsInterface && t.IsAssignableTo(typeof(IEntityTrait)))
            .Where(t => t.GetInterfaces().Any(i => i != typeof(IEntityTrait) && i != typeof(IEntity)))
            .ToArray();

        if (types.Length > 0)
            throw new ApplicationException(
                $"Traits must be atomic, inheritance of traits is not allowed except from {nameof(IEntityTrait)}:" +
                Environment.NewLine +
                string.Join(
                    Environment.NewLine,
                    types.Select(t => $"{t.Name} is inheritance from " +
                                      string.Join(" ",
                                          t.GetInterfaces()
                                              .Where(i => i != typeof(IEntityTrait) && i != typeof(IEntity))))));
    }

    private static void AssertIEntityHostInheritance(params Assembly[] assemblies)
    {
        var types = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Distinct()
            .Where(t => t.IsAbstract && t.IsInterface && t.IsAssignableTo(typeof(IEntityHost<>)))
            .Where(t => t.GetInterfaces().Any(i => i != typeof(IEntityHost<>) && i != typeof(IEntity)))
            .ToArray();

        if (types.Length > 0)
            throw new ApplicationException(
                $"Hosts must be atomic, inheritance of hosts is not allowed except from {typeof(IEntityHost<>).Name}:" +
                Environment.NewLine +
                string.Join(
                    Environment.NewLine,
                    types.Select(t => $"{t.Name} is inheritance from " +
                                      string.Join(" ",
                                          t.GetInterfaces().Where(i =>
                                              i != typeof(IEntityHost<>) && i != typeof(IEntity))))));
    }
}