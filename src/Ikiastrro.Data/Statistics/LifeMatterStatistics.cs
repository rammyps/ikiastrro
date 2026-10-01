using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;
using Ikiastrro.Data;

namespace Ikiastrro.Data.Statistics;

/// <summary>Three-way reading of one strength statistic, plus None when there is nothing to read.
/// Strong/Weak are the app's existing bands, not outcome judgments.</summary>
public enum StrengthBand { Strong, Middle, Weak, None }

public enum ArgalaVerdict { Holds, Contested, Obstructed }

/// <summary>One overall reading of the banded signals (strong count minus weak count): ±2 or more is
/// Strong/Weak, ±1 leans that way, 0 is Mixed; None when no signal can be read. A strength summary,
/// never an outcome.</summary>
public enum StrengthLean { Strong, LeansStrong, Mixed, LeansWeak, Weak, None }

/// <summary>One Argala pair on a house: planets intervening from <see cref="ArgalaOffset"/> and
/// the planets obstructing them from <see cref="ObstructionOffset"/>. <see cref="Verdict"/> is null
/// when only obstructing planets are present (nothing to obstruct).</summary>
public sealed record ArgalaPair(
    int ArgalaOffset, int ObstructionOffset, string Kind,
    IReadOnlyList<string> ArgalaPlanets, IReadOnlyList<string> ObstructingPlanets,
    bool ExceptionApplied, ArgalaVerdict? Verdict);

public sealed record ArgalaSummary(IReadOnlyList<ArgalaPair> Pairs)
{
    public int Holds => Pairs.Count(p => p.Verdict == ArgalaVerdict.Holds);
    public int Contested => Pairs.Count(p => p.Verdict == ArgalaVerdict.Contested);
    public int Obstructed => Pairs.Count(p => p.Verdict == ArgalaVerdict.Obstructed);
    public int Net => Holds - Obstructed;
    public bool Any => Pairs.Any(p => p.ArgalaPlanets.Count > 0 || p.ObstructingPlanets.Count > 0);
}

/// <summary>One planet's strength read for a matter: its Ṣaḍbala % of required minimum (Capacity)
/// and Shodasavarga Amsabala % (Consistency). Null where the planet has none (Rahu/Ketu).</summary>
public sealed record PlanetStrength(string Planet, decimal? ShadbalaPercent, int? AmsabalaPercent);

