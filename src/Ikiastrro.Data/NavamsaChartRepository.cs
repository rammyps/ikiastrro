using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;

namespace Ikiastrro.Data;

/// <summary>Reads the stored D1 and D9 grahas the Navamsa comparison needs (sign, house from Lagna,
/// dignity, and the D1 chara karaka). Nothing is written and nothing is recomputed.</summary>
public sealed class NavamsaChartRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public NavamsaChartRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    private sealed record Row(string ChartType, string Planet, string Sign, byte? HouseNumberFromLagna, string? DignityStatus, string? CharaKaraka);

    /// <summary>Null when the person lacks a stored D1 or D9 Ascendant or any of the nine grahas in either chart.</summary>
    public NavamsaChart? GetByBirthDetailId(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        var rows = connection.Query<Row>("""
            SELECT c.ChartType, k.Planet, k.Sign, k.HouseNumberFromLagna, k.DignityStatus, k.CharaKaraka
            FROM dbo.tbl_ChartResults c
            JOIN dbo.tbl_Chart_KeyDetails k ON k.ChartResultId = c.Id AND k.PointKind = 'Graha'
            WHERE c.BirthDetailId = @Id AND c.ChartType IN ('D1', 'D9')
            """, new { Id = birthDetailId }).ToList();

        ZodiacName? d1Lagna = null, d9Lagna = null;
        var d1 = new Dictionary<PlanetName, ZodiacName>();
        var d9 = new Dictionary<PlanetName, VargaPoint>();
        PlanetName? darakaraka = null;

        foreach (var r in rows)
        {
            if (!Enum.TryParse<ZodiacName>(r.Sign, out var sign)) continue;
            var isD1 = r.ChartType == "D1";
            if (r.Planet == "Ascendant")
            {
                if (isD1) d1Lagna = sign; else d9Lagna = sign;
                continue;
            }
            if (!Enum.TryParse<PlanetName>(r.Planet, out var planet) || r.HouseNumberFromLagna is not byte house) continue;
            if (isD1)
            {
                d1[planet] = sign;
                if (r.CharaKaraka == "DK") darakaraka = planet;
            }
            else d9[planet] = new VargaPoint(sign, house, r.DignityStatus);
        }
        return d1Lagna is { } a && d9Lagna is { } b && d1.Count == 9 && d9.Count == 9
            ? new NavamsaChart(a, d1, b, d9, darakaraka)
            : null;
    }
}
