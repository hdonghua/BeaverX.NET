using BeaverX.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BeaverX.EntityFrameworkCore.PostgreSql.Uow;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly IEnumerable<DbContext> _dbContexts;
    private IDbContextTransaction? _currentTransaction;
    private int _transactionCounter = 0;
    private bool _isRolledBack = false;

    public UnitOfWork(IEnumerable<DbContext> dbContexts)
    {
        _dbContexts = dbContexts ?? throw new ArgumentNullException(nameof(dbContexts));
    }

    /// <summary>
    /// 开启事务上下文，仅做嵌套计数与重置标记
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transactionCounter++;

        if (_transactionCounter == 1)
        {
            _isRolledBack = false;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 提交事务，利用单一 ExecutionStrategy 块闭环控制整个事务的生命周期
    /// </summary>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        _transactionCounter--;

        if (_isRolledBack)
        {
            throw new InvalidOperationException("Transaction failed: Nested transaction has been rolled back.");
        }

        // 仅在最外层方法退出、计数器归零时，发起真正的物理事务流水线
        if (_transactionCounter == 0)
        {
            var dbList = _dbContexts.ToList();
            if (dbList.Count == 0) return;

            var mainDbContext = dbList[0];
            var strategy = mainDbContext.Database.CreateExecutionStrategy();

            // 将“开启事务”、“数据持久化”、“提交事务”全部锁死在同一个重试执行块中
            await strategy.ExecuteAsync(async () =>
            {
                try
                {
                    // 开启主上下文物理事务
                    _currentTransaction = await mainDbContext.Database.BeginTransactionAsync(cancellationToken);
                    var dbTransaction = _currentTransaction.GetDbTransaction();

                    // 将物理事务句柄同步挂载到其余上下文
                    for (int i = 1; i < dbList.Count; i++)
                    {
                        dbList[i].Database.UseTransaction(dbTransaction);
                    }

                    // 依次触发所有上下文的数据持久化
                    foreach (var dbContext in dbList)
                    {
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }

                    // 一键提交物理事务
                    await _currentTransaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    if (_currentTransaction != null)
                    {
                        await _currentTransaction.RollbackAsync(cancellationToken);
                    }
                    throw;
                }
                finally
                {
                    if (_currentTransaction != null)
                    {
                        await _currentTransaction.DisposeAsync();
                        _currentTransaction = null;
                    }
                }
            });
        }
    }

    /// <summary>
    /// 回滚事务，标记当前上下文状态为已回滚
    /// </summary>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _isRolledBack = true;

        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        _transactionCounter = 0;
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
        }
    }
}