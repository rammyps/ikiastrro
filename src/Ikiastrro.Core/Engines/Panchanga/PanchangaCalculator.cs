using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Panchanga;

/// <summary>
/// Tithi, Karana, Nitya Yoga, Vedic Weekday, and Hora Lord for one birth moment — pure and
/// deterministic over the D1 Sun/Moon sidereal longitudes and <see cref="SunTimes"/>.
///
/// Every formula is transcribed from SRC_PVR_INTEGRATED §1.3.8-1.3.12 (the book's only
/// panchanga chapter) — see db/081_create_panchanga_schema.sql's <c>tbl_Rule_PanchangaFormula</c>
/// for the same narrative, and the migration header for the JHora cross-check
/// (Krishna Tritiya / Vyatipaata / Tuesday / Hora Lord Venus / Janma Ghatis 58.8892, all
/// reproduced exactly for 1_Ramakrishnan). Karana lord, Nitya Yoga lord, Samvatsara, lunar
/// month, Mahakala Hora, and Kaala Lord are deliberately not computed — no PVR ch.1 source.
/// </summary>
public static class PanchangaCalculator
{
    /// <summary>Decreasing geocentric speed — the 24-hora cycle order. Mirrors
    /// <c>tbl_Dim_HoraSequence</c> (migration 081, SRC_PVR_INTEGRATED §1.3.11).</summary>
    private static readonly PlanetName[] HoraSequence =
    {
        PlanetName.Saturn, PlanetName.Jupiter, PlanetName.Mars, PlanetName.Sun,
        PlanetName.Venus, PlanetName.Mercury, PlanetName.Moon,
    };

    private const double NakshatraSpanDegrees = 800.0 / 60.0; // 13°20'

    public static PanchangaResult Calculate(BirthDetails birth, SiderealPositions positions, SunTimes sunTimes)
    {
        var moment = BirthMomentFactory.Create(birth);
        var sun = positions.PlanetLongitudes[PlanetName.Sun];
        var moon = positions.PlanetLongitudes[PlanetName.Moon];

        // Tithi — §1.3.8.1: floor((Moon-Sun)/12) + 1, 1-30. tbl_Dim_Tithi.Id is seeded 1-30 in
        // this same order, so the index IS the FK.
        var delta = AstroMath.Normalize(moon - sun);
        var tithiId = (int)(delta / 12.0) + 1;
        var tithiIntoSpan = delta % 12.0;
        var tithiPercentRemaining = 100.0 - tithiIntoSpan / 12.0 * 100.0;

        // Nitya Yoga — §1.3.9: floor((Sun+Moon)/13°20') + 1, 1-27. tbl_Dim_NityaYoga.Id mirrors
        // the index the same way.
        var sum = AstroMath.Normalize(sun + moon);
        var nityaYogaId = (int)(sum / NakshatraSpanDegrees) + 1;
        var yogaIntoSpan = sum % NakshatraSpanDegrees;
        var nityaYogaPercentRemaining = 100.0 - yogaIntoSpan / NakshatraSpanDegrees * 100.0;

        // Karana — §1.3.10: each tithi splits into 2 half-tithis. The 7 movable karanas
        // (tbl_Dim_Karana.Id 1-7) repeat 8x (56 slots) from the 2nd half of the month's 1st
        // tithi; the 4 fixed karanas (Id 8-11) cover the 2nd half of tithi 29 through the 1st
        // half of the next month's tithi 1.
        var isSecondHalf = tithiIntoSpan >= 6.0;
        var halfTithiIntoSpan = isSecondHalf ? tithiIntoSpan - 6.0 : tithiIntoSpan;
        var karanaPercentRemaining = 100.0 - halfTithiIntoSpan / 6.0 * 100.0;
        var halfIndex0 = (tithiId - 1) * 2 + (isSecondHalf ? 1 : 0);       // 0-59 within the lunar month
        var shifted = ((halfIndex0 - 1) % 60 + 60) % 60;                   // 0 = 2nd half of tithi 1
        var karanaId = shifted < 56 ? shifted % 7 + 1 : 7 + (shifted - 56) + 1;

        // Vedic weekday — the calendar day the Panchanga day's opening sunrise falls on.
        // tbl_Dim_VedicWeekday.Id 1=Sunday..7=Saturday matches .NET's DayOfWeek + 1, and its
        // planetary-lord order (Sun,Moon,Mars,Mercury,Jupiter,Venus,Saturn) matches PlanetName's
        // declaration order exactly, so weekdayId - 1 IS the lord's PlanetName ordinal.
        var vedicWeekdayId = (int)sunTimes.Sunrise.DayOfWeek + 1;
        var weekdayLordPlanet = (PlanetName)(vedicWeekdayId - 1);

        // Hora Lord — §1.3.11: 24 equal horas from sunrise to next sunrise; hora 1 is the
        // weekday lord, then HoraSequence cycles.
        var dayLengthMinutes = (sunTimes.NextSunrise - sunTimes.Sunrise).TotalMinutes;
        var elapsedMinutes = (moment - sunTimes.Sunrise).TotalMinutes;
        var horaNumber = Math.Min(24, (int)(elapsedMinutes / (dayLengthMinutes / 24.0)) + 1);
        var startIndex = Array.IndexOf(HoraSequence, weekdayLordPlanet);
        var horaLordPlanet = HoraSequence[(startIndex + horaNumber - 1) % 7];

        // Janma Ghatis — minutes elapsed since sunrise / 24 (1 ghati = 24 minutes).
        var janmaGhatis = elapsedMinutes / 24.0;

        return new PanchangaResult(
            sunTimes.Sunrise, sunTimes.Sunset, sunTimes.NextSunrise, sunTimes.IsNightBirth,
            janmaGhatis, delta,
            tithiId, tithiPercentRemaining,
            karanaId, karanaPercentRemaining,
            sum, nityaYogaId, nityaYogaPercentRemaining,
            vedicWeekdayId, AstroIds.PlanetId(horaLordPlanet));
    }
}
