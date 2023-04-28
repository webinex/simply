using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Webinex.Simply.AspNetCore;

internal class SimplyAspNetCoreInternalService<TEntity, TKey> : ISimplyAspNetCoreInternalService<TEntity, TKey>
{
    private readonly IOptions<JsonSerializerOptions> _jsonOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISimply<TEntity, TKey> _simply;

    public SimplyAspNetCoreInternalService(
        IOptions<JsonSerializerOptions> jsonOptions,
        IServiceProvider serviceProvider,
        ISimply<TEntity, TKey> simply)
    {
        _jsonOptions = jsonOptions;
        _serviceProvider = serviceProvider;
        _simply = simply;
    }

    public async Task<TEntity> CreateAsync(Stream requestBody)
    {
        var binder = MethodBinder.Of<TEntity>("NewAsync", BindingFlags.Static | BindingFlags.Public);

        var entity = await binder
            .BindJsonStream(requestBody, _jsonOptions.Value)
            .BindServiceProvider(_serviceProvider)
            .InvokeAsync<TEntity>();

        return await _simply.AddAsync(entity);
    }

    public async Task<TEntity> UpdateAsync(TKey key, Stream requestBody)
    {
        var binder = MethodBinder.Of<TEntity>("UpdateAsync", BindingFlags.Static | BindingFlags.Public);
        var entity = await _simply.ByIdAsync(key) ??
                     throw new InvalidOperationException($"{typeof(TEntity).Name} with key {key} not found");

        await binder
            .BindJsonStream(requestBody, _jsonOptions.Value)
            .BindServiceProvider(_serviceProvider)
            .InvokeAsync<TEntity>(entity);

        return await _simply.UpdateAsync(entity);
    }
}