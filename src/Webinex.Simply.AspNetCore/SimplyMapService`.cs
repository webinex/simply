using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Webinex.Simply.AspNetCore;

public interface ISimplyMapService<TEntity>
{
    Task<TEntity> MapJsonToNewAsync(Stream requestBody);
}

internal class SimplyMapService<TEntity> : ISimplyMapService<TEntity>
{
    private readonly IOptions<JsonSerializerOptions> _jsonOptions;
    private readonly IServiceProvider _serviceProvider;

    public SimplyMapService(IOptions<JsonSerializerOptions> jsonOptions, IServiceProvider serviceProvider)
    {
        _jsonOptions = jsonOptions;
        _serviceProvider = serviceProvider;
    }

    public async Task<TEntity> MapJsonToNewAsync(Stream requestBody)
    {
        var jsonBody = await new StreamReader(requestBody).ReadToEndAsync();
        return await MapNewMethodAsync(jsonBody);
    }

    private async Task<TEntity> MapNewMethodAsync(string jsonBody)
    {
        var binder = MethodBinder.Of<TEntity>("NewAsync", BindingFlags.Static | BindingFlags.Public);
        binder.BindJson(jsonBody, _jsonOptions.Value);
        binder.BindServiceProvider(_serviceProvider);
        return await binder.InvokeAsync<TEntity>();
    }
}