using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Houses;

/// <summary>
/// Argala (intervention) / Virodhargala (obstruction) — P.V.R. Narasimha Rao, Vedic Astrology:
/// An Integrated Approach, sec.10.5 "Argala (Intervention)" / sec.10.6 "Virodhargala
/// (Obstruction)", pp.104-107 (SRC_PVR_INTEGRATED, verified against the raw book extract and
/// cross-checked against Exercise 16's own worked answer for Chart 5, pp.110-111):
///
///   - A planet/house in the 2nd, 4th or 11th from a target causes PRIMARY argala on it
///     (subhaargala if the occupant is a natural benefic, paapaargala if malefic).
///   - The 5th causes SECONDARY argala.
///   - Virodhargala from the 12th, 10th, 3rd and 9th obstructs the 2nd/4th/11th/5th argala
///     respectively.
///   - Exception (sec.10.6, stated right after Exercise 16): 2+ malefics in the 3rd-from
///     position cause argala instead of virodhargala. Exercise 16's own house-11 answer is
///     the source for the ">=2" threshold — the book itself only says "several".
///   - If the target sign holds Ketu, argala/virodhargala on it is counted anti-zodiacally
///     (backward) instead of forward.
///
/// Sign-based target (ZodiacName), not a house number — PVR runs the same technique against
/// both a house and a karaka's own occupied sign in the same worked example (sec.10.7), so the
/// primitive has to accept either; a house-number caller resolves via HouseEngine.GetHouseSign
/// first (see the overload below). Mirrors tbl_Rule_Argala (migration 127) — cited there but
/// not read from there, same rationale as RasiDrishtiCalculator.
/// </summary>
public static class ArgalaCalculator
{
    public enum RelationType { Argala, Virodhargala }

    public readonly record struct Position(int HouseOffset, bool IsPrimary, ZodiacName Sign);

    public sealed record OccupiedPosition(Position Position, IReadOnlyList<PlanetName> Occupants);

    public sealed record ArgalaEvaluation(
        ZodiacName Target,
        IReadOnlyList<OccupiedPosition> Argala,
        IReadOnlyList<OccupiedPosition> Virodhargala,
        bool ThirdHouseExceptionApplied,
        bool CountedAntiZodiacally);

    public readonly record struct ComparisonResult(
        int ArgalaCount, int VirodhargalaCount,
        int ArgalaDignitySum, int VirodhargalaDignitySum,
        RelationType? Dominant);

    private static readonly (int Offset, bool IsPrimary)[] ArgalaOffsets =
        { (2, true), (4, true), (11, true), (5, false) };

    private static readonly (int Offset, bool IsPrimary)[] VirodhargalaOffsets =
        { (12, true), (10, true), (3, true), (9, false) };

    /// <summary>
    /// Natural (naisargika) benefic/malefic status, matching tbl_Planets.NaturalNature: Sun,
    /// Mars, Saturn, Rahu, Ketu are always Malefic; Jupiter, Venus always Benefic; Moon and
    /// Mercury are Conditional (tbl_Planets.ConditionalRule) and are resolved here from the
    /// chart itself, at the same whole-sign granularity the rest of this calculator uses:
    ///   - Moon: malefic when waning (Krishna Paksha) — more than 6 signs ahead of the Sun
    ///     (past the Full Moon point). A whole-sign approximation of a longitude-based rule,
    ///     since this calculator never sees exact degrees.
    ///   - Mercury: malefic when conjunct (same sign as) a fixed natural malefic — a
    ///     conjunction-only reading of "afflicted" (tbl_Planets' own text also covers aspects,
    ///     which this calculator has no input for).
    /// If the Sun or Moon isn't present in <paramref name="occupancy"/> (an incomplete/synthetic
    /// chart), Moon falls back to benefic — the prior simplification's default.
    /// </summary>
    public static bool IsNaturalMalefic(
        PlanetName planet, IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> occupancy) =>
        planet switch
        {
            PlanetName.Sun or PlanetName.Mars or PlanetName.Saturn or PlanetName.Rahu or PlanetName.Ketu => true,
            PlanetName.Jupiter or PlanetName.Venus => false,
            PlanetName.Moon => IsWaningMoon(occupancy),
            PlanetName.Mercury => IsMercuryAfflicted(occupancy),
            _ => throw new ArgumentOutOfRangeException(nameof(planet))
        };

    private static bool IsWaningMoon(IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> occupancy)
    {
        var sunSign = FindSign(PlanetName.Sun, occupancy);
        var moonSign = FindSign(PlanetName.Moon, occupancy);
        if (sunSign is null || moonSign is null) return false;
        return AstroMath.CountFromSignToSign(sunSign.Value, moonSign.Value) > 6;
    }

