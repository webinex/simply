namespace Webinex.Simply.AspNetCore;

public static class SimplyConfigurationExtensions
{
    public static ISimplyConfiguration AddController(this ISimplyConfiguration configuration)
    {
        return AddController(configuration, x => {});
    }

    public static ISimplyConfiguration AddController(this ISimplyConfiguration configuration, string route)
    {
        return AddController(configuration, x => x.UseRoute(route));
    }

    public static ISimplyConfiguration AddController(
        this ISimplyConfiguration configuration,
        Action<SimplyAspNetControllerConfiguration> configure)
    {
        var config = new SimplyAspNetControllerConfiguration(configuration);
        configure(config);
        config.Complete();
        
        return configuration;
    }
}