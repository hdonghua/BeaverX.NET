using BeaverX.Core.Modules;
using BeaverX.Domain.IdGeneration;
using BeaverX.Domain.Users;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BeaverX.Domain;

public class BeaverXDomainModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.TryAddScoped<ICurrentUser, NullCurrentUser>();
        context.Services.TryAddSingleton(typeof(IIdGenerator<>), typeof(DefaultIdGenerator<>));
        context.Services.TryAddSingleton<IIdGenerator<Guid>, GuidIdGenerator>();
    }
}
