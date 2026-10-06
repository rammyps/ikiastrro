using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.KeyInfo;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR sec.14.3 "Rudra, Trishoola and Maheswara", using the book's own Exercise 23 answer (p.186)
/// and the illustrations given for each Maheswara exception.</summary>
public sealed class RudraMaheswaraTests
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
    public void Table_32_matches_the_book_row_and_its_worked_examples()
    {
        ZodiacName[] expected =
        [
            ZodiacName.Scorpio, ZodiacName.Gemini, ZodiacName.Capricornus, ZodiacName.Sagittarius, ZodiacName.Cancer, ZodiacName.Aquarius,
            ZodiacName.Taurus, ZodiacName.Sagittarius, ZodiacName.Cancer, ZodiacName.Gemini, ZodiacName.Capricornus, ZodiacName.Leo,
        ];
        for (var i = 0; i < 12; i++) Assert.Equal(expected[i], RudraMaheswara.RudraEighth((ZodiacName)i));
        // Example 84: "the 8th lords from Le and Aq" are the Moon (Cancer) and Saturn (Capricornus).
        Assert.Equal(ZodiacName.Cancer, RudraMaheswara.RudraEighth(ZodiacName.Leo));
        Assert.Equal(ZodiacName.Capricornus, RudraMaheswara.RudraEighth(ZodiacName.Aquarius));
    }

    // Exercise 23: Scorpio Lagna, Taurus 7th. Jupiter, Venus and Mercury join in Libra, Mercury further on;
    // Rahu is in the 7th (Taurus). Mercury is the Atma Karaka.
    private static ChartAnalysisInput Exercise23() => Chart(ZodiacName.Scorpio,
        (PlanetName.Jupiter, ZodiacName.Libra, 10), (PlanetName.Venus, ZodiacName.Libra, 5), (PlanetName.Mercury, ZodiacName.Libra, 20),
        (PlanetName.Rahu, ZodiacName.Taurus, 10), (PlanetName.Ketu, ZodiacName.Scorpio, 10),
        (PlanetName.Sun, ZodiacName.Leo, 10), (PlanetName.Moon, ZodiacName.Leo, 12), (PlanetName.Mars, ZodiacName.Leo, 14),
        (PlanetName.Saturn, ZodiacName.Leo, 16));

    [Fact]
    public void Exercise_23_rudra_is_mercury_in_libra_and_the_trishoola_spikes_are_in_gemini_libra_aquarius()
    {
        var r = RudraMaheswara.ReadRudra(Exercise23())!;
        Assert.Equal(ZodiacName.Sagittarius, r.EighthFromLagna);
        Assert.Equal(PlanetName.Jupiter, r.LordFromLagna);
        Assert.Equal(ZodiacName.Taurus, r.SeventhSign);
        Assert.Equal(ZodiacName.Gemini, r.EighthFromSeventh);
        Assert.Equal(PlanetName.Mercury, r.LordFromSeventh);
        Assert.Equal(PlanetName.Mercury, r.Rudra.Planet);   // "stronger, as he is more advanced in his rasi"
        Assert.Contains("further advanced", r.Rudra.Why);
        Assert.Equal([ZodiacName.Libra, ZodiacName.Aquarius, ZodiacName.Gemini], r.Trishoola);
        // The book's answer names Mercury only. By the afflicted-weaker rule Jupiter (an enemy in Libra, rasi-aspected
        // by Rahu in Taurus) also qualifies, which the book's "can also become Rudra" allows; it is not asserted here.
    }

    [Fact]
    public void Exercise_23_maheswara_is_jupiter_by_the_node_exception()
    {
        // The 8th from Mercury (Libra) is Taurus, which holds Rahu, so the 6th from Libra (Pisces) is read: Jupiter.
        var m = RudraMaheswara.ReadMaheswara(Exercise23(), PlanetName.Mercury)!;
        Assert.Equal(6, m.HouseCounted);
        Assert.Equal(ZodiacName.Pisces, m.HouseSign);
        Assert.Equal(PlanetName.Jupiter, m.Maheswara.Planet);
        Assert.NotNull(m.Exception);
    }

    [Fact]
    public void Maheswara_exception_2_ketu_in_the_ak_sign_or_its_8th_reads_the_6th()
    {
        // "AK is Mars and he is in Taurus ... Ketu is in Ta or Sg": the 6th from Taurus is Libra, Venus is Maheswara.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Taurus, 10), (PlanetName.Ketu, ZodiacName.Sagittarius, 10), (PlanetName.Rahu, ZodiacName.Gemini, 10),
            (PlanetName.Sun, ZodiacName.Leo, 10), (PlanetName.Moon, ZodiacName.Leo, 12), (PlanetName.Mercury, ZodiacName.Leo, 14),
            (PlanetName.Jupiter, ZodiacName.Leo, 16), (PlanetName.Venus, ZodiacName.Leo, 18), (PlanetName.Saturn, ZodiacName.Leo, 20));
        var m = RudraMaheswara.ReadMaheswara(chart, PlanetName.Mars)!;
        Assert.Equal(6, m.HouseCounted);
        Assert.Equal(PlanetName.Venus, m.Maheswara.Planet);
    }

    [Fact]
    public void Maheswara_without_exception_is_the_lord_of_the_8th_from_the_ak()
    {
        // "AK is Mars and he is in Taurus": the 8th is Sagittarius and Jupiter is Maheswara, with no node in Taurus or
        // Sagittarius and Jupiter neither in its own nor its exaltation sign.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Taurus, 10), (PlanetName.Jupiter, ZodiacName.Virgo, 10),
            (PlanetName.Rahu, ZodiacName.Cancer, 10), (PlanetName.Ketu, ZodiacName.Capricornus, 10),
            (PlanetName.Sun, ZodiacName.Leo, 10), (PlanetName.Moon, ZodiacName.Leo, 12), (PlanetName.Mercury, ZodiacName.Leo, 14),
            (PlanetName.Venus, ZodiacName.Leo, 18), (PlanetName.Saturn, ZodiacName.Leo, 20));
        var m = RudraMaheswara.ReadMaheswara(chart, PlanetName.Mars)!;
        Assert.Equal(8, m.HouseCounted);
        Assert.Equal(ZodiacName.Sagittarius, m.HouseSign);
        Assert.Equal(PlanetName.Jupiter, m.Maheswara.Planet);
        Assert.Null(m.Exception);
    }

    [Fact]
    public void Maheswara_exception_1_an_8th_lord_in_exaltation_hands_over_to_the_stronger_of_its_8th_and_12th_lords()
    {
        // "AK is Mars and he is in Ge. The 8th is Cp, Saturn is Maheswara. Saturn is exalted in Li. From Li, Venus owns the 8th (Ta)
        // and Mercury the 12th (Vi). The stronger of Mercury and Venus becomes Maheswara." Mercury here joins the Sun, Venus is alone.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Gemini, 10), (PlanetName.Saturn, ZodiacName.Libra, 10),
            (PlanetName.Venus, ZodiacName.Taurus, 10), (PlanetName.Mercury, ZodiacName.Virgo, 10), (PlanetName.Sun, ZodiacName.Virgo, 12),
            (PlanetName.Moon, ZodiacName.Leo, 12), (PlanetName.Jupiter, ZodiacName.Aquarius, 10),
            (PlanetName.Rahu, ZodiacName.Aquarius, 10), (PlanetName.Ketu, ZodiacName.Leo, 10));
        var m = RudraMaheswara.ReadMaheswara(chart, PlanetName.Mars)!;
        Assert.Equal(8, m.HouseCounted);
        Assert.Equal(PlanetName.Mercury, m.Maheswara.Planet);
        Assert.Contains("exaltation", m.Exception);
    }

    [Fact]
    public void An_afflicted_debilitated_weaker_lord_is_also_rudra()
    {
        // Aries Lagna: 8th by Table 32 is Scorpio (Mars); 7th is Libra, whose Table-32 8th is Taurus (Venus).
        // Mars is in Leo with three planets (strong); Venus is alone in debilitating Virgo, conjoined by nobody but aspected by
        // Saturn's... Put Saturn in Virgo with Venus: Venus then has one companion, Mars three, so Mars is stronger; Venus is
        // debilitated and joined by Saturn, so it counts as well.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Mars, ZodiacName.Leo, 10), (PlanetName.Sun, ZodiacName.Leo, 12), (PlanetName.Moon, ZodiacName.Leo, 14),
            (PlanetName.Jupiter, ZodiacName.Leo, 16), (PlanetName.Venus, ZodiacName.Virgo, 10), (PlanetName.Saturn, ZodiacName.Virgo, 12),
            (PlanetName.Mercury, ZodiacName.Cancer, 10), (PlanetName.Rahu, ZodiacName.Gemini, 10), (PlanetName.Ketu, ZodiacName.Sagittarius, 10));
        var r = RudraMaheswara.ReadRudra(chart)!;
        Assert.Equal(PlanetName.Mars, r.Rudra.Planet);
        Assert.Equal(PlanetName.Venus, r.AlsoRudra?.Planet);
        Assert.Equal([ZodiacName.Virgo, ZodiacName.Capricornus, ZodiacName.Taurus], r.AlsoTrishoola);
    }

    [Fact]
    public void A_node_that_is_itself_the_atma_karaka_does_not_trigger_the_6th_house_exception()
    {
        // Rahu is the AK in Cancer; Ketu is in Capricorn, not in Cancer or its 8th (Aquarius), so the plain 8th is read.
        var chart = Chart(ZodiacName.Aries,
            (PlanetName.Rahu, ZodiacName.Cancer, 12), (PlanetName.Ketu, ZodiacName.Capricornus, 12),
            (PlanetName.Sun, ZodiacName.Leo, 10), (PlanetName.Moon, ZodiacName.Leo, 12), (PlanetName.Mars, ZodiacName.Leo, 14),
            (PlanetName.Mercury, ZodiacName.Leo, 16), (PlanetName.Jupiter, ZodiacName.Virgo, 10),
            (PlanetName.Venus, ZodiacName.Virgo, 12), (PlanetName.Saturn, ZodiacName.Virgo, 14));
        var m = RudraMaheswara.ReadMaheswara(chart, PlanetName.Rahu)!;
        Assert.Equal(8, m.HouseCounted);
        Assert.Equal(ZodiacName.Aquarius, m.HouseSign);
    }
}
