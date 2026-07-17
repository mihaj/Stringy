using System.Text;
using Net.Codecrete.QrCodeGenerator;

namespace Stringy.Core;

/// <summary>Renders text as a QR code SVG. Generation is pure managed code, so it runs in the browser/WASM sandbox.</summary>
public static class QrTool
{
    public const int MaxLength = 250;

    public record SizeOption(int Pixels, string Label);

    public record EccOption(string Value, string Label, string Hint);

    public static readonly IReadOnlyList<SizeOption> Sizes =
    [
        new(160, "Small"),
        new(256, "Medium"),
        new(384, "Large"),
    ];

    public static readonly IReadOnlyList<EccOption> EccLevels =
    [
        new("low", "Low", "recovers ~7% damage"),
        new("medium", "Medium", "recovers ~15% damage"),
        new("quartile", "Quartile", "recovers ~25% damage"),
        new("high", "High", "recovers ~30% damage"),
    ];

    public record Result(string Svg, int Modules);

    /// <summary>Builds a scalable SVG: always black-on-white (scanners need the contrast) with a 4-module quiet zone.</summary>
    public static Result Generate(string text, string ecc)
    {
        var qr = QrCode.EncodeText(text, ParseEcc(ecc));
        const int border = 4;
        var span = qr.Size + border * 2;

        var path = new StringBuilder();
        for (var y = 0; y < qr.Size; y++)
        {
            for (var x = 0; x < qr.Size; x++)
            {
                if (!qr.GetModule(x, y)) continue;
                var run = 1;
                while (x + run < qr.Size && qr.GetModule(x + run, y)) run++;
                path.Append($"M{x + border},{y + border}h{run}v1h-{run}z");
                x += run - 1;
            }
        }

        var svg = $"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {span} {span}" shape-rendering="crispEdges" role="img" aria-label="QR code"><rect width="{span}" height="{span}" fill="#ffffff"/><path d="{path}" fill="#000000"/></svg>""";
        return new Result(svg, qr.Size);
    }

    /// <summary>Data URL for a download link, with explicit pixel dimensions stamped on the SVG.</summary>
    public static string ToDownloadUrl(string svg, int pixels)
    {
        var sized = svg.Replace("<svg ", $"""<svg width="{pixels}" height="{pixels}" """);
        return "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(sized));
    }

    private static QrCode.Ecc ParseEcc(string value) => value switch
    {
        "low" => QrCode.Ecc.Low,
        "medium" => QrCode.Ecc.Medium,
        "quartile" => QrCode.Ecc.Quartile,
        "high" => QrCode.Ecc.High,
        _ => throw new ArgumentException($"Unsupported error correction level: {value}"),
    };
}
