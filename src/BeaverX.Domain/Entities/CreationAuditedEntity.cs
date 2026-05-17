namespace BeaverX.Domain.Entities;

/// <summary>
/// 创建审计泛型实体抽象基类
/// </summary>
public abstract class CreationAuditedEntity<TKey, TUserKey> : Entity<TKey>, IHasCreationTime<TUserKey>
    where TUserKey : struct
{
    // 创建审计属性
    public virtual DateTime CreationTime { get; set; } = DateTime.Now;
    public virtual TUserKey? CreatorId { get; set; }
}

/// <summary>
/// 实体主键与操作用户主键全长整型（雪花 ID）的创建审计实体基类
/// </summary>
public abstract class CreationAuditedEntity : CreationAuditedEntity<long, long>
{
    protected CreationAuditedEntity()
    {
        Id = 0;
    }
}