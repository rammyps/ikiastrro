using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Web.Components.Charts;

/// <summary>The D1 positions the Astro Facts key-info tables (special tārās, lattā, sphuṭas, sahams, special tithis,
/// malicious divisions) all start from, read once from the D1 key details. <see cref="Complete"/> is false when a point
/// the tables need is missing, and each table then shows its "needs …" note instead of guessing.</summary>
public sealed class KeyInfoInputs
{
    public double Lagna { get; private init; }
    public double? Gulika { get; private init; }
    public IReadOnlyDictionary<PlanetName, double> Grahas { get; private init; } = new Dictionary<PlanetName, double>();
    public bool Complete { get; private init; }

    public static KeyInfoInputs From(IReadOnlyList<ChartKeyDetail> keyDetails)
    {
        double? Lon(string kind, string name) =>
            keyDetails.FirstOrDefault(k => k.PointKind == kind && k.Planet == name)?.NirayanaLongitudeDegrees;

        var grahas = new Dictionary<PlanetName, double>();
        foreach (var planet in Enum.GetValues<PlanetName>())
            if (Lon("Graha", planet.ToString()) is { } lon) grahas[planet] = lon;

        var lagna = Lon("Graha", "Ascendant");
        return new KeyInfoInputs
        {
            Lagna = lagna ?? 0,
            // The sphuṭa formulas use JHora's Gulika, which this app stores under the name "Maandi" (the two
            // upagraha names are swapped against JHora: Ananya, app Māndi 28°22'49" Taurus = JHora Gulika 28°24'30").
            Gulika = Lon("Upagraha", "Maandi"),
            Grahas = grahas,
            Complete = lagna is not null && grahas.Count == 9,
        };
    }

    /// <summary>House number (1–12) of a longitude, whole-sign from the Lagna.</summary>
    public int HouseOf(double longitude) =>
        ((int)(AstroMath.Normalize(longitude) / 30) - (int)(AstroMath.Normalize(Lagna) / 30) + 12) % 12 + 1;
}

public static class KeyInfoFormat
{
    public static string Sign(double longitude)
    {
        var sign = ((ZodiacName)(int)(AstroMath.Normalize(longitude) / 30)).ToString();
        return sign == "Capricornus" ? "Capricorn" : sign;
    }

    public static string Degree(double longitude) => AstroMath.FormatDegreesMinutesSeconds(AstroMath.Normalize(longitude) % 30);

    public static string Star(double longitude)
    {
        var (nak, pada) = AstroMath.GetNakshatraAndPada(longitude);
        return $"{AstroMath.GetNakshatraName(nak)} {pada}";
    }

    public static string Ordinal(int n) => n switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{n}th" };
}
