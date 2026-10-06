using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>A planet chosen by one of the PVR §14.3 rules, with the rule that settled it.</summary>
public sealed record RudraPlanet(PlanetName Planet, ZodiacName Sign, string Why);

/// <summary>Rudra and the Trishoola rasis. <see cref="Rudra"/> is the stronger of the two candidate lords;
/// <see cref="AlsoRudra"/> is the weaker one when it is afflicted enough to count too. A Trishoola is the three
/// trines (the Rudra's sign, the 5th and the 9th from it) of each Rudra.</summary>
public sealed record RudraReading(
    ZodiacName EighthFromLagna, PlanetName LordFromLagna,
    ZodiacName SeventhSign, ZodiacName EighthFromSeventh, PlanetName LordFromSeventh,
    RudraPlanet Rudra, IReadOnlyList<ZodiacName> Trishoola,
    RudraPlanet? AlsoRudra, IReadOnlyList<ZodiacName> AlsoTrishoola);

/// <summary>Maheswara: the lord of the 8th house from the Atma Karaka, with the house actually counted (8, or 6
/// when a node sits in the Atma Karaka's sign or its 8th) and the note on any exception applied.</summary>
public sealed record MaheswaraReading(
    PlanetName AtmaKaraka, ZodiacName AtmaKarakaSign, ZodiacName HouseSign, int HouseCounted,
    RudraPlanet Maheswara, string? Exception);

/// <summary>
/// Rudra, Trishoola and Maheswara, P.V.R. Narasimha Rao, <i>Vedic Astrology: An Integrated Approach</i>,
/// sec.14.3, printed pp.182-183 (<c>SRC_PVR_INTEGRATED</c>, checked against the raw extract and the book's
/// Exercise 23 answer, p.186).
/// <list type="bullet">
/// <item><b>Rudra</b>: the stronger of the lords of the 8th house from the Lagna and from the 7th house, the 8th
/// found in PVR's Table 32 (not by plain counting). Stronger means, in order: joined by more planets; exalted or
/// in its own sign; joined by more exalted planets; aspected (rasi drishti) by more planets; further advanced in
/// its sign. If the weaker one is debilitated or in an enemy's sign and conjoined or aspected by Mars, Saturn, Rahu
/// or Ketu, it is Rudra as well.</item>
/// <item><b>Trishoola rasis</b>: the three trines from the sign Rudra occupies.</item>
/// <item><b>Maheswara</b>: the lord of the 8th from the Atma Karaka (plain counting, as the book's examples do).
/// If that lord is in its own or exaltation sign, the stronger of the 8th and 12th lords from it. If Rahu or Ketu
/// joins the Atma Karaka or its 8th (a node that is itself the Atma Karaka does not count), the 6th house from the Atma Karaka is read instead. A node as Maheswara is
/// replaced by Mercury (Rahu) or Jupiter (Ketu).</item>
/// </list>
/// A sign's lord is the stronger co-lord for Scorpio and Aquarius (<see cref="StrongerCoLord"/>), as everywhere
/// else in the repo. Where the book gives a tie no rule, the planet read first wins. The book itself says these
/// definitions are not infallible and that longevity knowledge is not to be used to frighten; they are
/// reference points for timing, shown as facts. Pure; no I/O.
/// </summary>
public static class RudraMaheswara
{
    // PVR Table 32, "Eighth House for Rudra Calculation", Aries to Pisces.
    private static readonly ZodiacName[] Table32 =
    [
        ZodiacName.Scorpio, ZodiacName.Gemini, ZodiacName.Capricornus, ZodiacName.Sagittarius, ZodiacName.Cancer, ZodiacName.Aquarius,
        ZodiacName.Taurus, ZodiacName.Sagittarius, ZodiacName.Cancer, ZodiacName.Gemini, ZodiacName.Capricornus, ZodiacName.Leo,
    ];

    /// <summary>The 8th house from <paramref name="sign"/> by PVR's Table 32.</summary>
    public static ZodiacName RudraEighth(ZodiacName sign) => Table32[(int)sign];

    public static RudraReading? ReadRudra(ChartAnalysisInput chart)
    {
        var ctx = Context.From(chart);
        if (ctx is null) return null;

        var seventh = HouseEngine.GetHouseSign(chart.AscendantSign, 7);
        var eighthA = RudraEighth(chart.AscendantSign);
        var eighthB = RudraEighth(seventh);
        var lordA = StrongerCoLord.For(eighthA, chart);
        var lordB = StrongerCoLord.For(eighthB, chart);

        PlanetName rudra, other;
        string why;
        if (lordA == lordB) { rudra = other = lordA; why = "Lord of the 8th from both the Lagna and the 7th."; }
        else
        {
            var (winner, reason) = ctx.Stronger(lordA, lordB);
            rudra = winner;
            other = winner == lordA ? lordB : lordA;
            why = $"Stronger of {lordA} (8th from Lagna) and {lordB} (8th from the 7th): {reason}.";
        }

        RudraPlanet? also = null;
        if (other != rudra && ctx.IsAfflictedEnemy(other))
            also = new(other, ctx.Sign[other], $"Weaker of the two, but {other} is {ctx.DignityOf(other).ToLowerInvariant()} and conjoined or aspected by Mars, Saturn, Rahu or Ketu.");

        return new(eighthA, lordA, seventh, eighthB, lordB,
            new(rudra, ctx.Sign[rudra], why), Trines(ctx.Sign[rudra]),
            also, also is null ? [] : Trines(also.Sign));
    }

