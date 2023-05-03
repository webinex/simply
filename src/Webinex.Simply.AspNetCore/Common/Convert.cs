using System.ComponentModel;

namespace Webinex.Simply.AspNetCore;

internal static class Convert
{
    public static TKey FromString<TKey>(string value)
    {
        return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(value)!;
    }
}