namespace Webinex.Simply.AspNetCore;

public class GetBinding
{
    public GetBinding(GetBindingType type, string route, string action, Type responseType)
    {
        Uid = Guid.NewGuid().ToString("N");
        Type = type;
        Route = route;
        Action = action;
        ResponseType = responseType;
    }
    
    public GetBindingType Type { get; }
    public string Route { get; }
    public string Action { get; }
    public Type ResponseType { get; }

    internal string Uid { get; }
}