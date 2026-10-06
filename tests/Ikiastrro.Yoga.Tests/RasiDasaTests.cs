using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR ch.18-21: Narayana, Lagna Kendradi, Sudasa and Drigdasa, against the book's worked examples (66, 67, 76,
/// 77, 79, 80).</summary>
public sealed class RasiDasaTests
{
    private static readonly Dictionary<string, ZodiacName> Abbr = new()
    {
        ["Ar"] = ZodiacName.Aries, ["Ta"] = ZodiacName.Taurus, ["Ge"] = ZodiacName.Gemini, ["Cn"] = ZodiacName.Cancer,
        ["Le"] = ZodiacName.Leo, ["Vi"] = ZodiacName.Virgo, ["Li"] = ZodiacName.Libra, ["Sc"] = ZodiacName.Scorpio,
        ["Sg"] = ZodiacName.Sagittarius, ["Cp"] = ZodiacName.Capricornus, ["Aq"] = ZodiacName.Aquarius, ["Pi"] = ZodiacName.Pisces,
    };

    private static ZodiacName[] Seq(string s) => s.Split(", ").Select(a => Abbr[a]).ToArray();

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

    // Chart 23 of the book (Example 66): Aquarius Lagna, Leo (3 planets) stronger than Aquarius. Sun Cn, Moon Ta (exalted),
    // Saturn Ta, Mercury Mars and Venus Le, Jupiter Sc. The nodes are placed opposite each other where they win nothing.
    private static ChartAnalysisInput Chart23() => Chart(ZodiacName.Aquarius,
        (PlanetName.Sun, ZodiacName.Cancer, 10), (PlanetName.Moon, ZodiacName.Taurus, 1), (PlanetName.Saturn, ZodiacName.Taurus, 10),
        (PlanetName.Mercury, ZodiacName.Leo, 10), (PlanetName.Mars, ZodiacName.Leo, 12), (PlanetName.Venus, ZodiacName.Leo, 14),
        (PlanetName.Jupiter, ZodiacName.Scorpio, 10), (PlanetName.Rahu, ZodiacName.Gemini, 10), (PlanetName.Ketu, ZodiacName.Sagittarius, 10));

    [Fact]
    public void Example_66_narayana_lengths_dates_and_second_cycle()
    {
        var birth = new DateTime(1912, 8, 15);
        var plan = RasiDasa.Compute(RasiDasaSystem.Narayana, Chart23(), birth)!;
        Assert.Equal(Seq("Le, Cp, Ge, Sc, Ar, Vi, Aq, Cn, Sg, Ta, Li, Pi"), plan.Sequence);

        var first = plan.Periods.Where(p => p.Cycle == 1).ToList();
        Assert.Equal([1, 8, 2, 9, 4, 1, 9, 3, 11, 3, 10, 4], first.Select(p => (int)p.Years));
        Assert.Equal(new DateTime(1913, 8, 15), first[0].End);        // Le: Aug 1912 - Aug 1913
        Assert.Equal(new DateTime(1921, 8, 15), first[1].End);        // Cp: to Aug 1921
        Assert.Equal(new DateTime(1977, 8, 15), first[^1].End);       // first cycle ends Aug 1977
        Assert.Contains("+1 (exalted)", first[7].LengthWhy);          // Cn: 3 including the exalted Moon

        var second = plan.Periods.Where(p => p.Cycle == 2).ToList();
        Assert.Equal([11, 4, 10], second.Take(3).Select(p => (int)p.Years));   // 12 - 1, 12 - 8, 12 - 2
        Assert.Equal(new DateTime(1988, 8, 15), second[0].End);       // Le: Aug 1977 - Aug 1988
        Assert.Equal(new DateTime(1992, 8, 15), second[1].End);
    }

