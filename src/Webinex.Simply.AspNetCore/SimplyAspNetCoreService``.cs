using Webinex.Asky;

namespace Webinex.Simply.AspNetCore;

internal class SimplyAspNetCoreService<TEntity, TKey> : ISimplyAspNetCoreService<TEntity, TKey>
{
    private readonly ISimply<TEntity, TKey> _simply;

    public SimplyAspNetCoreService(ISimply<TEntity, TKey> simply)
    {
        _simply = simply;
    }

    public async Task<TEntity> CreateAsync(MethodBinder binder)
    {
        return await _simply.AddAsync(await binder.InvokeAsync<TEntity>());
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, MethodBinder binder)
    {
        await binder.InvokeAsync(entity);
        return await _simply.UpdateAsync(entity);
    }

    public async Task<TEntity> GetAsync(TKey key)
    {
        return await _simply.ByIdAsync(key);
    }

    public async Task<TEntity[]> GetAllAsync(FilterRule? filter, SortRule? sorting, PagingRule? paging)
    {
        return await _simply.GetAllAsync(filter, sorting, paging);
    }

    public async Task DeleteAsync(TEntity entity)
    {
        await _simply.DeleteAsync(entity);
    }
}