using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR's own interpretation rules for the rasi dasas (sec.18.4, 19.4, 20.3, 21.3), tested on hand-built charts and on
/// the Sudasa example the book gives for HL (p.265).</summary>
public sealed class PhalitaDasaReadingTests
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

    private static readonly DasaReferencePoints NoRefs = new(null, null, null, null, null, null);

    private static PvrPoint Point(PhalitaReading r, string startsWith) => r.Points.Single(p => p.Statement.StartsWith(startsWith));

    // Chart 23 (PVR Example 66): Aquarius Lagna, dasas start from the 7th (Leo).
    private static ChartAnalysisInput Chart23() => Chart(ZodiacName.Aquarius,
        (PlanetName.Sun, ZodiacName.Cancer, 10), (PlanetName.Moon, ZodiacName.Taurus, 1), (PlanetName.Saturn, ZodiacName.Taurus, 10),
        (PlanetName.Mercury, ZodiacName.Leo, 10), (PlanetName.Mars, ZodiacName.Leo, 12), (PlanetName.Venus, ZodiacName.Leo, 14),
        (PlanetName.Jupiter, ZodiacName.Scorpio, 10), (PlanetName.Rahu, ZodiacName.Gemini, 10), (PlanetName.Ketu, ZodiacName.Sagittarius, 10));

    [Fact]
    public void Narayana_dasa_lagna_is_the_7th_from_the_dasa_rasi_when_dasas_start_from_the_7th()
    {
        // Capricorn dasa in Chart 23: the dasas start from the 7th, so the dasa lagna is Cancer (the 7th from Capricorn);
        // its lord, the Moon, is exalted in Taurus (the paaka rasi).
        var r = PhalitaDasaReader.ReadNarayana(Chart23(), ZodiacName.Capricornus, dasasStartFromLagna: false, NoRefs)!;
        Assert.Contains("dasa lagna Cancer", r.Context[0]);
        Assert.Contains("Moon, in Taurus (the paaka rasi)", r.Context[1]);

        Assert.True(Point(r, "Natural malefics in the 3rd and 6th").Holds);          // Ketu in Sagittarius, the 6th from Cancer
        Assert.Contains("Ketu in the 6th", Point(r, "Natural malefics in the 3rd and 6th").Detail);
        Assert.False(Point(r, "Natural benefics in the 3rd and 6th").Holds);
        Assert.True(Point(r, "A planet, benefic or malefic, in the 11th").Holds);     // Moon and Saturn in Taurus
        Assert.True(Point(r, "Rahu in the 8th or 12th").Holds);                       // Rahu in Gemini, the 12th from Cancer
        var strong = Point(r, "The lord of the dasa lagna, or of a trine or quadrant from it, exalted");
        Assert.True(strong.Holds);
        Assert.Contains("Moon (1st) exalted", strong.Detail);
        Assert.Equal("excellent results", strong.Result);
    }

    [Fact]
    public void Narayana_natal_references_mark_the_dasa_rasi()
    {
        var refs = new DasaReferencePoints(null, GhatiLagna: ZodiacName.Capricornus, null, Upapada: ZodiacName.Aries, RajyaPada: ZodiacName.Capricornus, null);
        var r = PhalitaDasaReader.ReadNarayana(Chart23(), ZodiacName.Capricornus, false, refs, antardasaRasi: ZodiacName.Scorpio)!;
        Assert.True(Point(r, "The dasa rasi contains the raajya pada").Holds);
        Assert.True(Point(r, "The dasa rasi contains Ghati Lagna").Holds);
        Assert.False(Point(r, "The dasa rasi is the upapada").Holds);
        Assert.True(Point(r, "The antardasa rasi aspects the upapada").Holds);        // Scorpio (fixed) aspects Aries; only the sign before it, Libra, is skipped
    }

    [Fact]
    public void Sudasa_hora_lagna_example_from_the_book_holds_three_ways()
    {
        // PVR p.265: HL in Aries, Mars in Leo, Sun in Scorpio. Leo dasa: (a) Leo aspects HL, (b) the lord of Leo aspects HL,
        // (c) the lord of HL occupies Leo.
        var chart = Chart(ZodiacName.Taurus,
            (PlanetName.Sun, ZodiacName.Scorpio, 10), (PlanetName.Mars, ZodiacName.Leo, 10), (PlanetName.Moon, ZodiacName.Pisces, 10),
            (PlanetName.Mercury, ZodiacName.Virgo, 10), (PlanetName.Jupiter, ZodiacName.Gemini, 10), (PlanetName.Venus, ZodiacName.Libra, 10),
            (PlanetName.Saturn, ZodiacName.Capricornus, 10), (PlanetName.Rahu, ZodiacName.Cancer, 10), (PlanetName.Ketu, ZodiacName.Capricornus, 20));
        var refs = new DasaReferencePoints(HoraLagna: ZodiacName.Aries, null, null, null, null, null);
        var r = PhalitaDasaReader.ReadSudasa(chart, ZodiacName.Leo, refs)!;
        Assert.True(Point(r, "The dasa rasi aspects Hora Lagna (HL)").Holds);
        Assert.True(Point(r, "The lord of the dasa rasi occupies or aspects Hora Lagna (HL)").Holds);
        Assert.True(Point(r, "The lord of Hora Lagna (HL) occupies or aspects the dasa rasi").Holds);
        Assert.All(r.Points.Where(p => p.Holds), p => Assert.StartsWith("financial prosperity", p.Result));
    }

    [Fact]
    public void Sudasa_arudha_lagna_upachayas_and_setbacks()
    {
        // AL in Leo: Libra is the 3rd (an upachaya), Gemini the 11th (particularly favourable), Pisces the 8th (a setback).
        var refs = new DasaReferencePoints(null, null, ZodiacName.Leo, null, null, null);
        var third = PhalitaDasaReader.ReadSudasa(Chart23(), ZodiacName.Libra, refs)!;
        Assert.True(Point(third, "The dasa rasi is an upachaya").Holds);
        Assert.False(Point(third, "The dasa rasi is the 11th").Holds);

        var eleventh = PhalitaDasaReader.ReadSudasa(Chart23(), ZodiacName.Gemini, refs)!;
        Assert.True(Point(eleventh, "The dasa rasi is the 11th").Holds);
        Assert.True(Point(eleventh, "The dasa rasi is an upachaya").Holds);

        var eighth = PhalitaDasaReader.ReadSudasa(Chart23(), ZodiacName.Pisces, refs)!;
        Assert.True(Point(eighth, "The dasa rasi is the 8th or 12th").Holds);
        Assert.False(Point(eighth, "The dasa rasi is an upachaya").Holds);
    }

    [Fact]
    public void Drigdasa_lagna_and_seventh_bring_internal_awakening()
    {
        var chart = Chart23();
        var lagna = PhalitaDasaReader.ReadDrig(chart, ZodiacName.Aquarius, NoRefs)!;
        var seventh = PhalitaDasaReader.ReadDrig(chart, ZodiacName.Leo, NoRefs)!;
        var other = PhalitaDasaReader.ReadDrig(chart, ZodiacName.Aries, NoRefs)!;
        Assert.True(Point(lagna, "The dasa rasi is the Lagna or the 7th house").Holds);
        Assert.True(Point(seventh, "The dasa rasi is the Lagna or the 7th house").Holds);
        Assert.False(Point(other, "The dasa rasi is the Lagna or the 7th house").Holds);
    }

    [Fact]
    public void Kendradi_dasa_rasi_with_the_atma_karaka_or_gl()
    {
        var refs = new DasaReferencePoints(null, GhatiLagna: ZodiacName.Leo, null, null, null, AtmaKaraka: PlanetName.Mars);
        var r = PhalitaDasaReader.ReadLagnaKendradi(Chart23(), ZodiacName.Leo, refs)!;
        Assert.True(Point(r, "The dasa rasi contains the Atma Karaka").Holds);   // Mars in Leo
        Assert.True(Point(r, "The dasa rasi contains Ghati Lagna").Holds);
        Assert.False(Point(r, "The dasa rasi contains the Lagna lord").Holds);   // Saturn (Aquarius lord) is in Taurus
    }
}
