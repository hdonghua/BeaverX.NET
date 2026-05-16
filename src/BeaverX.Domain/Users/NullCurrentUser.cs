namespace BeaverX.Domain.Users;

/// <summary>
/// 👻 影子空当前用户（当系统未处于 Web 上下文，或用户未注册实现时兜底）
/// </summary>
internal class NullCurrentUser : ICurrentUser
{
    // 单例实例，减少内存分配
    public static readonly NullCurrentUser Instance = new();

    public long? Id => null;
    public string? UserName => null;
}
