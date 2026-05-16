namespace BeaverX.Domain.Entities;

/// <summary>
/// 聚合了创建、修改、删除全套审计行为的终极接口契约
/// </summary>
public interface IAuditedObject<TUserKey> :
    IHasCreationTime<TUserKey>,
    IHasModificationTime<TUserKey>,
    IHasDeletionTime<TUserKey>
    where TUserKey : struct
{
}
