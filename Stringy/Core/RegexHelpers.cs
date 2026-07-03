namespace Stringy.Core;

/// <summary>Reference data for the regex tool: ready-made patterns and a token cheatsheet.</summary>
public static class RegexHelpers
{
    public record Pattern(string Label, string Value, string Sample);
    public record Token(string Symbol, string Meaning);

    public static readonly IReadOnlyList<Pattern> Patterns =
    [
        new("Email", @"^[^\s@]+@[^\s@]+\.[^\s@]+$", "name@example.com"),
        new("URL", @"^https?:\/\/[^\s/$.?#].[^\s]*$", "https://example.com/path"),
        new("IPv4", @"^(\d{1,3}\.){3}\d{1,3}$", "192.168.1.1"),
        new("Hex colour", @"^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", "#ff0088"),
        new("Slug", @"^[a-z0-9]+(?:-[a-z0-9]+)*$", "my-post-title"),
        new("Date (YYYY-MM-DD)", @"^\d{4}-\d{2}-\d{2}$", "2026-07-03"),
        new("Time (HH:MM)", @"^([01]\d|2[0-3]):[0-5]\d$", "09:30"),
        new("UUID", @"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", "3f2504e0-4f89-41d3-9a0c-0305e82c3301"),
        new("Digits only", @"^\d+$", "12345"),
        new("Letters only", @"^[A-Za-z]+$", "abcXYZ"),
        new("Alphanumeric", @"^[A-Za-z0-9]+$", "abc123"),
        new("Strong password", @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$", "Aa1!aaaa"),
    ];

    public static readonly IReadOnlyList<Token> Cheatsheet =
    [
        new(".", "any single character"),
        new(@"\d", "a digit (0–9)"),
        new(@"\w", "word char (a–z, 0–9, _)"),
        new(@"\s", "whitespace"),
        new("[abc]", "any of a, b or c"),
        new("[^abc]", "none of a, b, c"),
        new("[a-z]", "range a to z"),
        new("^ $", "start / end of line"),
        new(@"\b", "word boundary"),
        new("*", "0 or more"),
        new("+", "1 or more"),
        new("?", "0 or 1 (optional)"),
        new("{n,m}", "between n and m times"),
        new("(…)", "capture group"),
        new("a|b", "a or b"),
        new(@"\.", "an escaped literal ."),
    ];
}