/// <summary>Strength statistics for one sign of one chart, read as a house from that chart's Lagna.
/// Every sign figure is sign-based, so a house counted from any lagna reads the same facts. Grouped
/// into stat_strength.md §4's three axes, each kept separate:
/// <list type="bullet">
/// <item>Capacity — Ṣaḍbala of the sign's lord and the matter's kārakas.</item>
/// <item>Consistency — the same planets' Shodasavarga Amsabala.</item>
/// <item>Context — the sign itself: SAV, the lord's own BAV there, independent Bhava Bala
/// (chart-relative, lord's Ṣaḍbala excluded so it isn't counted twice), and Argala.</item>
/// </list></summary>
public sealed record HouseStatistics(
    string Sign, int HouseFromLagna, int? SavBindus,
    decimal? BhavaBalaRupas, decimal? IndependentBhavaRupas, double? IndependentBhavaZ,
    string LordPlanet, decimal? LordShadbalaPercent, int? LordBavBindus, int? LordAmsabalaPercent,
    ArgalaSummary Argala, IReadOnlyList<PlanetStrength> Karakas)
{
    public StrengthBand SavBand => LifeMatterStatistics.SavBand(SavBindus);
    public StrengthBand LordBavBand => LifeMatterStatistics.BavBand(LordBavBindus);
    public StrengthBand BhavaBand => LifeMatterStatistics.IndependentBhavaBand(IndependentBhavaZ);
    public StrengthBand LordShadbalaBand => LifeMatterStatistics.ShadbalaBand(LordShadbalaPercent);
    public StrengthBand ArgalaBand => !Argala.Any ? StrengthBand.None
        : Argala.Net > 0 ? StrengthBand.Strong : Argala.Net < 0 ? StrengthBand.Weak : StrengthBand.Middle;

    /// <summary>The banded signals, for the Strong-minus-Weak reading and sort: SAV · lord's BAV ·
    /// independent Bhava Bala · lord Ṣaḍbala · Argala.</summary>
    public IReadOnlyList<StrengthBand> Bands => [SavBand, LordBavBand, BhavaBand, LordShadbalaBand, ArgalaBand];

    /// <summary>The lord first, then the matter's kārakas, each planet once.</summary>
    public IReadOnlyList<PlanetStrength> Planets =>
        [new PlanetStrength(LordPlanet, LordShadbalaPercent, LordAmsabalaPercent),
         .. Karakas.Where(k => !string.Equals(k.Planet, LordPlanet, StringComparison.OrdinalIgnoreCase))];

    /// <summary>Context's four parts as 0–100 indices: SAV · lord's BAV · independent Bhava Bala · Argala.</summary>
    public IReadOnlyList<int?> ContextParts =>
    [
        LifeMatterStatistics.SavIndex(SavBindus), LifeMatterStatistics.BavIndex(LordBavBindus),
        LifeMatterStatistics.IndependentBhavaIndex(IndependentBhavaZ), LifeMatterStatistics.ArgalaIndex(Argala)
    ];

    public int? Capacity => LifeMatterStatistics.Mean(Planets.Select(p => LifeMatterStatistics.ShadbalaIndex(p.ShadbalaPercent)));
    public int? Consistency => LifeMatterStatistics.Mean(Planets.Select(p => p.AmsabalaPercent));
    public int? Context => LifeMatterStatistics.Mean(ContextParts);

    /// <summary>Capacity · Consistency · Context, in that order.</summary>
    public IReadOnlyList<int?> Axes => [Capacity, Consistency, Context];

    /// <summary>The page's one strength figure: the mean of the readable axes, so each axis counts
    /// equally however many signals it holds. Null when no axis can be read.</summary>
    public int? StrengthPercent => LifeMatterStatistics.Mean(Axes);
}

/// <summary>
/// Statistics for the Key Inference page, built once per person per chart (D1 or any varga) from
/// persisted facts (vw_ChartAshtakavarga, vw_ChartBhavaBala + tbl_Fact_BhavaStrengthComponent,
/// vw_ChartShadbala, vw_ChartAmsabala, tbl_Fact_Argala) and queried per sign. In a varga: SAV and
/// the lord's BAV are that varga's own Ashtakavarga; Argala is the varga's own occupancy; the lord
/// is the varga sign's lord; Ṣaḍbala and Amsabala exist once per planet; Bhava Bala is a D1 house
/// computation and reads as nothing in a varga. Bands are StrengthBands — the cut-offs Key
/// Inference step 3 shows. They describe strength, not outcomes.
/// </summary>
public sealed class LifeMatterStatistics
{
    // Primary pairs (PVR ch.9): 2nd/12th, 4th/10th, 11th/3rd, then secondary 5th/9th. A 3rd-house
    // argala (malefics) is obstructed from the 11th and is added only when present.
    private static readonly (int Argala, int Obstruction, string Kind)[] Pairs =
        [(2, 12, "Primary"), (4, 10, "Primary"), (11, 3, "Primary"), (5, 9, "Secondary")];

    /// <summary>Consistency reads one canonical Amsabala scheme, never an average of the four
    /// overlapping ones (stat_strength.md §1.2): Shodasavarga, since every varga is generated.</summary>
    public const string AmsabalaScheme = "SHODASAVARGA";

