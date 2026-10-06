using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Dasha;

public enum RasiDasaSystem { Narayana, LagnaKendradi, Sudasa, Drig }

/// <summary>One rasi dasa. <see cref="Years"/> may be fractional for the first period of a Sudasa; <see cref="LengthWhy"/>
/// shows how the length was counted.</summary>
public sealed record RasiDasaPeriod(
    int Cycle, ZodiacName Rasi, double Years, DateTime Start, DateTime End, PlanetName Lord, int HouseCount, string LengthWhy);

/// <summary>An antardasa inside a Narayana dasa: one rasi for <see cref="Start"/> to <see cref="End"/> (n months for an n-year dasa / 12).</summary>
public sealed record RasiAntardasa(ZodiacName Rasi, DateTime Start, DateTime End);

/// <summary>The antardasas of one Narayana dasa with how their order was found.</summary>
public sealed record RasiAntardasaPlan(
    ZodiacName Seed, string SeedWhy, ZodiacName StartRasi, bool Forward, string Pattern, IReadOnlyList<RasiAntardasa> Periods);

/// <summary>A rasi dasa system for one chart: where it starts and why, the order, and the dated periods (two cycles).</summary>
public sealed record RasiDasaPlan(
    RasiDasaSystem System, ZodiacName Seed, string SeedWhy, bool Forward, string Pattern,
    IReadOnlyList<ZodiacName> Sequence, IReadOnlyList<RasiDasaPeriod> Periods)
{
    public RasiDasaPeriod? Running(DateTime on) => Periods.FirstOrDefault(p => p.Start <= on && on < p.End);
}

/// <summary>
/// The four rasi dasas of P.V.R. Narasimha Rao, <i>Vedic Astrology: An Integrated Approach</i> (<c>SRC_PVR_INTEGRATED</c>):
/// Narayana (ch.18, pp.231-240), Lagna Kendradi Rasi (ch.19, pp.259-263), Sudasa (ch.20, pp.263-266) and Drigdasa
/// (ch.21, pp.267-269). All four find a dasa's length the same way (sec.18.2.2).
/// <list type="bullet">
/// <item><b>Length</b>: count the houses from the dasa rasi to its lord, forward when the rasi is odd-footed and
/// backward when even-footed, and subtract one. A lord in its own rasi (count one) gives 12 years; an exalted lord adds
/// a year; a debilitated lord takes one off. The stronger co-lord rules Scorpio and Aquarius. The second cycle's length is
/// 12 minus the first.</item>
/// <item><b>Narayana</b>: from the stronger of the Lagna and the 7th, by <see cref="NarayanaProgression"/>.</item>
/// <item><b>Lagna Kendradi</b>: from the stronger of the Lagna and the 7th; the kendras, then the panapharas, then the
/// apoklimas from it; forward when the Lagna is in an odd sign (the book's wording), Saturn in the seed forces forward,
/// Ketu there reverses.</item>
/// <item><b>Sudasa</b>: the same kendra-panaphara-apoklima order from the sign holding Sree Lagna, forward when that sign is
/// odd; only (30 degrees minus Sree Lagna's advancement) over 30 of the first dasa is left at birth.</item>
/// <item><b>Drigdasa</b>: from the 9th house; the 9th and the three signs it aspects, then the 10th and its three, then the
/// 11th and its three; each group counted forward from an odd-footed house and backward from an even-footed one.</item>
/// </list>
/// Antardasas are given for Narayana only (sec.18.3); the other chapters give dasa periods alone. Pure; no I/O.
/// </summary>
public static class RasiDasa
{
    private const double DaysPerYear = 365.25;

