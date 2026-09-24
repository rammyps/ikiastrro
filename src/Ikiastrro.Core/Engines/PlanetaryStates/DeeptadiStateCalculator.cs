namespace Ikiastrro.Core.Engines.PlanetaryStates;

/// <summary>
/// Dīptādi Avastha (PVR sec 15.4.3) — "state related to attitude and mood," 9 states. The first 6
/// are dignity-tier (mutually exclusive among themselves, keyed off DignityStatus via
/// tbl_Rule_DeeptadiState — same shape as Jagradadi/tbl_Rule_WakefulnessState): Deepta (exalted),
/// Swastha (own sign), Mudita (great friend's sign), Saanta (friend's sign), Deena (neutral sign),
/// Duhkhita (enemy's sign). The last 3 are independent affliction flags that can stack on top of any
/// dignity tier — the book states no precedence, so every match is kept:
///   - Vikala: joined by (conjunct) a natural malefic planet.
///   - Khala: the sign is owned by a natural malefic planet (the sign lord is a natural malefic).
///   - Kopita: joined closely by the Sun — read here as combustion (Astangata), an already-sourced,
///     tested orb, rather than inventing a new "closeness" threshold the book doesn't give.
///
/// Works for any chart type, like Jagradadi — Dīptādi only needs DignityStatus/SignLordPlanet/
/// conjunctions, all of which ChartAnalyzer already produces per chart type.
/// </summary>
public static class DeeptadiStateCalculator
{
    public static List<byte> For(
        string? dignityStatus,
        string? signLordPlanet,
        IReadOnlyList<string> conjunctPlanets,
        Func<string, bool> isNaturalMalefic,
        bool isCombust,
        PlanetaryStateRuleSet rules)
    {
        var ids = new List<byte>();

        if (dignityStatus is not null && rules.DeeptadiByDignity.TryGetValue(dignityStatus, out var tier))
            ids.Add(tier.AvasthaStateId);

        if (conjunctPlanets.Any(isNaturalMalefic))
            ids.Add(rules.DeeptadiStatesByName["Vikala"].Id);

        if (signLordPlanet is not null && isNaturalMalefic(signLordPlanet))
            ids.Add(rules.DeeptadiStatesByName["Khala"].Id);

        if (isCombust)
            ids.Add(rules.DeeptadiStatesByName["Kopita"].Id);

        return ids;
    }
}
