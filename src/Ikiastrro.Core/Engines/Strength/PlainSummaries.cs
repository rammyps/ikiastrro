namespace Ikiastrro.Core.Engines.Strength;

// Plain-English readings for the Astro Facts tables, written from the chart's own numbers (Dignity,
// Planet strength, House strength, Yogas). Same idea as DispositorSummary: the method is the table's,
// the "what it means" wording is the project's own synthesis, not a quotation from a classical text.

public sealed record DignityFact(
    string Planet, string? Dignity, int? HouseFromLagna, bool IsCombust, bool IsRetrograde);

public static class DignitySummary
{
    private static readonly string[] Classical = { "Sun", "Moon", "Mars", "Mercury", "Jupiter", "Venus", "Saturn" };
    private static readonly string[] Strong = { "Exalted", "Moolatrikona", "Own Sign" };
    private static readonly string[] Weak = { "Debilitated", "Enemy", "Great Enemy" };

    public static IReadOnlyList<string> Build(IReadOnlyList<DignityFact> facts)
    {
        var lines = new List<string>();
        var seven = facts.Where(f => Classical.Contains(f.Planet) && f.Dignity is not null).ToList();
        if (seven.Count == 0) return lines;

        var strong = seven.Where(f => Strong.Contains(f.Dignity!)).ToList();
        var weak = seven.Where(f => Weak.Contains(f.Dignity!)).ToList();
        var middle = seven.Count - strong.Count - weak.Count;

        lines.Add($"Of the {seven.Count} classical planets, {strong.Count} are at home or exalted, {weak.Count} are uncomfortable, and {middle} are in friendly or neutral signs.");
        if (strong.Count > 0)
            lines.Add($"Comfortable (own sign, moolatrikona or exalted): {List(strong)}. These deliver their results most easily.");
        var debilitated = seven.Where(f => f.Dignity == "Debilitated").ToList();
        if (debilitated.Count > 0)
            lines.Add($"Debilitated: {List(debilitated)}. Debilitated planets work harder to give results; the Dispositors table shows whether the weakness is repaired.");
        var enemy = seven.Where(f => f.Dignity is "Enemy" or "Great Enemy").ToList();
        if (enemy.Count > 0)
            lines.Add($"In an enemy's sign: {List(enemy)}. Results come with friction.");

        var combust = facts.Where(f => f.IsCombust).Select(f => f.Planet).ToList();
        if (combust.Count > 0)
            lines.Add($"Combust (too close to the Sun, results dimmed): {string.Join(", ", combust)}.");
        var retro = facts.Where(f => f.IsRetrograde && f.Planet is not ("Rahu" or "Ketu")).Select(f => f.Planet).ToList();
        if (retro.Count > 0)
            lines.Add($"Retrograde (energy turned inward): {string.Join(", ", retro)}.");

        if (seven.Count(f => f.Dignity is "Debilitated" or "Enemy" or "Great Enemy") > seven.Count / 2)
            lines.Add("Most planets are in unfriendly signs, so this chart leans on strength and yogas elsewhere to deliver.");
        return lines;
    }

    private static string List(IEnumerable<DignityFact> facts)
        => string.Join(", ", facts.Select(f => f.HouseFromLagna is { } h ? $"{f.Planet} (house {h})" : f.Planet));
}

public sealed record PlanetStrengthFact(string Planet, decimal Rupas, decimal? MinimumRupas, decimal? PercentOfMinimum);

public static class PlanetStrengthSummary
{
    public static IReadOnlyList<string> Build(IReadOnlyList<PlanetStrengthFact> facts, string? lagnaLord = null)
    {
        var lines = new List<string>();
        var ranked = facts.Where(f => f.PercentOfMinimum is not null).OrderByDescending(f => f.PercentOfMinimum).ToList();
        if (ranked.Count == 0) return lines;

        var meets = ranked.Where(f => f.PercentOfMinimum >= 100m).ToList();
        lines.Add($"{meets.Count} of {ranked.Count} planets reach the strength they need (100% of their required minimum).");
        var top = ranked[0];
        var bottom = ranked[^1];
        lines.Add($"Strongest: {top.Planet} at {top.PercentOfMinimum:0}% — the planet most able to deliver its results.");
        lines.Add($"Weakest: {bottom.Planet} at {bottom.PercentOfMinimum:0}%"
                  + (StrengthBands.ShadbalaPercentOfMinimum.Classify(bottom.PercentOfMinimum) == StrengthTier.Weak
                      ? " — below the moderate band, so its themes need support from elsewhere."
                      : " — still within an acceptable range."));
        var weak = ranked.Where(f => StrengthBands.ShadbalaPercentOfMinimum.Classify(f.PercentOfMinimum) == StrengthTier.Weak).ToList();
        if (weak.Count > 1)
            lines.Add($"Planets in the weak band (under 80%): {string.Join(", ", weak.Select(f => $"{f.Planet} {f.PercentOfMinimum:0}%"))}.");
        if (lagnaLord is not null)
        {
            var ll = ranked.FirstOrDefault(f => f.Planet == lagnaLord);
            if (ll is not null)
                lines.Add($"Your Lagna lord, {lagnaLord}, stands at {ll.PercentOfMinimum:0}% ({TierWord(ll.PercentOfMinimum)}) — it carries the body, health and overall direction of the chart.");
        }
        lines.Add("Rahu and Ketu have no Ṣaḍbala; a planet's dasha tends to deliver more when it is at or above 100%.");
        return lines;
    }

