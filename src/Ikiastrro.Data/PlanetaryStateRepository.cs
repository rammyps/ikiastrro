using Dapper;
using Ikiastrro.Core.Engines.PlanetaryStates;

namespace Ikiastrro.Data;

/// <summary>tbl_Fact_PlanetaryState — computed avastha states per planet per chart, shared across
/// every chart type (rows discriminated by ChartResultId / ChartType). Mirrors
/// ChartKeyDetailsRepository's insert / delete / get-by-* shape.</summary>
public class PlanetaryStateRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public PlanetaryStateRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    private const string FlagInsertSql = """
        INSERT INTO dbo.tbl_Fact_PlanetaryStateFlag (PlanetaryStateFactId, AvasthaStateId)
        VALUES (@PlanetaryStateFactId, @AvasthaStateId)
        """;

    /// <summary>Inserts each fact (returning its Id, needed for the flag child rows) then its
    /// Deeptadi/Lajjitadi flag rows — mirrors ChartMultiGrahaConjunctionRepository's group+member
    /// per-row OUTPUT INSERTED.Id pattern, since tbl_Fact_PlanetaryStateFlag needs the parent's
    /// generated Id.</summary>
    public void InsertAll(IEnumerable<PlanetaryStateFact> rows)
    {
        const string sql = """
            INSERT INTO dbo.tbl_Fact_PlanetaryState
                (ChartResultId, Planet, RuleSetId,
                 AgeStateId, AgeEffectFraction, WakefulnessStateId,
                 PlanetId, PostureStateId)
            OUTPUT INSERTED.Id
            VALUES
                (@ChartResultId, @Planet, @RuleSetId,
                 @AgeStateId, @AgeEffectFraction, @WakefulnessStateId,
                 @PlanetId, @PostureStateId)
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        foreach (var row in rows)
        {
            row.Id = connection.ExecuteScalar<int>(sql, row);
            var flags = row.DeeptadiStateIds.Concat(row.LajjitadiStateIds)
                .Select(stateId => new { PlanetaryStateFactId = row.Id, AvasthaStateId = stateId })
                .ToList();
            if (flags.Count > 0) connection.Execute(FlagInsertSql, flags);
        }
    }

    /// <summary>Attaches each fact's Deeptadi/Lajjitadi flag rows (split by AvasthaSystem, joined to
    /// tbl_Dim_PlanetaryState to tell the two systems apart) — same two-query-then-group shape as
    /// ChartMultiGrahaConjunctionRepository's group+member load.</summary>
    private static void AttachFlags(System.Data.IDbConnection connection, IReadOnlyList<PlanetaryStateFact> facts)
    {
        if (facts.Count == 0) return;
        const string sql = """
            SELECT f.PlanetaryStateFactId, f.AvasthaStateId, s.AvasthaSystem
            FROM dbo.tbl_Fact_PlanetaryStateFlag f
            JOIN dbo.tbl_Dim_PlanetaryState s ON s.Id = f.AvasthaStateId
            WHERE f.PlanetaryStateFactId IN @Ids
            """;
        var flagRows = connection.Query<(int PlanetaryStateFactId, byte AvasthaStateId, string AvasthaSystem)>(
            sql, new { Ids = facts.Select(f => f.Id).ToList() });
        var byFact = flagRows.ToLookup(r => r.PlanetaryStateFactId);
        foreach (var fact in facts)
        {
            var flagsForFact = byFact[fact.Id];
            fact.DeeptadiStateIds = flagsForFact.Where(r => r.AvasthaSystem == "Deeptadi").Select(r => r.AvasthaStateId).ToList();
            fact.LajjitadiStateIds = flagsForFact.Where(r => r.AvasthaSystem == "Lajjitadi").Select(r => r.AvasthaStateId).ToList();
        }
    }

    public IReadOnlyList<PlanetaryStateFact> GetByChartResultId(int chartResultId)
    {
        const string sql = "SELECT * FROM dbo.tbl_Fact_PlanetaryState WHERE ChartResultId = @ChartResultId ORDER BY Id";
        using var connection = _connectionFactory.CreateOpenConnection();
        var facts = connection.Query<PlanetaryStateFact>(sql, new { ChartResultId = chartResultId }).ToList();
        AttachFlags(connection, facts);
        return facts;
    }

    /// <summary>Every avastha row for one person, all chart types — for the Web workspace's one-shot load.</summary>
    public IReadOnlyList<PlanetaryStateFact> GetByBirthDetailId(int birthDetailId)
    {
        const string sql = """
            SELECT * FROM dbo.tbl_Fact_PlanetaryState
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId)
            ORDER BY Id
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        var facts = connection.Query<PlanetaryStateFact>(sql, new { BirthDetailId = birthDetailId }).ToList();
        AttachFlags(connection, facts);
        return facts;
    }

    /// <summary>Deletes every row (every chart type) for one person — used by BirthDetailDeletionService / GenerateAll.</summary>
    public void DeleteByBirthDetailId(int birthDetailId)
    {
        const string sql = """
            DELETE FROM dbo.tbl_Fact_PlanetaryState
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId)
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(sql, new { BirthDetailId = birthDetailId });
    }

    /// <summary>Deletes just one ChartResult's rows — used by ChartGenerationService.RecomputeAnalytics.</summary>
    public void DeleteByChartResultId(int chartResultId)
    {
        const string sql = "DELETE FROM dbo.tbl_Fact_PlanetaryState WHERE ChartResultId = @ChartResultId";
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(sql, new { ChartResultId = chartResultId });
    }
}
