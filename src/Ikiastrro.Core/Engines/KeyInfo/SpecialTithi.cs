using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>One special tithi: the k-th multiple of the birth Moon–Sun elongation, read as a tithi.</summary>
public sealed record SpecialTithiRow(
    string Name, int Cycle, int Multiple, int TithiNumber, string TithiName, double PercentLeft,
    PlanetName Lord, string Deity, int IndexInPaksha, bool Bright);

/// <summary>
/// Special tithis (Janma, Dhana, Bhrātṛ, Mātṛ, Putra, Śatru, Kalatra, Mṛtyu, Bhāgya, Karma, Lābha, Vyaya, then
/// the same twelve for cycles 2 and 3): the k-th special tithi is the tithi at k × the birth elongation
/// (Moon − Sun), k = 1…36, taken mod 360°. Tithi lords run Sun…Rahu over 1–8 and 16–23, Sun…Venus over 9–14
/// and 24–29, with Saturn on Pūrṇimā and Rahu on Amāvasyā; deities are the fifteen Nityā devīs. Matches
/// JHora's "Special Tithis" for 1_Ramakrishnan, all 36 rows.
/// </summary>
public static class SpecialTithi
{
    private static readonly string[] Names =
    {
        "Janma", "Dhana", "Bhratri", "Matri", "Putra", "Satru", "Kalatra", "Mrityu", "Bhagya", "Karma", "Laabha", "Vyaya",
    };

    private static readonly string[] TithiNames =
    {
        "Pratipat", "Dwitiya", "Tritiya", "Chaturthi", "Panchami", "Shashthi", "Sapthami", "Ashtami", "Navami",
        "Dasami", "Ekadasi", "Dwadasi", "Trayodasi", "Chaturdasi",
    };

    private static readonly string[] Deities =
    {
        "Kaameswari", "Bhaga Maalini", "Nitya Klinna", "Bherunda", "Vahni Vaasini", "Vajreswari", "Siva Dooti",
        "Tvarita", "Kula Sundari", "Nitya", "Neela Pataaka", "Vijaya", "Sarva Mangala", "Jwaalaa Maalini", "Chitra",
    };

    private static readonly PlanetName[] LordCycle =
    {
        PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter,
        PlanetName.Venus, PlanetName.Saturn, PlanetName.Rahu,
    };

    public static PlanetName LordOf(int tithi)
    {
        if (tithi == 15) return PlanetName.Saturn;
        if (tithi == 30) return PlanetName.Rahu;
        var inPaksha = (tithi - 1) % 15;                       // 0-13
        return inPaksha < 8 ? LordCycle[inPaksha] : LordCycle[inPaksha - 8];
    }

    public static IReadOnlyList<SpecialTithiRow> Compute(double sunLongitude, double moonLongitude)
    {
        var elongation = AstroMath.Normalize(moonLongitude - sunLongitude);
        return Enumerable.Range(1, 36).Select(k =>
        {
            var phase = (k * elongation) % 360.0;
            var tithi = (int)(phase / 12) + 1;
            var left = (1 - (phase % 12) / 12) * 100;
            var inPaksha = (tithi - 1) % 15 + 1;
            var bright = tithi <= 15;
            var tithiName = inPaksha == 15
                ? (bright ? "Pournimasya" : "Amavasya")
                : (bright ? "Sukla " : "Krishna ") + TithiNames[inPaksha - 1];
            var cycle = (k - 1) / 12 + 1;
            var name = Names[(k - 1) % 12] + (cycle > 1 ? $" (cycle {cycle})" : "");
            return new SpecialTithiRow(name, cycle, k, tithi, tithiName, left, LordOf(tithi), Deities[inPaksha - 1], inPaksha, bright);
        }).ToList();
    }
}
