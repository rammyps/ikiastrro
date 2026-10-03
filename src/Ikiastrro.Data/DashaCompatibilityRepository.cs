using Dapper;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;

namespace Ikiastrro.Data;

/// <summary>
/// Reads one person's dasha inputs for the pair page from what is already stored: the Moon nakshatra's
/// ruling planet (tbl_Nakshatras), the Vimshottari periods (tbl_Chart_DashaPeriods) and the D1 Darakaraka
/// (tbl_Chart_KeyDetails). Nothing is written. The D1 signs come from <see cref="DoshaChartRepository"/>.
/// </summary>
public sealed class DashaCompatibilityRepository
{
    private readonly SqlConnectionFactory _connectionFactory;
    private readonly DoshaChartRepository _chartRepository;

    public DashaCompatibilityRepository(SqlConnectionFactory connectionFactory, DoshaChartRepository chartRepository)
    {
        _connectionFactory = connectionFactory;
        _chartRepository = chartRepository;
    }

    private sealed record SpanRow(byte LevelNumber, string Lord, DateTime StartDate, DateTime EndDate);

    /// <summary>Null when the person lacks a stored D1 chart, a Moon nakshatra, or any dasha period.</summary>
    public DashaPerson? Get(int birthDetailId, string name, DateTime asOf, DateTime horizon)
    {
        var chart = _chartRepository.GetByBirthDetailId(birthDetailId);
        if (chart is null) return null;

        using var connection = _connectionFactory.CreateOpenConnection();
        var birthLord = connection.QuerySingleOrDefault<string>("""
            SELECT p.PlanetName
            FROM dbo.tbl_ChartResults c
            JOIN dbo.tbl_Chart_KeyDetails k ON k.ChartResultId = c.Id AND k.Planet = 'Moon' AND k.PointKind = 'Graha'
            JOIN dbo.tbl_Nakshatras n ON n.Id = k.NakshatraId
            JOIN dbo.tbl_Planets p ON p.Id = n.RulingPlanetId
            WHERE c.BirthDetailId = @Id AND c.ChartType = 'D1'
            """, new { Id = birthDetailId });
        if (birthLord is null) return null;

        var spans = connection.Query<SpanRow>("""
            SELECT LevelNumber, Lord, StartDate, EndDate
            FROM dbo.tbl_Chart_DashaPeriods
            WHERE ChartResultId IN (SELECT Id FROM dbo.tbl_ChartResults WHERE BirthDetailId = @Id)
            ORDER BY StartDayOffset, LevelNumber
            """, new { Id = birthDetailId })
            .Select(r => new DashaSpan(r.LevelNumber, r.Lord, r.StartDate, r.EndDate)).ToList();
        if (spans.Count == 0) return null;

        var running = spans.Where(s => s.Start <= asOf && asOf < s.End).OrderBy(s => s.Level).ToList();
        var upcoming = spans.Where(s => s.Level <= 2 && s.End > asOf && s.Start < horizon).ToList();

        var dk = connection.QuerySingleOrDefault<string>("""
            SELECT k.Planet
            FROM dbo.tbl_ChartResults c
            JOIN dbo.tbl_Chart_KeyDetails k ON k.ChartResultId = c.Id AND k.PointKind = 'Graha' AND k.CharaKaraka = 'DK'
            WHERE c.BirthDetailId = @Id AND c.ChartType = 'D1'
            """, new { Id = birthDetailId });

        return new DashaPerson(name, birthLord, running, upcoming, chart,
            Enum.TryParse<PlanetName>(dk, out var planet) ? planet : null);
    }
}
