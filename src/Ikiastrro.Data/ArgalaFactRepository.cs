using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Data;

/// <summary>One persisted tbl_Fact_Argala row with its chart type and occupant planet name resolved.
/// Column types follow migration 128 (TINYINT → byte, BIT → bool) so Dapper can bind the constructor.</summary>
public sealed record ArgalaFactRow(string ChartType, string TargetKind, string TargetKey, byte TargetHouseNumber,
    string RelationTypeCode, byte HouseOffset, bool IsPrimary, string OccupantPlanet, bool ExceptionApplied,
    bool CountedAntiZodiacally, string? SourceRefCode);

/// <summary>Persists tbl_Fact_Argala rows (migration 128) — ArgalaFactBuilder's flattened output
/// for one chart. Delete-then-reinsert per chart, same pattern as the analytics backfill modes
/// (ChartGenerationService.RecomputeAnalytics), which writes D1's rows on every GenerateAll.</summary>
public sealed class ArgalaFactRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public ArgalaFactRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    /// <summary>Every stored Argala / Virodhargala fact for one person, all charts — Life Matters' D1
    /// Argala analysis. The first read path for this table; before it, rows were only written by
    /// chart generation.</summary>
    public IReadOnlyList<ArgalaFactRow> GetByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<ArgalaFactRow>("""
            SELECT c.ChartType, a.TargetKind, a.TargetKey, a.TargetHouseNumber, a.RelationTypeCode,
                   a.HouseOffset, a.IsPrimary, p.PlanetName AS OccupantPlanet, a.ExceptionApplied,
                   a.CountedAntiZodiacally, a.SourceRefCode
            FROM dbo.tbl_Fact_Argala a
            JOIN dbo.tbl_ChartResults c ON c.Id = a.ChartResultId
            JOIN dbo.tbl_Planets p ON p.Id = a.OccupantPlanetId
            WHERE c.BirthDetailId = @birthDetailId
            ORDER BY c.ChartType, a.TargetKind, a.TargetHouseNumber, a.RelationTypeCode, a.HouseOffset
            """, new { birthDetailId }).ToList();
    }

    public void DeleteForChart(int chartResultId, int? chartTypeId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(
            "DELETE dbo.tbl_Fact_Argala WHERE ChartResultId = @ChartResultId " +
            "AND ((@ChartTypeId IS NULL AND ChartTypeId IS NULL) OR ChartTypeId = @ChartTypeId)",
            new { ChartResultId = chartResultId, ChartTypeId = chartTypeId });
    }

    /// <summary>Clears every stored chart's Argala fact rows for one person — the delete-first step of
    /// ChartGenerationService.GenerateAll/BirthDetailDeletionService.DeleteBirthDetail (this FK to
    /// tbl_ChartResults does not cascade; matches KpSubLordChainRepository's own method of the same name).</summary>
    public void DeleteByBirthDetailId(int birthDetailId)
    {
        const string sql = """
            DELETE FROM dbo.tbl_Fact_Argala
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId)
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(sql, new { BirthDetailId = birthDetailId });
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
