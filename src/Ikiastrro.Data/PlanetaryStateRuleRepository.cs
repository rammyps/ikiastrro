using Dapper;
using Ikiastrro.Core.Engines.PlanetaryStates;

namespace Ikiastrro.Data;

/// <summary>Loads the avastha rule/dimension layer (tbl_Rule_AgeState + tbl_Rule_WakefulnessState,
/// joined to tbl_Dim_PlanetaryState for state names) for one RuleSetId into a PlanetaryStateRuleSet
/// bundle. Same shape as CombustionRuleRepository — join the dim tables, filter by RuleSetId.</summary>
public class PlanetaryStateRuleRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public PlanetaryStateRuleRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    /// <summary>Loads the rule set flagged active in tbl_Rule_Sets (currently 'Parashari-Classical', Id 1).</summary>
    public PlanetaryStateRuleSet GetActiveRuleSet()
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        var activeId = connection.QuerySingle<byte>("SELECT Id FROM dbo.tbl_Rule_Sets WHERE IsActive = 1");
        return GetRuleSet(activeId);
    }

    /// <summary>Every `tbl_Dim_PlanetaryState` row keyed by Id — resolves `PlanetaryStateFact`'s
    /// AgeStateId/WakefulnessStateId/PostureStateId FKs (and, via `tbl_Fact_PlanetaryStateFlag`, the
    /// Deeptadi/Lajjitadi state ids) to a display name/meaning for the Astro Facts "Planet States"
    /// table, across all five avastha systems in one lookup, unlike <see cref="GetRuleSet"/>'s
    /// per-system shape.</summary>
    public IReadOnlyDictionary<byte, PlanetaryStateRow> GetAllStates()
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<PlanetaryStateRow>(
                "SELECT Id, AvasthaSystem, StateName, SequenceOrder, Meaning FROM dbo.tbl_Dim_PlanetaryState")
            .ToDictionary(r => r.Id, r => r);
    }

    public PlanetaryStateRuleSet GetRuleSet(byte ruleSetId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();

        const string ageSql = """
            SELECT r.Id, r.RuleSetId, r.AvasthaStateId, s.StateName,
                   r.OddSignFromDegree, r.OddSignToDegree, r.EvenSignFromDegree, r.EvenSignToDegree, r.EffectFraction
            FROM dbo.tbl_Rule_AgeState r
            JOIN dbo.tbl_Dim_PlanetaryState s ON s.Id = r.AvasthaStateId
            WHERE r.RuleSetId = @RuleSetId
            ORDER BY s.SequenceOrder
            """;
        var ageBands = connection.Query<AgeStateRuleRow>(ageSql, new { RuleSetId = ruleSetId }).ToList();

        const string wakefulnessSql = """
            SELECT r.Id, r.RuleSetId, r.DignityStatus, r.AvasthaStateId, s.StateName
            FROM dbo.tbl_Rule_WakefulnessState r
            JOIN dbo.tbl_Dim_PlanetaryState s ON s.Id = r.AvasthaStateId
            WHERE r.RuleSetId = @RuleSetId
            """;
        var wakefulness = connection.Query<WakefulnessStateRuleRow>(wakefulnessSql, new { RuleSetId = ruleSetId })
            .ToDictionary(r => r.DignityStatus, r => r);

        // Sayanaadi: no per-state Rule row (the formula is one fixed narrative in
        // tbl_Rule_PostureStateFormula, not a lookup) — just the 12 named Dim states, keyed by
        // their SequenceOrder (the index PostureStateCalculator computes).
        const string postureSql = """
            SELECT Id, AvasthaSystem, StateName, SequenceOrder, Meaning
            FROM dbo.tbl_Dim_PlanetaryState
            WHERE AvasthaSystem = 'Sayanadi'
            """;
        var postureStates = connection.Query<PlanetaryStateRow>(postureSql)
            .ToDictionary(r => r.SequenceOrder, r => r);

        const string deeptadiSql = """
            SELECT r.Id, r.RuleSetId, r.DignityStatus, r.AvasthaStateId, s.StateName
            FROM dbo.tbl_Rule_DeeptadiState r
            JOIN dbo.tbl_Dim_PlanetaryState s ON s.Id = r.AvasthaStateId
            WHERE r.RuleSetId = @RuleSetId
            """;
        var deeptadiByDignity = connection.Query<DeeptadiStateRuleRow>(deeptadiSql, new { RuleSetId = ruleSetId })
            .ToDictionary(r => r.DignityStatus, r => r);

        // Vikala/Khala/Kopita (Deeptadi) and all 6 Lajjitadi states are direct predicates, not
        // DignityStatus-keyed — just the named Dim vocabulary, same reasoning as Sayanaadi above.
        const string deeptadiNamesSql = "SELECT Id, AvasthaSystem, StateName, SequenceOrder, Meaning FROM dbo.tbl_Dim_PlanetaryState WHERE AvasthaSystem = 'Deeptadi'";
        var deeptadiStatesByName = connection.Query<PlanetaryStateRow>(deeptadiNamesSql).ToDictionary(r => r.StateName, r => r);

        const string lajjitadiNamesSql = "SELECT Id, AvasthaSystem, StateName, SequenceOrder, Meaning FROM dbo.tbl_Dim_PlanetaryState WHERE AvasthaSystem = 'Lajjitadi'";
        var lajjitadiStatesByName = connection.Query<PlanetaryStateRow>(lajjitadiNamesSql).ToDictionary(r => r.StateName, r => r);

        return new PlanetaryStateRuleSet(
            ruleSetId, ageBands, wakefulness, postureStates, deeptadiByDignity, deeptadiStatesByName, lajjitadiStatesByName);
    }
}
