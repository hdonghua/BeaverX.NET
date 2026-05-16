namespace BeaverX.Domain.Entities;

/// <summary>
/// 软删除接口契约
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
