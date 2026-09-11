using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Ghati Lagna (Ghatika Lagna). Classical rule (PVR sec 5.4, tbl_Rule_SpecialLagnaTimeRate row
/// GHATI_LAGNA): take the Sun's sidereal longitude at the Vedic day's opening sunrise
/// (<see cref="SunTimes.Sunrise"/> — the PREVIOUS calendar day's sunrise for a night birth)
/// and add 1.25° per minute of clock time-of-day elapsed since that sunrise (one rasi per
/// ghati = 24 minutes). Same time-of-day-difference shortcut as
/// <see cref="HoraLagnaCalculator"/> — valid because 1.25°/min × 1440min = 1800° = 5×360°, so
/// the real cross-midnight elapsed time and the bounded time-of-day difference normalize to the
/// identical result.
///
/// PVR sec 5.5: a 1-minute birthtime error shifts GL by 1°15', more sensitive than any other
/// special lagna in this book — correct the birthtime before trusting GL in vargas.
/// </summary>
public static class GhatiLagnaCalculator
{
    public static SpecialPointSeed Compute(BirthDetails bd, SunTimes sun, AyanamsaDefinition? ayanamsa = null)
    {
        var reference = sun.Sunrise;
        var sunLonAtSunrise = SwissEphemerisProvider
            .GetSiderealPositions(reference, bd.Latitude, bd.Longitude, ayanamsa)
            .PlanetLongitudes[PlanetName.Sun];

        var birth = BirthMomentFactory.Create(bd);
        var elapsedMinutes = (birth.TimeOfDay - reference.TimeOfDay).TotalMinutes;

        var gl = AstroMath.Normalize(sunLonAtSunrise + elapsedMinutes * 1.25);
        return new SpecialPointSeed("GL", "SpecialLagna", gl);
    }
}
