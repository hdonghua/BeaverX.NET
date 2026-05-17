namespace BeaverX.Domain.Entities;

/// <summary>
/// 审计泛型实体抽象基类
/// </summary>
public abstract class AuditedEntity<TKey, TUserKey> : Entity<TKey>, IHasCreationTime<TUserKey>, IHasModificationTime<TUserKey>
    where TUserKey : struct
{
    // 创建审计属性
    public virtual DateTime CreationTime { get; set; } = DateTime.Now;
    public virtual TUserKey? CreatorId { get; set; }

    // 修改审计属性
    public virtual DateTime? LastModificationTime { get; set; }
    public virtual TUserKey? LastModifierId { get; set; }
}

/// <summary>
/// 实体主键与操作用户主键全长整型（雪花 ID）的审计实体基类
/// </summary>
public abstract class AuditedEntity : AuditedEntity<long, long>
{
    protected AuditedEntity()
    {
        Id = 0;
    }
}