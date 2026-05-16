namespace BeaverX.Core.Modules;

/// <summary>
/// 所有 BeaverX 模块的终极基类
/// </summary>
public abstract class BeaverXModule
{
    /// <summary>
    /// 注册服务到容器
    /// </summary>
    /// <param name="context"></param>
    public virtual void ConfigureServices(ServiceConfigurationContext context) { }

    /// <summary>
    /// 初始化中间件
    /// </summary>
    /// <param name="context"></param>
    public virtual void OnApplicationInitialization(ApplicationInitializationContext context) { }
}
