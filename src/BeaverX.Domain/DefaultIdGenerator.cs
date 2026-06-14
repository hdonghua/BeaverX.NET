using BeaverX.Domain.IdGeneration;

namespace BeaverX.Domain
{
    internal class DefaultIdGenerator<TKey> : IIdGenerator<TKey>
    {
        public TKey Generate()
        {
            return default(TKey);
        }
    }
}
