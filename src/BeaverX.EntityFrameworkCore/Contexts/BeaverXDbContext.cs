using BeaverX.Domain.Entities;
using BeaverX.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BeaverX.EntityFrameworkCore.Contexts;

public abstract class BeaverXDbContext<TDbContext> : DbContext
    where TDbContext : DbContext
{
    public ICurrentUser CurrentUser { get; }

    protected BeaverXDbContext(DbContextOptions<TDbContext> options, ICurrentUser currentUser) : base(options)
    {
        CurrentUser = currentUser;
    }

    // ==========================================
    // 全自动注入：全局软删除查询过滤器
    // ==========================================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 遍历所有实体配置，如果实现了 ISoftDelete，自动追加 e.IsDeleted == false 过滤器
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var compare = Expression.Equal(property, falseConstant);
                var lambda = Expression.Lambda(compare, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    // ==========================================
    // 全自动拦截：落库时的生命周期审计
    // ==========================================
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyBeaverXConcepts();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyBeaverXConcepts();
        return base.SaveChanges();
    }

    private void ApplyBeaverXConcepts()
    {
        long? currentUserId = CurrentUser.Id;

        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                // 新增时的审计
                case EntityState.Added:
                    if (entry.Entity is IHasCreationTime<long> creationEntity)
                    {
                        creationEntity.CreationTime = DateTime.Now;
                        if (currentUserId.HasValue) creationEntity.CreatorId = currentUserId.Value;
                    }
                    break;

                // 修改时的审计
                case EntityState.Modified:
                    if (entry.Entity is IHasModificationTime<long> modificationEntity)
                    {
                        modificationEntity.LastModificationTime = DateTime.Now;
                        if (currentUserId.HasValue) modificationEntity.LastModifierId = currentUserId.Value;
                    }
                    break;

                // 🌟 拦截物理删除，自动转换为软删除！
                case EntityState.Deleted:
                    if (entry.Entity is ISoftDelete softDeleteEntity)
                    {
                        // 强行把状态从“从数据库抹去”改为“更新数据”
                        entry.State = EntityState.Modified;
                        softDeleteEntity.IsDeleted = true;

                        // 如果还支持删除审计，一并补全时间
                        if (entry.Entity is IHasDeletionTime<long> deletionEntity)
                        {
                            deletionEntity.DeletionTime = DateTime.Now;
                            if (currentUserId.HasValue) deletionEntity.DeleterId = currentUserId.Value;
                        }
                    }
                    break;
            }
        }
    }
}