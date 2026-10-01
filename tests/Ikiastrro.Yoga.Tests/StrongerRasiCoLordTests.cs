using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR sec.15.5.2 "Stronger Rasi" for Aquarius / Scorpio: both co-lords count in rule 2
/// (Exercise 26 answer 5), and the stronger co-lord is "the lord" in rules 4 and 6. In each
/// fixture the sole-lord reading (Saturn) would hand the comparison to Leo by rule 6, because
/// the Sun is further advanced than Saturn.</summary>
public sealed class StrongerRasiCoLordTests
{
    private static ChartAnalysisInput Chart(params (PlanetName Planet, ZodiacName Sign, double Degree)[] placements) =>
        new("D1", ZodiacName.Aries,
        [
            new PlanetPosition { Planet = "Ascendant", Sign = "Aries", NirayanaLongitudeDegrees = 10 },
            .. placements.Select(p => new PlanetPosition
            {
                Planet = p.Planet.ToString(), Sign = p.Sign.ToString(),
                NirayanaLongitudeDegrees = (int)p.Sign * 30 + p.Degree,
            }),
        ]);

    [Fact]
    public void Rule_2_counts_the_co_lords_aspect()
    {
        // Exercise 26 (5): Le and Aq empty; nothing of Jupiter, Mercury, Sun reaches Le;
        // "Aq is aspected by co-lord Rahu … Aq is stronger than Le, from rule (2)."
        var chart = Chart(
            (PlanetName.Sun, ZodiacName.Gemini, 25), (PlanetName.Moon, ZodiacName.Gemini, 10),
            (PlanetName.Mars, ZodiacName.Gemini, 10), (PlanetName.Mercury, ZodiacName.Virgo, 10),
            (PlanetName.Jupiter, ZodiacName.Virgo, 10), (PlanetName.Venus, ZodiacName.Gemini, 10),
            (PlanetName.Saturn, ZodiacName.Gemini, 5), (PlanetName.Rahu, ZodiacName.Cancer, 10),
            (PlanetName.Ketu, ZodiacName.Capricornus, 10));
        Assert.Equal(ZodiacName.Aquarius, StrongerRasiComparator.Compare(ZodiacName.Leo, ZodiacName.Aquarius, chart));
    }

    [Fact]
    public void Rule_4_reads_the_stronger_co_lord()
    {
        // Rules 1-3 tie (Moon in Le, Saturn in Aq; Sun aspects Le, Saturn occupies Aq; no
        // exaltation). Saturn sits in Aq, so Rahu is Aq's stronger lord (basic rule); Rahu in
        // Vi (even) differs in oddity from Aq (odd), while Le's lord Sun in Li (odd) does not.
        var chart = Chart(
            (PlanetName.Sun, ZodiacName.Libra, 25), (PlanetName.Moon, ZodiacName.Leo, 10),
            (PlanetName.Mars, ZodiacName.Gemini, 10), (PlanetName.Mercury, ZodiacName.Virgo, 10),
            (PlanetName.Jupiter, ZodiacName.Gemini, 10), (PlanetName.Venus, ZodiacName.Gemini, 10),
            (PlanetName.Saturn, ZodiacName.Aquarius, 5), (PlanetName.Rahu, ZodiacName.Virgo, 10),
            (PlanetName.Ketu, ZodiacName.Pisces, 10));
        Assert.Equal(ZodiacName.Aquarius, StrongerRasiComparator.Compare(ZodiacName.Leo, ZodiacName.Aquarius, chart));
    }
}
