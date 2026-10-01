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
    ];

    private static readonly Dictionary<PlanetName, ZodiacName> Transits = new()
    {
        [PlanetName.Mercury] = ZodiacName.Gemini, // 4th: auspicious, Vedha 3rd (Taurus)
        [PlanetName.Mars] = ZodiacName.Taurus,    // in Mercury's Vedha house
        [PlanetName.Jupiter] = ZodiacName.Cancer, // 5th: auspicious, Vedha 4th (Gemini) holds Mercury
        [PlanetName.Sun] = ZodiacName.Aries,      // 2nd: not listed for the Sun
        [PlanetName.Saturn] = ZodiacName.Pisces,  // over the Moon
        [PlanetName.Rahu] = ZodiacName.Virgo,
    };

    private static GocharaReadingResult Read() =>
        GocharaReading.Read(ZodiacName.Pisces, Transits, Rules, (p, s) => p == PlanetName.Mercury ? 5 : 2);

    private static GocharaRow Row(PlanetName p) => Read().Rows.Single(r => r.Planet == p);

    [Fact]
    public void Auspicious_house_blocked_by_a_planet_in_its_vedha_house()
    {
        Assert.Equal(4, Row(PlanetName.Mercury).HouseFromMoon);
        Assert.Equal(GocharaVerdict.Blocked, Row(PlanetName.Mercury).Verdict);
        Assert.Equal([PlanetName.Mars], Row(PlanetName.Mercury).Vedha!.Obstructors);
        Assert.Equal(GocharaVerdict.Blocked, Row(PlanetName.Jupiter).Verdict);
    }

    [Fact]
    public void Unlisted_house_is_not_favourable_and_nodes_are_not_tabled()
    {
        Assert.Equal(GocharaVerdict.NotFavourable, Row(PlanetName.Sun).Verdict);
        Assert.Equal(GocharaVerdict.NotTabled, Row(PlanetName.Rahu).Verdict);
        Assert.Null(Row(PlanetName.Rahu).Bindus);
        Assert.Null(Row(PlanetName.Rahu).Vedha);
    }

    [Fact]
    public void Bindus_are_reported_beside_the_verdict()
    {
        Assert.Equal(5, Row(PlanetName.Mercury).Bindus);
        Assert.Equal(2, Row(PlanetName.Sun).Bindus);
    }

    [Fact]
    public void Rows_follow_planet_order_and_skip_planets_without_a_transit() =>
        Assert.Equal(
            [PlanetName.Sun, PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Saturn, PlanetName.Rahu],
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
