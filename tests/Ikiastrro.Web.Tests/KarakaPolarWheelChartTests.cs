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
        cut.MatchesGolden(nameof(KarakaPolarWheelChart));
    }

    private static readonly string[] Signs = { "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricornus", "Aquarius", "Pisces" };
    private static readonly string[] NaturalPlanets = { "Sun", "Jupiter", "Mars", "Moon", "Jupiter", "Mars", "Venus", "Saturn", "Jupiter", "Mercury", "Jupiter", "Saturn" };
}
