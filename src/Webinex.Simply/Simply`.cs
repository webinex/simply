using Webinex.Asky;

namespace Webinex.Simply;

internal class Simply<TEntity> : ISimply<TEntity>
{
    private readonly ISimply<TEntity, Guid> _inner;

    public Simply(ISimply<TEntity, Guid> inner)
    {
        _inner = inner;
    }

    public Task<TEntity[]> AddRangeAsync(TEntity[] entities)
    {
        return _inner.AddRangeAsync(entities);
    }

    public Task<TEntity[]> UpdateRangeAsync(TEntity[] entities)
    {
        return _inner.UpdateRangeAsync(entities);
    }

    public Task<TEntity[]> ByIdAsync(IEnumerable<Guid> keys)
    {
        return _inner.ByIdAsync(keys);
    }

    public Task DeleteRangeAsync(TEntity[] entities)
    {
        return _inner.DeleteRangeAsync(entities);
    }

    public Task<TEntity[]> GetAllAsync(FilterRule? filter = null, PagingRule? paging = null, SortRule? sorting = null)
    {
        return _inner.GetAllAsync(filter, paging, sorting);
    }

    public Task<int> CountAsync(FilterRule? filter = null)
    {
        return _inner.CountAsync(filter);
    }
}