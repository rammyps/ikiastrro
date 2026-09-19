using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Data;

/// <summary>Persists tbl_Fact_Argala rows (migration 128) — ArgalaFactBuilder's flattened output
/// for one chart. Delete-then-reinsert per chart, same pattern as the analytics backfill modes
/// (ChartGenerationService.RecomputeAnalytics), since this isn't yet wired into that pipeline.</summary>
public sealed class ArgalaFactRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public ArgalaFactRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public void DeleteForChart(int chartResultId, int? chartTypeId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(
            "DELETE dbo.tbl_Fact_Argala WHERE ChartResultId = @ChartResultId " +
            "AND ((@ChartTypeId IS NULL AND ChartTypeId IS NULL) OR ChartTypeId = @ChartTypeId)",
            new { ChartResultId = chartResultId, ChartTypeId = chartTypeId });
    }

    public void InsertAll(int chartResultId, int ruleSetId, int? chartTypeId, IEnumerable<ChartArgalaFact> facts)
    {
        var rows = facts.ToList();
        if (rows.Count == 0) return;

        using var connection = _connectionFactory.CreateOpenConnection();
        const string sql = """
            INSERT dbo.tbl_Fact_Argala
                (ChartResultId, RuleSetId, ChartTypeId, TargetKind, TargetKey, TargetHouseNumber, TargetSignId,
                 RelationTypeCode, HouseOffset, IsPrimary, OccupantPlanetId, ExceptionApplied, CountedAntiZodiacally, SourceRefCode)
            VALUES
                (@ChartResultId, @RuleSetId, @ChartTypeId, @TargetKind, @TargetKey, @TargetHouseNumber, @TargetSignId,
                 @RelationTypeCode, @HouseOffset, @IsPrimary, @OccupantPlanetId, @ExceptionApplied, @CountedAntiZodiacally, 'SRC_PVR_INTEGRATED')
            """;
        connection.Execute(sql, rows.Select(f => new
        {
            ChartResultId = chartResultId,
            RuleSetId = ruleSetId,
            ChartTypeId = chartTypeId,
            f.TargetKind,
            f.TargetKey,
            f.TargetHouseNumber,
            TargetSignId = AstroIds.SignId(f.TargetSign),
            f.RelationTypeCode,
            f.HouseOffset,
            f.IsPrimary,
            OccupantPlanetId = AstroIds.PlanetId(f.OccupantPlanet),
            f.ExceptionApplied,
            f.CountedAntiZodiacally
        }));
    }
}
