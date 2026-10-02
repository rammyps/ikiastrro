using Dapper;

namespace Ikiastrro.Data;

public sealed record YogaEvaluationRow(
    string SourceRefCode, string YogaCode, bool? Present, string EvaluationStatus,
    string? YogaTypeCode, string? YogaRule, string? Notes,
    string SourceVariantCode = "", string? SourceLocator = null,
    string? YogaSetCode = null, string? YogaSetName = null, string? VariantDisplayName = null,
    string? OutcomeNatureCode = null, string? InferenceText = null,
    string? InferenceSourceRefCode = null, string? InferenceSourceLocator = null);

public sealed record YogaLifeMatterPathRow(
    string SourceRefCode, string SourceVariantCode, string YogaCode, byte PathRank,
    string Area, string SubArea1, string SubArea2, string SubArea3,
    string SubArea4, string SubArea5, string SubArea6,
    decimal RelevanceScore, string ReviewStatusCode);

/// <summary>
/// Typed read over vw_ChartYogaEvaluations (source-attributed yoga presence/absence,
/// db/053 + db/079's YogaTypeCode/YogaRule columns). Previously only reachable through
/// AstrologerEvidenceRepository's generic dynamic-row query, built for the raw
/// /charts/{id}/evidence page; this is the typed model for Astro Facts's Yoga step.
/// Evaluation itself is already wired into ChartGenerationService.PersistAnalytics
/// (YogaInputRepository.Replace -> ProductionYogaEngine) — this repository only reads.
/// </summary>
public sealed class YogaEvaluationRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public YogaEvaluationRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public IReadOnlyList<YogaEvaluationRow> GetByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<YogaEvaluationRow>("""
            SELECT SourceRefCode, YogaCode, Present, EvaluationStatus, YogaTypeCode, YogaRule, Notes,
                   SourceVariantCode, SourceLocator, YogaSetCode, YogaSetName, VariantDisplayName,
                   OutcomeNatureCode, InferenceText, InferenceSourceRefCode, InferenceSourceLocator
            FROM dbo.vw_ChartYogaEvaluations
            WHERE BirthDetailId = @birthDetailId
            ORDER BY SourceRefCode, YogaCode
            """, new { birthDetailId }).ToList();
    }

    public IReadOnlyList<YogaLifeMatterPathRow> GetLifeMatterPathsByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<YogaLifeMatterPathRow>("""
            SELECT DISTINCT m.SourceRefCode, m.SourceVariantCode, m.YogaCode, m.PathRank,
                   m.Area, m.SubArea1, m.SubArea2, m.SubArea3, m.SubArea4, m.SubArea5, m.SubArea6,
                   m.RelevanceScore, m.ReviewStatusCode
            FROM dbo.vw_YogaLifeMatter7x7 m
            JOIN dbo.vw_ChartYogaEvaluations y
              ON y.RuleSetId = m.RuleSetId
             AND y.SourceRefCode = m.SourceRefCode
             AND y.SourceVariantCode = m.SourceVariantCode
            WHERE y.BirthDetailId = @birthDetailId
            ORDER BY m.SourceRefCode, m.SourceVariantCode, m.PathRank
            """, new { birthDetailId }).ToList();
    }
}
