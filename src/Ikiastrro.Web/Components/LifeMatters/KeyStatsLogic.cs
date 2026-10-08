using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Data;
using Ikiastrro.Data.Statistics;

namespace Ikiastrro.Web.Components.LifeMatters;

/// <summary>Pure rules behind the POPULATION view: which houses to flag, what to filter, what is
/// still missing before statistics exist, and which Key Inference matter a house title opens.</summary>
public static class KeyStatsLogic
{
    public const int MinimumCohort = 30;

    public static bool IsThin(PopulationComparison c) => PopulationStrip.Tone(c) == "is-thin";

    public static bool IsNotable(PopulationComparison c) => c.RobustZ is { } z && Math.Abs(z) >= 1m;

    public static IEnumerable<PopulationComparison> Filter(IEnumerable<PopulationComparison> rows, string key) => key switch
    {
        "notable" => rows.Where(IsNotable),
        "thin" => rows.Where(IsThin),
        _ => rows
    };

    /// <summary>Highest or lowest published percentile; thin houses and unpublished percentiles never count.</summary>
    public static PopulationComparison? Extreme(IEnumerable<PopulationComparison> rows, bool highest)
    {
        var ranked = rows.Where(c => c.Percentile is not null && !IsThin(c)).ToList();
        return ranked.Count == 0 ? null : highest ? ranked.MaxBy(c => c.Percentile) : ranked.MinBy(c => c.Percentile);
    }

    public const string MatterSupportFeature = "KI_LM_SUPPORT_V1";

    public static bool IsThin(LifeMatterPopulationComparison c) => c.SufficiencyCode is "INSUFFICIENT" or "INCOMPLETE";

    public static string LensLabel(string lens) => lens switch
    {
        "D1_PROMISE" => "D1 promise",
        "VARGA_CONFIRMATION" => "Varga confirmation",
        _ => lens
    };

    /// <summary>
    /// Overall-support comparisons grouped by life matter, in the order the repository returned them
    /// (matter display order). Only the overall-support feature is shown; thin rows are kept so the
    /// reader sees the limited reference data rather than a silently shorter list.
    /// </summary>
    public static IReadOnlyList<(string Code, string Name, IReadOnlyList<LifeMatterPopulationComparison> Rows)> MatterGroups(
        IEnumerable<LifeMatterPopulationComparison> rows) =>
        rows.Where(c => c.FeatureCode == MatterSupportFeature)
            .GroupBy(c => (c.LifeMatterCode, c.LifeMatterName))
            .Select(g => (g.Key.LifeMatterCode, g.Key.LifeMatterName,
                (IReadOnlyList<LifeMatterPopulationComparison>)g
                    .OrderBy(c => c.EvidenceLensCode == "D1_PROMISE" ? 0 : 1)
                    .ThenBy(c => c.LifeMatterFocusId).ToList()))
            .ToList();

    public static string Reliability(LifeMatterPopulationComparison c)
    {
        var text = $"{c.MeasuredCount} of {c.EligibleCount} measured";
        if (c.ReferenceMedian is { } m)
        {
            text += $" · median {m:0.#}%";
            if (c.MedianCiLow is { } lo && c.MedianCiHigh is { } hi)
                text += $" (95% CI {lo:0.#}–{hi:0.#})";
        }
        return text;
    }

    public static readonly (string Scope, string Heading)[] DashaScopes =
    [
        ("EXAMPLE", "PVR examples"), ("CHART_THEME", "Divisional chart themes"),
        ("HOUSE", "House lords and occupants"), ("NATURAL_KARAKA", "Natural karakas")
    ];

    /// <summary>One dasha-matter rule for this person against the population.</summary>
    public sealed record DashaMatterItem(
        int Number, string Varga, string Statement, string Result, decimal? PlanetCount,
        decimal? PresentPercent, bool IsThin, string Reliability);

    private static readonly Lazy<IReadOnlyDictionary<int, DashaMatterRule>> DashaCatalogue = new(() =>
    {
        var none = new Dictionary<string, DashaMatterChart>();
        return DashaMatters.Evaluate(none, null).Concat(DashaMatters.EvaluateExtended(none))
            .ToDictionary(r => r.Number);
    });

