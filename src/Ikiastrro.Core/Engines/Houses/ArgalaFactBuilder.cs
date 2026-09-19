using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Houses;

/// <summary>
/// Flattens ArgalaCalculator's evaluation into tbl_Fact_Argala rows (migration 128) for a whole
/// chart — the 12 houses (PVR's own Exercise 16 scope) and, separately, the 9 grahas' own
/// occupied signs (PVR's other worked-example usage, sec.10.7, targeting a karaka directly).
/// Pure C#, no DB read — mirrors HouseEngine.BuildHouseLords' role of turning a chart-agnostic
/// calculator's output into a flat row list a repository can insert as-is.
/// </summary>
public static class ArgalaFactBuilder
{
    public static List<ChartArgalaFact> BuildForHouses(
        ZodiacName ascendantSign,
        IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> occupancy)
    {
        var facts = new List<ChartArgalaFact>();
        for (var houseNumber = 1; houseNumber <= 12; houseNumber++)
        {
            var targetSign = HouseEngine.GetHouseSign(ascendantSign, houseNumber);
            var evaluation = ArgalaCalculator.Evaluate(targetSign, occupancy);
            AppendFacts(facts, "House", houseNumber.ToString(), houseNumber, evaluation);
        }
        return facts;
    }

    public static List<ChartArgalaFact> BuildForPlanets(
        ZodiacName ascendantSign,
        IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> occupancy)
    {
        var facts = new List<ChartArgalaFact>();
        var signByPlanet = occupancy
            .SelectMany(kv => kv.Value.Select(planet => (Planet: planet, Sign: kv.Key)))
            .ToDictionary(x => x.Planet, x => x.Sign);

        foreach (var (planet, sign) in signByPlanet)
        {
            var houseNumber = AstroMath.CountFromSignToSign(ascendantSign, sign);
            var evaluation = ArgalaCalculator.Evaluate(sign, occupancy);
            AppendFacts(facts, "Graha", planet.ToString(), houseNumber, evaluation);
        }
        return facts;
    }

    private static void AppendFacts(
        List<ChartArgalaFact> facts, string targetKind, string targetKey, int targetHouseNumber,
        ArgalaCalculator.ArgalaEvaluation evaluation)
    {
        foreach (var position in evaluation.Argala)
        {
            // HouseOffset 3 can only appear on the Argala side via the sec.10.6 exception —
            // ArgalaCalculator moves it there itself, never seeds 3 as a normal argala offset.
            var exceptionApplied = position.Position.HouseOffset == 3;
            foreach (var occupant in position.Occupants)
                facts.Add(new ChartArgalaFact(targetKind, targetKey, targetHouseNumber, evaluation.Target,
                    "ARGALA", position.Position.HouseOffset, position.Position.IsPrimary, occupant,
                    exceptionApplied, evaluation.CountedAntiZodiacally));
        }

        foreach (var position in evaluation.Virodhargala)
        foreach (var occupant in position.Occupants)
            facts.Add(new ChartArgalaFact(targetKind, targetKey, targetHouseNumber, evaluation.Target,
                "VIRODHARGALA", position.Position.HouseOffset, position.Position.IsPrimary, occupant,
                false, evaluation.CountedAntiZodiacally));
    }
}
