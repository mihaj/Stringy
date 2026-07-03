using System.Globalization;

namespace Stringy.Core;

/// <summary>
/// Validates 5-field (standard) cron expressions and renders them as plain English.
/// Covers the common shapes precisely (*, */n, single values, ranges and lists,
/// month/weekday names) and degrades gracefully to a field-by-field readout otherwise.
/// </summary>
public static class CronTool
{
    public record Preset(string Expression, string Label);
    public record Result(bool Ok, string Text, string? Error = null);

    public static readonly IReadOnlyList<Preset> Presets =
    [
        new("* * * * *", "Every minute"),
        new("*/5 * * * *", "Every 5 minutes"),
        new("0 * * * *", "Hourly"),
        new("0 0 * * *", "Daily at midnight"),
        new("30 9 * * 1-5", "09:30 on weekdays"),
        new("0 9 * * 1", "Mondays at 09:00"),
        new("0 0 1 * *", "1st of every month"),
        new("0 0 1 1 *", "Yearly on Jan 1"),
    ];

    private static readonly string[] MonthAbbr =
        ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"];
    private static readonly string[] MonthFull =
        ["January", "February", "March", "April", "May", "June",
         "July", "August", "September", "October", "November", "December"];
    private static readonly string[] DowAbbr =
        ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"];
    private static readonly string[] DowFull =
        ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

    public static Result Describe(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return new Result(false, "");

        var parts = expression.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
            return new Result(false, "", "A cron expression needs exactly 5 fields: minute hour day month weekday.");

        try
        {
            Validate(parts[0], 0, 59, null, 0, "minute");
            Validate(parts[1], 0, 23, null, 0, "hour");
            Validate(parts[2], 1, 31, null, 0, "day-of-month");
            Validate(parts[3], 1, 12, MonthAbbr, 1, "month");
            Validate(parts[4], 0, 7, DowAbbr, 0, "day-of-week");
        }
        catch (FormatException ex)
        {
            return new Result(false, "", ex.Message);
        }

        return new Result(true, Compose(parts));
    }

    // ---- Validation --------------------------------------------------------

    private static void Validate(string field, int min, int max, string[]? names, int nameOffset, string label)
    {
        foreach (var token in field.Split(','))
        {
            var body = token;
            var slash = body.Split('/');
            if (slash.Length == 2)
            {
                if (!int.TryParse(slash[1], out var step) || step <= 0)
                    throw new FormatException($"Invalid step '/{slash[1]}' in {label}.");
                body = slash[0];
            }
            else if (slash.Length != 1)
            {
                throw new FormatException($"Invalid {label} value '{token}'.");
            }

            if (body == "*") continue;

            var dash = body.Split('-');
            if (dash.Length is < 1 or > 2)
                throw new FormatException($"Invalid {label} value '{token}'.");
            foreach (var v in dash)
                ParseValue(v, min, max, names, nameOffset, label);
        }
    }

    private static int ParseValue(string raw, int min, int max, string[]? names, int nameOffset, string label)
    {
        var v = raw.Trim();
        int value;
        if (names is not null && !int.TryParse(v, out _))
        {
            var idx = Array.IndexOf(names, v.ToUpperInvariant());
            if (idx < 0) throw new FormatException($"Unknown {label} name '{raw}'.");
            value = idx + nameOffset;
        }
        else if (!int.TryParse(v, out value))
        {
            throw new FormatException($"Invalid {label} value '{raw}'.");
        }

        if (value < min || value > max)
            throw new FormatException($"{label} value {value} is out of range ({min}–{max}).");
        return value;
    }

    // ---- Description -------------------------------------------------------

    private static string Compose(string[] p)
    {
        string minute = p[0], hour = p[1], dom = p[2], month = p[3], dow = p[4];

        var clauses = new List<string> { DescribeTime(minute, hour) };

        var domClause = DescribeDom(dom);
        var dowClause = DescribeDow(dow);

        if (dom != "*" && dow != "*")
            clauses.Add($"{domClause} or {dowClause}");
        else
        {
            if (domClause.Length > 0) clauses.Add(domClause);
            if (dowClause.Length > 0) clauses.Add(dowClause);
        }

        var monthClause = DescribeMonth(month);
        if (monthClause.Length > 0) clauses.Add(monthClause);

        var text = string.Join(", ", clauses.Where(c => c.Length > 0));
        return char.ToUpperInvariant(text[0]) + text[1..] + ".";
    }

