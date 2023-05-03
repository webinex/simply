using Webinex.Asky;

namespace Webinex.Simply.AspNetCore;

public interface ISimplyAspNetCoreService<TEntity, TKey>
{
    Task<TEntity> CreateAsync(MethodBinder binder);
    Task<TEntity> UpdateAsync(TEntity entity, MethodBinder binder);
    Task<TEntity> GetAsync(TKey key);
    Task<TEntity[]> GetAllAsync(FilterRule? filter, SortRule? sorting, PagingRule? paging);
    Task DeleteAsync(TEntity entity);
}