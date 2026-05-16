using Microsoft.AspNetCore.Builder;

namespace BeaverX.Core.Modules;

public record ApplicationInitializationContext(IApplicationBuilder App);