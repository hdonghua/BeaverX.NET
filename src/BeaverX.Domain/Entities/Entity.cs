namespace BeaverX.Domain.Entities;

public abstract class Entity<TKey> : IEntity<TKey>
{
    public virtual TKey Id { get; set; } = default!;
}

public abstract class Entity : Entity<long>, IEntity
{
    protected Entity()
    {
    }
}
