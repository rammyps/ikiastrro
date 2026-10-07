using Ikiastrro.Core.Engines.Strength;

namespace Ikiastrro.Core.Engines.Dispositors;

/// <summary>What the summary needs to know about one graha beyond its chain.</summary>
public sealed record DispositorPlanetFacts(
    string Planet,
    int HouseFromLagna,
    string Sign,
    string? Dignity,
    bool IsCombust,
    bool IsRetrograde,
    decimal? ShadbalaPercentOfMinimum,
    IReadOnlyList<int> HousesRuled);

/// <summary>
/// A plain-English reading of a chart's dispositor chains for the Astro Facts Dispositors table:
/// where the chains end, how strong that planet is, which planets are rescued through it, and which
/// planets on the way are weak links. Generated from the same chains and facts the table shows.
/// The method (following sign lords) is standard; the "what it means" wording is project synthesis,
/// not a classical text.
/// </summary>
public static class DispositorSummary
{
    private static readonly int[] Kendras = { 1, 4, 7, 10 };

    public static IReadOnlyList<string> Build(
        IReadOnlyList<DispositorChain> chains, IReadOnlyDictionary<string, DispositorPlanetFacts> facts)
    {
        var lines = new List<string>();
        if (chains.Count == 0) return lines;

        var finals = chains.Where(c => c.FinalDispositor is not null)
            .GroupBy(c => c.FinalDispositor!).OrderByDescending(g => g.Count()).ToList();
        var loops = chains.Where(c => c.FinalDispositor is null && c.Cycle is { Count: > 0 }).ToList();

        if (finals.Count == 1 && loops.Count == 0)
        {
            var final = finals[0].Key;
            lines.Add($"Every planet's chain ends at {final}, so {final} is the chart's single final dispositor — " +
                      "the one planet the whole chart ultimately answers to.");
            lines.AddRange(Profile(final, facts));
        }
        else if (finals.Count >= 1)
        {
            var parts = string.Join("; ", finals.Select(g => $"{g.Key} ({string.Join(", ", g.Select(c => c.Planet))})"));
            lines.Add($"The chains end at more than one planet: {parts}. The chart has several independent anchors, " +
                      "so its parts work more separately than in a chart with one final dispositor.");
            foreach (var g in finals) lines.AddRange(Profile(g.Key, facts));
        }

        foreach (var loop in loops.GroupBy(c => string.Join("-", c.Cycle!.OrderBy(x => x))).Select(g => g.First()))
        {
            var cycle = string.Join(" and ", loop.Cycle!);
            lines.Add(loop.InMutualReception
                ? $"{cycle} sit in each other's signs (mutual reception), so they share the final say instead of one planet ruling alone. Planets that lead into them depend on both."
                : $"{cycle} pass the chain round in a loop with no planet in its own sign, so no single planet anchors those planets.");
        }

        lines.AddRange(Rescues(chains, facts));
        lines.AddRange(WeakLinks(chains, facts));
        return lines;
    }

    private static IEnumerable<string> Profile(string planet, IReadOnlyDictionary<string, DispositorPlanetFacts> facts)
    {
        if (!facts.TryGetValue(planet, out var f)) yield break;
        var rules = f.HousesRuled.Count == 0 ? "" : $", ruling the {HouseList(f.HousesRuled)}";
        var dignity = string.IsNullOrWhiteSpace(f.Dignity) ? "" : $" ({f.Dignity!.ToLowerInvariant()})";
        yield return $"{planet} is in {f.Sign}{dignity}, in house {f.HouseFromLagna}{rules}.";

        if (f.ShadbalaPercentOfMinimum is { } pct)
        {
            var tier = StrengthBands.ShadbalaPercentOfMinimum.Classify(pct);
            var verdict = tier switch
            {
                StrengthTier.Strong => "at or above the strength it needs, so the chains resting on it are well supported",
                StrengthTier.Moderate => "a little under the strength it needs, so support is real but not full",
                _ => "well under the strength it needs, so everything resting on it is only weakly supported",
            };
            yield return $"Its Ṣaḍbala is {pct:0}% of the required minimum — {verdict}.";
        }

        var cautions = new List<string>();
        if (f.IsCombust) cautions.Add("combust (too close to the Sun, which dims a planet's results)");
        if (f.IsRetrograde) cautions.Add("retrograde");
        if (cautions.Count > 0) yield return $"{planet} is also {string.Join(" and ", cautions)}.";
    }

    // A debilitated planet whose dispositor stands in a kendra from the Lagna: one classical condition
    // for cancelling the debilitation (Neecha Bhanga).
    private static IEnumerable<string> Rescues(
        IReadOnlyList<DispositorChain> chains, IReadOnlyDictionary<string, DispositorPlanetFacts> facts)
    {
        foreach (var f in facts.Values.Where(f => string.Equals(f.Dignity, "Debilitated", StringComparison.OrdinalIgnoreCase)))
        {
            var chain = chains.FirstOrDefault(c => c.Planet == f.Planet);
            if (chain is null || chain.Chain.Count < 2) continue;
            var dispositor = chain.Chain[1];
            if (dispositor == f.Planet || !facts.TryGetValue(dispositor, out var d)) continue;
            if (Kendras.Contains(d.HouseFromLagna))
                yield return $"{f.Planet} is debilitated, but its dispositor {dispositor} stands in a kendra (house {d.HouseFromLagna}) from the Lagna — " +
                             "one classical condition for cancelling a debilitation, so the weakness is partly repaired.";
        }
    }

    // Planets that other planets' chains pass through, and that are weak or combust.
    private static IEnumerable<string> WeakLinks(
        IReadOnlyList<DispositorChain> chains, IReadOnlyDictionary<string, DispositorPlanetFacts> facts)
    {
        var finals = chains.Select(c => c.FinalDispositor).Where(x => x is not null).ToHashSet();
        var carried = new Dictionary<string, List<string>>();
        foreach (var c in chains)
            foreach (var step in c.Chain.Skip(1).Where(s => s != c.Planet && !finals.Contains(s)))
            {
                if (!carried.TryGetValue(step, out var list)) carried[step] = list = new();
                if (!list.Contains(c.Planet)) list.Add(c.Planet);
            }

        foreach (var (link, planets) in carried.OrderBy(kv => kv.Key))
        {
            if (!facts.TryGetValue(link, out var f)) continue;
            var weak = StrengthBands.ShadbalaPercentOfMinimum.Classify(f.ShadbalaPercentOfMinimum) == StrengthTier.Weak;
            if (!weak && !f.IsCombust) continue;
            var why = weak && f.IsCombust ? "combust and weak" : weak ? "weak" : "combust";
            var pct = f.ShadbalaPercentOfMinimum is { } p ? $" (Ṣaḍbala {p:0}% of its minimum)" : "";
            yield return $"Weak link: {link} is {why}{pct}, and {string.Join(", ", planets)} reach the final dispositor only through it.";
        }
    }

    private static string HouseList(IReadOnlyList<int> houses)
    {
        var names = houses.OrderBy(h => h).Select(h => h == 1 ? "Lagna (1st)" : Ordinal(h)).ToList();
        return names.Count == 1 ? names[0] : string.Join(", ", names[..^1]) + " and " + names[^1];
    }

    private static string Ordinal(int n) => n + (n % 100 is >= 11 and <= 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" });
}
