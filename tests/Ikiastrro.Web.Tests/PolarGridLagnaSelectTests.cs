using Bunit;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class PolarGridLagnaSelectTests : BunitContext
{
    [Fact]
    public void RendersOrderedAdditiveLagnaChoicesWithoutKarakasOrDetailTable()
    {
        var cut = Render<PolarGridLagnaSelect>(p => p
            .Add(x => x.Sectors, Sectors)
            .Add(x => x.ReferencePoints, References)
            .Add(x => x.ChartCode, "D1")
            .Add(x => x.AscendantSign, "Aries"));

        Assert.Equal(12, cut.FindAll(".pgls-sector").Count);
        Assert.Equal(
            new[] { "SIGN LAGNA", "ARUDHA LAGNA", "HORA LAGNA", "SREE LAGNA", "GHATI LAGNA" },
            cut.FindAll(".pgls-check span").Select(x => x.TextContent.Trim()).ToArray());
        Assert.Single(cut.FindAll(".pgls-check.is-active"));
        Assert.True(cut.FindAll(".pgls-check input")[0].HasAttribute("checked"));
        Assert.DoesNotContain("Naisargika", cut.Markup);
        Assert.DoesNotContain("Chara", cut.Markup);
        Assert.Empty(cut.FindAll("table"));
        Assert.Empty(cut.FindAll("aside"));
    }

    [Fact]
    public void SelectsMultipleReferencesAndSupportsSideBySideSouthIndianGrids()
    {
        var polar = Render<PolarGridLagnaSelect>(p => p
            .Add(x => x.Sectors, Sectors)
            .Add(x => x.ReferencePoints, References)
            .Add(x => x.ChartCode, "D1"));

        polar.FindAll(".pgls-check input")[1].Change(true);
        Assert.Equal(2, polar.FindAll(".pgls-check.is-active").Count);
        Assert.Equal(2, polar.FindAll(".pgls-check input[checked]").Count);
        Assert.Contains("SI-1", polar.Markup);
        Assert.Contains("AR-2", polar.Markup);
        Assert.Equal(24, polar.FindAll(".pgls-house tspan").Count);
        Assert.Contains("ref-lagna", polar.FindAll(".pgls-check")[0].ClassList);
        Assert.Contains("ref-arudha", polar.FindAll(".pgls-check")[1].ClassList);
        Assert.Contains("AR-1", polar.Markup); // Arudha Lagna (sign Pisces) at its own sector reads "AR-1", same self-reference pattern as "SI-1" for Sign Lagna

        var south = Render<PolarGridLagnaSelect>(p => p
            .Add(x => x.Sectors, Sectors)
            .Add(x => x.ReferencePoints, References)
            .Add(x => x.ChartCode, "D9")
            .Add(x => x.View, "south"));

        south.FindAll(".pgls-check input")[1].Change(true);
        Assert.Equal(2, south.FindAll(".pgls-south-grid").Count);
        Assert.Equal(2, south.FindAll(".chart").Count);
        Assert.Empty(south.FindAll(".pgls-polar"));
    }

    private static readonly IReadOnlyList<PolarGridLagnaSelect.Sector> Sectors =
        new[] { "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricornus", "Aquarius", "Pisces" }
            .Select((sign, index) => new PolarGridLagnaSelect.Sector(index + 1, sign)).ToList();

    private static readonly IReadOnlyList<PolarGridLagnaSelect.ReferencePoint> References =
    [
        new("LAGNA", "Sign Lagna", "Aries"),
        new("ARUDHA_LAGNA", "Arudha Lagna", "Pisces"),
        new("HORA_LAGNA", "Hora Lagna", "Taurus"),
        new("SREE_LAGNA", "Sree Lagna", "Gemini"),
        new("GHATI_LAGNA", "Ghati Lagna", "Cancer")
    ];
}
