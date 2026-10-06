using Bunit;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>The House lord placement table shows Scorpio's and Aquarius's stronger co-lord and the rule that decided it
/// (PVR sec.15.5.1) when it is given them, and nothing extra otherwise.</summary>
public sealed class HouseLordshipCoLordTests : BunitContext
{
    private static ChartHouseLord House(int number, string sign, string lord) => new()
    {
        HouseNumber = number, HouseSign = sign, LordPlanet = lord, LordPlacedInSign = "Leo",
        LordPlacedInHouseFromLagna = 5, LordPlacedInHouseFromMoon = 5,
    };

    [Fact]
    public void ScorpioAndAquariusShowTheStrongerCoLordAndWhy()
    {
        var rows = new[] { House(1, "Aries", "Mars"), House(8, "Scorpio", "Mars"), House(11, "Aquarius", "Saturn") };
        var coLords = new Dictionary<ZodiacName, CoLordReading>
        {
            [ZodiacName.Scorpio] = new(ZodiacName.Scorpio, PlanetName.Mars, PlanetName.Ketu, PlanetName.Ketu, "Mars is in Scorpio, so Ketu rules it"),
            [ZodiacName.Aquarius] = new(ZodiacName.Aquarius, PlanetName.Saturn, PlanetName.Rahu, PlanetName.Saturn, "joined by more planets (2 against 1)"),
        };
        var cut = Render<HouseLordshipTable>(p => p.Add(x => x.Rows, rows).Add(x => x.CoLords, coLords));

        var notes = cut.FindAll(".hlt-colord");
        Assert.Equal(2, notes.Count);   // only the Scorpio and Aquarius houses
        Assert.Contains("Stronger co-lord: Ketu", notes[0].Normalized());
        Assert.Contains("Mars is in Scorpio, so Ketu rules it", notes[0].Normalized());
        Assert.Contains("Stronger co-lord: Saturn", notes[1].Normalized());
    }

    [Fact]
    public void NoCoLordNoteWithoutTheParameter()
    {
        var cut = Render<HouseLordshipTable>(p => p.Add(x => x.Rows, [House(8, "Scorpio", "Mars")]));
        Assert.Empty(cut.FindAll(".hlt-colord"));
    }
}
