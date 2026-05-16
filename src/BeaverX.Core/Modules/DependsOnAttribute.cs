namespace BeaverX.Core.Modules;

/// <summary>
/// 模块依赖定义特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DependsOnAttribute : Attribute
{
    public Type[] DependedModuleTypes { get; }
    public DependsOnAttribute(params Type[] dependedModuleTypes) => DependedModuleTypes = dependedModuleTypes;
}