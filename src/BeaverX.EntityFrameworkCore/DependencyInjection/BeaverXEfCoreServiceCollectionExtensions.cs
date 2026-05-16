using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        // 利用常规注入，避开了程序集反射，AOT 极其安全
        services.AddDbContext<TDbContext>((provider, options) =>
        {
            var driverBuilder = provider.GetService<IDbDriverOptionsBuilder>();
            if (driverBuilder == null)
            {
                throw new InvalidOperationException("未检测到任何 BeaverX 数据库驱动包！");
            }

            // 让具体的驱动包去执行 UseNpgsql 等行为
            driverBuilder.Configure<TDbContext>(options, connectionString);
        });
        return services;
    }
}