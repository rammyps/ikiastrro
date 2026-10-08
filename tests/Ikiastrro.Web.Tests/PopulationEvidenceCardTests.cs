using Bunit;
using Ikiastrro.Data.Statistics;
using Ikiastrro.Web.Components.LifeMatters;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class PopulationEvidenceCardTests : BunitContext
{
    private static PopulationEvidenceSnapshot Snapshot(string code, int eligible, params PopulationComparison[] rows) =>
        new(code, "ELIGIBLE", "PERSONAL", eligible, 7, "KI_D1_HOUSE_STRENGTH", 2,
            "KI_D1_HOUSE_STRENGTH_V2", 1, new DateTime(2026, 10, 8, 0, 0, 0, DateTimeKind.Utc), rows);

    private static PopulationComparison Row(decimal? percentile, decimal? z = 0.4m, string sufficiency = "SUFFICIENT",
        decimal? personal = 62m) =>
        new(7, "KI_D1_HOUSE_SUPPORT_V2", personal, 120, 118, 0.0167m, 55m, 40m, 70m, z, percentile, 50m, 60m, sufficiency);

    [Fact]
    public void Available_ShowsPercentileSampleSizeAndUncertainty()
    {
        var cut = Render<PopulationEvidenceCard>(p => p
            .Add(x => x.Snapshot, Snapshot("AVAILABLE", 120, Row(76m)))
            .Add(x => x.Rows, [Row(76m)]));

        var text = cut.Markup.Normalized();
        Assert.Contains("76th percentile", text);
        Assert.Contains("Higher than 76 of every 100 comparable charts", text);
        Assert.Contains("118 of 120 measured", text);
        Assert.Contains("95% CI 50–60", text);
        Assert.Contains("Within the typical range", text);
        Assert.Single(cut.FindAll(".pop-you"));
        Assert.Single(cut.FindAll(".pop-iqr"));
    }

    [Fact]
    public void NullPercentile_IsNotPublishedAndNeverZero()
    {
        var thin = Row(null, null, "INSUFFICIENT");
        var cut = Render<PopulationEvidenceCard>(p => p
            .Add(x => x.Snapshot, Snapshot("AVAILABLE", 12, thin))
            .Add(x => x.Rows, [thin]));

        var text = cut.Markup.Normalized();
        Assert.Contains("Not published", text);
        Assert.DoesNotContain("0th percentile", text);
        Assert.Contains("Insufficient reference data", text);
    }

    [Fact]
    public void Unavailable_ShowsReasonAndNoRows()
    {
        var cut = Render<PopulationEvidenceCard>(p => p
            .Add(x => x.Snapshot, Snapshot("NO_COMPARISONS", 6)));

        Assert.Contains("Insufficient reference data", cut.Markup.Normalized());
        Assert.Empty(cut.FindAll(".pop-row"));
    }

    [Theory]
    [InlineData(1, "1st")]
    [InlineData(2, "2nd")]
    [InlineData(3, "3rd")]
    [InlineData(11, "11th")]
    [InlineData(12, "12th")]
    [InlineData(21, "21st")]
    [InlineData(76, "76th")]
    public void Ordinal_FollowsEnglishRules(int value, string expected) =>
        Assert.Equal(expected, PopulationStrip.Ordinal(value));

    [Fact]
    public void Band_ClampsAndOrdersEnds_AndIsNullWhenAnEndIsMissing()
    {
        Assert.Null(PopulationStrip.Band(null, 50m));
        var band = PopulationStrip.Band(120m, -5m)!.Value;
        Assert.Equal("0", band.Left);
        Assert.Equal("100", band.Width);
    }

    [Theory]
    [InlineData(2.4, "Unusually high for this population")]
    [InlineData(-2.0, "Unusually low for this population")]
    [InlineData(1.2, "Somewhat above typical")]
    [InlineData(-1.5, "Somewhat below typical")]
    [InlineData(0.3, "Within the typical range")]
    public void Position_ReadsRobustZ(double z, string expected) =>
        Assert.Equal(expected, PopulationStrip.Position(Row(50m, (decimal)z)));
}
