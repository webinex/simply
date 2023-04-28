namespace Webinex.Simply.AspNetCore;

internal interface ISimplyAspNetCoreInternalService<TEntity, TKey>
{
    Task<TEntity> CreateAsync(Stream requestBody);
    Task<TEntity> UpdateAsync(TKey key, Stream requestBody);
}