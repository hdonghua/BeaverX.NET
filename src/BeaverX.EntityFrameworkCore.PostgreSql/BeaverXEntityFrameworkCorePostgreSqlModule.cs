using BeaverX.Core.Modules;
using BeaverX.EntityFrameworkCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BeaverX.EntityFrameworkCore.PostgreSql;

[DependsOn(typeof(BeaverXEntityFrameworkCoreModule))]
public class BeaverXEntityFrameworkCorePostgreSqlModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.TryAddSingleton<IDbDriverOptionsBuilder, PostgreSqlDbDriverOptionsBuilder>();
    }
}
