using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using BeaverX.Domain.Users;

namespace BeaverX.WebMvc.Controllers; 

/// <summary>
/// BeaverX 统一 Web 门面控制器基类（支持 Web API 和传统 MVC 行为扩展）
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BeaverXController : ControllerBase
{
    private ICurrentUser? _currentUser;

    /// <summary>
    /// 当前用户上下文
    /// </summary>
    protected ICurrentUser CurrentUser => _currentUser ??= HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
}