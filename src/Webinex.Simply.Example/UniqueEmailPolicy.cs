using Microsoft.EntityFrameworkCore;

namespace Webinex.Simply.Example;

public interface IUniqueEmailPolicy
{
    Task<bool> ExistsAsync(string email);
}

public class UniqueEmailPolicy : IUniqueEmailPolicy
{
    private readonly ExampleDbContext _dbContext;

    public UniqueEmailPolicy(ExampleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(string email)
    {
        return _dbContext.Users.AnyAsync(x => x.Email == email);
    }
}