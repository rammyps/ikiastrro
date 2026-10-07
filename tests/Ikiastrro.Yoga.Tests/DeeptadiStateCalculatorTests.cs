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

    private static string[] Run(string? dignity, string? relation, string? lord, string[] conjunct, decimal? sunDistance) =>
        Names(DeeptadiStateCalculator.For(dignity, relation, lord, conjunct, sunDistance, Rules));

    [Fact]
    public void Exalted_planet_keeps_its_relationship_tier_to_the_sign_lord()
    {
        // Sun exalted in Aries (lord Mars: natural friend, temporary enemy -> Neutral): JHora prints Deepta, Deena.
        Assert.Equal(new[] { "Deena", "Deepta", "Khala" }, Run("Exalted", "Neutral", "Mars", Array.Empty<string>(), null));
    }

    [Fact]
    public void Own_sign_has_no_relationship_tier()
    {
        Assert.Equal(new[] { "Swastha" }, Run("Own Sign", null, "Moon", Array.Empty<string>(), null));
    }

    [Fact]
    public void Vikala_needs_two_malefics()
    {
        Assert.Empty(Run(null, null, "Jupiter", new[] { "Saturn" }, null));
        Assert.Equal(new[] { "Vikala" }, Run(null, null, "Jupiter", new[] { "Saturn", "Rahu" }, null));
    }

    [Fact]
    public void Moon_and_Mercury_are_never_malefic_for_vikala_or_khala()
    {
        Assert.Empty(Run(null, null, "Mercury", new[] { "Moon", "Mercury" }, null));
        Assert.Empty(Run(null, null, "Moon", Array.Empty<string>(), null));
    }

    [Fact]
    public void Khala_when_the_sign_lord_is_sun_mars_or_saturn()
    {
        foreach (var lord in new[] { "Sun", "Mars", "Saturn" })
            Assert.Equal(new[] { "Khala" }, Run(null, null, lord, Array.Empty<string>(), null));
    }

    [Fact]
    public void Kopita_inside_the_orb_only()
    {
        Assert.Equal(new[] { "Kopita" }, Run(null, null, "Jupiter", Array.Empty<string>(), 4.25m));
        Assert.Empty(Run(null, null, "Jupiter", Array.Empty<string>(), 6.4m));
    }

    [Fact]
    public void Relationship_tier_maps_each_panchadha_value()
    {
        Assert.Equal(new[] { "Mudita" }, Run(null, "Great Friend", "Jupiter", Array.Empty<string>(), null));
        Assert.Equal(new[] { "Saanta" }, Run(null, "Friend", "Jupiter", Array.Empty<string>(), null));
        Assert.Equal(new[] { "Duhkhita" }, Run(null, "Great Enemy", "Jupiter", Array.Empty<string>(), null));
    }

    [Fact]
    public void Node_without_a_relation_falls_back_to_its_dignity_label()
    {
        Assert.Equal(new[] { "Deena" }, Run("Neutral", null, "Venus", Array.Empty<string>(), null));
    }

    [Fact]
    public void No_dignity_no_affliction_gives_nothing()
    {
        Assert.Empty(Run(null, null, "Jupiter", Array.Empty<string>(), null));
    }

    [Fact]
    public void Null_sign_lord_skips_khala_without_throwing()
    {
        Assert.Equal(new[] { "Saanta" }, Run("Friend", "Friend", null, Array.Empty<string>(), null));
    }
}
