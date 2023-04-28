using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Webinex.Simply;

public interface ISimplyConfiguration
{
    Type KeyType { get; }
    Type EntityType { get; }
    IServiceCollection Services { get; }
    IDictionary<string, object> Values { get; }
}

internal class SimplyConfiguration<TEntity, TKey> : ISimplyConfiguration
{
    public SimplyConfiguration(IServiceCollection services)
    {
        Services = services;

        services.AddScoped(
            typeof(ISimply<,>).MakeGenericType(EntityType, KeyType),
            typeof(Simply<,>).MakeGenericType(EntityType, KeyType));

        if (typeof(TKey) == typeof(Guid))
            services.AddScoped<ISimply<TEntity>, Simply<TEntity>>();
    }

    public Type KeyType { get; } = typeof(TKey);
    public Type EntityType { get; } = typeof(TEntity);
    public IServiceCollection Services { get; }
    public IDictionary<string, object> Values { get; } = new Dictionary<string, object>();
}

public static class SimplyConfigurationExtensions
{
    public static ISimplyConfiguration AddDbContext<TDbContext>(this ISimplyConfiguration configuration)
        where TDbContext : DbContext
    {
        return AddDbContext(configuration, typeof(TDbContext));
    }
    
    public static ISimplyConfiguration AddDbContext(this ISimplyConfiguration configuration, Type dbContextType)
    {
        configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        dbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));

        if (!dbContextType.IsAssignableTo(typeof(DbContext)))
            throw new InvalidOperationException($"Might be assignable to {nameof(DbContext)}");

        configuration.Services.AddTransient(
            typeof(ISimplyDbContextProvider<>).MakeGenericType(configuration.EntityType),
            typeof(SimplyDbContextProvider<,>).MakeGenericType(dbContextType, configuration.EntityType));

        return configuration;
    }
}