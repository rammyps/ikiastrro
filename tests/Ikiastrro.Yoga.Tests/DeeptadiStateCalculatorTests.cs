using Ikiastrro.Core.Engines.PlanetaryStates;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>DeeptadiStateCalculator (PVR sec 15.4.3's 9 states) — no DB dependency, a hand-built
/// PlanetaryStateRuleSet stands in for PlanetaryStateRuleRepository.</summary>
public class DeeptadiStateCalculatorTests
{
    private static readonly PlanetaryStateRuleSet Rules = Build();

    private static PlanetaryStateRuleSet Build()
    {
        byte id = 1;
        PlanetaryStateRow Row(string system, string name) => new(id++, system, name, 1, null);

        var deepta = Row("Deeptadi", "Deepta");
        var swastha = Row("Deeptadi", "Swastha");
        var mudita = Row("Deeptadi", "Mudita");
        var saanta = Row("Deeptadi", "Saanta");
        var deena = Row("Deeptadi", "Deena");
        var duhkhita = Row("Deeptadi", "Duhkhita");
        var vikala = Row("Deeptadi", "Vikala");
        var khala = Row("Deeptadi", "Khala");
        var kopita = Row("Deeptadi", "Kopita");

        var byName = new[] { deepta, swastha, mudita, saanta, deena, duhkhita, vikala, khala, kopita }
            .ToDictionary(r => r.StateName, r => r);

        byte ruleId = 1;
        DeeptadiStateRuleRow Rule(string dignity, PlanetaryStateRow state) =>
            new(ruleId++, 1, dignity, state.Id, state.StateName);

        var byDignity = new[]
        {
            Rule("Exalted", deepta), Rule("Moolatrikona", swastha), Rule("Own Sign", swastha),
            Rule("Great Friend", mudita), Rule("Friend", saanta), Rule("Neutral", deena),
            Rule("Enemy", duhkhita), Rule("Great Enemy", duhkhita), Rule("Debilitated", duhkhita),
        }.ToDictionary(r => r.DignityStatus, r => r);

        return new PlanetaryStateRuleSet(1, new List<AgeStateRuleRow>(),
            new Dictionary<string, WakefulnessStateRuleRow>(),
            new Dictionary<byte, PlanetaryStateRow>(),
            byDignity, byName, new Dictionary<string, PlanetaryStateRow>());
    }

    private static string[] Names(List<byte> ids) =>
        ids.Select(id => Rules.DeeptadiStatesByName.Values.Single(r => r.Id == id).StateName).OrderBy(n => n).ToArray();

    [Fact]
    public void Exalted_alone_gives_only_deepta()
    {
        var ids = DeeptadiStateCalculator.For("Exalted", "Sun", Array.Empty<string>(), _ => false, false, Rules);
        Assert.Equal(new[] { "Deepta" }, Names(ids));
    }

    [Fact]
    public void Own_sign_conjunct_a_malefic_gives_swastha_and_vikala()
    {
        var ids = DeeptadiStateCalculator.For("Own Sign", "Moon", new[] { "Saturn" }, p => p == "Saturn", false, Rules);
        Assert.Equal(new[] { "Swastha", "Vikala" }, Names(ids));
    }

    [Fact]
    public void Neutral_with_a_malefic_sign_lord_and_combust_stacks_three_states()
    {
        // PVR gives no precedence between the dignity tier and the affliction flags — all three
        // should be kept at once (the exact ambiguity the book leaves unresolved).
        var ids = DeeptadiStateCalculator.For("Neutral", "Saturn", Array.Empty<string>(), p => p == "Saturn", true, Rules);
        Assert.Equal(new[] { "Deena", "Khala", "Kopita" }, Names(ids));
    }

    [Fact]
    public void No_dignity_no_affliction_gives_nothing()
    {
        var ids = DeeptadiStateCalculator.For(null, "Jupiter", Array.Empty<string>(), _ => false, false, Rules);
        Assert.Empty(ids);
    }

    [Fact]
    public void Null_sign_lord_skips_khala_without_throwing()
    {
        var ids = DeeptadiStateCalculator.For("Friend", null, Array.Empty<string>(), _ => true, false, Rules);
        Assert.Equal(new[] { "Saanta" }, Names(ids));
    }
}
