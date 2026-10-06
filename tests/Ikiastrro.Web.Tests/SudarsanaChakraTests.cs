using Bunit;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>Sudarsana chakra: Cancer lagna, Moon in Pisces (house 9 from lagna), Sun in Scorpio (house 5 from lagna).</summary>
public sealed class SudarsanaChakraTests : BunitContext
{
    private static readonly SudarsanaChakra.Planet[] Planets =
    [
        new("Sun", 220), new("Moon", 340), new("Mars", 250), new("Mercury", 255), new("Jupiter", 335),
        new("Venus", 190), new("Saturn", 170), new("Rahu", 260), new("Ketu", 80),
    ];

    [Fact]
    public void DrawsThreeRingsOfTwelveCellsEachWithEveryGrahaInEachRing()
    {
        var cut = Render<SudarsanaChakra>(p => p.Add(x => x.AscendantSign, "Cancer").Add(x => x.Planets, Planets));

        Assert.Equal(3, cut.FindAll("g.sdc-ring").Count);
        Assert.Equal(36, cut.FindAll("path.sdc-cell").Count);
        Assert.Equal(27, cut.FindAll("g.sdc-planet").Count);                       // 9 grahas x 3 rings
        Assert.Contains("Lagna Can", cut.Markup.Normalized());
        Assert.Contains("Moon Pis", cut.Markup.Normalized());
        Assert.Contains("Sun Sco", cut.Markup.Normalized());
    }

    [Fact]
    public void EachRingCountsItsHousesFromItsOwnReference()
    {
        var cut = Render<SudarsanaChakra>(p => p.Add(x => x.AscendantSign, "Cancer").Add(x => x.Planets, Planets));
        var titles = cut.FindAll("path.sdc-cell title").Select(t => t.TextContent.Trim()).ToList();

        Assert.Contains("Pisces · house 9 from Lagna", titles);   // Pisces is the 9th from Cancer
        Assert.Contains("Pisces · house 1 from Moon", titles);
        Assert.Contains("Pisces · house 5 from Sun", titles);     // Scorpio is the Sun's 1st; Pisces is 5th from it
        Assert.Contains("Scorpio · house 1 from Sun", titles);
    }

    [Fact]
    public void RunningDashaLordsLightUpInTheirOwnRing()
    {
        var cut = Render<SudarsanaChakra>(p => p.Add(x => x.AscendantSign, "Cancer").Add(x => x.Planets, Planets)
            .Add(x => x.MahaLord, "Mercury").Add(x => x.AntarLord, "Venus").Add(x => x.PratyantarLord, "Ketu"));

        Assert.Equal(3, cut.FindAll("g.sdc-ring.is-active").Count);
        Assert.Equal(["Me"], cut.FindAll("g.sdc-ring-sun g.sdc-planet.is-lord").Select(g => g.TextContent.Trim()));    // mahadasa -> Sun ring
        Assert.Equal(["Ve"], cut.FindAll("g.sdc-ring-moon g.sdc-planet.is-lord").Select(g => g.TextContent.Trim()));   // antardasa -> Moon ring
        Assert.Equal(["Ke"], cut.FindAll("g.sdc-ring-lagna g.sdc-planet.is-lord").Select(g => g.TextContent.Trim()));  // pratyantar -> Lagna ring
    }
}
