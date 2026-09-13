using Dapper;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Data;

/// <summary>
/// dbo.vw_ChartHouseLordInterpretation — joins tbl_Chart_HouseLords (this chart's actually-computed
/// lord placements) to tbl_Rule_HouseLordPlacement (the classical claims), one or more interpretation
/// rows per house. Read-only view, so there's no InsertAll/Delete companion here — see
/// db/094_promote_house_lord_placement_claims.sql.
/// </summary>
public class ChartHouseLordInterpretationRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public ChartHouseLordInterpretationRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IReadOnlyList<HouseLordInterpretation> GetByChartResultId(int chartResultId)
    {
        const string sql = """
            SELECT * FROM dbo.vw_ChartHouseLordInterpretation
            WHERE ChartResultId = @ChartResultId
            ORDER BY OwnedHouseNumber, BranchCode
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<HouseLordInterpretation>(sql, new { ChartResultId = chartResultId }).ToList();
    }

    /// <summary>Every interpretation row for one person, all chart types — for the Web workspace's one-shot load.</summary>
    public IReadOnlyList<HouseLordInterpretation> GetByBirthDetailId(int birthDetailId)
    {
        const string sql = """
            SELECT * FROM dbo.vw_ChartHouseLordInterpretation
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId)
            ORDER BY ChartResultId, OwnedHouseNumber, BranchCode
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<HouseLordInterpretation>(sql, new { BirthDetailId = birthDetailId }).ToList();
    }
}
