using Microsoft.EntityFrameworkCore;

namespace BeaverX.EntityFrameworkCore.DependencyInjection;

/// <summary>
/// 🔌 数据库驱动选项构建器（由具体驱动包实现，如 PG 包）
/// </summary>
public interface IDbDriverOptionsBuilder
{
    /// <summary>
    /// 配置具体的数据库驱动（如 UseNpgsql 或 UseSqlServer）
    /// </summary>
    void Configure<TDbContext>(DbContextOptionsBuilder optionsBuilder, string connectionString)
        where TDbContext : DbContext;
}