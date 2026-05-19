namespace BeaverX.Domain.Uow;

/// <summary>
/// BeaverX 工作单元生命周期契约
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 开启事务
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交所有 DbContext 的更改并提交物理事务
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 回滚事务
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
