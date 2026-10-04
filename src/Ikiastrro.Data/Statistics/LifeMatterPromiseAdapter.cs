using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Core.LifeMatters.Promise;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Data.Statistics;

/// <summary>One D1 target (a Lagna house of the matter) and its promise.</summary>
public sealed record MatterTargetPromise(string Label, int House, ZodiacName Sign, LifeMatterPromise Promise);

/// <summary>A matter's D1 promise, one per Lagna house the matter is read from. A matter with several
/// houses keeps them apart; no overall verdict is derived across them (that is a later decision).</summary>
/// <param name="Note">Why there is no target, when there is none.</param>
public sealed record MatterPromiseReading(
    string LifeMatterCode, string MatterText, string CategoryCode, string CategoryName,
    IReadOnlyList<MatterTargetPromise> Targets, string? Note)
{
    /// <summary>The matter's first house by focus priority.</summary>
    public MatterTargetPromise? Primary => Targets.FirstOrDefault();
}

/// <summary>
/// Turns one life matter and a person's persisted D1 facts into <see cref="MatterPromiseInput"/>s for
/// <see cref="D1PromiseEngine"/>: the target signs from the matter's Lagna house foci, its karakas,
/// each planet's sign, dignity, combustion, nakshatra lord and Ṣaḍbala band, and the target's house
/// capacity, SAV, lord's BAV and Argala — all from the same <see cref="LifeMatterStatistics"/> and
/// focus rules Key Inference already reads, so the two never disagree on a figure. It adds no rule.
/// Not yet read: yogas (the evaluation rows carry no planets or direction) and special-point foci
/// (Āruḍhas and the other lenses, phase 4); varga reconciliation is phase 3. Pure given its inputs.
/// </summary>
public static class LifeMatterPromiseAdapter
{
    private const string Lagna = "LAGNA";

    public static MatterPromiseReading Read(
        LifeMatterStepRow step,
        ResolvedLifeMatterFocus focus,
        IReadOnlyList<ChartKeyDetail> d1KeyDetails,
        string ascendantSign,
        LifeMatterStatistics d1Stats,
        IReadOnlyList<ShadbalaSummaryRow> shadbala)
    {
        var grahas = d1KeyDetails.Where(k => k.PointKind == "Graha").ToList();
        var planets = PlanetFacts(grahas, shadbala);
        var karakas = KarakaPlanets(grahas, focus);

        var houses = focus.HouseAndSpecialPointFoci
            .Where(f => f is { FocusKind: LifeMatterFocusKind.House, HouseNumber: not null } && (f.ReferenceCode ?? Lagna) == Lagna)
            .OrderBy(f => f.Priority).ThenBy(f => f.HouseNumber)
            .Select(f => f.HouseNumber!.Value)
            .Distinct()
            .ToList();
        if (houses.Count == 0)
            return new MatterPromiseReading(step.LifeMatterCode, step.MatterText, step.CategoryCode, step.CategoryName, [],
                "No Lagna house focus: this matter is read from a special point or another lagna.");

        var targets = new List<MatterTargetPromise>();
        foreach (var house in houses)
        {
            var sign = LifeMatterFocusResolver.ResolveHouseSign(Enum.Parse<ZodiacName>(ascendantSign), house);
            var stats = d1Stats.ForSign(sign.ToString(), karakas.Select(k => k.ToString()));
            var label = $"{Ordinal(house)} house";
            var input = new MatterPromiseInput(
                step.LifeMatterCode, Enum.Parse<ZodiacName>(ascendantSign), label, sign, planets,
                karakas, ToCapacity(stats.BhavaBand), ToCapacity(stats.SavBand), ToCapacity(stats.LordBavBand),
                ArgalaPlanets(stats.Argala, held: true), ArgalaPlanets(stats.Argala, held: false),
                Yogas: null, TechnicalSupportIndex: stats.StrengthPercent);
            targets.Add(new MatterTargetPromise(label, house, sign, D1PromiseEngine.Read(input)));
        }
        return new MatterPromiseReading(step.LifeMatterCode, step.MatterText, step.CategoryCode, step.CategoryName, targets, null);
    }

    /// <summary>Every graha in D1 as the engine needs it. Ṣaḍbala is banded with
    /// <see cref="StrengthBands.ShadbalaPercentOfMinimum"/>; Rāhu and Ketu have none (Unknown).</summary>
    public static Dictionary<PlanetName, PlanetFact> PlanetFacts(
        IEnumerable<ChartKeyDetail> grahas, IReadOnlyList<ShadbalaSummaryRow> shadbala)
    {
        var percent = shadbala.ToDictionary(r => r.Planet, r => r.PercentOfMinimum, StringComparer.OrdinalIgnoreCase);
        var facts = new Dictionary<PlanetName, PlanetFact>();
        foreach (var g in grahas)
        {
            if (!Enum.TryParse<PlanetName>(g.Planet, out var planet) || !Enum.TryParse<ZodiacName>(g.Sign, out var sign)) continue;
            var capacity = StrengthBands.ShadbalaPercentOfMinimum.Classify(percent.GetValueOrDefault(g.Planet)).ToCapacity();
            PlanetName? star = Enum.TryParse<PlanetName>(g.NakshatraLordPlanet, out var lord) ? lord : null;
            facts[planet] = new PlanetFact(sign, g.DignityStatus, g.IsCombust == true, capacity, star);
        }
        return facts;
    }

