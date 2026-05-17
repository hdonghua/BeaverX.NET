using BeaverX.Core.Modules;
using BeaverX.Domain;

namespace BeaverX.EntityFrameworkCore;

[DependsOn(typeof(BeaverXDomainModule))]
public class BeaverXEntityFrameworkCoreModule : BeaverXModule
{
}