    public string ChartType { get; }
    private readonly ZodiacName _ascendant;
    private readonly IReadOnlyDictionary<int, int> _savBySignNumber;
    private readonly IReadOnlyDictionary<(string Planet, int SignNumber), int> _bavByPlanetSign;
    private readonly IReadOnlyDictionary<int, decimal> _bhavaByHouse;
    private readonly IReadOnlyDictionary<int, decimal> _independentBhavaByHouse;
    private readonly IReadOnlyDictionary<int, double> _independentBhavaZByHouse;
    private readonly IReadOnlyDictionary<string, decimal?> _shadbalaPercentByPlanet;
    private readonly IReadOnlyDictionary<string, int> _amsabalaPercentByPlanet;
    private readonly ILookup<int, ArgalaFactRow> _argalaByHouse;

    public LifeMatterStatistics(
        string d1AscendantSign,
        IEnumerable<AshtakavargaRow> ashtakavarga,
        IEnumerable<BhavaBalaSummaryRow> bhavaBala,
        IEnumerable<ShadbalaSummaryRow> shadbala,
        IEnumerable<ArgalaFactRow> argala)
        : this("D1", d1AscendantSign, ashtakavarga, bhavaBala, shadbala, argala)
    {
    }

    /// <param name="argala">House-target Argala facts for <paramref name="chartType"/>; rows for other
    /// charts are ignored. Only D1 is persisted today, so a varga's rows come from
    /// <see cref="ArgalaFacts.ForChart"/>.</param>
    /// <param name="bhavaComponents">D1's tbl_Fact_BhavaStrengthComponent rows, for independent
    /// Bhava Bala; none means Bhava Bala's part of Context can't be read.</param>
    /// <param name="amsabala">The person's Amsabala rows (all schemes); only
    /// <see cref="AmsabalaScheme"/> is read.</param>
    public LifeMatterStatistics(
        string chartType,
        string ascendantSign,
        IEnumerable<AshtakavargaRow> ashtakavarga,
        IEnumerable<BhavaBalaSummaryRow> bhavaBala,
        IEnumerable<ShadbalaSummaryRow> shadbala,
        IEnumerable<ArgalaFactRow> argala,
        IEnumerable<BhavaBalaComponentRow>? bhavaComponents = null,
        IEnumerable<AmsabalaRow>? amsabala = null)
    {
        ChartType = chartType;
        _ascendant = Enum.Parse<ZodiacName>(ascendantSign);
        var chartAv = ashtakavarga.Where(r => r.ChartType == chartType).ToList();
        _savBySignNumber = chartAv
            .Where(r => r.SarvaBindus is not null)
            .GroupBy(r => (int)r.SignNumber)
            .ToDictionary(g => g.Key, g => (int)g.First().SarvaBindus!.Value);
        _bavByPlanetSign = chartAv
            .GroupBy(r => (r.RecipientCode.ToUpperInvariant(), (int)r.SignNumber))
            .ToDictionary(g => g.Key, g => (int)g.First().BinduCount);
        var isD1 = chartType == "D1";
        _bhavaByHouse = isD1
            ? bhavaBala.ToDictionary(r => (int)r.HouseNumber, r => r.BhavaBalaRupas)
            : new Dictionary<int, decimal>();
        _independentBhavaByHouse = isD1 && bhavaComponents is not null
            ? IndependentBhavaBala.RupasByHouse(bhavaComponents)
            : new Dictionary<int, decimal>();
        _independentBhavaZByHouse = ZScores(_independentBhavaByHouse);
        _shadbalaPercentByPlanet = shadbala.ToDictionary(r => r.Planet, r => r.PercentOfMinimum, StringComparer.OrdinalIgnoreCase);
        _amsabalaPercentByPlanet = (amsabala ?? [])
            .Where(r => r.SchemeCode == AmsabalaScheme && r.GroupSize > 0)
            .GroupBy(r => r.PlanetCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (int)Math.Round(100.0 * g.First().GoodCount / g.First().GroupSize, MidpointRounding.AwayFromZero),
                StringComparer.OrdinalIgnoreCase);
        _argalaByHouse = argala
            .Where(r => r.ChartType == chartType && r.TargetKind == "House")
            .ToLookup(r => (int)r.TargetHouseNumber);
    }

