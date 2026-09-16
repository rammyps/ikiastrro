using Dapper;
using Ikiastrro.Core.Engines.Strength;

namespace Ikiastrro.Data;

public sealed record AmsabalaRow(string PlanetCode, string SchemeCode, byte GroupSize, byte GoodCount, string? AmsaName, string Detail);

/// <summary>
/// Persists AmsabalaCalculator's per-planet, per-scheme amsa (migration 101) on the D1
/// <c>tbl_ChartResults</c> row, mirroring AshtakavargaRepository. Also the read model for
/// Key Inference 3.4, backed by vw_ChartAmsabala.
/// </summary>
public sealed class AmsabalaRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public AmsabalaRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public IReadOnlyList<AmsabalaRow> GetByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<AmsabalaRow>("""
            SELECT PlanetCode, SchemeCode, GroupSize, GoodCount, AmsaName, Detail
            FROM dbo.vw_ChartAmsabala
            WHERE BirthDetailId = @birthDetailId AND ChartType = 'D1'
            ORDER BY CASE PlanetCode
                WHEN 'SUN' THEN 1 WHEN 'MOON' THEN 2 WHEN 'MARS' THEN 3 WHEN 'MERCURY' THEN 4
                WHEN 'JUPITER' THEN 5 WHEN 'VENUS' THEN 6 WHEN 'SATURN' THEN 7 ELSE 8 END,
                CASE SchemeCode
                WHEN 'SHADVARGA' THEN 1 WHEN 'SAPTAVARGA' THEN 2 WHEN 'DASAVARGA' THEN 3 ELSE 4 END
            """, new { birthDetailId }).ToList();
    }

    public void Insert(int chartResultId, int ruleSetId, string planetCode, IReadOnlyList<AmsabalaResult> results)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(
            """
            INSERT dbo.tbl_Fact_Amsabala
                (ChartResultId, RuleSetId, PlanetCode, SchemeCode, GroupSize, GoodCount, AmsaName, Detail)
            VALUES (@ChartResultId, @RuleSetId, @PlanetCode, @SchemeCode, @GroupSize, @GoodCount, @AmsaName, @Detail)
            """,
            results.Select(r => new
            {
                ChartResultId = chartResultId,
                RuleSetId = ruleSetId,
                PlanetCode = planetCode,
                r.SchemeCode,
                r.GroupSize,
                r.GoodCount,
                r.AmsaName,
                r.Detail,
            }));
    }

    public void DeleteByChartResultId(int chartResultId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("DELETE FROM dbo.tbl_Fact_Amsabala WHERE ChartResultId = @ChartResultId",
            new { ChartResultId = chartResultId });
    }

    /// <summary>Clears every stored chart's Amsabala rows for one person — the delete-first step
    /// of ChartGenerationService.GenerateAll (this FK to tbl_ChartResults does not cascade).</summary>
    public void DeleteByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(
            """
            DELETE FROM dbo.tbl_Fact_Amsabala
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId)
            """,
            new { BirthDetailId = birthDetailId });
    }
}
