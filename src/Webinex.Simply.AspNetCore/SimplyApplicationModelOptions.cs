namespace Webinex.Simply.AspNetCore;

// ReSharper disable once UnusedTypeParameter
internal class SimplyApplicationModelOptions<TEntity>
{
    public SimplyApplicationModelOptions(string route, MethodBinding[] methodBindings, GetBinding[] getBindings, bool delete)
    {
        Route = route;
        MethodBindings = methodBindings;
        Delete = delete;
        GetBindings = getBindings;
    }

    public string Route { get; }
    public bool Delete { get; }
    public MethodBinding[] MethodBindings { get; }
    public GetBinding[] GetBindings { get; }

    public GetBinding GetGetBinding(string uid)
    {
        return GetBindings.SingleOrDefault(x => x.Uid == uid)
               ?? throw new InvalidOperationException("Unable to find get binding");
    }

    public MethodBinding GetMethodBinding(string uid)
    {
        return MethodBindings.SingleOrDefault(x => x.Uid == uid)
               ?? throw new InvalidOperationException("Unable to find method binding");
    }
}