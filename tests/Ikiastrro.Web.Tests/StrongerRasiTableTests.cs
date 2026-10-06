using Bunit;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Ikiastrro.Web.Components.Workspace;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>The stronger-rasi card: the Narayana dasa seed with its order, the six rasi/7th axes and the five planets' two
/// rasis, from a hand-built chart.</summary>
public sealed class StrongerRasiTableTests : BunitContext
{
    private static ChartKeyDetail G(string planet, string sign, double lon) => new()
        { PointKind = "Graha", Planet = planet, Sign = sign, NirayanaLongitudeDegrees = lon };

    private static LoadedChart Chart() => new("D1", "Rasi", "Rasi", null, "Taurus", "Scorpio", null,
        [
            G("Ascendant", "Taurus", 40),
            G("Moon", "Scorpio", 220), G("Mars", "Scorpio", 222), G("Sun", "Leo", 130), G("Mercury", "Virgo", 160),
            G("Jupiter", "Gemini", 70), G("Venus", "Libra", 190), G("Saturn", "Capricornus", 280),
            G("Rahu", "Cancer", 100), G("Ketu", "Capricornus", 290),
        ], [], [], [], [], [], null, null, "test");

    [Fact]
    public void ShowsSeedOrderAxesAndPlanetPairs()
    {
        // Lagna Taurus, Scorpio holds the Moon and Mars: the 7th is stronger, so the seed is Scorpio (PVR Example 63).
        var cut = Render<StrongerRasiTable>(p => p.Add(x => x.Chart, Chart()));

        Assert.Contains("Narayana dasa seed", cut.Markup.Normalized());
        Assert.Contains("is Scorpio", cut.Find(".sr-lead").Normalized());
        Assert.Equal(12, cut.FindAll(".sr-chip").Count);
        Assert.Contains("1. Scorpio", cut.FindAll(".sr-chip")[0].Normalized());
        Assert.Contains("2. Gemini", cut.FindAll(".sr-chip")[1].Normalized());   // Sc, Ge, Cp, Le ... backward every 6th
        var tables = cut.FindAll("table");
        Assert.Equal(6, tables[0].QuerySelectorAll("tbody tr").Length);
        Assert.Equal(5, tables[1].QuerySelectorAll("tbody tr").Length);
        Assert.Single(cut.FindAll("tr.sr-lagna"));   // the Taurus-Scorpio axis
    }
}
