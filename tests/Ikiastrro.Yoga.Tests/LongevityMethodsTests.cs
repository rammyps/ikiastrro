using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.KeyInfo;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR sec.14.4 "The Method of Three Pairs" (Tables 33 and 34, Example 47) and sec.14.5 "The Eighth Lord
/// Method" (Example 48).</summary>
public sealed class LongevityMethodsTests
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

    [Theory]
    [InlineData(ZodiacName.Taurus, ZodiacName.Gemini, LongevityCategory.Long)]        // Fixed + Dual
    [InlineData(ZodiacName.Gemini, ZodiacName.Taurus, LongevityCategory.Long)]        // order does not matter
    [InlineData(ZodiacName.Aries, ZodiacName.Cancer, LongevityCategory.Long)]         // Movable + Movable
    [InlineData(ZodiacName.Aries, ZodiacName.Taurus, LongevityCategory.Middle)]       // Movable + Fixed
    [InlineData(ZodiacName.Gemini, ZodiacName.Virgo, LongevityCategory.Middle)]       // Dual + Dual
    [InlineData(ZodiacName.Aries, ZodiacName.Gemini, LongevityCategory.Short)]        // Movable + Dual
    [InlineData(ZodiacName.Taurus, ZodiacName.Leo, LongevityCategory.Short)]          // Fixed + Fixed
    public void Table_33_gives_the_category_for_each_modality_combination(ZodiacName a, ZodiacName b, LongevityCategory expected) =>
        Assert.Equal(expected, LongevityMethods.PairCategory(a, b));

    [Fact]
    public void Example_47_two_pairs_long_and_one_middle_is_long_life_with_paramaayush_108()
    {
        // Lagna Ta, HL Ar, Moon Ta, Mercury Cn, Venus Cp, Saturn Ge.
        var chart = Chart(ZodiacName.Taurus,
            (PlanetName.Moon, ZodiacName.Taurus, 10), (PlanetName.Mercury, ZodiacName.Cancer, 10), (PlanetName.Venus, ZodiacName.Capricornus, 10),
            (PlanetName.Saturn, ZodiacName.Gemini, 10), (PlanetName.Sun, ZodiacName.Leo, 10), (PlanetName.Mars, ZodiacName.Virgo, 10),
            (PlanetName.Jupiter, ZodiacName.Libra, 10), (PlanetName.Rahu, ZodiacName.Aquarius, 10), (PlanetName.Ketu, ZodiacName.Leo, 20));
        var r = LongevityMethods.ReadThreePairs(chart, ZodiacName.Aries)!;
        Assert.Equal([LongevityCategory.Long, LongevityCategory.Long, LongevityCategory.Middle], r.Pairs.Select(p => p.Category));
        Assert.Equal(LongevityCategory.Long, r.Category);
        Assert.Equal(108, r.Paramaayush);
        Assert.Equal(PlanetName.Mercury.ToString(), r.Pairs[0].SecondLabel.Split(' ')[^1]);   // 8th lord by Table 32 is Mercury
    }

    [Fact]
    public void All_three_different_prefers_lagna_and_hora_lagna()
    {
        // Pair 1 long (Mars with the Sun in Cancer is both Lagna lord and 8th lord: Movable+Movable), pair 2 short
        // (Moon Ta + Saturn Le: Fixed+Fixed), pair 3 middle (Lagna Ar + HL Ta: Movable+Fixed). The Moon is not in the Lagna or 7th.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Cancer, 10), (PlanetName.Sun, ZodiacName.Cancer, 12), (PlanetName.Ketu, ZodiacName.Pisces, 10),
            (PlanetName.Moon, ZodiacName.Taurus, 10), (PlanetName.Saturn, ZodiacName.Leo, 10), (PlanetName.Mercury, ZodiacName.Virgo, 10),
            (PlanetName.Jupiter, ZodiacName.Sagittarius, 10), (PlanetName.Venus, ZodiacName.Libra, 10), (PlanetName.Rahu, ZodiacName.Virgo, 20));
        var r = LongevityMethods.ReadThreePairs(chart, ZodiacName.Taurus)!;
        Assert.Equal([LongevityCategory.Long, LongevityCategory.Short, LongevityCategory.Middle], r.Pairs.Select(p => p.Category));
        Assert.Equal(LongevityCategory.Middle, r.Category);
        Assert.Null(r.Paramaayush);
        Assert.Contains("Lagna and Hora Lagna", r.Why);
    }

    [Fact]
    public void All_three_different_with_the_moon_in_the_lagna_prefers_moon_and_saturn()
    {
        // As above, but the Moon is in the Lagna (Ar, movable) with Saturn in Ge (dual): Movable+Dual is short.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Cancer, 10), (PlanetName.Sun, ZodiacName.Cancer, 12), (PlanetName.Ketu, ZodiacName.Pisces, 10),
            (PlanetName.Moon, ZodiacName.Aries, 10), (PlanetName.Saturn, ZodiacName.Gemini, 10), (PlanetName.Mercury, ZodiacName.Virgo, 10),
            (PlanetName.Jupiter, ZodiacName.Sagittarius, 10), (PlanetName.Venus, ZodiacName.Libra, 10), (PlanetName.Rahu, ZodiacName.Virgo, 20));
        var r = LongevityMethods.ReadThreePairs(chart, ZodiacName.Taurus)!;
        Assert.Equal([LongevityCategory.Long, LongevityCategory.Short, LongevityCategory.Middle], r.Pairs.Select(p => p.Category));
        Assert.Equal(LongevityCategory.Short, r.Category);
        Assert.Contains("Moon and Saturn", r.Why);
    }

    [Theory]
    [InlineData(ZodiacName.Libra, LongevityCategory.Long)]         // quadrant from Libra
    [InlineData(ZodiacName.Capricornus, LongevityCategory.Long)]
    [InlineData(ZodiacName.Scorpio, LongevityCategory.Middle)]     // panaphara
    [InlineData(ZodiacName.Leo, LongevityCategory.Middle)]
    [InlineData(ZodiacName.Sagittarius, LongevityCategory.Short)]  // apoklima
    [InlineData(ZodiacName.Virgo, LongevityCategory.Short)]
    public void Example_48_the_8th_lord_from_the_stronger_reference_gives_the_category_by_its_house(ZodiacName venus, LongevityCategory expected)
    {
        // Lagna Ar, Libra stronger (it holds Sun, Moon and Mars; Aries holds nothing): the 8th from Libra is Taurus, lord Venus.
        // Venus is moved around; where it joins Libra the reference stays Libra because Venus only adds to Libra's count.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Sun, ZodiacName.Libra, 10), (PlanetName.Moon, ZodiacName.Libra, 12), (PlanetName.Mars, ZodiacName.Libra, 14),
            (PlanetName.Venus, venus, 10), (PlanetName.Mercury, ZodiacName.Gemini, 10), (PlanetName.Jupiter, ZodiacName.Gemini, 12),
            (PlanetName.Saturn, ZodiacName.Gemini, 14), (PlanetName.Rahu, ZodiacName.Gemini, 16), (PlanetName.Ketu, ZodiacName.Sagittarius, 16));
        var r = LongevityMethods.ReadEighthLord(chart)!;
        Assert.Equal(ZodiacName.Libra, r.Reference);
        Assert.Equal(ZodiacName.Taurus, r.EighthSign);
        Assert.Equal(PlanetName.Venus, r.EighthLord);
        Assert.Equal(expected, r.Category);
    }
}
