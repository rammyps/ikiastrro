using Dapper;

namespace Ikiastrro.Data;

public sealed record PlanetInHouseRow(
    int PlanetId, string PlanetName, int HouseNumber, string ResultText,
    string InterpretationStatusCode, string? SourceRefCode,
    int? DispositorPlanetId, string? DispositorPlanetName, int? DispositorHouseNumber,
    int? DispositorSignId, string? DispositorSignName, string? DispositorDignityStatus,
    bool IsSelfDisposed);

/// <summary>
/// dbo.tbl_Rule_PlanetInHouse (migration 113) joined against a specific chart's placements
/// via vw_ChartPlanetInHouseInterpretation (migration 115 promotion pass, migration 116
/// dispositor cross-reference) — B. V. Raman's "How to Judge a Horoscope" (SRC_RAMAN_HTJH)
/// planet-in-house interpretations, one row per graha actually placed in that chart, each
/// cross-referenced with its own dispositor (sign lord) and that dispositor's placement.
/// Read-only.
/// </summary>
public sealed class PlanetInHouseRepository(SqlConnectionFactory factory)
{
    public IReadOnlyList<PlanetInHouseRow> GetForChart(int chartResultId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<PlanetInHouseRow>("""
            SELECT CAST(PlanetId AS INT) AS PlanetId, PlanetName, CAST(HouseNumber AS INT) AS HouseNumber, ResultText,
                   InterpretationStatusCode, SourceRefCode,
                   CAST(DispositorPlanetId AS INT) AS DispositorPlanetId, DispositorPlanetName,
                   CAST(DispositorHouseNumber AS INT) AS DispositorHouseNumber,
                   CAST(DispositorSignId AS INT) AS DispositorSignId, DispositorSignName,
                   DispositorDignityStatus, IsSelfDisposed
            FROM dbo.vw_ChartPlanetInHouseInterpretation
            WHERE ChartResultId = @ChartResultId
            ORDER BY HouseNumber, PlanetId
            """, new { ChartResultId = chartResultId }).ToList();
    }
}
