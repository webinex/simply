using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Webinex.Asky;

namespace Webinex.Simply.AspNetCore;

internal class SimplyController<TEntity, TKey>
{
    private readonly ISimplyAspNetCoreInternalService<TEntity, TKey> _internalService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISimplyDbContextProvider<TEntity> _dbContextProvider;
    private readonly SimplyApplicationModelOptions<TEntity> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAskyFieldMap<TEntity> _fieldMap;

    public SimplyController(
        ISimplyAspNetCoreInternalService<TEntity, TKey> internalService,
        IHttpContextAccessor httpContextAccessor,
        ISimplyDbContextProvider<TEntity> dbContextProvider,
        SimplyApplicationModelOptions<TEntity> options,
        IServiceProvider serviceProvider,
        IAskyFieldMap<TEntity> fieldMap)
    {
        _internalService = internalService;
        _httpContextAccessor = httpContextAccessor;
        _dbContextProvider = dbContextProvider;
        _options = options;
        _serviceProvider = serviceProvider;
        _fieldMap = fieldMap;
    }

    private Stream RequestBody => _httpContextAccessor.HttpContext!.Request.Body;
    private DbContext DbContext => _dbContextProvider.Value;

    public async Task<object> InvokeGetAsync([FromRoute] string key)
    {
        var bindingUid = _httpContextAccessor.HttpContext!.Request.RouteValues["--binding-uid"]!.ToString()!;
        var getBinding = _options.GetGetBinding(bindingUid);
        var entity = await _internalService.GetAsync(Convert.FromString<TKey>(key));

        if (typeof(TEntity) == getBinding.ResponseType)
            return entity!;
        
        var mapperType = typeof(ISimplyMapper<,>).MakeGenericType(typeof(TEntity), getBinding.ResponseType);
        var mapper = _serviceProvider.GetRequiredService(mapperType);

        var result = (Task)mapperType.GetMethod("MapAsync", BindingFlags.Instance | BindingFlags.Public)!
            .Invoke(mapper, new object?[] { new[] { entity } })!;

        await result;

        var value = (IEnumerable)result.GetType().GetProperty("Result")!.GetValue(result)!;
        return value.Cast<object>().First();
    }

    public async Task<object[]> InvokeGetAllAsync(
        [FromQuery(Name = "filterBy")] string? filterJson,
        [FromQuery(Name = "sortBy")] string? sortJson,
        [FromQuery(Name = "pageBy")] string? pagingJson)
    {
        var filter = FilterRule.FromJson(filterJson, _fieldMap);
        var sorting = SortRule.FromJson(sortJson);
        var paging = PagingRule.FromJson(pagingJson);

        var bindingUid = _httpContextAccessor.HttpContext!.Request.RouteValues["--binding-uid"]!.ToString()!;
        var getBinding = _options.GetGetBinding(bindingUid);
        var entities = await _internalService.GetAllAsync(filter, sorting, paging);
        
        
        if (typeof(TEntity) == getBinding.ResponseType)
            return entities.Cast<object>().ToArray();
        
        var mapperType = typeof(ISimplyMapper<,>).MakeGenericType(typeof(TEntity), getBinding.ResponseType);
        var mapper = _serviceProvider.GetRequiredService(mapperType);
        var result = (Task)mapperType.GetMethod("MapAsync", BindingFlags.Instance | BindingFlags.Public)!
            .Invoke(mapper, new object?[] { entities.ToArray() })!;

        await result;

        var value = (IEnumerable)result.GetType().GetProperty("Result")!.GetValue(result)!;
        return value.Cast<object>().ToArray();
    }

    public async Task<TEntity> InvokeCreateAsync()
    {
        var bindingUid = _httpContextAccessor.HttpContext!.Request.RouteValues["--binding-uid"]!.ToString()!;
        var methodBinding = _options.GetMethodBinding(bindingUid);
        var entity = await _internalService.CreateAsync(methodBinding.Method, RequestBody);
        await _dbContextProvider.Value.SaveChangesAsync();
        return entity;
    }

    public async Task<TEntity> InvokeUpdateAsync([FromRoute] string key)
    {
        var bindingUid = _httpContextAccessor.HttpContext!.Request.RouteValues["--binding-uid"]!.ToString()!;
        var methodBinding = _options.GetMethodBinding(bindingUid);
        var entity =
            await _internalService.UpdateAsync(Convert.FromString<TKey>(key), methodBinding.Method, RequestBody);
        await DbContext.SaveChangesAsync();
        return entity;
    }

    [HttpDelete("{key}")]
    public async Task DeleteAsync([FromRoute] string key)
    {
        await _internalService.DeleteAsync(Convert.FromString<TKey>(key));
        await DbContext.SaveChangesAsync();
    }
}