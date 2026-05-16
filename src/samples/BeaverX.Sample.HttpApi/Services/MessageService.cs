using BeaverX.Core.Dependency;

namespace BeaverX.Sample.HttpApi.Services;

public class MessageService : ITransientDependency
{
    public string SayHello(string nickname) => $"{nickname}, Hello";
}
