using Bunit;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Ikiastrro.Web.Components.Workspace;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>The Phalita dasas panel on PVR's Chart 23 (Example 66: born August 1912, Aquarius Lagna, Narayana dasa from Leo).</summary>
public sealed class PhalitaDasaPanelTests : BunitContext
{
    private static ChartKeyDetail G(string planet, string sign, double lon, string? karaka = null) => new()
        { PointKind = "Graha", Planet = planet, Sign = sign, NirayanaLongitudeDegrees = lon, CharaKaraka = karaka };

    private static ChartKeyDetail P(string kind, string name, string sign, double lon) => new()
        { PointKind = kind, Planet = name, Sign = sign, NirayanaLongitudeDegrees = lon };

    private static LoadedChart Chart23() => new("D1", "Rasi", "Rasi", null, "Aquarius", "Taurus", null,
        [
            G("Ascendant", "Aquarius", 310),
            G("Sun", "Cancer", 100), G("Moon", "Taurus", 31), G("Saturn", "Taurus", 40), G("Mercury", "Leo", 130, "AK"),
            G("Mars", "Leo", 132), G("Venus", "Leo", 134), G("Jupiter", "Scorpio", 220), G("Rahu", "Gemini", 70), G("Ketu", "Sagittarius", 250),
            P("SpecialLagna", "SL", "Capricornus", 282.35), P("SpecialLagna", "HL", "Aries", 10), P("SpecialLagna", "GL", "Leo", 130),
            P("Arudha", "AL", "Cancer", 100), P("Arudha", "A10", "Capricornus", 280), P("Arudha", "A12", "Aries", 5),
        ], [], [], [], [], [], null, null, "test");

    private static BirthDetails Person() => new()
        { Id = 1, Name = "Chart 23", DateOfBirth = new DateOnly(1912, 8, 15), TimeOfBirth = new TimeOnly(10, 0), UtcOffset = "05:30" };

    private IRenderedComponent<PhalitaDasaPanel> Render23() =>
        Render<PhalitaDasaPanel>(p => p.Add(x => x.Chart, Chart23()).Add(x => x.Person, Person()));

    [Fact]
    public void NarayanaTabShowsSeedRunningDasaAntardasasAndPvrReading()
    {
        var cut = Render23();
        cut.Find("input[type=date]").Change("2020-01-01");

        var text = cut.Markup.Normalized();
        Assert.Contains("Narayana dasa (PVR ch.18)", text);
        Assert.Contains("Leo", cut.FindAll(".pd-chip")[0].Normalized());          // the seed comes first: Le, Cp, Ge, Sc ...
        Assert.Equal(12, cut.FindAll(".pd-chip").Count);
        // 1 Jan 2020 falls in the second cycle's Ge dasa? first cycle ended Aug 1977; Le 11, Cp 4, Ge 10 -> Ge runs 1992-2002, Sc follows.
        Assert.Contains("Running on 1 Jan 2020:", text);
        Assert.Contains("Antardasas of", text);
        Assert.Contains("What PVR reads in the running", text);
        Assert.Contains("PVR §18.4, pp.238-240", text);
    }

    [Fact]
    public void FiveTabsSudasaNeedsSreeLagnaAndAshtottariShowsTheThreeViews()
    {
        var cut = Render23();
        Assert.Equal(5, cut.FindAll(".pd-tab").Count);

        cut.FindAll(".pd-tab")[2].Click();   // SUDASA: SL is in this chart
        Assert.Contains("Sudasa (PVR ch.20)", cut.Markup.Normalized());
        Assert.Contains("Capricorn", cut.FindAll(".pd-chip")[0].Normalized());

        cut.FindAll(".pd-tab")[3].Click();   // DRIGDASA from the 9th (Libra for Aquarius Lagna)
        Assert.Contains("Drigdasa (PVR ch.21)", cut.Markup.Normalized());

        cut.FindAll(".pd-tab")[4].Click();   // ASHTOTTARI
        var text = cut.Markup.Normalized();
        Assert.Contains("Ashtottari dasa (PVR ch.17)", text);
        Assert.Contains("Applicable in all charts", text);
        Assert.Contains("Rahu, not in the Lagna", text);
    }

    [Fact]
    public void SudasaWithoutSreeLagnaSaysSo()
    {
        var chart = Chart23() with { KeyDetails = Chart23().KeyDetails.Where(k => k.Planet != "SL").ToList() };
        var cut = Render<PhalitaDasaPanel>(p => p.Add(x => x.Chart, chart).Add(x => x.Person, Person()));
        cut.FindAll(".pd-tab")[2].Click();
        Assert.Contains("Needs Sree Lagna", cut.Markup.Normalized());
    }
}
