using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Panchanga;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>
/// ShadbalaCalculator's Dina/Hora/Tribhaga Bala (Kaala Bala sub-components, migration
/// 072, SRC_RAMAN_GRAHA_BHAVA_BALAS via SRC_PVR_INTEGRATED) and Graha Yuddha detection
/// (tbl_Rule_PlanetaryWar). Dina/Hora reuse PanchangaCalculator's own verified weekday lord
/// (Tuesday/Mars) and Hora Lord (Venus) for 1_Ramakrishnan
/// (docs/artifacts/reference-charts/Rammy_Jagannatha.txt) -- same fixture as
/// PanchangaCalculatorTests, so a regression there is caught here too.
/// </summary>
public class ShadbalaKalaBalaTests
{
    // JHora D1 longitudes for 1_Ramakrishnan (sign*30 + degree-in-sign).
    private static readonly IReadOnlyDictionary<PlanetName, (ZodiacName Sign, double Degree, int House)> D1 =
        new Dictionary<PlanetName, (ZodiacName, double, int)>
        {
            [PlanetName.Sun] = (ZodiacName.Aries, 8 + 12 / 60.0 + 18.13 / 3600.0, 1),
            [PlanetName.Moon] = (ZodiacName.Scorpio, 7 + 17 / 60.0 + 33.70 / 3600.0, 8),
            [PlanetName.Mars] = (ZodiacName.Aries, 3 + 57 / 60.0 + 0.54 / 3600.0, 1),
            [PlanetName.Mercury] = (ZodiacName.Aries, 1 + 50 / 60.0 + 33.50 / 3600.0, 1),
            [PlanetName.Jupiter] = (ZodiacName.Virgo, 8 + 43 / 60.0 + 35.30 / 3600.0, 6),
            [PlanetName.Venus] = (ZodiacName.Aries, 11 + 59 / 60.0 + 32.24 / 3600.0, 1),
            [PlanetName.Saturn] = (ZodiacName.Virgo, 10 + 57 / 60.0 + 23.35 / 3600.0, 6),
        };

    private static readonly DateTimeOffset Sunrise = new(1981, 4, 21, 5, 56, 39, TimeSpan.FromHours(5.5));
    private static readonly DateTimeOffset Sunset = new(1981, 4, 21, 18, 18, 53, TimeSpan.FromHours(5.5));
    private static SunTimes Times() => new(Sunrise, Sunset, Sunrise.AddDays(1), IsNightBirth: true);

    private static BirthDetails Ramakrishnan() => new()
    {
        Name = "Ramakrishnan",
        DateOfBirth = new DateOnly(1981, 4, 22),
        TimeOfBirth = new TimeOnly(5, 30, 0),
        PlaceCity = "Chennai",
        PlaceCountry = "India",
        UtcOffset = "05:30:00",
    };

    private static ChartAnalysisInput D1Chart() => new("D1", ZodiacName.Aries,
        D1.Select(kv => new PlanetPosition
        {
            Planet = kv.Key.ToString(),
            Sign = kv.Value.Sign.ToString(),
            HouseNumber = kv.Value.House,
            NirayanaLongitudeDegrees = (int)kv.Value.Sign * 30 + kv.Value.Degree,
            IsRetrograde = false,
        }).ToList());

    private static SiderealPositions Positions() => new(
        AscendantLongitude: 0,
        PlanetLongitudes: D1.ToDictionary(kv => kv.Key, kv => (int)kv.Value.Sign * 30 + kv.Value.Degree),
        PlanetLatitudes: new Dictionary<PlanetName, double>(),
        PlanetSpeeds: new Dictionary<PlanetName, double>(),
        AyanamshaDegrees: 0, LocalSiderealTimeHours: 0);

    private static IReadOnlyList<PlanetaryStrengthResult> Calculate()
    {
        var positions = Positions();
        var sun = Times();
        var panchanga = PanchangaCalculator.Calculate(Ramakrishnan(), positions, sun);
        return ShadbalaCalculator.Calculate(new[] { D1Chart() }, positions, sun, panchanga);
    }

    private static double Component(PlanetaryStrengthResult r, string subComponentCode) =>
        r.Components.Single(c => c.SubComponentCode == subComponentCode).ValueVirupas;

    [Fact]
    public void Dina_bala_goes_only_to_the_weekday_lord()
    {
        // JHora: Tuesday (Mars) -- verify-panchanga / PanchangaCalculatorTests.
        var results = Calculate();

        Assert.Equal(45, Component(results.Single(r => r.Planet == "Mars"), "DINA_BALA"));
        foreach (var other in results.Where(r => r.Planet != "Mars"))
            Assert.Equal(0, Component(other, "DINA_BALA"));
    }

