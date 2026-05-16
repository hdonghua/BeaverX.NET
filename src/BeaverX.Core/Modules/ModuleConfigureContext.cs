using Microsoft.Extensions.DependencyInjection;

namespace BeaverX.Core.Modules;

public record ModuleConfigureContext(IServiceCollection Services);