    public static RasiDasaPlan? Compute(RasiDasaSystem system, ChartAnalysisInput chart, DateTime birth, double? sreeLagnaLongitude = null)
    {
        var ctx = Ctx.From(chart);
        if (ctx is null) return null;
        var lagna = chart.AscendantSign;
        var seventh = HouseEngine.GetHouseSign(lagna, 7);

        ZodiacName seed;
        string seedWhy, pattern;
        bool forward;
        List<ZodiacName> sequence;
        double firstFraction = 1;

        switch (system)
        {
            case RasiDasaSystem.Narayana:
            {
                var n = NarayanaProgression.Read(chart);
                if (n is null) return null;
                (seed, seedWhy, pattern, forward, sequence) = (n.Seed,
                    $"Stronger of the Lagna ({Label(lagna)}) and the 7th ({Label(seventh)}): {Label(n.Seed)}, because {n.Comparison.Rule}.",
                    n.Pattern + (n.Variant == NarayanaVariant.Normal ? "" : $" ({n.Variant} exception)"), n.Forward, n.Sequence.ToList());
                break;
            }
            case RasiDasaSystem.LagnaKendradi:
            {
                var cmp = StrongerRasiComparator.Explain(lagna, seventh, chart);
                seed = cmp.Stronger;
                seedWhy = $"Stronger of the Lagna ({Label(lagna)}) and the 7th ({Label(seventh)}): {Label(seed)}, because {cmp.Rule}.";
                forward = IsOddSign(lagna);
                pattern = "kendras, then panapharas, then apoklimas from the seed";
                if (ctx.Sign[PlanetName.Saturn] == seed) { forward = true; pattern += ", forward because Saturn is in the seed"; }
                else if (ctx.Sign[PlanetName.Ketu] == seed) { forward = !forward; pattern += ", direction reversed because Ketu is in the seed"; }
                sequence = KendradiOrder(seed, forward);
                break;
            }
            case RasiDasaSystem.Sudasa:
            {
                if (sreeLagnaLongitude is not { } sl) return null;
                seed = (ZodiacName)(int)(((sl % 360) + 360) % 360 / 30);
                var advancement = ((sl % 30) + 30) % 30;
                firstFraction = (30 - advancement) / 30;
                seedWhy = $"The sign holding Sree Lagna ({Label(seed)}, {advancement:0.00}° in); {firstFraction:0.####} of the first dasa is left at birth.";
                forward = IsOddSign(seed);
                pattern = "kendras, then panapharas, then apoklimas from Sree Lagna";
                sequence = KendradiOrder(seed, forward);
                break;
            }
            case RasiDasaSystem.Drig:
            {
                seed = HouseEngine.GetHouseSign(lagna, 9);
                seedWhy = $"The 9th house from the Lagna ({Label(lagna)}): {Label(seed)}.";
                forward = IsOddFooted(seed);
                pattern = "the 9th and the three signs it aspects, then the 10th's, then the 11th's";
                sequence = DrigOrder(lagna);
                break;
            }
            default:
                return null;
        }

        var periods = new List<RasiDasaPeriod>();
        var lengths = sequence.Select(r => ctx.Length(r, chart)).ToList();
        var start = birth;
        for (var cycle = 1; cycle <= 2; cycle++)
        {
            for (var i = 0; i < sequence.Count; i++)
            {
                var (first, lord, count, why) = lengths[i];
                double years = cycle == 1 ? first : 12 - first;
                if (cycle == 1 && i == 0 && system == RasiDasaSystem.Sudasa) years *= firstFraction;
                if (years <= 0) continue;   // a 12-year first cycle leaves no second-cycle dasa
                var end = Advance(start, years);
                periods.Add(new(cycle, sequence[i], years, start, end, lord, count,
                    cycle == 1 ? why : $"12 minus the first cycle's {first} years"));
                start = end;
            }
        }
        return new(system, seed, seedWhy, forward, pattern, sequence, periods);
    }

    /// <summary>The twelve antardasas of a Narayana dasa (sec.18.3): from the rasi holding the lord of the stronger of the
    /// dasa rasi and its 7th, counted forward from an odd sign and backward from an even one; Saturn in that stronger rasi
    /// forces forward, Ketu there reverses. Each is an n-year dasa's n months.</summary>
    public static RasiAntardasaPlan? Antardasas(ChartAnalysisInput chart, RasiDasaPeriod dasa)
    {
        var ctx = Ctx.From(chart);
        if (ctx is null) return null;
        var seventh = HouseEngine.GetHouseSign(dasa.Rasi, 7);
        var cmp = StrongerRasiComparator.Explain(dasa.Rasi, seventh, chart);
        var seed = cmp.Stronger;
        var lord = StrongerCoLord.For(seed, chart);
        var startRasi = ctx.Sign[lord];
        var forward = IsOddSign(startRasi);
        var pattern = $"from {Label(startRasi)}, where {lord} (lord of {Label(seed)}) is, counted {(forward ? "forward" : "backward")} from an {(forward ? "odd" : "even")} sign";
        if (ctx.Sign[PlanetName.Saturn] == seed) { forward = true; pattern = $"from {Label(startRasi)}, forward because Saturn is in {Label(seed)}"; }
        else if (ctx.Sign[PlanetName.Ketu] == seed) { forward = !IsOddSign(startRasi); pattern = $"from {Label(startRasi)}, direction reversed because Ketu is in {Label(seed)}"; }

        var span = (dasa.End - dasa.Start).TotalDays / 12;
        var periods = Enumerable.Range(0, 12).Select(i =>
        {
            var rasi = (ZodiacName)((((int)startRasi + (forward ? i : -i)) % 12 + 12) % 12);
            return new RasiAntardasa(rasi, dasa.Start.AddDays(span * i), i == 11 ? dasa.End : dasa.Start.AddDays(span * (i + 1)));
        }).ToList();
        return new(seed, $"Stronger of {Label(dasa.Rasi)} and its 7th {Label(seventh)}: {Label(seed)}, because {cmp.Rule}.", startRasi, forward, pattern, periods);
    }

