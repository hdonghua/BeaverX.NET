using BeaverX.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BeaverX.EntityFrameworkCore.MySql.Uow;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly IReadOnlyList<DbContext> _dbContexts;
    private int _transactionCounter;

    public UnitOfWork(IEnumerable<DbContext> dbContexts)
    {
        _dbContexts = (dbContexts ?? throw new ArgumentNullException(nameof(dbContexts))).ToList();
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        _transactionCounter++;
        try
        {
            if (_transactionCounter == 1)
            {
                await CommitWithExecutionStrategyAsync(action, cancellationToken);
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

    private async Task CommitWithExecutionStrategyAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        if (_dbContexts.Count == 0)
        {
            await action(cancellationToken);
            return;
        }

        var dbList = _dbContexts;
        var mainDbContext = dbList[0];
        var strategy = mainDbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await mainDbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var dbTransaction = transaction.GetDbTransaction();

                for (var i = 1; i < dbList.Count; i++)
                {
                    await dbList[i].Database.UseTransactionAsync(dbTransaction, cancellationToken);
                }

                await action(cancellationToken);

                foreach (var dbContext in dbList)
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                ClearChangeTrackers();
                throw;
            }
        });
    }

    private void ClearChangeTrackers()
    {
        foreach (var dbContext in _dbContexts)
        {
            dbContext.ChangeTracker.Clear();
        }
    }

    public void Dispose()
    {
        if (_transactionCounter > 0)
        {
            ClearChangeTrackers();
            _transactionCounter = 0;
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