    private static string DescribeTime(string minute, string hour)
    {
        if (minute == "*" && hour == "*") return "every minute";

        var stepM = Step(minute);
        if (stepM is not null && hour == "*")
            return $"every {stepM} minutes";

        var stepH = Step(hour);
        if ((minute == "0" || Single(minute) == 0) && stepH is not null)
            return $"every {stepH} hours";

        var m = Single(minute);
        var h = Single(hour);

        if (m is not null && h is not null)
            return $"at {h:00}:{m:00}";

        if (m is not null && hour == "*")
            return m == 0 ? "every hour, on the hour" : $"at {m} minutes past every hour";

        if (minute == "0" && hour == "*")
            return "every hour, on the hour";

        var hr = RangeVals(hour);
        if (m is not null && hr is not null)
            return $"at {m} minutes past the hour, from {hr.Value.lo:00}:00 to {hr.Value.hi:00}:59";

        if (minute == "*" && h is not null)
            return $"every minute during the {h:00}:00 hour";

        return $"at minute “{minute}” of hour “{hour}”";
    }

    private static string DescribeDom(string dom)
    {
        if (dom == "*") return "";
        var s = Single(dom);
        if (s is not null) return $"on day {s} of the month";
        var step = Step(dom);
        if (step is not null) return $"every {step} days";
        var r = RangeVals(dom);
        if (r is not null) return $"on days {r.Value.lo}–{r.Value.hi} of the month";
        return $"on days {dom} of the month";
    }

    private static string DescribeMonth(string month)
    {
        if (month == "*") return "";
        var r = RangeNames(month, MonthAbbr, MonthFull, 1);
        if (r is not null) return $"in {r}";
        var joined = JoinNames(month, MonthAbbr, MonthFull, 1);
        return joined is not null ? $"in {joined}" : $"in month(s) {month}";
    }

    private static string DescribeDow(string dow)
    {
        if (dow == "*") return "";
        var r = RangeDow(dow);
        if (r is not null)
        {
            if (r.Value.lo == 1 && r.Value.hi == 5) return "only on weekdays";
            return $"{DowFull[r.Value.lo]} through {DowFull[r.Value.hi]}";
        }

        var names = new List<string>();
        foreach (var item in dow.Split(','))
        {
            var v = DowValue(item);
            if (v is null) return $"on weekday “{dow}”";
            names.Add(DowFull[v.Value]);
        }
        return "only on " + HumanList(names);
    }

    // ---- Token helpers -----------------------------------------------------

    private static int? Single(string field) =>
        int.TryParse(field, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;

    private static int? Step(string field)
    {
        var parts = field.Split('/');
        if (parts.Length == 2 && parts[0] == "*" &&
            int.TryParse(parts[1], out var n) && n > 0)
            return n;
        return null;
    }

    private static (int lo, int hi)? RangeVals(string field)
    {
        var parts = field.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out var a) && int.TryParse(parts[1], out var b))
            return (a, b);
        return null;
    }

    private static string? RangeNames(string field, string[] abbr, string[] full, int offset)
    {
        var parts = field.Split('-');
        if (parts.Length != 2) return null;
        var lo = NameOrNumber(parts[0], abbr, offset);
        var hi = NameOrNumber(parts[1], abbr, offset);
        if (lo is null || hi is null) return null;
        return $"{full[lo.Value]} through {full[hi.Value]}";
    }

    private static (int lo, int hi)? RangeDow(string field)
    {
        var parts = field.Split('-');
        if (parts.Length != 2) return null;
        var lo = DowValue(parts[0]);
        var hi = DowValue(parts[1]);
        return lo is not null && hi is not null ? (lo.Value, hi.Value) : null;
    }

    private static string? JoinNames(string field, string[] abbr, string[] full, int offset)
    {
        var names = new List<string>();
        foreach (var item in field.Split(','))
        {
            var idx = NameOrNumber(item, abbr, offset);
            if (idx is null) return null;
            names.Add(full[idx.Value]);
        }
        return HumanList(names);
    }

    // Returns a 0-based index into the full-name array, or null if unrecognised.
    private static int? NameOrNumber(string token, string[] abbr, int offset)
    {
        var t = token.Trim();
        if (int.TryParse(t, out var num)) return num - offset;
        var idx = Array.IndexOf(abbr, t.ToUpperInvariant());
        return idx < 0 ? null : idx;
    }

    // Day-of-week to 0..6 (Sunday..Saturday); accepts 0-7 (7 = Sunday) and names.
    private static int? DowValue(string token)
    {
        var t = token.Trim();
        if (int.TryParse(t, out var num))
            return num is >= 0 and <= 7 ? num % 7 : null;
        var idx = Array.IndexOf(DowAbbr, t.ToUpperInvariant());
        return idx < 0 ? null : idx;
    }

    private static string HumanList(IReadOnlyList<string> items) => items.Count switch
    {
        0 => "",
        1 => items[0],
        2 => $"{items[0]} and {items[1]}",
        _ => string.Join(", ", items.Take(items.Count - 1)) + " and " + items[^1],
    };
}
