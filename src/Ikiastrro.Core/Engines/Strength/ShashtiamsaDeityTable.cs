using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// The 60 Shashtiamsha (D60) names and their benefic/malefic nature, BPHS order. Names follow the
/// list Jagannatha Hora shows (Basics → Amsa rulers → D-60, RamakrishnanP: parts 2, 4, 5, 7, 8, 10,
/// 11, 13–17, 23, 24, 29, 30, 35, 37–41, 43, 44, 46, 47, 51, 59 checked), which PyJHora
/// (const.py, varga 60) and Maitreya8 (Lang::getShastiamsaName) list identically; the nature is
/// Maitreya8's k_shastiamsa_benefic (GenericTableWriter::writeShastiamsaLords). Replaces an
/// unsourced list that drifted from part 30 on (decisions/006). The shashtiamsha number (1-60)
/// comes from the degree traversed in the D1 sign (Traditional Parashari,
/// `LinearVargaSignRule(60, 1)`: floor(degreeInSign * 2) + 1); odd signs read the name list
/// 1→60, even signs reverse it (part 1 of an even sign = name 60).
/// </summary>
public static class ShashtiamsaDeityTable
{
    public readonly record struct Deity(int Number, string Name, bool IsMalefic);

    private static readonly (string Name, bool IsMalefic)[] Names =
    [
        ("Ghora", true), ("Rakshasa", true), ("Deva", false), ("Kubera", false), ("Yaksha", false),
        ("Kinnara", false), ("Bhrashta", true), ("Kulaghna", true), ("Garala", true), ("Vahni", true),
        ("Maaya", true), ("Purishaka", true), ("Apampati", false), ("Marut", false), ("Kaala", true),
        ("Sarpa", true), ("Amrita", false), ("Indu", false), ("Mridu", false), ("Komala", false),
        ("Heramba", false), ("Brahma", false), ("Vishnu", false), ("Maheshwara", false), ("Deva", false),
        ("Ardra", false), ("Kalinaasha", false), ("Kshiteesha", false), ("Kamalaakara", false), ("Gulika", true),
        ("Mrityu", true), ("Kaala", true), ("Davaagni", true), ("Ghora", true), ("Yama", true),
        ("Kantaka", true), ("Sudha", false), ("Amrita", false), ("Poorna-Chandra", false), ("Visha-dagdha", true),
        ("Kulanaasha", true), ("Vamsha-kshaya", true), ("Utpaata", true), ("Kaala", true), ("Saumya", false),
        ("Komala", false), ("Sheetala", false), ("Karaala-damshtra", true), ("Chandramukhi", false), ("Praveena", false),
        ("Kaala-paavaka", true), ("Dandaayudha", true), ("Nirmala", false), ("Saumya", false), ("Kroora", true),
        ("Ati-sheetala", false), ("Amrita", false), ("Payodhi", false), ("Bhramana", true), ("Chandra-rekha", false)
    ];

    /// <summary>The shashtiamsha number (1-60) for a degree traversed within the sign.</summary>
    public static int NumberFor(double degreeInSign) => (int)(((degreeInSign % 30) + 30) % 30 * 2) + 1;

    /// <summary>The deity for a planet's placement, given its (D1) sign and degree within
    /// that sign. Odd signs (Aries, Gemini, Leo, ...) read the list 1→60; even signs
    /// reverse it.</summary>
    public static Deity Lookup(ZodiacName sign, double degreeInSign)
    {
        var number = NumberFor(degreeInSign);
        var odd = (int)sign % 2 == 0; // Aries = 0 is the 1st sign, i.e. odd
        var index = (odd ? number : 61 - number) - 1;
        var (name, isMalefic) = Names[index];
        return new Deity(number, name, isMalefic);
    }
}
