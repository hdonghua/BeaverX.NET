using BeaverX.Core.Modules;
using BeaverX.Domain.Users;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BeaverX.Domain;

public class BeaverXDomainModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context) 
    {
        context.Services.TryAddScoped<ICurrentUser, NullCurrentUser>();
    }
}
