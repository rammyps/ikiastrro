using SwissEphemeris.Interpreter;
using Xunit;

namespace SwissEphemeris.Interpreter.Tests;

/// <summary>
/// Spot-checks <see cref="SwissEphemerisInterpreter.GetSunEvents"/> against ikiastrro's own
/// golden record (JHora prints "Sunrise: 5:56:39 (April 21)" / "Sunset: 18:18:53 (April 21)"
/// for this exact birth — <c>docs/artifacts/reference-charts/Rammy_Jagannatha.txt</c>).
/// ikiastrro's CLI <c>verify-jaimini</c> asserts a 5-second tolerance against the same
/// numbers for the same reason quoted here: SwissEphNet ships no <c>.se1</c> files, so
/// <c>swe_rise_trans</c> falls back to Moshier rather than the full ephemeris JHora uses.
/// </summary>
public class GetSunEventsTests
{
    // 22 Apr 1981 05:30:00 +05:30, Chennai — a night birth (before that calendar day's
    // sunrise), so the day arc it falls in opened at the PREVIOUS day's (21 Apr) sunrise.
    private static readonly DateTimeOffset ReferenceMoment =
        new(1981, 4, 22, 5, 30, 0, TimeSpan.FromHours(5.5));
    private const double ChennaiLatitude = 13.083694;
    private const double ChennaiLongitude = 80.270186;
    private static readonly TimeSpan Tolerance = TimeSpan.FromSeconds(5);

    private static SunEvents GetReferenceEvents() =>
        SwissEphemerisInterpreter.GetSunEvents(ReferenceMoment, ChennaiLatitude, ChennaiLongitude);

    [Fact]
    public void Sunrise_opens_the_previous_calendar_days_arc_for_a_night_birth()
    {
        var events = GetReferenceEvents();
        var expected = new DateTimeOffset(1981, 4, 21, 5, 56, 39, TimeSpan.FromHours(5.5));
        Assert.True((events.Sunrise - expected).Duration() <= Tolerance,
            $"Expected {expected:O} +/-{Tolerance}, got {events.Sunrise:O}");
    }

    [Fact]
    public void Sunset_splits_that_same_arc_from_its_night()
    {
        var events = GetReferenceEvents();
        var expected = new DateTimeOffset(1981, 4, 21, 18, 18, 53, TimeSpan.FromHours(5.5));
        Assert.True((events.Sunset - expected).Duration() <= Tolerance,
            $"Expected {expected:O} +/-{Tolerance}, got {events.Sunset:O}");
    }

    [Fact]
    public void Next_sunrise_closes_the_arc_on_the_birth_calendar_date()
    {
        var events = GetReferenceEvents();
        Assert.Equal(new DateOnly(1981, 4, 22), DateOnly.FromDateTime(events.NextSunrise.Date));
        Assert.Equal(5, events.NextSunrise.Hour);
        Assert.Equal(56, events.NextSunrise.Minute);
    }

    [Fact]
    public void Arc_ordering_is_sunrise_before_sunset_before_next_sunrise()
    {
        var events = GetReferenceEvents();
        Assert.True(events.Sunrise < events.Sunset);
        Assert.True(events.Sunset < events.NextSunrise);
    }

    [Fact]
    public void A_daytime_moment_uses_its_own_calendar_days_sunrise()
    {
        // Same place, but a moment safely after that day's sunrise (10:00 local) — the arc
        // should open with the 22 Apr sunrise itself, not the 21 Apr one.
        var daytimeMoment = new DateTimeOffset(1981, 4, 22, 10, 0, 0, TimeSpan.FromHours(5.5));
        var events = SwissEphemerisInterpreter.GetSunEvents(daytimeMoment, ChennaiLatitude, ChennaiLongitude);
        Assert.Equal(new DateOnly(1981, 4, 22), DateOnly.FromDateTime(events.Sunrise.Date));
    }
}
