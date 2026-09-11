namespace Ikiastrro.Core.Engines.Panchanga;

/// <summary>
/// The Panchanga ("five limbs") layer for one birth moment: Tithi, Karana, Nitya Yoga, Vedic
/// Weekday, and Hora Lord, plus the Sunrise/Sunset/Janma-Ghatis time frame they're derived from.
/// D1-only — this describes the birth moment, not a per-varga concept, mirroring
/// <c>tbl_Chart_DashaPeriods</c>' key onto the D1 <c>ChartResultId</c>.
///
/// <see cref="TithiId"/> / <see cref="KaranaId"/> / <see cref="NityaYogaId"/> /
/// <see cref="VedicWeekdayId"/> are <c>tbl_Dim_*.Id</c> values (migration 081); the Dim rows
/// carry the display names. <see cref="HoraLordPlanetId"/> is a <c>tbl_Planets.Id</c>
/// (<see cref="Astronomy.AstroIds.PlanetId"/>).
/// </summary>
public sealed record PanchangaResult(
    DateTimeOffset SunriseLocal,
    DateTimeOffset SunsetLocal,
    DateTimeOffset NextSunriseLocal,
    bool IsNightBirth,
    double JanmaGhatis,
    double SunMoonDeltaDegrees,
    int TithiId,
    double TithiPercentRemaining,
    int KaranaId,
    double KaranaPercentRemaining,
    double SunMoonSumDegrees,
    int NityaYogaId,
    double NityaYogaPercentRemaining,
    int VedicWeekdayId,
    int HoraLordPlanetId)
{
    /// <summary>The only source PVR ch.1 (§1.3.8-1.3.12) gives for this layer — see
    /// <see cref="PanchangaCalculator"/> and db/081_create_panchanga_schema.sql.</summary>
    public const string SourceRefCode = "SRC_PVR_INTEGRATED";
}
