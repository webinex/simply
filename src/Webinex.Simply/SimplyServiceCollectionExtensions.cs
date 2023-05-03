using Microsoft.Extensions.DependencyInjection;

namespace Webinex.Simply;

public static class SimplyServiceCollectionExtensions
{
    public static IServiceCollection AddSimply<TEntity>(this IServiceCollection services, Action<ISimplyConfiguration> configure)
    {
        return services.AddSimply<TEntity, Guid>(configure);
    }

    public static IServiceCollection AddSimply<TEntity, TKey>(this IServiceCollection services, Action<ISimplyConfiguration> configure)
    {
        var configuration = new SimplyConfiguration<TEntity, TKey>(services);
        configure(configuration);
        
        configuration.Complete();

        return services;
    }
}