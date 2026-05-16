namespace BeaverX.Domain.Entities;

/// <summary>
/// 包含最后修改时间与修改人审计的契约
/// </summary>
public interface IHasModificationTime<TUserKey> where TUserKey : struct
{
    DateTime? LastModificationTime { get; set; }
    TUserKey? LastModifierId { get; set; }
}
