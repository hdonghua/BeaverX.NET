using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using BeaverX.Domain.Users;

namespace BeaverX.WebMvc.Users; 

public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? Id
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? user.FindFirst("sub")?.Value;

            return long.TryParse(userIdStr, out var id) ? id : null;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value;
}