using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Ghati Lagna (GL, also Ghatika Lagna). Classical rule (PVR sec 5.4, tbl_Rule_SpecialLagnaTimeRate,
/// db/28) — the TIME_FROM_SUNRISE family's 1.25°/minute coefficient (one rāśi per ghaṭi = 24
/// minutes); see <see cref="SpecialLagnaTimeRateCalculator"/> for the shared mechanics.
///
/// Self with respect to fame, power and authority; weighed heavily when timing periods for a
/// politician (PVR sec 5.6). PVR sec 5.5 notes GL is unusually birthtime-sensitive — a 1-minute
/// error shifts it 1°15', more than the normal Lagna, especially once projected into vargas.
///
/// Verified against docs/artifacts/reference-charts/Rammy_Jagannatha.txt: 3 Pi 55' 31.30".
/// </summary>
public static class GhatiLagnaCalculator
{
    public static SpecialPointSeed Compute(BirthDetails bd, SunTimes sun, AyanamsaDefinition? ayanamsa = null) =>
        SpecialLagnaTimeRateCalculator.Compute("GL", 1.25, bd, sun, ayanamsa);
}
