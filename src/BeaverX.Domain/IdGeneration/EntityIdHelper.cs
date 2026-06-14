namespace BeaverX.Domain.IdGeneration;

public static class EntityIdHelper
{
    public static bool IsDefault<TKey>(TKey id)
    {
        if (id is null)
        {
            return true;
        }

        return id switch
        {
            string s => string.IsNullOrEmpty(s),
            Guid g => g == Guid.Empty,
            _ => EqualityComparer<TKey>.Default.Equals(id, default)
        };
    }
}
