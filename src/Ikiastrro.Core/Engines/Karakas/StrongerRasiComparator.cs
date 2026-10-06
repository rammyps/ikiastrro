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
///     rasi wins. For Aquarius and Scorpio both co-lords count (Saturn and Rahu, Mars and Ketu):
///     PVR's Exercise 26 answer — "Aq is aspected by co-lord Rahu (though Saturn is the
///     primary/stronger lord, Rahu's aspect also counts)".
/// (3) A rasi containing an exalted planet (<see cref="DignityEngine"/>'s "Exalted" status,
///     which already carries PVR's own Moon-under-3°/Mercury-under-15° exaltation-segment rule)
///     wins.
/// (4) The rasi whose lord sits in a rasi of different oddity (odd/even) than the rasi itself
///     wins over one whose lord sits in a rasi of the same oddity. For Aquarius / Scorpio the
///     lord is the stronger co-lord (<see cref="StrongerCoLord"/>, sec.15.5.1) — PVR's own
///     Narayana-dasa example: "Aq is an odd sign and its stronger lord is also in an odd sign". PVR notes this rule always
///     resolves the tie when comparing a single planet's own 2 owned signs (their oddity always
///     differs) — so rules 5/6 exist for completeness and for reuse outside graha arudha, but
///     are not expected to fire from <see cref="GrahaArudhaCalculator"/>.
/// (5) Natural strength: Dual > Fixed > Movable.
/// (6) Longer advancement of the rasi's own lord within the lord's occupied sign wins (measured
///     from the end of the sign for Rahu/Ketu). PVR's note: "In the case of Aq and Sc, we use the
///     stronger lord" — so Rahu/Ketu can be the lord here when they are the stronger co-lord.
/// </summary>
public static class StrongerRasiComparator
{
    /// <summary>The stronger of <paramref name="a"/>/<paramref name="b"/> per the sec.15.5.2
    /// cascade, given <paramref name="d1"/>'s actual planet placements.</summary>
    public static ZodiacName Compare(ZodiacName a, ZodiacName b, ChartAnalysisInput d1) =>
        Decide(a, b, d1).Winner;

    /// <summary>The same decision as <see cref="Compare"/> with the rule that settled it, for display.</summary>
    public static RasiStrengthReading Explain(ZodiacName a, ZodiacName b, ChartAnalysisInput d1)
    {
        var (winner, rule) = Decide(a, b, d1);
        return new RasiStrengthReading(a, b, winner, rule);
    }

