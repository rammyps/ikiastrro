using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Punya Saham (PS) — "Fortune/good deeds," the first row of PVR's Table 74 (sec 28.8, Tajaka
/// Analysis), the Vedic Saham that plays the role western astrology's "Part of Fortune"/Pars
/// Fortuna plays (PVR's own note, same section, draws the parallel — Sahams are not a KP
/// technique). Formula: Moon − Sun + Lagna for day births; Sun − Moon + Lagna for night births
/// (PVR's general reversal rule, sec 28.8.1, applies to every Saham in the table).
///
/// PVR's Saham method is not plain modular arithmetic — sec 28.8.1's general rule for any
/// A − B + C formula: if C does not lie on the zodiacal arc going forward from B to A, add 30°.
/// Verified against PVR's own worked example (vanik saham, sec 28.8.1): starting from
/// Moon=345°14', reaching Mercury=311°28' forward (through 0°) covers ~330° and does pass
/// through Lagna=280°50', so no 30° is added there; the samartha saham example (B=354°58',
/// A=19°10', a short ~24° forward hop) does NOT pass through C=280°50', so 30° is added. Both
/// reproduced exactly by the arc check below.
/// </summary>
public static class PunyaSahamCalculator
{
    public static SpecialPointSeed Compute(
        double sunLongitude, double moonLongitude, double natalLagnaLongitude, bool isNightBirth)
    {
        var (a, b, c) = isNightBirth
            ? (sunLongitude, moonLongitude, natalLagnaLongitude)   // night: Sun - Moon + Lagna
            : (moonLongitude, sunLongitude, natalLagnaLongitude);  // day:   Moon - Sun + Lagna

        var arc = AstroMath.Normalize(a - b);
        var distanceToC = AstroMath.Normalize(c - b);
        var onArc = distanceToC <= arc;

        var longitude = AstroMath.Normalize(a - b + c + (onArc ? 0.0 : 30.0));
        return new SpecialPointSeed("PS", "SpecialLagna", longitude);
    }
}
