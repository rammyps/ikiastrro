using Dapper;

namespace Ikiastrro.Data;

public sealed record DashaLordRelationshipRow(
    string LevelCode, string LevelName, int DashaPeriodId, int? ParentDashaPeriodId,
    int SequenceInParent, DateTime StartDate, DateTime EndDate,
    int DashaLordPlanetId, string DashaLordName,
    int? SignId, string? RasiName, int? RasiLordPlanetId, string? RasiLordName,
    int? NakshatraId, string? NakshatraName, int? NakshatraLordPlanetId, string? NakshatraLordName,
    int? SubLordL1PlanetId, string? SubLordL1Name);

/// <summary>
/// dbo.tvf_Chart_DashaLordRelationship (migration 110) — for each of a person's dasha periods
/// (all 3 levels: Maha/Antar/Pratyantar), that period's Lord cross-referenced against its own
/// D1 Rasi lord, Nakshatra lord and KP level-1 sub-lord. Read-only, computed on demand.
/// </summary>
public sealed class DashaLordRelationshipRepository(SqlConnectionFactory factory)
{
    public IReadOnlyList<DashaLordRelationshipRow> GetByBirthDetailId(int birthDetailId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<DashaLordRelationshipRow>("""
            SELECT LevelCode, LevelName, DashaPeriodId, ParentDashaPeriodId, CAST(SequenceInParent AS INT) AS SequenceInParent,
                   StartDate, EndDate, CAST(DashaLordPlanetId AS INT) AS DashaLordPlanetId, DashaLordName,
                   CAST(SignId AS INT) AS SignId, RasiName, CAST(RasiLordPlanetId AS INT) AS RasiLordPlanetId, RasiLordName,
                   CAST(NakshatraId AS INT) AS NakshatraId, NakshatraName,
                   CAST(NakshatraLordPlanetId AS INT) AS NakshatraLordPlanetId, NakshatraLordName,
                   CAST(SubLordL1PlanetId AS INT) AS SubLordL1PlanetId, SubLordL1Name
            FROM dbo.tvf_Chart_DashaLordRelationship(@BirthDetailId)
            ORDER BY DashaPeriodId
            """, new { BirthDetailId = birthDetailId }).ToList();
    }
}
