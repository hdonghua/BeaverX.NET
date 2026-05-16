namespace BeaverX.Domain.Entities;

/// <summary>
/// 包含创建时间与创建人审计的契约
/// </summary>
public interface IHasCreationTime<TUserKey> where TUserKey : struct
{
    DateTime CreationTime { get; set; }
    TUserKey? CreatorId { get; set; }
}
