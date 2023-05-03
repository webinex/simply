namespace Webinex.Simply.Example;

public class User
{
    public Guid Id { get; protected set; }
    public string Email { get; protected set; } = null!;
    public string Name { get; protected set; } = null!;
    public string Status { get; protected set; } = null!;

    protected User()
    {
    }

    public async Task UpdateStatusAsync(StatusPolicy statusPolicy, string status)
    {
        await statusPolicy.VerifyAllowedAsync(status);
    
        Status = status;
    }
    
    public Task UpdateAsync(string name)
    {
        Name = name;
        return Task.CompletedTask;
    }

    public static async Task<User> CreateInactiveAsync(IUniqueEmailPolicy emailPolicy, string email, string name)
    {
        email = email.ToLowerInvariant();
    
        if (await emailPolicy.ExistsAsync(email))
            throw new InvalidOperationException($"User with email {email} already exists.");
        
        return new User { Id = Guid.NewGuid(), Name = name, Email = email, Status = "Inactive" };
    }

    public static async Task<User> CreateAsync(IUniqueEmailPolicy emailPolicy, string email, string name)
    {
        email = email.ToLowerInvariant();

        if (await emailPolicy.ExistsAsync(email))
            throw new InvalidOperationException($"User with email {email} already exists.");
        
        return new User { Id = Guid.NewGuid(), Name = name, Email = email, Status = "Active" };
    }
}