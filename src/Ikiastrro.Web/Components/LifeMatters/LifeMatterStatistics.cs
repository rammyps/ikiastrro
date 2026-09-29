using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Data;

namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>Three-way reading of one strength statistic, plus None when there is nothing to read.
/// Strong/Weak are the app's existing bands, not outcome judgments.</summary>
public enum StrengthBand { Strong, Middle, Weak, None }

public enum ArgalaVerdict { Holds, Contested, Obstructed }

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

/// <summary>D1 strength statistics for one sign, read as a house. Every figure is sign-based, so a
/// house counted from any lagna reads the same persisted D1 facts.</summary>
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
}

/// <summary>
/// D1 statistics for the Life Matters page, built once per person from persisted facts
/// (vw_ChartAshtakavarga, vw_ChartBhavaBala, vw_ChartShadbala, tbl_Fact_Argala) and queried per
/// sign. Bands reuse the thresholds the app already shows elsewhere: SAV above 30 favourable and
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
    {
        _ascendant = Enum.Parse<ZodiacName>(d1AscendantSign);
        _savBySignNumber = ashtakavarga
            .Where(r => r.ChartType == "D1" && r.SarvaBindus is not null)
            .GroupBy(r => (int)r.SignNumber)
            .ToDictionary(g => g.Key, g => (int)g.First().SarvaBindus!.Value);
        _bhavaByHouse = bhavaBala.ToDictionary(r => (int)r.HouseNumber, r => r.BhavaBalaRupas);
        _shadbalaPercentByPlanet = shadbala.ToDictionary(r => r.Planet, r => r.PercentOfMinimum, StringComparer.OrdinalIgnoreCase);
        _argalaByHouse = argala
            .Where(r => r.ChartType == "D1" && r.TargetKind == "House")
            .ToLookup(r => (int)r.TargetHouseNumber);
    }

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

    /// <summary>Order key for "Strongest first": strong signals minus weak ones. A sort aid only —
    /// the page never presents it as a score.</summary>
    public static int SortKey(IEnumerable<StrengthBand> bands) =>
        bands.Count(b => b == StrengthBand.Strong) - bands.Count(b => b == StrengthBand.Weak);
}
