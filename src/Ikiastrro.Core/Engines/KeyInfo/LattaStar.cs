using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>A graha's lattā (kick) nakṣatra: the one it occupies and the one it strikes.</summary>
public sealed record LattaRow(PlanetName Planet, ConstellationName Occupied, ConstellationName Latta);

/// <summary>
/// Lattā nakṣatras. Each graha strikes the n-th nakṣatra from the one it occupies, counted forward (+) or
/// backward (−) with its own nakṣatra as 1: Sun 12 forward, Moon 22 back, Mars 3 forward, Mercury 7 back,
/// Jupiter 6 forward, Venus 5 back, Saturn 8 forward, Rahu and Ketu 9 back (PyJHora <c>latta_stars_of_planets</c>,
/// AGPL — table only). Counted on the 27 nakṣatras, which is what JHora's output needs: all eight grahas JHora
/// lists for 1_Ramakrishnan match. JHora's "Aspected Stars" column is a separate, undocumented rule and is not
/// reproduced.
/// </summary>
public static class LattaStar
{
    private static readonly (PlanetName Planet, int Count, int Direction)[] Rules =
    {
        (PlanetName.Sun, 12, 1), (PlanetName.Moon, 22, -1), (PlanetName.Mars, 3, 1), (PlanetName.Mercury, 7, -1),
        (PlanetName.Jupiter, 6, 1), (PlanetName.Venus, 5, -1), (PlanetName.Saturn, 8, 1),
        (PlanetName.Rahu, 9, -1), (PlanetName.Ketu, 9, -1),
    };

    public static ConstellationName For(PlanetName planet, double longitude)
    {
        var (_, count, direction) = Rules.First(r => r.Planet == planet);
        var occupied = (int)AstroMath.GetNakshatraAndPada(longitude).Nakshatra;
        return (ConstellationName)((((occupied + direction * (count - 1)) % 27) + 27) % 27);
    }

    public static LattaRow Row(PlanetName planet, double longitude) =>
        new(planet, AstroMath.GetNakshatraAndPada(longitude).Nakshatra, For(planet, longitude));
}
