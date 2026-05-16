namespace BeaverX.Domain.Entities;

/// <summary>
/// 泛型实体接口契约
/// </summary>
/// <typeparam name="TKey">主键类型（如 Guid, int, long）</typeparam>
public interface IEntity<TKey>
{
    TKey Id { get; set; }
}

/// <summary>
/// 默认以 long 为主键的实体接口简写
/// </summary>
public interface IEntity : IEntity<long> { }