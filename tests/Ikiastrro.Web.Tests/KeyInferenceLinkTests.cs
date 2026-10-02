using Ikiastrro.Web.Components.LifeMatters;
using Xunit;

namespace Ikiastrro.Web.Tests;

public class KeyInferenceLinkTests
{
    [Fact]
    public void Url_leaves_out_the_defaults() =>
        Assert.Equal("/key-inference/2?matter=WEALTH_02", KeyInferenceLink.Url(2, "WEALTH_02", "LAGNA", null));

    [Fact]
    public void Url_names_the_perspective_by_its_tag_and_the_chart() =>
        Assert.Equal("/key-inference/2?matter=WEALTH_02&lagna=MO&chart=D2-US",
            KeyInferenceLink.Url(2, "WEALTH_02", "CHANDRA_LAGNA", "D2-US"));

    [Fact]
    public void Url_without_a_matter_is_the_bare_page() =>
        Assert.Equal("/key-inference/7", KeyInferenceLink.Url(7, null, "LAGNA", null));

    [Theory]
    [InlineData("MO", "CHANDRA_LAGNA")]
    [InlineData("hl", "HORA_LAGNA")]
    [InlineData("chandra_lagna", "CHANDRA_LAGNA")]
    [InlineData("LAG", "LAGNA")]
    [InlineData("nonsense", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void ResolveLagna_accepts_a_tag_or_a_reference_code(string? value, string? expected) =>
        Assert.Equal(expected, KeyInferenceLink.ResolveLagna(value));

    [Theory]
    [InlineData("d10", "D10")]
    [InlineData("D2-us", "D2-US")]
    [InlineData("D99", null)]
    [InlineData(null, null)]
    public void ResolveChart_matches_only_a_generated_chart(string? value, string? expected) =>
        Assert.Equal(expected, KeyInferenceLink.ResolveChart(value, ["D1", "D2-US", "D10"]));
}
