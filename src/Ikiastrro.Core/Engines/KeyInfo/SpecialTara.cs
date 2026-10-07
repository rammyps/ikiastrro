using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>One of the eleven special tārās: its name, meaning, the nakṣatra it falls on (null when that is
/// Abhijit, which has no <see cref="ConstellationName"/>) and that nakṣatra's Vimśottarī lord.</summary>
public sealed record SpecialTaraRow(
    string Name, string Meaning, ConstellationName? Nakshatra, bool IsAbhijit, PlanetName Lord);

/// <summary>
/// Special tārās, counted from the Moon's or the Lagna's nakṣatra in the 28-nakṣatra circle that includes
/// Abhijit (between U.Āṣāḍhā and Śravaṇa). The offsets from the reference nakṣatra are 1, 10, 18, 16, 4, 7, 12,
/// 28, 19, 22 and 25 for Janma, Karma, Sāmudāyika, Sāṅghātika, Jāti, Naidhana, Deśa, Abhiṣeka, Ādhāna,
/// Vainaśika and Mānasa (PyJHora <c>special_thaara_map</c>, AGPL — table only, not code).
/// Abhijit's lord is the Sun, as JHora prints it. Matches JHora's "Special Taras" for 1_Ramakrishnan, from both
/// the Moon and the Lagna.
/// </summary>
public static class SpecialTara
{
    private const int AbhijitIndex = 21;   // position of Abhijit in the 28-nakṣatra circle (0-based)

    private static readonly (string Name, string Meaning, int Offset)[] Taras =
    {
        ("Janma", "Existence", 1), ("Karma", "Work", 10), ("Samudayika", "Collective", 18),
        ("Sanghatika", "Social", 16), ("Jaati", "Family", 4), ("Naidhana", "Death", 7),
        ("Desa", "Nation", 12), ("Abhisheka", "Coronation", 28), ("Aadhaana", "Kindling", 19),
        ("Vainasika", "Destructive", 22), ("Maanasa", "Mental", 25),
    };

    public static IReadOnlyList<SpecialTaraRow> FromLongitude(double longitude)
    {
        var baseStar = (int)AstroMath.GetNakshatraAndPada(longitude).Nakshatra;
        if (baseStar >= AbhijitIndex) baseStar++;   // step over Abhijit in the 28-circle

        return Taras.Select(t =>
        {
            var star = (baseStar + t.Offset - 1) % 28;
            if (star == AbhijitIndex)
                return new SpecialTaraRow(t.Name, t.Meaning, null, true, PlanetName.Sun);
            var nak = (ConstellationName)(star > AbhijitIndex ? star - 1 : star);
            return new SpecialTaraRow(t.Name, t.Meaning, nak, false, AstroMath.NakshatraLordOrder[(int)nak % 9]);
        }).ToList();
    }
}
