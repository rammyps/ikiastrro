using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Relationships;

/// <summary>One planet's combustion (Asta) evaluation against the Sun's real longitude.</summary>
public record CombustionResult(bool IsCombust, decimal DistanceFromSunDegrees, decimal OrbUsedDegrees);

/// <summary>
/// Classical combustion (Asta): a planet sitting within its orb of the Sun's real longitude is
/// "combust" — a distinct affliction layer, independent of sign-dignity (see DignityEngine).
/// Orbs are the standard BPHS/Phaladeepika values, narrower when the planet is retrograde (its
/// apparent motion reverses, changing the classically-observed combustion window) — see the
/// 2026-08-28 backlog note in ikiastrro.md for the worked example that surfaced this gap.
///
/// Applies only to the 6 classical grahas that can meaningfully be combust: Moon, Mars, Mercury,
/// Jupiter, Venus, Saturn. The Sun cannot combust itself, and Rahu/Ketu are excluded — shadow
/// points with no physical orb-of-the-Sun concept in standard Parashari texts, same convention
/// DignityEngine already applies to their more limited dignity treatment.
///
/// ChartAnalyzer always passes real (D1) NirayanaLongitudeDegrees, for every chart type, so a varga
/// row inherits its D1 combustion — combustion is astronomical, not a varga-space relationship
/// (per-varga evaluation from VargaLongitudeDegrees was tried 2026-08-28 and reverted 2026-09-28).
/// </summary>
public static class CombustionEngine
{
    /// <summary>Base orb, in degrees, used when the planet is direct (or has no retrograde-specific override below).</summary>
    private static readonly IReadOnlyDictionary<string, decimal> DirectOrbDegrees = new Dictionary<string, decimal>
    {
        ["Moon"] = 12m,
        ["Mars"] = 17m,
        ["Mercury"] = 14m,
        ["Jupiter"] = 11m,
        ["Venus"] = 10m,
        ["Saturn"] = 15m
    };

    /// <summary>Narrower orb used when the planet is retrograde. Only planets with a documented retrograde-specific orb appear here — others fall back to DirectOrbDegrees regardless of motion.</summary>
    private static readonly IReadOnlyDictionary<string, decimal> RetrogradeOrbDegrees = new Dictionary<string, decimal>
    {
        ["Mars"] = 8m,
        ["Mercury"] = 12m,
        ["Venus"] = 8m
    };

    /// <summary>True for the 6 planets combustion applies to (Moon, Mars, Mercury, Jupiter, Venus, Saturn).</summary>
    public static bool IsApplicable(string planet) => DirectOrbDegrees.ContainsKey(planet);

    /// <summary>Shortest angular separation between two sidereal longitudes, 0-180°.</summary>
    public static decimal AngularSeparation(double longitudeA, double longitudeB)
    {
        var diff = Math.Abs(AstroMath.Normalize(longitudeA) - AstroMath.Normalize(longitudeB));
        if (diff > 180) diff = 360 - diff;
        return (decimal)diff;
    }

    /// <summary>
    /// Evaluates combustion for one planet. Caller must check <see cref="IsApplicable"/> first —
    /// throws for the Sun itself, Rahu, Ketu, or the Ascendant.
    /// </summary>
    public static CombustionResult Evaluate(string planet, double planetLongitude, double sunLongitude, bool? isRetrograde)
    {
        if (!DirectOrbDegrees.TryGetValue(planet, out var directOrb))
        {
            throw new InvalidOperationException($"Combustion does not apply to '{planet}' — check IsApplicable first.");
        }

        var orb = isRetrograde == true && RetrogradeOrbDegrees.TryGetValue(planet, out var retrogradeOrb)
            ? retrogradeOrb
            : directOrb;

        var distance = AngularSeparation(planetLongitude, sunLongitude);
        return new CombustionResult(distance <= orb, Math.Round(distance, 4), orb);
    }
}
