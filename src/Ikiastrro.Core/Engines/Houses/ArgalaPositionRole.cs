namespace Ikiastrro.Core.Engines.Houses;

/// <summary>What each argala position does for the matter it acts on, from PVR sec.10.7 "Use of Argala"
/// (pp.106-107, SRC_PVR_INTEGRATED) — the short form of <c>tbl_Rule_Argala.SignificanceNote</c> (migration 129).
/// The 2nd and 4th are the primary argalas, the 11th is the catalyst, the 5th is secondary.</summary>
public static class ArgalaPositionRole
{
    /// <summary>Short label for the column header, or null when the offset is not an argala position.</summary>
    public static string? Label(int offset) => offset switch
    {
        2 => "ingredient",
        4 => "driver",
        11 => "catalyst",
        5 => "secondary",
        _ => null
    };

    /// <summary>One-line meaning of the position, for reading a pair aloud.</summary>
    public static string? Meaning(int offset) => offset switch
    {
        2 => "the basic ingredient that sustains the matter",
        4 => "the factor that drives its mood, state and progress",
        11 => "the catalyst that can bring it gains",
        5 => "a secondary, contributing factor",
        _ => null
    };
}
