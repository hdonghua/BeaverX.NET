using BeaverX.Sample.HttpApi.Services;
using BeaverX.WebMvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace BeaverX.Sample.HttpApi.Controllers;

public class TestController : BeaverXController
{
    private readonly MessageService messageService;

    public TestController(MessageService messageService)
    {
        this.messageService = messageService;
    }

    [HttpGet]
    public string SayHello(string nickname) => messageService.SayHello(nickname);
}
