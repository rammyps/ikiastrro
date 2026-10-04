using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.LifeMatters.Promise;

/// <summary>
/// The independence families of docs/architecture/key_inference_promise.md §5. The verdict counts one
/// testimony per family, so evidence that shares one cause is not counted twice while every row stays
/// visible. The key formats live here, in one place, and tests pin them:
/// <list type="bullet">
/// <item><c>Placement:{planet}</c> — sign dignity, combustion and the avastha of one planet.</item>
/// <item><c>LordshipPlacement:{planet}</c> — where a lord or karaka stands from the target (and any
/// aspect that follows from that same placement).</item>
/// <item><c>Conjunction:{planet}</c> — a conjunction on that planet and any yoga formed by it.</item>
/// <item><c>Influence:{planet}</c> — a planet that is neither lord nor karaka acting on the target.</item>
/// <item><c>Intervention</c> — Argala and the Virodhargala that answers it.</item>
/// <item><c>Ashtakavarga</c> — the sign's SAV and the lord's BAV there.</item>
/// </list>
/// Ṣaḍbala and Amsabala are not families: they set the capacity of a testimony and never count.
/// </summary>
public static class PromiseFamilies
{
    public const string Target = "Target";
    public const string Intervention = "Intervention";
    public const string Ashtakavarga = "Ashtakavarga";

    public static string Placement(PlanetName p) => $"Placement:{p}";
    public static string LordshipPlacement(PlanetName p) => $"LordshipPlacement:{p}";
    public static string Conjunction(PlanetName p) => $"Conjunction:{p}";
    public static string Influence(PlanetName p) => $"Influence:{p}";
    public static string Yoga(string name) => $"Yoga:{name}";

    /// <summary>One family's direction from its testimonies: Neutral ones are ignored; supportive and
    /// obstructive together, or any Mixed, read Mixed.</summary>
    public static Direction Resolve(IEnumerable<Direction> directions)
    {
        var set = directions.Where(d => d != Direction.Neutral).ToHashSet();
        if (set.Count == 0) return Direction.Neutral;
        if (set.Contains(Direction.Mixed) || set.Count > 1) return Direction.Mixed;
        return set.Single();
    }

    /// <summary>The weakest known capacity; Unknown only when none is known.</summary>
    public static Capacity Weakest(IEnumerable<Capacity> capacities)
    {
        var known = capacities.Where(c => c != Capacity.Unknown).ToList();
        return known.Count == 0 ? Capacity.Unknown : known.OrderByDescending(c => (int)c).First();
    }
}
