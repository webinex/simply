using System.Reflection;
using Webinex.Asky;

namespace Webinex.Simply.AspNetCore;

internal interface ISimplyAspNetCoreInternalService<TEntity, TKey>
{
    Task<TEntity> CreateAsync(MethodInfo method, Stream requestBody);
    Task<TEntity> UpdateAsync(TKey key, MethodInfo method, Stream requestBody);
    Task<TEntity> GetAsync(TKey key);
    Task<TEntity[]> GetAllAsync(FilterRule? filter, SortRule? sorting, PagingRule? paging);
    Task DeleteAsync(TKey key);
}