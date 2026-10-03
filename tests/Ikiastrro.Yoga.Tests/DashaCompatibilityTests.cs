using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>DashaCompatibility: natural attitude between dasha lords, a running lord's lordships and house,
/// and the Mahadasha junction notes. Facts only; no verdicts are asserted because none are given.</summary>
public class DashaCompatibilityTests
{
    private static readonly DateTime AsOf = new(2026, 10, 3);

    private static DoshaChart LibraChart(ZodiacName venus = ZodiacName.Libra, ZodiacName rahu = ZodiacName.Gemini) =>
        new(ZodiacName.Libra, new Dictionary<PlanetName, ZodiacName>
        {
            [PlanetName.Sun] = ZodiacName.Leo, [PlanetName.Moon] = ZodiacName.Cancer, [PlanetName.Mars] = ZodiacName.Aries,
            [PlanetName.Mercury] = ZodiacName.Gemini, [PlanetName.Jupiter] = ZodiacName.Gemini, [PlanetName.Venus] = venus,
            [PlanetName.Saturn] = ZodiacName.Leo, [PlanetName.Rahu] = rahu, [PlanetName.Ketu] = ZodiacName.Sagittarius,
        });

    private static DashaPerson Person(string name, string birth, string mahaLord, DateTime mahaStart, DateTime mahaEnd,
        DoshaChart? chart = null, PlanetName? dk = null) =>
        new(name, birth, [new DashaSpan(1, mahaLord, mahaStart, mahaEnd)], [], chart ?? LibraChart(), dk);

    [Fact]
    public void Attitude_is_directional_and_undefined_for_the_nodes()
    {
        Assert.Equal("Enemy", DashaCompatibility.Attitude("Saturn", "Moon").Attitude);
        Assert.Equal("Neutral", DashaCompatibility.Attitude("Moon", "Saturn").Attitude);
        Assert.Null(DashaCompatibility.Attitude("Rahu", "Moon").Attitude);
    }

    [Fact]
    public void Venus_for_a_Libra_lagna_rules_the_1st_and_8th_and_sits_in_the_1st()
    {
        var person = Person("A", "Saturn", "Venus", new DateTime(2018, 10, 14), new DateTime(2038, 10, 13), dk: PlanetName.Venus);
        var facts = DashaCompatibility.Facts(person.Running[0], person);

        Assert.Equal([1, 8], facts.Rules);
        Assert.Equal(1, facts.PlacedIn);
        Assert.True(facts.IsDarakaraka);
        Assert.True(facts.IsVenusOrJupiter);
    }

    [Fact]
    public void A_node_rules_no_house_but_has_a_placement()
    {
        var person = Person("A", "Saturn", "Rahu", new DateTime(2020, 1, 1), new DateTime(2038, 1, 1));
        var facts = DashaCompatibility.Facts(person.Running[0], person);

        Assert.Empty(facts.Rules);
        Assert.Equal(9, facts.PlacedIn);
    }

    [Fact]
    public void Compare_reads_both_regards_and_flags_a_junction_within_a_year()
    {
        var first = Person("A", "Saturn", "Venus", new DateTime(2018, 10, 14), new DateTime(2027, 3, 1));
        var second = Person("B", "Moon", "Saturn", new DateTime(2026, 6, 6), new DateTime(2045, 6, 5));

        var reading = DashaCompatibility.Compare(first, second, AsOf, AsOf.AddYears(25));

        Assert.Equal("Enemy", reading.BirthLordFirstToSecond.Attitude);
        Assert.Equal("Neutral", reading.BirthLordSecondToFirst.Attitude);
        Assert.Equal("Friend", reading.MahaFirstToSecond!.Attitude);
        Assert.Equal(2, reading.SandhiNotes.Count);
        Assert.Contains(reading.SandhiNotes, n => n.Contains("A's Venus Mahadasha ends"));
        Assert.Contains(reading.SandhiNotes, n => n.Contains("B's Saturn Mahadasha began"));
    }
}
