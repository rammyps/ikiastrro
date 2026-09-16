using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Pipeline;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// The arudha pada of each of the 9 grahas (Parashara/Jaimini via PVR) — the graha analog of
/// <see cref="ArudhaCalculator"/>'s bhava arudhas. For graha G, occupying sign S, owning sign(s)
/// O: let n = signs counted from S to O (inclusive, 1..12); the pada is n signs on from O
/// (inclusive). If the pada lands on S itself or the 7th from it, take the 10th sign from the
/// pada instead. Sun/Moon/Rahu/Ketu own exactly 1 sign each (Rahu = Aquarius, Ketu = Scorpio, per
/// this project's convention — <see cref="Dignity.DignityEngine"/>'s own-sign check). Mars/
/// Mercury/Jupiter/Venus/Saturn own 2 signs each — <see cref="StrongerRasiComparator"/> picks
/// which one to use, per PVR sec.15.5.2.
///
/// SRC_PVR_INTEGRATED sec.9.5 "Computation of Graha Arudhas" (verified against the raw book
/// extract, incl. worked Example 30). Emitted as PointKind "GrahaArudha", Code "GA_&lt;Planet&gt;"
/// (e.g. "GA_Sun" .. "GA_Ketu").
///
/// Degree-in-sign: PVR's text only fixes the pada's sign, not a degree — it has no continuous-
/// longitude concept for arudhas at all. Following the same practical convention
/// <see cref="ArudhaCalculator"/> uses for bhava arudhas (placing the pada at a fixed reference
/// degree so it has a longitude for varga projection), each graha arudha here is placed at its
/// own source planet's degree-in-sign — a deliberate implementation choice, not from PVR text.
/// </summary>
public static class GrahaArudhaCalculator
{
    private static readonly PlanetName[] Grahas =
    {
        PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter,
        PlanetName.Venus, PlanetName.Saturn, PlanetName.Rahu, PlanetName.Ketu
    };

    public static IReadOnlyList<SpecialPointSeed> Compute(ChartAnalysisInput d1)
    {
        var byPlanet = d1.Planets
            .Where(p => p.Planet != "Ascendant")
            .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet));

        var seeds = new List<SpecialPointSeed>();
        foreach (var graha in Grahas)
        {
            var placement = byPlanet[graha];
            var ownSign = OwnedSignFor(graha, d1);
            var planetSign = Enum.Parse<ZodiacName>(placement.Sign);

            var n = AstroMath.CountFromSignToSign(planetSign, ownSign);
            var pada = HouseEngine.GetHouseSign(ownSign, n);
            var fromPlanetSign = AstroMath.CountFromSignToSign(planetSign, pada);
            if (fromPlanetSign == 1 || fromPlanetSign == 7)
                pada = HouseEngine.GetHouseSign(pada, 10);

            var degreeInSign = AstroMath.GetDegreesInSign(placement.NirayanaLongitudeDegrees!.Value);
            var longitude = (int)pada * 30.0 + degreeInSign;
            seeds.Add(new SpecialPointSeed($"GA_{graha}", "GrahaArudha", AstroMath.Normalize(longitude)));
        }
        return seeds;
    }

    /// <summary>The sign this graha's own arudha is computed from — its single own sign for
    /// Sun/Moon/Rahu/Ketu, or the PVR sec.15.5.2 "stronger" of its 2 own signs otherwise.</summary>
    private static ZodiacName OwnedSignFor(PlanetName graha, ChartAnalysisInput d1)
    {
        if (graha == PlanetName.Rahu) return ZodiacName.Aquarius;
        if (graha == PlanetName.Ketu) return ZodiacName.Scorpio;

        var owned = HouseEngine.OwnSigns[graha.ToString()];
        return owned.Length == 1 ? owned[0] : StrongerRasiComparator.Compare(owned[0], owned[1], d1);
    }
}
