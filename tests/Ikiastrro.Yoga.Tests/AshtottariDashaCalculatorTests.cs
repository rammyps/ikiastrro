using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;

namespace Ikiastrro.Yoga.Tests;

public sealed class AshtottariDashaCalculatorTests
{
    [Fact]
    public void Pvr_example_59_Moon_at_Leo_24_has_six_years_of_Moon_remaining()
    {
        var birth = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var periods = AshtottariDashaCalculator.ComputeFromMoonLongitude(birth, 144d, 20);

        Assert.Equal(PlanetName.Moon, periods[0].Lord);
        Assert.Equal(6d, (periods[0].EndDate - birth).TotalDays / 365.2425, 8);
        Assert.Equal(PlanetName.Mars, periods[1].Lord);
        Assert.Equal(8d, (periods[1].EndDate - periods[1].StartDate).TotalDays / 365.2425, 8);
    }

    [Theory]
    [InlineData(10d, PlanetName.Rahu)]
    [InlineData(350d, PlanetName.Rahu)]
    public void Rahu_arc_wraps_across_zero(double longitude, PlanetName expected)
    {
        var periods = AshtottariDashaCalculator.ComputeFromMoonLongitude(DateTimeOffset.UnixEpoch, longitude, 1);
        Assert.Equal(expected, periods[0].Lord);
    }

    [Fact]
    public void Jupiter_antardasas_begin_with_Rahu_and_end_with_Jupiter()
    {
        var periods = AshtottariDashaCalculator.ComputeFromMoonLongitude(DateTimeOffset.UnixEpoch, 293d + 20d / 60d, 1);
        Assert.Equal(PlanetName.Jupiter, periods[0].Lord);
        Assert.Equal(PlanetName.Rahu, periods[0].Children[0].Lord);
        Assert.Equal(PlanetName.Jupiter, periods[0].Children[^1].Lord);
    }
}