    public static MaheswaraReading? ReadMaheswara(ChartAnalysisInput chart, PlanetName atmaKaraka)
    {
        var ctx = Context.From(chart);
        if (ctx is null || !ctx.Sign.TryGetValue(atmaKaraka, out var akSign)) return null;

        var eighth = HouseEngine.GetHouseSign(akSign, 8);
        var nodeNear = new[] { PlanetName.Rahu, PlanetName.Ketu }.Any(n => n != atmaKaraka && (ctx.Sign[n] == akSign || ctx.Sign[n] == eighth));
        var counted = nodeNear ? 6 : 8;
        var houseSign = HouseEngine.GetHouseSign(akSign, counted);
        var lord = StrongerCoLord.For(houseSign, chart);
        string? exception = nodeNear ? "Rahu or Ketu joins the Atma Karaka or its 8th, so the 6th from the Atma Karaka is read." : null;

        if (!nodeNear && ctx.InOwnOrExaltation(lord))
        {
            var lordSign = ctx.Sign[lord];
            var a = StrongerCoLord.For(HouseEngine.GetHouseSign(lordSign, 8), chart);
            var b = StrongerCoLord.For(HouseEngine.GetHouseSign(lordSign, 12), chart);
            var (winner, _) = a == b ? (a, "") : ctx.Stronger(a, b);
            exception = $"The 8th lord {lord} is in its own or exaltation sign, so the stronger of the 8th and 12th lords from it ({a}, {b}) is read.";
            lord = winner;
        }

        var substituted = lord switch { PlanetName.Rahu => PlanetName.Mercury, PlanetName.Ketu => PlanetName.Jupiter, _ => lord };
        if (substituted != lord)
            exception = (exception is null ? "" : exception + " ") + $"{lord} as Maheswara is replaced by {substituted}.";

        return new(atmaKaraka, akSign, houseSign, counted,
            new(substituted, ctx.Sign[substituted], $"Lord of the {counted}th house ({houseSign}) from the Atma Karaka {atmaKaraka}."), exception);
    }

    private static IReadOnlyList<ZodiacName> Trines(ZodiacName sign) =>
        [sign, HouseEngine.GetHouseSign(sign, 5), HouseEngine.GetHouseSign(sign, 9)];

    /// <summary>Per-chart facts the strength comparisons read.</summary>
    private sealed class Context
    {
        public required Dictionary<PlanetName, ZodiacName> Sign { get; init; }
        public required Dictionary<PlanetName, double> Degree { get; init; }
        private Dictionary<string, ZodiacName> _names = null!;

        public static Context? From(ChartAnalysisInput chart)
        {
            var planets = chart.Planets
                .Where(p => p.Planet != "Ascendant" && Enum.TryParse<PlanetName>(p.Planet, out _))
                .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet));
            if (planets.Count != 9) return null;
            double Deg(Ikiastrro.Core.Models.PlanetPosition p) =>
                (((p.VargaLongitudeDegrees ?? p.NirayanaLongitudeDegrees ?? 0) % 30) + 30) % 30;
            var ctx = new Context
            {
                Sign = planets.ToDictionary(kv => kv.Key, kv => Enum.Parse<ZodiacName>(kv.Value.Sign)),
                Degree = planets.ToDictionary(kv => kv.Key, kv => Deg(kv.Value)),
            };
            ctx._names = ctx.Sign.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
            return ctx;
        }

        public string DignityOf(PlanetName p) =>
            DignityEngine.Evaluate(p.ToString(), Sign[p], Degree[p], _names).DignityStatus ?? "Neutral";

        private bool Exalted(PlanetName p) => DignityOf(p) == "Exalted";
        public bool InOwnOrExaltation(PlanetName p) => DignityOf(p) is "Exalted" or "Own Sign" or "Moolatrikona";

        private IEnumerable<PlanetName> Joined(PlanetName p) => Sign.Keys.Where(o => o != p && Sign[o] == Sign[p]);
        private int AspectedBy(PlanetName p) => Sign.Keys.Count(o => o != p && RasiDrishtiCalculator.Aspects(Sign[o], Sign[p]));

        /// <summary>Conjoined or rasi-aspected by Mars, Saturn, Rahu or Ketu, and debilitated or in an enemy sign.</summary>
        public bool IsAfflictedEnemy(PlanetName p)
        {
            var weak = DignityOf(p) is "Debilitated" or "Enemy" or "Great Enemy";
            var malefics = new[] { PlanetName.Mars, PlanetName.Saturn, PlanetName.Rahu, PlanetName.Ketu };
            var touched = malefics.Any(m => m != p && (Sign[m] == Sign[p] || RasiDrishtiCalculator.Aspects(Sign[m], Sign[p])));
            return weak && touched;
        }

        /// <summary>The stronger of two planets by the §14.3 order; the first wins a full tie.</summary>
        public (PlanetName Winner, string Reason) Stronger(PlanetName a, PlanetName b)
        {
            (PlanetName, string)? Decide<T>(Func<PlanetName, T> score, Func<T, T, string> say) where T : IComparable<T>
            {
                var c = score(a).CompareTo(score(b));
                return c == 0 ? null : c > 0 ? (a, say(score(a), score(b))) : (b, say(score(b), score(a)));
            }

            return Decide(p => Joined(p).Count(), (w, l) => $"joined by more planets ({w} against {l})")
                ?? Decide(p => InOwnOrExaltation(p) ? 1 : 0, (_, _) => "in exaltation or its own sign")
                ?? Decide(p => Joined(p).Count(Exalted), (w, l) => $"joined by more exalted planets ({w} against {l})")
                ?? Decide(AspectedBy, (w, l) => $"aspected by more planets ({w} against {l})")
                ?? Decide(p => Degree[p], (_, _) => "further advanced in its sign")
                ?? (a, "equal on every test, the first read is kept");
        }
    }
}
