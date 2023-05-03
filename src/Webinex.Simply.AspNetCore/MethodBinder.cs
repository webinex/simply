using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Webinex.Simply.AspNetCore;

// TODO: s.skalaban, add nullability checks, well-formed exception on incomplete binding
public class MethodBinder
{
    private readonly IDictionary<ParameterInfo, object?> _boundParameters = new Dictionary<ParameterInfo, object?>();
    public MethodInfo MethodInfo { get; }
    public ParameterInfo[] Parameters { get; }
    public IImmutableDictionary<ParameterInfo, object?> BoundParameters => _boundParameters.ToImmutableDictionary();

    internal MethodBinder(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
        Parameters = methodInfo.GetParameters();
    }

    public MethodBinder BindJson(string json, JsonSerializerOptions? options = null)
    {
        var jsonObject = JsonSerializer.Deserialize<JsonObject>(json, options);
        jsonObject = jsonObject ?? throw new InvalidOperationException();

        return BindJsonObject(jsonObject, options);
    }

    public MethodBinder BindJsonObject(JsonObject jsonObject, JsonSerializerOptions? options = null)
    {
        foreach (var jsonProperty in jsonObject)
            BindJsonProperty(jsonProperty.Key, jsonProperty.Value, options);

        return this;
    }

    // TODO: add well-formed exception about deserialization problem
    private void BindJsonProperty(string name, JsonNode? jsonValue, JsonSerializerOptions? options)
    {
        var param = Parameters.FirstOrDefault(x => x.Name!.Equals(name, StringComparison.InvariantCultureIgnoreCase));
       
        if (param == null)
            return;

        _boundParameters[param] = jsonValue?.Deserialize(param.ParameterType, options);
    }

    public MethodBinder BindServiceProvider(IServiceProvider serviceProvider)
    {
        foreach (var param in Parameters)
        {
            var service = serviceProvider.GetService(param.ParameterType);

            if (service != null)
                _boundParameters[param] = service;
        }

        return this;
    }

    public MethodBinder Bind(ParameterInfo parameter, object? value)
    {
        _boundParameters[parameter] = value;
        return this;
    }

    internal async Task<TResult> InvokeAsync<TResult>(object? instance = null)
    {
        return await (Task<TResult>)Invoke(instance)!;
    }

    internal async Task InvokeAsync(object? instance)
    {
        await (Task)Invoke(instance)!;
    }

    private object? Invoke(object? instance)
    {
        return MethodInfo.Invoke(instance, Parameters.Select(x => _boundParameters[x]).ToArray())!;
    }

    internal static MethodBinder Of(MethodInfo method)
    {
        return new MethodBinder(method);
    }
}