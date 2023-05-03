namespace Webinex.Simply.Example;

public class StatusPolicy
{
    public Task VerifyAllowedAsync(string status)
    {
        if (status == "NotAllowed")
            throw new InvalidOperationException();
        
        return Task.CompletedTask;
    }
}