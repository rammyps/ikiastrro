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
    /// houses (most specific to this house), then the reference order — never the alphabetical
    /// category order, which would send house 1 to Career.
    /// </summary>
    public static IReadOnlyDictionary<int, string> BestMatterByHouse(
        IEnumerable<LifeMatterStepRow> steps, IEnumerable<LifeMatterFocusRule> foci)
    {
        var byId = steps.Select((s, i) => (Step: s, Index: i)).ToDictionary(x => x.Step.LifeMatterId);
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
