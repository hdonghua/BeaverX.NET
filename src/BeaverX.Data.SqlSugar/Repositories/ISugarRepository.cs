using System.Linq.Expressions;
using BeaverX.Domain.Entities;
using BeaverX.Domain.Repositories;
using SqlSugar;

namespace BeaverX.Data.SqlSugar.Repositories;

/// <summary>
/// SqlSugar 专属仓储扩展：复杂查询请使用 <see cref="GetSugarQueryable"/>，勿依赖<see cref="IRepository.GetQueryable"/>
/// </summary>
public interface ISugarRepository<TEntity, TKey> : IRepository<TEntity, TKey>, ISimpleClient<TEntity>
    where TEntity : class, IEntity<TKey>, new()
{
    ISqlSugarClient Client { get; }

    ISugarQueryable<TEntity> GetSugarQueryable();

    /// <inheritdoc cref="IRepository{TEntity,TKey}.GetListAsync(CancellationToken)"/>
    new Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IRepository{TEntity,TKey}.GetListAsync(Expression{Func{TEntity,bool}},CancellationToken)"/>
    new Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 默认 long 主键的 SqlSugar 仓储扩展。
/// </summary>
public interface ISugarRepository<TEntity> : ISugarRepository<TEntity, long>, IRepository<TEntity>
    where TEntity : class, IEntity<long>, new()
{
}
