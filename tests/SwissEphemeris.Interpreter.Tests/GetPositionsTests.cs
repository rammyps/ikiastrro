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
            new object[] { "Sun", 8.1838962001970987 },
            new object[] { "Moon", 217.27711923514235 },
            new object[] { "Mars", 3.9241242774169556 },
            new object[] { "Mercury", 1.8115564192190432 },
            new object[] { "Jupiter", 158.71365169336244 },
            new object[] { "Venus", 11.964536501879603 },
            new object[] { "Saturn", 160.94421816272668 },
            new object[] { "Rahu", 103.04347855579427 },
            new object[] { "Ketu", 283.04347855579425 },
        };

    [Theory]
    [MemberData(nameof(ExpectedLongitudes))]
    public void Planet_longitude_matches_ikiastrros_verified_reference(string body, double expectedLongitudeDeg)
    {
        var snapshot = GetReferenceSnapshot();
        var planet = Assert.Single(snapshot.Planets, p => p.Body == body);
        Assert.Equal(expectedLongitudeDeg, planet.LongitudeDeg, precision: 6);
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
