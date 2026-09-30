using Ikiastrro.Data.Statistics;
using Ikiastrro.Web.Components.LifeMatters;
using Xunit;

namespace Ikiastrro.Web.Tests;

public class LifeMatterReadingTests
{
    [Theory]
    [InlineData(80, "Strong support")]
    [InlineData(65, "Strong support")]
    [InlineData(58, "Good support")]
    [InlineData(50, "Moderate support")]
    [InlineData(45, "Moderate support")]
    [InlineData(40, "Limited support")]
    [InlineData(20, "Weak support")]
    public void Band_puts_the_figure_into_words_around_the_50_midpoint(int percent, string label) =>
        Assert.Equal(label, LifeMatterReading.Band(percent).Label);

    [Fact]
    public void Band_of_no_figure_is_not_measured() =>
        Assert.Equal("Not measured", LifeMatterReading.Band(null).Label);

    // SAV 36 → 64, BAV 2 → 25, Shadbala 150% → 75, Amsabala 40, no Bhava Bala (varga), no Argala.
    private static HouseStatistics House() => new(
        "Aries", 2, 36, null, null, null, "Mars", 150m, 2, 40, new ArgalaSummary([]), []);

    [Fact]
    public void Factors_rank_the_strongest_and_the_limiting_signals()
    {
        var factors = LifeMatterReading.Factors([House()], "D2");

        Assert.Equal(["Strength of the ruling planets", "Support the house receives"],
            LifeMatterReading.Strongest(factors).Select(f => f.Name));
        Assert.Equal(["Ruler's comfort in the house", "Consistency across the divisional charts"],
            LifeMatterReading.Limiting(factors).Select(f => f.Name));
    }

    [Fact]
    public void Limiting_falls_back_to_unmeasured_signals_as_uncertain()
    {
        var strong = House() with { LordBavBindus = 7, LordAmsabalaPercent = 70 };
        var limiting = LifeMatterReading.Limiting(LifeMatterReading.Factors([strong], "D2"));

        Assert.All(limiting, f => Assert.Null(f.Percent));
        Assert.Equal(["Strength of the house itself", "Help or hindrance from other planets"], limiting.Select(f => f.Name));
    }

    [Fact]
    public void Only_four_perspectives_are_primary()
    {
        Assert.Equal(["LAGNA", "CHANDRA_LAGNA", "RAVI_LAGNA", "ARUDHA_LAGNA"],
            LifeMatterReading.PrimaryPerspectives.Select(p => p.ReferenceCode));
        Assert.Equal("Overall", LifeMatterReading.PerspectiveName(LifeMatterQuestions.Get("LAGNA")));
        Assert.Equal("Hora Lagna", LifeMatterReading.PerspectiveName(LifeMatterQuestions.Get("HORA_LAGNA")));
    }
}
