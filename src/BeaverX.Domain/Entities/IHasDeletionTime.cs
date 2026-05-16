namespace BeaverX.Domain.Entities;

/// <summary>
/// 包含删除时间与删除人审计的契约，隐式继承软删除
/// </summary>
public interface IHasDeletionTime<TUserKey> : ISoftDelete where TUserKey : struct
{
    DateTime? DeletionTime { get; set; }
    TUserKey? DeleterId { get; set; }
}
