using Dapper;

namespace Ikiastrro.Data;

public sealed record AshtakavargaRow(string RecipientCode, byte SignNumber, byte BinduCount, byte? SarvaBindus);
public sealed record AshtakavargaPindaRow(string RecipientCode, short? RasiPinda, short? GrahaPinda, short? SodhyaPinda);

/// <summary>Read model for Key Inference 3.3, backed by vw_ChartAshtakavarga and the post-sodhana pinda facts.</summary>
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
}
