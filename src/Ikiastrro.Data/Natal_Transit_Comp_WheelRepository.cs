using Dapper;

namespace Ikiastrro.Data;

/// <summary>One row of the Transit landing "D1 Birth" tab
/// (<c>docs/ui/components/spec_Natal_Transit_Comp_Wheel.md</c>) —
/// a persisted position from <c>vw_ChartPlanetEvidence</c>, no computation. Settable properties
/// so Dapper name-maps and coerces the view's tinyint / bit columns.</summary>
public sealed class Natal_Transit_Comp_WheelD1Row
{
    public int? House { get; set; }
    public string Planet { get; set; } = string.Empty;
    public string PointKind { get; set; } = string.Empty;
    public bool IsRetrograde { get; set; }
    public bool IsCombust { get; set; }
    public string? DegreesInSignDisplay { get; set; }
    public double LongitudeDegrees { get; set; }
    public string? Sign { get; set; }
    public string? Nakshatra { get; set; }
    public int? NakshatraPada { get; set; }
    public string? DignityStatus { get; set; }
    public decimal? DeepExaltationDegree { get; set; }
    public string? CharaKaraka { get; set; }
}

/// <summary>
/// Focused reads for the Transit landing page. Deliberately narrow — the D1 Birth tab needs
/// only the D1 planetary positions, so it does not go through
/// <see cref="AstrologerEvidenceRepository"/> (which fans out ~20 evidence sections).
/// </summary>
public sealed class Natal_Transit_Comp_WheelRepository(SqlConnectionFactory factory)
{
    public IReadOnlyList<Natal_Transit_Comp_WheelD1Row> LoadD1Birth(int birthDetailId)
    {
        using var connection = factory.CreateOpenConnection();
        return connection.Query<Natal_Transit_Comp_WheelD1Row>("""
            SELECT e.HouseNumberFromLagna AS House, e.Planet, e.PointKind,
                   e.IsRetrograde, e.IsCombust, e.DegreesInSignDisplay, e.Sign, e.Nakshatra, e.NakshatraPada,
                   e.DignityStatus, e.CharaKaraka, deepExaltation.DeepDegree AS DeepExaltationDegree,
                   COALESCE(e.VargaLongitudeDegrees, e.NirayanaLongitudeDegrees) AS LongitudeDegrees
            FROM dbo.vw_ChartPlanetEvidence e
            OUTER APPLY
            (
                SELECT TOP (1) d.DeepDegree
                FROM dbo.tbl_Rule_GrahaDignity d
                JOIN dbo.tbl_Planets p ON p.Id=d.PlanetId AND p.PlanetName=e.Planet
                WHERE d.RuleSetId=e.RuleSetId AND d.DignityTypeCode='EXALTED'
                  AND d.DeepDegree IS NOT NULL AND d.IsActive=1
                ORDER BY d.IsPrimary DESC,d.Id
            ) deepExaltation
            WHERE e.BirthDetailId = @birthDetailId AND e.ChartType = 'D1'
              AND (e.PointKind = 'Graha' OR e.Planet IN ('Lagna', 'Ascendant'))
            """, new { birthDetailId }).ToList();
    }
}
