using BeaverX.Domain.Entities;
using BeaverX.Domain.Repositories;
using BeaverX.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BeaverX.EntityFrameworkCore.DependencyInjection;

public static class BeaverXEfCoreServiceCollectionExtensions
{
    /// <summary>
    /// 统一注册 BeaverX 数据上下文的超级扩展方法
    /// </summary>
    /// <typeparam name="TDbContext">用户自定义的 DbContext 类型</typeparam>
    /// <param name="services">DI 容器</param>
    /// <param name="connectionString">数据库连接字符串</param>
    public static IServiceCollection AddBeaverXDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString)
        where TDbContext : DbContext
    {
        // 动态从容器中获取已经安装的具体驱动构建器
        services.AddDbContext<TDbContext>((provider, options) =>
        {
            var driverBuilder = provider.GetService<IDbDriverOptionsBuilder>() ?? throw new InvalidOperationException("未检测到任何 BeaverX 数据库驱动包！");

            // 让具体的驱动包去执行 UseNpgsql 等行为
            driverBuilder.Configure<TDbContext>(options, connectionString);
        });

        // 抓取当前 TDbContext 中所有类型为 DbSet<T> 的属性
        var dbSetProperties = typeof(TDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        foreach (var prop in dbSetProperties)
        {
            // 拿到实体的强类型，例如：Order
            var entityType = prop.PropertyType.GetGenericArguments()[0];

            // 寻找该实体继承的 IEntity<TKey> 接口，从而抠出主键 TKey 的真实类型
            var entityInterface = entityType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));

            if (entityInterface == null) continue;

            // 拿到主键类型，例如：Guid
            var keyType = entityInterface.GetGenericArguments()[0];

            // 拼装出业务层需要的仓储接口：IRepository<Order, Guid>
            var repositoryInterfaceType = typeof(IRepository<,>).MakeGenericType(entityType, keyType);

            // 拼装出当前 DbContext 专属的底层完全体实现类：EfCoreRepository<OrderDbContext, Order, Guid>
            var repositoryImplementationType = typeof(EfCoreRepository<,,>).MakeGenericType(typeof(TDbContext), entityType, keyType);

            // 静态绑定注册到容器中
            services.AddScoped(repositoryInterfaceType, repositoryImplementationType);

            // 如果主键是 long，顺手把单参数的 IRepository<Order> 接口也给注册了
            if (keyType == typeof(long))
            {
                var longRepositoryImplementationType = typeof(EfCoreRepository<,>).MakeGenericType(typeof(TDbContext), entityType);
                var singleInterfaceType = typeof(IRepository<>).MakeGenericType(entityType);
                services.AddScoped(singleInterfaceType, longRepositoryImplementationType);
            }
        }

        return services;
    }
}