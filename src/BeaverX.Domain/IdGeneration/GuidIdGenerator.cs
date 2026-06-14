namespace BeaverX.Domain.IdGeneration;

/// <summary>
/// 默认 Guid 主键生成器。
/// </summary>
public sealed class GuidIdGenerator : IIdGenerator<Guid>
{
    public Guid Generate() => Guid.NewGuid();
}
