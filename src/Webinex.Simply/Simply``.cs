using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Webinex.Asky;

namespace Webinex.Simply;

internal class Simply<TEntity, TKey> : ISimply<TEntity, TKey>
    where TEntity : class
{
    private readonly ISimplyDbContextProvider<TEntity> _dbContextProvider;
    private readonly IAskyFieldMap<TEntity> _fieldMap;

    private DbContext DbContext => _dbContextProvider.Value;
    private DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    private Expression<Func<TEntity, TKey>> KeySelector =>
        SimplyExpressionFactory.KeySelector<TEntity, TKey>(DbContext);

    public Simply(ISimplyDbContextProvider<TEntity> dbContextProvider, IAskyFieldMap<TEntity> fieldMap)
    {
        _dbContextProvider = dbContextProvider;
        _fieldMap = fieldMap;
    }

    public async Task<TEntity[]> AddRangeAsync(TEntity[] entities)
    {
        await DbSet.AddRangeAsync(entities);
        return entities;
    }

    public Task<TEntity[]> UpdateRangeAsync(TEntity[] entities)
    {
        DbSet.UpdateRange(entities);
        return Task.FromResult(entities);
    }

    public async Task<TEntity[]> ByIdAsync(IEnumerable<TKey> keys)
    {
        keys = keys.Distinct().ToArray();
        var predicateExpression = SimplyExpressionFactory.KeyIn<TEntity, TKey>(DbContext, keys);
        var predicate = predicateExpression.Compile();

        var local = DbSet.Local.Where(predicate).ToArray();
        if (local.Length == keys.Count())
            return local;

        return await DbSet.Where(predicateExpression).ToArrayAsync();
    }

    public Task DeleteRangeAsync(TEntity[] entities)
    {
        DbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public async Task<TEntity[]> GetAllAsync(FilterRule? filter = null, SortRule? sorting = null, PagingRule? paging = null)
    {
        var queryable = DbSet.AsQueryable();
        if (filter != null)
            queryable = queryable.Where(_fieldMap, filter);

        if (sorting != null)
            queryable = queryable.SortBy(_fieldMap, sorting);

        if (paging != null && sorting == null)
            queryable = queryable.OrderBy(KeySelector);

        if (paging != null)
            queryable = queryable.PageBy(paging);

        return await queryable.ToArrayAsync();
    }

    public async Task<int> CountAsync(FilterRule? filter = null)
    {
        if (filter == null)
            return await DbSet.CountAsync();
        
        return await DbSet.Where(_fieldMap, filter).CountAsync();
    }
}