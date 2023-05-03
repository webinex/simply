namespace Webinex.Simply.Example;

public class UserDto
{
    public Guid Id { get; }
    public string Name { get; }
    public string Status { get; }

    public UserDto(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Status = user.Status;
    }
}