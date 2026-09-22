using Dapper;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Data;

/// <summary>dbo.tbl_Rule_RasiNakshatraCombination (migration 124) — the 36-row Rāśi×Nakṣatra
/// reference. Read-only, no chart dependency — same "small static reference table" shape as
/// LifeAreaReferenceRepository.</summary>
public class RasiNakshatraCombinationRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public RasiNakshatraCombinationRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public IReadOnlyList<RasiNakshatraCombinationRow> GetAll(int ruleSetId)
    {
        const string sql = """
            SELECT RasiId, NakshatraId, SpanStartDegree, SpanEndDegree,
                   CombinedCharacter, MainSignifications, PotentialBenefits, PotentialDisadvantages, JudgmentNote,
                   LordRelation, AspectingRasis, SourceRefCode
            FROM dbo.tbl_Rule_RasiNakshatraCombination
            WHERE RuleSetId = @RuleSetId AND IsActive = 1
            ORDER BY RasiId, SpanStartDegree
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<RasiNakshatraCombinationRow>(sql, new { RuleSetId = ruleSetId }).ToList();
    }
}
