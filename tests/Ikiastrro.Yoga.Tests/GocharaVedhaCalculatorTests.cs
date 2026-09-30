using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Transits;

namespace Ikiastrro.Yoga.Tests;

public sealed class GocharaVedhaCalculatorTests
{
    [Fact]
    public void Bill_Gates_shape_Mercury_in_fourth_is_blocked_by_planets_in_third()
    {
        var result = GocharaVedhaCalculator.Evaluate(
            PlanetName.Mercury, ZodiacName.Pisces,
            new Dictionary<PlanetName, ZodiacName>
            {
                [PlanetName.Mercury] = ZodiacName.Gemini,
                [PlanetName.Mars] = ZodiacName.Taurus
            },
            [new(PlanetName.Mercury, 4, 3, PlanetName.Moon)]);

        Assert.True(result.IsAuspicious);
        Assert.True(result.IsObstructed);
        Assert.Equal([PlanetName.Mars], result.Obstructors);
    }

    [Fact]
    public void Moon_does_not_obstruct_Mercury()
    {
        var result = GocharaVedhaCalculator.Evaluate(
            PlanetName.Mercury, ZodiacName.Pisces,
            new Dictionary<PlanetName, ZodiacName>
            {
                [PlanetName.Mercury] = ZodiacName.Gemini,
                [PlanetName.Moon] = ZodiacName.Taurus
            },
            [new(PlanetName.Mercury, 4, 3, PlanetName.Moon)]);

        Assert.True(result.CanDeliverAuspiciousResult);
    }

    [Fact]
    public void Unlisted_house_is_not_an_auspicious_transit()
    {
        var result = GocharaVedhaCalculator.Evaluate(
            PlanetName.Mars, ZodiacName.Aries,
            new Dictionary<PlanetName, ZodiacName> { [PlanetName.Mars] = ZodiacName.Taurus },
            [new(PlanetName.Mars, 3, 12)]);

        Assert.False(result.IsAuspicious);
        Assert.Null(result.VedhaHouse);
    }
}
