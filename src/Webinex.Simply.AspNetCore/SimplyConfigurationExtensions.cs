using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Webinex.Simply.AspNetCore;

public static class SimplyConfigurationExtensions
{
    public static ISimplyConfiguration AddController(this ISimplyConfiguration configuration, string route)
    {
        route = route ?? throw new ArgumentNullException(nameof(route));

        RegisterController(configuration);
        AddDynamicRouting(configuration, route);

        configuration.Services.AddTransient(
            typeof(ISimplyMapService<>).MakeGenericType(configuration.EntityType),
            typeof(SimplyMapService<>).MakeGenericType(configuration.EntityType));

        return configuration;
    }

    private static void RegisterController(ISimplyConfiguration configuration)
    {
        var featureProvider = new ControllerRegistrationFeatureProvider(GetControllerType(configuration));
        GetPartManager(configuration.Services).FeatureProviders.Add(featureProvider);
    }

    private static ApplicationPartManager GetPartManager(IServiceCollection services)
    {
        var partManagerDescriptor =
            services.FirstOrDefault(x => x.ServiceType.IsAssignableTo(typeof(ApplicationPartManager)));
        partManagerDescriptor = partManagerDescriptor ??
                                throw new InvalidOperationException(
                                    $"{nameof(AddController)}() might be called after services.AddControllers()");
        return (ApplicationPartManager)partManagerDescriptor.ImplementationInstance;
    }

    private static Type GetControllerType(ISimplyConfiguration configuration)
    {
        return typeof(SimplyController<,>).MakeGenericType(configuration.EntityType, configuration.KeyType);
    }

    private static void AddDynamicRouting(ISimplyConfiguration configuration, string route)
    {
        var routeProviderType = typeof(DynamicRouteProvider<,>).MakeGenericType(configuration.EntityType,
            configuration.KeyType);
        var routeProviderInstance = Activator.CreateInstance(routeProviderType, route);
        configuration.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IApplicationModelProvider),
            routeProviderInstance));
    }

    private class ControllerRegistrationFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly Type _controllerType;

        public ControllerRegistrationFeatureProvider(Type controllerType)
        {
            _controllerType = controllerType ?? throw new ArgumentNullException(nameof(controllerType));
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            if (feature.Controllers.Contains(_controllerType.GetTypeInfo()))
                return;

            feature.Controllers.Add(_controllerType.GetTypeInfo());
        }
    }

    private class DynamicRouteProvider<TEntity, TKey> : IApplicationModelProvider
    {
        private readonly string _route;

        public DynamicRouteProvider(string route)
        {
            _route = route;
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            var serviceControllerModel =
                context.Result.Controllers.FirstOrDefault(c =>
                    c.ControllerType.IsAssignableFrom(typeof(SimplyController<TEntity, TKey>)));

            if (serviceControllerModel == null)
            {
                return;
            }

            var selectorModel = serviceControllerModel.Selectors.FirstOrDefault();
            if (selectorModel is { AttributeRouteModel: { } })
            {
                selectorModel.AttributeRouteModel.Template = _route;
            }
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        public int Order => -1000 + 10;
    }
}