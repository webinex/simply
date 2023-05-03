using System.Reflection;

namespace Webinex.Simply.AspNetCore;

public class MethodBinding
{
    public MethodBinding(MethodBindingType type, MethodInfo method, string route, string action)
    {
        Type = type;
        Method = method;
        Route = route;
        Action = action;
        Uid = Guid.NewGuid().ToString("N");
    }

    internal string Uid { get; }
    public string Route { get; }
    public string Action { get; }
    public MethodBindingType Type { get; }
    public MethodInfo Method { get; }
}