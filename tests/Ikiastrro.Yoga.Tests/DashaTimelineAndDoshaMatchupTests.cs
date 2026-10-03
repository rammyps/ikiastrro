using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

public class DashaTimelineAndDoshaMatchupTests
{
    private static readonly DateTime AsOf = new(2026, 1, 1);
    private static readonly DateTime Horizon = new(2036, 1, 1);

    private static DoshaChart Chart(ZodiacName lagna = ZodiacName.Aries) =>
        new(lagna, Enum.GetValues<PlanetName>().ToDictionary(p => p, _ => ZodiacName.Leo));

    private static DateTime D(int y, int m = 1, int d = 1) => new(y, m, d);

    // Mahadasha: Venus to 2030, then Sun to 2036+. Antardasha: Venus to 2028, Moon to 2030, Sun to 2031.
    private static DashaPerson First() => new("A", "Venus",
        [new(1, "Venus", D(2020), D(2030)), new(2, "Venus", D(2025), D(2028))],
        [new(1, "Sun", D(2030), D(2040)), new(2, "Moon", D(2028), D(2030)), new(2, "Sun", D(2030), D(2031))],
        Chart(), null);

    // Mahadasha: Moon to 2029, then Sun. Antardasha: Venus to 2028 (also Venus for A), Sun to 2029.
    private static DashaPerson Second() => new("B", "Moon",
        [new(1, "Moon", D(2019), D(2029)), new(2, "Venus", D(2025), D(2028))],
        [new(1, "Sun", D(2029), D(2036)), new(2, "Sun", D(2028), D(2029))],
        Chart(), null);

    [Fact]
    public void Changes_are_every_maha_and_antar_start_after_the_as_of_date_in_date_order()
    {
        var r = PairDashaTimeline.Build(First(), Second(), AsOf, Horizon);
        (DateTime, string, int, string, string)[] expected =
        [
            (D(2028), "A", 2, "Venus", "Moon"), (D(2028), "B", 2, "Venus", "Sun"), (D(2029), "B", 1, "Moon", "Sun"),
            (D(2030), "A", 1, "Venus", "Sun"), (D(2030), "A", 2, "Moon", "Sun"),
        ];
        Assert.Equal(expected, r.Changes.Select(c => (c.Date, c.Person, c.Level, c.FromLord, c.ToLord)).ToArray());
    }

    [Fact]
    public void A_change_names_the_person_it_affects_and_what_the_new_lord_rules_in_their_chart()
    {
        var r = PairDashaTimeline.Build(First(), Second(), AsOf, Horizon);
        var aSun = r.Changes.Single(c => c.IsFirst && c.Level == 1);
        Assert.Equal("A", aSun.Person);
        Assert.Equal("Venus", aSun.FromLord);
        Assert.Equal("Sun", aSun.ToLord);
        Assert.Equal([5], aSun.Rules);            // Aries Lagna: Sun rules Leo, the 5th
        Assert.Equal(5, aSun.PlacedIn);           // every graha is placed in Leo here
    }

    [Fact]
    public void Changes_outside_the_window_are_left_out()
    {
        var r = PairDashaTimeline.Build(First(), Second(), AsOf, D(2028, 6, 1));
        Assert.DoesNotContain(r.Changes, c => c.Date > D(2028, 6, 1));
        Assert.DoesNotContain(r.Changes, c => c.Date <= AsOf);
    }

    [Fact]
    public void Shared_windows_are_where_both_are_in_the_same_lord_at_the_same_level()
    {
        var r = PairDashaTimeline.Build(First(), Second(), AsOf, Horizon);
        // Antardasha: both are in Venus 2026-01-01 to 2028-01-01 (clipped to the as-of date).
        Assert.Contains(new SharedDashaWindow(2, "Venus", AsOf, D(2028)), r.SharedWindows);
        // Mahadasha: A is Sun from 2030, B is Sun from 2029: shared 2030 to the horizon.
        Assert.Contains(new SharedDashaWindow(1, "Sun", D(2030), Horizon), r.SharedWindows);
        // A's Antardasha Sun 2030-2031 overlaps B's Mahadasha, not B's Antardasha (which is Sun only 2028-2029).
        Assert.DoesNotContain(r.SharedWindows, w => w.Level == 2 && w.Lord == "Sun");
    }

    private static DoshaReading Reading(params (PlanetName Planet, DoshaReference From, int House, DoshaWeight Weight)[] a) =>
        new(a.Select(x => new DoshaAffliction(x.Planet, x.From, x.House, x.Weight, "note")).ToList(),
            new Dictionary<DoshaReference, int>());

    [Fact]
    public void Matchup_sorts_afflictions_by_kind_and_says_whether_both_charts_have_it()
    {
        var first = Reading((PlanetName.Mars, DoshaReference.Lagna, 8, DoshaWeight.Severe), (PlanetName.Rahu, DoshaReference.Moon, 4, DoshaWeight.Notable));
        var second = Reading((PlanetName.Mars, DoshaReference.Venus, 7, DoshaWeight.Severe));
        var rows = DoshaMatchup.Build(first, second).ToDictionary(r => r.Dosha);

        var mars = rows["Mars in the 7th or 8th"];
        Assert.Equal(["8th from Lagna"], mars.First);
        Assert.Equal(["7th from Venus"], mars.Second);
        Assert.True(mars.SameInBoth);

        var rahu4 = rows["Rahu in the 4th"];
        Assert.Equal(["4th from Moon"], rahu4.First);
        Assert.Empty(rahu4.Second);
        Assert.False(rahu4.SameInBoth);

        Assert.True(rows["Rahu in the 7th or 8th"].SameInBoth);   // neither has it
    }
}
