using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR sec.17.2.3: the three views on when Ashtottari dasa applies.</summary>
public sealed class AshtottariApplicabilityTests
{
    private static ChartAnalysisInput Chart(ZodiacName lagna, ZodiacName rahu, ZodiacName lagnaLord) =>
        new("D1", lagna,
        [
            new PlanetPosition { Planet = "Ascendant", Sign = lagna.ToString(), NirayanaLongitudeDegrees = (int)lagna * 30 + 10 },
            .. new (PlanetName Planet, ZodiacName Sign)[]
            {
                (PlanetName.Sun, ZodiacName.Leo), (PlanetName.Moon, ZodiacName.Leo), (PlanetName.Mars, lagnaLord), (PlanetName.Mercury, ZodiacName.Leo),
                (PlanetName.Jupiter, ZodiacName.Leo), (PlanetName.Venus, ZodiacName.Leo), (PlanetName.Saturn, ZodiacName.Leo),
                (PlanetName.Rahu, rahu), (PlanetName.Ketu, (ZodiacName)(((int)rahu + 6) % 12)),
            }.Select(p => new PlanetPosition { Planet = p.Planet.ToString(), Sign = p.Sign.ToString(), NirayanaLongitudeDegrees = (int)p.Sign * 30 + 10 }),
        ]);

    [Fact]
    public void First_view_always_applies()
    {
        var views = AshtottariApplicability.Evaluate(Chart(ZodiacName.Aries, ZodiacName.Gemini, ZodiacName.Leo), false, false);
        Assert.True(views[0].Holds);
        Assert.Equal(3, views.Count);
    }

    [Theory]
    [InlineData(ZodiacName.Leo, true)]          // Rahu in Leo, the same sign as the Lagna lord Mars (Mars in Leo): the 1st from it
    [InlineData(ZodiacName.Sagittarius, true)]  // 5th from Leo
    [InlineData(ZodiacName.Scorpio, true)]      // 4th from Leo, a quadrant
    [InlineData(ZodiacName.Virgo, false)]       // 2nd from Leo
    [InlineData(ZodiacName.Libra, false)]       // 3rd from Leo
    public void Second_view_needs_rahu_in_a_quadrant_or_trine_from_the_lagna_lord(ZodiacName rahu, bool expected)
    {
        // Aries Lagna, its lord Mars in Leo; none of these rows puts Rahu in Aries, the Lagna.
        var views = AshtottariApplicability.Evaluate(ChartWithMarsAsLord(rahu), false, false);
        Assert.Equal(expected, views[1].Holds);
    }

    // Aries Lagna: its lord is Mars, placed in Leo.
    private static ChartAnalysisInput ChartWithMarsAsLord(ZodiacName rahu) => Chart(ZodiacName.Aries, rahu, ZodiacName.Leo);

    [Fact]
    public void Rahu_in_the_lagna_fails_the_second_view_even_in_a_trine()
    {
        // Lagna Leo's lord is the Sun (in Leo), so Rahu in Leo would be the 1st from it, but Rahu is in the Lagna.
        var views = AshtottariApplicability.Evaluate(Chart(ZodiacName.Leo, ZodiacName.Leo, ZodiacName.Leo), false, false);
        Assert.False(views[1].Holds);
        Assert.Contains("the Lagna", views[1].Detail);
    }

    [Theory]
    [InlineData(false, true, true)]   // day birth, waning Moon (Krishna)
    [InlineData(true, false, true)]     // night birth, waxing Moon (Shukla)
    [InlineData(false, false, false)]   // day birth, Shukla
    [InlineData(true, true, false)]   // night birth, Krishna
    public void Third_view_pairs_day_with_krishna_and_night_with_shukla(bool night, bool krishna, bool expected)
    {
        var views = AshtottariApplicability.Evaluate(Chart(ZodiacName.Aries, ZodiacName.Gemini, ZodiacName.Leo), night, krishna);
        Assert.Equal(expected, views[2].Holds);
    }
}
