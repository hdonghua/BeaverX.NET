using BeaverX.Core.Modules;
using BeaverX.Domain;

namespace BeaverX.EntityFrameworkCore;

[DependsOn(typeof(BeaverXDomainModule))]
public class BeaverXEntityFrameworkCoreModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);
    }
}
