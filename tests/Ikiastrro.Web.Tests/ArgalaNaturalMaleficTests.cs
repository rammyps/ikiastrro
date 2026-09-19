using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using static Ikiastrro.Core.Engines.Houses.ArgalaCalculator;

namespace Ikiastrro.Web.Tests;

/// <summary>
/// ArgalaCalculator.IsNaturalMalefic — the Moon/Mercury Conditional resolution added 2026-09-19
/// (tbl_Planets.NaturalNature: Sun/Mars/Saturn/Rahu/Ketu always Malefic, Jupiter/Venus always
/// Benefic, Moon/Mercury Conditional). Fixed planets need no chart context; Moon/Mercury do.
/// </summary>
public sealed class ArgalaNaturalMaleficTests
{
    private static IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> Chart(
        params (ZodiacName Sign, PlanetName Planet)[] placements) =>
        placements.GroupBy(p => p.Sign)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<PlanetName>)g.Select(p => p.Planet).ToList());

    [Theory]
    [InlineData(PlanetName.Sun, true)]
    [InlineData(PlanetName.Mars, true)]
    [InlineData(PlanetName.Saturn, true)]
    [InlineData(PlanetName.Rahu, true)]
    [InlineData(PlanetName.Ketu, true)]
    [InlineData(PlanetName.Jupiter, false)]
    [InlineData(PlanetName.Venus, false)]
    public void Fixed_Planets_Ignore_Chart_Context(PlanetName planet, bool expectedMalefic)
    {
        var empty = new Dictionary<ZodiacName, IReadOnlyList<PlanetName>>();
        Assert.Equal(expectedMalefic, IsNaturalMalefic(planet, empty));
    }

    [Fact]
    public void Moon_Is_Malefic_When_Waning_More_Than_Six_Signs_Ahead_Of_Sun()
    {
        // Sun in Taurus, Moon in Aries: 12 signs ahead (i.e. just short of conjunction — deep
        // Krishna Paksha, the tail end of waning) — same as Chart 5's own Sun/Moon placement.
        var chart = Chart((ZodiacName.Taurus, PlanetName.Sun), (ZodiacName.Aries, PlanetName.Moon));
        Assert.True(IsNaturalMalefic(PlanetName.Moon, chart));
    }

    [Fact]
    public void Moon_Is_Benefic_When_Waxing_Within_Six_Signs_Of_Sun()
    {
        // Sun in Aries, Moon in Leo: 5 signs ahead — Shukla Paksha (waxing, pre-Full Moon).
        var chart = Chart((ZodiacName.Aries, PlanetName.Sun), (ZodiacName.Leo, PlanetName.Moon));
        Assert.False(IsNaturalMalefic(PlanetName.Moon, chart));
    }

    [Fact]
    public void Moon_Same_Sign_As_Sun_Is_Benefic_Boundary_Case()
    {
        // Conjunction itself (offset 1) is the start of Shukla Paksha by classical convention.
        var chart = Chart((ZodiacName.Aries, PlanetName.Sun), (ZodiacName.Aries, PlanetName.Moon));
        Assert.False(IsNaturalMalefic(PlanetName.Moon, chart));
    }

    [Fact]
    public void Moon_Falls_Back_To_Benefic_When_Sun_Is_Missing_From_Occupancy()
    {
        var chart = Chart((ZodiacName.Aries, PlanetName.Moon));
        Assert.False(IsNaturalMalefic(PlanetName.Moon, chart));
    }

    [Fact]
    public void Mercury_Is_Malefic_When_Conjunct_A_Fixed_Malefic()
    {
        var chart = Chart((ZodiacName.Gemini, PlanetName.Mercury), (ZodiacName.Gemini, PlanetName.Mars));
        Assert.True(IsNaturalMalefic(PlanetName.Mercury, chart));
    }

    [Fact]
    public void Mercury_Is_Benefic_When_Alone()
    {
        var chart = Chart((ZodiacName.Gemini, PlanetName.Mercury));
        Assert.False(IsNaturalMalefic(PlanetName.Mercury, chart));
    }

    [Fact]
    public void Mercury_Is_Benefic_When_Conjunct_Only_Benefics()
    {
        var chart = Chart((ZodiacName.Gemini, PlanetName.Mercury), (ZodiacName.Gemini, PlanetName.Venus));
        Assert.False(IsNaturalMalefic(PlanetName.Mercury, chart));
    }

    [Fact]
    public void Mercury_Missing_From_Occupancy_Is_Benefic()
    {
        var chart = new Dictionary<ZodiacName, IReadOnlyList<PlanetName>>();
        Assert.False(IsNaturalMalefic(PlanetName.Mercury, chart));
    }

    // ---- End-to-end: the sec.10.6 exception can now fire via a Conditional-malefic Moon ----
    [Fact]
    public void ThirdHouse_Exception_Can_Fire_Via_A_Waning_Moon_Plus_A_Fixed_Malefic()
    {
        // Target Aries; 3rd-from-target = Gemini. Put a fixed malefic (Mars) there plus a Moon
        // that's waning relative to the Sun (Sun in Virgo -> Moon in Gemini is 10 signs ahead,
        // past the Full Moon point) — before this change Moon always counted as benefic, so this
        // configuration could never trigger the exception.
        var chart = Chart(
            (ZodiacName.Gemini, PlanetName.Mars),
            (ZodiacName.Gemini, PlanetName.Moon),
            (ZodiacName.Virgo, PlanetName.Sun));

        var eval = Evaluate(ZodiacName.Aries, chart);

        Assert.True(eval.ThirdHouseExceptionApplied);
        Assert.Equal(new[] { PlanetName.Mars, PlanetName.Moon },
            eval.Argala.Single(p => p.Position.HouseOffset == 3).Occupants);
    }
}
