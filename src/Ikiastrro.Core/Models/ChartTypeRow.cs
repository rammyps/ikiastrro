namespace Ikiastrro.Core.Models;

/// <summary>One row of dbo.tbl_Dim_ChartType — the controlled vocabulary for ChartResults.ChartTypeId.</summary>
public record ChartTypeRow(
    int Id, string Code, string DisplayName, int? DivisionalFactor, string Category, int DisplayOrder,
    /// <summary>Short reader-facing phrase for what the chart signifies (e.g. D1's "Personality,
    /// Expression, Logic"), shown in SouthIndianTemplate's dynamic heading. Null for every chart
    /// type that hasn't been given one yet (added in migration 22; only D1 is seeded so far).</summary>
    string? Description = null,
    /// <summary>Short "Primary Domain" tag (e.g. D1's "Self / Life", D9's "Marriage / Dharma"),
    /// distinct from <see cref="Description"/>. Null for chart types with no accepted domain tag
    /// yet (added in migration 23; D2-US, D5, D6, D8, D11 are still NULL).</summary>
    string? ChartShortDescription = null);