    private static ChartAnalysisInput Example67Chart(ZodiacName ketuSign, ZodiacName rahuSign) => Chart(ZodiacName.Aries,
        (PlanetName.Saturn, ZodiacName.Leo, 10), (PlanetName.Moon, ZodiacName.Taurus, 10), (PlanetName.Jupiter, ZodiacName.Cancer, 10),
        (PlanetName.Sun, ZodiacName.Virgo, 10), (PlanetName.Mars, ZodiacName.Libra, 10), (PlanetName.Mercury, ZodiacName.Virgo, 12),
        (PlanetName.Venus, ZodiacName.Libra, 12), (PlanetName.Rahu, rahuSign, 10), (PlanetName.Ketu, ketuSign, 10));

    private static RasiDasaPeriod CapricornDasa() =>
        new(1, ZodiacName.Capricornus, 5, new DateTime(2000, 1, 1), new DateTime(2005, 1, 1), PlanetName.Saturn, 6, "");

    [Fact]
    public void Example_67_antardasas_start_from_the_lord_of_the_stronger_of_dasa_rasi_and_its_7th()
    {
        // Cp dasa of 5 years: Saturn in Le (6th backward from Cp), Moon in Ta. Jupiter in Cn makes Cn stronger than Cp.
        var a = RasiDasa.Antardasas(Example67Chart(ZodiacName.Libra, ZodiacName.Aries), CapricornDasa())!;
        Assert.Equal(ZodiacName.Cancer, a.Seed);
        Assert.Equal(ZodiacName.Taurus, a.StartRasi);
        Assert.False(a.Forward);   // Taurus is an even sign
        Assert.Equal(Seq("Ta, Ar, Pi, Aq, Cp, Sg, Sc, Li, Vi, Le, Cn, Ge"), a.Periods.Select(p => p.Rasi));
        Assert.Equal(new DateTime(2005, 1, 1), a.Periods[^1].End);
        Assert.All(a.Periods.Take(11), p => Assert.InRange((p.End - p.Start).TotalDays, 150, 155));   // 5 months each
    }

    [Fact]
    public void Example_67_ketu_in_the_antardasa_seed_reverses_the_direction()
    {
        // The same chart with Ketu in Cn (the seed): Ta, Ge, Cn, Le, Vi, Li, Sc, Sg, Cp, Aq, Pi, Ar.
        var a = RasiDasa.Antardasas(Example67Chart(ZodiacName.Cancer, ZodiacName.Capricornus), CapricornDasa())!;
        Assert.True(a.Forward);
        Assert.Equal(Seq("Ta, Ge, Cn, Le, Vi, Li, Sc, Sg, Cp, Aq, Pi, Ar"), a.Periods.Select(p => p.Rasi));
    }

    [Fact]
    public void Example_76_lagna_kendradi_from_taurus_goes_backward()
    {
        // Lagna Scorpio; Taurus is stronger (Jupiter aspects it; none sit in either). Taurus is the seed, an even sign.
        var chart = Chart(ZodiacName.Scorpio,
            (PlanetName.Jupiter, ZodiacName.Virgo, 10), (PlanetName.Sun, ZodiacName.Aquarius, 10), (PlanetName.Moon, ZodiacName.Aquarius, 12),
            (PlanetName.Mars, ZodiacName.Sagittarius, 10), (PlanetName.Mercury, ZodiacName.Capricornus, 10),
            (PlanetName.Venus, ZodiacName.Capricornus, 12), (PlanetName.Saturn, ZodiacName.Libra, 10),
            (PlanetName.Rahu, ZodiacName.Gemini, 10), (PlanetName.Ketu, ZodiacName.Sagittarius, 20));
        var plan = RasiDasa.Compute(RasiDasaSystem.LagnaKendradi, chart, new DateTime(1911, 2, 6))!;
        Assert.Equal(ZodiacName.Taurus, plan.Seed);
        Assert.False(plan.Forward);
        Assert.Equal(Seq("Ta, Aq, Sc, Le, Ar, Cp, Li, Cn, Pi, Sg, Vi, Ge"), plan.Sequence);   // Table 41's order
    }

