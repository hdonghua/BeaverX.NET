namespace BeaverX.Domain.Uow;

/// <summary>
/// BeaverX 工作单元契约
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 在 ExecutionStrategy 可重试块内执行数据库操作（委托内可含查询与 SaveChanges），并提交物理事务。
    /// 嵌套调用时仅执行委托，共享最外层的物理事务与提交；任一层抛出异常则整体回滚。
    /// </summary>
    /// <param name="action">数据库操作委托</param>
    /// <param name="cancellationToken">取消令牌，会传入 <paramref name="action"/></param>
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}
