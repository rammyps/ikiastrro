using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// PVR sec.15.5.2 "Stronger Rasi" — the 6-rule cascade used to pick the stronger of two rasis
/// given this chart's actual placements (SRC_PVR_INTEGRATED, verified against the raw book
/// extract). Rules are applied in order; the first rule with a winner stops the cascade.
/// Built for <see cref="GrahaArudhaCalculator"/> (comparing the 2 signs one of Mars/Mercury/
/// Jupiter/Venus/Saturn owns), but the algorithm itself is general — PVR reuses it for Narayana
/// Dasa's own rasi-strength comparisons.
///
/// (1) More planets occupying the rasi wins.
/// (2) More of {Jupiter, Mercury, the rasi's own lord} occupying/aspecting (rasi drishti) the
///     rasi wins. "The rasi's own lord": this project's classical-sole-lord convention
///     (<see cref="HouseEngine.GetSignLord"/>) — same known Aquarius/Scorpio (Saturn-vs-Rahu,
///     Mars-vs-Ketu) simplification already documented on <see cref="ArudhaCalculator"/>.
/// (3) A rasi containing an exalted planet (<see cref="DignityEngine"/>'s "Exalted" status,
///     which already carries PVR's own Moon-under-3°/Mercury-under-15° exaltation-segment rule)
///     wins.
/// (4) The rasi whose lord sits in a rasi of different oddity (odd/even) than the rasi itself
///     wins over one whose lord sits in a rasi of the same oddity. PVR notes this rule always
///     resolves the tie when comparing a single planet's own 2 owned signs (their oddity always
///     differs) — so rules 5/6 exist for completeness and for reuse outside graha arudha, but
///     are not expected to fire from <see cref="GrahaArudhaCalculator"/>.
/// (5) Natural strength: Dual > Fixed > Movable.
/// (6) Longer advancement of the rasi's own lord within the lord's occupied sign wins (measured
///     from the end of the sign for Rahu/Ketu) — unreachable under this project's classical-sole-
///     lord convention (the lord is never Rahu/Ketu), kept faithful to the book text regardless.
/// </summary>
public static class StrongerRasiComparator
{
    /// <summary>The stronger of <paramref name="a"/>/<paramref name="b"/> per the sec.15.5.2
    /// cascade, given <paramref name="d1"/>'s actual planet placements.</summary>
    public static ZodiacName Compare(ZodiacName a, ZodiacName b, ChartAnalysisInput d1)
    {
        if (a == b) return a;

        var planets = d1.Planets.Where(p => p.Planet != "Ascendant").ToList();
        var signByPlanet = planets.ToDictionary(p => p.Planet, p => Enum.Parse<ZodiacName>(p.Sign));

        bool Influences(PlanetName planet, ZodiacName sign) =>
            signByPlanet.TryGetValue(planet.ToString(), out var s)
            && (s == sign || RasiDrishtiCalculator.Aspects(s, sign));

        // Rule 1: planet count.
        int PlanetCount(ZodiacName s) => planets.Count(p => p.Sign == s.ToString());
        var winner = WinnerOf(a, b, PlanetCount);
        if (winner is not null) return winner.Value;

        // Rule 2: occupied/aspected by Jupiter, Mercury, or the rasi's own lord.
        int SpecialInfluenceCount(ZodiacName s)
        {
            var lord = Enum.Parse<PlanetName>(HouseEngine.GetSignLord(s));
            var specials = new HashSet<PlanetName> { PlanetName.Jupiter, PlanetName.Mercury, lord };
            return specials.Count(p => Influences(p, s));
        }
        winner = WinnerOf(a, b, SpecialInfluenceCount);
        if (winner is not null) return winner.Value;

        // Rule 3: contains an exalted planet.
        bool HasExalted(ZodiacName s) => planets
            .Where(p => p.Sign == s.ToString())
            .Any(p => DignityEngine.Evaluate(
                p.Planet,
                Enum.Parse<ZodiacName>(p.Sign),
                p.NirayanaLongitudeDegrees is { } lon ? AstroMath.GetDegreesInSign(lon) : null,
                signByPlanet).DignityStatus == "Exalted");
        var (exaltedA, exaltedB) = (HasExalted(a), HasExalted(b));
        if (exaltedA != exaltedB) return exaltedA ? a : b;

        // Rule 4: lord in a rasi of different oddity than the rasi itself.
        bool IsOddSign(ZodiacName s) => (int)s % 2 == 0; // 0-based enum: Aries(0) is the 1st (odd) sign
        bool LordInDifferentOddity(ZodiacName s)
        {
            var lord = HouseEngine.GetSignLord(s);
            var lordSign = signByPlanet[lord];
            return IsOddSign(s) != IsOddSign(lordSign);
        }
        var (differentA, differentB) = (LordInDifferentOddity(a), LordInDifferentOddity(b));
        if (differentA != differentB) return differentA ? a : b;

        // Rule 5: natural strength — Dual > Fixed > Movable.
        int ModalityRank(ZodiacName s) => BaadhakaCalculator.GetModality(s) switch
        {
            SignModality.Dual => 2,
            SignModality.Fixed => 1,
            _ => 0
        };
        winner = WinnerOf(a, b, ModalityRank);
        if (winner is not null) return winner.Value;

        // Rule 6: the rasi's own lord's advancement within its occupied sign (from the sign's
        // end for Rahu/Ketu — never actually the lord here, per this project's classical-sole-
        // lord convention, but kept faithful to the book text).
        double LordAdvancement(ZodiacName s)
        {
            var lord = HouseEngine.GetSignLord(s);
            var placement = planets.First(p => p.Planet == lord);
            var degreeInSign = AstroMath.GetDegreesInSign(placement.NirayanaLongitudeDegrees!.Value);
            return lord is "Rahu" or "Ketu" ? 30.0 - degreeInSign : degreeInSign;
        }
        winner = WinnerOf(a, b, LordAdvancement);
        // No further tiebreaker exists in the book; PVR's own note guarantees rule 4 always
        // resolves this for a single planet's 2 owned signs, so this is defensive-only.
        return winner ?? a;
    }

    private static ZodiacName? WinnerOf<T>(ZodiacName a, ZodiacName b, Func<ZodiacName, T> score)
        where T : IComparable<T>
    {
        var cmp = score(a).CompareTo(score(b));
        return cmp == 0 ? null : cmp > 0 ? a : b;
    }
}
