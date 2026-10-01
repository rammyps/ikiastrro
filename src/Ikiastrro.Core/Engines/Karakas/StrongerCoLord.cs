using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// PVR sec.15.5.1 "Stronger Co-Lord" (SRC_PVR_INTEGRATED, verified against the raw book
/// extract): Scorpio has two lords (Mars, Ketu) and Aquarius two (Saturn, Rahu); the stronger
/// one acts as the sign's lord when finding its arudha pada (sec.9.2 step 2). Every other sign
/// has one lord (<see cref="HouseEngine.GetSignLord"/>).
///
/// Basic rule: if one co-lord is in the sign and the other is not, the other is the lord.
/// Otherwise the first of these that separates them decides:
/// (1) joined by more planets;
/// (2) conjoined / rāśi-aspected by more of Jupiter, Mercury and its dispositor (each role
///     counts on its own, so Mercury as both Mercury and dispositor counts twice — PVR's own
///     example; a planet is not counted as influencing itself);
/// (3) exalted (<see cref="DignityEngine"/>, degree-aware in D1 only — as ChartAnalyzer stores dignity);
/// (4) in the naturally stronger sign: dual &gt; fixed &gt; movable;
/// (5b) further advanced in its sign, Rahu/Ketu measured from the sign's end.
/// Rule (5a) (dasa length) is for rasi dasas, not padas, and is not used here.
///
/// Works on any chart: degrees are the chart's own (varga longitude when set, else the real one).
/// </summary>
public static class StrongerCoLord
{
    /// <summary>The lord used for <paramref name="sign"/>'s arudha pada in <paramref name="chart"/>.</summary>
    public static PlanetName For(ZodiacName sign, ChartAnalysisInput chart) => sign switch
    {
        ZodiacName.Scorpio => Stronger(PlanetName.Mars, PlanetName.Ketu, sign, chart),
        ZodiacName.Aquarius => Stronger(PlanetName.Saturn, PlanetName.Rahu, sign, chart),
        _ => Enum.Parse<PlanetName>(HouseEngine.GetSignLord(sign)),
    };

    /// <summary>The co-lord of <paramref name="sign"/> that rules it: <paramref name="a"/> wins
    /// a full tie (PVR gives no further rule).</summary>
    public static PlanetName Stronger(PlanetName a, PlanetName b, ZodiacName sign, ChartAnalysisInput chart)
    {
        var planets = chart.Planets
            .Where(p => p.Planet != "Ascendant" && Enum.TryParse<PlanetName>(p.Planet, out _))
            .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet));
        var signOf = planets.ToDictionary(kv => kv.Key, kv => Enum.Parse<ZodiacName>(kv.Value.Sign));
        double Degree(PlanetName p) => ((planets[p].VargaLongitudeDegrees ?? planets[p].NirayanaLongitudeDegrees ?? 0) % 30 + 30) % 30;

        // Basic rule.
        var (aIn, bIn) = (signOf[a] == sign, signOf[b] == sign);
        if (aIn != bIn) return aIn ? b : a;

        // (1) joined by more planets.
        int Joined(PlanetName p) => signOf.Count(kv => kv.Key != p && kv.Value == signOf[p]);
        if (Winner(a, b, Joined) is { } w1) return w1;

        // (2) Jupiter, Mercury, dispositor — conjoining or rāśi-aspecting the planet.
        bool Influences(PlanetName by, PlanetName on) =>
            by != on && signOf.TryGetValue(by, out var s)
            && (s == signOf[on] || RasiDrishtiCalculator.Aspects(s, signOf[on]));
        int Influence(PlanetName p)
        {
            var dispositor = Enum.Parse<PlanetName>(HouseEngine.GetSignLord(signOf[p]));
            return new[] { PlanetName.Jupiter, PlanetName.Mercury, dispositor }.Count(by => Influences(by, p));
        }
        if (Winner(a, b, Influence) is { } w2) return w2;

        // (3) exalted.
        var signNames = signOf.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
        bool Exalted(PlanetName p) =>
            DignityEngine.Evaluate(p.ToString(), signOf[p],
            planets[p].VargaLongitudeDegrees is null ? Degree(p) : null, signNames).DignityStatus == "Exalted";
        var (aEx, bEx) = (Exalted(a), Exalted(b));
        if (aEx != bEx) return aEx ? a : b;

        // (4) natural strength of the occupied sign.
        int Modality(PlanetName p) => BaadhakaCalculator.GetModality(signOf[p]) switch
        {
            SignModality.Dual => 2,
            SignModality.Fixed => 1,
            _ => 0,
        };
        if (Winner(a, b, Modality) is { } w4) return w4;

        // (5b) advancement in its sign; nodes from the end.
        double Advancement(PlanetName p) => p is PlanetName.Rahu or PlanetName.Ketu ? 30 - Degree(p) : Degree(p);
        return Winner(a, b, Advancement) ?? a;
    }

    private static PlanetName? Winner<T>(PlanetName a, PlanetName b, Func<PlanetName, T> score) where T : IComparable<T>
    {
        var cmp = score(a).CompareTo(score(b));
        return cmp == 0 ? null : cmp > 0 ? a : b;
    }
}
