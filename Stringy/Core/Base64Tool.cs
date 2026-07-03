using System.Text;

namespace Stringy.Core;

/// <summary>UTF-8 aware Base64 encode/decode, matching the browser's btoa/atob + UTF-8 handling.</summary>
public static class Base64Tool
{
    public static string Encode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
    }

    public static string Decode(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var bytes = Convert.FromBase64String(input.Trim());
        return Encoding.UTF8.GetString(bytes);
    }
}