    // ---- orders ----------------------------------------------------------------------------------

    /// <summary>The kendras (1, 4, 7, 10), then the panapharas (2, 5, 8, 11), then the apoklimas (3, 6, 9, 12) from the seed.</summary>
    public static List<ZodiacName> KendradiOrder(ZodiacName seed, bool forward) =>
        new[] { 1, 4, 7, 10, 2, 5, 8, 11, 3, 6, 9, 12 }
            .Select(h => (ZodiacName)((((int)seed + (forward ? h - 1 : -(h - 1))) % 12 + 12) % 12)).ToList();

    /// <summary>Drigdasa's order: the 9th, 10th and 11th from the Lagna, each followed by the three signs it aspects in its own direction.</summary>
    public static List<ZodiacName> DrigOrder(ZodiacName lagna)
    {
        var result = new List<ZodiacName>();
        foreach (var house in new[] { 9, 10, 11 })
        {
            var head = HouseEngine.GetHouseSign(lagna, house);
            var step = IsOddFooted(head) ? 1 : -1;
            result.Add(head);
            var aspected = RasiDrishtiCalculator.GetAspectedSigns(head);
            result.AddRange(aspected.OrderBy(s => step > 0 ? ((int)s - (int)head + 12) % 12 : ((int)head - (int)s + 12) % 12));
        }
        return result;
    }

    // ---- helpers ---------------------------------------------------------------------------------

    private static DateTime Advance(DateTime from, double years)
    {
        var whole = (int)Math.Floor(years);
        return from.AddYears(whole).AddDays((years - whole) * DaysPerYear);
    }

    private static bool IsOddFooted(ZodiacName s) =>
        s is ZodiacName.Aries or ZodiacName.Taurus or ZodiacName.Gemini or ZodiacName.Libra or ZodiacName.Scorpio or ZodiacName.Sagittarius;

    /// <summary>Odd signs by number (Aries 1st, Gemini 3rd...), not odd-footed: the rule for Lagna Kendradi, Sudasa and antardasas.</summary>
    private static bool IsOddSign(ZodiacName s) => (int)s % 2 == 0;

    private static string Label(ZodiacName s) => s == ZodiacName.Capricornus ? "Capricorn" : s.ToString();

    private sealed class Ctx
    {
        public required Dictionary<PlanetName, ZodiacName> Sign { get; init; }
        public required Dictionary<PlanetName, double> Degree { get; init; }
        public required Dictionary<PlanetName, bool> VargaPositions { get; init; }
        private Dictionary<string, ZodiacName> _names = null!;

        public static Ctx? From(ChartAnalysisInput chart)
        {
            var planets = chart.Planets
                .Where(p => p.Planet != "Ascendant" && Enum.TryParse<PlanetName>(p.Planet, out _))
                .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet));
            if (planets.Count != 9) return null;
            var c = new Ctx
            {
                Sign = planets.ToDictionary(kv => kv.Key, kv => Enum.Parse<ZodiacName>(kv.Value.Sign)),
                Degree = planets.ToDictionary(kv => kv.Key, kv => (((kv.Value.VargaLongitudeDegrees ?? kv.Value.NirayanaLongitudeDegrees ?? 0) % 30) + 30) % 30),
                VargaPositions = planets.ToDictionary(kv => kv.Key, kv => kv.Value.VargaLongitudeDegrees is not null),
            };
            c._names = c.Sign.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
            return c;
        }

        /// <summary>Sec.18.2.2: the first-cycle length, the lord, the house count and the working.</summary>
        public (int Years, PlanetName Lord, int Count, string Why) Length(ZodiacName rasi, ChartAnalysisInput chart)
        {
            var lord = StrongerCoLord.For(rasi, chart);
            var lordSign = Sign[lord];
            var forward = IsOddFooted(rasi);
            var count = forward ? ((int)lordSign - (int)rasi + 12) % 12 + 1 : ((int)rasi - (int)lordSign + 12) % 12 + 1;
            var years = count == 1 ? 12 : count - 1;
            var why = count == 1
                ? $"{lord} is in {Label(rasi)} itself (count 1): 12 years"
                : $"{lord} is the {count}th {(forward ? "forward" : "backward")} from {Label(rasi)}: {count} - 1 = {count - 1}";
            var dignity = DignityEngine.Evaluate(lord.ToString(), lordSign, VargaPositions[lord] ? null : Degree[lord], _names).DignityStatus;
            if (dignity == "Exalted") { years += 1; why += ", +1 (exalted)"; }
            else if (dignity == "Debilitated") { years -= 1; why += ", -1 (debilitated)"; }
            return (years, lord, count, why);
        }
    }
}
