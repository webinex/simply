using Microsoft.EntityFrameworkCore;

namespace Webinex.Simply.Example;

public class ExampleDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;

    public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>(user =>
        {
            user.ToTable("Users");
            user.HasKey(x => x.Id);
        });
        
        model.Entity<Company>(company =>
        {
            company.ToTable("Companies");
            company.HasKey(x => x.Id);
        });
    }
}