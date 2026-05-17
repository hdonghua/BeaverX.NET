using System.Linq.Expressions;
using BeaverX.Domain.Entities;

namespace BeaverX.Domain.Repositories;

/// <summary>
/// 灵活主键的 BeaverX 泛型仓储核心契约
/// </summary>
public interface IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default); // 找不到直接抛异常
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    IQueryable<TEntity> GetQueryable();
    Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);
    Task<long> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    Task<TEntity> InsertAsync(TEntity entity, bool autoSave = true, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = true, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, bool autoSave = true, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, bool autoSave = true, CancellationToken cancellationToken = default);

    Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default);
    Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default);
    Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default);
    Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}

/// <summary>
/// BeaverX 默认推荐泛型仓储（雪花 ID 主键）
/// </summary>
/// <typeparam name="TEntity">继承自 IEntity（即主键为 long）的实体</typeparam>
public interface IRepository<TEntity> : IRepository<TEntity, long>
    where TEntity : class, IEntity<long>
{
}