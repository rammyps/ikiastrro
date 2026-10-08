using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;

namespace Ikiastrro.Yoga.Tests;

public sealed class DashaMatterFeaturesTests
{
    private static DashaMatterRule Rule(int number, DashaMatterScope? scope, int planets, string? notEvaluated = null) =>
        new(number, "D9", "statement", "result",
            Enumerable.Range(0, planets).Select(i => new DashaMatterPlanet((PlanetName)i, "why")).ToList(),
            notEvaluated, null, "", scope);

    [Fact]
    public void Count_is_the_number_of_planets_meeting_the_rule_and_zero_is_a_real_zero()
    {
        var features = DashaMatterFeatures.Build([Rule(1, null, 2), Rule(2, null, 0)]);
        Assert.Equal([2, 0], features.Select(f => f.TargetCount));
        Assert.All(features, f => Assert.Null(f.MissingReasonCode));
    }

    [Fact]
    public void Unevaluated_rule_is_missing_not_zero()
    {
        var features = DashaMatterFeatures.Build([
            Rule(4, null, 0, "Needs the Atma Karaka."), Rule(5, null, 0, "Needs the D9 chart.")]);
        Assert.All(features, f => Assert.Null(f.TargetCount));
        Assert.Equal(["SOURCE_PARTIAL", "CHART_NOT_AVAILABLE"], features.Select(f => f.MissingReasonCode));
    }

    [Fact]
    public void Scope_house_and_karaka_are_carried_through()
    {
        var features = DashaMatterFeatures.Build([
            Rule(1, null, 1), Rule(10, new(DashaMatterScopeKind.ChartTheme), 1),
            Rule(30, new(DashaMatterScopeKind.House, House: 7), 1),
            Rule(31, new(DashaMatterScopeKind.NaturalKaraka, Karaka: PlanetName.Venus), 1)]);
        Assert.Equal(["EXAMPLE", "CHART_THEME", "HOUSE", "NATURAL_KARAKA"], features.Select(f => f.ScopeKind));
        Assert.Equal(7, features[2].House);
        Assert.Equal("VENUS", features[3].KarakaCode);
    }

    [Fact]
    public void Every_rule_of_the_real_engine_maps_to_a_feature_with_a_unique_number()
    {
        var none = new Dictionary<string, DashaMatterChart>();
        var features = DashaMatterFeatures.Build([.. DashaMatters.Evaluate(none, null), .. DashaMatters.EvaluateExtended(none)]);
        Assert.Equal(features.Count, features.Select(f => f.RuleNumber).Distinct().Count());
        Assert.All(features, f => Assert.Null(f.TargetCount));
        // Rule 4 is blocked on the Atma Karaka before it looks for its chart.
        Assert.All(features.Where(f => f.RuleNumber != 4), f => Assert.Equal("CHART_NOT_AVAILABLE", f.MissingReasonCode));
        Assert.Equal("SOURCE_PARTIAL", features.Single(f => f.RuleNumber == 4).MissingReasonCode);
    }
}
