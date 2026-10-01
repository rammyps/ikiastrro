using Ikiastrro.Core.Pipeline;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Engines.Karakas;

namespace Ikiastrro.Core.Engines.DivisionalCharts;

/// <summary>
/// Computes ANY divisional chart from an injected division factor + IVargaSignRule.
/// Replaces the per-varga D2/D6/D9/D10/D11 ChartComputers: same shape (real
/// longitude + varga longitude + Whole-Sign house from the varga Lagna), only the
/// sign rule and the factor vary. Shared with ChartAnalyzer via ChartAnalysisInput.
/// The ChartType field on the returned input is left blank - VargaCalculator sets it.
/// </summary>
public static class VargaChartComputer
{
    public static ChartAnalysisInput Compute(
        BirthDetails birthDetails, int divisionFactor, IVargaSignRule rule,
        IReadOnlyList<SpecialPointSeed>? seeds = null,
        AyanamsaDefinition? ayanamsa = null)
    {
        var localMoment = BirthMomentFactory.Create(birthDetails);
        var positions = SwissEphemerisProvider.GetSiderealPositions(localMoment, birthDetails.Latitude, birthDetails.Longitude, ayanamsa);

        var lagnaSign = rule.SignFor(positions.AscendantLongitude);
        var lagnaVargaLon = AstroMath.GetVargaLongitude(positions.AscendantLongitude, divisionFactor);

        var planetPositions = new List<PlanetPosition>
        {
            new PlanetPosition
            {
                Planet = "Ascendant",
                Sign = lagnaSign.ToString(),
                NirayanaLongitudeDegrees = positions.AscendantLongitude,
                VargaLongitudeDegrees = lagnaVargaLon,
                DegreesInSign = AstroMath.FormatDegreesMinutesSeconds(lagnaVargaLon % 30),
                HouseNumber = 1
            }
        };

        foreach (var planet in PlanetNames.All9)
        {
            var realLon = positions.PlanetLongitudes[planet];
            var vargaSign = rule.SignFor(realLon);
            var vargaLon = AstroMath.GetVargaLongitude(realLon, divisionFactor);
            planetPositions.Add(new PlanetPosition
            {
                Planet = planet.ToString(),
                Sign = vargaSign.ToString(),
                NirayanaLongitudeDegrees = realLon,
                EclipticLatitudeDegrees = positions.PlanetLatitudes[planet],
                SpeedLongitudeDegPerDay = positions.PlanetSpeeds[planet],
                VargaLongitudeDegrees = vargaLon,
                DegreesInSign = AstroMath.FormatDegreesMinutesSeconds(vargaLon % 30),
                HouseNumber = AstroMath.CountFromSignToSign(lagnaSign, vargaSign),
                IsRetrograde = positions.PlanetSpeeds[planet] < 0
            });
        }

        // Arudha padas (bhava and graha) are computed inside this chart from its own placements
        // (PVR sec.9.2 / 9.5 — "in the divisional chart of interest"), not projected from D1. They
        // are produced only when the D1 seeds carry them, so callers that pass no seeds get none.
        // Every other special point (special lagnas, upagrahas, Punya Saham) is a real longitude
        // and is projected as before.
        seeds ??= Array.Empty<SpecialPointSeed>();
        var specialPoints = SpecialPointProjector.Project(
            seeds.Where(s => !SpecialPointProjector.IsArudha(s.PointKind)), rule, divisionFactor, lagnaSign);
        var chart = new ChartAnalysisInput(ChartType: "", lagnaSign, planetPositions);
        if (seeds.Any(s => s.PointKind == "Arudha"))
            specialPoints.AddRange(SpecialPointProjector.InChart(ArudhaCalculator.Compute(chart), lagnaSign));
        if (seeds.Any(s => s.PointKind == "GrahaArudha"))
            specialPoints.AddRange(SpecialPointProjector.InChart(GrahaArudhaCalculator.Compute(chart), lagnaSign));
        return chart with { SpecialPoints = specialPoints };
    }
}
