using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BeaverX.Core.Modules;
using BeaverX.Domain.Users;
using BeaverX.WebMvc.Users;
using BeaverX.Domain;

namespace BeaverX.WebMvc;

/// <summary>
/// 🛡️ BeaverX WebMvc 表现层核心模块
/// </summary>
[DependsOn(typeof(BeaverXDomainModule))]
public class BeaverXWebMvcModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        services.AddHttpContextAccessor();

        services.Replace(ServiceDescriptor.Scoped<ICurrentUser, HttpContextCurrentUser>());
    }
}