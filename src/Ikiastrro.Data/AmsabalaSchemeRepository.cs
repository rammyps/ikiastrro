using Dapper;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Data;

/// <summary>tbl_Rule_AmsabalaGroup + tbl_Rule_AmsabalaName - PVR Sec. 6.6 varga-group
/// membership and amsa-name lookup, read by AmsabalaCalculator.</summary>
public class AmsabalaSchemeRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public AmsabalaSchemeRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public IReadOnlyList<AmsabalaGroupMember> GetGroups(int ruleSetId)
    {
        const string sql = """
            SELECT g.SchemeCode, ct.Code AS ChartType
            FROM dbo.tbl_Rule_AmsabalaGroup g
            JOIN dbo.tbl_Dim_ChartType   ct ON ct.Id = g.ChartTypeId
            WHERE g.RuleSetId = @RuleSetId
            ORDER BY g.SchemeCode, ct.DisplayOrder
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<AmsabalaGroupMember>(sql, new { RuleSetId = ruleSetId }).ToList();
    }

    public IReadOnlyList<AmsabalaNameEntry> GetNames(int ruleSetId)
    {
        const string sql = """
            SELECT SchemeCode, CAST(GoodCount AS INT) AS GoodCount, AmsaName
            FROM dbo.tbl_Rule_AmsabalaName
            WHERE RuleSetId = @RuleSetId
            ORDER BY SchemeCode, GoodCount
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<AmsabalaNameEntry>(sql, new { RuleSetId = ruleSetId }).ToList();
    }
}
