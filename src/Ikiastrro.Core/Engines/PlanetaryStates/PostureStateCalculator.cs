using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.PlanetaryStates;

/// <summary>
/// Sayanaadi Avastha — the 12-state "activity" avastha (PVR sec 15.4.4, Table 36): Sayana
/// (lying down) ... Nidraa (sleeping). Index = ((C x P x A) + M + G + L) mod 12, remainder 0
/// read as 12:
///
///   C = the planet's own nakshatra number (1-27, Aswini = 1)
///   P = the planet's index (Sun=1 .. Saturn=7, Rahu=8, Ketu=9 - matches AstroIds.PlanetId)
///   A = the navamsa the planet occupies within its own rasi (1-9)
///   M = Moon's own nakshatra number (1-27)
///   G = the ghati running at birth (1-60) = floor(JanmaGhatis) + 1
///   L = the rasi occupied by Lagna (1-12, Aries = 1)
///
/// D1-only, like AgeStateCalculator — needs a continuous within-sign degree (for A) and the
/// birth-moment Janma Ghatis (G), neither of which a discrete varga sign carries.
///
/// Hardcoded here rather than read from tbl_Rule_PostureStateFormula at runtime (there is
/// nothing to version — a different convention would be a different formula, not a different
/// parameter), the same "verified mirror" pattern as AshtakavargaCalculator / PanchangaCalculator.
/// Verified against the JHora export for 1_Ramakrishnan's printed Activity table: Sun -> Aagama
/// (index 8), Moon -> Kautuka (index 11), both exact.
/// </summary>
public static class PostureStateCalculator
{
    /// <summary>The raw 1-12 Sayanaadi index for one planet.</summary>
    public static int ComputeIndex(int nakshatraNumber, int planetIndex, int navamsaIndex,
        int moonNakshatraNumber, int ghati, int lagnaRasiNumber)
    {
        var raw = (nakshatraNumber * planetIndex * navamsaIndex
                   + moonNakshatraNumber + ghati + lagnaRasiNumber) % 12;
        return raw == 0 ? 12 : raw;
    }

    /// <summary>Navamsa index (1-9) within a planet's own rasi, from its within-sign degree (0-30).</summary>
    public static int NavamsaIndex(decimal degreeInSign) =>
        (int)(degreeInSign / (30m / 9m)) + 1;

    /// <summary>Ghati running at birth (1-60), from the continuous Janma Ghatis value.</summary>
    public static int GhatiRunning(double janmaGhatis) => (int)janmaGhatis + 1;

    /// <summary>The named state row for a computed index, or null if the Dim seed is missing it
    /// (should never happen — the 12 slots are always seeded together).</summary>
    public static PlanetaryStateRow? For(int index, IReadOnlyDictionary<byte, PlanetaryStateRow> statesBySequence) =>
        statesBySequence.GetValueOrDefault((byte)index);
}