    /// <summary>
    /// Natal dasha-matter rules grouped by scope. The rule wording comes from Core's own catalogue, so the page never
    /// restates a rule. "Present in X%" is the leave-one-out mean of the 0/100 presence feature, i.e. how many other
    /// comparable charts have at least one planet meeting the rule; it is a prevalence, not a score.
    /// </summary>
    public static IReadOnlyList<(string Heading, IReadOnlyList<DashaMatterItem> Items)> DashaGroups(
        IEnumerable<DashaMatterPopulationComparison> rows)
    {
        var byRule = rows.GroupBy(c => c.RuleNumber).ToList();
        var items = new List<(string Scope, DashaMatterItem Item)>();
        foreach (var rule in byRule.OrderBy(g => g.Key))
        {
            var count = rule.FirstOrDefault(c => c.FeatureCode == DashaMatterFeatures.TargetCountFeature);
            var present = rule.FirstOrDefault(c => c.FeatureCode == DashaMatterFeatures.PresentFeature);
            var head = count ?? present;
            if (head is null) continue;
            var known = DashaCatalogue.Value.TryGetValue(rule.Key, out var r);
            var thin = (present ?? head).SufficiencyCode is "INSUFFICIENT" or "INCOMPLETE";
            var text = $"{head.MeasuredCount} of {head.EligibleCount} measured";
            items.Add((head.ScopeKind, new DashaMatterItem(
                rule.Key, head.Varga, known ? r!.Statement : $"Rule {rule.Key}", known ? r!.Result : "",
                count?.PersonalValue, present?.ReferenceMean, thin, text)));
        }
        return DashaScopes
            .Select(s => (s.Heading, (IReadOnlyList<DashaMatterItem>)items.Where(i => i.Scope == s.Scope).Select(i => i.Item).ToList()))
            .Where(g => g.Item2.Count > 0).ToList();
    }

    public static IReadOnlyList<(bool Done, string Text)> UnlockSteps(PopulationEvidenceSnapshot snapshot)
    {
        var enrolled = snapshot.AvailabilityCode is not ("NOT_ENROLLED" or "NOT_ELIGIBLE");
        var enough = snapshot.EligiblePeople >= MinimumCohort;
        return
        [
            (enrolled, "Enrol this person on Saved Charts: Research classification plus the explicit-permission confirmation."),
            (enough, $"Reach at least {MinimumCohort} eligible people in the reference population (now {snapshot.EligiblePeople})."),
            (enough && snapshot.AnalyticsRunId is not null, "Complete a statistical analytics run over the eligible cohort.")
        ];
    }

    /// <summary>
    /// The matter a house title opens. Only Lagna-counted House foci count. Prefer the matter whose
    /// own primary house this is (lowest focus Priority), then the one that reads from the fewest
    /// houses (most specific to this house), then the app's own area and question order — never the alphabetical
    /// order of the step list, which would send house 1 to Career.
    /// </summary>
    public static IReadOnlyDictionary<int, string> BestMatterByHouse(
        IEnumerable<LifeMatterStepRow> steps, IEnumerable<LifeMatterFocusRule> foci,
        IEnumerable<LifeMatterCategoryRow> categories)
    {
        var areaOrder = categories.ToDictionary(c => c.CategoryCode, c => c.DisplayOrder);
        var byId = steps.Select(s => (Step: s, Index: (areaOrder.GetValueOrDefault(s.CategoryCode, int.MaxValue), s.DisplayOrder)))
            .ToDictionary(x => x.Step.LifeMatterId);
        var houseFoci = foci
            .Where(f => f.IsActive && f.FocusKind == LifeMatterFocusKind.House && f.HouseNumber is >= 1 and <= 12
                        && (f.ReferenceCode ?? "LAGNA") == "LAGNA" && byId.ContainsKey(f.LifeMatterId))
            .ToList();
        var houseCount = houseFoci.GroupBy(f => f.LifeMatterId)
            .ToDictionary(g => g.Key, g => g.Select(f => f.HouseNumber!.Value).Distinct().Count());

        return houseFoci
            .GroupBy(f => f.HouseNumber!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(f => f.LifeMatterId)
                    .Select(m => (Id: m.Key, Priority: m.Min(f => f.Priority)))
                    .OrderBy(m => m.Priority).ThenBy(m => houseCount[m.Id]).ThenBy(m => byId[m.Id].Index)
                    .Select(m => byId[m.Id].Step.LifeMatterCode)
                    .First());
    }
}

/// <summary>The three Key Inference views and their <c>?view=</c> names.</summary>
public static class KeyInferenceView
{
    public const string Reading = "READING";
    public const string Timing = "STATISTICAL";   // internal key kept from the original tab name
    public const string Population = "STATS";

    /// <summary>reading | timing | stats (any case). Unknown values open READING; null means "no change".</summary>
    public static string? Parse(string? view) => view is null ? null : view.ToLowerInvariant() switch
    {
        "stats" or "population" => Population,
        "timing" => Timing,
        _ => Reading
    };
}
