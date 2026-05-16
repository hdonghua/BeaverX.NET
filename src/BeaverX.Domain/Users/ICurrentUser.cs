namespace BeaverX.Domain.Users;

/// <summary>
/// 当前登录用户上下文契约
/// </summary>
public interface ICurrentUser 
{
    /// <summary>
    /// 当前用户 ID（未登录则为 null，默认锁定雪花 ID 的 long 类型）
    /// </summary>
    long? Id { get; }

    /// <summary>
    /// 当前用户名
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// 是否已认证/已登录
    /// </summary>
    bool IsAuthenticated => Id.HasValue;
}
