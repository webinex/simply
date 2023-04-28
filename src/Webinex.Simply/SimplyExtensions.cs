namespace Webinex.Simply;

public static class SimplyExtensions
{
    public static async Task<TEntity> AddAsync<TEntity, TKey>(this ISimply<TEntity, TKey> simply, TEntity entity)
    {
        simply = simply ?? throw new ArgumentNullException(nameof(simply));
        var result = await simply.AddRangeAsync(new[] { entity });
        return result.Single();
    }

    public static async Task<TEntity> AddAsync<TEntity>(this ISimply<TEntity, Guid> simply, TEntity entity)
    {
        simply = simply ?? throw new ArgumentNullException(nameof(simply));
        var result = await simply.AddRangeAsync(new[] { entity });
        return result.Single();
    }

    public static async Task<TEntity> UpdateAsync<TEntity, TKey>(this ISimply<TEntity, TKey> simply, TEntity entity)
    {
        simply = simply ?? throw new ArgumentNullException(nameof(simply));
        var result = await simply.UpdateRangeAsync(new[] { entity });
        return result.Single();
    }

    public static async Task<TEntity> UpdateAsync<TEntity>(this ISimply<TEntity, Guid> simply, TEntity entity)
    {
        simply = simply ?? throw new ArgumentNullException(nameof(simply));
        var result = await simply.UpdateRangeAsync(new[] { entity });
        return result.Single();
    }

    public static async Task<TEntity> ByIdAsync<TEntity>(this ISimply<TEntity, Guid> simply, Guid id)
    {
        simply = simply ?? throw new ArgumentNullException(nameof(simply));
        var result = await simply.ByIdAsync(new[] { id });
        return result.Single();
    }

    public static async Task<TEntity> ByIdAsync<TEntity, TKey>(this ISimply<TEntity, TKey> simply, TKey key)
    {
        simply = simply ?? throw new ArgumentNullException(nameof(simply));
        var result = await simply.ByIdAsync(new[] { key });
        return result.Single();
    }
}