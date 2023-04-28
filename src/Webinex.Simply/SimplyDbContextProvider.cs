using Microsoft.EntityFrameworkCore;

namespace Webinex.Simply;

internal class SimplyDbContextProvider<TDbContext, TEntity> : ISimplyDbContextProvider<TEntity>
    where TDbContext : DbContext
{
    public SimplyDbContextProvider(TDbContext dbContext)
    {
        Value = dbContext;
    }

    public DbContext Value { get; }
}