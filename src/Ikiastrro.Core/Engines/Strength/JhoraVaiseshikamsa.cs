using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// Jagannatha Hora's Vaiseshikamsa tiers over the Dasa Varga (10) and Shodasa Varga (16) groups.
/// Unlike Raman's Swavarga ladder (<see cref="VaiseshikamsaCalculator"/>, own sign only), JHora counts a
/// varga whenever the graha sits in its own OR exaltation sign. Confirmed against JHora's
/// "Copy complete calculations" export for RamakrishnanP.jhd — all 14 Dasa/Shodasa tiers for the seven
/// classical grahas match (JhoraVaiseshikamsaGoldenTests). Rahu/Ketu are not covered: JHora's node
/// own/exalted signs are a Preferences option and the observed counts fit no single convention yet.
/// Tier names are the classical Dasavarga / Shodasavarga ladders, as JHora prints them.
/// </summary>
public static class JhoraVaiseshikamsa
{
    /// <summary>JHora's Dasa Varga (10) group.</summary>
    public static readonly string[] DasaVarga = ["D1", "D2", "D3", "D7", "D9", "D10", "D12", "D16", "D30", "D60"];

    /// <summary>JHora's Shodasa Varga (16) group — same 16 charts as <see cref="VaiseshikamsaCalculator"/>.</summary>
    public static readonly string[] ShodasaVarga =
        ["D1", "D2", "D3", "D4", "D7", "D9", "D10", "D12", "D16", "D20", "D24", "D27", "D30", "D40", "D45", "D60"];

    private static readonly string[] DasaNames =
        ["Paarijaata", "Uttama", "Gopura", "Simhasana", "Paaraavata", "Devaloka", "Brahmaloka", "Sakravahana", "Sridhama"];

    private static readonly string[] ShodasaNames =
    [
        "Bhedaka", "Kusuma", "Nagapurusha", "Kanduka", "Kerala", "Kalpavriksha", "ChandanaVana", "Poornachandra",
        "Uchchaisravas", "Dhanvantari", "Suryakanta", "Vidruma", "Indrasimhasana", "Goloka", "Sri Vallabha"
    ];

    /// <summary>Number of charts in <paramref name="group"/> where the graha is in its own or exaltation sign.</summary>
    public static int Count(IReadOnlyList<ChartAnalysisInput> charts, PlanetName planet, IReadOnlyList<string> group) =>
        Matching(charts, planet, group).Count;

    /// <summary>The charts of <paramref name="group"/> where the graha is in its own or exaltation sign, in group order.</summary>
    public static IReadOnlyList<string> Matching(IReadOnlyList<ChartAnalysisInput> charts, PlanetName planet, IReadOnlyList<string> group) =>
        group.Where(t => InOwnOrExaltedSign(charts, t, planet)).ToList();

    /// <summary>"N-Name" as JHora prints it (e.g. "6-Kerala"), or null below 2 charts.</summary>
    public static string? DasaTier(int count) => Tier(count, DasaNames);

    public static string? ShodasaTier(int count) => Tier(count, ShodasaNames);

    private static string? Tier(int count, string[] names) =>
        count >= 2 && count - 2 < names.Length ? $"{count}-{names[count - 2]}" : null;

    private static bool InOwnOrExaltedSign(IReadOnlyList<ChartAnalysisInput> charts, string chartType, PlanetName planet)
    {
        var chart = charts.FirstOrDefault(c => c.ChartType.Equals(chartType, StringComparison.OrdinalIgnoreCase));
        var position = chart?.Planets.FirstOrDefault(p => p.Planet.Equals(planet.ToString(), StringComparison.OrdinalIgnoreCase));
        if (position is null || !Enum.TryParse<ZodiacName>(position.Sign, true, out var sign)) return false;
        if (Enum.TryParse<PlanetName>(HouseEngine.GetSignLord(sign), out var lord) && lord == planet) return true;
        return AstroMath.DeepExaltationPoints.TryGetValue(planet, out var exaltation) && exaltation.Sign == sign;
    }
}
