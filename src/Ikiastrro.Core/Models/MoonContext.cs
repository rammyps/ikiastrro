namespace Ikiastrro.Core.Models;

/// <summary>
/// One row of <c>dbo.vw_ChartMoonContext</c> — D1's Sun/Moon longitudes, tithi/pakṣa, and
/// day/night birth. Derived read-only from tbl_Chart_KeyDetails' own Sun/Moon rows plus
/// tbl_Fact_YogaInputEvaluations (no dedicated fact table of its own — see
/// db/053_create_astrologer_evidence_views.sql). One row per person (D1 only), or none if D1
/// isn't computed yet.
/// </summary>
public class MoonContext
{
    public int BirthDetailId { get; set; }
    public int ChartResultId { get; set; }
    public double? SunLongitudeDegrees { get; set; }
    public double? MoonLongitudeDegrees { get; set; }
    public double? ElongationDegrees { get; set; }

    /// <summary>1–30, Śukla (waxing) tithis 1–15 then Kṛṣṇa (waning) 16–30. See
    /// <see cref="Presentation.ChartViewModel.TithiDisplay"/> for the display label.</summary>
    public byte? TithiNumber { get; set; }
    /// <summary>"SHUKLA" or "KRISHNA".</summary>
    public string? PakshaCode { get; set; }
    public bool? IsWaxingMoon { get; set; }
    public bool? IsFullMoon { get; set; }
    public bool? IsNightBirth { get; set; }
    public string? LunarPhasePolicyCode { get; set; }
    public string? SunriseMethodCode { get; set; }
}
