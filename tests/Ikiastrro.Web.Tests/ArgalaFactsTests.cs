using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Models;
using Ikiastrro.Data;

namespace Ikiastrro.Web.Tests;

/// <summary>ArgalaFacts.ForChart — the one source Key Inference's ArgalaTable and Life Matters
/// both read — plus the flat-list ArgalaCalculator.Compare the table's Net column now uses.</summary>
public sealed class ArgalaFactsTests
{
    private static readonly ChartKeyDetail[] Grahas =
    {
        new() { Planet = "Ascendant", Sign = "Aries", PointKind = "Graha" },
        new() { Planet = "Jupiter", Sign = "Taurus", PointKind = "Graha" },
        new() { Planet = "Saturn", Sign = "Pisces", PointKind = "Graha" },
    };

    [Fact]
    public void ForChart_PrefersStoredRowsForThatChart()
    {
        var stored = new[]
        {
            new ArgalaFactRow("D1", "House", "1", 1, "ARGALA", 2, true, "Venus", false, false, "SRC_PVR_INTEGRATED"),
            new ArgalaFactRow("D1", "Graha", "Sun", 5, "ARGALA", 2, true, "Moon", false, false, "SRC_PVR_INTEGRATED"),
            new ArgalaFactRow("D9", "House", "1", 1, "ARGALA", 2, true, "Mars", false, false, "SRC_PVR_INTEGRATED"),
        };

        var rows = ArgalaFacts.ForChart(stored, "D1", "Aries", Grahas);

        var row = Assert.Single(rows);   // D1 house rows only — not the Graha target, not D9
        Assert.Equal("Venus", row.OccupantPlanet);
    }

    [Fact]
    public void ForChart_ComputesLiveWhenNothingIsStoredForThatChart()
    {
        var stored = new[] { new ArgalaFactRow("D1", "House", "1", 1, "ARGALA", 2, true, "Venus", false, false, null) };

        var rows = ArgalaFacts.ForChart(stored, "D10", "Aries", Grahas);

        Assert.All(rows, r => Assert.Equal("D10", r.ChartType));
        Assert.Contains(rows, r => r is { TargetHouseNumber: 1, RelationTypeCode: "ARGALA", HouseOffset: 2, OccupantPlanet: "Jupiter" });
    }

    [Fact]
    public void Compare_OverFactRows_AgreesWithCompareOverTheLiveEvaluation()
    {
        // Chart 5 (PVR Exercise 16), the book-verified fixture ArgalaFactBuilderTests also uses.
        var occupancy = new Dictionary<ZodiacName, IReadOnlyList<PlanetName>>
        {
            [ZodiacName.Scorpio] = new[] { PlanetName.Mars, PlanetName.Saturn },
            [ZodiacName.Sagittarius] = new[] { PlanetName.Mercury },
            [ZodiacName.Capricornus] = new[] { PlanetName.Venus },
            [ZodiacName.Aquarius] = new[] { PlanetName.Ketu },
            [ZodiacName.Aries] = new[] { PlanetName.Moon },
            [ZodiacName.Taurus] = new[] { PlanetName.Sun },
            [ZodiacName.Leo] = new[] { PlanetName.Rahu },
            [ZodiacName.Virgo] = new[] { PlanetName.Jupiter },
        };
        static int Score(PlanetName p) => (int)p % 3 - 1;
        var facts = ArgalaFactBuilder.BuildForHouses(ZodiacName.Scorpio, occupancy);

        for (var house = 1; house <= 12; house++)
        {
            var expected = ArgalaCalculator.Compare(ArgalaCalculator.Evaluate(ZodiacName.Scorpio, house, occupancy), Score);
            var houseFacts = facts.Where(f => f.TargetHouseNumber == house).ToList();
            var actual = ArgalaCalculator.Compare(
                houseFacts.Where(f => f.RelationTypeCode == "ARGALA").Select(f => f.OccupantPlanet).ToList(),
                houseFacts.Where(f => f.RelationTypeCode == "VIRODHARGALA").Select(f => f.OccupantPlanet).ToList(),
                Score);
            Assert.Equal(expected, actual);
        }
    }
}
