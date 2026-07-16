using BeaverX.Domain.Uow;
using SqlSugar;

namespace BeaverX.Data.SqlSugar.Uow;

/// <summary>
/// SqlSugar 工作单元：嵌套计数，仅最外层开启/提交事务
/// </summary>
internal sealed class SqlSugarUnitOfWork : IUnitOfWork
{
    private readonly ISqlSugarClient _client;
    private int _transactionCounter;

    public SqlSugarUnitOfWork(ISqlSugarClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        _transactionCounter++;
        try
        {
            if (_transactionCounter == 1)
            {
                await CommitWithTransactionAsync(action, cancellationToken);
            }
            else
            {
                await action(cancellationToken);
            }
        }
        finally
        {
            _transactionCounter--;
        }
    }

    private async Task CommitWithTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        try
        {
            await _client.Ado.BeginTranAsync();
            await action(cancellationToken);
            await _client.Ado.CommitTranAsync();
        }
        catch
        {
            try
            {
                await _client.Ado.RollbackTranAsync();
            }
            catch
            {
                // 回滚失败时保留原始异常
            }

            throw;
        }
    }

    public void Dispose()
    {
        if (_transactionCounter > 0)
        {
            try
            {
                _client.Ado.RollbackTran();
            }
            catch
            {
                // ignore
            }

            _transactionCounter = 0;
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
