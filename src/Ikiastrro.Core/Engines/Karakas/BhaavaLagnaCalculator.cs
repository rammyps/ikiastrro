using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Bhaava Lagna (BL). Classical rule (PVR sec 5.2, tbl_Rule_SpecialLagnaTimeRate, db/28) — the
/// TIME_FROM_SUNRISE family's 0.25°/minute coefficient (one rāśi per 2 hours); see
/// <see cref="SpecialLagnaTimeRateCalculator"/> for the shared mechanics.
///
/// PVR carries this point "for the sake of completeness" only (<c>UsedInBook = 0</c> in
/// tbl_Dim_SpecialLagnas) and it is not otherwise used in the book. PVR sec 5.2's own method
/// step and worked Example 7 take minutes-since-sunrise directly as degrees (1.0°/min), which
/// contradicts the section's stated rate by a factor of 4 — treated as a book erratum
/// (rammyps, 2026-09-04, recorded in db/28); this calculator follows the stated 0.25°/min rate,
/// matching JHora / PyJHora.
///
/// Verified against docs/artifacts/reference-charts/Rammy_Jagannatha.txt: 0 Ar 35' 00.46".
/// </summary>
public static class BhaavaLagnaCalculator
{
    public static SpecialPointSeed Compute(BirthDetails bd, SunTimes sun, AyanamsaDefinition? ayanamsa = null) =>
        SpecialLagnaTimeRateCalculator.Compute("BL", 0.25, bd, sun, ayanamsa);
}
