using BeaverX.Core.Modules;
using BeaverX.EntityFrameworkCore.PostgreSql;
using BeaverX.WebMvc;

namespace BeaverX.Sample.HttpApi;

[DependsOn(typeof(BeaverXWebMvcModule))]
public class HttpApiSampleModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddControllers();
        context.Services.AddOpenApi();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = (WebApplication)context.App;
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.MapControllers();
    }
}