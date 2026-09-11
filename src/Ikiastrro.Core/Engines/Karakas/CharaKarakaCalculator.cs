using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Jaimini 8-karaka (Ashta) assignment from the D1 chart. Ranks the 8 grahas
/// (Sun..Saturn, Rahu) by longitude WITHIN their sign, descending; Rahu's key is
/// (30 - itsDegreeInSign) because it is always retrograde. Highest -> AK, lowest -> DK.
/// Ketu is not ranked.
///
/// SRC_PVR_INTEGRATED sec.8.2, Table 13 (verified against the raw book extract). Mirrors
/// <c>tbl_Rule_Karaka</c> WHERE <c>KarakaScheme='Chara'</c> (migration 085) — cited there but
/// not read from there (the "verified mirror" pattern); CLI <c>verify-jaimini</c> cross-checks
/// this order against that table. PVR's own tie-break for an exact-longitude tie (share the
/// karakatva, fall back to the sthira karaka) is deliberately not implemented in the stable
/// <c>ThenBy</c> below — it never fires on real ephemeris data, but a non-C# port should know
/// the simplification is there.
/// </summary>
public static class CharaKarakaCalculator
{
    private static readonly PlanetName[] Ranked =
    {
        PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury,
        PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn, PlanetName.Rahu
    };

    public static IReadOnlyDictionary<PlanetName, CharaKaraka> Assign(
        IReadOnlyDictionary<PlanetName, double> degreeInSignByPlanet)
    {
        double Key(PlanetName p)
        {
            var deg = degreeInSignByPlanet[p] % 30.0;
            if (deg < 0) deg += 30.0;
            return p == PlanetName.Rahu ? 30.0 - deg : deg;
        }

        var order = Ranked
            .OrderByDescending(Key)
            .ThenBy(p => Array.IndexOf(Ranked, p))   // stable tie-break (never fires on real data)
            .ToArray();

        var result = new Dictionary<PlanetName, CharaKaraka>();
        for (var i = 0; i < order.Length; i++)
            result[order[i]] = (CharaKaraka)i;
        return result;
    }
}
