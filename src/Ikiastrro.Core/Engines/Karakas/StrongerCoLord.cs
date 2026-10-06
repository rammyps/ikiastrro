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
    public static PlanetName Stronger(PlanetName a, PlanetName b, ZodiacName sign, ChartAnalysisInput chart) =>
        Decide(a, b, sign, chart).Winner;

    /// <summary>
    /// Scorpio's or Aquarius's two lords, the one that rules in this chart, and the rule that decided it, for display
    /// beside the house lord (Astro Facts, About Houses). Null for any other sign, which has one lord. Same decision as
    /// <see cref="For"/>; this only adds the reason.
    /// </summary>
    public static CoLordReading? Explain(ZodiacName sign, ChartAnalysisInput chart)
    {
        var (a, b) = sign switch
        {
            ZodiacName.Scorpio => (PlanetName.Mars, PlanetName.Ketu),
            ZodiacName.Aquarius => (PlanetName.Saturn, PlanetName.Rahu),
            _ => (default(PlanetName), default(PlanetName)),
        };
        if (sign is not (ZodiacName.Scorpio or ZodiacName.Aquarius)) return null;
        var (winner, rule) = Decide(a, b, sign, chart);
        return new CoLordReading(sign, a, b, winner, rule);
    }

    private static (PlanetName Winner, string Rule) Decide(PlanetName a, PlanetName b, ZodiacName sign, ChartAnalysisInput chart)
    {
        var planets = chart.Planets
            .Where(p => p.Planet != "Ascendant" && Enum.TryParse<PlanetName>(p.Planet, out _))
            .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet));
        var signOf = planets.ToDictionary(kv => kv.Key, kv => Enum.Parse<ZodiacName>(kv.Value.Sign));
        double Degree(PlanetName p) => ((planets[p].VargaLongitudeDegrees ?? planets[p].NirayanaLongitudeDegrees ?? 0) % 30 + 30) % 30;

        (PlanetName, string)? Pick<T>(Func<PlanetName, T> score, Func<T, T, string> say) where T : IComparable<T>
        {
            var cmp = score(a).CompareTo(score(b));
            if (cmp == 0) return null;
            var (w, l) = cmp > 0 ? (a, b) : (b, a);
            return (w, say(score(w), score(l)));
        }

        // Basic rule.
        var (aIn, bIn) = (signOf[a] == sign, signOf[b] == sign);
        if (aIn != bIn) return aIn ? (b, $"{a} is in {sign}, so {b} rules it") : (a, $"{b} is in {sign}, so {a} rules it");

        // (1) joined by more planets.
        int Joined(PlanetName p) => signOf.Count(kv => kv.Key != p && kv.Value == signOf[p]);
        if (Pick(Joined, (w, l) => $"joined by more planets ({w} against {l})") is { } r1) return r1;

        // (2) Jupiter, Mercury, dispositor — conjoining or rāśi-aspecting the planet.
        bool Influences(PlanetName by, PlanetName on) =>
            by != on && signOf.TryGetValue(by, out var s)
            && (s == signOf[on] || RasiDrishtiCalculator.Aspects(s, signOf[on]));
        int Influence(PlanetName p)
        {
            var dispositor = Enum.Parse<PlanetName>(HouseEngine.GetSignLord(signOf[p]));
            return new[] { PlanetName.Jupiter, PlanetName.Mercury, dispositor }.Count(by => Influences(by, p));
        }
        if (Pick(Influence, (w, l) => $"conjoined or aspected by more of Jupiter, Mercury and its dispositor ({w} against {l})") is { } r2) return r2;

        // (3) exalted.
        var signNames = signOf.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
        bool Exalted(PlanetName p) =>
            DignityEngine.Evaluate(p.ToString(), signOf[p],
            planets[p].VargaLongitudeDegrees is null ? Degree(p) : null, signNames).DignityStatus == "Exalted";
        var (aEx, bEx) = (Exalted(a), Exalted(b));
        if (aEx != bEx) return aEx ? (a, $"{a} is exalted") : (b, $"{b} is exalted");

        // (4) natural strength of the occupied sign.
        int Modality(PlanetName p) => BaadhakaCalculator.GetModality(signOf[p]) switch
        {
            SignModality.Dual => 2,
            SignModality.Fixed => 1,
            _ => 0,
        };
        if (Pick(Modality, (_, _) => "in the naturally stronger sign (dual over fixed over movable)") is { } r4) return r4;

        // (5b) advancement in its sign; nodes from the end.
        double Advancement(PlanetName p) => p is PlanetName.Rahu or PlanetName.Ketu ? 30 - Degree(p) : Degree(p);
        if (Pick(Advancement, (_, _) => "further advanced in its sign") is { } r5) return r5;

        return (a, $"equal on every test, so {a} is kept");
    }
}

/// <summary>Scorpio's (Mars, Ketu) or Aquarius's (Saturn, Rahu) two lords, the stronger one, and the rule that
/// decided it (PVR sec.15.5.1).</summary>
public sealed record CoLordReading(ZodiacName Sign, PlanetName Primary, PlanetName CoLord, PlanetName Stronger, string Rule);
