using Ikiastrro.Core.Engines.Strength;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

public class PlainSummariesTests
{
    [Fact]
    public void Dignity_counts_comfortable_and_uncomfortable_planets_and_flags_combustion()
    {
        var facts = new[]
        {
            new DignityFact("Mars", "Moolatrikona", 1, true, false),
            new DignityFact("Sun", "Exalted", 1, false, false),
            new DignityFact("Moon", "Debilitated", 8, false, false),
            new DignityFact("Mercury", "Enemy", 1, true, false),
            new DignityFact("Jupiter", "Neutral", 6, false, true),
            new DignityFact("Rahu", null, 4, false, true),
        };
        var text = string.Join("\n", DignitySummary.Build(facts));

        Assert.Contains("2 are at home or exalted, 2 are uncomfortable, and 1 are in friendly or neutral", text);
        Assert.Contains("Debilitated: Moon (house 8)", text);
        Assert.Contains("Combust (too close to the Sun, results dimmed): Mars, Mercury", text);
        Assert.Contains("Retrograde (energy turned inward): Jupiter", text);
        Assert.DoesNotContain("Rahu", string.Join("\n", DignitySummary.Build(facts).Where(l => l.StartsWith("Retrograde"))));
    }

    [Fact]
    public void Planet_strength_names_strongest_weakest_and_the_lagna_lord()
    {
        var facts = new[]
        {
            new PlanetStrengthFact("Mars", 7.5m, 5m, 195m),
            new PlanetStrengthFact("Mercury", 6.8m, 9.5m, 72m),
            new PlanetStrengthFact("Venus", 6.8m, 5.5m, 130m),
        };
        var text = string.Join("\n", PlanetStrengthSummary.Build(facts, "Mars"));

        Assert.Contains("2 of 3 planets reach the strength they need", text);
        Assert.Contains("Strongest: Mars at 195%", text);
        Assert.Contains("Weakest: Mercury at 72% — below the moderate band", text);
        Assert.Contains("Your Lagna lord, Mars, stands at 195% (strong)", text);
    }

    [Fact]
    public void House_strength_ties_strong_and_weak_houses_to_their_life_areas()
    {
        var facts = new[]
        {
            new HouseStrengthFact(10, 8.2m, "Saturn"),
            new HouseStrengthFact(7, 6m, "Venus"),
            new HouseStrengthFact(12, 4.1m, "Jupiter"),
        };
        var text = string.Join("\n", HouseStrengthSummary.Build(facts));

        Assert.Contains("Strongest house: house 10 (career and status), lord Saturn, 8.2 rūpas", text);
        Assert.Contains("Weak houses (under 5 rūpas): house 12 (losses, foreign matters and liberation)", text);
    }

    [Fact]
    public void Yogas_are_counted_once_and_split_by_nature_and_strength()
    {
        var facts = new[]
        {
            new YogaFact("Hamsa", "AUSPICIOUS", "Lagna based", true, new[] { "Jupiter" }, 150m),
            new YogaFact("Hamsa", "AUSPICIOUS", "Lagna based", true, new[] { "Jupiter" }, 150m),
            new YogaFact("Adhi", "AUSPICIOUS", "Moon based", true, new[] { "Moon" }, 60m),
            new YogaFact("Kemadruma", "INAUSPICIOUS", "Moon based", true, new[] { "Moon" }, 60m),
            new YogaFact("Sasa", "AUSPICIOUS", "Lagna based", false, Array.Empty<string>(), null),
        };
        var text = string.Join("\n", YogaSummary.Build(facts));

        Assert.Contains("3 yogas are present: 2 good, 1 challenging", text);
        Assert.Contains("2 Moon based, 1 Lagna based", text);
        Assert.Contains("all their planets at full strength (most likely to deliver): Hamsa", text);
        Assert.Contains("resting on a weak planet (promise is there, delivery is faint): Adhi", text);
        Assert.Contains("Challenging yogas present: Kemadruma", text);
    }
}
