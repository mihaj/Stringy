using System.Globalization;
using System.Text.RegularExpressions;

namespace Stringy.Core;

/// <summary>RGB &lt;-&gt; HEX conversion (with optional alpha) plus HSL helpers used by the colour wheel.</summary>
public static partial class ColorTool
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static bool ValidateRgb(string rgb)
    {
        var m = RgbPattern().Match(rgb ?? "");
        if (!m.Success) return false;
        int r = int.Parse(m.Groups[1].Value, Inv);
        int g = int.Parse(m.Groups[2].Value, Inv);
        int b = int.Parse(m.Groups[3].Value, Inv);
        return r is >= 0 and <= 255 && g is >= 0 and <= 255 && b is >= 0 and <= 255;
    }

    public static bool ValidateHex(string hex) => HexPattern().IsMatch(hex ?? "");

    public static string RgbToHex(string rgb)
    {
        var m = RgbPattern().Match(rgb ?? "");
        if (!m.Success) throw new FormatException("Invalid RGB format");

        int r = int.Parse(m.Groups[1].Value, Inv);
        int g = int.Parse(m.Groups[2].Value, Inv);
        int b = int.Parse(m.Groups[3].Value, Inv);
        double? a = m.Groups[4].Success ? double.Parse(m.Groups[4].Value, Inv) : null;

        if (r > 255 || g > 255 || b > 255)
            throw new FormatException("RGB values must be between 0 and 255");

        var hex = $"#{r:x2}{g:x2}{b:x2}";
        if (a is not null)
        {
            int alpha = (int)Math.Round(a.Value * 255, MidpointRounding.AwayFromZero);
            hex += alpha.ToString("x2", Inv);
        }
        return hex;
    }

    public static string HexToRgb(string hex)
    {
        var m = HexPattern().Match(hex ?? "");
        if (!m.Success) throw new FormatException("Invalid HEX format");

        var value = m.Groups[1].Value;
        int r = Convert.ToInt32(value.Substring(0, 2), 16);
        int g = Convert.ToInt32(value.Substring(2, 2), 16);
        int b = Convert.ToInt32(value.Substring(4, 2), 16);

        if (value.Length == 8)
        {
            int a = Convert.ToInt32(value.Substring(6, 2), 16);
            double alpha = a / 255.0;
            return $"rgba({r}, {g}, {b}, {alpha.ToString(Inv)})";
        }
        return $"rgb({r}, {g}, {b})";
    }

    // ---- HSL helpers (colour wheel) ----

    public static string RgbToHex(int r, int g, int b) => $"#{r:x2}{g:x2}{b:x2}";

    public static (int R, int G, int B) HslToRgb(double h, double s, double l)
    {
        double r, g, b;
        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = Hue2Rgb(p, q, h + 1.0 / 3);
            g = Hue2Rgb(p, q, h);
            b = Hue2Rgb(p, q, h - 1.0 / 3);
        }
        return ((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
    }

    private static double Hue2Rgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2) return q;
        if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
        return p;
    }

    [GeneratedRegex(@"^rgba?\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)(?:\s*,\s*(\d*\.?\d+)\s*)?\)$")]
    private static partial Regex RgbPattern();

    [GeneratedRegex("^#([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$")]
    private static partial Regex HexPattern();
}