    /// <param name="karakas">The matter's kāraka planets in this chart; they join the lord in
    /// Capacity and Consistency. None for a house read with no matter (the area summary).</param>
    public HouseStatistics ForSign(string sign, IEnumerable<string>? karakas = null)
    {
        var zodiac = Enum.Parse<ZodiacName>(sign);
        var house = AstroMath.CountFromSignToSign(_ascendant, zodiac);
        var lord = HouseEngine.GetSignLord(zodiac);
        var signNumber = (int)zodiac + 1;
        return new HouseStatistics(
            sign, house,
            _savBySignNumber.TryGetValue(signNumber, out var sav) ? sav : null,
            _bhavaByHouse.TryGetValue(house, out var bhava) ? bhava : null,
            _independentBhavaByHouse.TryGetValue(house, out var independent) ? independent : null,
            _independentBhavaZByHouse.TryGetValue(house, out var z) ? z : null,
            lord,
            _shadbalaPercentByPlanet.GetValueOrDefault(lord),
            _bavByPlanetSign.TryGetValue((lord.ToUpperInvariant(), signNumber), out var bav) ? bav : null,
            AmsabalaPercent(lord),
            BuildArgala(_argalaByHouse[house].ToList()),
            (karakas ?? []).Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(k => new PlanetStrength(k, _shadbalaPercentByPlanet.GetValueOrDefault(k), AmsabalaPercent(k)))
                .ToList());
    }

    private int? AmsabalaPercent(string planet) =>
        _amsabalaPercentByPlanet.TryGetValue(planet, out var pct) ? pct : null;

    /// <summary>Each house's z-score against the chart's own houses (population SD); every house
    /// reads 0 when they're all equal. Empty when fewer than two houses are known.</summary>
    private static IReadOnlyDictionary<int, double> ZScores(IReadOnlyDictionary<int, decimal> byHouse)
    {
        if (byHouse.Count < 2) return new Dictionary<int, double>();
        var values = byHouse.Values.Select(v => (double)v).ToList();
        var mean = values.Average();
        var sd = Math.Sqrt(values.Sum(v => (v - mean) * (v - mean)) / values.Count);
        return byHouse.ToDictionary(kv => kv.Key, kv => sd == 0 ? 0 : ((double)kv.Value - mean) / sd);
    }

    public decimal? ShadbalaPercent(string planet) => _shadbalaPercentByPlanet.GetValueOrDefault(planet);

    public static ArgalaSummary BuildArgala(IReadOnlyList<ArgalaFactRow> facts)
    {
        List<string> On(string relation, int offset) => facts
            .Where(f => f.RelationTypeCode == relation && f.HouseOffset == offset)
            .Select(f => f.OccupantPlanet).ToList();

        var pairs = new List<ArgalaPair>();
        foreach (var (argala, obstruction, kind) in Pairs)
            pairs.Add(Pair(argala, obstruction, kind));
        if (On("ARGALA", 3).Count > 0)
            pairs.Add(Pair(3, 11, "Malefic 3rd"));
        return new ArgalaSummary(pairs);

        ArgalaPair Pair(int argala, int obstruction, string kind)
        {
            var a = On("ARGALA", argala);
            var v = On("VIRODHARGALA", obstruction);
            ArgalaVerdict? verdict = a.Count == 0 ? null
                : a.Count > v.Count ? ArgalaVerdict.Holds
                : a.Count == v.Count ? ArgalaVerdict.Contested
                : ArgalaVerdict.Obstructed;
            var exception = facts.Any(f => f.RelationTypeCode == "ARGALA" && f.HouseOffset == argala && f.ExceptionApplied);
            return new ArgalaPair(argala, obstruction, kind, a, v, exception, verdict);
        }
    }

