using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// Pakṣa Bala (BPHS, Kāla Bala). With e = the Moon's elongation from the Sun folded to 0–180°,
/// a benefic gets e/3 and a malefic 60 − e/3. Jupiter and Venus are benefics; the Sun, Mars and
/// Saturn malefics. The Moon always takes e/3, doubled (JHora does the same, even for a waning
/// Moon). Mercury is a benefic alone or with more benefics than malefics in his sign, a malefic
/// with more malefics; on a tie the sign-mate nearest in longitude decides (PVR Integrated, as
/// PyJHora's <c>benefics_and_malefics</c> applies it). For that count the Moon is a benefic when
/// waxing and a malefic when waning; the nodes are not counted.
/// </summary>
public static class PakshaBala
{
    public static double Compute(PlanetName planet, IReadOnlyDictionary<PlanetName, double> longitudes)
    {
        var sun = longitudes[PlanetName.Sun];
        var moon = longitudes[PlanetName.Moon];
        var ahead = Normalize(moon - sun);
        var e = ahead > 180 ? 360 - ahead : ahead;

        if (planet == PlanetName.Moon) return 2 * e / 3.0;
        return IsBenefic(planet, longitudes, waxingMoon: ahead < 180) ? e / 3.0 : 60 - e / 3.0;
    }

    /// <summary>Benefic or malefic for Pakṣa Bala — fixed for every planet but Mercury and the Moon.</summary>
    public static bool IsBenefic(PlanetName planet, IReadOnlyDictionary<PlanetName, double> longitudes, bool waxingMoon)
    {
        bool Fixed(PlanetName p) => p switch
        {
            PlanetName.Jupiter or PlanetName.Venus => true,
            PlanetName.Moon => waxingMoon,
            _ => false,
        };
        if (planet != PlanetName.Mercury) return Fixed(planet);

        var mercury = longitudes[PlanetName.Mercury];
        var sign = (int)(Normalize(mercury) / 30);
        var mates = longitudes
            .Where(kv => kv.Key is not (PlanetName.Mercury or PlanetName.Rahu or PlanetName.Ketu)
                         && (int)(Normalize(kv.Value) / 30) == sign)
            .ToList();
        var benefics = mates.Count(kv => Fixed(kv.Key));
        var malefics = mates.Count - benefics;
        if (benefics != malefics) return benefics > malefics;
        if (mates.Count == 0) return true;
        var nearest = mates.MinBy(kv => Math.Abs(kv.Value - mercury)).Key;
        return Fixed(nearest);
    }

    private static double Normalize(double degrees) => ((degrees % 360) + 360) % 360;
}
