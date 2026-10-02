using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Transits;

namespace Ikiastrro.Yoga.Tests;

/// <summary>Transits judged from a natal Moon in Pisces against PVR ch.26.3 rows.</summary>
public sealed class GocharaReadingTests
{
    private static readonly GocharaVedhaRule[] Rules =
    [
        new(PlanetName.Mercury, 4, 3, PlanetName.Moon),
        new(PlanetName.Jupiter, 5, 4),
        new(PlanetName.Sun, 3, 9, PlanetName.Saturn),
        new(PlanetName.Saturn, 11, 5, PlanetName.Sun),
        new(PlanetName.Mars, 6, 9),
    ];

    private static readonly Dictionary<PlanetName, ZodiacName> Transits = new()
    {
        [PlanetName.Mercury] = ZodiacName.Gemini, // 4th: auspicious, Vedha 3rd (Taurus)
        [PlanetName.Mars] = ZodiacName.Taurus,    // in Mercury's Vedha house
        [PlanetName.Jupiter] = ZodiacName.Cancer, // 5th: auspicious, Vedha 4th (Gemini) holds Mercury
        [PlanetName.Sun] = ZodiacName.Aries,      // 2nd: not listed for the Sun
        [PlanetName.Saturn] = ZodiacName.Pisces,  // over the Moon
        [PlanetName.Rahu] = ZodiacName.Capricornus, // 11th: Saturn's row, Vedha 5th (Cancer) holds Jupiter
        [PlanetName.Ketu] = ZodiacName.Cancer,      // 5th: not listed for Mars
    };

    private static GocharaReadingResult Read() =>
        GocharaReading.Read(ZodiacName.Pisces, Transits, Rules, (p, s) => p == PlanetName.Mercury ? 5 : 2);

    private static GocharaRow Row(PlanetName p) => Read().Rows.Single(r => r.Planet == p);

    [Fact]
    public void Auspicious_house_blocked_by_a_planet_in_its_vedha_house()
    {
        Assert.Equal(4, Row(PlanetName.Mercury).HouseFromMoon);
        Assert.Equal(GocharaVerdict.Blocked, Row(PlanetName.Mercury).Verdict);
        Assert.Equal([PlanetName.Mars], Row(PlanetName.Mercury).Vedha.Obstructors);
        Assert.Equal(GocharaVerdict.Blocked, Row(PlanetName.Jupiter).Verdict);
    }

    [Fact]
    public void Unlisted_house_is_not_favourable() =>
        Assert.Equal(GocharaVerdict.NotFavourable, Row(PlanetName.Sun).Verdict);

    [Fact]
    public void Rahu_is_read_by_saturns_row_and_ketu_by_mars_row()
    {
        var rahu = Row(PlanetName.Rahu);
        Assert.Equal(PlanetName.Saturn, rahu.ReadAs);
        Assert.Equal(11, rahu.HouseFromMoon);
        Assert.Equal(GocharaVerdict.Blocked, rahu.Verdict);
        Assert.Equal(5, rahu.Vedha.VedhaHouse);
        Assert.Equal([PlanetName.Jupiter], rahu.Vedha.Obstructors); // Ketu also in the 5th, but nodes never block each other
        Assert.Null(rahu.Bindus);

        var ketu = Row(PlanetName.Ketu);
        Assert.Equal(PlanetName.Mars, ketu.ReadAs);
        Assert.Equal(GocharaVerdict.NotFavourable, ketu.Verdict);
        Assert.Null(ketu.Bindus);
        Assert.Null(Row(PlanetName.Saturn).ReadAs);
    }

    [Fact]
    public void Node_does_not_inherit_the_sun_saturn_exception()
    {
        var transits = new Dictionary<PlanetName, ZodiacName>
        {
            [PlanetName.Rahu] = ZodiacName.Capricornus, // 11th from Pisces Moon
            [PlanetName.Sun] = ZodiacName.Cancer,       // its Vedha house
        };
        var rahu = GocharaReading.Read(ZodiacName.Pisces, transits, Rules).Rows.Single(r => r.Planet == PlanetName.Rahu);
        Assert.Equal([PlanetName.Sun], rahu.Vedha.Obstructors);
    }

    [Fact]
    public void Bindus_are_reported_beside_the_verdict()
    {
        Assert.Equal(5, Row(PlanetName.Mercury).Bindus);
        Assert.Equal(2, Row(PlanetName.Sun).Bindus);
    }

    [Fact]
    public void Rows_run_slowest_first_and_skip_planets_without_a_transit() =>
        Assert.Equal(
            [PlanetName.Saturn, PlanetName.Jupiter, PlanetName.Rahu, PlanetName.Ketu, PlanetName.Mars, PlanetName.Mercury, PlanetName.Sun],
            Read().Rows.Select(r => r.Planet));

    [Theory]
    [InlineData(12, SaturnPhase.SadeSatiRising)]
    [InlineData(1, SaturnPhase.SadeSatiPeak)]
    [InlineData(2, SaturnPhase.SadeSatiSetting)]
    [InlineData(4, SaturnPhase.Kantaka)]
    [InlineData(8, SaturnPhase.Ashtama)]
    [InlineData(3, SaturnPhase.None)]
    public void Saturn_phase_from_the_moon(int house, SaturnPhase expected) =>
        Assert.Equal(expected, GocharaReading.PhaseOf(house));

    [Fact]
    public void Saturn_over_the_moon_is_sade_sati_peak() =>
        Assert.Equal(SaturnPhase.SadeSatiPeak, Read().Saturn);
}