    // The three bands read StrengthBands (tbl_Rule_StrengthBand, migration 154) — the same cut-offs
    // Astro Facts step 3 shows, so a planet or house never gets two different labels.
    public static StrengthBand SavBand(int? bindus) => ToBand(StrengthBands.SarvaAshtakavargaBindus.Classify(bindus));

    public static StrengthBand BavBand(int? bindus) => ToBand(StrengthBands.BhinnaAshtakavargaBindus.Classify(bindus));

    public static StrengthBand IndependentBhavaBand(double? z) =>
        ToBand(StrengthBands.IndependentBhavaBalaZ.Classify(z is { } v ? (decimal)v : null));

    public static StrengthBand ShadbalaBand(decimal? percentOfMinimum) =>
        ToBand(StrengthBands.ShadbalaPercentOfMinimum.Classify(percentOfMinimum));

    private static StrengthBand ToBand(StrengthTier tier) => tier switch
    {
        StrengthTier.Strong => StrengthBand.Strong,
        StrengthTier.Moderate => StrengthBand.Middle,
        StrengthTier.Weak => StrengthBand.Weak,
        _ => StrengthBand.None,
    };

    // 0–100 strength indices, each centred on its own reference point so the middle reads about 50:
    // SAV out of 56 bindus (28, the average sign, is 50%); a planet's BAV out of 8 (4, the middle, is
    // 50%); independent Bhava Bala as 50 + 10z against the chart's 12 houses (the chart's own mean
    // is 50%, stat_strength.md §1.3); Ṣaḍbala % of minimum out of 200 (the required minimum is 50%);
    // Argala as the share of pairs that hold (contested counts half, obstruction-only is 50%).
    // Amsabala (Consistency) is its own share of vargas, 0–100. Presentation scales, not sourced
    // rules — strength, never an outcome.
    public static int? SavIndex(int? bindus) => bindus is { } b ? Index(b / 56.0) : null;

    public static int? BavIndex(int? bindus) => bindus is { } b ? Index(b / 8.0) : null;

    public static int? IndependentBhavaIndex(double? z) => z is { } v ? Index((50 + 10 * v) / 100) : null;

    public static int? ShadbalaIndex(decimal? percentOfMinimum) => percentOfMinimum is { } p ? Index((double)p / 200) : null;

    public static int? ArgalaIndex(ArgalaSummary argala)
    {
        if (!argala.Any) return null;
        var judged = argala.Pairs.Where(p => p.Verdict is not null).ToList();
        if (judged.Count == 0) return 50;
        var score = judged.Sum(p => p.Verdict switch { ArgalaVerdict.Holds => 1.0, ArgalaVerdict.Contested => 0.5, _ => 0.0 });
        return Index(score / judged.Count);
    }

    /// <summary>Rounded mean of the non-null values; null when there are none.</summary>
    public static int? Mean(IEnumerable<int?> values)
    {
        var read = values.Where(v => v is not null).Select(v => v!.Value).ToList();
        return read.Count == 0 ? null : (int)Math.Round(read.Average(), MidpointRounding.AwayFromZero);
    }

    private static int Index(double ratio) => (int)Math.Round(Math.Clamp(ratio, 0, 1) * 100, MidpointRounding.AwayFromZero);

    public static StrengthLean Lean(IReadOnlyList<StrengthBand> bands)
    {
        if (bands.All(b => b == StrengthBand.None)) return StrengthLean.None;
        return SortKey(bands) switch
        {
            >= 2 => StrengthLean.Strong,
            1 => StrengthLean.LeansStrong,
            0 => StrengthLean.Mixed,
            -1 => StrengthLean.LeansWeak,
            _ => StrengthLean.Weak
        };
    }

    /// <summary>Order key for "Strongest first": strong signals minus weak ones. A sort aid only —
    /// the page never presents it as a score.</summary>
    public static int SortKey(IEnumerable<StrengthBand> bands) =>
        bands.Count(b => b == StrengthBand.Strong) - bands.Count(b => b == StrengthBand.Weak);
}
