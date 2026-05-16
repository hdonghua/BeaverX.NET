using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using BeaverX.Domain.Entities;
using BeaverX.Domain.Repositories;

namespace BeaverX.EntityFrameworkCore.Repositories;

public class EfCoreRepository<TDbContext, TEntity, TKey> : IRepository<TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : class, IEntity<TKey>
{
    protected readonly TDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public EfCoreRepository(TDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual IQueryable<TEntity> GetQueryable() => DbSet.AsQueryable();

    public virtual async Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object?[] { id }, cancellationToken);
    }

    public virtual async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity == null)
            throw new InvalidOperationException($"在数据库中未找到 ID 为 '{id}' 的 [{typeof(TEntity).Name}] 实体！");
        return entity;
    }

    public virtual async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default) => await DbSet.LongCountAsync(cancellationToken);

    public virtual async Task<long> GetCountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => await DbSet.LongCountAsync(predicate, cancellationToken);

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) => await DbSet.AnyAsync(predicate, cancellationToken);

    // ==========================================
    // 写操作实现（结合默认 autoSave 自动存盘）
    // ==========================================

    public virtual async Task<TEntity> InsertAsync(TEntity entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        if (autoSave) await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        DbSet.Attach(entity);
        DbContext.Entry(entity).State = EntityState.Modified;
        if (autoSave) await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task DeleteAsync(TEntity entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        if (autoSave) await DbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TKey id, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, autoSave, cancellationToken);
        }
    }

    // ==========================================
    // ⚡ 工业级批量操作（彻底超越 ABP 性能的关键部分）
    // ==========================================

    public virtual async Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
        if (autoSave) await DbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        DbSet.UpdateRange(entities);
        if (autoSave) await DbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        DbSet.RemoveRange(entities);
        if (autoSave) await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 条件批量删除
    /// 直接编译为物理 SQL，不在内存中加载实体，不进 EF 状态跟踪，速度狂飙！
    /// </summary>
    public virtual async Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await DbSet.Where(predicate).ExecuteDeleteAsync(cancellationToken);
    }
}

/// <summary>
/// 默认雪花 ID 仓储公共实现基类
/// </summary>
public class EfCoreRepository<TDbContext, TEntity> : EfCoreRepository<TDbContext, TEntity, long>, IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class, IEntity
{
    public EfCoreRepository(TDbContext dbContext) : base(dbContext)
    {
    }
}