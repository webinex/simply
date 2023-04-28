using Webinex.Asky;

namespace Webinex.Simply;

public interface ISimply<TEntity, TKey>
{
    Task<TEntity[]> AddRangeAsync(TEntity[] entities);
    Task<TEntity[]> UpdateRangeAsync(TEntity[] entities);
    Task<TEntity[]> ByIdAsync(IEnumerable<TKey> keys);
    Task DeleteRangeAsync(TEntity[] entities);
    Task<TEntity[]> GetAllAsync(FilterRule? filter = null, PagingRule? paging = null, SortRule? sorting = null);
    Task<int> CountAsync(FilterRule? filter = null);
}