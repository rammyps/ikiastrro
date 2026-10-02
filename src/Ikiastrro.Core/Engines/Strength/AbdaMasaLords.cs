using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// Abda (Varṣa) and Māsa lords for Kāla Bala (SRC_RAMAN_GRAHA_BHAVA_BALAS): the weekday lord of the
/// first day of the 360-day year (Abda, 15 virūpas) and of the 30-day month (Māsa, 30 virūpas) in which
/// the birth falls, counted on Raman's ahargana. The day count follows Raman's Table I as PyJHora
/// transcribes it (<c>_days_elapsed_since_base</c>: 174 days at the end of 1951); a 360-day year
/// advances the weekday by 3 and a 30-day month by 2, from the Tuesday-first weekday cycle.
/// Both match JHora for 1_Ramakrishnan (Abda Mars, Māsa Saturn).
/// </summary>
public static class AbdaMasaLords
{
    private static readonly DateOnly Base = new(1951, 12, 31);
    private const int BaseDays = 174;

    // Tuesday-first, as Raman's table counts it.
    private static readonly PlanetName[] WeekdayLords =
    {
        PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus,
        PlanetName.Saturn, PlanetName.Sun, PlanetName.Moon,
    };

    /// <summary>Raman's ahargana for a civil date.</summary>
    public static int Ahargana(DateOnly date) => BaseDays + (date.DayNumber - Base.DayNumber);

    public static PlanetName AbdaLord(DateOnly date) => Lord(Ahargana(date), 360, 3);

    public static PlanetName MasaLord(DateOnly date) => Lord(Ahargana(date), 30, 2);

    private static PlanetName Lord(int ahargana, int periodDays, int weekdayStep)
    {
        var periods = (int)Math.Floor(ahargana / (double)periodDays);
        var index = ((periods * weekdayStep + 1) % 7 + 7) % 7;
        return WeekdayLords[index];
    }
}
