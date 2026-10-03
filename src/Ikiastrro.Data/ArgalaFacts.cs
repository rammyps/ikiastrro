using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Models;
using Ikiastrro.Data.Statistics;

namespace Ikiastrro.Data;

/// <summary>One chart's house-target Argala / Virodhargala rows, shared by Astro Facts's
/// ArgalaTable and Key Inference so both pages read the same facts. Stored tbl_Fact_Argala rows
/// win (ChartGenerationService persists them for D1); a chart with none stored — every varga,
/// and any D1 not regenerated since migration 128 — gets the same ArgalaFactBuilder output live.</summary>
public static class ArgalaFacts
{
    public static IReadOnlyList<ArgalaFactRow> ForChart(
        IEnumerable<ArgalaFactRow> stored, string chartType, string ascendantSign, IEnumerable<ChartKeyDetail> grahas)
    {
        var rows = stored.Where(r => r.ChartType == chartType && r.TargetKind == "House").ToList();
        return rows.Count > 0 ? rows : Live(chartType, ascendantSign, grahas);
    }

    /// <summary>House-target rows computed from a chart's own graha positions.</summary>
    public static IReadOnlyList<ArgalaFactRow> Live(string chartType, string ascendantSign, IEnumerable<ChartKeyDetail> grahas) =>
        ArgalaFactBuilder.BuildForHouses(Enum.Parse<ZodiacName>(ascendantSign), ArgalaFactBuilder.BuildOccupancy(grahas))
            .Select(f => new ArgalaFactRow(chartType, f.TargetKind, f.TargetKey, (byte)f.TargetHouseNumber,
                f.RelationTypeCode, (byte)f.HouseOffset, f.IsPrimary, f.OccupantPlanet.ToString(),
                f.ExceptionApplied, f.CountedAntiZodiacally, null))
            .ToList();

    /// <summary>"What acts on each house" for a chart (Core <see cref="Core.Engines.Houses.HouseVerdicts"/>),
    /// with each house's Argala read from <paramref name="argalaFacts"/> through
    /// <see cref="LifeMatterStatistics.BuildArgala"/> — the same verdicts Key Inference uses, with no
    /// kāraka since no life matter is chosen. Pairs that hold or are contested count as acting
    /// Argala; every obstructing planet is Virodhargala.</summary>
    public static IReadOnlyList<HouseVerdictRow> HouseVerdicts(
        string ascendantSign, IReadOnlyList<ChartKeyDetail> grahas, IReadOnlyList<ArgalaFactRow> argalaFacts) =>
        Core.Engines.Houses.HouseVerdicts.For(ascendantSign, grahas, house =>
        {
            var argala = LifeMatterStatistics.BuildArgala(argalaFacts.Where(f => f.TargetHouseNumber == house).ToList());
            return new HouseArgalaPlanets(
                Planets(argala.Pairs.Where(p => p.Verdict is ArgalaVerdict.Holds or ArgalaVerdict.Contested).SelectMany(p => p.ArgalaPlanets)),
                Planets(argala.Pairs.SelectMany(p => p.ObstructingPlanets)));
        });

    private static IReadOnlyList<PlanetName> Planets(IEnumerable<string> names) =>
        names.Select(n => Enum.TryParse<PlanetName>(n, true, out var p) ? p : (PlanetName?)null)
            .Where(p => p is not null).Select(p => p!.Value).Distinct().ToList();
}
