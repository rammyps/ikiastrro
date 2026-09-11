using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Sree Lagna. Classical rule (PVR sec 5.7, tbl_Rule_SpecialLagnaFraction row SREE_LAGNA):
/// find the fraction of its nakshatra the Moon has traversed, take the same fraction of the
/// full zodiac (360°), and add that to the natal Lagna's longitude. Pure — both inputs are
/// already computed as part of the D1 chart, so this needs no fresh ephemeris call (unlike
/// the sunrise-anchored Bhaava/Hora/Ghati Lagna).
///
/// The seed point for Sudasa ("Sree Lagna Kendradi Rasi Dasa") — PVR sec 5.7 and the Sudasa
/// chapter. Not read as a chart of its own (`HouseReferenceScope = RasiChartOnly` on the
/// seeded row).
/// </summary>
public static class SreeLagnaCalculator
{
    public static SpecialPointSeed Compute(double lagnaLongitude, double moonLongitude)
    {
        var fractionElapsed = AstroMath.GetNakshatraIndexAndFractionElapsed(moonLongitude).FractionElapsed;
        var sl = AstroMath.Normalize(lagnaLongitude + fractionElapsed * 360.0);
        return new SpecialPointSeed("SL", "SpecialLagna", sl);
    }
}
