using Bunit;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class HouseStrengthChartTests : BunitContext
{
    [Fact]
    public void StacksTheThreeBhavaBalaPartsWithCutOffLinesAndNegativeDrik()
    {
        var rows = new[]
        {
            new BhavaBalaSummaryRow(1, "Aries", "Mars", 1, "Aries", "Own Sign", 660m, 11m),
            new BhavaBalaSummaryRow(7, "Libra", "Venus", 1, "Aries", "Enemy", 240m, 4m),
        };
        var components = new[]
        {
            new BhavaBalaComponentRow(1, "BHAVADHIPATI_BALA", 600m), new BhavaBalaComponentRow(1, "BHAVA_DIG_BALA", 60m),
            new BhavaBalaComponentRow(7, "BHAVADHIPATI_BALA", 300m), new BhavaBalaComponentRow(7, "BHAVA_DIG_BALA", 0m),
            new BhavaBalaComponentRow(7, "BHAVA_DRIK_BALA", -60m),
        };

        var cut = Render<HouseStrengthChart>(p => p.Add(x => x.Rows, rows).Add(x => x.Components, components));

        var houseRows = cut.FindAll(".hsc-row");
        Assert.Contains("H1", houseRows[0].TextContent);   // strength rank first
        Assert.Equal(2, houseRows[0].QuerySelectorAll(".hsc-seg").Length);
        Assert.Contains("Bhāvādhipati: 10.00 rūpas", houseRows[0].QuerySelector(".hsc-seg")!.GetAttribute("title"));
        Assert.Contains("hsc-seg-neg", houseRows[1].QuerySelectorAll(".hsc-seg")[1].ClassName);
        Assert.Equal(4, cut.FindAll(".hsc-band").Count);   // Moderate + Strong on each row
        Assert.Contains("-1.00", houseRows[1].QuerySelectorAll(".hsc-c-num")[1].TextContent);   // Indep. = Dig + Dṛk
        Assert.Empty(cut.FindAll(".hsc-detail"));
    }

    [Fact]
    public void HouseOrderKeepsStrengthRanks()
    {
        var rows = new[]
        {
            new BhavaBalaSummaryRow(1, "Aries", "Mars", 1, "Aries", null, 240m, 4m),
            new BhavaBalaSummaryRow(2, "Taurus", "Venus", 1, "Aries", null, 480m, 8m),
        };
        var cut = Render<HouseStrengthChart>(p => p.Add(x => x.Rows, rows).Add(x => x.Components, Array.Empty<BhavaBalaComponentRow>()));

        cut.FindAll(".hsc-toggle button")[0].Click();

        var first = cut.FindAll(".hsc-row")[0];
        Assert.Contains("H1", first.TextContent);
        Assert.Equal("2", first.QuerySelector(".hsc-rankbadge")!.TextContent);
    }
}
