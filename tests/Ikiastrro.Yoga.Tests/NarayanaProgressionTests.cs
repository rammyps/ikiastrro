using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR sec.18.2.1: the Narayana dasa seed and progression, against the book's Table 40 (all 36 sequences) and
/// Examples 63-65.</summary>
public sealed class NarayanaProgressionTests
{
    private static readonly Dictionary<string, ZodiacName> Abbr = new()
    {
        ["Ar"] = ZodiacName.Aries, ["Ta"] = ZodiacName.Taurus, ["Ge"] = ZodiacName.Gemini, ["Cn"] = ZodiacName.Cancer,
        ["Le"] = ZodiacName.Leo, ["Vi"] = ZodiacName.Virgo, ["Li"] = ZodiacName.Libra, ["Sc"] = ZodiacName.Scorpio,
        ["Sg"] = ZodiacName.Sagittarius, ["Cp"] = ZodiacName.Capricornus, ["Aq"] = ZodiacName.Aquarius, ["Pi"] = ZodiacName.Pisces,
    };

    private static ZodiacName[] Seq(string s) => s.Split(", ").Select(a => Abbr[a]).ToArray();

    // Table 40 as printed: seed, then Normal, Saturn, Ketu.
    public static TheoryData<string, string, string, string> Table40 => new()
    {
        { "Ar", "Ar, Ta, Ge, Cn, Le, Vi, Li, Sc, Sg, Cp, Aq, Pi", "Ar, Ta, Ge, Cn, Le, Vi, Li, Sc, Sg, Cp, Aq, Pi", "Ar, Pi, Aq, Cp, Sg, Sc, Li, Vi, Le, Cn, Ge, Ta" },
        { "Ta", "Ta, Sg, Cn, Aq, Vi, Ar, Sc, Ge, Cp, Le, Pi, Li", "Ta, Ge, Cn, Le, Vi, Li, Sc, Sg, Cp, Aq, Pi, Ar", "Ta, Li, Pi, Le, Cp, Ge, Sc, Ar, Vi, Aq, Cn, Sg" },
        { "Ge", "Ge, Aq, Li, Vi, Ta, Cp, Sg, Le, Ar, Pi, Sc, Cn", "Ge, Cn, Le, Vi, Li, Sc, Sg, Cp, Aq, Pi, Ar, Ta", "Ge, Li, Aq, Pi, Cn, Sc, Sg, Ar, Le, Vi, Cp, Ta" },
        { "Cn", "Cn, Ge, Ta, Ar, Pi, Aq, Cp, Sg, Sc, Li, Vi, Le", "Cn, Le, Vi, Li, Sc, Sg, Cp, Aq, Pi, Ar, Ta, Ge", "Cn, Le, Vi, Li, Sc, Sg, Cp, Aq, Pi, Ar, Ta, Ge" },
        { "Le", "Le, Cp, Ge, Sc, Ar, Vi, Aq, Cn, Sg, Ta, Li, Pi", "Le, Vi, Li, Sc, Sg, Cp, Aq, Pi, Ar, Ta, Ge, Cn", "Le, Pi, Li, Ta, Sg, Cn, Aq, Vi, Ar, Sc, Ge, Cp" },
        { "Vi", "Vi, Cp, Ta, Ge, Li, Aq, Pi, Cn, Sc, Sg, Ar, Le", "Vi, Li, Sc, Sg, Cp, Aq, Pi, Ar, Ta, Ge, Cn, Le", "Vi, Ta, Cp, Sg, Le, Ar, Pi, Sc, Cn, Ge, Aq, Li" },
        { "Li", "Li, Sc, Sg, Cp, Aq, Pi, Ar, Ta, Ge, Cn, Le, Vi", "Li, Sc, Sg, Cp, Aq, Pi, Ar, Ta, Ge, Cn, Le, Vi", "Li, Vi, Le, Cn, Ge, Ta, Ar, Pi, Aq, Cp, Sg, Sc" },
        { "Sc", "Sc, Ge, Cp, Le, Pi, Li, Ta, Sg, Cn, Aq, Vi, Ar", "Sc, Sg, Cp, Aq, Pi, Ar, Ta, Ge, Cn, Le, Vi, Li", "Sc, Ar, Vi, Aq, Cn, Sg, Ta, Li, Pi, Le, Cp, Ge" },
        { "Sg", "Sg, Le, Ar, Pi, Sc, Cn, Ge, Aq, Li, Vi, Ta, Cp", "Sg, Cp, Aq, Pi, Ar, Ta, Ge, Cn, Le, Vi, Li, Sc", "Sg, Ar, Le, Vi, Cp, Ta, Ge, Li, Aq, Pi, Cn, Sc" },
        { "Cp", "Cp, Sg, Sc, Li, Vi, Le, Cn, Ge, Ta, Ar, Pi, Aq", "Cp, Aq, Pi, Ar, Ta, Ge, Cn, Le, Vi, Li, Sc, Sg", "Cp, Aq, Pi, Ar, Ta, Ge, Cn, Le, Vi, Li, Sc, Sg" },
        { "Aq", "Aq, Cn, Sg, Ta, Li, Pi, Le, Cp, Ge, Sc, Ar, Vi", "Aq, Pi, Ar, Ta, Ge, Cn, Le, Vi, Li, Sc, Sg, Cp", "Aq, Vi, Ar, Sc, Ge, Cp, Le, Pi, Li, Ta, Sg, Cn" },
        { "Pi", "Pi, Cn, Sc, Sg, Ar, Le, Vi, Cp, Ta, Ge, Li, Aq", "Pi, Ar, Ta, Ge, Cn, Le, Vi, Li, Sc, Sg, Cp, Aq", "Pi, Sc, Cn, Ge, Aq, Li, Vi, Ta, Cp, Sg, Le, Ar" },
    };

