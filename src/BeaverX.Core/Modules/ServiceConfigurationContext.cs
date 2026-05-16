using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeaverX.Core.Modules;

public record ServiceConfigurationContext
{
    public IServiceCollection Services { get; }

    public IConfiguration Configuration => _configurationLazy.Value;

    private readonly Lazy<IConfiguration> _configurationLazy;
    public ServiceConfigurationContext(IServiceCollection services)
    {
        Services = services;

        _configurationLazy = new Lazy<IConfiguration>(() =>
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));

            if (descriptor?.ImplementationInstance is IConfiguration configInstance)
            {
                return configInstance;
            }

            return new ConfigurationBuilder().Build();
        });
    }
}