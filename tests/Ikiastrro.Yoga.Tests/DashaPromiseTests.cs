using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>Pins the dasha-promise arithmetic (PVR 16.5) on a hand-built chart: lordship and occupancy of the
/// area's houses counted from the varga Lagna and from the tripod reference, and the Kendradi caution.</summary>
public class DashaPromiseTests
{
    // Aries Lagna. Sun Taurus, Moon Gemini, Mars Leo, Mercury Virgo, Jupiter Sagittarius, Venus Scorpio, Saturn Aquarius, Rahu Libra, Ketu Aries.
    private static DoshaChart Chart() => new(ZodiacName.Aries, new Dictionary<PlanetName, ZodiacName>
    {
        [PlanetName.Sun] = ZodiacName.Taurus, [PlanetName.Moon] = ZodiacName.Gemini, [PlanetName.Mars] = ZodiacName.Leo,
        [PlanetName.Mercury] = ZodiacName.Virgo, [PlanetName.Jupiter] = ZodiacName.Sagittarius, [PlanetName.Venus] = ZodiacName.Scorpio,
        [PlanetName.Saturn] = ZodiacName.Aquarius, [PlanetName.Rahu] = ZodiacName.Libra, [PlanetName.Ketu] = ZodiacName.Aries,
    });

    private static DashaAreaReading Get(IReadOnlyList<DashaAreaReading> r, string area, DashaLevel level) =>
        r.Single(x => x.Area.Code == area && x.Level == level);

    private static IReadOnlyList<DashaAreaReading> Read(RunningDasha running) =>
        DashaPromiseReader.Read(new Dictionary<string, DoshaChart> { ["D1"] = Chart(), ["D9"] = Chart() }, running);

    [Fact]
    public void Mahadasha_is_read_from_the_sun_as_well_as_from_the_lagna()
    {
        // Marriage reads D9's 7th. Venus owns Libra, the 7th from Aries, and is the karaka; it sits in Scorpio,
        // the 7th from Taurus (the Sun).
        var m = Get(Read(new(PlanetName.Venus, PlanetName.Mars, PlanetName.Moon)), "marriage", DashaLevel.Maha);
        Assert.Equal("Sun", m.Reference);
        Assert.Contains("rules 7th", m.LinksFromLagna);
        Assert.Contains("karaka", m.LinksFromLagna);
        Assert.Contains("in 7th from Sun", m.LinksFromReference);
        Assert.Equal(7, m.HouseFromReference);
        Assert.Equal([1, 6], m.RulesFromReference);   // from the Sun in Taurus: Taurus is the 1st and Libra the 6th
    }

    [Fact]
    public void Antardasha_is_read_from_the_moon_and_pratyantar_from_the_lagna_only()
    {
        var r = Read(new(PlanetName.Venus, PlanetName.Mars, PlanetName.Moon));
        Assert.Equal("Moon", Get(r, "marriage", DashaLevel.Antar).Reference);
        var p = Get(r, "marriage", DashaLevel.Pratyantar);
        Assert.Equal("Lagna", p.Reference);
        Assert.Empty(p.LinksFromReference);   // the reference is the Lagna: it would repeat LinksFromLagna
    }

    [Fact]
    public void A_node_owns_no_sign_so_it_links_only_by_occupying()
    {
        // Rahu sits in Libra, the 7th from Aries.
        var m = Get(Read(new(PlanetName.Rahu, PlanetName.Rahu, PlanetName.Rahu)), "marriage", DashaLevel.Maha);
        Assert.Empty(m.RulesFromReference);
        Assert.Equal(["in 7th"], m.LinksFromLagna);
    }

    [Fact]
    public void Areas_without_a_house_or_without_their_varga_are_left_out()
    {
        var r = Read(new(PlanetName.Venus, PlanetName.Mars, PlanetName.Moon));
        Assert.DoesNotContain(r, x => x.Area.Code == "moon");     // no house to read
        Assert.DoesNotContain(r, x => x.Area.Code == "children"); // D7 not supplied
        Assert.Contains(r, x => x.Area.Code == "marriage");
    }

    [Fact]
    public void Kendradi_check_counts_planets_and_occupied_quadrants_from_lagna_and_moon()
    {
        // Aries Lagna: the quadrants are Aries, Cancer, Libra, Capricornus. None of Sun to Saturn sits in them
        // (Ketu in Aries and Rahu in Libra are nodes and are not counted).
        var (fromLagna, fromMoon) = KendradiGrahaDasaCheck.Evaluate(Chart());
        Assert.Equal(0, fromLagna.PlanetsInQuadrants);
        Assert.False(fromLagna.AllQuadrantsOccupied);
        // From the Moon in Gemini the quadrants are Gemini, Virgo, Sagittarius, Pisces: Moon, Mercury, Jupiter.
        Assert.Equal(3, fromMoon.PlanetsInQuadrants);
        Assert.Equal(3, fromMoon.QuadrantsOccupied);
        Assert.False(fromMoon.AllQuadrantsOccupied);
    }

    [Fact]
    public void Kendradi_check_flags_all_four_quadrants_occupied()
    {
        var chart = new DoshaChart(ZodiacName.Aries, new Dictionary<PlanetName, ZodiacName>
        {
            [PlanetName.Sun] = ZodiacName.Aries, [PlanetName.Moon] = ZodiacName.Cancer, [PlanetName.Mars] = ZodiacName.Libra,
            [PlanetName.Saturn] = ZodiacName.Capricornus, [PlanetName.Mercury] = ZodiacName.Leo, [PlanetName.Jupiter] = ZodiacName.Leo,
            [PlanetName.Venus] = ZodiacName.Leo, [PlanetName.Rahu] = ZodiacName.Leo, [PlanetName.Ketu] = ZodiacName.Leo,
        });
        var (fromLagna, _) = KendradiGrahaDasaCheck.Evaluate(chart);
        Assert.Equal(4, fromLagna.PlanetsInQuadrants);
        Assert.True(fromLagna.AllQuadrantsOccupied);
    }
}
