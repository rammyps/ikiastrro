using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Hora Lagna. Classical rule (PyJHora <c>special_ascendant</c>, lagna_rate_factor 0.5) — the
/// TIME_FROM_SUNRISE family's 0.5°/minute coefficient; see
/// <see cref="SpecialLagnaTimeRateCalculator"/> for the shared mechanics (also used by
/// Bhaava/Ghati Lagna, tbl_Rule_SpecialLagnaTimeRate, db/28).
///
/// Verified against docs/artifacts/reference-charts/Rammy_Jagannatha.txt: 23 Pi 55' 08" (Pisces; Navamsa Aquarius).
/// </summary>
public static class HoraLagnaCalculator
{
    public static SpecialPointSeed Compute(BirthDetails bd, SunTimes sun, AyanamsaDefinition? ayanamsa = null) =>
        SpecialLagnaTimeRateCalculator.Compute("HL", 0.5, bd, sun, ayanamsa);
}
