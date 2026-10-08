using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

public class TrioDashaTimelineTests
{
    private static readonly DateTime AsOf = new(2026, 1, 1);
    private static readonly DateTime Horizon = new(2036, 1, 1);
    private static DateTime D(int y) => new(y, 1, 1);

    private static DoshaChart Chart() =>
        new(ZodiacName.Aries, Enum.GetValues<PlanetName>().ToDictionary(p => p, _ => ZodiacName.Leo));

    private static DashaPerson Person(string name, params DashaSpan[] spans) =>
        new(name, "Venus", spans.Take(1).ToList(), spans.Skip(1).ToList(), Chart(), null);

    [Fact]
    public void Window_is_where_all_three_run_the_same_lord_at_the_same_level()
    {
        var father = Person("F", new DashaSpan(1, "Venus", D(2020), D(2030)), new DashaSpan(1, "Sun", D(2030), D(2040)));
        var mother = Person("M", new DashaSpan(1, "Venus", D(2024), D(2029)), new DashaSpan(1, "Sun", D(2029), D(2040)));
        var child  = Person("C", new DashaSpan(1, "Venus", D(2027), D(2033)), new DashaSpan(1, "Sun", D(2033), D(2050)));

        var w = TrioDashaTimeline.SharedByAll([father, mother, child], AsOf, Horizon);

        var venus = Assert.Single(w, x => x.Lord == "Venus");
        Assert.Equal((D(2027), D(2029)), (venus.Start, venus.End));
        var sun = Assert.Single(w, x => x.Lord == "Sun");
        Assert.Equal((D(2033), D(2036)), (sun.Start, sun.End));   // clamped to the horizon
    }

    [Fact]
    public void No_window_when_one_person_never_runs_the_lord_together_with_the_others()
    {
        var father = Person("F", new DashaSpan(1, "Venus", D(2020), D(2030)));
        var mother = Person("M", new DashaSpan(1, "Venus", D(2020), D(2030)));
        var child  = Person("C", new DashaSpan(1, "Moon", D(2020), D(2030)));

        Assert.Empty(TrioDashaTimeline.SharedByAll([father, mother, child], AsOf, Horizon));
    }

    [Fact]
    public void Levels_are_kept_apart()
    {
        var father = Person("F", new DashaSpan(1, "Venus", D(2020), D(2030)));
        var mother = Person("M", new DashaSpan(2, "Venus", D(2020), D(2030)));

        Assert.Empty(TrioDashaTimeline.SharedByAll([father, mother], AsOf, Horizon));
    }
}
