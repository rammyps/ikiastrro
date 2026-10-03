using Dapper;

namespace Ikiastrro.Data.Statistics;

public sealed record PopulationComparison(
    byte HouseFromLagna,
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
    string SufficiencyCode);

public sealed record PopulationEvidenceSnapshot(
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
    IReadOnlyList<PopulationComparison> Comparisons)
{
    public bool IsAvailable => AvailabilityCode == "AVAILABLE";
}

/// <summary>
/// Reads the latest completed v10 descriptive-comparison run. It never computes astrology or
/// statistics and never exposes the anonymous SubjectKey outside this repository.
/// </summary>
public sealed class PopulationEvidenceRepository(SqlConnectionFactory connectionFactory)
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

    public PopulationEvidenceSnapshot GetLatestForBirthDetail(int birthDetailId)
    {
        using var connection = connectionFactory.CreateOpenConnection();
        var subject = connection.QuerySingleOrDefault<SubjectRow>("""
            SELECT SubjectKey, ResearchUseStatus, SubjectClassification
            FROM dbo.tbl_Dim_AnalyticsSubjects
            WHERE BirthDetailId = @birthDetailId
            """, new { birthDetailId });

        var eligiblePeople = connection.ExecuteScalar<int?>("""
            SELECT SUM(EligiblePeople) FROM dbo.vw_AnalyticsCohortReadiness
            """) ?? 0;

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
              AND dataset.DatasetCode = 'KI_D1_HOUSE_STRENGTH'
            ORDER BY run.CompletedAtUtc DESC, run.Id DESC
            """);

        if (subject is null)
            return Empty("NOT_ENROLLED", null, null);
        if (subject.ResearchUseStatus != "ELIGIBLE" ||
            subject.SubjectClassification is not ("RESEARCH" or "PERSONAL"))
            return Empty("NOT_ELIGIBLE", subject.ResearchUseStatus, subject.SubjectClassification);
        if (run is null)
            return Empty("NO_COMPLETED_RUN", subject.ResearchUseStatus, subject.SubjectClassification);

        var comparisons = connection.Query<PopulationComparison>("""
            SELECT HouseFromLagna, FeatureCode, PersonalValue, EligibleCount, MeasuredCount,
                   MissingRate, ReferenceMedian, ReferenceQ1, ReferenceQ3, RobustZ, Percentile,
                   MedianCiLow, MedianCiHigh, SufficiencyCode
            FROM dbo.tbl_Fact_StatisticalComparisons
            WHERE AnalyticsRunId = @analyticsRunId AND SubjectKey = @subjectKey
            ORDER BY HouseFromLagna, FeatureCode
            """, new { analyticsRunId = run.AnalyticsRunId, subjectKey = subject.SubjectKey }).ToList();

        return new PopulationEvidenceSnapshot(
            comparisons.Count == 0 ? "NO_COMPARISONS" : "AVAILABLE",
            subject.ResearchUseStatus,
            subject.SubjectClassification,
            eligiblePeople,
            run.AnalyticsRunId,
            run.DatasetCode,
            run.DatasetVersion,
            run.FeatureContractVersion,
            run.RuleSetId,
            run.CompletedAtUtc,
            comparisons);

        PopulationEvidenceSnapshot Empty(string code, string? status, string? classification) =>
            new(code, status, classification, eligiblePeople, run?.AnalyticsRunId,
                run?.DatasetCode, run?.DatasetVersion, run?.FeatureContractVersion,
                run?.RuleSetId, run?.CompletedAtUtc, []);
    }
}
