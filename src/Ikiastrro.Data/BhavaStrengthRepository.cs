using Dapper;
using Ikiastrro.Core.Engines.Strength;

namespace Ikiastrro.Data;

/// <summary>Key Inference 3.2 "House Strength" — one row per bhava, `vw_ChartBhavaBala`
/// (db/053). <c>HouseSign</c>/<c>LordPlanet</c>/... are null only if `tbl_Chart_HouseLords`
/// hasn't been (re)computed for this chart yet — the view's LEFT JOIN.</summary>
/// <summary>Virupas/Rupas columns are SQL `DECIMAL(12,3)` (db/39) and house numbers `TINYINT`
/// (db/ikiastrro.sql `tbl_Chart_HouseLords`) — `decimal`/`byte`, not `double`/`int`, or Dapper
/// can't match this record's constructor to the row. `LordPlacedInHouseFromLagna` and the
/// lord/sign/dignity strings are nullable — the view LEFT JOINs `tbl_Chart_HouseLords`, null
/// only if house lords haven't been (re)computed for this chart yet.</summary>
public sealed record BhavaBalaSummaryRow(byte HouseNumber, string? HouseSign, string? LordPlanet,
    byte? LordPlacedInHouseFromLagna, string? LordPlacedInSign, string? LordDignityStatus,
    decimal BhavaBalaVirupas, decimal BhavaBalaRupas);

/// <summary>Key Inference 3.2 expanded row — the three `tbl_Fact_BhavaStrengthComponent` rows
/// under one house (BHAVADHIPATI_BALA / BHAVA_DIG_BALA / BHAVA_DRIK_BALA, BhavaBalaCalculator).</summary>
public sealed record BhavaBalaComponentRow(byte HouseNumber, string ComponentCode, decimal ValueVirupas);

/// <summary>Persists the three auditable Bhava Bala components and twelve house totals.</summary>
public sealed class BhavaStrengthRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public BhavaStrengthRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public void InsertAll(int chartResultId, int ruleSetId, IEnumerable<BhavaBalaResult> results)
    {
        var rows = results.ToList();
        if (rows.Count == 0) return;
        using var connection = _connectionFactory.CreateOpenConnection();
        const string summarySql = """
            INSERT dbo.tbl_Fact_BhavaStrength
                (ChartResultId, HouseNumber, RuleSetId, StrengthProfileCode, FormulaSourceRefCode,
                 BhavaBalaVirupas, BhavaBalaRupas, CalculationNarrative)
            VALUES
                (@ChartResultId, @HouseNumber, @RuleSetId, 'PVR_INTEGRATED_STRENGTH', 'SRC_RAMAN_GRAHA_BHAVA_BALAS',
                 @Virupas, @Rupas, @Narrative)
            """;
        connection.Execute(summarySql, rows.Select(r => new
        {
            ChartResultId = chartResultId, r.HouseNumber, RuleSetId = ruleSetId,
            Virupas = r.BhavaBalaVirupas, Rupas = r.BhavaBalaRupas,
            Narrative = string.Join("; ", r.Components.Select(c => $"{c.ComponentCode}={c.ValueVirupas:0.###}"))
        }));

        const string componentSql = """
            INSERT dbo.tbl_Fact_BhavaStrengthComponent
                (ChartResultId, HouseNumber, RuleSetId, StrengthProfileCode, ComponentCode,
                 ValueVirupas, FormulaSourceRefCode, CalculationNarrative)
            VALUES
                (@ChartResultId, @HouseNumber, @RuleSetId, 'PVR_INTEGRATED_STRENGTH', @ComponentCode,
                 @ValueVirupas, 'SRC_RAMAN_GRAHA_BHAVA_BALAS', @Narrative)
            """;
        connection.Execute(componentSql, rows.SelectMany(r => r.Components.Select(c => new
        {
            ChartResultId = chartResultId, c.HouseNumber, RuleSetId = ruleSetId,
            c.ComponentCode, ValueVirupas = c.ValueVirupas, Narrative = c.Narrative
        })));
    }

    /// <summary>Key Inference 3.2 — one row per house for the "House Strength" table.</summary>
    public IReadOnlyList<BhavaBalaSummaryRow> GetSummaryByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<BhavaBalaSummaryRow>("""
            SELECT HouseNumber, HouseSign, LordPlanet, LordPlacedInHouseFromLagna, LordPlacedInSign,
                   LordDignityStatus, BhavaBalaVirupas, BhavaBalaRupas
            FROM dbo.vw_ChartBhavaBala WHERE BirthDetailId = @birthDetailId ORDER BY HouseNumber
            """, new { birthDetailId }).ToList();
    }

    /// <summary>Key Inference 3.2 — every stored component row, for the per-house expandable
    /// breakdown (Bhavadhipati / Bhava Dig / Bhava Drik Bala).</summary>
    public IReadOnlyList<BhavaBalaComponentRow> GetComponentsByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<BhavaBalaComponentRow>("""
            SELECT s.HouseNumber, s.ComponentCode, s.ValueVirupas
            FROM dbo.tbl_Fact_BhavaStrengthComponent s
            JOIN dbo.tbl_ChartResults c ON c.Id = s.ChartResultId
            WHERE c.BirthDetailId = @birthDetailId ORDER BY s.HouseNumber, s.ComponentCode
            """, new { birthDetailId }).ToList();
    }

    public void DeleteByChartResultId(int chartResultId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("DELETE FROM dbo.tbl_Fact_BhavaStrengthComponent WHERE ChartResultId = @ChartResultId; DELETE FROM dbo.tbl_Fact_BhavaStrength WHERE ChartResultId = @ChartResultId;", new { ChartResultId = chartResultId });
    }

    /// <summary>Clears every stored chart's bhava-strength rows for one person — the delete-first step of ChartGenerationService.GenerateAll (these FKs to tbl_ChartResults do not cascade).</summary>
    public void DeleteByBirthDetailId(int birthDetailId)
    {
        const string sql = """
            DELETE FROM dbo.tbl_Fact_BhavaStrengthComponent
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId);
            DELETE FROM dbo.tbl_Fact_BhavaStrength
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId);
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(sql, new { BirthDetailId = birthDetailId });
    }
}
