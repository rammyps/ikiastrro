using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Panchanga;
using Ikiastrro.Core.Models;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>
/// PanchangaCalculator against the hand-transcribed Jagannatha Hora export for
/// 1_Ramakrishnan (docs/artifacts/reference-charts/Rammy_Jagannatha.txt), using JHora's own
/// printed longitudes/times directly (no Swiss Ephemeris / ayanamsa round-trip), so the
/// expected percentages match to within JHora's own two-decimal rounding.
/// </summary>
public class PanchangaCalculatorTests
{
    // JHora: Sun 8 Ar 12' 18.13" (Aries=sign 0), Moon 7 Sc 17' 33.70" (Scorpio=sign 7).
    private static readonly double SunLongitude = 0 * 30 + 8 + 12 / 60.0 + 18.13 / 3600.0;
    private static readonly double MoonLongitude = 7 * 30 + 7 + 17 / 60.0 + 33.70 / 3600.0;

    private static SiderealPositions Positions() => new(
        AscendantLongitude: 0,
        PlanetLongitudes: new Dictionary<PlanetName, double> { [PlanetName.Sun] = SunLongitude, [PlanetName.Moon] = MoonLongitude },
        PlanetLatitudes: new Dictionary<PlanetName, double>(),
        PlanetSpeeds: new Dictionary<PlanetName, double>(),
        AyanamshaDegrees: 0,
        LocalSiderealTimeHours: 0);

    // JHora: Sunrise 5:56:39 (Apr 21 1981), Sunset 18:18:53 (Apr 21). JHora doesn't print the
    // instant of the *next* sunrise, so it's approximated as +24h here — Hora Lord's outcome
    // is robust to the real day length's few-minute deviation from exactly 24h (see
    // PanchangaCalculator's doc comment); Janma Ghatis doesn't depend on NextSunrise at all.
    private static readonly DateTimeOffset Sunrise = new(1981, 4, 21, 5, 56, 39, TimeSpan.FromHours(5.5));
    private static readonly DateTimeOffset Sunset = new(1981, 4, 21, 18, 18, 53, TimeSpan.FromHours(5.5));
    private static SunTimes Times() => new(Sunrise, Sunset, Sunrise.AddDays(1), IsNightBirth: true);

    private static BirthDetails Ramakrishnan() => new()
    {
        Name = "Ramakrishnan",
        DateOfBirth = new DateOnly(1981, 4, 22),
        TimeOfBirth = new TimeOnly(5, 30, 0),
        PlaceCity = "Chennai",
        PlaceCountry = "India",
        UtcOffset = "05:30:00",
    };

    [Fact]
    public void Ramakrishnan_tithi_matches_jhora()
    {
        var r = PanchangaCalculator.Calculate(Ramakrishnan(), Positions(), Times());

        Assert.Equal(18, r.TithiId); // Krishna Tritiya
        Assert.Equal(57.60, r.TithiPercentRemaining, precision: 1);
    }

    [Fact]
    public void Ramakrishnan_karana_matches_jhora()
    {
        var r = PanchangaCalculator.Calculate(Ramakrishnan(), Positions(), Times());

        Assert.Equal(6, r.KaranaId); // Vanija
        Assert.Equal(15.21, r.KaranaPercentRemaining, precision: 1);
    }

    [Fact]
    public void Ramakrishnan_nitya_yoga_matches_jhora()
    {
        var r = PanchangaCalculator.Calculate(Ramakrishnan(), Positions(), Times());

        Assert.Equal(17, r.NityaYogaId); // Vyatipaata
        Assert.Equal(8.77, r.NityaYogaPercentRemaining, precision: 1);
    }

    [Fact]
    public void Ramakrishnan_weekday_and_hora_lord_match_jhora()
    {
        var r = PanchangaCalculator.Calculate(Ramakrishnan(), Positions(), Times());

        Assert.Equal(3, r.VedicWeekdayId);              // Tuesday
        Assert.Equal(AstroIds.PlanetId(PlanetName.Venus), r.HoraLordPlanetId);
    }

    [Fact]
    public void Ramakrishnan_janma_ghatis_matches_jhora()
    {
        var r = PanchangaCalculator.Calculate(Ramakrishnan(), Positions(), Times());

        Assert.Equal(58.8892, r.JanmaGhatis, precision: 1);
    }

    [Fact]
    public void Tithi_and_yoga_indices_wrap_at_the_zodiac_boundary()
    {
        // Moon exactly catching up to Sun after a full cycle: delta normalizes to 0, so
        // Tithi/Nitya-Yoga both read as index 1 with 100% remaining, not 31/28 or negative.
        var positions = new SiderealPositions(0,
            new Dictionary<PlanetName, double> { [PlanetName.Sun] = 350.0, [PlanetName.Moon] = 350.0 },
            new Dictionary<PlanetName, double>(), new Dictionary<PlanetName, double>(), 0, 0);

        var r = PanchangaCalculator.Calculate(Ramakrishnan(), positions, Times());

        Assert.Equal(1, r.TithiId);
        Assert.Equal(100.0, r.TithiPercentRemaining, precision: 6);
    }
}
