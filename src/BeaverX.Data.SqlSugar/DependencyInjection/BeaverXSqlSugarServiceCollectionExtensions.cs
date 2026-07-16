using BeaverX.Data.SqlSugar.Internal;
using BeaverX.Data.SqlSugar.Repositories;
using BeaverX.Data.SqlSugar.Uow;
using BeaverX.Domain.Entities;
using BeaverX.Domain.Repositories;
using BeaverX.Domain.Uow;
using BeaverX.Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace BeaverX.Data.SqlSugar.DependencyInjection;

public static class BeaverXSqlSugarServiceCollectionExtensions
{
    /// <summary>
    /// 注册 SqlSugar 客户端、工作单元，已声明实体绑定仓储
    /// </summary>
    public static IServiceCollection AddBeaverXSqlSugar(
        this IServiceCollection services,
        Action<BeaverXSqlSugarOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<BeaverXSqlSugarOptions>();
        services.Configure(configure);

        services.TryAddScoped<ISqlSugarClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<BeaverXSqlSugarOptions>>();
            var currentUser = sp.GetRequiredService<ICurrentUser>();
            return SqlSugarClientFactory.Create(options, currentUser);
        });

        services.TryAddScoped<IUnitOfWork, SqlSugarUnitOfWork>();

        // 先应用配置以拿到实体列表（与 EF 反射 DbSet 对等）
        var optionsInstance = new BeaverXSqlSugarOptions();
        configure(optionsInstance);

        foreach (var entityType in optionsInstance.EntityTypes.Distinct())
        {
            RegisterRepository(services, entityType);
        }

        return services;
    }

    /// <summary>
    /// 简化重载：连接字符串 + 数据库类型 + 实体程序集扫描。
    /// </summary>
    public static IServiceCollection AddBeaverXSqlSugar(
        this IServiceCollection services,
        string connectionString,
        DbType dbType,
        params System.Reflection.Assembly[] entityAssemblies)
    {
        return services.AddBeaverXSqlSugar(options =>
        {
            options.ConnectionString = connectionString;
            options.DbType = dbType;
            foreach (var assembly in entityAssemblies)
            {
                options.AddEntitiesFromAssembly(assembly);
            }
        });
    }

    private static void RegisterRepository(IServiceCollection services, Type entityType)
    {
        var entityInterface = entityType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));

        if (entityInterface == null)
        {
            return;
        }

        // SqlSugar Insertable/Updateable 需要无参构造；无则跳过自动注册
        if (entityType.GetConstructor(Type.EmptyTypes) == null)
        {
            throw new InvalidOperationException(
                $"实体 [{entityType.Name}] 必须提供无参构造函数，才能由 BeaverX.Data.SqlSugar 自动注册仓储。");
        }

        var keyType = entityInterface.GetGenericArguments()[0];

        var repositoryInterfaceType = typeof(IRepository<,>).MakeGenericType(entityType, keyType);
        var sugarRepositoryInterfaceType = typeof(ISugarRepository<,>).MakeGenericType(entityType, keyType);
        var repositoryImplementationType = typeof(SugarRepository<,>).MakeGenericType(entityType, keyType);

        services.AddScoped(repositoryImplementationType);
        services.AddScoped(repositoryInterfaceType, sp => sp.GetRequiredService(repositoryImplementationType));
        services.AddScoped(sugarRepositoryInterfaceType, sp => sp.GetRequiredService(repositoryImplementationType));

        if (keyType == typeof(long))
        {
            var longRepoImplementationType = typeof(SugarRepository<>).MakeGenericType(entityType);
            var singleInterfaceType = typeof(IRepository<>).MakeGenericType(entityType);
            var singleSugarInterfaceType = typeof(ISugarRepository<>).MakeGenericType(entityType);

            services.AddScoped(longRepoImplementationType);
            services.AddScoped(singleInterfaceType, sp => sp.GetRequiredService(longRepoImplementationType));
            services.AddScoped(singleSugarInterfaceType, sp => sp.GetRequiredService(longRepoImplementationType));
        }
    }
}
