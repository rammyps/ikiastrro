using Bunit;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class NatalTransitWheelChartTests : BunitContext
{
    [Fact]
    public void NakshatraRingRotatesWithScorpioAscendant()
    {
        var cut = Render<Natal_Transit_Comp_WheelChart>(parameters => parameters
            .Add(p => p.NatalPoints, Array.Empty<Natal_Transit_Comp_WheelChart.Point>())
            .Add(p => p.AscendantSign, "Scorpio"));

        var ashwin = cut.FindAll(".ntw-nak-name").Single(e => e.TextContent == "Ashwini");

        Assert.Equal("495.85", ashwin.GetAttribute("x"));
        Assert.Equal("622.21", ashwin.GetAttribute("y"));
    }
}
