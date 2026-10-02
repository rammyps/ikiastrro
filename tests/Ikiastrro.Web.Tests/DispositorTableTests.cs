using Bunit;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>Astro Facts 1.4 "Rāśi &amp; graha dispositors" — one row per graha from the chart's
/// own key details (a real D1 placement: Ramya, Gemini lagna).</summary>
public class DispositorTableTests : BunitContext
{
    private static ChartKeyDetail Graha(string planet, int? planetId, string sign, int house) =>
        new() { Planet = planet, PlanetId = planetId, Sign = sign, PointKind = "Graha", HouseNumberFromLagna = house };

    private static readonly ChartKeyDetail[] RamyaD1 =
    {
        Graha("Ascendant", null, "Gemini", 1),
        Graha("Sun", 1, "Scorpio", 6), Graha("Moon", 2, "Capricornus", 8), Graha("Mars", 3, "Virgo", 4),
        Graha("Mercury", 4, "Sagittarius", 7), Graha("Jupiter", 5, "Scorpio", 6), Graha("Venus", 6, "Libra", 5),
        Graha("Saturn", 7, "Libra", 5), Graha("Rahu", 8, "Taurus", 12), Graha("Ketu", 9, "Scorpio", 6),
        new() { Planet = "AL", Sign = "Leo", PointKind = "Arudha" },
    };

    [Fact]
    public void RendersOneRowPerGrahaWithItsDispositorChain()
    {
        var cut = Render<DispositorTable>(ps => ps
            .Add(p => p.ChartType, "D1")
            .Add(p => p.AscendantSign, "Gemini")
            .Add(p => p.KeyDetails, RamyaD1));

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(9, rows.Count);

        var sun = rows[0].TextContent;
        Assert.Contains("Sun", sun);
        Assert.Contains("Mars", sun);                    // Scorpio's lord
        Assert.Contains("Sun → Mars → Mercury → Jupiter", sun);
    }

    [Fact]
    public void UnreadableAscendant_ShowsAnEmptyStateInsteadOfABlankTable()
    {
        // Regression: AstroFacts passed the literal text "relationshipChart.AscendantSign" (no @),
        // which parsed as no sign and rendered a header with no rows.
        var cut = Render<DispositorTable>(ps => ps
            .Add(p => p.ChartType, "D1")
            .Add(p => p.AscendantSign, "relationshipChart.AscendantSign")
            .Add(p => p.KeyDetails, RamyaD1));

        Assert.Empty(cut.FindAll("table"));
        Assert.Contains("No dispositor data", cut.Markup);
    }
}