    /// <summary>The matter's karaka planets in this chart: a fixed graha (GRAHA_x) or whoever holds
    /// a Jaimini Chara Karaka role (KARAKA_AK ...).</summary>
    public static List<PlanetName> KarakaPlanets(IEnumerable<ChartKeyDetail> grahas, ResolvedLifeMatterFocus focus)
    {
        var list = grahas.ToList();
        string? Resolve(string code) =>
            code.StartsWith("GRAHA_", StringComparison.Ordinal)
                ? list.FirstOrDefault(g => string.Equals(g.Planet, code["GRAHA_".Length..], StringComparison.OrdinalIgnoreCase))?.Planet
            : code.StartsWith("KARAKA_", StringComparison.Ordinal)
                ? list.FirstOrDefault(g => string.Equals(g.CharaKaraka, code["KARAKA_".Length..], StringComparison.OrdinalIgnoreCase))?.Planet
            : null;
        return focus.Karakas.Select(k => Resolve(k.KarakaCode))
            .Select(n => Enum.TryParse<PlanetName>(n, out var p) ? p : (PlanetName?)null)
            .Where(p => p is not null).Select(p => p!.Value).Distinct().ToList();
    }

    /// <summary>Planets whose Argala holds or is contested (<paramref name="held"/>), or every planet
    /// obstructing an Argala, as Key Inference's influence step reads them.</summary>
    public static List<PlanetName> ArgalaPlanets(ArgalaSummary argala, bool held) =>
        (held
            ? argala.Pairs.Where(p => p.Verdict is ArgalaVerdict.Holds or ArgalaVerdict.Contested).SelectMany(p => p.ArgalaPlanets)
            : argala.Pairs.SelectMany(p => p.ObstructingPlanets))
        .Select(n => Enum.TryParse<PlanetName>(n, out var planet) ? planet : (PlanetName?)null)
        .Where(p => p is not null).Select(p => p!.Value).Distinct().ToList();

    public static Capacity ToCapacity(StrengthBand band) => band switch
    {
        StrengthBand.Strong => Capacity.Strong,
        StrengthBand.Middle => Capacity.Moderate,
        StrengthBand.Weak => Capacity.Weak,
        _ => Capacity.Unknown,
    };

    private static string Ordinal(int n) =>
        n + (n % 100 is 11 or 12 or 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" });
}

/// <summary>Reads every life matter's D1 promise for one saved person, from the persisted chart and
/// strength facts. The loader mirrors <c>MarriageMattersReader</c>; Core and the adapter hold the logic.</summary>
public sealed class LifeMatterPromiseService(
    ChartResultsRepository chartResults,
    ChartKeyDetailsRepository keyDetails,
    LifeMatterReferenceRepository references,
    LifeMatterFocusRepository foci,
    KarakaMatterRepository karakaMatters,
    PlanetaryStrengthRepository strength,
    AshtakavargaRepository ashtakavarga,
    BhavaStrengthRepository bhava,
    AmsabalaRepository amsabala,
    ArgalaFactRepository argala)
{
    private const byte RuleSetId = 1;
    private static readonly LifeMatterFocusResolver Resolver = new();

    /// <summary>All active matters in display order, or null when the person has no stored D1.</summary>
    public IReadOnlyList<MatterPromiseReading>? ReadAll(int birthDetailId)
    {
        var d1 = chartResults.GetByBirthDetailId(birthDetailId).FirstOrDefault(c => c.ChartType == "D1");
        if (d1 is null) return null;
        var details = keyDetails.GetByChartResultId(d1.Id);
        var asc = details.FirstOrDefault(k => k.Planet == "Ascendant")?.Sign;
        if (asc is null) return null;

        var steps = references.GetAllSteps(RuleSetId);
        var subjects = foci.GetSubjects(RuleSetId);
        var focusRules = foci.GetFoci(RuleSetId);
        var karakas = karakaMatters.GetForRuleSet(RuleSetId);
        var shadbala = strength.GetSummaryByBirthDetailId(birthDetailId);

        var stats = new LifeMatterStatistics("D1", asc, ashtakavarga.GetByBirthDetailId(birthDetailId),
            bhava.GetSummaryByBirthDetailId(birthDetailId), shadbala,
            ArgalaFacts.ForChart(argala.GetByBirthDetailId(birthDetailId), "D1", asc, details),
            bhava.GetComponentsByBirthDetailId(birthDetailId), amsabala.GetByBirthDetailId(birthDetailId));

        return steps
            .Select(s => LifeMatterPromiseAdapter.Read(
                s, Resolver.Resolve(RuleSetId, s.LifeMatterId, subjects, karakas, focusRules), details, asc, stats, shadbala))
            .ToList();
    }
}
