using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// Ayana Bala (BPHS, Kāla Bala): (24° ± declination) × 60/48 virūpas. North declination adds for the
/// Sun, Mars, Jupiter and Venus; south declination adds for the Moon and Saturn; Mercury's always
/// adds. The Sun's is doubled inside Kāla Bala; <see cref="Undoubled"/> is also the Sun's Cheṣṭā Bala.
/// Declination is the kranti of the planet's ecliptic longitude (tropical longitude and the mean
/// obliquity, latitude ignored) — with latitude the Moon drifts ~6 virūpas from JHora for
/// 1_Ramakrishnan; without it all seven agree within 0.5 (`verify-strength`).
/// </summary>
public static class AyanaBala
{
    /// <summary>Ayana Bala before the Sun's doubling.</summary>
    public static double Undoubled(PlanetName planet, double siderealLongitude, double ayanamshaDegrees,
        DateTimeOffset birth)
    {
        var declination = Declination(siderealLongitude + ayanamshaDegrees, birth);
        var signed = planet switch
        {
            PlanetName.Moon or PlanetName.Saturn => -declination,
            PlanetName.Mercury => Math.Abs(declination),
            _ => declination,
        };
        return (24 + signed) * 60.0 / 48.0;
    }

    /// <summary>Ayana Bala as Kāla Bala counts it — the Sun's doubled.</summary>
    public static double ForKalaBala(PlanetName planet, double siderealLongitude, double ayanamshaDegrees,
        DateTimeOffset birth)
    {
        var value = Undoubled(planet, siderealLongitude, ayanamshaDegrees, birth);
        return planet == PlanetName.Sun ? 2 * value : value;
    }

    /// <summary>Declination (degrees, north positive) of the ecliptic point at tropical longitude
    /// <paramref name="tropicalLongitude"/>.</summary>
    public static double Declination(double tropicalLongitude, DateTimeOffset birth)
    {
        var centuries = (birth.UtcDateTime - new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc)).TotalDays / 36525.0;
        var obliquity = Rad(23.4392911 - 0.0130042 * centuries);
        return Math.Asin(Math.Sin(obliquity) * Math.Sin(Rad(tropicalLongitude))) * 180 / Math.PI;
    }

    private static double Rad(double degrees) => degrees * Math.PI / 180;
}
