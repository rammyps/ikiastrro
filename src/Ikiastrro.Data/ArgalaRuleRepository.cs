using Dapper;

namespace Ikiastrro.Data;

/// <summary>tbl_Rule_Argala (migration 127) read path — currently just SignificanceNote
/// (migration 129, PVR sec.10.7) for the Key Inference 2.1 Argala table's column-header
/// tooltips. The calculation itself never reads this table — ArgalaCalculator mirrors it in
/// code, same as RasiDrishtiCalculator/tbl_Rule_RasiDrishti — this is presentation-only, the
/// same status as tbl_Rule_Yoga.ShortFormationRule feeding vw_ChartYogaEvaluations.YogaRule.</summary>
public sealed class ArgalaRuleRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public ArgalaRuleRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    /// <summary>HouseOffset (2/4/11/5) -> SignificanceNote, ARGALA rows only — the 4
    /// VIRODHARGALA rows have no note (PVR gives none in sec.10.7).</summary>
    public IReadOnlyDictionary<int, string> GetArgalaSignificanceNotes(int ruleSetId)
    {
        const string sql = """
            SELECT HouseOffset, SignificanceNote FROM dbo.tbl_Rule_Argala
            WHERE RuleSetId = @RuleSetId AND RelationTypeCode = 'ARGALA' AND SignificanceNote IS NOT NULL
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<(int HouseOffset, string SignificanceNote)>(sql, new { RuleSetId = ruleSetId })
            .ToDictionary(r => r.HouseOffset, r => r.SignificanceNote);
    }
}
