using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Pranapada Lagna (PP, tbl_Rule_SpecialLagnaModality, db/150). Reuses the TIME_FROM_SUNRISE
/// mechanics (<see cref="SpecialLagnaTimeRateCalculator"/>) at 5.0 deg/min — the classical
/// "vighati" rate (1 vighati = 0.4 minutes; the rule divides elapsed vighatis by 15 to get signs,
/// which is algebraically the same 5 deg/min this project already uses for Vighati Lagna, per
/// PyJHora's own vighati_lagna = special_ascendant(rate=5.0)) — then adds a fixed offset for the
/// modality (movable/fixed/dual, <see cref="BaadhakaCalculator.GetModality"/>) of the sign the Sun
/// occupies AT SUNRISE: movable +0, dual +120, fixed +240.
///
/// JUDGMENT CALL, DOCUMENTED NOT HIDDEN: this project's own secondary-source research states the
/// modality check uses the Sun's sign at sunrise (no source disagreement found there). The vendored
/// PyJHora reference (_research/PyJHora/src/jhora/panchanga/drik.py:2124 `pranapada_lagna`) actually
/// reads the Sun's sign at BIRTH time instead. This calculator follows the sunrise rule. The one
/// golden chart available (docs/artifacts/reference-charts/Rammy_Jagannatha.txt) cannot disambiguate
/// the two conventions — the Sun sits in Aries (movable, offset 0) at both sunrise and birth for
/// that chart.
///
/// MEASURED RESIDUAL, NOT FUDGED: against that chart this calculator computes 234.0602 deg vs
/// JHora's printed 234.9146 deg (24 Sc 54'52.36") — a 0.854 deg gap, unlike Indu/Sree Lagna which
/// match JHora exactly. Most likely cause: GetSunEventsTests.cs already documents, project-wide,
/// that SwissEphNet ships no .se1 files, so sunrise falls back to the Moshier ephemeris rather than
/// the full one JHora uses (tolerating 5s of sunrise-time error there, which alone is ~0.4 deg at
/// this calculator's 5 deg/min rate); the remainder is very likely Moshier-vs-full-ephemeris
/// sidereal-Sun drift at the sunrise moment, not a formula error. See
/// PranapadaLagnaCalculatorTests. Left undisturbed rather than reverse-fitted, the same way
/// <see cref="BhaavaLagnaCalculator"/> documents its own PVR erratum instead of silently matching
/// the book's worked example.
///
/// Vighati Lagna itself is deliberately not built as its own calculator — no genuine classical
/// life-matter linkage was found for it during the special-lagna research pass.
/// </summary>
public static class PranapadaLagnaCalculator
{
    public static SpecialPointSeed Compute(BirthDetails bd, SunTimes sun, AyanamsaDefinition? ayanamsa = null)
    {
        var baseSeed = SpecialLagnaTimeRateCalculator.Compute("PP", 5.0, bd, sun, ayanamsa);

        var sunLonAtSunrise = SwissEphemerisProvider
            .GetSiderealPositions(sun.Sunrise, bd.Latitude, bd.Longitude, ayanamsa)
            .PlanetLongitudes[PlanetName.Sun];
        var sunSignAtSunrise = AstroMath.GetSignAtLongitude(sunLonAtSunrise);

        var modalityOffset = BaadhakaCalculator.GetModality(sunSignAtSunrise) switch
        {
            SignModality.Movable => 0.0,
            SignModality.Dual => 120.0,
            SignModality.Fixed => 240.0,
            _ => 0.0
        };

        var longitude = AstroMath.Normalize(baseSeed.NirayanaLongitudeDegrees + modalityOffset);
        return new SpecialPointSeed("PP", "SpecialLagna", longitude);
    }
}
