using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Presentation;

namespace Ikiastrro.Web.Tests;

/// <summary>Key Inference 1.2 "About Planets" exaltation columns read the shared
/// AstroMath.DeepExaltationPoints (the copy verify-dignity checks against the database), not a
/// private copy of the seven Uchcha Bindu constants.</summary>
public class ChartViewModelExaltationTests
{
    [Theory]
    [InlineData("Sun", "10° Aries")]
    [InlineData("Mars", "28° Capricorn")]
    [InlineData("Venus", "27° Pisces")]
    [InlineData("Saturn", "20° Libra")]
    public void ExaltationPointDisplay_MatchesTheSharedDeepExaltationPoints(string planet, string expected) =>
        Assert.Equal(expected, ChartViewModel.ExaltationPointDisplay(planet));

    [Fact]
    public void ExaltationPointDisplay_CoversExactlyTheSevenClassicalGrahas()
    {
        foreach (var planet in AstroMath.DeepExaltationPoints.Keys)
            Assert.NotNull(ChartViewModel.ExaltationPointDisplay(planet.ToString()));
        Assert.Null(ChartViewModel.ExaltationPointDisplay("Rahu"));
        Assert.Null(ChartViewModel.ExaltationPointDisplay("Ketu"));
    }

    [Fact]
    public void BuildExaltationRows_MeasuresDistanceFromTheSharedPoint()
    {
        // Venus at 357° is exactly exalted; at 177° (Virgo 27°) exactly debilitated.
        var rows = ChartViewModel.BuildExaltationRows(new[]
        {
            new ChartKeyDetail { Planet = "Venus", PointKind = "Graha", NirayanaLongitudeDegrees = 357 },
            new ChartKeyDetail { Planet = "Saturn", PointKind = "Graha", NirayanaLongitudeDegrees = 20 },
            new ChartKeyDetail { Planet = "Rahu", PointKind = "Graha", NirayanaLongitudeDegrees = 100 },
        });

        var venus = rows.Single(r => r.Planet == "Venus");
        Assert.Equal(0, venus.DeltaFromExaltationDegrees);
        Assert.Equal(100, venus.ClosenessPercent);

        var saturn = rows.Single(r => r.Planet == "Saturn");
        Assert.Equal(180, saturn.DeltaFromExaltationDegrees);
        Assert.Equal(0, saturn.ClosenessPercent);

        Assert.Null(rows.Single(r => r.Planet == "Rahu").ClosenessPercent);
    }
}
