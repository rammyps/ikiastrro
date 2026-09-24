using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.PlanetaryStates;

/// <summary>
/// The unnamed 6-state avastha group PVR gives right after Dīptādi (sec 15.4.3, "6 additional states
/// ... related to attitude and mood") — conventionally called Lajjitādi elsewhere in the jyotish
/// literature, though PVR itself doesn't name the group. All 6 conditions are independent (no shared
/// axis like Dīptādi's dignity tiers) and the book gives no precedence, so every match is kept —
/// several can hold for one planet at once (e.g. Garvita and Mudita together).
///
///   - Lajjita:   in the 5th house (from Lagna) and conjunct Sun/Mars/Saturn/Rahu/Ketu.
///   - Garvita:   exaltation or moolatrikona sign.
///   - Kshudhita: enemy's sign, or conjunct/aspected by an enemy planet, or conjunct Saturn.
///   - Trishita:  a watery sign (Cancer/Scorpio/Pisces — PVR sec 2.2, not the differently-sourced
///                6-sign set RamanYogaBatchSevenEvaluator uses for an unrelated yoga), aspected by an
///                enemy planet, with no benefic aspect.
///   - Mudita:    friend's sign, or conjunct/aspected by a friend planet, or conjunct Jupiter. Same
///                name as one of Dīptādi's 6 states — the book reuses it; tbl_Dim_PlanetaryState's
///                UNIQUE(AvasthaSystem, StateName) already allows this.
///   - Kshobhita: conjunct the Sun and aspected by a malefic or enemy planet.
///
/// "Friend"/"enemy" of an associated (conjunct/aspecting) planet is the directional Panchadha Maitri
/// from this planet's own perspective — DignityEngine.EvaluatePairRelationship, not DignityStatus
/// (which is this planet's relationship to its own sign lord, a different question).
/// </summary>
public static class LajjitadiStateCalculator
{
    private static readonly HashSet<ZodiacName> WaterySigns = new() { ZodiacName.Cancer, ZodiacName.Scorpio, ZodiacName.Pisces };

    private static bool IsEnemyTier(string? tier) => tier is "Enemy" or "Great Enemy";
    private static bool IsFriendTier(string? tier) => tier is "Friend" or "Great Friend";

    public static List<byte> For(
        string sign,
        string? dignityStatus,
        int houseNumberFromLagna,
        IReadOnlyList<string> conjunctPlanets,
        IReadOnlyList<string> aspectingPlanets,
        Func<string, bool> isNaturalMalefic,
        Func<string, string?> relationshipOf,
        PlanetaryStateRuleSet rules)
    {
        var ids = new List<byte>();
        var states = rules.LajjitadiStatesByName;

        // Lajjita — 5th house, conjunct a fixed malefic-leaning graha.
        if (houseNumberFromLagna == 5 &&
            conjunctPlanets.Any(p => p is "Sun" or "Mars" or "Saturn" or "Rahu" or "Ketu"))
            ids.Add(states["Lajjita"].Id);

        // Garvita — exalted or moolatrikona.
        if (dignityStatus is "Exalted" or "Moolatrikona")
            ids.Add(states["Garvita"].Id);

        // Kshudhita — enemy's sign, or conjunct/aspected by an enemy, or conjunct Saturn.
        if (dignityStatus is "Enemy" or "Great Enemy"
            || conjunctPlanets.Any(p => IsEnemyTier(relationshipOf(p)))
            || aspectingPlanets.Any(p => IsEnemyTier(relationshipOf(p)))
            || conjunctPlanets.Contains("Saturn"))
            ids.Add(states["Kshudhita"].Id);

        // Trishita — watery sign, aspected by an enemy, no benefic aspect.
        if (Enum.TryParse<ZodiacName>(sign, out var signEnum) && WaterySigns.Contains(signEnum)
            && aspectingPlanets.Any(p => IsEnemyTier(relationshipOf(p)))
            && !aspectingPlanets.Any(p => !isNaturalMalefic(p)))
            ids.Add(states["Trishita"].Id);

        // Mudita — friend's sign, or conjunct/aspected by a friend, or conjunct Jupiter.
        if (dignityStatus is "Friend" or "Great Friend"
            || conjunctPlanets.Any(p => IsFriendTier(relationshipOf(p)))
            || aspectingPlanets.Any(p => IsFriendTier(relationshipOf(p)))
            || conjunctPlanets.Contains("Jupiter"))
            ids.Add(states["Mudita"].Id);

        // Kshobhita — conjunct the Sun, aspected by a malefic or an enemy.
        if (conjunctPlanets.Contains("Sun")
            && aspectingPlanets.Any(p => isNaturalMalefic(p) || IsEnemyTier(relationshipOf(p))))
            ids.Add(states["Kshobhita"].Id);

        return ids;
    }
}
