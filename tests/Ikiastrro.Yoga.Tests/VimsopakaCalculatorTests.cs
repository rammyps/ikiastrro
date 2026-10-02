using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

public sealed class VimsopakaCalculatorTests
{
    [Fact]
    public void Own_sign_in_every_required_chart_scores_twenty()
    {
        var charts = new[] { Chart("D1"), Chart("D9") };
        var weights = new[] { new VimsopakaWeight("TEST", "D1", 10m), new VimsopakaWeight("TEST", "D9", 10m) };

        var sun = VimsopakaCalculator.Calculate(charts, weights).Single(r => r.Planet == "Sun");

        Assert.Equal(20m, sun.Score);
        Assert.Equal(20m, sun.Max);
    }

    [Fact]
    public void Rejects_a_scheme_that_does_not_total_twenty()
    {
        var error = Assert.Throws<ArgumentException>(() =>
            VimsopakaCalculator.Calculate([Chart("D1")], [new VimsopakaWeight("BAD", "D1", 19m)]));

        Assert.Contains("expected 20", error.Message);
    }

    private static ChartAnalysisInput Chart(string chartType) => new(chartType, ZodiacName.Aries,
    [
        P(PlanetName.Sun, ZodiacName.Leo), P(PlanetName.Moon, ZodiacName.Cancer),
        P(PlanetName.Mars, ZodiacName.Aries), P(PlanetName.Mercury, ZodiacName.Gemini),
        P(PlanetName.Jupiter, ZodiacName.Sagittarius), P(PlanetName.Venus, ZodiacName.Taurus),
        P(PlanetName.Saturn, ZodiacName.Capricornus)
    ]);

    private static PlanetPosition P(PlanetName planet, ZodiacName sign) => new()
    {
        Planet = planet.ToString(), Sign = sign.ToString(), PointKind = "Graha",
        VargaLongitudeDegrees = 15d
    };
}
