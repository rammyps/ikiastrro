using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Core.Models;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.LifeMatters;
using Ikiastrro.Web.Components.Workspace;
using Xunit;

namespace Ikiastrro.Web.Tests;

public class LifeMatterFoundationTests
{
    private static LoadedChart D1(decimal ascDegree, string lordPlanet = "Sun") => new(
        "D1", "Rasi", "Rāśi", null, "Leo", "Taurus", "Rohini",
        [
            new ChartKeyDetail { Planet = "Ascendant", Sign = "Leo", DegreesInSignDecimal = ascDegree },
            new ChartKeyDetail { Planet = "Moon", Sign = "Taurus", Nakshatra = "Rohini", HouseNumberFromLagna = 10, DignityStatus = "Exalted" },
        ],
        [new ChartHouseLord { HouseNumber = 1, HouseSign = "Leo", LordPlanet = lordPlanet, LordPlacedInHouseFromLagna = 11, LordDignityStatus = "Friend" }],
        [], [], [], [], null, null, "test");

    private static ShadbalaSummaryRow Strength(string planet, decimal percent) =>
        new(planet, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, percent);

    [Fact]
    public void Lists_rising_sign_lagna_lord_moon_and_birth_time_check()
    {
        var items = LifeMatterFoundation.Build(D1(12.3m), [Strength("Sun", 132)]);
        Assert.Equal(["Rising sign", "Lagna lord", "Moon", "Birth-time check"], items.Select(i => i.Label));
        Assert.Equal("Leo 12.3°", items[0].Value);
        Assert.Equal("Sun in the 11th · Friend", items[1].Value);
        Assert.Equal("good", items[1].Tone);
        Assert.Equal("Taurus · Rohini", items[2].Value);
        Assert.Equal("good", items[3].Tone);
    }

    [Fact]
    public void A_lagna_lord_below_its_minimum_reads_weak() =>
        Assert.Equal("weak", LifeMatterFoundation.Build(D1(12m), [Strength("Sun", 84)])[1].Tone);

    [Fact]
    public void Without_shadbala_the_lagna_lord_is_neutral() =>
        Assert.Null(LifeMatterFoundation.Build(D1(12m), [])[1].Tone);

    [Theory]
    [InlineData(0.4, "weak", "Lagna 0.4° from a sign edge")]
    [InlineData(29.5, "weak", "Lagna 0.5° from a sign edge")]
    [InlineData(2.0, "caution", "Lagna 2.0° from a sign edge")]
    [InlineData(15.0, "good", "Lagna 15.0° inside its sign")]
    public void Birth_time_check_measures_the_distance_to_the_nearer_sign_edge(double degree, string tone, string value)
    {
        var check = LifeMatterFoundation.Build(D1((decimal)degree), [])[3];
        Assert.Equal(tone, check.Tone);
        Assert.Equal(value, check.Value);
    }
}

public class LifeMatterHouseIndexTests
{
    private static LifeMatterStepRow Step(int id, string code, string area) =>
        new(id, code, area, area, id, $"Matter {code}", "D1", "", "", "CLASSICAL", null);

    private static LifeMatterFocusRule House(int matterId, int house, string? reference = "LAGNA", int priority = 1) =>
        new(matterId * 100 + house, 1, matterId, LifeMatterFocusKind.House, reference, house, null, priority);

    [Fact]
    public void Groups_lagna_house_foci_by_house_in_priority_then_step_order()
    {
        var steps = new[] { Step(1, "WEALTH_01", "Wealth"), Step(2, "WEALTH_02", "Wealth"), Step(3, "CAREER_01", "Career") };
        var foci = new[]
        {
            House(2, 2), House(1, 2, priority: 2), House(3, 10),
            House(3, 2, reference: "ARUDHA_LAGNA"),                                     // not counted from the Lagna
            new LifeMatterFocusRule(9, 1, 1, LifeMatterFocusKind.SpecialPoint, null, null, "GULIKA", 1),
            House(4, 5),                                                              // no active step
        };

        var index = LifeMatterHouseIndex.ByHouse(steps, foci);

        Assert.Equal([2, 10], index.Keys.Order());
        Assert.Equal(["WEALTH_02", "WEALTH_01"], index[2].Select(m => m.MatterCode));
        Assert.Equal("Career", index[10][0].AreaName);
    }

    [Fact]
    public void A_matter_with_two_foci_on_one_house_appears_once() =>
        Assert.Single(LifeMatterHouseIndex.ByHouse([Step(1, "A_01", "A")], [House(1, 4), House(1, 4, priority: 2)])[4]);
}

public class KeyInferenceLinkTests
{
    [Fact]
    public void D1_is_left_out() =>
        Assert.Equal("/key-inference/2?step=houses", KeyInferenceLink.Url(2, KeyInferenceLink.Houses, "D1"));

    [Fact]
    public void A_varga_is_named() =>
        Assert.Equal("/key-inference/2?step=houses&chart=D2-US", KeyInferenceLink.Url(2, KeyInferenceLink.Houses, "D2-US"));
}
