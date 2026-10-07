using Dapper;

namespace Ikiastrro.Data.Statistics;

public sealed record LifeMatterPopulationComparison(
    int LifeMatterId,
    int LifeMatterFocusId,
    string LifeMatterCode,
    string LifeMatterName,
    string EvidenceLensCode,
    string ChartType,
    byte? HouseFromLagna,
    string FeatureCode,
    decimal? PersonalValue,
    int EligibleCount,
    int MeasuredCount,
    decimal MissingRate,
    decimal? ReferenceMedian,
    decimal? ReferenceQ1,
    decimal? ReferenceQ3,
    decimal? RobustZ,
    decimal? Percentile,
    decimal? MedianCiLow,
    decimal? MedianCiHigh,
    string SufficiencyCode,
    string? SourceMissingReasonCode);

public sealed record LifeMatterPopulationEvidenceSnapshot(
    string AvailabilityCode,
    string? ResearchUseStatus,
    string? SubjectClassification,
    int EligiblePeople,
    long? AnalyticsRunId,
    string? DatasetCode,
    int? DatasetVersion,
    string? FeatureContractVersion,
    int? RuleSetId,
    DateTime? CompletedAtUtc,
    IReadOnlyList<LifeMatterPopulationComparison> Comparisons)
{
    public bool IsAvailable => AvailabilityCode == "AVAILABLE";
}

/// <summary>
/// Reads the latest completed Life Matter/Varga descriptive run. It performs no astrology or
/// statistics and never exposes the anonymous SubjectKey outside this repository.
/// </summary>
public sealed class LifeMatterPopulationEvidenceRepository(SqlConnectionFactory connectionFactory)
{
    private sealed record SubjectRow(
        Guid SubjectKey,
        string ResearchUseStatus,
        string SubjectClassification);

    private sealed record RunRow(
        long AnalyticsRunId,
        string DatasetCode,
        int DatasetVersion,
        string FeatureContractVersion,
        byte RuleSetId,
        DateTime CompletedAtUtc);

    public LifeMatterPopulationEvidenceSnapshot GetLatestForBirthDetail(
        int birthDetailId,
        int? lifeMatterId = null)
    {
        using var connection = connectionFactory.CreateOpenConnection();
        var subject = connection.QuerySingleOrDefault<SubjectRow>("""
            SELECT SubjectKey, ResearchUseStatus, SubjectClassification
            FROM dbo.tbl_Dim_AnalyticsSubjects
            WHERE BirthDetailId = @birthDetailId
            """, new { birthDetailId });

        var eligiblePeople = connection.ExecuteScalar<int>("""
            SELECT COUNT(DISTINCT SubjectKey)
            FROM dbo.vw_AnalyticsLifeMatterFeaturesV1
            """);

        var run = connection.QuerySingleOrDefault<RunRow>("""
            SELECT TOP (1)
                   run.Id AS AnalyticsRunId,
                   dataset.DatasetCode,
                   dataset.DatasetVersion,
                   dataset.FeatureContractVersion,
                   dataset.RuleSetId,
                   run.CompletedAtUtc
            FROM dbo.tbl_Fact_AnalyticsRuns run
            JOIN dbo.tbl_Dim_AnalyticsDatasets dataset ON dataset.Id = run.DatasetId
            WHERE run.StatusCode = 'COMPLETED'
              AND dataset.DatasetCode = 'KI_LIFE_MATTER_VARGA'
            ORDER BY run.CompletedAtUtc DESC, run.Id DESC
            """);

        if (subject is null)
            return Empty("NOT_ENROLLED", null, null);
        if (subject.ResearchUseStatus != "ELIGIBLE" ||
            subject.SubjectClassification is not ("RESEARCH" or "PERSONAL"))
            return Empty("NOT_ELIGIBLE", subject.ResearchUseStatus, subject.SubjectClassification);
        if (run is null)
            return Empty("NO_COMPLETED_RUN", subject.ResearchUseStatus, subject.SubjectClassification);

        var comparisons = connection.Query<LifeMatterPopulationComparison>("""
            SELECT matter.Id AS LifeMatterId,
                   comparison.LifeMatterFocusId,
                   matter.Code AS LifeMatterCode,
                   matter.EnglishName AS LifeMatterName,
                   comparison.EvidenceLensCode,
                   comparison.ChartType,
                   comparison.HouseFromLagna,
                   comparison.FeatureCode,
                   comparison.PersonalValue,
                   comparison.EligibleCount,
                   comparison.MeasuredCount,
                   comparison.MissingRate,
                   comparison.ReferenceMedian,
                   comparison.ReferenceQ1,
                   comparison.ReferenceQ3,
                   comparison.RobustZ,
                   comparison.Percentile,
                   comparison.MedianCiLow,
                   comparison.MedianCiHigh,
                   comparison.SufficiencyCode,
                   comparison.SourceMissingReasonCode
            FROM dbo.tbl_Fact_LifeMatterStatisticalComparisons comparison
            JOIN dbo.tbl_Rule_LifeMatterFocus focus ON focus.Id = comparison.LifeMatterFocusId
            JOIN dbo.tbl_Dim_LifeMatter matter ON matter.Id = focus.LifeMatterId
            WHERE comparison.AnalyticsRunId = @analyticsRunId
              AND comparison.SubjectKey = @subjectKey
              AND (@lifeMatterId IS NULL OR matter.Id = @lifeMatterId)
            ORDER BY matter.DisplayOrder, comparison.LifeMatterFocusId,
                     comparison.EvidenceLensCode, comparison.FeatureCode
            """, new
            {
                analyticsRunId = run.AnalyticsRunId,
                subjectKey = subject.SubjectKey,
                lifeMatterId
            }).ToList();

        return new LifeMatterPopulationEvidenceSnapshot(
            comparisons.Count == 0 ? "NO_COMPARISONS" : "AVAILABLE",
            subject.ResearchUseStatus, subject.SubjectClassification, eligiblePeople,
            run.AnalyticsRunId, run.DatasetCode, run.DatasetVersion,
            run.FeatureContractVersion, run.RuleSetId, run.CompletedAtUtc, comparisons);

        LifeMatterPopulationEvidenceSnapshot Empty(string code, string? status, string? classification) =>
            new(code, status, classification, eligiblePeople, run?.AnalyticsRunId,
                run?.DatasetCode, run?.DatasetVersion, run?.FeatureContractVersion,
                run?.RuleSetId, run?.CompletedAtUtc, []);
    }
}
