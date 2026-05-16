using BeaverX.EntityFrameworkCore.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace BeaverX.EntityFrameworkCore.PostgreSql;

/// <summary>
/// PostgreSQL 驱动的具体实现
/// </summary>
public class PostgreSqlDbDriverOptionsBuilder : IDbDriverOptionsBuilder
{
    public void Configure<TDbContext>(DbContextOptionsBuilder optionsBuilder, string connectionString)
        where TDbContext : DbContext
    {
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        });
    }
}
