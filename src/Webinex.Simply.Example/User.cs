namespace Webinex.Simply.Example;

public class User
{
    public Guid Id { get; protected set; }
    public string Email { get; protected set; } = null!;
    public string Name { get; protected set; } = null!;

    protected User()
    {
    }

    public static async Task UpdateAsync(string email, string name)
    {
        
    }

    public static async Task<User> NewAsync(IUniqueEmailPolicy emailPolicy, string email, string name)
    {
        if (await emailPolicy.ExistsAsync(email))
            throw new InvalidOperationException($"User with email {email} already exists.");
        
        return new User { Id = Guid.NewGuid(), Name = name, Email = email.ToUpperInvariant() };
    }
}