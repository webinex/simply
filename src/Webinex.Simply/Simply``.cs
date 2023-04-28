using Webinex.Asky;

namespace Webinex.Simply;

internal class Simply<TEntity, TKey> : ISimply<TEntity, TKey>
    where TEntity : class
{
    private readonly ISimplyDbContextProvider<TEntity> _dbContextProvider;

    public Simply(ISimplyDbContextProvider<TEntity> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<TEntity[]> AddRangeAsync(TEntity[] entities)
    {
        await _dbContextProvider.Value.Set<TEntity>().AddRangeAsync(entities);
        return entities;
    }

    public Task<TEntity[]> UpdateRangeAsync(TEntity[] entities)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity[]> ByIdAsync(IEnumerable<TKey> keys)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRangeAsync(TEntity[] entities)
    {
        throw new NotImplementedException();
    }

    public Task<TEntity[]> GetAllAsync(FilterRule? filter = null, PagingRule? paging = null, SortRule? sorting = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(FilterRule? filter = null)
    {
        throw new NotImplementedException();
    }
}