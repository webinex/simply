namespace Webinex.Simply.AspNetCore;

public interface ISimplyMapper<TEntity, TResult>
{
    Task<TResult[]> MapAsync(IEnumerable<TEntity> entities);
}