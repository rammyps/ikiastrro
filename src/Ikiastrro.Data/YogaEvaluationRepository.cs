using Dapper;

namespace Ikiastrro.Data;

public sealed record YogaEvaluationRow(
    string SourceRefCode, string YogaCode, bool? Present, string EvaluationStatus,
    string? YogaTypeCode, string? YogaRule);

/// <summary>
/// Typed read over vw_ChartYogaEvaluations (source-attributed yoga presence/absence,
/// db/053 + db/079's YogaTypeCode/YogaRule columns). Previously only reachable through
/// AstrologerEvidenceRepository's generic dynamic-row query, built for the raw
/// /charts/{id}/evidence page; this is the typed model for Key Inference's Yoga step.
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
            SELECT SourceRefCode, YogaCode, Present, EvaluationStatus, YogaTypeCode, YogaRule
            FROM dbo.vw_ChartYogaEvaluations
            WHERE BirthDetailId = @birthDetailId
            ORDER BY SourceRefCode, YogaCode
            """, new { birthDetailId }).ToList();
    }
}
