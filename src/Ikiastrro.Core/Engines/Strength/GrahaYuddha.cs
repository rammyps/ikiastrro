using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>One Graha Yuddha (planetary war) pair: who wins, who loses, and how close they are.</summary>
public sealed record GrahaYuddhaPair(PlanetName Winner, PlanetName Loser, double OrbDegrees);

/// <summary>
/// Graha Yuddha detection (tbl_Rule_PlanetaryWar, migration 072, SRC_RAMAN_GRAHA_BHAVA_BALAS via
/// SRC_PVR_INTEGRATED): two of the five tara grahas within <see cref="OrbDegrees"/> of D1
/// longitude are at war, and the one with the more northern ecliptic latitude wins. Sun, Moon
/// and the nodes never take part. Shared by <see cref="ShadbalaCalculator"/>'s Yuddha Bala and
/// Astro Facts' planetary-war table so the two cannot disagree.
/// </summary>
public static class GrahaYuddha
{
    public static readonly PlanetName[] Participants =
        { PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn };

    public const double OrbDegrees = 1.00;

    /// <summary>Every war pair among the participants that have both a longitude and a latitude.
    /// A missing latitude counts as 0°, the same default Yuddha Bala has always used.</summary>
    public static IReadOnlyList<GrahaYuddhaPair> Find(
        IReadOnlyDictionary<PlanetName, double> longitudes,
        IReadOnlyDictionary<PlanetName, double> latitudes)
    {
        var pairs = new List<GrahaYuddhaPair>();
        for (var i = 0; i < Participants.Length; i++)
        for (var j = i + 1; j < Participants.Length; j++)
        {
            var a = Participants[i];
            var b = Participants[j];
            if (!longitudes.TryGetValue(a, out var lonA) || !longitudes.TryGetValue(b, out var lonB)) continue;
            var orb = Math.Abs(((lonA - lonB) % 360 + 360) % 360);
            if (orb > 180) orb = 360 - orb;
            if (orb > OrbDegrees) continue;

            var (winner, loser) = latitudes.GetValueOrDefault(a) >= latitudes.GetValueOrDefault(b) ? (a, b) : (b, a);
            pairs.Add(new GrahaYuddhaPair(winner, loser, orb));
        }
        return pairs;
    }
}
