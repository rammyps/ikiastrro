using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Data;
using Ikiastrro.Data.Statistics;
using Ikiastrro.Web.Components.Workspace;

namespace Ikiastrro.Web.Components.Charts;

/// <summary>One house of one chart read two ways: Raman's benefic/malefic count
/// (<see cref="HouseBeneficMaleficCalculator"/>) and PVR step 5's influences on it
/// (<see cref="TargetInfluences"/>).</summary>
public sealed record HouseVerdictRow(int House, ZodiacName Sign, HouseBeneficMaleficResult Raman, TargetInfluenceReading Influences);

/// <summary>
/// Key Inference About Houses' "What acts on each house" table, for the chart in the picker.
/// Argala comes from <see cref="ArgalaFacts.ForChart"/> through
/// <see cref="LifeMatterStatistics.BuildArgala"/> — the same facts and verdicts Life Matters
/// uses — so a house reads the same here as in Life Matters' "What acts on it" (with no kāraka,
/// since no life matter is chosen). Computed live; nothing stored.
/// </summary>
public static class HouseVerdicts
{
    /// <returns>Empty when the chart lacks a graha's placement.</returns>
    public static IReadOnlyList<HouseVerdictRow> For(LoadedChart chart, IReadOnlyList<ArgalaFactRow> argalaFacts)
    {
        var placements = new Dictionary<PlanetName, ZodiacName>();
        foreach (var g in chart.Grahas)
            if (Enum.TryParse<PlanetName>(g.Planet, out var planet) && Enum.TryParse<ZodiacName>(g.Sign, out var sign))
                placements[planet] = sign;
        if (placements.Count < 9) return [];

        var lagna = Enum.Parse<ZodiacName>(chart.AscendantSign);
        var raman = HouseBeneficMaleficCalculator.ComputeAll(lagna, chart.Grahas);
        return raman.Select(r =>
        {
            var argala = LifeMatterStatistics.BuildArgala(argalaFacts.Where(f => f.TargetHouseNumber == r.HouseNumber).ToList());
            var influences = TargetInfluences.Read(lagna, placements, r.Sign,
                argalaPlanets: Planets(argala.Pairs.Where(p => p.Verdict is ArgalaVerdict.Holds or ArgalaVerdict.Contested).SelectMany(p => p.ArgalaPlanets)),
                virodhargalaPlanets: Planets(argala.Pairs.SelectMany(p => p.ObstructingPlanets)));
            return new HouseVerdictRow(r.HouseNumber, r.Sign, r, influences);
        }).ToList();
    }

    private static IEnumerable<PlanetName> Planets(IEnumerable<string> names) =>
        names.Select(n => Enum.TryParse<PlanetName>(n, true, out var p) ? p : (PlanetName?)null)
            .Where(p => p is not null).Select(p => p!.Value).Distinct();
}
