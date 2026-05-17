using BeaverX.Domain.Entities;

namespace BeaverX.Sample.HttpApi.Models;

public class User : Entity<Guid>
{
    public string UserName { get; set; } = null!;
}
