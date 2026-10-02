using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
using Ikiastrro.Web.Components.Workspace;

namespace Ikiastrro.Web.Components.Charts;

/// <summary>
/// Vimśopaka Bala for Astro Facts's Vargas step, computed live from the person's stored charts
/// (<see cref="VimsopakaCalculator"/>, weights from tbl_Rule_VimsopakaWeight). Each scheme is
/// computed on its own, so a person missing one varga loses only the schemes that need it.
/// </summary>
public static class VimsopakaRead
{
    public static readonly string[] SchemeOrder = ["SHADVARGA", "SAPTAVARGA", "DASAVARGA", "SHODASAVARGA"];

    /// <summary>Planet -> scheme -> score out of 20; a scheme is absent when a chart it needs is missing.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, decimal>> Compute(
        IReadOnlyDictionary<string, LoadedChart> charts, IReadOnlyList<VimsopakaWeight> weights)
    {
        var inputs = charts.Values.Select(c => c.ToAnalysisInput()).ToList();
        var byPlanet = new Dictionary<string, Dictionary<string, decimal>>(StringComparer.OrdinalIgnoreCase);
        foreach (var scheme in weights.GroupBy(w => w.SchemeCode, StringComparer.OrdinalIgnoreCase))
        {
            if (scheme.Any(w => !charts.ContainsKey(w.VargaChartType))) continue;
            foreach (var r in VimsopakaCalculator.Calculate(inputs, scheme.ToList()))
            {
                if (!byPlanet.TryGetValue(r.Planet, out var schemes)) byPlanet[r.Planet] = schemes = new(StringComparer.OrdinalIgnoreCase);
                schemes[r.Scheme] = r.Score;
            }
        }
        return byPlanet.ToDictionary(kv => kv.Key, kv => (IReadOnlyDictionary<string, decimal>)kv.Value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>One varga's share of a planet's score: the chart, its weight in the scheme, the
    /// planet's dignity factor there (out of 20) and the points that earns (weight × factor / 20).</summary>
    public sealed record VargaShare(string ChartType, decimal Weight, decimal Factor, decimal Points);

    /// <summary>The per-varga lines behind one planet's score in one scheme; empty when a chart is missing.</summary>
    public static IReadOnlyList<VargaShare> Breakdown(
        IReadOnlyDictionary<string, LoadedChart> charts, IReadOnlyList<VimsopakaWeight> weights, string scheme, string planet)
    {
        var rules = weights.Where(w => w.SchemeCode.Equals(scheme, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!Enum.TryParse<PlanetName>(planet, true, out var p) || rules.Any(w => !charts.ContainsKey(w.VargaChartType)))
            return [];
        return rules.Select(w =>
        {
            var factor = VimsopakaCalculator.PlacementValue(charts[w.VargaChartType].ToAnalysisInput(), p);
            return new VargaShare(w.VargaChartType, w.Weight, factor, decimal.Round(w.Weight * factor / 20m, 2));
        }).ToList();
    }

    /// <summary>The charts a scheme weighs and their points, in stored order — "D1 6 · D2 2 · …".</summary>
    public static string WeightsText(IReadOnlyList<VimsopakaWeight> weights, string scheme) =>
        string.Join(" · ", weights.Where(w => w.SchemeCode.Equals(scheme, StringComparison.OrdinalIgnoreCase))
            .Select(w => $"{w.VargaChartType} {w.Weight:0.##}"));
}
