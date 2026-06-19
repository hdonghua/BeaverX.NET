using BeaverX.EntityFrameworkCore.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace BeaverX.EntityFrameworkCore.MySql;

/// <summary>
/// MySQL / MariaDB 驱动的具体实现（基于 MySql.EntityFrameworkCore）。
/// </summary>
public class MySqlDbDriverOptionsBuilder : IDbDriverOptionsBuilder
{
    public void Configure<TDbContext>(DbContextOptionsBuilder optionsBuilder, string connectionString)
        where TDbContext : DbContext
    {
        optionsBuilder.UseMySQL(connectionString, mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
        });
    }
}
