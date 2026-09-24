using Ikiastrro.Core.Engines.PlanetaryStates;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>LajjitadiStateCalculator (PVR sec 15.4.3's unnamed 6-state group) — no DB dependency, a
/// hand-built PlanetaryStateRuleSet stands in for PlanetaryStateRuleRepository.</summary>
public class LajjitadiStateCalculatorTests
{
    private static readonly PlanetaryStateRuleSet Rules = Build();

    private static PlanetaryStateRuleSet Build()
    {
        byte id = 1;
        PlanetaryStateRow Row(string name) => new(id++, "Lajjitadi", name, 1, null);
        var byName = new[] { "Lajjita", "Garvita", "Kshudhita", "Trishita", "Mudita", "Kshobhita" }
            .Select(Row).ToDictionary(r => r.StateName, r => r);

        return new PlanetaryStateRuleSet(1, new List<AgeStateRuleRow>(),
            new Dictionary<string, WakefulnessStateRuleRow>(),
            new Dictionary<byte, PlanetaryStateRow>(),
            new Dictionary<string, DeeptadiStateRuleRow>(),
            new Dictionary<string, PlanetaryStateRow>(), byName);
    }

    private static string[] Names(List<byte> ids) =>
        ids.Select(id => Rules.LajjitadiStatesByName.Values.Single(r => r.Id == id).StateName).OrderBy(n => n).ToArray();

    [Fact]
    public void Fifth_house_conjunct_mars_gives_lajjita()
    {
        // Mars, not Saturn, so Kshudhita's separate "conjoined by Saturn" clause doesn't also fire —
        // isolates Lajjita's own condition.
        var ids = LajjitadiStateCalculator.For(
            "Leo", "Neutral", houseNumberFromLagna: 5, conjunctPlanets: new[] { "Mars" },
            aspectingPlanets: Array.Empty<string>(), isNaturalMalefic: p => p == "Mars",
            relationshipOf: _ => null, Rules);
        Assert.Equal(new[] { "Lajjita" }, Names(ids));
    }

    [Fact]
    public void Exalted_and_conjunct_jupiter_gives_garvita_and_mudita_together()
    {
        // The exact ambiguity PVR leaves unresolved: two independent conditions from this 6-state
        // group can hold for the same planet at once.
        var ids = LajjitadiStateCalculator.For(
            "Cancer", "Exalted", houseNumberFromLagna: 3, conjunctPlanets: new[] { "Jupiter" },
            aspectingPlanets: Array.Empty<string>(), isNaturalMalefic: _ => false,
            relationshipOf: _ => null, Rules);
        Assert.Equal(new[] { "Garvita", "Mudita" }, Names(ids));
    }

    [Fact]
    public void Enemy_sign_alone_gives_kshudhita()
    {
        var ids = LajjitadiStateCalculator.For(
            "Libra", "Enemy", houseNumberFromLagna: 8, conjunctPlanets: Array.Empty<string>(),
            aspectingPlanets: Array.Empty<string>(), isNaturalMalefic: _ => false,
            relationshipOf: _ => null, Rules);
        Assert.Equal(new[] { "Kshudhita" }, Names(ids));
    }

    [Fact]
    public void Watery_sign_aspected_only_by_an_enemy_gives_trishita_and_kshudhita()
    {
        // "Aspected by an enemy" alone also satisfies Kshudhita's own "conjoined/aspected by
        // enemies" clause — the two conditions genuinely overlap here, both fire, no precedence.
        var ids = LajjitadiStateCalculator.For(
            "Cancer", "Neutral", houseNumberFromLagna: 2, conjunctPlanets: Array.Empty<string>(),
            aspectingPlanets: new[] { "Mars" }, isNaturalMalefic: p => p == "Mars",
            relationshipOf: p => p == "Mars" ? "Enemy" : null, Rules);
        Assert.Equal(new[] { "Kshudhita", "Trishita" }, Names(ids));
    }

    [Fact]
    public void Watery_sign_aspected_by_an_enemy_and_a_benefic_does_not_give_trishita()
    {
        // PVR: "aspected by enemies WITHOUT the aspect of benefics" — a benefic aspect cancels it.
        var ids = LajjitadiStateCalculator.For(
            "Pisces", "Neutral", houseNumberFromLagna: 2, conjunctPlanets: Array.Empty<string>(),
            aspectingPlanets: new[] { "Mars", "Jupiter" }, isNaturalMalefic: p => p == "Mars",
            relationshipOf: p => p == "Mars" ? "Enemy" : "Friend", Rules);
        Assert.DoesNotContain(ids, id => Rules.LajjitadiStatesByName["Trishita"].Id == id);
    }

    [Fact]
    public void Conjunct_sun_and_aspected_by_a_malefic_gives_kshobhita()
    {
        var ids = LajjitadiStateCalculator.For(
            "Aries", "Own Sign", houseNumberFromLagna: 1, conjunctPlanets: new[] { "Sun" },
            aspectingPlanets: new[] { "Saturn" }, isNaturalMalefic: p => p == "Saturn",
            relationshipOf: _ => null, Rules);
        Assert.Equal(new[] { "Kshobhita" }, Names(ids));
    }

    [Fact]
    public void No_conditions_met_gives_nothing()
    {
        var ids = LajjitadiStateCalculator.For(
            "Taurus", "Neutral", houseNumberFromLagna: 6, conjunctPlanets: Array.Empty<string>(),
            aspectingPlanets: Array.Empty<string>(), isNaturalMalefic: _ => false,
            relationshipOf: _ => null, Rules);
        Assert.Empty(ids);
    }
}
