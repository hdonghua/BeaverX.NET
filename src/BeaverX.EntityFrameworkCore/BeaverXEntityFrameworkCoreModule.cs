using BeaverX.Core.Modules;
using BeaverX.Domain;
using BeaverX.Domain.Repositories;
using BeaverX.EntityFrameworkCore.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BeaverX.EntityFrameworkCore;

[DependsOn(typeof(BeaverXDomainModule))]
public class BeaverXEntityFrameworkCoreModule : BeaverXModule
{
    public override void ConfigureServices(ModuleConfigureContext context)
    {
        var services = context.Services;

        services.AddScoped(typeof(IRepository<,>), typeof(EfCoreRepository<,,>));
        services.AddScoped(typeof(IRepository<>), typeof(EfCoreRepository<,>));
    }
}
