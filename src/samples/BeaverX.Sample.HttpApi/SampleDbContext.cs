using BeaverX.Domain.Users;
using BeaverX.EntityFrameworkCore.Contexts;
using BeaverX.Sample.HttpApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BeaverX.Sample.HttpApi;

public class SampleDbContext : BeaverXDbContext<SampleDbContext>
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options, ICurrentUser currentUser) : base(options, currentUser)
    {
    }

    public DbSet<User> User { get; set;  }
}