    [Fact]
    public void Hora_bala_goes_only_to_the_running_hora_lord()
    {
        // JHora: Hora Lord Venus -- verify-panchanga / PanchangaCalculatorTests.
        var results = Calculate();

        Assert.Equal(60, Component(results.Single(r => r.Planet == "Venus"), "HORA_BALA"));
        foreach (var other in results.Where(r => r.Planet != "Venus"))
            Assert.Equal(0, Component(other, "HORA_BALA"));
    }

    [Fact]
    public void Tribhaga_bala_goes_to_the_night_third_lord_and_always_to_jupiter()
    {
        // Birth is 23h33m21s after sunrise (Janma Ghatis 58.8892), well past sunset (12h22m14s
        // day length) into an 11h37m46s night: intoNight ~671.1min / (nightLength/3 ~232.6min)
        // -> the 3rd night-third (index 2), ruled by Mars. Jupiter is classically exempt and
        // always scores the full 60 regardless of which third the birth fell in.
        var results = Calculate();

        Assert.Equal(60, Component(results.Single(r => r.Planet == "Mars"), "TRIBHAGA_BALA"));
        Assert.Equal(60, Component(results.Single(r => r.Planet == "Jupiter"), "TRIBHAGA_BALA"));
        foreach (var other in results.Where(r => r.Planet is not ("Mars" or "Jupiter")))
            Assert.Equal(0, Component(other, "TRIBHAGA_BALA"));
    }

    [Fact]
    public void No_war_is_detected_for_ramakrishnan()
    {
        // Mars/Mercury/Venus are all >2 degrees apart in Aries; Jupiter/Saturn are 2.23 degrees
        // apart in Virgo -- no pair of the five tara grahas is within the 1-degree war orb.
        var results = Calculate();

        Assert.All(results, r => Assert.DoesNotContain(r.Components, c => c.BalaCode == "YUDDHA_BALA"));
        Assert.All(results, r => Assert.Equal(0, r.YuddhaBalaVirupas));
    }

    [Fact]
    public void War_winner_is_the_more_northern_planet_with_magnitude_deliberately_deferred()
    {
        // Synthetic: Mars and Saturn conjunct within orb (0.3 degrees), Mars more northern.
        var chart = new ChartAnalysisInput("D1", ZodiacName.Aries, new List<PlanetPosition>
        {
            new() { Planet = "Sun", Sign = "Aries", HouseNumber = 1, NirayanaLongitudeDegrees = 10, IsRetrograde = false },
            new() { Planet = "Moon", Sign = "Taurus", HouseNumber = 2, NirayanaLongitudeDegrees = 40, IsRetrograde = false },
            new() { Planet = "Mars", Sign = "Cancer", HouseNumber = 4, NirayanaLongitudeDegrees = 100.0, IsRetrograde = false },
            new() { Planet = "Mercury", Sign = "Leo", HouseNumber = 5, NirayanaLongitudeDegrees = 130, IsRetrograde = false },
            new() { Planet = "Jupiter", Sign = "Virgo", HouseNumber = 6, NirayanaLongitudeDegrees = 160, IsRetrograde = false },
            new() { Planet = "Venus", Sign = "Libra", HouseNumber = 7, NirayanaLongitudeDegrees = 190, IsRetrograde = false },
            new() { Planet = "Saturn", Sign = "Cancer", HouseNumber = 4, NirayanaLongitudeDegrees = 100.3, IsRetrograde = false },
        });
        var positions = new SiderealPositions(0,
            new Dictionary<PlanetName, double> { [PlanetName.Sun] = 10, [PlanetName.Moon] = 40 },
            new Dictionary<PlanetName, double> { [PlanetName.Mars] = 1.2, [PlanetName.Saturn] = -0.5 },
            new Dictionary<PlanetName, double>(), 0, 0);
        var sun = Times();
        var panchanga = PanchangaCalculator.Calculate(Ramakrishnan(), positions, sun);

        var results = ShadbalaCalculator.Calculate(new[] { chart }, positions, sun, panchanga);

        var mars = results.Single(r => r.Planet == "Mars");
        var saturn = results.Single(r => r.Planet == "Saturn");
        Assert.Contains(mars.Components, c => c.BalaCode == "YUDDHA_BALA" && c.Narrative.Contains("wins"));
        Assert.Contains(saturn.Components, c => c.BalaCode == "YUDDHA_BALA" && c.Narrative.Contains("loses"));
        // Magnitude is deliberately 0 -- the diameter-based delta needs the cited Raman edition,
        // which has no text extract available (see ShadbalaCalculator.ComputeYuddha).
        Assert.Equal(0, mars.YuddhaBalaVirupas);
        Assert.Equal(0, saturn.YuddhaBalaVirupas);
    }
}
