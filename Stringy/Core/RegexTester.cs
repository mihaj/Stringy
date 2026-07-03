using System.Text.RegularExpressions;

namespace Stringy.Core;

/// <summary>Tests an input string against a regular expression, mapping JS-style flags to .NET options.</summary>
public static class RegexTester
{
    public record Result(bool Valid, string? Error = null, int MatchCount = 0, string? FirstMatch = null);

    public static Result Validate(string input, string pattern, string? flags = null)
    {
        if (string.IsNullOrEmpty(pattern))
            return new Result(false, "Enter a regex pattern to test.");

        try
        {
            var options = RegexOptions.None;
            if (flags is not null)
            {
                if (flags.Contains('i')) options |= RegexOptions.IgnoreCase;
                if (flags.Contains('m')) options |= RegexOptions.Multiline;
                if (flags.Contains('s')) options |= RegexOptions.Singleline;
                // g (global), u (unicode) and y (sticky) have no bearing on a simple match test.
            }

            var regex = new Regex(pattern, options, TimeSpan.FromSeconds(2));
            var text = input ?? string.Empty;
            var matches = regex.Matches(text);
            var valid = matches.Count > 0;
            var first = valid ? matches[0].Value : null;
            return new Result(valid, null, matches.Count, first);
        }
        catch (RegexParseException ex)
        {
            return new Result(false, $"Invalid pattern: {ex.Message}");
        }
        catch (RegexMatchTimeoutException)
        {
            return new Result(false, "Pattern took too long to evaluate.");
        }
    }
}
