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
            SELECT HouseNumberFromLagna AS House, Planet, PointKind,
                   IsRetrograde, IsCombust, DegreesInSignDisplay, Sign, Nakshatra, NakshatraPada
                   ,COALESCE(VargaLongitudeDegrees, NirayanaLongitudeDegrees) AS LongitudeDegrees
            FROM dbo.vw_ChartPlanetEvidence
            WHERE BirthDetailId = @birthDetailId AND ChartType = 'D1'
              AND (PointKind = 'Graha' OR Planet IN ('Lagna', 'Ascendant'))
            """, new { birthDetailId }).ToList();
    }
}
