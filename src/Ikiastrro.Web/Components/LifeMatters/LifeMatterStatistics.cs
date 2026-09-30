using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;
using Ikiastrro.Data;

namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>Three-way reading of one strength statistic, plus None when there is nothing to read.
/// Strong/Weak are the app's existing bands, not outcome judgments.</summary>
public enum StrengthBand { Strong, Middle, Weak, None }

public enum ArgalaVerdict { Holds, Contested, Obstructed }

/// <summary>One overall reading of the four signals (strong count minus weak count): ±2 or more is
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
    public int Obstructed => Pairs.Count(p => p.Verdict == ArgalaVerdict.Obstructed);
    public int Net => Holds - Obstructed;
    public bool Any => Pairs.Any(p => p.ArgalaPlanets.Count > 0 || p.ObstructingPlanets.Count > 0);
}

/// <summary>Strength statistics for one sign of one chart, read as a house from that chart's Lagna.
/// Every figure is sign-based, so a house counted from any lagna reads the same facts.</summary>
public sealed record HouseStatistics(
    string Sign, int HouseFromLagna, int? SavBindus, decimal? BhavaBalaRupas,
    string LordPlanet, decimal? LordShadbalaPercent, ArgalaSummary Argala)
{
    public StrengthBand SavBand => LifeMatterStatistics.SavBand(SavBindus);
    public StrengthBand BhavaBand => LifeMatterStatistics.BhavaBand(BhavaBalaRupas);
    public StrengthBand LordShadbalaBand => LifeMatterStatistics.ShadbalaBand(LordShadbalaPercent);
    public StrengthBand ArgalaBand => !Argala.Any ? StrengthBand.None
        : Argala.Net > 0 ? StrengthBand.Strong : Argala.Net < 0 ? StrengthBand.Weak : StrengthBand.Middle;

    /// <summary>The four signals in fixed display order: SAV · Bhava Bala · lord Ṣaḍbala · Argala.</summary>
    public IReadOnlyList<StrengthBand> Bands => [SavBand, BhavaBand, LordShadbalaBand, ArgalaBand];

    /// <summary>The four signals as 0–100 indices, same order as <see cref="Bands"/>; null where
    /// nothing can be read. See <see cref="LifeMatterStatistics.SavIndex"/> and siblings.</summary>
    public IReadOnlyList<int?> Percents =>
    [
        LifeMatterStatistics.SavIndex(SavBindus), LifeMatterStatistics.BhavaIndex(BhavaBalaRupas),
        LifeMatterStatistics.ShadbalaIndex(LordShadbalaPercent), LifeMatterStatistics.ArgalaIndex(Argala)
    ];

    /// <summary>The mean of the readable signal indices — the page's one strength figure. Null
    /// when no signal can be read.</summary>
    public int? StrengthPercent => LifeMatterStatistics.Mean(Percents);
}

/// <summary>
/// Statistics for the Life Matters page, built once per person per chart (D1 or any varga) from
/// persisted facts (vw_ChartAshtakavarga, vw_ChartBhavaBala, vw_ChartShadbala, tbl_Fact_Argala) and
/// queried per sign. In a varga: SAV is that varga's own Sarva Ashtakavarga (still 337 bindus in
/// total, so the same bands apply); Argala is the varga's own occupancy; the lord is the varga
/// sign's lord, and its Ṣaḍbala is the planet's (Ṣaḍbala exists only once per planet); Bhava Bala
/// is a D1 house computation and reads as nothing in a varga. Bands reuse the thresholds the app already shows elsewhere: SAV above 30 favourable and
/// below 25 unfavourable (AshtakavargaChart's cited rule), Bhava Bala 7+/under 5 rupas
/// (HouseStrengthChart) and Ṣaḍbala 110%+/under 90% of the required minimum (PlanetaryStateTable's
/// strong/weak bands). They describe strength, not outcomes.
/// </summary>
public sealed class LifeMatterStatistics
{
    // Primary pairs (PVR ch.9): 2nd/12th, 4th/10th, 11th/3rd, then secondary 5th/9th. A 3rd-house
    // argala (malefics) is obstructed from the 11th and is added only when present.
    private static readonly (int Argala, int Obstruction, string Kind)[] Pairs =
        [(2, 12, "Primary"), (4, 10, "Primary"), (11, 3, "Primary"), (5, 9, "Secondary")];

