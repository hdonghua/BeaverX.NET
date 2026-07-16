using System.Linq.Expressions;
using BeaverX.Data.SqlSugar.Internal;
using BeaverX.Domain.Entities;
using BeaverX.Domain.IdGeneration;
using BeaverX.Domain.Users;
using SqlSugar;

namespace BeaverX.Data.SqlSugar.Repositories;

internal class SugarRepository<TEntity, TKey> : SimpleClient<TEntity>, ISugarRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, new()
{
    protected readonly IIdGenerator<TKey> IdGenerator;
    protected readonly ICurrentUser CurrentUser;

    public ISqlSugarClient Client { get; }

    public SugarRepository(
        ISqlSugarClient client,
        IIdGenerator<TKey> idGenerator,
        ICurrentUser currentUser)
    {
        Client = client;
        IdGenerator = idGenerator;
        CurrentUser = currentUser;
        Context = client;
    }

    public virtual ISugarQueryable<TEntity> GetSugarQueryable() => Client.Queryable<TEntity>();

    /// <summary>
    /// Domain 契约保留项。SqlSugar 请改用 <see cref="GetSugarQueryable"/>。
    /// </summary>
    public virtual IQueryable<TEntity> GetQueryable() =>
        throw new NotSupportedException(
            "BeaverX.Data.SqlSugar 不支持 IQueryable。请注入 ISugarRepository<TEntity, TKey> 并使用 GetSugarQueryable()。");

    protected virtual void TrySetIdIfNeeded(TEntity entity)
    {
        if (!EntityIdHelper.IsDefault(entity.Id))
        {
            return;
        }

        entity.Id = IdGenerator.Generate();
    }

    protected virtual bool IsSoftDeleteEntity => typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));

    public virtual async Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await Client.Queryable<TEntity>().InSingleAsync(id);
    }

    public virtual async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new InvalidOperationException($"在数据库中未找到 ID 为 '{id}' 的 [{typeof(TEntity).Name}] 实体！");
        }

        return entity;
    }

    public virtual async Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var list = await Client.Queryable<TEntity>().Where(predicate).Take(1).ToListAsync(cancellationToken);
        return list.FirstOrDefault();
    }

    /// <summary>
    /// IRepository / ISugarRepository 语义（隐藏 SimpleClient 同名方法）。
    /// </summary>
    public new virtual async Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await Client.Queryable<TEntity>().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// IRepository / ISugarRepository 语义（隐藏 SimpleClient 同名方法）。
    /// </summary>
    public new virtual async Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await Client.Queryable<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await Client.Queryable<TEntity>().CountAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await Client.Queryable<TEntity>().Where(predicate).CountAsync(cancellationToken);
    }

    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await Client.Queryable<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<TEntity> InsertAsync(
        TEntity entity,
        bool autoSave = true,
        CancellationToken cancellationToken = default)
    {
        TrySetIdIfNeeded(entity);
        await Client.Insertable(entity).ExecuteCommandAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(
        TEntity entity,
        bool autoSave = true,
        CancellationToken cancellationToken = default)
    {
        await Client.Updateable(entity).ExecuteCommandAsync(cancellationToken);
        return entity;
    }

    public virtual async Task DeleteAsync(
        TEntity entity,
        bool autoSave = true,
        CancellationToken cancellationToken = default)
    {
        if (IsSoftDeleteEntity)
        {
            SqlSugarClientFactory.ApplySoftDelete(entity, CurrentUser.Id);
            await Client.Updateable(entity).ExecuteCommandAsync(cancellationToken);
            return;
        }

        await Client.Deleteable(entity).ExecuteCommandAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(
        TKey id,
        bool autoSave = true,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, autoSave, cancellationToken);
        }
    }

    public virtual async Task InsertManyAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = true,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities as IList<TEntity> ?? entities.ToList();
        foreach (var entity in entityList)
        {
            TrySetIdIfNeeded(entity);
        }

        if (entityList.Count == 0)
        {
            return;
        }

        await Client.Insertable(entityList.ToList()).ExecuteCommandAsync(cancellationToken);
    }

    public virtual async Task UpdateManyAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = true,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities as IList<TEntity> ?? entities.ToList();
        if (entityList.Count == 0)
        {
            return;
        }

        await Client.Updateable(entityList.ToList()).ExecuteCommandAsync(cancellationToken);
    }

    public virtual async Task DeleteManyAsync(
        IEnumerable<TEntity> entities,
        bool autoSave = true,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities as IList<TEntity> ?? entities.ToList();
        if (entityList.Count == 0)
        {
            return;
        }

        if (IsSoftDeleteEntity)
        {
            foreach (var entity in entityList)
            {
                SqlSugarClientFactory.ApplySoftDelete(entity, CurrentUser.Id);
            }

            await Client.Updateable(entityList.ToList()).ExecuteCommandAsync(cancellationToken);
            return;
        }

        await Client.Deleteable(entityList.ToList()).ExecuteCommandAsync(cancellationToken);
    }

    /// <summary>
    /// 条件批量删除：与 EF 实现一致，走物理删除，不经过软删除转换。
    /// </summary>
    public virtual async Task DeleteManyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        await Client.Deleteable<TEntity>().Where(predicate).ExecuteCommandAsync(cancellationToken);
    }
}

internal class SugarRepository<TEntity> : SugarRepository<TEntity, long>, ISugarRepository<TEntity>
    where TEntity : class, IEntity<long>, new()
{
    public SugarRepository(
        ISqlSugarClient client,
        IIdGenerator<long> idGenerator,
        ICurrentUser currentUser)
        : base(client, idGenerator, currentUser)
    {
    }
}
