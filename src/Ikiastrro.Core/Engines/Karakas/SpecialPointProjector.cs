using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.DivisionalCharts;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>Turns D1 SpecialPointSeeds into per-chart PlanetPositions using the chart's own
/// IVargaSignRule + division factor — the exact transform VargaChartComputer applies to a
/// planet. For D1 pass a factor-1 identity rule (LinearVargaSignRule(1, 1)).</summary>
public static class SpecialPointProjector
{
    public static List<PlanetPosition> Project(
        IEnumerable<SpecialPointSeed> seeds, IVargaSignRule rule, int divisionFactor, ZodiacName lagnaSign)
    {
        var list = new List<PlanetPosition>();
        foreach (var s in seeds)
        {
            var sign = rule.SignFor(s.NirayanaLongitudeDegrees);
            var vargaLon = AstroMath.GetVargaLongitude(s.NirayanaLongitudeDegrees, divisionFactor);
            list.Add(new PlanetPosition
            {
                Planet = s.Code,
                PointKind = s.PointKind,
                Sign = sign.ToString(),
                NirayanaLongitudeDegrees = s.NirayanaLongitudeDegrees,
                VargaLongitudeDegrees = vargaLon,
                DegreesInSign = AstroMath.FormatDegreesMinutesSeconds(vargaLon % 30),
                HouseNumber = AstroMath.CountFromSignToSign(lagnaSign, sign)
            });
        }
        return list;
    }

    /// <summary>Arudha padas (bhava 'Arudha', graha 'GrahaArudha') are computed per chart, not projected.</summary>
    public static bool IsArudha(string pointKind) => pointKind is "Arudha" or "GrahaArudha";

    /// <summary>Points computed inside a varga (arudha padas), whose longitude is already in the
    /// varga's own zodiac. A pada computed in a varga has no real ecliptic longitude, so
    /// NirayanaLongitudeDegrees carries the same varga-space value.</summary>
    public static List<PlanetPosition> InChart(IEnumerable<SpecialPointSeed> chartSeeds, ZodiacName lagnaSign) =>
        chartSeeds.Select(s =>
        {
            var lon = AstroMath.Normalize(s.NirayanaLongitudeDegrees);
            var sign = (ZodiacName)(int)(lon / 30);
            return new PlanetPosition
            {
                Planet = s.Code,
                PointKind = s.PointKind,
                Sign = sign.ToString(),
                NirayanaLongitudeDegrees = lon,
                VargaLongitudeDegrees = lon,
                DegreesInSign = AstroMath.FormatDegreesMinutesSeconds(lon % 30),
                HouseNumber = AstroMath.CountFromSignToSign(lagnaSign, sign)
            };
        }).ToList();
}
