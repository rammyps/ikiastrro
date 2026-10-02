using Bunit;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>The step-2 key-info tables render JHora's readings for 1_Ramakrishnan's D1.</summary>
public sealed class KeyInfoTablesTests : BunitContext
{
    private static ChartKeyDetail Point(string kind, string name, double lon) => new()
    {
        PointKind = kind, Planet = name, NirayanaLongitudeDegrees = lon,
    };

    private static readonly ChartKeyDetail[] D1 =
    [
        Point("Graha", "Ascendant", 0.6574), Point("Graha", "Sun", 8.2019), Point("Graha", "Moon", 217.2088),
        Point("Graha", "Mars", 3.9433), Point("Graha", "Mercury", 1.8326), Point("Graha", "Jupiter", 158.7238),
        Point("Graha", "Venus", 11.9832), Point("Graha", "Saturn", 160.9595), Point("Graha", "Rahu", 102.9146),
        Point("Graha", "Ketu", 282.9146), Point("Upagraha", "Maandi", 198.1023), Point("SpecialLagna", "HL", 354.9145),
    ];

    [Fact]
    public void SensitivePointsShowYogiAvayogiBhriguAndVarnada()
    {
        var cut = Render<SensitivePointsTable>(p => p.Add(x => x.KeyDetails, D1));

        Assert.Contains("Yogi Rahu · Sahayogi Saturn", cut.Markup);
        Assert.Contains("Avayogi Venus", cut.Markup);
        Assert.Equal(15, cut.FindAll("tbody tr").Count);   // 3 points + V1–V12
        var v1 = cut.FindAll("tbody tr")[3];
        Assert.Contains("Varṇada V1", v1.TextContent);
        Assert.Contains("Pisces", v1.TextContent);
        Assert.Contains("H12", v1.TextContent);
    }

    [Fact]
    public void NavaTaraListsNineTarasFromMoonAndLagna()
    {
        var cut = Render<NavaTaraTable>(p => p.Add(x => x.KeyDetails, D1));

        Assert.Equal(9, cut.FindAll("tbody tr").Count);
        Assert.Contains("Anuradha", cut.FindAll("tbody tr")[0].TextContent);
        Assert.Contains("Ashwini", cut.FindAll("tbody tr")[0].TextContent);
    }

    [Fact]
    public void MrityuPushkaraFlagsJupiterAndLagna()
    {
        var cut = Render<MrityuPushkaraTable>(p => p.Add(x => x.KeyDetails, D1));

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(11, rows.Count);
        Assert.Contains("In", rows[4].QuerySelectorAll("td")[4].TextContent);    // Jupiter in Puṣkara bhāga
        Assert.Contains("Lagna", rows[10].TextContent);
        Assert.Contains("In", rows[10].QuerySelectorAll("td")[1].TextContent);   // Lagna in Mrityu bhāga
    }
}
