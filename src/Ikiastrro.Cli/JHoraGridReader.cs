using System.Text.RegularExpressions;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Cli;

/// <summary>
/// Reads the South-Indian chart grids of a Jagannatha Hora text export
/// (docs/artifacts/reference-charts/*_Jagannatha.txt): for each grid, its chart label ("Rasi",
/// "D-10 (Trd)", "D-2 (US)", …) and which sign each token ("As", "AL", "Su", "GL", …) sits in.
/// Fixed South-Indian layout: top row Pi Ar Ta Ge, left column Aq Cp, right column Cn Le,
/// bottom row Sg Sc Li Vi; the label is written in the centre box.
/// </summary>
public static partial class JHoraGridReader
{
    public sealed record Grid(string Label, IReadOnlyDictionary<string, ZodiacName> SignOf);

    private static readonly ZodiacName[][] Layout =
    [
        [ZodiacName.Pisces, ZodiacName.Aries, ZodiacName.Taurus, ZodiacName.Gemini],
        [ZodiacName.Aquarius, ZodiacName.Cancer],
        [ZodiacName.Capricornus, ZodiacName.Leo],
        [ZodiacName.Sagittarius, ZodiacName.Scorpio, ZodiacName.Libra, ZodiacName.Virgo],
    ];

    /// <summary>"1_Ramakrishnan" -> "Ramakrishnan", from the export's first line (its data path).</summary>
    public static string PersonName(string firstLine) =>
        Regex.Replace(firstLine.Trim().Split('\\', '/').Last(), @"^\d+_", "");

    public static IReadOnlyList<Grid> Read(IReadOnlyList<string> lines)
    {
        var grids = new List<Grid>();
        for (var i = 0; i < lines.Count; i++)
        {
            if (!IsBorder(lines[i])) continue;
            var end = i + 1;
            while (end < lines.Count && !IsBorder(lines[end])) end++;
            if (end >= lines.Count) break;
            if (Parse(lines.Skip(i + 1).Take(end - i - 1).ToList()) is { } grid) grids.Add(grid);
            i = end;
        }
        return grids;
    }

    private static bool IsBorder(string line) => BorderRegex().IsMatch(line.TrimEnd());

    private static Grid? Parse(List<string> body)
    {
        // Rows are separated by lines beginning "|-"; the separator between the two middle rows
        // carries the centre box's text, so its middle part can hold the label.
        var rows = new List<List<string>> { new() };
        var centre = new List<string>();
        foreach (var line in body)
        {
            var parts = line.Trim().Trim('|').Split('|');
            if (line.StartsWith("|-"))
            {
                if (parts.Length == 3) centre.Add(parts[1].Trim());
                rows.Add(new List<string>());
                continue;
            }
            rows[^1].Add(line);
            if (parts.Length == 3) centre.Add(parts[1].Trim());
        }
        if (rows.Count != 4) return null;

        var label = centre.FirstOrDefault(c => LabelRegex().IsMatch(c))
                    ?? centre.FirstOrDefault(c => c == "Rasi");
        if (label is null) return null;

        var signOf = new Dictionary<string, ZodiacName>(StringComparer.Ordinal);
        for (var r = 0; r < 4; r++)
        {
            foreach (var line in rows[r])
            {
                var parts = line.Trim().Trim('|').Split('|');
                var cells = parts.Length == 3 ? new[] { parts[0], parts[2] } : parts;
                if (cells.Length != Layout[r].Length) return null;
                for (var c = 0; c < cells.Length; c++)
                    foreach (var token in cells[c].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        signOf.TryAdd(token, Layout[r][c]);
            }
        }
        return new Grid(label, signOf);
    }

    /// <summary>The project chart type a JHora grid label stands for, or null when the project
    /// has no such chart ("D-81", "D-108", "D-144").</summary>
    public static string? ChartTypeOf(string label, IReadOnlyCollection<string> projectChartTypes)
    {
        if (label == "Rasi") return "D1";
        var m = LabelRegex().Match(label);
        if (!m.Success) return null;
        var code = "D" + m.Groups[1].Value + (m.Groups[2].Value == "US" ? "-US" : "");
        return projectChartTypes.Contains(code) ? code : null;
    }

    [GeneratedRegex(@"^\+-+\+$")]
    private static partial Regex BorderRegex();

    [GeneratedRegex(@"^D-(\d+)(?: \((\w+)\))?$")]
    private static partial Regex LabelRegex();
}
