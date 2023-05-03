using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Webinex.Asky;

namespace Webinex.Simply.AspNetCore;

internal class SimplyAspNetCoreInternalService<TEntity, TKey> : ISimplyAspNetCoreInternalService<TEntity, TKey>
{
    private readonly IOptions<JsonSerializerOptions> _jsonOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISimplyAspNetCoreService<TEntity, TKey> _simplyAspNetCore;
    private readonly ISimply<TEntity, TKey> _simply;

    public SimplyAspNetCoreInternalService(
        IOptions<JsonSerializerOptions> jsonOptions,
        IServiceProvider serviceProvider,
        ISimplyAspNetCoreService<TEntity, TKey> simplyAspNetCore,
        ISimply<TEntity, TKey> simply)
    {
        _jsonOptions = jsonOptions;
        _serviceProvider = serviceProvider;
        _simplyAspNetCore = simplyAspNetCore;
        _simply = simply;
    }

    public async Task<TEntity> CreateAsync(MethodInfo method, Stream requestBody)
    {
        using var streamReader = new StreamReader(requestBody);
        var binder = MethodBinder.Of(method)
            .BindServiceProvider(_serviceProvider)
            .BindJson(await streamReader.ReadToEndAsync(), _jsonOptions.Value);

        return await _simplyAspNetCore.CreateAsync(binder);
    }

    public async Task<TEntity> UpdateAsync(TKey key, MethodInfo method, Stream requestBody)
    {
        using var streamReader = new StreamReader(requestBody);
        var entity = await _simply.ByIdAsync(key) ??
                     throw new InvalidOperationException($"{typeof(TEntity).Name} with key {key} not found");

        var binder = MethodBinder.Of(method)
            .BindServiceProvider(_serviceProvider)
            .BindJson(await streamReader.ReadToEndAsync());

        return await _simplyAspNetCore.UpdateAsync(entity, binder);
    }

    public async Task<TEntity> GetAsync(TKey key)
    {
        return await _simplyAspNetCore.GetAsync(key);
    }

    public async Task<TEntity[]> GetAllAsync(FilterRule? filter, SortRule? sorting, PagingRule? paging)
    {
        return await _simplyAspNetCore.GetAllAsync(filter, sorting, paging);
    }

    public async Task DeleteAsync(TKey key)
    {
        var entity = await _simply.ByIdAsync(key) ??
                     throw new InvalidOperationException($"{typeof(TEntity).Name} with key {key} not found");
        await _simplyAspNetCore.DeleteAsync(entity);
    }
}