    private static bool IsMercuryAfflicted(IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> occupancy)
    {
        var mercurySign = FindSign(PlanetName.Mercury, occupancy);
        if (mercurySign is null) return false;
        return occupancy[mercurySign.Value].Any(p =>
            p is PlanetName.Sun or PlanetName.Mars or PlanetName.Saturn or PlanetName.Rahu or PlanetName.Ketu);
    }

    private static ZodiacName? FindSign(
        PlanetName planet, IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> occupancy)
    {
        foreach (var (sign, occupants) in occupancy)
            if (occupants.Contains(planet)) return sign;
        return null;
    }

    /// <summary>Convenience overload for a house-number target (the common case; PVR's karaka-sign
    /// usage calls <see cref="Evaluate(ZodiacName,IReadOnlyDictionary{ZodiacName,IReadOnlyList{PlanetName}})"/> directly).</summary>
    public static ArgalaEvaluation Evaluate(
        ZodiacName ascendantSign, int houseNumber,
        IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> occupancy) =>
        Evaluate(HouseEngine.GetHouseSign(ascendantSign, houseNumber), occupancy);

    public static ArgalaEvaluation Evaluate(
        ZodiacName target,
        IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> occupancy)
    {
        var reverse = occupancy.TryGetValue(target, out var targetOccupants) &&
                      targetOccupants.Contains(PlanetName.Ketu);

        var argala = ArgalaOffsets
            .Select(o => BuildPosition(target, o.Offset, o.IsPrimary, reverse, occupancy))
            .ToList();
        var virodhargala = VirodhargalaOffsets
            .Select(o => BuildPosition(target, o.Offset, o.IsPrimary, reverse, occupancy))
            .ToList();

        var thirdPosition = virodhargala.Single(p => p.Position.HouseOffset == 3);
        var maleficCount = thirdPosition.Occupants.Count(p => IsNaturalMalefic(p, occupancy));
        var exceptionApplies = maleficCount >= 2;
        if (exceptionApplies)
        {
            virodhargala.Remove(thirdPosition);
            argala.Add(thirdPosition);
        }

        return new ArgalaEvaluation(target, argala, virodhargala, exceptionApplies, reverse);
    }

    /// <summary>
    /// sec.10.7's own comparison method: "see if more planets cause argala or virodhargala. If
    /// they are caused by the same number of planets, compare the strengths." PVR never gives a
    /// formula for "strength" — the count comparator is sourced directly; summing the caller's
    /// dignity score per side as the tie-break is this project's own synthesis (PROJECT_SYNTHESIS),
    /// reusing the existing DignityScore ordinal (dignity-pvr.md) rather than inventing a new
    /// number. dignityScore is injected (not looked up internally) so this calculator stays
    /// degree-independent and pure, like RasiDrishtiCalculator.
    /// </summary>
    public static ComparisonResult Compare(ArgalaEvaluation evaluation, Func<PlanetName, int> dignityScore)
    {
        var argalaOccupants = evaluation.Argala.SelectMany(p => p.Occupants).ToList();
        var virodhOccupants = evaluation.Virodhargala.SelectMany(p => p.Occupants).ToList();

        if (argalaOccupants.Count != virodhOccupants.Count)
        {
            var dominant = argalaOccupants.Count > virodhOccupants.Count ? RelationType.Argala : RelationType.Virodhargala;
            return new ComparisonResult(argalaOccupants.Count, virodhOccupants.Count, 0, 0, dominant);
        }

        var argalaDignitySum = argalaOccupants.Sum(dignityScore);
        var virodhDignitySum = virodhOccupants.Sum(dignityScore);
        RelationType? tieBreakDominant = argalaDignitySum == virodhDignitySum
            ? null
            : argalaDignitySum > virodhDignitySum ? RelationType.Argala : RelationType.Virodhargala;

        return new ComparisonResult(argalaOccupants.Count, virodhOccupants.Count,
            argalaDignitySum, virodhDignitySum, tieBreakDominant);
    }

    private static OccupiedPosition BuildPosition(
        ZodiacName target, int houseOffset, bool isPrimary, bool reverse,
        IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> occupancy)
    {
        var sign = CountFrom(target, houseOffset, reverse);
        var occupants = occupancy.TryGetValue(sign, out var list) ? list : Array.Empty<PlanetName>();
        return new OccupiedPosition(new Position(houseOffset, isPrimary, sign), occupants);
    }

    /// <summary>The Nth house from <paramref name="from"/> (1 = itself), forward or anti-zodiacally.</summary>
    private static ZodiacName CountFrom(ZodiacName from, int houseNumber, bool reverse)
    {
        var step = reverse ? -(houseNumber - 1) : houseNumber - 1;
        var index = (((int)from + step) % 12 + 12) % 12;
        return (ZodiacName)index;
    }
}
