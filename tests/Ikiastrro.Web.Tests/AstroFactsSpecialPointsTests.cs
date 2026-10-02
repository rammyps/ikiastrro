using Bunit;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class AstroFactsSpecialPointsTests : BunitContext
{
    [Fact]
    public void GrahaYuddhaPairsWithinOneDegreeAndNorthernLatitudeWins()
    {
        var longitudes = new Dictionary<PlanetName, double>
        {
            [PlanetName.Mars] = 359.6, [PlanetName.Venus] = 0.3,   // 0.7° apart across 0° Aries
            [PlanetName.Jupiter] = 160.0, [PlanetName.Saturn] = 162.0, // 2° apart — no war
            [PlanetName.Sun] = 0.0,                                  // never a participant
        };
        var latitudes = new Dictionary<PlanetName, double> { [PlanetName.Mars] = -1.2, [PlanetName.Venus] = 2.3 };

        var war = Assert.Single(GrahaYuddha.Find(longitudes, latitudes));

        Assert.Equal(PlanetName.Venus, war.Winner);
        Assert.Equal(PlanetName.Mars, war.Loser);
        Assert.Equal(0.7, war.OrbDegrees, 6);
    }

    [Fact]
    public void MotionTableShowsCombustionAndWarOutcome()
    {
        var rows = new[]
        {
            Graha("Sun", 10.0, 0.0, null, null, null),
            Graha("Mars", 100.2, -1.0, false, 90.2m, 17m),
            Graha("Venus", 100.5, 2.0, true, 90.5m, 10m),
            Graha("Rahu", 200.0, 0.0, null, null, null),
        };

        var cut = Render<PlanetMotionTable>(p => p.Add(x => x.KeyDetails, rows));

        Assert.Contains("Combust", cut.Markup);
        Assert.Contains("Wins vs Mars (0.30°)", cut.Markup);
        Assert.Contains("Loses to Venus (0.30°)", cut.Markup);
        Assert.Contains("90.50°", cut.Markup);
    }

    [Fact]
    public void SpecialPointsTableLabelsUpagrahasAndArudhas()
    {
        var upagrahas = new[] { Point("Upagraha", "Maandi", "Leo", 143.156, 3) };
        var arudhas = new[] { Point("Arudha", "A12", "Pisces", 349.24, 10), Point("GrahaArudha", "GA_Sun", "Aries", 19.24, 11) };

        var up = Render<SpecialPointsTable>(p => p.Add(x => x.Rows, upagrahas));
        var ar = Render<SpecialPointsTable>(p => p.Add(x => x.Rows, arudhas).Add(x => x.ShowDegree, false));

        Assert.Contains("Māndi", up.Markup);
        Assert.Contains("Purva Phalguni", up.Markup);
        Assert.Contains("H3", up.Markup);
        Assert.Contains("Upapada (UL)", ar.Markup);
        Assert.Contains("Sun's ārūḍha", ar.Markup);
        Assert.DoesNotContain("Nakshatra", ar.Markup);
    }

    private static ChartKeyDetail Graha(string planet, double lon, double lat, bool? combust, decimal? fromSun, decimal? orb) => new()
    {
        Planet = planet, PointKind = "Graha", NirayanaLongitudeDegrees = lon, EclipticLatitudeDegrees = lat,
        IsCombust = combust, DistanceFromSunDegrees = fromSun, CombustionOrbUsedDegrees = orb, SpeedLongitudeDegPerDay = 1.0,
    };

    private static ChartKeyDetail Point(string kind, string name, string sign, double lon, int house) => new()
    {
        Planet = name, PointKind = kind, Sign = sign, NirayanaLongitudeDegrees = lon, HouseNumberFromLagna = house,
    };
}
