using System.Collections.Immutable;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Webinex.Simply.AspNetCore;

public class SimplyAspNetControllerConfiguration
{
    private readonly ISimplyConfiguration _configuration;
    private bool _mapDefaultDelete = true;
    private bool? _mapDefaultCreate = null;
    private bool? _mapDefaultUpdate = null;

    private bool _mapDefaultGet = true;
    private Type? _mapDefaultGetResponseType = null;

    private bool _mapDefaultGetMany = true;
    private Type? _mapDefaultGetManyResponseType = null;

    private readonly List<MethodBinding> _methodBindings = new();
    private readonly List<GetBinding> _getBindings = new();

    public SimplyAspNetControllerConfiguration(ISimplyConfiguration configuration)
    {
        _configuration = configuration;
        Route = "/api/" + StringCaseConverter.PascalCaseToKebabCase(_configuration.EntityType.Name);
    }

    public string Route { get; private set; }
    public ImmutableArray<MethodBinding> MethodBindings => _methodBindings.ToImmutableArray();

    public SimplyAspNetControllerConfiguration UseRoute(string route)
    {
        Route = route;
        return this;
    }

    public SimplyAspNetControllerConfiguration MapDefaultDelete(bool value)
    {
        _mapDefaultDelete = value;
        return this;
    }

    public SimplyAspNetControllerConfiguration MapDefaultUpdate(bool? value)
    {
        _mapDefaultUpdate = value;
        return this;
    }

    public SimplyAspNetControllerConfiguration MapDefaultCreate(bool? value)
    {
        _mapDefaultCreate = value;
        return this;
    }

    public SimplyAspNetControllerConfiguration MapDefaultGet(bool value)
    {
        _mapDefaultGet = true;
        return this;
    }

    public SimplyAspNetControllerConfiguration MapDefaultGet<TDto>()
    {
        return MapDefaultGet(typeof(TDto));
    }

    public SimplyAspNetControllerConfiguration MapDefaultGet(Type dtoType)
    {
        _mapDefaultGet = true;
        _mapDefaultGetResponseType = dtoType;
        return this;
    }

    public SimplyAspNetControllerConfiguration MapDefaultGetMany(bool value)
    {
        _mapDefaultGet = true;
        return this;
    }

    public SimplyAspNetControllerConfiguration MapDefaultGetMany<TDto>()
    {
        return MapDefaultGetMany(typeof(TDto));
    }

    public SimplyAspNetControllerConfiguration MapDefaultGetMany(Type dtoType)
    {
        _mapDefaultGetMany = true;
        _mapDefaultGetManyResponseType = dtoType;
        return this;
    }

    public SimplyAspNetControllerConfiguration MapMethod(MethodBinding methodBinding)
    {
        _methodBindings.Add(methodBinding);
        return this;
    }

    public SimplyAspNetControllerConfiguration MapGet<TResult>(string route, string actionName)
    {
        return MapGet(new GetBinding(GetBindingType.Get, route, actionName, typeof(TResult)));
    }

    public SimplyAspNetControllerConfiguration MapGetAll<TResult>(string route, string actionName)
    {
        return MapGet(new GetBinding(GetBindingType.GetAll, route, actionName, typeof(TResult)));
    }

    public SimplyAspNetControllerConfiguration MapGet(GetBinding getBinding)
    {
        _configuration.Services.TryAddSingleton(
            typeof(ISimplyMapper<,>).MakeGenericType(_configuration.EntityType, getBinding.ResponseType),
            typeof(ConstructorMapper<,>).MakeGenericType(_configuration.EntityType, getBinding.ResponseType));
        _getBindings.Add(getBinding);
        return this;
    }

    public SimplyAspNetControllerConfiguration MapCreate(string route, string method)
    {
        if (route.StartsWith("/"))
            throw new InvalidOperationException("Route might not start with `/`");

        var methodInfo = _configuration.EntityType.GetMethod(method, BindingFlags.Static | BindingFlags.Public);
        methodInfo = methodInfo ??
                     throw new InvalidOperationException(
                         $"Unable to find method {method} on entity {_configuration.EntityType.Name}");

        var actionName = methodInfo.Name.EndsWith("Async")
            ? methodInfo.Name.Substring(0, methodInfo.Name.Length - "Async".Length)
            : methodInfo.Name;

        return MapMethod(new MethodBinding(MethodBindingType.Create, methodInfo, route, actionName));
    }

