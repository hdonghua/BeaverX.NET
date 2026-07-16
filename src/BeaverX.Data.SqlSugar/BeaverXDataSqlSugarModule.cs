using BeaverX.Core.Modules;
using BeaverX.Domain;

namespace BeaverX.Data.SqlSugar;

/// <summary>
/// SqlSugar 数据访问模块
/// </summary>
[DependsOn(typeof(BeaverXDomainModule))]
public class BeaverXDataSqlSugarModule : BeaverXModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);
    }
}
