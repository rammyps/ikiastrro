using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Bhaava Lagna. Classical rule (PVR sec 5.2, tbl_Rule_SpecialLagnaTimeRate row BHAAVA_LAGNA):
/// take the Sun's sidereal longitude at the Vedic day's opening sunrise
/// (<see cref="SunTimes.Sunrise"/> — the PREVIOUS calendar day's sunrise for a night birth)
/// and add 0.25° per minute of clock time-of-day elapsed since that sunrise (one rasi per 2
/// hours). Same time-of-day-difference shortcut as <see cref="HoraLagnaCalculator"/> — valid
/// because 0.25°/min × 1440min = 360° exactly, so the real cross-midnight elapsed time and the
/// bounded time-of-day difference normalize to the identical result.
///
/// PVR sec 5.2 defines BL "only for the sake of completeness" and never uses it further in the
/// book (`UsedInBook = 0` on the seeded row) — built here anyway for JHora/UI parity.
/// </summary>
public static class BhaavaLagnaCalculator
{
    public static SpecialPointSeed Compute(BirthDetails bd, SunTimes sun, AyanamsaDefinition? ayanamsa = null)
    {
        var reference = sun.Sunrise;
        var sunLonAtSunrise = SwissEphemerisProvider
            .GetSiderealPositions(reference, bd.Latitude, bd.Longitude, ayanamsa)
            .PlanetLongitudes[PlanetName.Sun];

        var birth = BirthMomentFactory.Create(bd);
        var elapsedMinutes = (birth.TimeOfDay - reference.TimeOfDay).TotalMinutes;

        var bl = AstroMath.Normalize(sunLonAtSunrise + elapsedMinutes * 0.25);
        return new SpecialPointSeed("BL", "SpecialLagna", bl);
    }
}
