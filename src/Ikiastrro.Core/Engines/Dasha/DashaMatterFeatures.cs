namespace Ikiastrro.Core.Engines.Dasha;

/// <summary>
/// One natal dasha-matter rule reduced to the numbers the v10 population statistics compare.
/// <see cref="TargetCount"/> is null (never zero) when the rule could not be evaluated, with the reason in
/// <see cref="MissingReasonCode"/>.
/// </summary>
public sealed record DashaMatterFeature(
    int RuleNumber, string Varga, string ScopeKind, int? House, string? KarakaCode,
    int? TargetCount, string? MissingReasonCode);

/// <summary>
/// Feature contract <c>KI_DASHA_MATTER_V1</c>. Describes only the natal rule structure (how many planets meet each
/// dasha-matter rule); the running period is deliberately excluded because it changes daily and would make a
/// statistical run irreproducible. Pure; no I/O.
/// </summary>
public static class DashaMatterFeatures
{
    public const string ContractVersion = "KI_DASHA_MATTER_V1";
    public const string TargetCountFeature = "KI_DM_TARGET_COUNT_V1";
    public const string PresentFeature = "KI_DM_PRESENT_V1";

    public static IReadOnlyList<DashaMatterFeature> Build(IEnumerable<DashaMatterRule> rules) =>
        rules.Select(rule => new DashaMatterFeature(
            rule.Number, rule.Varga, ScopeKind(rule.Scope), rule.Scope?.House,
            rule.Scope?.Karaka?.ToString().ToUpperInvariant(),
            rule.NotEvaluated is null ? rule.Planets.Count : null,
            rule.NotEvaluated is null ? null : MissingReason(rule.NotEvaluated))).ToList();

    /// <summary>PVR's nine examples carry no scope; they are stored as EXAMPLE.</summary>
    public static string ScopeKind(DashaMatterScope? scope) => scope?.Kind switch
    {
        null or DashaMatterScopeKind.Example => "EXAMPLE",
        DashaMatterScopeKind.ChartTheme => "CHART_THEME",
        DashaMatterScopeKind.House => "HOUSE",
        _ => "NATURAL_KARAKA"
    };

    private static string MissingReason(string notEvaluated) =>
        notEvaluated.StartsWith("Needs the D", StringComparison.Ordinal) && notEvaluated.EndsWith("chart.", StringComparison.Ordinal)
            ? "CHART_NOT_AVAILABLE"
            : "SOURCE_PARTIAL";
}
