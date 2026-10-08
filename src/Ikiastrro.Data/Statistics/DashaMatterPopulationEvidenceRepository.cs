using Dapper;

namespace Ikiastrro.Data.Statistics;

public sealed record DashaMatterPopulationComparison(
    int RuleNumber,
    string Varga,
    string ScopeKind,
    byte? HouseNumber,
    string? KarakaCode,
    string FeatureCode,
    decimal? PersonalValue,
    int EligibleCount,
    int MeasuredCount,
    decimal MissingRate,
    decimal? ReferenceMedian,
    decimal? ReferenceMean,
    decimal? Percentile,
    decimal? MedianCiLow,
    decimal? MedianCiHigh,
    string SufficiencyCode,
    string? SourceMissingReasonCode);

public sealed record DashaMatterPopulationEvidenceSnapshot(
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
    IReadOnlyList<DashaMatterPopulationComparison> Comparisons)
{
    public bool IsAvailable => AvailabilityCode == "AVAILABLE";
}

/// <summary>
/// Reads the latest completed natal Dasha Matter descriptive run (<c>KI_DASHA_MATTER</c>). It performs no astrology or
/// statistics and never exposes the anonymous SubjectKey outside this repository.
/// </summary>
public sealed class DashaMatterPopulationEvidenceRepository(SqlConnectionFactory connectionFactory)
{
    private sealed record SubjectRow(Guid SubjectKey, string ResearchUseStatus, string SubjectClassification);

    private sealed record RunRow(
        long AnalyticsRunId, string DatasetCode, int DatasetVersion, string FeatureContractVersion,
        byte RuleSetId, DateTime CompletedAtUtc);

    public DashaMatterPopulationEvidenceSnapshot GetLatestForBirthDetail(int birthDetailId)
    {
        using var connection = connectionFactory.CreateOpenConnection();
        var subject = connection.QuerySingleOrDefault<SubjectRow>("""
            SELECT SubjectKey, ResearchUseStatus, SubjectClassification
            FROM dbo.tbl_Dim_AnalyticsSubjects
            WHERE BirthDetailId = @birthDetailId
            """, new { birthDetailId });

        var eligiblePeople = connection.ExecuteScalar<int>("""
            SELECT COUNT(DISTINCT SubjectKey) FROM dbo.vw_AnalyticsDashaMatterFeaturesV1
            """);

        var run = connection.QuerySingleOrDefault<RunRow>("""
            SELECT TOP (1)
                   run.Id AS AnalyticsRunId, dataset.DatasetCode, dataset.DatasetVersion,
                   dataset.FeatureContractVersion, dataset.RuleSetId, run.CompletedAtUtc
            FROM dbo.tbl_Fact_AnalyticsRuns run
            JOIN dbo.tbl_Dim_AnalyticsDatasets dataset ON dataset.Id = run.DatasetId
            WHERE run.StatusCode = 'COMPLETED' AND dataset.DatasetCode = 'KI_DASHA_MATTER'
            ORDER BY run.CompletedAtUtc DESC, run.Id DESC
            """);

        if (subject is null)
            return Empty("NOT_ENROLLED", null, null);
        if (subject.ResearchUseStatus != "ELIGIBLE" ||
            subject.SubjectClassification is not ("RESEARCH" or "PERSONAL"))
            return Empty("NOT_ELIGIBLE", subject.ResearchUseStatus, subject.SubjectClassification);
        if (run is null)
            return Empty("NO_COMPLETED_RUN", subject.ResearchUseStatus, subject.SubjectClassification);

        var comparisons = connection.Query<DashaMatterPopulationComparison>("""
            SELECT RuleNumber, Varga, ScopeKind, HouseNumber, KarakaCode, FeatureCode, PersonalValue,
                   EligibleCount, MeasuredCount, MissingRate, ReferenceMedian, ReferenceMean, Percentile,
                   MedianCiLow, MedianCiHigh, SufficiencyCode, SourceMissingReasonCode
            FROM dbo.tbl_Fact_DashaMatterStatisticalComparisons
            WHERE AnalyticsRunId = @analyticsRunId AND SubjectKey = @subjectKey
            ORDER BY RuleNumber, FeatureCode
            """, new { analyticsRunId = run.AnalyticsRunId, subjectKey = subject.SubjectKey }).ToList();

        return new DashaMatterPopulationEvidenceSnapshot(
            comparisons.Count == 0 ? "NO_COMPARISONS" : "AVAILABLE",
            subject.ResearchUseStatus, subject.SubjectClassification, eligiblePeople,
            run.AnalyticsRunId, run.DatasetCode, run.DatasetVersion,
            run.FeatureContractVersion, run.RuleSetId, run.CompletedAtUtc, comparisons);

        DashaMatterPopulationEvidenceSnapshot Empty(string code, string? status, string? classification) =>
            new(code, status, classification, eligiblePeople, run?.AnalyticsRunId,
                run?.DatasetCode, run?.DatasetVersion, run?.FeatureContractVersion,
                run?.RuleSetId, run?.CompletedAtUtc, []);
    }
}
