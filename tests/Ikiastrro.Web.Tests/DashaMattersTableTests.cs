using Bunit;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Ikiastrro.Web.Components.Workspace;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>The Dasha matters table (PVR sec.16.5.1) on PVR's Chart 23: Aquarius Lagna, so the 8th is Virgo and its lord Mercury.</summary>
public sealed class DashaMattersTableTests : BunitContext
{
    private static ChartKeyDetail G(string planet, string sign, double lon, string? karaka = null) => new()
        { PointKind = "Graha", Planet = planet, Sign = sign, NirayanaLongitudeDegrees = lon, CharaKaraka = karaka };

    private static LoadedChart D1() => new("D1", "Rasi", "Rasi", null, "Aquarius", "Taurus", null,
        [
            G("Ascendant", "Aquarius", 310),
            G("Sun", "Cancer", 100), G("Moon", "Taurus", 31), G("Saturn", "Taurus", 40), G("Mercury", "Leo", 130, "AK"),
            G("Mars", "Leo", 132), G("Venus", "Leo", 134), G("Jupiter", "Scorpio", 220), G("Rahu", "Gemini", 70), G("Ketu", "Sagittarius", 250),
        ], [], [], [], [], [], null, null, "test");

    private static DashaPeriodRecord Maha(string lord, int from, int to) =>
        new() { Lord = lord, StartDate = new DateTime(from, 1, 1), EndDate = new DateTime(to, 1, 1), LevelNumber = 1 };

    [Fact]
    public void ShowsNineRulesWithTheEighthLordAndItsMahadasa()
    {
        var charts = new Dictionary<string, LoadedChart> { ["D1"] = D1() };
        var cut = Render<DashaMattersTable>(p => p.Add(x => x.Charts, charts).Add(x => x.Periods, [Maha("Mercury", 2000, 2017), Maha("Venus", 2017, 2037)]));

        Assert.Equal(9, cut.FindAll("tbody tr").Count);
        var text = cut.Markup.Normalized();
        Assert.Contains("The 8th lord in the rasi chart (D1) can give some troubles and frustration", text);
        Assert.Contains("Mercury — Lord of the 8th (Virgo)", text);
        Assert.Contains("Mercury 01/2000 – 01/2017", text);
        Assert.DoesNotContain("Venus 01/2017", text);                    // Venus meets no rule
        Assert.Contains("Needs the D7 chart.", text);                    // charts that are not there are named, not guessed
    }

    [Fact]
    public void WithoutTheRasiChartItSaysSo()
    {
        var cut = Render<DashaMattersTable>(p => p.Add(x => x.Charts, new Dictionary<string, LoadedChart>()));
        Assert.Contains("Needs the rasi chart.", cut.Markup.Normalized());
    }
}
