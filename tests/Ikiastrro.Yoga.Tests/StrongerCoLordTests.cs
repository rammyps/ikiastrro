using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR sec.15.5.1 "Stronger Co-Lord", using the book's own illustrations of each rule.</summary>
public sealed class StrongerCoLordTests
{
    private static ChartAnalysisInput Chart(ZodiacName lagna, params (PlanetName Planet, ZodiacName Sign, double Degree)[] placements) =>
        new("D1", lagna,
        [
            new PlanetPosition { Planet = "Ascendant", Sign = lagna.ToString(), NirayanaLongitudeDegrees = (int)lagna * 30 + 10 },
            .. placements.Select(p => new PlanetPosition
            {
                Planet = p.Planet.ToString(), Sign = p.Sign.ToString(),
                NirayanaLongitudeDegrees = (int)p.Sign * 30 + p.Degree,
            }),
        ]);

    [Fact]
    public void Basic_rule_a_co_lord_in_the_sign_gives_it_to_the_other()
    {
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Scorpio, 10), (PlanetName.Ketu, ZodiacName.Aries, 10),
            (PlanetName.Saturn, ZodiacName.Aquarius, 10), (PlanetName.Rahu, ZodiacName.Libra, 10));
        Assert.Equal(PlanetName.Ketu, StrongerCoLord.For(ZodiacName.Scorpio, chart));
        Assert.Equal(PlanetName.Rahu, StrongerCoLord.For(ZodiacName.Aquarius, chart));
    }

    [Fact]
    public void Rule_1_joined_by_more_planets()
    {
        // "Saturn is in Pi with Mars and Sun and Rahu is in Ar with Jupiter."
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Saturn, ZodiacName.Pisces, 10), (PlanetName.Mars, ZodiacName.Pisces, 10), (PlanetName.Sun, ZodiacName.Pisces, 10),
            (PlanetName.Rahu, ZodiacName.Aries, 10), (PlanetName.Jupiter, ZodiacName.Aries, 10));
        Assert.Equal(PlanetName.Saturn, StrongerCoLord.For(ZodiacName.Aquarius, chart));
    }

    [Fact]
    public void Rule_2_jupiter_mercury_and_dispositor_by_rasi_drishti()
    {
        // Saturn and Rahu each alone (rule 1 ties). Saturn in Ge is aspected by Jupiter from Pi and
        // conjoined by nobody; its dispositor Mercury aspects from Vi -> 2. Rahu in Ar: dispositor
        // Mars in Le (fixed aspects movable except the adjacent Cn) -> 1.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Saturn, ZodiacName.Gemini, 10), (PlanetName.Rahu, ZodiacName.Aries, 10),
            (PlanetName.Jupiter, ZodiacName.Pisces, 10), (PlanetName.Mercury, ZodiacName.Virgo, 10),
            (PlanetName.Mars, ZodiacName.Leo, 10));
        Assert.Equal(PlanetName.Saturn, StrongerCoLord.For(ZodiacName.Aquarius, chart));
    }

    [Fact]
    public void Rule_4_dual_over_fixed()
    {
        // "Mars is in Ge and Ketu is in Aq": Jupiter, Mercury and Saturn in Ta reach neither.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Gemini, 10), (PlanetName.Ketu, ZodiacName.Aquarius, 10),
            (PlanetName.Rahu, ZodiacName.Leo, 10),
            (PlanetName.Jupiter, ZodiacName.Taurus, 10), (PlanetName.Mercury, ZodiacName.Taurus, 10), (PlanetName.Saturn, ZodiacName.Taurus, 10));
        Assert.Equal(PlanetName.Mars, StrongerCoLord.For(ZodiacName.Scorpio, chart));
    }

    [Fact]
    public void Rule_5b_advancement_with_nodes_from_the_end()
    {
        // "Mars is at 23Li17 and Ketu is at 5Cn54 … Ketu is more advanced" (24°06' vs 23°17').
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Libra, 23 + 17 / 60.0), (PlanetName.Ketu, ZodiacName.Cancer, 5 + 54 / 60.0),
            (PlanetName.Rahu, ZodiacName.Capricornus, 5 + 54 / 60.0),
            (PlanetName.Jupiter, ZodiacName.Gemini, 10), (PlanetName.Mercury, ZodiacName.Gemini, 10),
            (PlanetName.Venus, ZodiacName.Gemini, 10), (PlanetName.Moon, ZodiacName.Gemini, 10));
        Assert.Equal(PlanetName.Ketu, StrongerCoLord.For(ZodiacName.Scorpio, chart));
    }

    [Fact]
    public void Single_lord_signs_are_unchanged() =>
        Assert.Equal(PlanetName.Mars, StrongerCoLord.For(ZodiacName.Aries, Chart(ZodiacName.Aries)));

    [Fact]
    public void Arudha_lagna_in_scorpio_uses_ketu_when_mars_sits_in_scorpio()
    {
        // Lagna Sc with Mars in it -> Ketu rules: Sc to Ketu's Cn is 9, 9 from Cn is Pi.
        // (Mars as lord would give Sc itself, then the 10th from it: Le.)
        var chart = Chart(ZodiacName.Scorpio,
            (PlanetName.Sun, ZodiacName.Aries, 1), (PlanetName.Moon, ZodiacName.Aries, 1), (PlanetName.Mars, ZodiacName.Scorpio, 1),
            (PlanetName.Mercury, ZodiacName.Aries, 1), (PlanetName.Jupiter, ZodiacName.Aries, 1), (PlanetName.Venus, ZodiacName.Aries, 1),
            (PlanetName.Saturn, ZodiacName.Aries, 1), (PlanetName.Rahu, ZodiacName.Capricornus, 1), (PlanetName.Ketu, ZodiacName.Cancer, 1));
        var al = ArudhaCalculator.Compute(chart).Single(s => s.Code == "AL");
        Assert.Equal(ZodiacName.Pisces, (ZodiacName)(int)(al.NirayanaLongitudeDegrees / 30));
    }

    [Fact]
    public void Explain_names_the_stronger_co_lord_and_the_rule_that_decided_it()
    {
        // Rule 1 example: Saturn in Pi with Mars and Sun, Rahu in Ar with Jupiter.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Saturn, ZodiacName.Pisces, 10), (PlanetName.Mars, ZodiacName.Pisces, 10), (PlanetName.Sun, ZodiacName.Pisces, 10),
            (PlanetName.Rahu, ZodiacName.Aries, 10), (PlanetName.Jupiter, ZodiacName.Aries, 10));
        var r = StrongerCoLord.Explain(ZodiacName.Aquarius, chart)!;
        Assert.Equal(PlanetName.Saturn, r.Primary);
        Assert.Equal(PlanetName.Rahu, r.CoLord);
        Assert.Equal(PlanetName.Saturn, r.Stronger);
        Assert.Equal(StrongerCoLord.For(ZodiacName.Aquarius, chart), r.Stronger);   // same decision as For
        Assert.Contains("joined by more planets (2 against 1)", r.Rule);
    }

    [Fact]
    public void Explain_basic_rule_and_single_lord_signs()
    {
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Scorpio, 10), (PlanetName.Ketu, ZodiacName.Aries, 10),
            (PlanetName.Saturn, ZodiacName.Aquarius, 10), (PlanetName.Rahu, ZodiacName.Libra, 10));
        var scorpio = StrongerCoLord.Explain(ZodiacName.Scorpio, chart)!;
        Assert.Equal(PlanetName.Ketu, scorpio.Stronger);
        Assert.Contains("Mars is in Scorpio, so Ketu rules it", scorpio.Rule);
        Assert.Null(StrongerCoLord.Explain(ZodiacName.Aries, chart));   // one lord: nothing to explain
    }
}
