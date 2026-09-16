using Dapper;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Data;

public sealed class KpSubLordChainRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public KpSubLordChainRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    /// <summary>
    /// Persists KP sub-lord chain levels 2-7 (from <see cref="AstroMath.GetKpSubLordChain"/>) for every
    /// graha, keyed by the D1 ChartResultId. Level 1 stays exclusively on
    /// tbl_Chart_KeyDetails.NakshatraSubLordPlanetId (migration 095's own design) — not duplicated here.
    /// </summary>
    public void InsertAll(int chartResultId, IEnumerable<(int PlanetId, double NirayanaLongitudeDegrees)> grahas)
    {
        const string sql = """
            INSERT dbo.tbl_Fact_KpSubLordChain (ChartResultId, PlanetId, Level, LordPlanetId)
            VALUES (@ChartResultId, @PlanetId, @Level, @LordPlanetId)
            """;
        var rows = grahas.SelectMany(g =>
        {
            var chain = AstroMath.GetKpSubLordChain(g.NirayanaLongitudeDegrees, 7);
            return Enumerable.Range(2, 6).Select(level => new
            {
                ChartResultId = chartResultId,
                g.PlanetId,
                Level = level,
                LordPlanetId = AstroIds.PlanetId(chain[level - 1])
            });
        });
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(sql, rows);
    }

    public void DeleteByChartResultId(int chartResultId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("DELETE FROM dbo.tbl_Fact_KpSubLordChain WHERE ChartResultId = @ChartResultId", new { ChartResultId = chartResultId });
    }

    /// <summary>Clears every stored chart's KP sub-lord chain rows for one person — the delete-first step of ChartGenerationService.GenerateAll (this FK to tbl_ChartResults does not cascade).</summary>
    public void DeleteByBirthDetailId(int birthDetailId)
    {
        const string sql = """
            DELETE FROM dbo.tbl_Fact_KpSubLordChain
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId)
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(sql, new { BirthDetailId = birthDetailId });
    }
}
