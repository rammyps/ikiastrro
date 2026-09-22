using Ikiastrro.Core.Models;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Dispositors;

public sealed class DispositorEngine : IDispositorEngine
{
    public IReadOnlyList<DispositorChain> Compute(ChartAnalysisInput chart)
    {
        var positions = chart.Planets
            .Where(p => p.PointKind == "Graha" && Enum.TryParse<PlanetName>(p.Planet, true, out _))
            .GroupBy(p => p.Planet, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => Enum.Parse<PlanetName>(g.Key, true), g => g.First());

        return positions.Keys.OrderBy(p => (int)p).Select(start => Follow(start, positions)).ToList();
    }

    private static DispositorChain Follow(PlanetName start, IReadOnlyDictionary<PlanetName, PlanetPosition> positions)
    {
        var chain = new List<string> { start.ToString() };
        var visited = new Dictionary<PlanetName, int> { [start] = 0 };
        var current = start;

        while (true)
        {
            if (!positions.TryGetValue(current, out var position) ||
                !Enum.TryParse<ZodiacName>(position.Sign, true, out var sign))
                return new(start.ToString(), chain, null, false, "MISSING_PLACEMENT");

            var lord = Enum.Parse<PlanetName>(HouseEngine.GetSignLord(sign));
            chain.Add(lord.ToString());

            if (lord == current)
                return new(start.ToString(), chain, current.ToString(), false, "SELF_DISPOSED");

            if (visited.TryGetValue(lord, out var cycleStart))
            {
                var cycle = chain.Skip(cycleStart).Take(chain.Count - cycleStart - 1).ToList();
                return new(start.ToString(), chain, null, cycle.Count == 2, cycle.Count == 2 ? "MUTUAL_RECEPTION" : "CYCLE", cycle);
            }

            visited[lord] = chain.Count - 1;
            current = lord;
        }
    }
}
