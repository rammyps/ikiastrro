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

    [Fact]
    public void RudraMaheswaraShowsRudraTrishoolaAndMaheswara()
    {
        var cut = Render<RudraMaheswaraTable>(p => p.Add(x => x.KeyDetails, D1));

        var rows = cut.FindAll("tbody tr");
        Assert.True(rows.Count >= 3);
        Assert.Contains("Rudra", rows[0].TextContent);
        Assert.Contains("Trishoola rasis", rows[1].TextContent);
        Assert.Equal(3, rows[1].QuerySelectorAll("td")[1].TextContent.Split('·').Length);   // three trine signs
        Assert.Contains(rows, r => r.TextContent.StartsWith("Maheswara"));
        Assert.Contains("Method of three pairs", cut.Markup);   // D1 fixture carries HL
        Assert.Contains("Eighth lord method", cut.Markup);
        Assert.Equal(3, cut.FindAll("tbody tr").Count(r => r.TextContent.StartsWith("Pair:")));
        Assert.Contains("life", cut.Markup);
    }

    [Fact]
    public void SayanadiWorkingShowsEachGrahasInputsAndState()
    {
        // Ananya-style inputs: Moon in nakshatra 26 (M), 36th ghati (Janma ghatis 35.x), Lagna sign 4 (L).
        ChartKeyDetail G(string name, int nak, decimal degree, int signId = 1) => new()
            { PointKind = "Graha", Planet = name, NakshatraId = (byte)nak, DegreesInSignDecimal = degree, SignId = (byte)signId };
        ChartKeyDetail[] kd =
        [
            G("Ascendant", 1, 6.8m, 4), G("Sun", 18, 28m), G("Moon", 26, 8m), G("Mars", 19, 14m), G("Mercury", 19, 14m),
            G("Jupiter", 25, 1m), G("Venus", 15, 20m), G("Saturn", 13, 25m), G("Rahu", 19, 7m), G("Ketu", 6, 7m),
        ];
        var states = Enumerable.Range(1, 12).ToDictionary(i => (byte)i,
            i => new Ikiastrro.Core.Engines.PlanetaryStates.PlanetaryStateRow((byte)i, "Sayanadi", $"State{i}", (byte)i, null));
        var cut = Render<SayanadiWorkingTable>(p => p
            .Add(x => x.KeyDetails, kd).Add(x => x.JanmaGhatis, 35.4m).Add(x => x.StateNames, states));

        Assert.Contains("M</b> = 26", cut.Markup);
        Assert.Contains("G</b> = 36", cut.Markup);
        Assert.Contains("L</b> = 4", cut.Markup);
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(9, rows.Count);
        // Sun: C=18, P=1, A=9 (28 degrees is the 9th navamsa) -> 162 + 26 + 36 + 4 = 228 -> mod 12 = 0 -> 12.
        var sun = rows[0].QuerySelectorAll("td");
        Assert.Equal("228", sun[6].TextContent);
        Assert.Equal("12", sun[7].TextContent);
        Assert.Contains("State12", sun[8].TextContent);
    }
}
