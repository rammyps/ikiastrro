using Ikiastrro.Core.Engines.Karakas;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>InduLagnaCalculator against the hand-transcribed JHora export for 1_Ramakrishnan
/// (no ephemeris dependency — IL is pure over Lagna + Moon longitude, like Sree Lagna).</summary>
public class InduLagnaCalculatorTests
{
    [Fact]
    public void Ramakrishnan_indu_lagna_matches_jhora()
    {
        // JHora: Lagna 0 Ar 38'50.97" (0.6475 deg, Aries), Moon 7 Sc 17'33.70" (217.2928 deg, Scorpio).
        // 9th from Lagna(Aries) = Sagittarius, lord Jupiter, Kala 10.
        // 9th from Moon(Scorpio) = Cancer, lord Moon, Kala 16.
        // Sum 26 mod 12 = 2 -> count 2 signs forward from Moon's sign inclusive = +1 sign (Scorpio -> Sagittarius).
        // IL = 217.2928 + 30 = 247.2928 (7 Sg 17'33.70") -- expect an EXACT match, unlike Pranapada's residual.
        var lagna = 0 + 38 / 60.0 + 50.97 / 3600.0;
        var moon = 7 * 30 + 7 + 17 / 60.0 + 33.70 / 3600.0;

        var il = InduLagnaCalculator.Compute(lagna, moon);

        Assert.Equal(247.2928, il.NirayanaLongitudeDegrees, precision: 3);
    }
}
