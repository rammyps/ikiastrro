using Bunit;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class PlanetStrengthChartTests : BunitContext
{
    private static StackPart Part(string key, decimal value) => new(key, key, "kala", value);

    [Fact]
    public void AxisCoversLongestStackAndMarkersAndDropsBelowZeroOnlyForNegatives()
    {
        var rows = new[]
        {
            (IReadOnlyList<StackPart>)new[] { Part("a", 3m), Part("b", 4.2m), Part("d", -0.4m) },
            new[] { Part("a", 2m) },
        };

        var axis = StackedBarLayout.Axis(rows, new decimal?[] { 5m, 9.5m });

        Assert.Equal(-0.5m, axis.Min);
        Assert.Equal(10m, axis.Max);
        Assert.Equal(0m, StackedBarLayout.Axis(new[] { (IReadOnlyList<StackPart>)new[] { Part("a", 2m) } }, new decimal?[] { 5m }).Min);
    }

    [Fact]
    public void PositivesStackRightFromZeroAndNegativesLeftOfIt()
    {
        var axis = new StackAxis(-1m, 9m);   // zero sits at 10%
        var segments = StackedBarLayout.Layout(new[] { Part("a", 2m), Part("d", -0.5m), Part("b", 3m), Part("z", 0m) }, axis);

        Assert.Equal(3, segments.Count);   // the zero part draws nothing
        Assert.Equal(10, segments[0].LeftPercent, 6);
        Assert.Equal(20, segments[0].WidthPercent, 6);
        Assert.True(segments[1].IsNegative);
        Assert.Equal(5, segments[1].LeftPercent, 6);
        Assert.Equal(5, segments[1].WidthPercent, 6);
        Assert.Equal(30, segments[2].LeftPercent, 6);   // continues after "a", not after the negative
        Assert.Equal(30, segments[2].WidthPercent, 6);
    }

    [Fact]
    public void RendersBalaSegmentsInOrderWithMinimumTickAndHoverBreakdown()
    {
        var cut = Render<PlanetStrengthChart>(p => p
            .Add(x => x.Rows, new[] { Mars() })
            .Add(x => x.Components, Components()));

        var segments = cut.FindAll(".psc-seg");
        Assert.Equal(new[] { "Sthāna", "Dig", "Kāla", "Cheṣṭā", "Naisargika", "Dṛk" },
            segments.Select(s => s.GetAttribute("title")!.Split(':')[0]));
        Assert.Contains("Nathonnata 60.00 · Pakṣa 9.70 · Ayana 43.24", segments[2].GetAttribute("title"));
        Assert.Contains("psc-seg-neg", segments[5].ClassName);   // Dṛk −0.41 drawn, left of zero
        Assert.Single(cut.FindAll(".psc-zero"));
        Assert.Contains("left:52.38%", cut.Find(".psc-min").GetAttribute("style"));   // 5 rūpas on a −0.5..10 axis
        Assert.Contains("46.6 · 7.9", cut.Markup);
        Assert.Empty(cut.FindAll("details"));
    }

    [Fact]
    public void SubComponentSwitchSplitsEachBala()
    {
        var cut = Render<PlanetStrengthChart>(p => p
            .Add(x => x.Rows, new[] { Mars() })
            .Add(x => x.Components, Components()));

        cut.FindAll(".psc-toggle button")[1].Click();

        var titles = cut.FindAll(".psc-seg").Select(s => s.GetAttribute("title")!).ToList();
        Assert.Contains(titles, t => t.StartsWith("Kāla · Pakṣa: 0.16 rūpas"));
        Assert.Contains(titles, t => t.StartsWith("Kāla · Ayana: 0.72 rūpas"));
        Assert.Contains("color-mix", cut.Markup);   // later sub-components fade within the bala's colour
    }

    private static ShadbalaSummaryRow Mars() => new("Mars",
        SthanaBalaVirupas: 330.6m, DigBalaVirupas: 30m, KalaBalaVirupas: 112.94m, CheshtaBalaVirupas: 57.15m,
        NaisargikaBalaVirupas: 17.14m, DrikBalaVirupas: -24.6m, YuddhaBalaVirupas: 0m,
        ShadbalaVirupas: 523.23m, ShadbalaRupas: 8.72m, MinimumRequiredRupas: 5m, PercentOfMinimum: 174m,
        IshtaBala: 46.62m, KashtaBala: 7.92m);

    private static ShadbalaComponentRow[] Components() =>
    [
        new("Mars", "KALA_BALA", "AYANA_BALA", 43.24m),
        new("Mars", "KALA_BALA", "NATHONNATA_BALA", 60m),
        new("Mars", "KALA_BALA", "PAKSHA_BALA", 9.7m),
        new("Mars", "STHANA_BALA", "UCHCHA_BALA", 38m),
        new("Mars", "STHANA_BALA", "SAPTAVARGAJA_BALA", 292.6m),
    ];
}
