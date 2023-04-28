using Microsoft.EntityFrameworkCore;

namespace Webinex.Simply;

public interface ISimplyDbContextProvider<TEntity>
{
    DbContext Value { get; }
}