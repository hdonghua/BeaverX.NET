using SqlSugar;

namespace BeaverX.Data.SqlSugar.DependencyInjection;

/// <summary>
/// SqlSugar 注册选项。由业务在 <c>AddBeaverXSqlSugar</c> 中配置连接与实体。
/// </summary>
public class BeaverXSqlSugarOptions
{
    /// <summary>
    /// 数据库连接字符串。
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 数据库类型，由业务自行选择（PostgreSQL / MySql / SqlServer / Sqlite 等）。
    /// </summary>
    public DbType DbType { get; set; } = DbType.PostgreSQL;

    /// <summary>
    /// 是否自动关闭连接（SqlSugar 推荐开启）。
    /// </summary>
    public bool IsAutoCloseConnection { get; set; } = true;

    /// <summary>
    /// 额外 ConnectionConfig 配置回调（可选）。
    /// </summary>
    public Action<ConnectionConfig>? ConfigureConnection { get; set; }

    /// <summary>
    /// 客户端创建后的回调（可挂日志、其它 AOP，可选）。
    /// </summary>
    public Action<ISqlSugarClient>? ConfigureClient { get; set; }

    /// <summary>
    /// 实体写入前处理回调。与框架内置审计处理在同一个 DataExecuting AOP 中执行，
    /// 避免业务侧覆盖内置审计处理器。
    /// </summary>
    public Action<object?>? NormalizeEntityBeforeWrite { get; set; }

    internal List<Type> EntityTypes { get; } = [];

    /// <summary>
    /// 注册需要自动绑定 <see cref="Domain.Repositories.IRepository{TEntity,TKey}"/> 的实体类型。
    /// </summary>
    public BeaverXSqlSugarOptions AddEntity<TEntity>()
        where TEntity : class
    {
        EntityTypes.Add(typeof(TEntity));
        return this;
    }

    /// <summary>
    /// 注册实体类型。
    /// </summary>
    public BeaverXSqlSugarOptions AddEntity(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        EntityTypes.Add(entityType);
        return this;
    }

    /// <summary>
    /// 从程序集扫描实现了 <see cref="Domain.Entities.IEntity"/> 的具体类并注册仓储。
    /// </summary>
    public BeaverXSqlSugarOptions AddEntitiesFromAssembly(System.Reflection.Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var entityTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Domain.Entities.IEntity<>)));

        foreach (var type in entityTypes)
        {
            EntityTypes.Add(type);
        }

        return this;
    }
}
