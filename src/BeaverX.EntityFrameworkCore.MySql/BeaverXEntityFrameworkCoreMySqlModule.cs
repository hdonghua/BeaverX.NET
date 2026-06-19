using BeaverX.Core.Modules;
using BeaverX.Domain.Uow;
using BeaverX.EntityFrameworkCore.DependencyInjection;
using BeaverX.EntityFrameworkCore.MySql.Uow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BeaverX.EntityFrameworkCore.MySql;

[DependsOn(typeof(BeaverXEntityFrameworkCoreModule))]
public class BeaverXEntityFrameworkCoreMySqlModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.TryAddSingleton<IDbDriverOptionsBuilder, MySqlDbDriverOptionsBuilder>();
        context.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
