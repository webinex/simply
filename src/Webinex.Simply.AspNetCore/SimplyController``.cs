using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Webinex.Simply.AspNetCore;

// Route is required only for dynamic registration,
// it would not be used and at startup would be changed to provided
[Route("/api/simply/{name}")]
public class SimplyController<TEntity, TKey> : ControllerBase
{
    private ISimplyAspNetCoreInternalService<TEntity, TKey> Simply => HttpContext.RequestServices
        .GetRequiredService<ISimplyAspNetCoreInternalService<TEntity, TKey>>();

    private DbContext DbContext => HttpContext.RequestServices
        .GetRequiredService<ISimplyDbContextProvider<TEntity>>().Value;

    [HttpPost]
    public async Task<TEntity> AddAsync()
    {
        var entity = await Simply.CreateAsync(Request.Body);
        await DbContext.SaveChangesAsync();
        return entity;
    }

    [HttpPut("{key}")]
    public async Task<TEntity> UpdateAsync([FromRoute] string key)
    {
        // TODO: s.skalaban, temp solution
        var keyValue = Guid.Parse(key);
        var entity = await Simply.UpdateAsync(((TKey)(object)keyValue), Request.Body);
        await DbContext.SaveChangesAsync();
        return entity;
    }
}