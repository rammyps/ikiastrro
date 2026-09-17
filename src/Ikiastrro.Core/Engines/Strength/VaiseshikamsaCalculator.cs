using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// Raman's Swavarga/Amsa ladder (SRC_RAMAN_300_COMBINATIONS, combination 156 remarks;
/// printed p.156, scan p.168): a planet is "in Swavarga" for any of the 16 classical
/// Shodasa Varga charts where it occupies a sign it owns. The number of such charts maps
/// to a named grade, from Parijatamsa (2) up to Vaiseshikamsa (13) — Raman's own words:
/// "When a planet attains Vaiseshikamsa, it is par excellence."
/// </summary>
public static class VaiseshikamsaCalculator
{
    /// <summary>The 16 Shodasa Varga chart types, per docs/cli/reading/varga-index.md's
    /// "Groups: 16" tags plus D1/D9/D10/D60 (full-guide charts, foundational Shodasavarga
    /// members not repeated in that index).</summary>
    private static readonly string[] ShodasaVarga =
        ["D1", "D2", "D3", "D4", "D7", "D9", "D10", "D12", "D16", "D20", "D24", "D27", "D30", "D40", "D45", "D60"];

    /// <summary>The 6 Shadvarga chart types — confirmed from ShadbalaCalculator.cs's
    /// registered Saptavarga-7 list ({"D1","D2","D3","D7","D9","D12","D30"}) minus D7, the
    /// one chart Saptavarga adds on top of Shadvarga.</summary>
    public static readonly string[] Shadvarga = ["D1", "D2", "D3", "D9", "D12", "D30"];

    private static readonly (int Count, string Name)[] Grades =
    {
        (2, "Parijatamsa"), (3, "Uttamamsa"), (4, "Gopuramsa"), (5, "Simhasanamsa"),
        (6, "Parvatamsa"), (7, "Devalokamsa"), (8, "Kunkumamsa"), (9, "Iravatamsa"),
        (10, "Vaishnavamsa"), (11, "Saivamsa"), (12, "Bhaswadamsa"), (13, "Vaiseshikamsa")
    };

    /// <summary>How many of the 16 Shodasa Varga charts <paramref name="planet"/> occupies
    /// a sign it owns in.</summary>
    public static int SwavargaCount(IReadOnlyList<ChartAnalysisInput> charts, PlanetName planet) =>
        ShodasaVarga.Count(t => OwnsSign(charts, t, planet));

    /// <summary>How many of an arbitrary set of charts (e.g. <see cref="Shadvarga"/>)
    /// <paramref name="planet"/> occupies a sign it owns in.</summary>
    public static int SwavargaCount(IReadOnlyList<ChartAnalysisInput> charts, PlanetName planet, IReadOnlyList<string> chartTypes) =>
        chartTypes.Count(t => OwnsSign(charts, t, planet));

    /// <summary>Raman's named grade for a given Swavarga count, or null below the first
    /// named grade (fewer than 2 own-sign charts).</summary>
    public static string? Grade(int swavargaCount) =>
        Grades.Where(g => swavargaCount >= g.Count).Select(g => g.Name).LastOrDefault();

    /// <summary>True once the planet has attained Vaiseshikamsa itself (13 of 16), the
    /// highest grade Raman names.</summary>
    public static bool HasVaiseshikamsa(IReadOnlyList<ChartAnalysisInput> charts, PlanetName planet) =>
        SwavargaCount(charts, planet) >= 13;

    /// <summary>True once the planet has reached at least <paramref name="minimumGrade"/>
    /// own-sign charts (e.g. Gopuramsa = 4, Simhasanamsa = 5, Parvatamsa = 6).</summary>
    public static bool HasAttained(IReadOnlyList<ChartAnalysisInput> charts, PlanetName planet, int minimumGrade) =>
        SwavargaCount(charts, planet) >= minimumGrade;

    private static bool OwnsSign(IReadOnlyList<ChartAnalysisInput> charts, string chartType, PlanetName planet)
    {
        var chart = charts.FirstOrDefault(c => c.ChartType.Equals(chartType, StringComparison.OrdinalIgnoreCase));
        var position = chart?.Planets.FirstOrDefault(p => p.Planet.Equals(planet.ToString(), StringComparison.OrdinalIgnoreCase));
        if (position is null) return false;
        return Enum.TryParse<ZodiacName>(position.Sign, true, out var sign)
            && Enum.TryParse<PlanetName>(HouseEngine.GetSignLord(sign), out var lord)
            && lord == planet;
    }
}
