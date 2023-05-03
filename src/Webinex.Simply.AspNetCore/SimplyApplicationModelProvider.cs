using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace Webinex.Simply.AspNetCore;

internal class SimplyApplicationModelProvider<TEntity, TKey> : IApplicationModelProvider
{
    private readonly SimplyApplicationModelOptions<TEntity> _options;
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public SimplyApplicationModelProvider(
        SimplyApplicationModelOptions<TEntity> options,
        IModelMetadataProvider modelMetadataProvider)
    {
        _options = options;
        _modelMetadataProvider = modelMetadataProvider;
    }

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
    }

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
        var controllerModel = CreateControllerModel();

        context.Result.Controllers.Add(controllerModel);
        controllerModel.Application = context.Result;

        AddActionModels(controllerModel);
    }

    private ControllerModel CreateControllerModel()
    {
        return new ControllerModel(typeof(SimplyController<TEntity, TKey>).GetTypeInfo(), new[]
        {
            new RouteAttribute(_options.Route),
        })
        {
            ControllerName = typeof(TEntity).Name,
            Selectors =
            {
                new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = _options.Route,
                    },
                },
            },
        };
    }

    private void AddActionModels(ControllerModel controllerModel)
    {
        var controllerType = controllerModel.ControllerType;

        if (_options.Delete)
        {
            AddActionModel(controllerModel, "{key}", HttpMethod.Delete, "Delete", false,
                controllerType.GetMethod("DeleteAsync")!, null);
        }

        foreach (var binding in _options.MethodBindings)
            AddMethodBinding(controllerModel, binding);

        foreach (var binding in _options.GetBindings)
            AddGetBinding(controllerModel, binding);
    }

    private void AddGetBinding(ControllerModel controllerModel, GetBinding binding)
    {
        switch (binding.Type)
        {
            case GetBindingType.Get:
                AddGetOneBinding(controllerModel, binding);
                break;
            
            case GetBindingType.GetAll:
                AddGetManyBinding(controllerModel, binding);
                break;
            
            default:
                throw new InvalidOperationException($"Unable to bind get binding of type {binding.Type}");
        }
    }

    private void AddGetManyBinding(ControllerModel controllerModel, GetBinding binding)
    {
        var controllerType = controllerModel.ControllerType;
        AddActionModel(controllerModel, binding.Route, HttpMethod.Get, binding.Action, false,
            controllerType.GetMethod("InvokeGetAllAsync")!, binding.Uid);
    }

    private void AddGetOneBinding(ControllerModel controllerModel, GetBinding binding)
    {
        var controllerType = controllerModel.ControllerType;
        var route = "{key}" + (string.IsNullOrWhiteSpace(binding.Route) ? string.Empty : "/" + binding.Route);
        AddActionModel(controllerModel, route, HttpMethod.Get, binding.Action, false,
            controllerType.GetMethod("InvokeGetAsync")!, binding.Uid);
    }

    private void AddMethodBinding(ControllerModel controllerModel, MethodBinding binding)
    {
        switch (binding.Type)
        {
            case MethodBindingType.Create:
                AddCreateActionBinding(controllerModel, binding);
                break;
            case MethodBindingType.Update:
                AddUpdateActionBinding(controllerModel, binding);
                break;
            default:
                throw new InvalidOperationException($"Unknown binding type {binding.Type}");
        }
    }

    private void AddCreateActionBinding(ControllerModel controllerModel, MethodBinding binding)
    {
        AddActionModel(controllerModel, binding.Route, HttpMethod.Post, binding.Action, true,
            controllerModel.ControllerType.GetMethod("InvokeCreateAsync")!, binding.Uid);
    }

    private void AddUpdateActionBinding(ControllerModel controllerModel, MethodBinding binding)
    {
        var route = "{key}";
        if (!string.IsNullOrWhiteSpace(binding.Route))
            route += $"/{binding.Route}";

        AddActionModel(controllerModel, route, HttpMethod.Put, binding.Action, true,
            controllerModel.ControllerType.GetMethod("InvokeUpdateAsync")!, binding.Uid);
    }

    private void AddActionModel(
        ControllerModel controllerModel,
        string route,
        HttpMethod httpMethod,
        string actionName,
        bool consumesJson,
        MethodInfo methodInfo,
        string? bindingUid)
    {
        var actionModel = CreateActionModel(route, httpMethod, actionName, consumesJson, methodInfo, bindingUid);
        actionModel.Controller = controllerModel;
        controllerModel.Actions.Add(actionModel);
    }

    private ActionModel CreateActionModel(
        string route,
        HttpMethod httpMethod,
        string actionName,
        bool consumesJson,
        MethodInfo methodInfo,
        string? bindingUid)
    {
        var attributes = methodInfo.GetCustomAttributes(inherit: true);
        var actionModel = new ActionModel(methodInfo, attributes)
        {
            ActionName = actionName,
            Selectors =
            {
                new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = route,
                    },

                    ActionConstraints =
                    {
                        new HttpMethodActionConstraint(new[] { httpMethod.Method }),
                    },

                    EndpointMetadata = { new HttpMethodMetadata(new[] { httpMethod.Method }) },
                },
            },
        };

        if (bindingUid != null)
        {
            actionModel.RouteValues["--binding-uid"] = bindingUid;
        }

        var selector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = route,
            },
        };

        if (consumesJson)
        {
            selector.ActionConstraints.Add(new ConsumesAttribute("application/json"));
        }

        var parameterModels = methodInfo.GetParameters().Select(CreateParameterModel).ToArray();
        foreach (var parameterModel in parameterModels)
        {
            parameterModel.Action = actionModel;
            actionModel.Parameters.Add(parameterModel);
        }

        return actionModel;
    }

    internal ParameterModel CreateParameterModel(ParameterInfo parameterInfo)
    {
        ArgumentNullException.ThrowIfNull(parameterInfo);

        var attributes = parameterInfo.GetCustomAttributes(inherit: true);

        BindingInfo? bindingInfo;
        if (_modelMetadataProvider is ModelMetadataProvider modelMetadataProviderBase)
        {
            var modelMetadata = modelMetadataProviderBase.GetMetadataForParameter(parameterInfo);
            bindingInfo = BindingInfo.GetBindingInfo(attributes, modelMetadata);
        }
        else
        {
            // GetMetadataForParameter should only be used if the user has opted in to the 2.1 behavior.
            bindingInfo = BindingInfo.GetBindingInfo(attributes);
        }

        var parameterModel = new ParameterModel(parameterInfo, attributes)
        {
            ParameterName = parameterInfo.Name!,
            BindingInfo = bindingInfo,
        };

        return parameterModel;
    }

    public int Order => -1000 + 10;
}