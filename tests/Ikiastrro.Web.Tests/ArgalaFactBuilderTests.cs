using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Web.Tests;

/// <summary>Flattening tests for ArgalaFactBuilder (migration 128's tbl_Fact_Argala shape),
/// reusing Chart 5 (Exercise 16, see ArgalaCalculatorTests) since its per-house occupants are
/// already verified against the book.</summary>
public sealed class ArgalaFactBuilderTests
{
    private static readonly IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> Chart5 =
        new Dictionary<ZodiacName, IReadOnlyList<PlanetName>>
        {
            [ZodiacName.Scorpio] = new[] { PlanetName.Mars, PlanetName.Saturn },      // house 1
            [ZodiacName.Sagittarius] = new[] { PlanetName.Mercury },                  // house 2
            [ZodiacName.Capricornus] = new[] { PlanetName.Venus },                    // house 3
            [ZodiacName.Aquarius] = new[] { PlanetName.Ketu },                        // house 4
            [ZodiacName.Aries] = new[] { PlanetName.Moon },                           // house 6
            [ZodiacName.Taurus] = new[] { PlanetName.Sun },                           // house 7
            [ZodiacName.Leo] = new[] { PlanetName.Rahu },                             // house 10
            [ZodiacName.Virgo] = new[] { PlanetName.Jupiter },                        // house 11
        };

    [Fact]
    public void BuildForHouses_House1_Matches_The_Verified_Occupant_Set()
    {
        var facts = ArgalaFactBuilder.BuildForHouses(ZodiacName.Scorpio, Chart5)
            .Where(f => f.TargetKind == "House" && f.TargetHouseNumber == 1)
            .ToList();

        // House 1: argala = Mercury(2nd), Ketu(4th), Jupiter(11th); virodhargala = Rahu(10th), Venus(3rd)
        Assert.Equal(5, facts.Count);
        Assert.Contains(facts, f => f is { RelationTypeCode: "ARGALA", HouseOffset: 2, OccupantPlanet: PlanetName.Mercury });
        Assert.Contains(facts, f => f is { RelationTypeCode: "ARGALA", HouseOffset: 4, OccupantPlanet: PlanetName.Ketu });
        Assert.Contains(facts, f => f is { RelationTypeCode: "ARGALA", HouseOffset: 11, OccupantPlanet: PlanetName.Jupiter });
        Assert.Contains(facts, f => f is { RelationTypeCode: "VIRODHARGALA", HouseOffset: 10, OccupantPlanet: PlanetName.Rahu });
        Assert.Contains(facts, f => f is { RelationTypeCode: "VIRODHARGALA", HouseOffset: 3, OccupantPlanet: PlanetName.Venus });
        Assert.All(facts, f => Assert.Equal(ZodiacName.Scorpio, f.TargetSign));
        Assert.All(facts, f => Assert.False(f.ExceptionApplied));
    }

    [Fact]
    public void BuildForHouses_House11_Marks_The_ThirdHouse_Exception_Rows()
    {
        var facts = ArgalaFactBuilder.BuildForHouses(ZodiacName.Scorpio, Chart5)
            .Where(f => f.TargetKind == "House" && f.TargetHouseNumber == 11)
            .ToList();

        var exceptionRows = facts.Where(f => f.HouseOffset == 3).ToList();
        Assert.Equal(2, exceptionRows.Count); // Mars + Saturn
        Assert.All(exceptionRows, f => Assert.Equal("ARGALA", f.RelationTypeCode));
        Assert.All(exceptionRows, f => Assert.True(f.ExceptionApplied));
        Assert.Equal(new[] { PlanetName.Mars, PlanetName.Saturn },
            exceptionRows.Select(f => f.OccupantPlanet).OrderBy(p => p));
    }

    [Fact]
    public void BuildForPlanets_Jupiter_Matches_BuildForHouses_House11_Because_Jupiter_Occupies_It()
    {
        // Jupiter sits in Virgo, which is house 11 from Scorpio Lagna in Chart 5 — targeting by
        // planet or by the house it occupies must resolve to the exact same evaluation.
        var byHouse = ArgalaFactBuilder.BuildForHouses(ZodiacName.Scorpio, Chart5)
            .Where(f => f.TargetHouseNumber == 11).ToList();
        var byPlanet = ArgalaFactBuilder.BuildForPlanets(ZodiacName.Scorpio, Chart5)
            .Where(f => f.TargetKey == "Jupiter").ToList();

        Assert.Equal(byHouse.Count, byPlanet.Count);
        Assert.Equal(
            byHouse.Select(f => (f.RelationTypeCode, f.HouseOffset, f.OccupantPlanet)).OrderBy(x => x),
            byPlanet.Select(f => (f.RelationTypeCode, f.HouseOffset, f.OccupantPlanet)).OrderBy(x => x));
        Assert.All(byPlanet, f => Assert.Equal(11, f.TargetHouseNumber));
        Assert.All(byPlanet, f => Assert.Equal(ZodiacName.Virgo, f.TargetSign));
    }

    [Fact]
    public void BuildForPlanets_Covers_All_Nine_Grahas()
    {
        var targets = ArgalaFactBuilder.BuildForPlanets(ZodiacName.Scorpio, Chart5)
            .Select(f => f.TargetKey).Distinct().OrderBy(k => k).ToList();

        Assert.Equal(
            Enum.GetValues<PlanetName>().Select(p => p.ToString()).OrderBy(k => k),
            targets);
    }

    /// <summary>BuildOccupancy (added for the Key Inference 2.1 ArgalaTable, which only has
    /// ChartKeyDetail rows in hand, not an already-built occupancy map) must reproduce Chart 5's
    /// dictionary exactly — Ascendant excluded, everything else grouped by sign.</summary>
    [Fact]
    public void BuildOccupancy_Reproduces_Chart5_From_KeyDetail_Rows()
    {
        var keyDetails = new List<ChartKeyDetail>
        {
            new() { Planet = "Ascendant", Sign = "Scorpio" },
            new() { Planet = "Mars", Sign = "Scorpio" },
            new() { Planet = "Saturn", Sign = "Scorpio" },
            new() { Planet = "Mercury", Sign = "Sagittarius" },
            new() { Planet = "Venus", Sign = "Capricornus" },
            new() { Planet = "Ketu", Sign = "Aquarius" },
            new() { Planet = "Moon", Sign = "Aries" },
            new() { Planet = "Sun", Sign = "Taurus" },
            new() { Planet = "Rahu", Sign = "Leo" },
            new() { Planet = "Jupiter", Sign = "Virgo" },
        };

        var occupancy = ArgalaFactBuilder.BuildOccupancy(keyDetails);

        // Ascendant excluded — Scorpio holds only Mars + Saturn, not a 3rd "Ascendant" entry.
        Assert.Equal(2, occupancy[ZodiacName.Scorpio].Count);
        Assert.Equal(Chart5.Count, occupancy.Count);
        foreach (var (sign, planets) in Chart5)
            Assert.Equal(planets.OrderBy(p => p), occupancy[sign].OrderBy(p => p));
    }
}
