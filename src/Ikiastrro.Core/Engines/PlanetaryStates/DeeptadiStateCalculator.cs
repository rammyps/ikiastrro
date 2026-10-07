namespace Ikiastrro.Core.Engines.PlanetaryStates;

/// <summary>
/// Dīptādi Avastha (PVR sec 15.4.3) — "state related to attitude and mood," 9 states, read the way
/// Jagannatha Hora prints them in its "Mood (D-1)" column (decision 012, golden:
/// <c>AvasthaJhoraMoodGoldenTests</c>, 4 reference charts, 36 planet cells):
///   - Deepta (exalted) and Swastha (own sign / moolatrikona) are placement flags.
///   - Mudita / Saanta / Deena / Duhkhita are the planet's compound (Panchadha) relationship to its
///     sign lord and are kept <i>alongside</i> Deepta/Swastha — an exalted Sun in Aries (lord Mars:
///     natural friend, temporary enemy) is Deepta and Deena. A planet in its own sign has no
///     separate sign lord, so gets no relationship tier.
///   - Vikala: joined by malefic planets (plural: at least two natural malefics in the sign).
///   - Khala: the sign is owned by a natural malefic (Sun, Mars, Saturn).
///   - Kopita: joined closely by the Sun — the Sun within <see cref="KopitaOrbDegrees"/>.
/// "Malefic" here is the fixed classical set (<see cref="ClassicalMalefics"/>); JHora does not make
/// the Moon or Mercury conditional for these states. Every match is kept (the book orders none).
///
/// Works for any chart type: it only needs dignity, the sign-lord relationship, conjunctions and the
/// Sun's real separation, all of which ChartAnalyzer already produces.
/// </summary>
public static class DeeptadiStateCalculator
{
    /// <summary>Kopita's "joined closely" orb. JHora flags 3.8°, 3.9°, 4.3° and 1.2° from the Sun and
    /// not 6.4°, so the true cut-off lies in [4.3°, 6.4°); 5° is the round value inside it.</summary>
    public const decimal KopitaOrbDegrees = 5m;

    public static List<byte> For(
        string? dignityStatus,
        string? relationToSignLord,
        string? signLordPlanet,
        IReadOnlyList<string> conjunctPlanets,
        decimal? distanceFromSunDegrees,
        PlanetaryStateRuleSet rules)
    {
        var ids = new List<byte>();

        // Placement tier: exaltation / own sign.
        var isPlacementTier = dignityStatus is "Exalted" or "Own Sign" or "Moolatrikona";
        if (isPlacementTier && rules.DeeptadiByDignity.TryGetValue(dignityStatus!, out var placement))
            ids.Add(placement.AvasthaStateId);

        // Relationship tier to the sign lord. Nodes have no Naisargika table, so for them (relation
        // null) the engine's own dignity label stands in, as before.
        var relationKey = relationToSignLord ?? (isPlacementTier ? null : dignityStatus);
        if (relationKey is not null && rules.DeeptadiByDignity.TryGetValue(relationKey, out var tier)
            && !ids.Contains(tier.AvasthaStateId))
            ids.Add(tier.AvasthaStateId);

        if (conjunctPlanets.Count(ClassicalMalefics.Contains) >= 2)
            ids.Add(rules.DeeptadiStatesByName["Vikala"].Id);

        if (signLordPlanet is not null && ClassicalMalefics.Contains(signLordPlanet))
            ids.Add(rules.DeeptadiStatesByName["Khala"].Id);

        if (distanceFromSunDegrees is { } d && d <= KopitaOrbDegrees)
            ids.Add(rules.DeeptadiStatesByName["Kopita"].Id);

        return ids;
    }
}
