namespace Webinex.Simply.Example;

public class Company
{
    public int Id { get; protected set; }
    public string Name { get; protected set; } = null!;

    protected Company()
    {
    }
}