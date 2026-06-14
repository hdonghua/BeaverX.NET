namespace BeaverX.Domain.IdGeneration;

/// <summary>
/// 实体主键生成器。实现并注册到 DI 后，默认仓储在 Insert 时若主键为默认值则自动赋值。
/// </summary>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IIdGenerator<TKey>
{
    TKey Generate();
}
