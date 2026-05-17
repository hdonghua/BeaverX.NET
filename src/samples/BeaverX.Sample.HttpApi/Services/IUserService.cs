using BeaverX.Sample.HttpApi.Models;

namespace BeaverX.Sample.HttpApi.Services;

public interface IUserService
{
    Task<List<User>> GetListAsync();
}
