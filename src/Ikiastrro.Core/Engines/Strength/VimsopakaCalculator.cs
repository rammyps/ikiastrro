using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>One active row from tbl_Rule_VimsopakaWeight.</summary>
public sealed record VimsopakaWeight(string SchemeCode, string VargaChartType, decimal Weight);

/// <summary>
/// Parāśara's 20-point weighted cross-varga strength. The caller supplies the versioned
/// chart weights; this pure calculator supplies the BPHS dignity factor for each placement.
/// Classical Vimśopaka covers the seven visible grahas, not Rāhu/Ketu.
/// </summary>
public static class VimsopakaCalculator
{
    private static readonly PlanetName[] ClassicalPlanets =
        [PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury,
         PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn];

    public static IReadOnlyList<VimsopakaResult> Calculate(
        IReadOnlyList<ChartAnalysisInput> charts,
        IReadOnlyList<VimsopakaWeight> weights)
    {
        var results = new List<VimsopakaResult>();
        foreach (var scheme in weights.GroupBy(w => w.SchemeCode, StringComparer.OrdinalIgnoreCase))
        {
            var max = scheme.Sum(w => w.Weight);
            if (max != 20m)
                throw new ArgumentException($"Vimsopaka scheme {scheme.Key} totals {max}, expected 20.", nameof(weights));

            foreach (var planet in ClassicalPlanets)
            {
                decimal score = 0;
                foreach (var rule in scheme)
                {
                    var chart = charts.FirstOrDefault(c => c.ChartType.Equals(rule.VargaChartType, StringComparison.OrdinalIgnoreCase));
                    if (chart is null)
                        throw new ArgumentException($"Vimsopaka scheme {scheme.Key} requires missing chart {rule.VargaChartType}.", nameof(charts));

                    score += rule.Weight * PlacementValue(chart, planet) / 20m;
                }

                results.Add(new VimsopakaResult(planet.ToString(), scheme.Key, decimal.Round(score, 4), max));
            }
        }
        return results;
    }

    /// <summary>The planet's dignity factor in one varga, out of 20 (own/mūlatrikoṇa 20, great
    /// friend 18, friend 15, neutral 10, enemy 7, great enemy 5). A scheme's score is
    /// Σ weight × factor / 20.</summary>
    public static decimal PlacementValue(ChartAnalysisInput chart, PlanetName planet)
    {
        var position = chart.Planets.FirstOrDefault(p => p.Planet.Equals(planet.ToString(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Chart {chart.ChartType} has no {planet} placement.", nameof(chart));
        if (!Enum.TryParse<ZodiacName>(position.Sign, true, out var sign))
            throw new ArgumentException($"Chart {chart.ChartType} has invalid sign '{position.Sign}' for {planet}.", nameof(chart));

        var signs = chart.Planets
            .Where(p => Enum.TryParse<PlanetName>(p.Planet, true, out _) && Enum.TryParse<ZodiacName>(p.Sign, true, out _))
            .ToDictionary(p => p.Planet, p => Enum.Parse<ZodiacName>(p.Sign, true), StringComparer.OrdinalIgnoreCase);
        var degree = ((position.VargaLongitudeDegrees ?? position.NirayanaLongitudeDegrees ?? 15d) % 30d + 30d) % 30d;
        var dignity = PvrDignityEvaluator.Evaluate(planet, sign, degree, signs);

        if (dignity.DignityTypeCode is "OWN" or "MOOLATRIKONA") return 20m;
        return dignity.CompoundRelationshipCode switch
        {
            "ADHIMITRA" => 18m,
            "MITRA" => 15m,
            "SAMA" => 10m,
            "SHATRU" => 7m,
            "ADHISHATRU" => 5m,
            _ => 10m
        };
    }
}
