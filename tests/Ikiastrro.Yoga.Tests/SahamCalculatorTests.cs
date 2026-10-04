using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Karakas;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR ch.28.8 Sahams — the A − B + C arithmetic checked on PVR's own worked examples (Example
/// 121: vanik and samartha), then Vivaha and Kali by day and night.</summary>
public sealed class SahamCalculatorTests
{
    private static double Dms(int deg, int min) => deg + min / 60.0;

    [Fact]
    public void Vanik_example_needs_no_arc_correction()
    {
        // Night chart: Mercury − Moon + Lagna = 311°28' − 345°14' + 280°50' = 247°04'.
        var lon = SahamCalculator.Evaluate(Dms(311, 28), Dms(345, 14), Dms(280, 50));
        Assert.Equal(Dms(247, 4), lon, 3);
    }

    [Fact]
    public void Samartha_example_adds_30_degrees_when_Lagna_is_not_on_the_arc()
    {
        // Mars − Lagna lord + Lagna = 19°10' − 354°58' + 280°50' (+360), then +30° = 335°02'.
        var lon = SahamCalculator.Evaluate(Dms(19, 10), Dms(354, 58), Dms(280, 50));
        Assert.Equal(Dms(335, 2), lon, 3);
    }

    [Fact]
    public void Punya_still_matches_the_general_engine()
    {
        var seed = PunyaSahamCalculator.Compute(sunLongitude: 100, moonLongitude: 200, natalLagnaLongitude: 150, isNightBirth: false);
        Assert.Equal(SahamCalculator.Evaluate(200, 100, 150), seed.NirayanaLongitudeDegrees, 6);
        var night = PunyaSahamCalculator.Compute(100, 200, 150, isNightBirth: true);
        Assert.Equal(SahamCalculator.Evaluate(100, 200, 150), night.NirayanaLongitudeDegrees, 6);
    }

    [Fact]
    public void Vivaha_is_Venus_minus_Saturn_plus_Lagna_by_day_and_reversed_by_night()
    {
        // Venus 100°, Saturn 40°, Lagna 70° lies on the arc 40°→100°, so no correction: 100 − 40 + 70 = 130°.
        var day = SahamCalculator.Vivaha(venus: 100, saturn: 40, lagna: 70, isNightBirth: false);
        Assert.Equal(130, day.LongitudeDegrees, 6);
        Assert.Equal(ZodiacName.Leo, day.Sign);
        // Night: Saturn − Venus + Lagna. Going forward from Venus (100°) to Saturn (40°), Lagna (70°) is not
        // reached, so PVR adds 30°; the result is whatever Evaluate gives.
        var night = SahamCalculator.Vivaha(100, 40, 70, isNightBirth: true);
        Assert.Equal(SahamCalculator.Evaluate(40, 100, 70), night.LongitudeDegrees, 6);
        Assert.Equal("VIVAHA", night.Code);
    }

    [Fact]
    public void Kali_is_Jupiter_minus_Mars_plus_Lagna_by_day_and_reversed_by_night()
    {
        var day = SahamCalculator.Kali(jupiter: 200, mars: 20, lagna: 90, isNightBirth: false);
        Assert.Equal(SahamCalculator.Evaluate(200, 20, 90), day.LongitudeDegrees, 6);
        var night = SahamCalculator.Kali(200, 20, 90, isNightBirth: true);
        Assert.Equal(SahamCalculator.Evaluate(20, 200, 90), night.LongitudeDegrees, 6);
        Assert.Equal("Great misfortune", night.Meaning);
    }

    [Fact]
    public void Longitudes_stay_in_0_to_360()
    {
        foreach (var (a, b, c) in new[] { (10.0, 350.0, 5.0), (359.9, 0.1, 359.0), (0.0, 0.0, 0.0), (180.0, 90.0, 270.0) })
        {
            var lon = SahamCalculator.Evaluate(a, b, c);
            Assert.InRange(lon, 0, 360);
            Assert.NotEqual(360, lon);
        }
    }

    [Fact]
    public void Transit_trigger_sahams_are_Vivaha_and_Kali_from_the_D1_longitudes()
    {
        var longitudes = new Dictionary<PlanetName, double>
        {
            [PlanetName.Venus] = 100, [PlanetName.Saturn] = 40, [PlanetName.Jupiter] = 200, [PlanetName.Mars] = 20,
        };
        var sahams = SahamCalculator.TransitTriggerSahams(longitudes, lagna: 70, isNightBirth: false);
        Assert.Equal(["VIVAHA", "KALI"], sahams.Select(s => s.Code));
        Assert.Equal(130, sahams[0].LongitudeDegrees, 6);
    }
}
