using System.Text;

namespace Webinex.Simply.AspNetCore;

internal static class StringCaseConverter
{
    public static string PascalCaseToKebabCase(string name)
    {
        var chars = name.Split();
        var stringBuilder = new StringBuilder(chars[0].ToLowerInvariant());

        foreach (var c in chars.Skip(1).ToArray())
        {
            if (c == c.ToUpperInvariant())
            {
                stringBuilder.Append("-");
                stringBuilder.Append(c.ToLowerInvariant());
                continue;
            }

            stringBuilder.Append(c);
        }

        return stringBuilder.ToString();
    }
}