using Ikiastrro.Core.Engines.Dispositors;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

public class DispositorSummaryTests
{
    private static DispositorChain Chain(string planet, params string[] path)
        => new(planet, path, path[^1], false, "SELF_DISPOSED");

    private static DispositorPlanetFacts Fact(string planet, int house, string sign, string? dignity = null,
        bool combust = false, bool retro = false, decimal? pct = null, params int[] rules)
        => new(planet, house, sign, dignity, combust, retro, pct, rules);

    // A chart where Mars is in its own sign in the Lagna, the Moon is debilitated in Mars's sign,
    // and Jupiter reaches Mars through a weak, combust Mercury.
    private static (IReadOnlyList<DispositorChain>, IReadOnlyDictionary<string, DispositorPlanetFacts>) SingleAnchorChart()
    {
        var chains = new[]
        {
            Chain("Mars", "Mars", "Mars"),
            Chain("Mercury", "Mercury", "Mars", "Mars"),
            Chain("Jupiter", "Jupiter", "Mercury", "Mars", "Mars"),
            Chain("Moon", "Moon", "Mars", "Mars"),
        };
        var facts = new Dictionary<string, DispositorPlanetFacts>
        {
            ["Mars"] = Fact("Mars", 1, "Aries", "Moolatrikona", combust: true, pct: 195m, rules: new[] { 1, 8 }),
            ["Mercury"] = Fact("Mercury", 1, "Aries", "Enemy", combust: true, pct: 72m, rules: new[] { 3, 6 }),
            ["Jupiter"] = Fact("Jupiter", 6, "Virgo", null, retro: true, pct: 109m, rules: new[] { 9, 12 }),
            ["Moon"] = Fact("Moon", 8, "Scorpio", "Debilitated", pct: 119m, rules: new[] { 4 }),
        };
        return (chains, facts);
    }

    [Fact]
    public void A_single_final_dispositor_is_named_and_profiled()
    {
        var (chains, facts) = SingleAnchorChart();
        var text = string.Join("\n", DispositorSummary.Build(chains, facts));

        Assert.Contains("single final dispositor", text);
        Assert.Contains("Mars is in Aries (moolatrikona), in house 1, ruling the Lagna (1st) and 8th", text);
        Assert.Contains("195%", text);
        Assert.Contains("combust", text);
    }

    [Fact]
    public void A_debilitated_planet_whose_dispositor_is_in_a_kendra_is_flagged_as_rescued()
    {
        var (chains, facts) = SingleAnchorChart();
        var text = string.Join("\n", DispositorSummary.Build(chains, facts));

        Assert.Contains("Moon is debilitated, but its dispositor Mars stands in a kendra (house 1)", text);
    }

    [Fact]
    public void A_weak_or_combust_intermediate_planet_is_named_as_a_weak_link()
    {
        var (chains, facts) = SingleAnchorChart();
        var text = string.Join("\n", DispositorSummary.Build(chains, facts));

        Assert.Contains("Weak link: Mercury is combust and weak", text);
        Assert.Contains("Jupiter reach the final dispositor only through it", text);
    }

    [Fact]
    public void A_mutual_reception_is_described_as_a_shared_final_say()
    {
        var chains = new[]
        {
            new DispositorChain("Sun", new[] { "Sun", "Moon", "Sun" }, null, true, "MUTUAL_RECEPTION", new[] { "Sun", "Moon" }),
            new DispositorChain("Moon", new[] { "Moon", "Sun", "Moon" }, null, true, "MUTUAL_RECEPTION", new[] { "Moon", "Sun" }),
        };
        var text = string.Join("\n", DispositorSummary.Build(chains, new Dictionary<string, DispositorPlanetFacts>()));

        Assert.Contains("mutual reception", text);
        Assert.Single(DispositorSummary.Build(chains, new Dictionary<string, DispositorPlanetFacts>()));
    }
}