    [Theory]
    [MemberData(nameof(Table40))]
    public void Progression_matches_every_row_of_table_40(string seed, string normal, string saturn, string ketu)
    {
        var s = Abbr[seed];
        Assert.Equal(Seq(normal), NarayanaProgression.Progression(s, NarayanaVariant.Normal).Sequence);
        Assert.Equal(Seq(saturn), NarayanaProgression.Progression(s, NarayanaVariant.Saturn).Sequence);
        Assert.Equal(Seq(ketu), NarayanaProgression.Progression(s, NarayanaVariant.Ketu).Sequence);
    }

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
    public void Example_63_lagna_taurus_with_scorpio_stronger_takes_scorpio_as_seed()
    {
        // Scorpio holds the Moon and Mars (Taurus holds none), so the 7th is stronger: seed Sc, fixed, every 6th backward.
        var chart = Chart(ZodiacName.Taurus,
            (PlanetName.Moon, ZodiacName.Scorpio, 10), (PlanetName.Mars, ZodiacName.Scorpio, 12),
            (PlanetName.Sun, ZodiacName.Leo, 10), (PlanetName.Mercury, ZodiacName.Virgo, 10), (PlanetName.Jupiter, ZodiacName.Gemini, 10),
            (PlanetName.Venus, ZodiacName.Libra, 10), (PlanetName.Saturn, ZodiacName.Capricornus, 10),
            (PlanetName.Rahu, ZodiacName.Cancer, 10), (PlanetName.Ketu, ZodiacName.Capricornus, 20));
        var r = NarayanaProgression.Read(chart)!;
        Assert.Equal(ZodiacName.Scorpio, r.Seed);
        Assert.Contains("contains more planets", r.Comparison.Rule);
        Assert.False(r.Forward);
        Assert.Equal(Seq("Sc, Ge, Cp, Le, Pi, Li, Ta, Sg, Cn, Aq, Vi, Ar"), r.Sequence);
    }

    [Fact]
    public void Example_65_lagna_cancer_with_capricorn_stronger_is_regular_and_backward()
    {
        var chart = Chart(ZodiacName.Cancer,
            (PlanetName.Moon, ZodiacName.Capricornus, 10), (PlanetName.Mars, ZodiacName.Capricornus, 12),
            (PlanetName.Sun, ZodiacName.Leo, 10), (PlanetName.Mercury, ZodiacName.Virgo, 10), (PlanetName.Jupiter, ZodiacName.Gemini, 10),
            (PlanetName.Venus, ZodiacName.Libra, 10), (PlanetName.Saturn, ZodiacName.Aquarius, 10),
            (PlanetName.Rahu, ZodiacName.Taurus, 10), (PlanetName.Ketu, ZodiacName.Scorpio, 10));
        var r = NarayanaProgression.Read(chart)!;
        Assert.Equal(ZodiacName.Capricornus, r.Seed);
        Assert.Equal(NarayanaVariant.Normal, r.Variant);
        Assert.Equal(Seq("Cp, Sg, Sc, Li, Vi, Le, Cn, Ge, Ta, Ar, Pi, Aq"), r.Sequence);
    }

    [Fact]
    public void Saturn_in_the_seed_makes_the_progression_regular_and_forward()
    {
        // Seed Scorpio holding Saturn: the book's "Sc, Sg, Cp, Aq, Pi, Ar etc".
        var chart = Chart(ZodiacName.Taurus,
            (PlanetName.Saturn, ZodiacName.Scorpio, 10), (PlanetName.Mars, ZodiacName.Scorpio, 12), (PlanetName.Moon, ZodiacName.Scorpio, 14),
            (PlanetName.Sun, ZodiacName.Leo, 10), (PlanetName.Mercury, ZodiacName.Virgo, 10), (PlanetName.Jupiter, ZodiacName.Gemini, 10),
            (PlanetName.Venus, ZodiacName.Libra, 10), (PlanetName.Rahu, ZodiacName.Cancer, 10), (PlanetName.Ketu, ZodiacName.Capricornus, 10));
        var r = NarayanaProgression.Read(chart)!;
        Assert.Equal(NarayanaVariant.Saturn, r.Variant);
        Assert.True(r.Forward);
        Assert.Equal(Seq("Sc, Sg, Cp, Aq, Pi, Ar, Ta, Ge, Cn, Le, Vi, Li"), r.Sequence);
    }
}
