using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Relationships;

namespace Ikiastrro.Yoga.Tests;

public sealed class GrahaDrishtiStrengthCalculatorTests
{
    [Fact]
    public void Mars_fourth_example_preserves_ordinary_and_special_breakdown()
    {
        var result = GrahaDrishtiStrengthCalculator.Calculate(PlanetName.Mars, 3.286, 90.0);

        Assert.Equal(86.714, result.DirectedSeparationDegrees, 6);
        Assert.Equal(41.714, result.OrdinaryVirupas, 6);
        Assert.Equal(13.357, result.SpecialVirupas, 6);
        Assert.Equal(55.071, result.TotalVirupas, 6);
        Assert.Equal(91.79, result.Percentage);
        Assert.Equal(4, result.DiscreteAspectHouse);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(30, 0)]
    [InlineData(60, 15)]
    [InlineData(90, 45)]
    [InlineData(120, 30)]
    [InlineData(150, 0)]
    [InlineData(180, 60)]
    [InlineData(300, 0)]
    public void Ordinary_piecewise_boundaries_are_stable(double separation, double expected)
    {
        var result = GrahaDrishtiStrengthCalculator.Calculate(PlanetName.Sun, 0, separation);

        Assert.Equal(expected, result.OrdinaryVirupas, 10);
    }

    [Theory]
    [InlineData(PlanetName.Mars, 90, 15)]
    [InlineData(PlanetName.Mars, 210, 15)]
    [InlineData(PlanetName.Jupiter, 120, 30)]
    [InlineData(PlanetName.Jupiter, 240, 30)]
    [InlineData(PlanetName.Saturn, 60, 45)]
    [InlineData(PlanetName.Saturn, 270, 45)]
    public void Special_aspect_contributions_are_separate_and_total_is_capped(
        PlanetName planet, double separation, double expectedSpecial)
    {
        var result = GrahaDrishtiStrengthCalculator.Calculate(planet, 0, separation);

        Assert.Equal(expectedSpecial, result.SpecialVirupas, 10);
        Assert.InRange(result.TotalVirupas, 0, 60);
        Assert.InRange(result.Percentage, 0, 100);
    }

    [Theory]
    [InlineData(PlanetName.Rahu)]
    [InlineData(PlanetName.Ketu)]
    public void Nodes_have_partial_trinal_strength_but_no_discrete_special_aspect(PlanetName node)
    {
        var result = GrahaDrishtiStrengthCalculator.Calculate(node, 0, 120);

        Assert.Equal(50.0, result.Percentage);
        Assert.False(result.IsDiscreteAspect);
        Assert.Null(result.DiscreteAspectHouse);
    }

    [Theory]
    [InlineData(PlanetName.Rahu)]
    [InlineData(PlanetName.Ketu)]
    [InlineData(PlanetName.Sun)]
    public void Seventh_is_a_discrete_aspect_for_every_graha(PlanetName planet)
    {
        var result = GrahaDrishtiStrengthCalculator.Calculate(planet, 5, 185);

        Assert.Equal(7, result.DiscreteAspectHouse);
        Assert.Equal(100.0, result.Percentage);
    }

    [Fact]
    public void Rejects_longitudes_outside_normalized_contract()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GrahaDrishtiStrengthCalculator.Calculate(PlanetName.Mars, -0.001, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GrahaDrishtiStrengthCalculator.Calculate(PlanetName.Mars, 0, 360));
    }
}
