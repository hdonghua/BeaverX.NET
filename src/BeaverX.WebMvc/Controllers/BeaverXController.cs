using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using BeaverX.Domain.Users;

namespace BeaverX.WebMvc.Controllers; 

/// <summary>
/// BeaverX API控制器基类
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BeaverXControllerBase : ControllerBase
{
    private ICurrentUser? _currentUser;

    /// <summary>
    /// 当前用户上下文
    /// </summary>
    protected ICurrentUser CurrentUser => _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
}