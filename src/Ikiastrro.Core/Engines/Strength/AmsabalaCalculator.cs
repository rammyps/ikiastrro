using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>One scheme's amsabala result for one planet - PVR Sec. 6.6.</summary>
public sealed record AmsabalaResult(
    string SchemeCode,
    int GroupSize,
    int GoodCount,
    string? AmsaName,
    string Detail);

/// <summary>
/// PVR Integrated Approach Sec. 6.6 "Varga Grouping and Amsabala": across each of 4 named
/// varga groups (Shadvarga/Saptavarga/Dasavarga/Shodasavarga), count the divisional charts
/// in which a planet occupies its own rasi, moolatrikona, or exaltation rasi, then name the
/// resulting "amsa" from that count (Kimsukamsa, Vyanjanamsa, ... Sree Vallabhamsa) via
/// tbl_Rule_AmsabalaName (migration 099).
///
/// Distinct from Saptavargaja Bala (a Shadbala Sthana Bala sub-component -- dignity POINTS
/// summed across the saptavarga, see PvrDignityEvaluator.SaptavargajaPoints) and from
/// Vimsopaka Bala (a weighted 0-20 total PVR names but never gives numeric weights for --
/// tbl_Rule_VimsopakaWeight stays empty/reserved). This is the varga-grouping layer both of
/// those build on, not either of them.
///
/// A planet with fewer than 2 good placements in a scheme has no named amsa -- PVR's own
/// tables start at count 2 -- so <see cref="AmsabalaResult.AmsaName"/> is null in that case,
/// not a data gap.
/// </summary>
public static class AmsabalaCalculator
{
    public static IReadOnlyList<AmsabalaResult> Calculate(
        PlanetName planet,
        IReadOnlyList<ChartAnalysisInput> charts,
        IReadOnlyList<AmsabalaGroupMember> groups,
        IReadOnlyList<AmsabalaNameEntry> names)
    {
        var planetName = planet.ToString();
        var results = new List<AmsabalaResult>();

        foreach (var scheme in groups.Select(g => g.SchemeCode).Distinct())
        {
            var chartTypes = groups.Where(g => g.SchemeCode == scheme).Select(g => g.ChartType).ToList();
            var goodCount = 0;
            var detail = new List<string>(chartTypes.Count);

            foreach (var chartType in chartTypes)
            {
                var chart = charts.FirstOrDefault(c => c.ChartType.Equals(chartType, StringComparison.OrdinalIgnoreCase));
                var position = chart?.Planets.FirstOrDefault(p => p.Planet.Equals(planetName, StringComparison.OrdinalIgnoreCase));
                if (position is null || string.IsNullOrEmpty(position.Sign) || !Enum.TryParse<ZodiacName>(position.Sign, true, out var sign))
                {
                    detail.Add($"{chartType}:?");
                    continue;
                }

                var rawDegree = position.VargaLongitudeDegrees ?? position.NirayanaLongitudeDegrees ?? 15.0;
                var degreeInSign = ((rawDegree % 30) + 30) % 30;
                var dignity = PvrDignityEvaluator.Evaluate(planet, sign, degreeInSign);
                var isGood = dignity.DignityTypeCode is "OWN" or "MOOLATRIKONA" or "EXALTED";
                detail.Add($"{chartType}:{position.Sign}{(isGood ? "*" : string.Empty)}");
                if (isGood) goodCount++;
            }

            var amsaName = names.FirstOrDefault(n => n.SchemeCode == scheme && n.GoodCount == goodCount)?.AmsaName;
            results.Add(new AmsabalaResult(scheme, chartTypes.Count, goodCount, amsaName, string.Join(", ", detail)));
        }

        return results;
    }
}