    public string ChartType { get; }
    private readonly ZodiacName _ascendant;
    private readonly IReadOnlyDictionary<int, int> _savBySignNumber;
    private readonly IReadOnlyDictionary<int, decimal> _bhavaByHouse;
    private readonly IReadOnlyDictionary<string, decimal?> _shadbalaPercentByPlanet;
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
    /// <see cref="LiveArgala"/>.</param>
    public LifeMatterStatistics(
        string chartType,
        string ascendantSign,
        IEnumerable<AshtakavargaRow> ashtakavarga,
        IEnumerable<BhavaBalaSummaryRow> bhavaBala,
        IEnumerable<ShadbalaSummaryRow> shadbala,
        IEnumerable<ArgalaFactRow> argala)
    {
        ChartType = chartType;
        _ascendant = Enum.Parse<ZodiacName>(ascendantSign);
        _savBySignNumber = ashtakavarga
            .Where(r => r.ChartType == chartType && r.SarvaBindus is not null)
            .GroupBy(r => (int)r.SignNumber)
            .ToDictionary(g => g.Key, g => (int)g.First().SarvaBindus!.Value);
        _bhavaByHouse = chartType == "D1"
            ? bhavaBala.ToDictionary(r => (int)r.HouseNumber, r => r.BhavaBalaRupas)
            : new Dictionary<int, decimal>();
        _shadbalaPercentByPlanet = shadbala.ToDictionary(r => r.Planet, r => r.PercentOfMinimum, StringComparer.OrdinalIgnoreCase);
        _argalaByHouse = argala
            .Where(r => r.ChartType == chartType && r.TargetKind == "House")
            .ToLookup(r => (int)r.TargetHouseNumber);
    }

    /// <summary>House-target Argala rows computed from a chart's own graha positions — the same
    /// ArgalaFactBuilder chart generation persists for D1, run live for a varga that has no stored rows.</summary>
    public static IReadOnlyList<ArgalaFactRow> LiveArgala(string chartType, string ascendantSign, IEnumerable<ChartKeyDetail> grahas) =>
        ArgalaFactBuilder.BuildForHouses(Enum.Parse<ZodiacName>(ascendantSign), ArgalaFactBuilder.BuildOccupancy(grahas))
            .Select(f => new ArgalaFactRow(chartType, f.TargetKind, f.TargetKey, (byte)f.TargetHouseNumber,
                f.RelationTypeCode, (byte)f.HouseOffset, f.IsPrimary, f.OccupantPlanet.ToString(),
                f.ExceptionApplied, f.CountedAntiZodiacally, null))
            .ToList();

    public HouseStatistics ForSign(string sign)
    {
        var zodiac = Enum.Parse<ZodiacName>(sign);
        var house = AstroMath.CountFromSignToSign(_ascendant, zodiac);
        var lord = HouseEngine.GetSignLord(zodiac);
        return new HouseStatistics(
            sign, house,
            _savBySignNumber.TryGetValue((int)zodiac + 1, out var sav) ? sav : null,
            _bhavaByHouse.TryGetValue(house, out var bhava) ? bhava : null,
            lord,
            _shadbalaPercentByPlanet.GetValueOrDefault(lord),
            BuildArgala(_argalaByHouse[house].ToList()));
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

    public static StrengthBand SavBand(int? bindus) => bindus switch
    {
        null => StrengthBand.None,
        > 30 => StrengthBand.Strong,
        < 25 => StrengthBand.Weak,
        _ => StrengthBand.Middle
    };

    public static StrengthBand BhavaBand(decimal? rupas) => rupas switch
    {
        null => StrengthBand.None,
        >= 7 => StrengthBand.Strong,
        >= 5 => StrengthBand.Middle,
        _ => StrengthBand.Weak
    };

    public static StrengthBand ShadbalaBand(decimal? percentOfMinimum) => percentOfMinimum switch
    {
        null => StrengthBand.None,
        >= 110 => StrengthBand.Strong,
        < 90 => StrengthBand.Weak,
        _ => StrengthBand.Middle
    };

    // 0–100 strength indices. Each scale puts the ordinary middle at about 50, so the four can be
    // averaged: SAV out of the 56 bindus a sign can hold (28, the average, is 50%); Bhava Bala out
    // of 12 rupas (the page's meter scale); Ṣaḍbala % of minimum out of 200 (the minimum is 50%);
    // Argala as the share of pairs that hold (contested counts half, obstruction-only is 50%).
    // Presentation scales, not sourced rules — strength, never an outcome.
    public static int? SavIndex(int? bindus) => bindus is { } b ? Index(b / 56.0) : null;

    public static int? BhavaIndex(decimal? rupas) => rupas is { } r ? Index((double)r / 12) : null;

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
