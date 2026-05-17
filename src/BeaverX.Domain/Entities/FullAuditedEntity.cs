namespace BeaverX.Domain.Entities;

/// <summary>
/// 带有完整审计和软删除功能的泛型实体抽象基类
/// </summary>
public abstract class FullAuditedEntity<TKey, TUserKey> : Entity<TKey>, IHasCreationTime<TUserKey>, IHasModificationTime<TUserKey>, IHasDeletionTime<TUserKey>
    where TUserKey : struct
{
    // 创建审计属性
    public virtual DateTime CreationTime { get; set; } = DateTime.Now;
    public virtual TUserKey? CreatorId { get; set; }

    // 修改审计属性
    public virtual DateTime? LastModificationTime { get; set; }
    public virtual TUserKey? LastModifierId { get; set; }

    // 删除与软删除属性
    public virtual bool IsDeleted { get; set; }
    public virtual DateTime? DeletionTime { get; set; }
    public virtual TUserKey? DeleterId { get; set; }
}

/// <summary>
/// BeaverX 默认推荐体系：实体主键与操作用户主键全长整型（雪花 ID）的审计实体基类
/// </summary>
public abstract class FullAuditedEntity : FullAuditedEntity<long, long>
{
    protected FullAuditedEntity()
    {
        Id = 0;
    }
}