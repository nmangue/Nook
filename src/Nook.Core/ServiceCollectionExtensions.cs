using Microsoft.Extensions.DependencyInjection;

namespace Nook.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNook(this IServiceCollection sc)
    {
        sc.AddSingleton(typeof(IUse<>), typeof(Use<>));
        return sc;
    }
}