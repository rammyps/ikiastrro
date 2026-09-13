using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Shared TIME_FROM_SUNRISE mechanics behind Bhaava / Hora / Ghati Lagna
/// (tbl_Rule_SpecialLagnaTimeRate, db/28 — one shared rule family, three DegreesPerMinute
/// coefficients: Bhaava 0.25, Hora 0.5, Ghati 1.25). PVR sec 5.2-5.4.
///
/// Rule: take the Sun's sidereal longitude at the Vedic day's opening sunrise
/// (<see cref="SunTimes.Sunrise"/> — the PREVIOUS calendar day's sunrise for a night birth)
/// and add <paramref name="degreesPerMinute"/> per minute of clock time-of-day elapsed since
/// that sunrise. For a night birth the time-of-day difference is negative (birth precedes the
/// opening sunrise), which is correct — all three points run behind the Sun.
///
/// Extracted from HoraLagnaCalculator (the only one of the three built before 2026-09-13) so
/// Bhaava/GhatiLagnaCalculator reuse the exact same mechanics rather than re-deriving them —
/// the DB already treats these as one rule family, not three independent ones.
/// </summary>
public static class SpecialLagnaTimeRateCalculator
{
    public static SpecialPointSeed Compute(string code, double degreesPerMinute, BirthDetails bd, SunTimes sun,
        AyanamsaDefinition? ayanamsa = null)
    {
        var reference = sun.Sunrise;
        var sunLonAtSunrise = SwissEphemerisProvider
            .GetSiderealPositions(reference, bd.Latitude, bd.Longitude, ayanamsa)
            .PlanetLongitudes[PlanetName.Sun];

        var birth = BirthMomentFactory.Create(bd);
        var elapsedMinutes = (birth.TimeOfDay - reference.TimeOfDay).TotalMinutes;

        var longitude = AstroMath.Normalize(sunLonAtSunrise + elapsedMinutes * degreesPerMinute);
        return new SpecialPointSeed(code, "SpecialLagna", longitude);
    }
}
