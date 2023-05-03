namespace Webinex.Simply.Example;

public class UserLookupDto
{
    public Guid Id { get; }
    public string Name { get; }

    public UserLookupDto(User user)
    {
        Id = user.Id;
        Name = user.Name;
    }
}