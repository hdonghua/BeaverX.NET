using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeaverX.Core.Modules;

public record ServiceConfigurationContext
{
    public IServiceCollection Services { get; }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; }

    public ServiceConfigurationContext(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment hostEnvironment)
    {
        Services = services;
        Configuration = configuration;
        Environment = hostEnvironment;
    }
}