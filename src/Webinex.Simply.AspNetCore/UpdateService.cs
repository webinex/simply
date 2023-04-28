namespace Webinex.Simply.AspNetCore;

internal interface IUpdateService<TEntity, TKey>
{
    Task<TEntity> UpdateAsync(TKey key, Stream requestBody);
}

internal class UpdateService<TEntity, TKey> : IUpdateService<TEntity, TKey>
{
    public Task<TEntity> UpdateAsync(TKey key, Stream requestBody)
    {
        var jsonBody = new StreamReader(requestBody);
        return Task.FromResult<TEntity>(default!);
    }
}