using Ikiastrro.Core.Engines.Karakas;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>SreeLagnaCalculator against PVR's own worked Example 10 (sec 5.7) and the
/// hand-transcribed JHora export for 1_Ramakrishnan (no ephemeris dependency — SL is pure
/// over Lagna + Moon longitude, unlike the sunrise-anchored Bhaava/Hora/Ghati Lagna).</summary>
public class SreeLagnaCalculatorTests
{
    [Fact]
    public void Pvr_example_10_moon_swathi_lagna_virgo()
    {
        // PVR sec 5.7 Example 10: Moon 13 Li 06 (180+13.1=193.1), Lagna 25 Vi 05 (150+25.0833=175.0833).
        // Moon's Swathi-fraction 0.4825 x 360 = 173.7 deg -> SL = 175.0833 + 173.7 = 348.7833 (18 Pi 47').
        var moon = 180 + 13 + 6 / 60.0;
        var lagna = 150 + 25 + 5 / 60.0;

        var sl = SreeLagnaCalculator.Compute(lagna, moon);

        Assert.Equal(348.78, sl.NirayanaLongitudeDegrees, precision: 1);
    }

    [Fact]
    public void Ramakrishnan_sree_lagna_matches_jhora()
    {
        // JHora: Lagna 0 Ar 38'51" (0.6475), Moon 7 Sc 17'34" (217.2928). SL: 17 Cn 33'01" (107.5502).
        var lagna = 0 + 38 / 60.0 + 50.97 / 3600.0;
        var moon = 7 * 30 + 7 + 17 / 60.0 + 33.70 / 3600.0;

        var sl = SreeLagnaCalculator.Compute(lagna, moon);

        Assert.Equal(107.55, sl.NirayanaLongitudeDegrees, precision: 1);
    }
}