    public SimplyAspNetControllerConfiguration MapUpdate(string route, string method)
    {
        if (route.StartsWith("/"))
            throw new InvalidOperationException("Route might not start with `/`");

        var methodInfo = _configuration.EntityType.GetMethod(method, BindingFlags.Instance | BindingFlags.Public);
        methodInfo = methodInfo ??
                     throw new InvalidOperationException(
                         $"Unable to find method {method} on entity {_configuration.EntityType.Name}");

        var actionName = methodInfo.Name.EndsWith("Async")
            ? methodInfo.Name.Substring(0, methodInfo.Name.Length - "Async".Length)
            : methodInfo.Name;

        return MapMethod(new MethodBinding(MethodBindingType.Update, methodInfo, route, actionName));
    }

    internal void Complete()
    {
        AddDefaultBindings();

        var applicationModelProviderOptionsType =
            typeof(SimplyApplicationModelOptions<>).MakeGenericType(_configuration.EntityType);
        var applicationModelProviderOptionsInstance =
            Activator.CreateInstance(applicationModelProviderOptionsType, Route, _methodBindings.ToArray(),
                _getBindings.ToArray(), _mapDefaultDelete)!;

        _configuration.Services.AddSingleton(
            applicationModelProviderOptionsType,
            applicationModelProviderOptionsInstance);

        var applicationModelProviderType = typeof(SimplyApplicationModelProvider<,>).MakeGenericType(
            _configuration.EntityType,
            _configuration.KeyType);

        _configuration.Services.TryAddEnumerable(
            new ServiceDescriptor(typeof(IApplicationModelProvider), applicationModelProviderType,
                ServiceLifetime.Singleton));

        _configuration.Services.AddTransient(
            typeof(ISimplyAspNetCoreService<,>).MakeGenericType(_configuration.EntityType, _configuration.KeyType),
            typeof(SimplyAspNetCoreService<,>).MakeGenericType(_configuration.EntityType, _configuration.KeyType));

        _configuration.Services.AddTransient(
            typeof(ISimplyAspNetCoreInternalService<,>).MakeGenericType(_configuration.EntityType,
                _configuration.KeyType),
            typeof(SimplyAspNetCoreInternalService<,>).MakeGenericType(_configuration.EntityType,
                _configuration.KeyType));
    }

    private void AddDefaultBindings()
    {
        AddDefaultCreateBinding();
        AddDefaultUpdateBinding();
        AddDefaultGetBinding();
        AddDefaultGetManyBinding();
    }

    private void AddDefaultGetBinding()
    {
        if (!_mapDefaultGet)
            return;

        var responseType = _mapDefaultGetResponseType ?? _configuration.EntityType;

        // TODO: s.skalaban add validation for constructor at the moment of registration

        if (_configuration.EntityType != responseType)
        {
            _configuration.Services.TryAddSingleton(
                typeof(ISimplyMapper<,>).MakeGenericType(_configuration.EntityType, responseType),
                typeof(ConstructorMapper<,>).MakeGenericType(_configuration.EntityType, responseType));
        }

        _getBindings.Add(new GetBinding(GetBindingType.Get, "", "Get", responseType));
    }

    private void AddDefaultGetManyBinding()
    {
        if (!_mapDefaultGetMany)
            return;

        var responseType = _mapDefaultGetManyResponseType ?? _configuration.EntityType;

        // TODO: s.skalaban add validation for constructor at the moment of registration

        if (_configuration.EntityType != responseType)
        {
            _configuration.Services.TryAddSingleton(
                typeof(ISimplyMapper<,>).MakeGenericType(_configuration.EntityType, responseType),
                typeof(ConstructorMapper<,>).MakeGenericType(_configuration.EntityType, responseType));
        }

        _getBindings.Add(new GetBinding(GetBindingType.GetAll, "", "GetAll", responseType));
    }

    private void AddDefaultUpdateBinding()
    {
        if (_mapDefaultUpdate == false)
            return;

        var matchMethod =
            _configuration.EntityType.GetMethod("UpdateAsync", BindingFlags.Instance | BindingFlags.Public);
        if (matchMethod != null)
        {
            _methodBindings.Add(new MethodBinding(MethodBindingType.Update, matchMethod, "", "Update"));
        }
        else if (_mapDefaultUpdate == true)
        {
            throw new InvalidOperationException(
                $"Unable to find `UpdateAsync` method on entity type {_configuration.EntityType.Name}");
        }
    }

    private void AddDefaultCreateBinding()
    {
        if (_mapDefaultCreate == false)
            return;

        var matchMethod = _configuration.EntityType.GetMethod("CreateAsync", BindingFlags.Static | BindingFlags.Public);
        if (matchMethod != null)
        {
            _methodBindings.Add(new MethodBinding(MethodBindingType.Create, matchMethod, "", "Create"));
        }
        else if (_mapDefaultCreate == true)
        {
            throw new InvalidOperationException(
                $"Unable to find `CreateAsync` method on entity type {_configuration.EntityType.Name}");
        }
    }
}