    private static (ZodiacName Winner, string Rule) Decide(ZodiacName a, ZodiacName b, ChartAnalysisInput d1)
    {
        if (a == b) return (a, "the same rasi");

        var planets = d1.Planets.Where(p => p.Planet != "Ascendant").ToList();
        var signByPlanet = planets.ToDictionary(p => p.Planet, p => Enum.Parse<ZodiacName>(p.Sign));

        bool Influences(PlanetName planet, ZodiacName sign) =>
            signByPlanet.TryGetValue(planet.ToString(), out var s)
            && (s == sign || RasiDrishtiCalculator.Aspects(s, sign));

        (ZodiacName, string)? Pick<T>(Func<ZodiacName, T> score, Func<T, T, string> say) where T : IComparable<T>
        {
            var cmp = score(a).CompareTo(score(b));
            if (cmp == 0) return null;
            var (w, l) = cmp > 0 ? (a, b) : (b, a);
            return (w, say(score(w), score(l)));
        }

        // Rule 1: planet count.
        int PlanetCount(ZodiacName s) => planets.Count(p => p.Sign == s.ToString());
        if (Pick(PlanetCount, (w, l) => $"it contains more planets ({w} against {l})") is { } r1) return r1;

        // Rule 2: occupied/aspected by Jupiter, Mercury, or the rasi's own lord.
        int SpecialInfluenceCount(ZodiacName s)
        {
            var specials = new HashSet<PlanetName> { PlanetName.Jupiter, PlanetName.Mercury };
            specials.UnionWith(Lords(s));
            return specials.Count(p => Influences(p, s));
        }
        if (Pick(SpecialInfluenceCount, (w, l) => $"more of Jupiter, Mercury and its lord occupy or aspect it ({w} against {l})") is { } r2) return r2;

        // Rule 3: contains an exalted planet.
        bool HasExalted(ZodiacName s) => planets
            .Where(p => p.Sign == s.ToString())
            .Any(p => DignityEngine.Evaluate(
                p.Planet,
                Enum.Parse<ZodiacName>(p.Sign),
                p.VargaLongitudeDegrees is null && p.NirayanaLongitudeDegrees is { } lon ? AstroMath.GetDegreesInSign(lon) : null,
                signByPlanet).DignityStatus == "Exalted");
        var (exaltedA, exaltedB) = (HasExalted(a), HasExalted(b));
        if (exaltedA != exaltedB) return exaltedA ? (a, "it contains an exalted planet") : (b, "it contains an exalted planet");

        // Rule 4: lord in a rasi of different oddity than the rasi itself.
        bool IsOddSign(ZodiacName s) => (int)s % 2 == 0; // 0-based enum: Aries(0) is the 1st (odd) sign
        bool LordInDifferentOddity(ZodiacName s)
        {
            var lord = StrongerCoLord.For(s, d1).ToString();
            var lordSign = signByPlanet[lord];
            return IsOddSign(s) != IsOddSign(lordSign);
        }
        var (differentA, differentB) = (LordInDifferentOddity(a), LordInDifferentOddity(b));
        if (differentA != differentB)
            return differentA ? (a, "its lord is in a rasi of different oddity") : (b, "its lord is in a rasi of different oddity");

        // Rule 5: natural strength — Dual > Fixed > Movable.
        int ModalityRank(ZodiacName s) => BaadhakaCalculator.GetModality(s) switch
        {
            SignModality.Dual => 2,
            SignModality.Fixed => 1,
            _ => 0
        };
        if (Pick(ModalityRank, (_, _) => "it is naturally stronger (dual over fixed over movable)") is { } r5) return r5;

        // Rule 6: the rasi's own lord's advancement within its occupied sign (from the sign's
        // end for Rahu/Ketu, which are the lord of Aq / Sc when they are the stronger co-lord).
        double LordAdvancement(ZodiacName s)
        {
            var lord = StrongerCoLord.For(s, d1).ToString();
            var placement = planets.First(p => p.Planet == lord);
            var degreeInSign = AstroMath.GetDegreesInSign(placement.VargaLongitudeDegrees ?? placement.NirayanaLongitudeDegrees!.Value);
            return lord is "Rahu" or "Ketu" ? 30.0 - degreeInSign : degreeInSign;
        }
        if (Pick(LordAdvancement, (_, _) => "its lord is further advanced in its sign") is { } r6) return r6;

        // No further tiebreaker exists in the book; PVR's own note guarantees rule 4 always
        // resolves this for a single planet's 2 owned signs, so this is defensive-only.
        return (a, "equal on every rule, so the first is kept");
    }

    /// <summary>Both co-lords of Aquarius (Saturn, Rahu) and Scorpio (Mars, Ketu); the one lord otherwise.</summary>
    private static IEnumerable<PlanetName> Lords(ZodiacName sign) => sign switch
    {
        ZodiacName.Aquarius => [PlanetName.Saturn, PlanetName.Rahu],
        ZodiacName.Scorpio => [PlanetName.Mars, PlanetName.Ketu],
        _ => [Enum.Parse<PlanetName>(HouseEngine.GetSignLord(sign))],
    };

    private static ZodiacName? WinnerOf<T>(ZodiacName a, ZodiacName b, Func<ZodiacName, T> score)
        where T : IComparable<T>
    {
        var cmp = score(a).CompareTo(score(b));
        return cmp == 0 ? null : cmp > 0 ? a : b;
    }
}

/// <summary>Two rasis, the stronger one, and the sec.15.5.2 rule that decided it.</summary>
public sealed record RasiStrengthReading(ZodiacName A, ZodiacName B, ZodiacName Stronger, string Rule)
{
    public bool Includes(ZodiacName sign) => A == sign || B == sign;
}
