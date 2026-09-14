using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;

namespace Ikiastrro.Data;

/// <summary>Key Inference 3.1 "Planet Strength" — one row per graha, `vw_ChartShadbala`
/// (db/071's consumer-contract columns). <see cref="PercentOfMinimum"/> null means
/// `MinimumRequiredRupas` wasn't seeded for that planet/rule-set (tbl_Rule_ShadbalaMinimumRupas).</summary>
/// <summary>All Virupas/Rupas columns are SQL `DECIMAL(12,3)` (db/39) — `decimal`, not
/// `double`, or Dapper can't match this record's constructor to the row.</summary>
public sealed record ShadbalaSummaryRow(string Planet, decimal SthanaBalaVirupas, decimal DigBalaVirupas,
    decimal KalaBalaVirupas, decimal CheshtaBalaVirupas, decimal NaisargikaBalaVirupas, decimal DrikBalaVirupas,
    decimal YuddhaBalaVirupas, decimal ShadbalaVirupas, decimal ShadbalaRupas, decimal? MinimumRequiredRupas,
    decimal? PercentOfMinimum);

/// <summary>Key Inference 3.1 expanded row — every `tbl_Fact_PlanetaryStrengthComponent` under
/// one planet, grouped by `BalaCode` in the UI (Sthāna/Dig/Kāla/Cheṣṭā/Naisargika/Dṛk/Yuddha).</summary>
public sealed record ShadbalaComponentRow(string Planet, string BalaCode, string SubComponentCode, decimal ValueVirupas);

/// <summary>Persists the PVR-first Shadbala summary and its auditable components.</summary>
public sealed class PlanetaryStrengthRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public PlanetaryStrengthRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public void InsertAll(int chartResultId, int ruleSetId, IEnumerable<PlanetaryStrengthResult> results)
    {
        var rows = results.ToList();
        if (rows.Count == 0) return;
        using var connection = _connectionFactory.CreateOpenConnection();
        const string summarySql = """
            INSERT dbo.tbl_Fact_PlanetaryStrength
                (ChartResultId, PlanetId, RuleSetId, StrengthProfileCode, FormulaSourceRefCode,
                 SthanaBalaVirupas, DigBalaVirupas, KalaBalaVirupas, CheshtaBalaVirupas,
                 NaisargikaBalaVirupas, DrikBalaVirupas, ShadbalaVirupas, ShadbalaRupas,
                 IshtaBala, KashtaBala)
            VALUES
                (@ChartResultId, @PlanetId, @RuleSetId, 'PVR_INTEGRATED_STRENGTH', 'SRC_RAMAN_GRAHA_BHAVA_BALAS',
                 @Sthana, @Dig, @Kala, @Cheshta, @Naisargika, @Drik, @Total, @Rupas, @Ishta, @Kashta)
            """;
        connection.Execute(summarySql, rows.Select(r => new
        {
            ChartResultId = chartResultId,
            PlanetId = AstroIds.PlanetId(Enum.Parse<PlanetName>(r.Planet)),
            RuleSetId = ruleSetId,
            Sthana = r.SthanaBalaVirupas,
            Dig = r.DigBalaVirupas,
            Kala = r.KalaBalaVirupas,
            Cheshta = r.CheshtaBalaVirupas,
            Naisargika = r.NaisargikaBalaVirupas,
            Drik = r.DrikBalaVirupas,
            Total = r.ShadbalaVirupas,
            Rupas = r.ShadbalaRupas,
            Ishta = r.IshtaBala,
            Kashta = r.KashtaBala
        }));

        const string componentSql = """
            INSERT dbo.tbl_Fact_PlanetaryStrengthComponent
                (ChartResultId, PlanetId, RuleSetId, StrengthProfileCode, BalaCode, SubComponentCode,
                 ValueVirupas, FormulaSourceRefCode, CalculationNarrative)
            VALUES
                (@ChartResultId, @PlanetId, @RuleSetId, 'PVR_INTEGRATED_STRENGTH', @BalaCode, @SubComponentCode,
                 @ValueVirupas, 'SRC_RAMAN_GRAHA_BHAVA_BALAS', @Narrative)
            """;
        connection.Execute(componentSql, rows.SelectMany(r => r.Components.Select(c => new
        {
            ChartResultId = chartResultId,
            PlanetId = AstroIds.PlanetId(Enum.Parse<PlanetName>(r.Planet)),
            RuleSetId = ruleSetId,
            c.BalaCode,
            c.SubComponentCode,
            ValueVirupas = c.ValueVirupas,
            Narrative = c.Narrative
        })));
    }

    public IReadOnlyList<dynamic> GetByChartResultId(int chartResultId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query("""
            SELECT * FROM dbo.tbl_Fact_PlanetaryStrength
            WHERE ChartResultId = @ChartResultId ORDER BY PlanetId
            """, new { ChartResultId = chartResultId }).ToList();
    }

    /// <summary>Key Inference 3.1 — one row per graha for the "Planet Strength" table.</summary>
    public IReadOnlyList<ShadbalaSummaryRow> GetSummaryByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<ShadbalaSummaryRow>("""
            SELECT Planet, SthanaBalaVirupas, DigBalaVirupas, KalaBalaVirupas, CheshtaBalaVirupas,
                   NaisargikaBalaVirupas, DrikBalaVirupas, YuddhaBalaVirupas, ShadbalaVirupas,
                   ShadbalaRupas, MinimumRequiredRupas, PercentOfMinimum
            FROM dbo.vw_ChartShadbala WHERE BirthDetailId = @birthDetailId ORDER BY Planet
            """, new { birthDetailId }).ToList();
    }

    /// <summary>Key Inference 3.1 — every stored component row, for the per-planet expandable
    /// breakdown (grouped client-side by <see cref="ShadbalaComponentRow.BalaCode"/>).</summary>
    public IReadOnlyList<ShadbalaComponentRow> GetComponentsByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<ShadbalaComponentRow>("""
            SELECT p.PlanetName AS Planet, s.BalaCode, s.SubComponentCode, s.ValueVirupas
            FROM dbo.tbl_Fact_PlanetaryStrengthComponent s
            JOIN dbo.tbl_ChartResults c ON c.Id = s.ChartResultId
            JOIN dbo.tbl_Planets p ON p.Id = s.PlanetId
            WHERE c.BirthDetailId = @birthDetailId ORDER BY p.Id, s.BalaCode, s.SubComponentCode
            """, new { birthDetailId }).ToList();
    }

    public void DeleteByChartResultId(int chartResultId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute("DELETE FROM dbo.tbl_Fact_PlanetaryStrengthComponent WHERE ChartResultId = @ChartResultId; DELETE FROM dbo.tbl_Fact_PlanetaryStrength WHERE ChartResultId = @ChartResultId;", new { ChartResultId = chartResultId });
    }

    /// <summary>Clears every stored chart's strength rows for one person — the delete-first step of ChartGenerationService.GenerateAll (these FKs to tbl_ChartResults do not cascade).</summary>
    public void DeleteByBirthDetailId(int birthDetailId)
    {
        const string sql = """
            DELETE FROM dbo.tbl_Fact_PlanetaryStrengthComponent
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId);
            DELETE FROM dbo.tbl_Fact_PlanetaryStrength
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId);
            """;
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(sql, new { BirthDetailId = birthDetailId });
    }
}
