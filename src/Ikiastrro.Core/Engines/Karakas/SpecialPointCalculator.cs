using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Position;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>Computes every special point's D1 longitude for a person: AL + the 12 Bhava
/// Arudhas (A2–A12), all 4 special lagnas (Bhaava/Hora/Ghati/Sree — tbl_Dim_SpecialLagnas,
/// db/28, all built as of 2026-09-13), and all eleven upagrahas when rules are supplied.
/// The compatibility API without rules returns only Gulika/Maandi. Points are projected into every varga
/// by SpecialPointProjector.</summary>
public static class SpecialPointCalculator
{
    public static IReadOnlyList<SpecialPointSeed> ComputeSeeds(
        BirthDetails birthDetails, AyanamsaDefinition? ayanamsa = null, SubPlanetRuleSet? subPlanetRules = null)
    {
        var d1 = D1ChartComputer.Compute(birthDetails, Array.Empty<SpecialPointSeed>(), ayanamsa);
        var sun = SwissEphemerisProvider.GetSunTimes(birthDetails);

        var seeds = new List<SpecialPointSeed>();
        seeds.AddRange(ArudhaCalculator.Compute(d1));
        seeds.Add(BhaavaLagnaCalculator.Compute(birthDetails, sun, ayanamsa));
        seeds.Add(HoraLagnaCalculator.Compute(birthDetails, sun, ayanamsa));
        seeds.Add(GhatiLagnaCalculator.Compute(birthDetails, sun, ayanamsa));
        seeds.Add(SreeLagnaCalculator.Compute(
            d1.Planets.Single(p => p.Planet == "Ascendant").NirayanaLongitudeDegrees!.Value,
            d1.Planets.Single(p => p.Planet == "Moon").NirayanaLongitudeDegrees!.Value));
        if (subPlanetRules is not null)
            seeds.AddRange(SubPlanetCalculator.Compute(birthDetails, sun,
                d1.Planets.Single(p => p.Planet == "Sun").NirayanaLongitudeDegrees!.Value, subPlanetRules, ayanamsa));
        else
        {
            var (gulika, maandi) = UpagrahaCalculator.Compute(birthDetails, sun, ayanamsa);
            seeds.Add(gulika);
            seeds.Add(maandi);
        }
        return seeds;
    }
}
