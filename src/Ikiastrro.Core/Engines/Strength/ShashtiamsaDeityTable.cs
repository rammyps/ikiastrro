using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// The 60 Shashtiamsha (D60) names and their benefic/malefic nature — "Classical set per
/// BPHS", as documented in docs/cli/reading/d60-shashtiamsa.md §4. The shashtiamsha number
/// (1-60) comes from the degree traversed in the D1 sign (Traditional Parashari,
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
        ("Sarpa", true), ("Amrita", false), ("Chandra", false), ("Mridu", false), ("Komala", false),
        ("Heramba", false), ("Brahma", false), ("Vishnu", false), ("Maheshwara", false), ("Deva", false),
        ("Ardra", true), ("Kalinaasha", true), ("Kshiteesha", true), ("Amrita", false), ("Payodhi", false),
        ("Kaala", true), ("Davaagni", true), ("Ghora", true), ("Yama", true), ("Ganda-antaka", true),
        ("Sudha", false), ("Amrita", false), ("Poorna-Chandra", false), ("Visha-daghdha", true), ("Kulanaasha", true),
        ("Vamsha-kshaya", true), ("Utpaata", true), ("Kaala", true), ("Saumya", false), ("Komala", false),
        ("Paasha", true), ("Danda-udyata", true), ("Bhaya", true), ("Yaksha", false), ("Kinnara", false),
        ("Bhrashta", true), ("Kulaghna", true), ("Mukhya", false), ("Vamsha-kshaya", true), ("Utpaata", true),
        ("Kaala", true), ("Saumya", false), ("Komala", false), ("Sheetala", false), ("Karaala-damshtra", true)
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
