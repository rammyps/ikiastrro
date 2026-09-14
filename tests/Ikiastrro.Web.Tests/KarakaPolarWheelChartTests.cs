using Bunit;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class KarakaPolarWheelChartTests : BunitContext
{
    [Fact]
    public void RendersNaturalRulesAndVargaCharaPlacements()
    {
        var sectors = Enumerable.Range(1, 12)
            .Select((h, i) => new KarakaPolarWheelChart.Sector(h, Signs[i])).ToList();
        var primary = Enumerable.Range(1, 12)
            .Select(h => new NaisargikaKarakaRow(h, NaturalPlanets[h - 1], $"House {h} matters")).ToList();
        var details = primary.Select((p, i) => new NaisargikaKarakatwaRow(p.HouseNumber, p.Graha, $"Detailed matter {i + 1}", 1)).ToList();
        var charas = new[]
        {
            new KarakaPolarWheelChart.CharaPoint("Sun", "AK", 1, "Aries"),
            new KarakaPolarWheelChart.CharaPoint("Moon", "AmK", 4, "Cancer"),
            new KarakaPolarWheelChart.CharaPoint("Mars", "BK", 7, "Libra"),
            new KarakaPolarWheelChart.CharaPoint("Mercury", "MK", 10, "Capricornus")
        };

        var cut = Render<KarakaPolarWheelChart>(p => p
            .Add(x => x.Sectors, sectors).Add(x => x.Primary, primary).Add(x => x.Details, details)
            .Add(x => x.CharaPoints, charas).Add(x => x.ChartCode, "D9").Add(x => x.AscendantSign, "Aries"));

        Assert.Equal(12, cut.FindAll(".kpw-sector").Count);
        Assert.Contains("KARAKA", cut.Markup);
        Assert.Contains("Chara Karakas in D9", cut.Markup);
        Assert.Contains(">D9</text>", cut.Markup);
        Assert.DoesNotContain("tbl_Rule_", cut.Markup);
        Assert.Contains(">H1</text>", cut.Markup);
        Assert.Contains(">H12</text>", cut.Markup);
        Assert.DoesNotContain("H@h", cut.Markup);
        cut.MatchesGolden(nameof(KarakaPolarWheelChart));
    }

    [Fact]
    public void HidesCharaKarakasForUnsupportedChartTypes()
    {
        var sectors = Enumerable.Range(1, 12)
            .Select((h, i) => new KarakaPolarWheelChart.Sector(h, Signs[i])).ToList();
        var primary = Enumerable.Range(1, 12)
            .Select(h => new NaisargikaKarakaRow(h, NaturalPlanets[h - 1], $"House {h} matters")).ToList();

        var cut = Render<KarakaPolarWheelChart>(p => p
            .Add(x => x.Sectors, sectors).Add(x => x.Primary, primary)
            .Add(x => x.Details, Array.Empty<NaisargikaKarakatwaRow>())
            .Add(x => x.CharaPoints, new[] { new KarakaPolarWheelChart.CharaPoint("Sun", "AK", 1, "Aries") })
            .Add(x => x.ShowCharaKarakas, false).Add(x => x.ChartCode, "D10"));

        Assert.Empty(cut.FindAll(".kpw-chara"));
        Assert.Empty(cut.FindAll(".kpw-chara-row"));
        Assert.DoesNotContain("Chara Karakas in D10", cut.Markup);
        Assert.DoesNotContain("role · Chara", cut.Markup);
    }

    [Fact]
    public void ShowsLifeMatterReferencesForSelectedChartAndHouseOnly()
    {
        var sectors = Enumerable.Range(1, 12).Select((h, i) => new KarakaPolarWheelChart.Sector(h, Signs[i])).ToList();
        var matters = new[]
        {
            new LifeMatterReferenceRow("Self", 1, "Physical self", "D1", "1st", "Sun", null, "1st from Sun", "PVR_DIRECT"),
            new LifeMatterReferenceRow("Self", 2, "Mind", "D1", "1st", "Moon", null, "1st from Moon", "PVR_DIRECT"),
            new LifeMatterReferenceRow("Marriage", 1, "Spouse", "D9", "7th", "Venus", null, "7th from Venus", "PVR_DIRECT"),
            new LifeMatterReferenceRow("Health", 1, "Illness", "D6", "1st", "Mars", null, "6th from Mars", "PVR_DIRECT")
        };
        var cut = Render<KarakaPolarWheelChart>(p => p
            .Add(x => x.Sectors, sectors).Add(x => x.Primary, new[] { new NaisargikaKarakaRow(1, "Sun", "Self") })
            .Add(x => x.Details, Array.Empty<NaisargikaKarakatwaRow>()).Add(x => x.LifeMatters, matters)
            .Add(x => x.PlanetPlacements, new[] { new KarakaPolarWheelChart.PlanetPlacement("Sun", "Aries", "Mars", "Ashwini", "Ketu", "Venus", 1) })
            .Add(x => x.HouseLords, new[] { new KarakaPolarWheelChart.HouseLordPoint(1, "Mars", 3, "Gemini") })
            .Add(x => x.ChartCode, "D1"));
        Assert.Contains("What does this placement indicate about physical self?", cut.Markup);
        Assert.DoesNotContain("Mind", cut.Markup);
        Assert.DoesNotContain("Spouse", cut.Markup);
        Assert.DoesNotContain("Illness", cut.Markup);
        Assert.Contains("House lord", cut.Markup);
        Assert.Contains("Ashwini", cut.Markup);
        Assert.Equal(2, cut.FindAll(".kpw-data-ring").Count);
    }

    private static readonly string[] Signs = { "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricornus", "Aquarius", "Pisces" };
    private static readonly string[] NaturalPlanets = { "Sun", "Jupiter", "Mars", "Moon", "Jupiter", "Mars", "Venus", "Saturn", "Jupiter", "Mercury", "Jupiter", "Saturn" };
}
