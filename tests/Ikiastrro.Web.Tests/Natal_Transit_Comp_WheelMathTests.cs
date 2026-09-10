using Ikiastrro.Web;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>
/// Pure display logic for the Transit page
/// (<c>docs/ui/components/spec_Natal_Transit_Comp_Wheel.md</c>) —
/// row ordering, the Motion cell, and the "House from D1" sign count. The two-tab table render
/// itself still needs a page-DI bUnit harness (tracked on the acceptance matrix).
/// </summary>
public class Natal_Transit_Comp_WheelMathTests
{
    [Theory]
    [InlineData("Lagna", -1)]
    [InlineData("Ascendant", -1)]
    [InlineData("Saturn", 0)]
    [InlineData("Jupiter", 1)]
    [InlineData("Rahu", 2)]
    [InlineData("Ketu", 3)]
    [InlineData("Sun", 8)]
    public void PlanetRank_follows_the_slow_to_fast_order(string planet, int expected)
        => Assert.Equal(expected, Natal_Transit_Comp_WheelMath.PlanetRank(planet));

    [Fact]
    public void PlanetRank_sorts_a_mixed_row_set_Lagna_first_then_slow_to_fast()
    {
        var input = new[] { "Sun", "Saturn", "Lagna", "Moon", "Rahu" };
        var ordered = input.OrderBy(Natal_Transit_Comp_WheelMath.PlanetRank).ToArray();
        Assert.Equal(new[] { "Lagna", "Saturn", "Rahu", "Moon", "Sun" }, ordered);
    }

    [Fact]
    public void PlanetRank_puts_unknown_names_last() =>
        Assert.True(Natal_Transit_Comp_WheelMath.PlanetRank("Gulika") > Natal_Transit_Comp_WheelMath.PlanetRank("Sun"));

    [Theory]
    [InlineData(false, false, "Direct")]
    [InlineData(true, false, "Retro")]
    [InlineData(false, true, "Direct · combust")]
    [InlineData(true, true, "Retro · combust")]
    public void FormatMotion_combines_retrograde_and_combust(bool retro, bool combust, string expected)
        => Assert.Equal(expected, Natal_Transit_Comp_WheelMath.FormatMotion(retro, combust));

    [Theory]
    [InlineData("Aries", "Aries", 1)]   // same sign = 1
    [InlineData("Aries", "Cancer", 4)]
    [InlineData("Capricorn", "Aries", 4)] // display spelling accepted
    [InlineData("Capricornus", "Aries", 4)]
    [InlineData("Pisces", "Aries", 2)]  // wraps the zodiac
    public void HouseFromD1_counts_signs_natal_to_transit(string natal, string transit, int expected)
        => Assert.Equal(expected, Natal_Transit_Comp_WheelMath.HouseFromD1(natal, transit));

    [Theory]
    [InlineData(null, "Aries")]
    [InlineData("Aries", "")]
    [InlineData("NotASign", "Aries")]
    public void HouseFromD1_is_null_when_a_sign_is_unrecognised(string? natal, string transit)
        => Assert.Null(Natal_Transit_Comp_WheelMath.HouseFromD1(natal, transit));
}
