using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Models;
using Ikiastrro.Data;

namespace Ikiastrro.Web.Components.Charts;

/// <summary>One chart's house-target Argala / Virodhargala rows, shared by Key Inference's
/// ArgalaTable and Life Matters so both pages read the same facts. Stored tbl_Fact_Argala rows
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
}
