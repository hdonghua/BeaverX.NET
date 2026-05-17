using BeaverX.Core.Modules;
using BeaverX.EntityFrameworkCore.DependencyInjection;
using BeaverX.EntityFrameworkCore.PostgreSql;
using BeaverX.WebMvc;

namespace BeaverX.Sample.HttpApi;

[DependsOn(typeof(BeaverXEntityFrameworkCorePostgreSqlModule), typeof(BeaverXWebMvcModule))]
public class HttpApiSampleModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddControllers();
        context.Services.AddOpenApi();
        context.Services.AddBeaverXDbContext<SampleDbContext>(context.Configuration.GetConnectionString("Default")!);
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