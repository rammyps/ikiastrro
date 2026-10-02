using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// Nathonnata Bala (BPHS, Kāla Bala), graded by the hour. Unnata = time from local apparent midnight
/// (midpoint of sunset and next sunrise; for a day birth, 12 h minus the time from apparent noon,
/// the midpoint of sunrise and sunset). The Sun, Jupiter and Venus get Unnata × 60/12 h; the Moon,
/// Mars and Saturn 60 minus that; Mercury always 60.
/// JHora reads 27.49 for 1_Ramakrishnan's Sun where this gives 26.87 — its midnight sits ~7½ minutes
/// earlier than the sunset/sunrise midpoint, a convention it does not document.
/// </summary>
public static class NathonnataBala
{
    public static double Compute(PlanetName planet, DateTimeOffset birth, SunTimes sun)
    {
        if (planet == PlanetName.Mercury) return 60;
        var unnata = HoursFromMidnight(birth, sun) * 60.0 / 12.0;
        return planet is PlanetName.Sun or PlanetName.Jupiter or PlanetName.Venus ? unnata : 60 - unnata;
    }

    /// <summary>Hours (0–12) between the birth and local apparent midnight.</summary>
    public static double HoursFromMidnight(DateTimeOffset birth, SunTimes sun)
    {
        double hours;
        if (sun.IsNightBirth)
        {
            var midnight = sun.Sunset + (sun.NextSunrise - sun.Sunset) / 2;
            hours = Math.Abs((birth - midnight).TotalHours);
        }
        else
        {
            var noon = sun.Sunrise + (sun.Sunset - sun.Sunrise) / 2;
            hours = 12 - Math.Abs((birth - noon).TotalHours);
        }
        return Math.Clamp(hours, 0, 12);
    }
}
