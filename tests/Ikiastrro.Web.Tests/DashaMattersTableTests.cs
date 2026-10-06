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

        Assert.Single(cut.FindAll("table"));                                     // one consolidated table
        Assert.True(cut.FindAll("tbody tr").Count > 9);                              // PVR's nine plus the extended rows
        Assert.Contains("PVR §16.5.1 example 2", cut.Markup.Normalized());
        var text = cut.Markup.Normalized();
        Assert.Contains("The 8th lord in the rasi chart (D1) can give some troubles and frustration", text);
        Assert.Contains("Mercury — Lord of the 8th (Virgo)", text);
        Assert.Contains("Mercury 01/2000 – 01/2017", text);
        Assert.DoesNotContain("Venus 01/2017", cut.FindAll("tbody tr").Take(9).Aggregate("", (a, r) => a + r.TextContent));   // Venus meets no PVR example
        Assert.Contains("Needs the D7 chart.", text);                    // charts that are not there are named, not guessed
    }

    [Fact]
    public void WithoutTheRasiChartItSaysSo()
    {
        var cut = Render<DashaMattersTable>(p => p.Add(x => x.Charts, new Dictionary<string, LoadedChart>()));
        Assert.Contains("Needs the rasi chart.", cut.Markup.Normalized());
    }

    [Fact]
    public void PlanetAndPeriodFiltersNarrowTheRows()
    {
        var charts = new Dictionary<string, LoadedChart> { ["D1"] = D1() };
        var future = DateTime.Today.Year + 5;
        var cut = Render<DashaMattersTable>(p => p.Add(x => x.Charts, charts)
            .Add(x => x.Periods, [Maha("Mercury", 1990, 2000), Maha("Mercury", future, future + 17), Maha("Venus", 1990, future + 40)]));
        var all = cut.FindAll("tbody tr").Count;

        cut.FindAll(".dm-filters select")[1].Change("Mercury");                    // planet
        var mercury = cut.FindAll("tbody tr");
        Assert.True(mercury.Count is > 0 && mercury.Count < all);
        Assert.All(mercury, r => Assert.Contains("Mercury", r.TextContent));

        cut.FindAll(".dm-filters select")[0].Change("ahead");                      // running now and upcoming: the 1990-2000 Mercury goes
        var text = string.Concat(cut.FindAll("tbody tr").Select(r => r.TextContent));
        Assert.DoesNotContain("01/1990", text);
        Assert.Contains($"01/{future}", text);

        cut.Find(".dm-reset").Click();
        Assert.Equal(all, cut.FindAll("tbody tr").Count);
    }
}
