using Bunit;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class GrahaDrishtiMatrixTests : BunitContext
{
    [Fact]
    public void SeparatesPercentageStrengthFromDiscreteAspectEvidence()
    {
        var rows = new[]
        {
            Row("Mars", "Ascendant", 91.79m, 4),
            Row("Rahu", "Jupiter", 50m, null),
            Row("Sun", "Jupiter", 0m, null)
        };

        var cut = Render<GrahaDrishtiMatrix>(p => p.Add(x => x.Rows, rows));

        Assert.Contains("91.79%", cut.Markup);
        Assert.Contains("4th", cut.Markup);
        Assert.Contains("50%", cut.Markup);
        Assert.DoesNotContain("5th", cut.Markup);
        Assert.Contains("Percentage alone = sphuṭa strength", cut.Markup);

        cut.Find("button[title^='Mars to Ascendant']").Click();
        Assert.Contains("55.071 / 60", cut.Markup);
        Assert.Contains("SRC_PVR_INTEGRATED", cut.Markup);
    }

    private static GrahaDrishtiStrengthRow Row(string source, string target, decimal percentage, byte? discrete) =>
        new(1, "D1", source, target == "Ascendant" ? "Lagna" : "Graha", target,
            target == "Ascendant" ? null : target, 3.286m, 90m, 86.714m,
            41.714m, source == "Mars" ? 13.357m : 0m,
            source == "Mars" ? 55.071m : percentage * .6m, percentage,
            discrete, discrete.HasValue, 1, "SRC_PVR_INTEGRATED");
}
