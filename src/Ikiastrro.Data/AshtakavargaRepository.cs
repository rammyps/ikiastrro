using Dapper;
using Ikiastrro.Core.Engines.Ashtakavarga;

namespace Ikiastrro.Data;

public sealed record AshtakavargaRow(string RecipientCode, byte SignNumber, byte BinduCount, byte? SarvaBindus);
public sealed record AshtakavargaPindaRow(string RecipientCode, short? RasiPinda, short? GrahaPinda, short? SodhyaPinda);

/// <summary>
/// Persists Parāśari Ashtakavarga for one chart — the seven Bhinnāṣṭakavargas (+ the 1/0
/// contribution detail), the Sarvāṣṭakavarga, and the Sodhya Piṇḍa reductions.
/// Written on the D1 <c>tbl_ChartResults</c> row, mirroring the strength / vargottama facts.
/// Also the read model for Key Inference 3.3, backed by vw_ChartAshtakavarga and the post-sodhana pinda facts.
/// </summary>
public sealed class AshtakavargaRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public AshtakavargaRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public IReadOnlyList<AshtakavargaRow> GetByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<AshtakavargaRow>("""
            SELECT RecipientCode, SignNumber, BinduCount, SarvaBindus
            FROM dbo.vw_ChartAshtakavarga
            WHERE BirthDetailId = @birthDetailId AND ChartType = 'D1'
            ORDER BY RecipientCode, SignNumber
            """, new { birthDetailId }).ToList();
    }

    public IReadOnlyList<AshtakavargaPindaRow> GetPindaByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        return connection.Query<AshtakavargaPindaRow>("""
            SELECT p.RecipientCode, p.RasiPinda, p.GrahaPinda, p.SodhyaPinda
            FROM dbo.tbl_Fact_AshtakavargaPinda p
            JOIN dbo.tbl_ChartResults c ON c.Id = p.ChartResultId
            WHERE c.BirthDetailId = @birthDetailId AND c.ChartType = 'D1'
            ORDER BY CASE p.RecipientCode
                WHEN 'SUN' THEN 1 WHEN 'MOON' THEN 2 WHEN 'MARS' THEN 3 WHEN 'MERCURY' THEN 4
                WHEN 'JUPITER' THEN 5 WHEN 'VENUS' THEN 6 WHEN 'SATURN' THEN 7 ELSE 8 END
            """, new { birthDetailId }).ToList();
    }

    public void Insert(int chartResultId, int ruleSetId, AshtakavargaResult result)
    {
        using var connection = _connectionFactory.CreateOpenConnection();

        connection.Execute(
            """
            INSERT dbo.tbl_Fact_BhinnaAshtakavarga
                (ChartResultId, RuleSetId, MethodCode, RecipientCode, SignNumber, BinduCount, SourceRefCode)
            VALUES (@ChartResultId, @RuleSetId, @MethodCode, @RecipientCode, @SignNumber, @BinduCount, @SourceRefCode)
            """,
            result.Bhinna.SelectMany(b => Enumerable.Range(0, 12).Select(s => new
            {
                ChartResultId = chartResultId,
                RuleSetId = ruleSetId,
                MethodCode = AshtakavargaResult.MethodCode,
                RecipientCode = b.Recipient,
                SignNumber = s + 1,
                BinduCount = b.Bindus[s],
                SourceRefCode = AshtakavargaResult.SourceRefCode,
            })));

        connection.Execute(
            """
            INSERT dbo.tbl_Fact_BhinnaAshtakavargaContribution
                (ChartResultId, RuleSetId, MethodCode, RecipientCode, ContributorCode, SignNumber,
                 IsBindu, ContributorSignNumber, HouseOffsetFromContributor)
            VALUES (@ChartResultId, @RuleSetId, @MethodCode, @RecipientCode, @ContributorCode, @SignNumber,
                 @IsBindu, @ContributorSignNumber, @HouseOffsetFromContributor)
            """,
            result.Contributions.Select(c => new
            {
                ChartResultId = chartResultId,
                RuleSetId = ruleSetId,
                MethodCode = AshtakavargaResult.MethodCode,
                RecipientCode = c.Recipient,
                ContributorCode = c.Contributor,
                SignNumber = c.SignNumber,
                IsBindu = c.IsBindu,
                ContributorSignNumber = c.ContributorSignNumber,
                HouseOffsetFromContributor = ((c.SignNumber - c.ContributorSignNumber) % 12 + 12) % 12 + 1,
            }));

        connection.Execute(
            """
            INSERT dbo.tbl_Fact_SarvaAshtakavarga
                (ChartResultId, RuleSetId, MethodCode, SignNumber, TotalBindus, IncludesLagna)
            VALUES (@ChartResultId, @RuleSetId, @MethodCode, @SignNumber, @TotalBindus, 0)
            """,
            Enumerable.Range(0, 12).Select(s => new
            {
                ChartResultId = chartResultId,
                RuleSetId = ruleSetId,
                MethodCode = AshtakavargaResult.MethodCode,
                SignNumber = s + 1,
                TotalBindus = result.Sarva.Bindus[s],
            }));

        connection.Execute(
            """
            INSERT dbo.tbl_Fact_AshtakavargaPinda
                (ChartResultId, RuleSetId, MethodCode, RecipientCode, RasiPinda, GrahaPinda, SodhyaPinda)
            VALUES (@ChartResultId, @RuleSetId, @MethodCode, @RecipientCode, @RasiPinda, @GrahaPinda, @SodhyaPinda)
            """,
            result.Pinda.Select(p => new
            {
                ChartResultId = chartResultId,
                RuleSetId = ruleSetId,
                MethodCode = AshtakavargaResult.MethodCode,
                RecipientCode = p.Recipient,
                p.RasiPinda,
                p.GrahaPinda,
                p.SodhyaPinda,
            }));
    }

    public void DeleteByChartResultId(int chartResultId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(
            """
            DELETE FROM dbo.tbl_Fact_BhinnaAshtakavargaContribution WHERE ChartResultId = @ChartResultId;
            DELETE FROM dbo.tbl_Fact_BhinnaAshtakavarga            WHERE ChartResultId = @ChartResultId;
            DELETE FROM dbo.tbl_Fact_SarvaAshtakavarga             WHERE ChartResultId = @ChartResultId;
            DELETE FROM dbo.tbl_Fact_AshtakavargaPinda             WHERE ChartResultId = @ChartResultId;
            """,
            new { ChartResultId = chartResultId });
    }

    /// <summary>Clears every stored chart's Ashtakavarga rows for one person — the delete-first step
    /// of ChartGenerationService.GenerateAll (these FKs to tbl_ChartResults do not cascade).</summary>
    public void DeleteByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        connection.Execute(
            """
            DECLARE @ids TABLE (Id INT PRIMARY KEY);
            INSERT @ids SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @BirthDetailId;
            DELETE FROM dbo.tbl_Fact_BhinnaAshtakavargaContribution WHERE ChartResultId IN (SELECT Id FROM @ids);
            DELETE FROM dbo.tbl_Fact_BhinnaAshtakavarga            WHERE ChartResultId IN (SELECT Id FROM @ids);
            DELETE FROM dbo.tbl_Fact_SarvaAshtakavarga             WHERE ChartResultId IN (SELECT Id FROM @ids);
            DELETE FROM dbo.tbl_Fact_AshtakavargaPinda             WHERE ChartResultId IN (SELECT Id FROM @ids);
            """,
            new { BirthDetailId = birthDetailId });
    }
}
