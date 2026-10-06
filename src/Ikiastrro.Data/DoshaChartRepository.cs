using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;

namespace Ikiastrro.Data;

/// <summary>Reads the D1 signs Dosha Samya needs (Ascendant and the nine grahas) from the stored key
/// details. Nothing is written and nothing is recomputed.</summary>
public sealed class DoshaChartRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    public DoshaChartRepository(SqlConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    private sealed record Row(string Planet, string Sign);

    /// <summary>Null when the person lacks a stored Ascendant or any of the nine grahas in the chart type (D1 by default).</summary>
    public DoshaChart? GetByBirthDetailId(int birthDetailId, string chartType = "D1")
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        var rows = connection.Query<Row>("""
            SELECT k.Planet, k.Sign
            FROM dbo.tbl_ChartResults c
            JOIN dbo.tbl_Chart_KeyDetails k ON k.ChartResultId = c.Id AND k.PointKind = 'Graha'
            WHERE c.BirthDetailId = @Id AND c.ChartType = @ChartType
            """, new { Id = birthDetailId, ChartType = chartType }).ToList();

        var signs = new Dictionary<PlanetName, ZodiacName>();
        ZodiacName? lagna = null;
        foreach (var r in rows)
        {
            if (!Enum.TryParse<ZodiacName>(r.Sign, out var sign)) continue;
            if (r.Planet == "Ascendant") lagna = sign;
            else if (Enum.TryParse<PlanetName>(r.Planet, out var planet)) signs[planet] = sign;
        }
        return lagna is { } l && signs.Count == 9 ? new DoshaChart(l, signs) : null;
    }

    /// <summary>The stored sign charts for the given varga codes, leaving out any the person lacks.</summary>
    public IReadOnlyDictionary<string, DoshaChart> GetVargas(int birthDetailId, IEnumerable<string> chartTypes)
    {
        var result = new Dictionary<string, DoshaChart>();
        foreach (var code in chartTypes)
            if (GetByBirthDetailId(birthDetailId, code) is { } chart) result[code] = chart;
        return result;
    }

    private sealed record DignityRow(string Planet, string? DignityStatus);

    /// <summary>The dignity the chart pipeline stored for each D1 graha (Exalted, Own, Debilitated, ...), null
    /// where none was recorded. Shown beside the dosha comparison; no cancellation is applied from it.</summary>
    public IReadOnlyDictionary<PlanetName, string?> GetDignities(int birthDetailId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        var rows = connection.Query<DignityRow>("""
            SELECT k.Planet, k.DignityStatus
            FROM dbo.tbl_ChartResults c
            JOIN dbo.tbl_Chart_KeyDetails k ON k.ChartResultId = c.Id AND k.PointKind = 'Graha'
            WHERE c.BirthDetailId = @Id AND c.ChartType = 'D1'
            """, new { Id = birthDetailId });
        var result = new Dictionary<PlanetName, string?>();
        foreach (var r in rows)
            if (Enum.TryParse<PlanetName>(r.Planet, out var planet)) result[planet] = r.DignityStatus;
        return result;
    }
}
