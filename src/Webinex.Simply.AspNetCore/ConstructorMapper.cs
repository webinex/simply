using BindingFlags = System.Reflection.BindingFlags;

namespace Webinex.Simply.AspNetCore;

internal class ConstructorMapper<TEntity, TResult> : ISimplyMapper<TEntity, TResult>
{
    public Task<TResult[]> MapAsync(IEnumerable<TEntity> entities)
    {
        var constructor = typeof(TResult).GetConstructor(BindingFlags.Public | BindingFlags.Instance, new[] { typeof(TEntity) });
        constructor = constructor ?? throw new InvalidOperationException(
            $"Unable to public constructor with single parameter of {typeof(TEntity).Name} on type {typeof(TResult).Name}");

        var result = entities.Select(entity => constructor.Invoke(new object?[] { entity })).Cast<TResult>()
            .ToArray();
        return Task.FromResult(result);
    }
}