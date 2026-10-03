using SwissEphemeris.Interpreter;
using Xunit;

namespace SwissEphemeris.Interpreter.Tests;

/// <summary>
/// Moshier-accuracy spot-checks against ikiastrro's established reference chart
/// (22 Apr 1981, 05:30:00 +05:30, Chennai — <c>BENCH_RAMAKRISHNAN_P_JHORA_1981</c>,
/// <c>docs/artifacts/reference-charts/Rammy_Jagannatha.txt</c> in the ikiastrro repo).
/// Expected longitudes are ikiastrro's own already-verified output (traditional Lahiri,
/// Swiss sidereal mode 1) captured from <c>tbl_Chart_KeyDetails</c> before this extraction —
/// this suite exists to prove the extraction didn't change a single digit, not to
/// re-derive astronomical truth independently.
/// </summary>
public class GetPositionsTests
{
    private static readonly DateTimeOffset ReferenceMoment =
        new(1981, 4, 22, 5, 30, 0, TimeSpan.FromHours(5.5));
    private const double ChennaiLatitude = 13.083694;
    private const double ChennaiLongitude = 80.270186;

    private static EphemerisSnapshot GetReferenceSnapshot() =>
        SwissEphemerisInterpreter.GetPositions(
            ReferenceMoment, ChennaiLatitude, ChennaiLongitude, AyanamsaDefinition.TraditionalLahiri);

    public static IEnumerable<object[]> ExpectedLongitudes =>
        new List<object[]>
        {
            new object[] { "Sun", 8.2009071927320161 },
            new object[] { "Moon", 217.20777319080989 },
            new object[] { "Mars", 3.9423471793158806 },
            new object[] { "Mercury", 1.8316128024392635 },
            new object[] { "Jupiter", 158.72289503163395 },
            new object[] { "Venus", 11.982232867907282 },
            new object[] { "Saturn", 160.95855158855355 },
            // Planets on the solar-system plane as true positions since decision 009; Rahu/Ketu
            // the true node since decision 008 (12°55'08" Cancer; JHora 12°54'53").
            new object[] { "Rahu", 102.91896206108494 },
            new object[] { "Ketu", 282.91896206108493 },
        };

    [Theory]
    [MemberData(nameof(ExpectedLongitudes))]
    public void Planet_longitude_matches_ikiastrros_verified_reference(string body, double expectedLongitudeDeg)
    {
        var snapshot = GetReferenceSnapshot();
        var planet = Assert.Single(snapshot.Planets, p => p.Body == body);
        Assert.Equal(expectedLongitudeDeg, planet.LongitudeDeg, precision: 6);
    }

    // JHora's own export for this chart (05:30:01, 80E17 13N05): sidereal longitude and the
    // latitude column of "Latitudes, speeds etc". The latitudes are measured from the
    // solar-system plane, which pins the projection (decision 009).
    public static IEnumerable<object[]> JhoraPositions =>
        new List<object[]>
        {
            new object[] { "Sun", 8.20185556, 1.530 },
            new object[] { "Moon", 217.20875833, 3.410 },
            new object[] { "Mars", 3.94327778, 1.098 },
            new object[] { "Mercury", 1.83256389, 0.349 },
            new object[] { "Jupiter", 158.72377222, 0.036 },
            new object[] { "Venus", 11.98317500, 0.670 },
            new object[] { "Saturn", 160.95951111, 1.097 },
        };

    [Theory]
    [MemberData(nameof(JhoraPositions))]
    public void Planet_matches_Jagannatha_Hora_within_the_ayanamsa_difference(
        string body, double jhoraLongitudeDeg, double jhoraLatitudeDeg)
    {
        var snapshot = SwissEphemerisInterpreter.GetPositions(
            new DateTimeOffset(1981, 4, 22, 5, 30, 1, TimeSpan.FromHours(5.5)),
            13 + 5 / 60.0, 80 + 17 / 60.0, AyanamsaDefinition.TraditionalLahiri);
        var planet = Assert.Single(snapshot.Planets, p => p.Body == body);

        // JHora's ayanamsa is 3.4 arcsec smaller than Traditional Lahiri's; allow 5 arcsec.
        Assert.InRange((planet.LongitudeDeg - jhoraLongitudeDeg) * 3600, -5.0, 5.0);
        Assert.Equal(jhoraLatitudeDeg, planet.LatitudeDeg, precision: 2);
    }

    [Fact]
    public void Ascendant_matches_ikiastrros_verified_reference()
    {
        var snapshot = GetReferenceSnapshot();
        Assert.Equal(0.64044582267632677, snapshot.AscendantLongitudeDeg, precision: 6);
    }

    [Fact]
    public void All_nine_classical_bodies_are_returned_exactly_once()
    {
        var snapshot = GetReferenceSnapshot();
        var expected = new[] { "Sun", "Moon", "Mars", "Mercury", "Jupiter", "Venus", "Saturn", "Rahu", "Ketu" };
        Assert.Equal(expected, snapshot.Planets.Select(p => p.Body));
    }

    [Fact]
    public void Ketu_is_always_exactly_180_degrees_from_Rahu()
    {
        var snapshot = GetReferenceSnapshot();
        var rahu = snapshot.Planets.Single(p => p.Body == "Rahu");
        var ketu = snapshot.Planets.Single(p => p.Body == "Ketu");

        var delta = (ketu.LongitudeDeg - rahu.LongitudeDeg + 360) % 360;
        Assert.Equal(180.0, delta, precision: 9);
    }

    [Fact]
    public void Ketu_mirrors_Rahus_latitude_and_shares_its_speed()
    {
        var snapshot = GetReferenceSnapshot();
        var rahu = snapshot.Planets.Single(p => p.Body == "Rahu");
        var ketu = snapshot.Planets.Single(p => p.Body == "Ketu");

        Assert.Equal(-rahu.LatitudeDeg, ketu.LatitudeDeg, precision: 12);
        Assert.Equal(rahu.SpeedDegPerDay, ketu.SpeedDegPerDay, precision: 12);
    }

    [Fact]
    public void Ayanamsha_for_1981_is_within_the_known_Lahiri_band()
    {
        // Lahiri ayanamsha crosses ~23.6-23.7 deg through the early 1980s (JHora's own
        // True-Chitrapaksha export for this exact chart prints 23-34-49.57 = 23.58 deg;
        // traditional Lahiri (mode 1, used here) runs a few arcminutes from that).
        var snapshot = GetReferenceSnapshot();
        Assert.InRange(snapshot.AyanamshaDegrees, 23.4, 23.8);
    }

    [Fact]
    public void Tropical_ayanamsa_applies_no_correction()
    {
        var snapshot = SwissEphemerisInterpreter.GetPositions(
            ReferenceMoment, ChennaiLatitude, ChennaiLongitude, AyanamsaDefinition.Tropical);
        Assert.Equal(0, snapshot.AyanamshaDegrees);
    }

    [Fact]
    public void Unimplemented_ayanamsa_throws_before_any_Swiss_Ephemeris_call()
    {
        Assert.Throws<NotSupportedException>(() =>
            SwissEphemerisInterpreter.GetPositions(
                ReferenceMoment, ChennaiLatitude, ChennaiLongitude, AyanamsaDefinition.DevaDatta));
    }

    [Fact]
    public void Non_whole_sign_house_system_is_rejected()
    {
        Assert.Throws<NotSupportedException>(() =>
            SwissEphemerisInterpreter.GetPositions(
                ReferenceMoment, ChennaiLatitude, ChennaiLongitude,
                AyanamsaDefinition.TraditionalLahiri, (HouseSystem)999));
    }
}
