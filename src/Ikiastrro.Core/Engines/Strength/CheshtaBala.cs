using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// Cheṣṭā Bala (motional strength), in virūpas.
/// <list type="bullet">
/// <item>Mars–Saturn (SRC_RAMAN_GRAHA_BHAVA_BALAS): Cheṣṭā kendra = Śīghrocca − ½(true + mean
/// longitude), folded to 0–180°, divided by 3. The Śīghrocca of Mars, Jupiter and Saturn is the
/// Sun's mean longitude; for Mercury and Venus the planet's own mean longitude is the Śīghrocca
/// and the Sun's mean longitude stands in as the mean. Mean longitudes come from Raman's
/// Ujjain 1900-01-01 epoch positions, daily motions and per-year corrections, as transcribed in
/// PyJHora (<c>_research/PyJHora/.../strength.py</c>, <c>get_planet_mean_longitude</c>).</item>
/// <item>Moon: her Pakṣa Bala — elongation from the Sun (0–180°) / 3.</item>
/// <item>Sun: his undoubled Ayana Bala (<see cref="AyanaBala.Undoubled"/>; BPHS: the Sun's Cheṣṭā
/// is his Ayana Bala). JHora shows a different figure for the Sun (40.40 vs 45.15 for 1_Ramakrishnan) whose
/// method is not documented; BPHS is followed here.</item>
/// </list>
/// Checked against JHora for 1_Ramakrishnan: the five tara grahas within 0.2 virūpas, the Moon exact.
/// </summary>
public static class CheshtaBala
{
    // Ujjain mean midnight, 1900-01-01: 00:00 local mean time at 76° E = 18:56 UT on 1899-12-31.
    private static readonly DateTimeOffset Epoch = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero).AddHours(-76.0 / 15.0);

    private sealed record MeanMotion(double EpochLongitude, double DegreesPerDay, int Sign, double Offset, double PerYear);

    private static readonly IReadOnlyDictionary<PlanetName, MeanMotion> Motions = new Dictionary<PlanetName, MeanMotion>
    {
        [PlanetName.Sun] = new(257.4568, 0.9856, 1, 0, 0),
        [PlanetName.Mars] = new(270.22, 0.524, 1, 0, 0),
        [PlanetName.Mercury] = new(164, 4.0923, 1, 6.67, -0.00133),
        [PlanetName.Jupiter] = new(220.04, 0.0831, -1, 3.3, 0.0067),
        [PlanetName.Venus] = new(328.51, 1.60215, -1, 5, 0.0001),
        [PlanetName.Saturn] = new(236.74, 0.033439, 1, 5, 0.001),
    };

    /// <summary>Raman's mean sidereal longitude of a planet at <paramref name="birth"/>.</summary>
    public static double MeanLongitude(PlanetName planet, DateTimeOffset birth)
    {
        var m = Motions[planet];
        var days = (birth - Epoch).TotalDays;
        var correction = m.Sign * (m.Offset + m.PerYear * (birth.Year - 1900));
        return Normalize(m.EpochLongitude + days * m.DegreesPerDay + correction);
    }

    /// <summary>Cheṣṭā Bala of Mars, Mercury, Jupiter, Venus or Saturn from its true sidereal longitude.</summary>
    public static double TaraGraha(PlanetName planet, double trueLongitude, DateTimeOffset birth)
    {
        var sunMean = MeanLongitude(PlanetName.Sun, birth);
        var mean = MeanLongitude(planet, birth);
        var seeghrocha = sunMean;
        if (planet is PlanetName.Mercury or PlanetName.Venus) (seeghrocha, mean) = (mean, sunMean);
        var kendra = Math.Abs(Normalize(seeghrocha - 0.5 * (trueLongitude + mean)));
        if (kendra > 180) kendra = 360 - kendra;
        return kendra / 3.0;
    }

    /// <summary>Moon's Cheṣṭā = her Pakṣa Bala: elongation from the Sun folded to 0–180°, / 3.</summary>
    public static double Moon(double sunLongitude, double moonLongitude)
    {
        var d = Math.Abs(Normalize(moonLongitude - sunLongitude));
        return (d > 180 ? 360 - d : d) / 3.0;
    }

    private static double Normalize(double degrees) => ((degrees % 360) + 360) % 360;
}
