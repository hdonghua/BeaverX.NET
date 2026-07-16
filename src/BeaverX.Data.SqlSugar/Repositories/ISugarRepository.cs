using BeaverX.Domain.Entities;
using BeaverX.Domain.Repositories;
using SqlSugar;

namespace BeaverX.Data.SqlSugar.Repositories;

/// <summary>
/// SqlSugar 专属仓储扩展：复杂查询请使用 <see cref="GetSugarQueryable"/>，勿依赖 Domain 的 <c>GetQueryable</c>。
/// </summary>
public interface ISugarRepository<TEntity, TKey> : IRepository<TEntity, TKey>, ISimpleClient<TEntity>
    where TEntity : class, IEntity<TKey>, new()
{
    ISqlSugarClient Client { get; }

    ISugarQueryable<TEntity> GetSugarQueryable();
}

/// <summary>
/// 默认 long 主键的 SqlSugar 仓储扩展。
/// </summary>
public interface ISugarRepository<TEntity> : ISugarRepository<TEntity, long>, IRepository<TEntity>
    where TEntity : class, IEntity<long>, new()
{
}
