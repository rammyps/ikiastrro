using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>Cheṣṭā Bala against JHora's figures for 1_Ramakrishnan (1981-04-22 05:30 IST),
/// using JHora's own true longitudes (explorer_output/jhora, 2026-09-23).</summary>
public sealed class CheshtaBalaTests
{
    private static readonly DateTimeOffset Birth = new(1981, 4, 22, 5, 30, 0, TimeSpan.FromHours(5.5));

    [Theory]
    [InlineData(PlanetName.Mars, 3.9433, 57.22)]
    [InlineData(PlanetName.Mercury, 1.8326, 4.31)]
    [InlineData(PlanetName.Jupiter, 158.7238, 51.71)]
    [InlineData(PlanetName.Venus, 11.9832, 4.01)]
    [InlineData(PlanetName.Saturn, 160.9595, 50.59)]
    public void TaraGrahaMatchesJhoraWithinAQuarterVirupa(PlanetName planet, double trueLongitude, double jhora) =>
        Assert.InRange(CheshtaBala.TaraGraha(planet, trueLongitude, Birth), jhora - 0.25, jhora + 0.25);

    [Theory]
    [InlineData(8.2019, 217.2088, 50.33)]   // 209° ahead of the Sun folds to 151°
    [InlineData(0, 90, 30)]
    [InlineData(0, 180, 60)]
    public void MoonIsHerPakshaBala(double sun, double moon, double expected) =>
        Assert.Equal(expected, CheshtaBala.Moon(sun, moon), 2);

    [Fact]
    public void AyanaBalaFollowsDeclinationDirection()
    {
        // Tropical 0° Aries: zero declination, (24 + 0) × 60/48 = 30 for everyone.
        Assert.Equal(30, AyanaBala.Undoubled(PlanetName.Sun, 336.0, 24.0, Birth), 3);
        // Tropical 90° is the full obliquity north: the Sun gains, Saturn loses, Mercury always gains.
        Assert.InRange(AyanaBala.Undoubled(PlanetName.Sun, 66.0, 24.0, Birth), 59.2, 59.4);
        Assert.InRange(AyanaBala.Undoubled(PlanetName.Saturn, 66.0, 24.0, Birth), 0.6, 0.8);
        Assert.InRange(AyanaBala.Undoubled(PlanetName.Mercury, 246.0, 24.0, Birth), 59.2, 59.4);
        // Kāla Bala doubles the Sun's only.
        Assert.Equal(2 * AyanaBala.Undoubled(PlanetName.Sun, 66.0, 24.0, Birth),
            AyanaBala.ForKalaBala(PlanetName.Sun, 66.0, 24.0, Birth), 6);
    }

    [Fact]
    public void PakshaBalaMatchesJhoraForRamakrishnan()
    {
        // Moon 209° ahead of the Sun (waning) → e = 151°, e/3 = 50.33. Mercury shares Aries with
        // the Sun and Mars (malefics) and Venus (benefic), so he is a malefic here.
        var longitudes = new Dictionary<PlanetName, double>
        {
            [PlanetName.Sun] = 8.2019, [PlanetName.Moon] = 217.2088, [PlanetName.Mars] = 3.9433,
            [PlanetName.Mercury] = 1.8326, [PlanetName.Jupiter] = 158.7238, [PlanetName.Venus] = 11.9832,
            [PlanetName.Saturn] = 160.9595, [PlanetName.Rahu] = 102.9146, [PlanetName.Ketu] = 282.9146,
        };
        Assert.Equal(9.67, PakshaBala.Compute(PlanetName.Sun, longitudes), 2);
        Assert.Equal(100.66, PakshaBala.Compute(PlanetName.Moon, longitudes), 2);
        Assert.Equal(9.67, PakshaBala.Compute(PlanetName.Mercury, longitudes), 2);
        Assert.Equal(50.33, PakshaBala.Compute(PlanetName.Jupiter, longitudes), 2);
        Assert.Equal(50.33, PakshaBala.Compute(PlanetName.Venus, longitudes), 2);
    }

    [Fact]
    public void MercuryAloneIsABeneficForPakshaBala()
    {
        var longitudes = new Dictionary<PlanetName, double>
        {
            [PlanetName.Sun] = 8.0, [PlanetName.Moon] = 98.0, [PlanetName.Mercury] = 40.0,
        };
        Assert.Equal(30, PakshaBala.Compute(PlanetName.Mercury, longitudes), 6);
    }
}
