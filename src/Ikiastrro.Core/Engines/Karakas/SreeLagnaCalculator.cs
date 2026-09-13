using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Sree Lagna (SL). Classical rule (PVR sec 5.7, tbl_Rule_SpecialLagnaFraction, db/28): the
/// natal D1 Lagna longitude plus the fraction of its nakshatra the Moon has already traversed,
/// scaled to the full 360° zodiac, mod 360. The reference point for Sudasa
/// ("Sree Lagna Kendradi Rasi Dasa"). PVR sec 5.8 notes SL moves at roughly twice the rate of
/// the normal Lagna — watch rāśi-border cases.
///
/// Distinct calculation family from Bhaava/Hora/Ghati (NAKSHATRA_FRACTION, not
/// TIME_FROM_SUNRISE) — needs no sunrise/sunset arc, only the already-computed D1 Lagna and
/// Moon longitudes, via the same fraction-elapsed helper Vimshottari Dasha uses
/// (<see cref="AstroMath.GetNakshatraIndexAndFractionElapsed"/>).
///
/// Verified against docs/artifacts/reference-charts/Rammy_Jagannatha.txt: 17 Cn 33' 00.80"
/// (PVR sec 5.7's own worked Example 10 uses the same mechanics).
/// </summary>
public static class SreeLagnaCalculator
{
    public static SpecialPointSeed Compute(double natalLagnaLongitude, double moonLongitude)
    {
        var fractionElapsed = AstroMath.GetNakshatraIndexAndFractionElapsed(moonLongitude).FractionElapsed;
        var longitude = AstroMath.Normalize(natalLagnaLongitude + fractionElapsed * 360.0);
        return new SpecialPointSeed("SL", "SpecialLagna", longitude);
    }
}