    private static string TierWord(decimal? pct) => StrengthBands.ShadbalaPercentOfMinimum.Classify(pct) switch
    {
        StrengthTier.Strong => "strong",
        StrengthTier.Moderate => "moderate",
        _ => "weak",
    };
}

public sealed record HouseStrengthFact(int House, decimal Rupas, string? Lord);

public static class HouseStrengthSummary
{
    private static readonly string[] Matters =
    {
        "", "self, body and temperament", "wealth, family and speech", "courage, effort and siblings",
        "home, mother and peace of mind", "children, intellect and creativity", "health problems, debts and rivals",
        "marriage and partnerships", "longevity, crises and transformation", "fortune, dharma and the father",
        "career and status", "gains and fulfilment of wishes", "losses, foreign matters and liberation",
    };

    public static IReadOnlyList<string> Build(IReadOnlyList<HouseStrengthFact> facts)
    {
        var lines = new List<string>();
        if (facts.Count == 0) return lines;

        var ordered = facts.OrderByDescending(f => f.Rupas).ToList();
        var strong = ordered.Where(f => StrengthBands.BhavaBalaRupas.Classify(f.Rupas) == StrengthTier.Strong).ToList();
        var weak = ordered.Where(f => StrengthBands.BhavaBalaRupas.Classify(f.Rupas) == StrengthTier.Weak).ToList();

        lines.Add($"Strongest house: {Describe(ordered[0])}, {ordered[0].Rupas:0.0} rūpas. Weakest: {Describe(ordered[^1])}, {ordered[^1].Rupas:0.0} rūpas.");
        if (strong.Count > 0)
            lines.Add($"Strong houses (7 rūpas or more): {string.Join("; ", strong.Select(Describe))}. These areas of life are well supported.");
        if (weak.Count > 0)
            lines.Add($"Weak houses (under 5 rūpas): {string.Join("; ", weak.Select(Describe))}. These areas need more effort or other support.");
        if (strong.Count == 0 && weak.Count == 0)
            lines.Add("All houses are in the middle band, so no area of life stands out as especially supported or starved.");
        lines.Add("Houses are best compared with each other within this chart rather than against a fixed score.");
        return lines;
    }

    private static string Describe(HouseStrengthFact f)
        => $"house {f.House} ({(f.House is >= 1 and <= 12 ? Matters[f.House] : "")})" + (f.Lord is null ? "" : $", lord {f.Lord}");
}

public sealed record YogaFact(string Name, string? Nature, string? Based, bool Present, IReadOnlyList<string> Planets, decimal? WeakestPercent);

public static class YogaSummary
{
    public static IReadOnlyList<string> Build(IReadOnlyList<YogaFact> facts)
    {
        var lines = new List<string>();
        var present = facts.Where(f => f.Present).GroupBy(f => f.Name).Select(g => g.First()).ToList();
        if (present.Count == 0)
        {
            lines.Add("No yogas from the evaluated list are present in this chart.");
            return lines;
        }

        int Count(string nature) => present.Count(f => f.Nature == nature);
        lines.Add($"{present.Count} yogas are present: {Count("AUSPICIOUS")} good, {Count("INAUSPICIOUS")} challenging, "
                  + $"{Count("MIXED") + Count("CONTEXTUAL")} mixed or dependent on other factors.");

        var based = present.Where(f => f.Based is not null).GroupBy(f => f.Based!).OrderByDescending(g => g.Count())
            .Select(g => $"{g.Count()} {g.Key}").ToList();
        if (based.Count > 0)
            lines.Add($"By reference point: {string.Join(", ", based)}.");

        var goodWell = present.Where(f => f.Nature == "AUSPICIOUS" && f.WeakestPercent >= 100m)
            .OrderByDescending(f => f.WeakestPercent).Take(5).ToList();
        if (goodWell.Count > 0)
            lines.Add($"Good yogas with all their planets at full strength (most likely to deliver): {string.Join(", ", goodWell.Select(f => f.Name))}.");

        var goodWeak = present.Where(f => f.Nature == "AUSPICIOUS" && f.WeakestPercent < 80m).Take(5).ToList();
        if (goodWeak.Count > 0)
            lines.Add($"Good yogas resting on a weak planet (promise is there, delivery is faint): {string.Join(", ", goodWeak.Select(f => f.Name))}.");

        var allBad = present.Where(f => f.Nature == "INAUSPICIOUS").ToList();
        var bad = allBad.Take(6).ToList();
        if (bad.Count > 0)
            lines.Add($"Challenging yogas present: {string.Join(", ", bad.Select(f => f.Name))}{(allBad.Count > bad.Count ? $" (first {bad.Count} of {allBad.Count}; filter the table by Bad for all)" : "")}. Check their planets' strength and the dasha timing before treating them as active.");

        lines.Add("A yoga shows potential; it usually matures in the dasha of one of its planets. Raman's and Narasimha Rao's versions of the same yoga are counted once here.");
        return lines;
    }
}
