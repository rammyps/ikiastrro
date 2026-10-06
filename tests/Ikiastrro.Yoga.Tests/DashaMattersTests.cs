using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR sec.16.5.1: the nine examples of a planet giving the results promised by its divisional-chart positions.</summary>
public sealed class DashaMattersTests
{
    private static readonly PlanetName[] All =
        [PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn, PlanetName.Rahu, PlanetName.Ketu];

    /// <summary>Every planet in Gemini at 10 degrees unless placed; the points are named signs.</summary>
    private static DashaMatterChart Chart(string type, ZodiacName lagna, IEnumerable<(PlanetName Planet, ZodiacName Sign)> placed,
        params (string Point, ZodiacName Sign)[] points)
    {
        var at = placed.ToDictionary(p => p.Planet, p => p.Sign);
        var planets = new List<PlanetPosition>
            { new() { Planet = "Ascendant", Sign = lagna.ToString(), NirayanaLongitudeDegrees = (int)lagna * 30 + 10 } };
        planets.AddRange(All.Select(p =>
        {
            var sign = at.GetValueOrDefault(p, ZodiacName.Gemini);
            return new PlanetPosition { Planet = p.ToString(), Sign = sign.ToString(), NirayanaLongitudeDegrees = (int)sign * 30 + 10 };
        }));
        return new(new ChartAnalysisInput(type, lagna, planets), points.ToDictionary(p => p.Point, p => p.Sign));
    }

    private static Dictionary<string, DashaMatterChart> Charts(params DashaMatterChart[] charts) => charts.ToDictionary(c => c.Chart.ChartType);

    private static IReadOnlyList<PlanetName> Planets(IReadOnlyList<DashaMatterRule> rules, int number) =>
        rules.Single(r => r.Number == number).Planets.Select(p => p.Planet).ToList();

    [Fact]
    public void Lords_of_the_5th_in_D7_8th_in_D1_and_7th_in_D9()
    {
        var rules = DashaMatters.Evaluate(Charts(
            Chart("D7", ZodiacName.Aries, []), Chart("D1", ZodiacName.Taurus, []), Chart("D9", ZodiacName.Aries, [])), null);
        Assert.Equal([PlanetName.Sun], Planets(rules, 1));      // 5th from Aries is Leo
        Assert.Equal([PlanetName.Jupiter], Planets(rules, 2));  // 8th from Taurus is Sagittarius
        Assert.Equal([PlanetName.Venus], Planets(rules, 5));    // 7th from Aries is Libra
    }

    [Fact]
    public void Exalted_planet_in_GL_in_D10_leaves_out_the_unexalted()
    {
        var rules = DashaMatters.Evaluate(Charts(Chart("D10", ZodiacName.Leo,
            [(PlanetName.Sun, ZodiacName.Aries), (PlanetName.Moon, ZodiacName.Aries)], ("GL", ZodiacName.Aries))), null);
        Assert.Equal([PlanetName.Sun], Planets(rules, 3));
    }

    [Fact]
    public void Exalted_planet_in_the_12th_from_AK_in_D9()
    {
        var charts = Charts(Chart("D9", ZodiacName.Aries, [(PlanetName.Moon, ZodiacName.Libra), (PlanetName.Mercury, ZodiacName.Virgo)]));
        Assert.Equal([PlanetName.Mercury], Planets(DashaMatters.Evaluate(charts, PlanetName.Moon), 4));
        Assert.Empty(Planets(DashaMatters.Evaluate(charts, PlanetName.Mercury), 4));   // 12th from Virgo is Leo
        Assert.Equal("Needs the Atma Karaka.", DashaMatters.Evaluate(charts, null).Single(r => r.Number == 4).NotEvaluated);
    }

    [Fact]
    public void Planet_with_Rahu_in_the_9th_in_D4_only_when_Rahu_is_in_the_9th()
    {
        var with = Chart("D4", ZodiacName.Aries, [(PlanetName.Rahu, ZodiacName.Sagittarius), (PlanetName.Mars, ZodiacName.Sagittarius)]);
        var without = Chart("D4", ZodiacName.Aries, [(PlanetName.Mars, ZodiacName.Sagittarius)]);
        Assert.Equal([PlanetName.Mars], Planets(DashaMatters.Evaluate(Charts(with), null), 6));
        Assert.Empty(Planets(DashaMatters.Evaluate(Charts(without), null), 6));
    }

    [Fact]
    public void Exalted_planet_aspecting_HL_from_the_11th_from_AL()
    {
        var d1 = Chart("D1", ZodiacName.Leo, [(PlanetName.Sun, ZodiacName.Aries)], ("AL", ZodiacName.Gemini), ("HL", ZodiacName.Leo));
        Assert.Equal([PlanetName.Sun], Planets(DashaMatters.Evaluate(Charts(d1), null), 7));   // Aries aspects Leo

        var elsewhere = Chart("D1", ZodiacName.Leo, [(PlanetName.Sun, ZodiacName.Aries)], ("AL", ZodiacName.Gemini), ("HL", ZodiacName.Libra));
        Assert.Empty(Planets(DashaMatters.Evaluate(Charts(elsewhere), null), 7));
    }

    [Fact]
    public void Planet_joined_by_Moon_and_Saturn_in_the_8th_in_D30()
    {
        var both = Chart("D30", ZodiacName.Aries,
            [(PlanetName.Moon, ZodiacName.Scorpio), (PlanetName.Saturn, ZodiacName.Scorpio), (PlanetName.Venus, ZodiacName.Scorpio)]);
        Assert.Equal([PlanetName.Venus], Planets(DashaMatters.Evaluate(Charts(both), null), 8));

        var moonOnly = Chart("D30", ZodiacName.Aries, [(PlanetName.Moon, ZodiacName.Scorpio), (PlanetName.Venus, ZodiacName.Scorpio)]);
        Assert.Empty(Planets(DashaMatters.Evaluate(Charts(moonOnly), null), 8));
    }

    [Fact]
    public void Well_disposed_planet_aspecting_A3_in_D10()
    {
        // Aquarius (fixed) aspects Libra (movable, not the adjacent Capricorn's side): Saturn is in its own sign, Moon is neutral there.
        var d10 = Chart("D10", ZodiacName.Aries,
            [(PlanetName.Saturn, ZodiacName.Aquarius), (PlanetName.Moon, ZodiacName.Aquarius)], ("A3", ZodiacName.Libra));
        var planets = Planets(DashaMatters.Evaluate(Charts(d10), null), 9);
        Assert.Contains(PlanetName.Saturn, planets);
        Assert.DoesNotContain(PlanetName.Moon, planets);
    }

    [Fact]
    public void Missing_chart_is_reported_not_guessed()
    {
        var rule = DashaMatters.Evaluate(Charts(), null).Single(r => r.Number == 1);
        Assert.Equal("Needs the D7 chart.", rule.NotEvaluated);
        Assert.Empty(rule.Planets);
        Assert.Equal(9, DashaMatters.Evaluate(Charts(), null).Count);
    }
}
