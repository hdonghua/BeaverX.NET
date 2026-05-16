using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using BeaverX.Core.Dependency;
using BeaverX.Core.Modules;

namespace BeaverX.Core;

public static class BeaverXEngine
{
    private static List<BeaverXModule> _modules = new();

    public static IServiceCollection AddBeaverX<TStartupModule>(this IServiceCollection services)
        where TStartupModule : BeaverXModule
    {
        var sortedModuleTypes = SortModuleTypes(typeof(TStartupModule));
        _modules = sortedModuleTypes.Select(t => (BeaverXModule)Activator.CreateInstance(t)!).ToList();

        var context = new ServiceConfigurationContext(services);
        foreach (var module in _modules)
        {
            module.ConfigureServices(context);
            RegisterAssemblyDependencies(services, module.GetType().Assembly);
        }

        return services;
    }

    public static IApplicationBuilder InitializeBeaverX(this IApplicationBuilder app)
    {
        var context = new ApplicationInitializationContext(app);
        foreach (var module in _modules)
        {
            module.OnApplicationInitialization(context);
        }
        return app;
    }

    private static List<Type> SortModuleTypes(Type startupModuleType)
    {
        var sorted = new List<Type>();
        var visited = new Dictionary<Type, bool>();

        void Visit(Type type)
        {
            if (visited.TryGetValue(type, out bool inProcess))
            {
                if (inProcess) throw new InvalidOperationException($"🚨 BeaverX 检测到循环依赖: {type.FullName}");
                return;
            }

            visited[type] = true;

            var dependencies = type.GetCustomAttributes<DependsOnAttribute>()
                                   .SelectMany(a => a.DependedModuleTypes);
            foreach (var dependency in dependencies)
            {
                Visit(dependency);
            }

            visited[type] = false;
            sorted.Add(type);
        }

        Visit(startupModuleType);
        return sorted;
    }

    private static void RegisterAssemblyDependencies(IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition);

        foreach (var type in types)
            if (typeof(ITransientDependency).IsAssignableFrom(type))
                RegisterTypeWithLifetime(services, type, ServiceLifetime.Transient);
            else if (typeof(IScopedDependency).IsAssignableFrom(type))
                RegisterTypeWithLifetime(services, type, ServiceLifetime.Scoped);
            else if (typeof(ISingletonDependency).IsAssignableFrom(type))
                RegisterTypeWithLifetime(services, type, ServiceLifetime.Singleton);
    }

    private static void RegisterTypeWithLifetime(IServiceCollection services, Type type, ServiceLifetime lifetime)
    {
        var interfaces = type.GetInterfaces()
            .Where(i => i != typeof(ITransientDependency) && i != typeof(IScopedDependency) && i != typeof(ISingletonDependency))
            .ToList();

        if (interfaces.Count > 0)
            foreach (var @interface in interfaces)
                services.Add(new ServiceDescriptor(@interface, type, lifetime));
        else
            services.Add(new ServiceDescriptor(type, type, lifetime));
    }
}