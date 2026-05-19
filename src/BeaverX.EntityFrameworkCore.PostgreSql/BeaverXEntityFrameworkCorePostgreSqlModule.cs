using BeaverX.Core.Modules;
using BeaverX.Domain.Uow;
using BeaverX.EntityFrameworkCore.DependencyInjection;
using BeaverX.EntityFrameworkCore.PostgreSql.Uow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BeaverX.EntityFrameworkCore.PostgreSql;

[DependsOn(typeof(BeaverXEntityFrameworkCoreModule))]
public class BeaverXEntityFrameworkCorePostgreSqlModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.TryAddSingleton<IDbDriverOptionsBuilder, PostgreSqlDbDriverOptionsBuilder>();
        context.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