    [Fact]
    public void Example_77_sudasa_from_capricorn_sree_lagna_goes_backward_and_leaves_part_of_the_first_dasa()
    {
        // SL at 12 deg 21 min Capricorn: dasas Cp, Li, Cn, Ar | Sg, Vi, Ge, Pi | Sc, Le, Ta, Aq; 0.5883 of the first is left.
        var chart = Chart23();
        var sl = 9 * 30 + 12 + 21.0 / 60;
        var plan = RasiDasa.Compute(RasiDasaSystem.Sudasa, chart, new DateTime(1950, 1, 1), sl)!;
        Assert.Equal(ZodiacName.Capricornus, plan.Seed);
        Assert.False(plan.Forward);
        Assert.Equal(Seq("Cp, Li, Cn, Ar, Sg, Vi, Ge, Pi, Sc, Le, Ta, Aq"), plan.Sequence);
        var capricornYears = RasiDasa.Compute(RasiDasaSystem.Narayana, chart, DateTime.Today)!.Periods.First(p => p.Rasi == ZodiacName.Capricornus).Years;
        Assert.Equal(capricornYears * (30 - (12 + 21.0 / 60)) / 30, plan.Periods[0].Years, 6);   // the book rounds the fraction to 0.5883
        Assert.Contains("0.5883", plan.SeedWhy);
    }

    [Fact]
    public void Example_79_sudasa_from_sagittarius_goes_forward_with_twelve_years_for_a_lord_in_its_own_sign()
    {
        // SL 22 deg 22 min Sagittarius, Jupiter in Sagittarius: first dasa 12 years, 12 x (30 - 22.37)/30 left at birth.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Jupiter, ZodiacName.Sagittarius, 5), (PlanetName.Sun, ZodiacName.Leo, 10), (PlanetName.Moon, ZodiacName.Leo, 12),
            (PlanetName.Mars, ZodiacName.Cancer, 10), (PlanetName.Mercury, ZodiacName.Leo, 14), (PlanetName.Venus, ZodiacName.Leo, 16),
            (PlanetName.Saturn, ZodiacName.Libra, 10), (PlanetName.Rahu, ZodiacName.Gemini, 10), (PlanetName.Ketu, ZodiacName.Sagittarius, 20));
        var sl = 8 * 30 + 22 + 22.0 / 60;
        var plan = RasiDasa.Compute(RasiDasaSystem.Sudasa, chart, new DateTime(1960, 1, 1), sl)!;
        Assert.Equal(Seq("Sg, Pi, Ge, Vi, Cp, Ar, Cn, Li, Aq, Ta, Le, Sc"), plan.Sequence);
        Assert.True(plan.Forward);
        Assert.Equal(12 * (30 - (22 + 22.0 / 60)) / 30, plan.Periods[0].Years, 3);   // about 3 years 19 days
    }

    [Fact]
    public void Example_80_drigdasa_for_libra_lagna()
    {
        var chart = Chart(ZodiacName.Libra,
            (PlanetName.Sun, ZodiacName.Aries, 10), (PlanetName.Moon, ZodiacName.Aries, 12), (PlanetName.Mars, ZodiacName.Leo, 10),
            (PlanetName.Mercury, ZodiacName.Taurus, 10), (PlanetName.Jupiter, ZodiacName.Cancer, 10), (PlanetName.Venus, ZodiacName.Pisces, 10),
            (PlanetName.Saturn, ZodiacName.Virgo, 10), (PlanetName.Rahu, ZodiacName.Gemini, 10), (PlanetName.Ketu, ZodiacName.Sagittarius, 10));
        var plan = RasiDasa.Compute(RasiDasaSystem.Drig, chart, new DateTime(1970, 1, 1))!;
        Assert.Equal(ZodiacName.Gemini, plan.Seed);
        Assert.Equal(Seq("Ge, Vi, Sg, Pi, Cn, Ta, Aq, Sc, Le, Ar, Cp, Li"), plan.Sequence);
    }
}
