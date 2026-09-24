using Dapper;

namespace Ikiastrro.Data;

/// <summary>Reads the source-grounded planet x Sayanaadi interpretation catalogue.
/// Interpretations are versioned by RuleSetId and remain separate from computed facts.</summary>
public sealed class PostureStateInterpretationRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public PostureStateInterpretationRepository(SqlConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public IReadOnlyDictionary<(byte PlanetaryStateId, byte PlanetId), PostureStateInterpretationRow>
        GetByRuleSet(byte ruleSetId)
    {
        const string sql = """
            SELECT Id, RuleSetId, PlanetaryStateId, PlanetId,
                   InterpretationText, ConditionNotes, SourceRefCode, SourceLocator
            FROM dbo.tbl_Rule_PostureStateInterpretation
            WHERE RuleSetId = @RuleSetId
            ORDER BY PlanetaryStateId, PlanetId
            """;

        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<PostureStateInterpretationRow>(sql, new { RuleSetId = ruleSetId })
            .ToDictionary(r => (r.PlanetaryStateId, r.PlanetId), r => r);
    }
}

public sealed record PostureStateInterpretationRow(
    int Id,
    byte RuleSetId,
    byte PlanetaryStateId,
    byte PlanetId,
    string InterpretationText,
    string? ConditionNotes,
    string SourceRefCode,
    string SourceLocator);
