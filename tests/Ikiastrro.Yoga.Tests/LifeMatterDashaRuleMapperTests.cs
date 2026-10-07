using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Core.LifeMatters.Activation;

namespace Ikiastrro.Yoga.Tests;

public sealed class LifeMatterDashaRuleMapperTests
{
    [Fact]
    public void Selects_theme_house_and_fixed_karaka_from_relevant_varga()
    {
        var matter = Matter("D10", houses: [10], karakas: ["GRAHA_SUN"]);
        var selection = LifeMatterDashaRuleMapper.Select(matter,
        [
            Rule(10, "D10", new(DashaMatterScopeKind.ChartTheme)),
            Rule(11, "D10", new(DashaMatterScopeKind.House, House: 10)),
            Rule(12, "D10", new(DashaMatterScopeKind.NaturalKaraka, Karaka: PlanetName.Sun)),
            Rule(13, "D1", new(DashaMatterScopeKind.House, House: 10)),
        ]);

        Assert.Equal("D10", selection.PrimaryChart);
        Assert.Equal([10, 11, 12], selection.Rules.Select(rule => rule.Number));
        Assert.Empty(selection.MissingMappings);
    }

    [Fact]
    public void Defaults_to_d1_when_no_divisional_subject_exists()
    {
        var selection = LifeMatterDashaRuleMapper.Select(
            Matter(chart: null, houses: [2], karakas: []),
            [Rule(1, "D1", new(DashaMatterScopeKind.ChartTheme)),
             Rule(2, "D1", new(DashaMatterScopeKind.House, House: 2))]);

        Assert.Equal("D1", selection.PrimaryChart);
        Assert.Equal([1, 2], selection.Rules.Select(rule => rule.Number));
    }

    [Fact]
    public void Dynamic_chara_karaka_is_not_misread_as_fixed_planet()
    {
        var selection = LifeMatterDashaRuleMapper.Select(
            Matter("D9", houses: [], karakas: ["KARAKA_AK"]),
            [Rule(1, "D9", new(DashaMatterScopeKind.ChartTheme)),
             Rule(2, "D9", new(DashaMatterScopeKind.NaturalKaraka, Karaka: PlanetName.Sun))]);

        Assert.Equal([1], selection.Rules.Select(rule => rule.Number));
        Assert.Empty(selection.MissingMappings);
    }

    [Fact]
    public void Reports_structured_gaps_instead_of_guessing_from_rule_text()
    {
        var selection = LifeMatterDashaRuleMapper.Select(
            Matter("D7", houses: [5], karakas: ["GRAHA_JUPITER"]),
            [Rule(1, "D7", new(DashaMatterScopeKind.ChartTheme))]);

        Assert.True(selection.IsMapped);
        Assert.Contains("No D7 house-5 dasha rule is available.", selection.MissingMappings);
        Assert.Contains("No D7 natural-karaka dasha rule is available for Jupiter.", selection.MissingMappings);
    }

    [Fact]
    public void No_available_scope_is_explicitly_unmapped()
    {
        var selection = LifeMatterDashaRuleMapper.Select(
            Matter("D6", houses: [6], karakas: []), []);

        Assert.False(selection.IsMapped);
        Assert.Contains("No chart-theme dasha rule is available for D6.", selection.MissingMappings);
        Assert.Contains("No D6 house-6 dasha rule is available.", selection.MissingMappings);
    }

    [Fact]
    public void Extended_dasha_rules_publish_machine_readable_scopes_even_when_chart_is_missing()
    {
        var rules = DashaMatters.EvaluateExtended(new Dictionary<string, DashaMatterChart>());

        Assert.Contains(rules, rule =>
            rule.Varga == "D10" && rule.Scope?.Kind == DashaMatterScopeKind.ChartTheme);
        Assert.Contains(rules, rule =>
            rule.Varga == "D10" &&
            rule.Scope is { Kind: DashaMatterScopeKind.House, House: 10 });
        Assert.Contains(rules, rule =>
            rule.Varga == "D10" &&
            rule.Scope is { Kind: DashaMatterScopeKind.NaturalKaraka, Karaka: PlanetName.Sun });
        Assert.All(rules, rule => Assert.NotNull(rule.Scope));
    }

    private static ResolvedLifeMatterFocus Matter(
        string? chart, IReadOnlyList<int> houses, IReadOnlyList<string> karakas)
    {
        const int matterId = 42;
        var subject = chart is null ? null : new LifeMatterSubjectRule(1, 1, matterId, "TEST", chart);
        var karakaRules = karakas.Select((code, index) =>
            new LifeMatterKarakaRule(index + 1, 1, matterId, index + 1, code, index + 1)).ToList();
        var foci = houses.Select((house, index) =>
            new LifeMatterFocusRule(index + 1, 1, matterId, LifeMatterFocusKind.House,
                "LAGNA", house, null, index + 1)).ToList();
        return new ResolvedLifeMatterFocus(matterId, subject, karakaRules, foci);
    }

    private static DashaMatterRule Rule(int number, string varga, DashaMatterScope scope) =>
        new(number, varga, "structured statement", "structured result",
            [new DashaMatterPlanet(PlanetName.Jupiter, "test")], null, null, "test", scope);
}
