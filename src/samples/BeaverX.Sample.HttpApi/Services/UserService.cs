using BeaverX.Core.Dependency;
using BeaverX.Domain.Repositories;
using BeaverX.Sample.HttpApi.Models;

namespace BeaverX.Sample.HttpApi.Services;

public class UserService : IUserService, IScopedDependency
{
    private readonly IRepository<User, Guid> repository;

    public UserService(IRepository<User, Guid> repository)
    {
        this.repository = repository;
    }

    public Task<List<User>> GetListAsync()
    {
        return repository.GetListAsync();
    }
}
