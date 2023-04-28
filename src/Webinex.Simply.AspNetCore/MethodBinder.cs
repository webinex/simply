using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Webinex.Simply.AspNetCore;

internal class MethodBinder
{
    private readonly IDictionary<ParameterInfo, object?> _boundParameters = new Dictionary<ParameterInfo, object?>();

    public MethodInfo MethodInfo { get; }
    public ParameterInfo[] Parameters { get; }
    public IImmutableDictionary<ParameterInfo, object?> BoundParameters => _boundParameters.ToImmutableDictionary();

    public MethodBinder(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
        Parameters = methodInfo.GetParameters();
    }

    public MethodBinder BindJsonStream(Stream jsonStream, JsonSerializerOptions? options = null)
    {
        using var reader = new StreamReader(jsonStream);
        var json = reader.ReadToEnd();
        return BindJson(json);
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

    public async Task<TResult> InvokeAsync<TResult>(object? instance = null)
    {
        return await (Task<TResult>)MethodInfo.Invoke(instance, Parameters.Select(x => _boundParameters[x]).ToArray())!;
    }

    public static MethodBinder Of<T>(string name, BindingFlags bindingFlags)
    {
        return Of(typeof(T), name, bindingFlags);
    }

    public static MethodBinder Of(Type type, string name, BindingFlags bindingFlags)
    {
        var methodInfo = type.GetMethod(name, bindingFlags);
        methodInfo = methodInfo ?? throw new InvalidOperationException();
        return new MethodBinder(methodInfo);
    }
}