using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;

namespace Ikiastrro.Core.LifeMatters.Activation;

/// <summary>The explicit DashaMatters selection for one stable LifeMatter identity.</summary>
public sealed record LifeMatterDashaRuleSelection(
    int LifeMatterId,
    string PrimaryChart,
    IReadOnlyList<DashaMatterRule> Rules,
    IReadOnlyList<string> MissingMappings)
{
    public bool IsMapped => Rules.Count > 0;
}

/// <summary>
/// Joins the canonical LifeMatter Subject/Focus mapping to structured DashaMatters scopes.
/// It never searches MatterText, Statement or Result, so wording changes cannot alter inference.
/// D1 remains the natal gate; this mapper selects timing rules from the relevant confirmation
/// varga, or D1 when the matter has no divisional subject.
/// </summary>
public static class LifeMatterDashaRuleMapper
{
    public static LifeMatterDashaRuleSelection Select(
        ResolvedLifeMatterFocus matter,
        IEnumerable<DashaMatterRule> availableRules)
    {
        ArgumentNullException.ThrowIfNull(matter);
        ArgumentNullException.ThrowIfNull(availableRules);

        var chart = string.IsNullOrWhiteSpace(matter.Subject?.ChartTypeCode)
            ? "D1"
            : matter.Subject.ChartTypeCode.ToUpperInvariant();
        var rules = availableRules.Where(rule =>
                string.Equals(rule.Varga, chart, StringComparison.OrdinalIgnoreCase) &&
                rule.Scope is not null)
            .ToList();

        var selected = new List<DashaMatterRule>();
        selected.AddRange(rules.Where(rule => rule.Scope!.Kind == DashaMatterScopeKind.ChartTheme));

        var houses = matter.HouseAndSpecialPointFoci
            .Where(focus => focus.FocusKind == LifeMatterFocusKind.House && focus.HouseNumber is not null)
            .Select(focus => focus.HouseNumber!.Value)
            .ToHashSet();
        selected.AddRange(rules.Where(rule =>
            rule.Scope is { Kind: DashaMatterScopeKind.House, House: { } house } && houses.Contains(house)));

        var karakas = matter.Karakas
            .Select(rule => FixedGraha(rule.KarakaCode))
            .Where(planet => planet is not null)
            .Select(planet => planet!.Value)
            .ToHashSet();
        selected.AddRange(rules.Where(rule =>
            rule.Scope is { Kind: DashaMatterScopeKind.NaturalKaraka, Karaka: { } karaka } && karakas.Contains(karaka)));

        var finalRules = selected.DistinctBy(rule => rule.Number).OrderBy(rule => rule.Number).ToList();
        var missing = new List<string>();
        if (!rules.Any(rule => rule.Scope!.Kind == DashaMatterScopeKind.ChartTheme))
            missing.Add($"No chart-theme dasha rule is available for {chart}.");
        foreach (var house in houses.Where(house => !rules.Any(rule =>
                     rule.Scope is { Kind: DashaMatterScopeKind.House, House: { } mapped } && mapped == house)))
            missing.Add($"No {chart} house-{house} dasha rule is available.");
        foreach (var karaka in karakas.Where(karaka => !rules.Any(rule =>
                     rule.Scope is { Kind: DashaMatterScopeKind.NaturalKaraka, Karaka: { } mapped } && mapped == karaka)))
            missing.Add($"No {chart} natural-karaka dasha rule is available for {karaka}.");

        return new LifeMatterDashaRuleSelection(matter.LifeMatterId, chart, finalRules, missing);
    }

    private static PlanetName? FixedGraha(string code)
    {
        const string prefix = "GRAHA_";
        if (!code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;

        return Enum.TryParse<PlanetName>(code[prefix.Length..], true, out var planet)
            ? planet
            : null;
    }
}
