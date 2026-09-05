using Ikiastrro.Core.Models;

namespace Ikiastrro.Web.Components.Workspace;

/// <summary>Groups the 21 chart types for the Web Workspace's varga rail — by life-area
/// (tbl_Dim_ChartType.Category), DB-sourced (rammyps's call, 2026-09-05). Replaces this class's
/// old hardcoded classical Shadvarga(6)/Saptavarga(7)/Dasavarga(10)/Shodasavarga(16)/Extra-vargas
/// bundles, which lived here as a static field: every varga now belongs to exactly one of the 6
/// life-area categories migration 25 seeded (Self &amp; Personality, Wealth &amp; Resources,
/// Health &amp; Vitality, Relationships &amp; Family, Career &amp; Status, Spirituality &amp;
/// Struggle), and a chart type's Category can change with a future migration without a code
/// deploy — the rail just reads whatever's in the DB.</summary>
public static class VargaBundles
{
    /// <summary>Category groups in display order (each category ordered first by its lowest
    /// member's DisplayOrder, so "Self &amp; Personality" — which contains D1 — leads), each
    /// group's own codes ordered by DisplayOrder. D1 is included (VargaRail gives it its own
    /// "hero" tile/caption instead of skipping it).</summary>
    public static IReadOnlyList<(string Title, IReadOnlyList<string> Codes)> Groups(IReadOnlyList<ChartTypeRow> chartTypes) =>
        chartTypes
            .GroupBy(t => t.Category)
            .OrderBy(g => g.Min(t => t.DisplayOrder))
            .Select(g => (g.Key, (IReadOnlyList<string>)g.OrderBy(t => t.DisplayOrder).Select(t => t.Code).ToList()))
            .ToList();

    /// <summary>Rail order = the sequence a prev/next in VargaView walks — groups top-to-bottom
    /// (per Groups' own ordering), codes in each group's listed order.</summary>
    public static IReadOnlyList<string> RailOrder(IReadOnlyList<ChartTypeRow> chartTypes) =>
        Groups(chartTypes).SelectMany(g => g.Codes).ToList();
}
