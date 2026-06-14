using BeaverX.Sample.HttpApi.Models;
using BeaverX.Sample.HttpApi.Services;
using BeaverX.WebMvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace BeaverX.Sample.HttpApi.Controllers;

public class TestController : BeaverXControllerBase
{
    private readonly MessageService messageService;
    private readonly IUserService userService;

    public TestController(MessageService messageService, IUserService userService)
    {
        this.messageService = messageService;
        this.userService = userService;
    }

    [HttpGet("sayHello")]
    public string SayHello(string nickname) => messageService.SayHello(nickname);

    [HttpGet("userList")]
    public Task<List<User>> GetListAsync() => userService.GetListAsync();
}
