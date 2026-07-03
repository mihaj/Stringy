using System.Text.RegularExpressions;

namespace Stringy.Core;

/// <summary>
/// Text case transformations. Behaviour mirrors the original TypeScript implementation exactly.
/// </summary>
public static partial class StringTransformer
{
    public record Option(string Value, string Label);

    public static readonly IReadOnlyList<Option> Options =
    [
        new("uppercase", "UPPERCASE"),
        new("lowercase", "lowercase"),
        new("camelCase", "camelCase"),
        new("PascalCase", "PascalCase"),
        new("kebab-case", "kebab-case"),
        new("permalink", "permalink"),
    ];

    public static string Transform(string input, string transformation)
    {
        input ??= string.Empty;
        return transformation switch
        {
            "uppercase" => input.ToUpperInvariant(),
            "lowercase" => input.ToLowerInvariant(),
            "camelCase" => ToCamelCase(input),
            "PascalCase" => ToPascalCase(input),
            "kebab-case" => WordBreak().Replace(input.Trim().ToLowerInvariant(), "-"),
            "permalink" => ToPermalink(input),
            _ => input,
        };
    }

    private static string ToCamelCase(string input)
    {
        var words = WordBreak().Split(input.Trim());
        if (words.Length == 0) return string.Empty;
        var first = words[0].ToLowerInvariant();
        var rest = string.Concat(words.Skip(1).Select(Capitalize));
        return first + rest;
    }

    private static string ToPascalCase(string input)
    {
        var words = WordBreak().Split(input.Trim());
        if (words.Length == 0) return string.Empty;
        return string.Concat(words.Select(Capitalize));
    }

    private static string ToPermalink(string input)
    {
        var s = input.Trim().ToLowerInvariant();
        s = StripSpecials().Replace(s, "");        // remove anything that's not a word char, whitespace or dash
        s = SpaceUnderscoreDash().Replace(s, "-"); // collapse spaces/underscores/dashes into a single dash
        s = TrimDashes().Replace(s, "");           // trim leading/trailing dashes
        return s;
    }

    private static string Capitalize(string word) =>
        word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();

    // JS `\w`/`\s` are ASCII-oriented; use explicit ASCII classes to match behaviour precisely.
    [GeneratedRegex(@"[\s_]+")]
    private static partial Regex WordBreak();

    [GeneratedRegex(@"[^A-Za-z0-9_\s-]")]
    private static partial Regex StripSpecials();

    [GeneratedRegex(@"[\s_-]+")]
    private static partial Regex SpaceUnderscoreDash();

    [GeneratedRegex(@"^-+|-+$")]
    private static partial Regex TrimDashes();
}
