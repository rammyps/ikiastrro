using Ikiastrro.Data.Statistics;

namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>A strength figure put into words: "Moderate support" / "Around the ordinary midpoint".</summary>
public sealed record SupportBand(string Label, string Meaning, string Tone);

/// <summary>One signal behind a strength figure, in customer copy with its technical name kept
/// for the details view. <see cref="Percent"/> is on the same 0–100 scale as the figure itself
/// (50 = the ordinary middle); null when the chart does not measure it.</summary>
public sealed record StrengthFactor(string Name, string Technical, int? Percent, string Detail);

/// <summary>
/// Customer-facing reading of the Key Inference statistics (docs/ui/components/specs_key_inference_page.md
/// "Customer flow"): support bands, the strongest and limiting factors, and which lagna
/// perspectives and matters show before "more". Presentation only — every number comes from
/// <see cref="LifeMatterStatistics"/>.
/// </summary>
public static class LifeMatterReading
{
    /// <summary>Matters shown before "More questions" (the rest stay one click away).</summary>
    public const int PrimaryMatterCount = 5;

    /// <summary>The perspectives shown by default — the rest sit under "Advanced perspectives".</summary>
    public static readonly IReadOnlyList<(string ReferenceCode, string Name)> PrimaryPerspectives =
    [
        ("LAGNA", "Overall"),
        ("CHANDRA_LAGNA", "Mind"),
        ("RAVI_LAGNA", "Soul"),
        ("ARUDHA_LAGNA", "Public image"),
    ];

    public static bool IsPrimaryPerspective(string referenceCode) =>
        PrimaryPerspectives.Any(p => p.ReferenceCode == referenceCode);

    /// <summary>Customer name of a perspective: "Overall" for Lagna, else the lagna's own label.</summary>
    public static string PerspectiveName(LifeMatterQuestion question) =>
        PrimaryPerspectives.FirstOrDefault(p => p.ReferenceCode == question.ReferenceCode).Name ?? question.Label;

    // The figure is an index where 50% is the ordinary middle (stat_strength.md §4). These word
    // bands are the app's own display copy, not classical cut-offs: ±5 points around the middle
    // reads "moderate", beyond ±15 "strong"/"weak".
    public static SupportBand Band(int? percent) => percent switch
    {
        null => new("Not measured", "No figure in this chart", "none"),
        >= 65 => new("Strong support", "Well above the ordinary midpoint", "strong"),
        >= 55 => new("Good support", "Above the ordinary midpoint", "good"),
        >= 45 => new("Moderate support", "Around the ordinary midpoint", "middle"),
        >= 35 => new("Limited support", "Below the ordinary midpoint", "limited"),
        _ => new("Weak support", "Well below the ordinary midpoint", "weak"),
    };

    /// <summary>The six signals behind one or more houses' figures, averaged across them.</summary>
    public static IReadOnlyList<StrengthFactor> Factors(IReadOnlyList<HouseStatistics> houses, string chartType)
    {
        if (houses.Count == 0) return [];
        var first = houses[0];
        var planets = string.Join(", ", houses.SelectMany(h => h.Planets.Select(p => p.Planet)).Distinct());
        var lords = string.Join(", ", houses.Select(h => h.LordPlanet).Distinct());
        int? Avg(Func<HouseStatistics, int?> pick) => LifeMatterStatistics.Mean(houses.Select(pick));

        return
        [
            new("Strength of the ruling planets", "Ṣaḍbala (Capacity)", Avg(h => h.Capacity), planets),
            new("Consistency across the divisional charts", "Shodasavarga Amsabala (Consistency)", Avg(h => h.Consistency), planets),
            new("Support the house receives", "Sarvashtakavarga (SAV)", Avg(h => h.ContextParts[0]),
                first.SavBindus is { } sav ? $"{sav} of 56 bindus" : "not computed"),
            new("Ruler's comfort in the house", "Lord's Bhinnashtakavarga (BAV)", Avg(h => h.ContextParts[1]),
                first.LordBavBindus is { } bav ? $"{lords}: {bav} of 8 bindus" : lords),
            new("Strength of the house itself", "Bhava Bala (Dig + Drik)", Avg(h => h.ContextParts[2]),
                chartType == "D1" ? "against this chart's 12 houses" : "measured in the main chart (D1) only"),
            new("Help or hindrance from other planets", "Argala / Virodhargala", Avg(h => h.ContextParts[3]),
                first.Argala.Any ? $"{first.Argala.Holds} helping, {first.Argala.Obstructed} obstructed" : "no planet intervenes"),
        ];
    }

    /// <summary>The two highest measured factors at or above the midpoint.</summary>
    public static IReadOnlyList<StrengthFactor> Strongest(IReadOnlyList<StrengthFactor> factors) =>
        factors.Where(f => f.Percent >= 50).OrderByDescending(f => f.Percent).Take(2).ToList();

    /// <summary>The two lowest measured factors below the midpoint; when fewer than two, the
    /// unmeasured ones fill in as "uncertain".</summary>
    public static IReadOnlyList<StrengthFactor> Limiting(IReadOnlyList<StrengthFactor> factors) =>
        factors.Where(f => f.Percent < 50).OrderBy(f => f.Percent)
            .Concat(factors.Where(f => f.Percent is null))
            .Take(2).ToList();
}
