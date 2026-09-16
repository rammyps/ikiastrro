namespace SwissEphemeris.Interpreter;

/// <summary>
/// House-cusp system for Ascendant/house-cusp retrieval. Only <see cref="WholeSign"/> is
/// implemented today (matches ikiastrro's current behavior) — the enum exists so other
/// systems are additive later, not a breaking change to <see cref="SwissEphemerisInterpreter.GetPositions"/>'s
/// signature. <c>swe_houses_ex</c> already returns the full 12-cusp array; only the
/// Ascendant (ascmc[0]) is read today, same as before this extraction.
/// </summary>
public enum HouseSystem
{
    WholeSign
}

/// <summary>
/// One body's sidereal longitude/latitude/speed at the requested moment. <paramref name="Body"/>
/// is a plain string ("Sun", "Moon", ..., "Rahu", "Ketu") rather than an enum, so this library
/// carries no caller-domain types in its public surface.
/// </summary>
public readonly record struct PlanetPosition(
    string Body,
    double LongitudeDeg,
    double LatitudeDeg,
    double SpeedDegPerDay);

/// <summary>
/// The Ascendant and the classical nine bodies (7 grahas + the two lunar nodes) at one
/// moment/place, plus the ayanamsha applied and the local sidereal time.
/// </summary>
public sealed record EphemerisSnapshot(
    double AscendantLongitudeDeg,
    IReadOnlyList<PlanetPosition> Planets,
    double AyanamshaDegrees,
    double LocalSiderealTimeHours);

/// <summary>
/// The sunrise/sunset/next-sunrise triple framing the day arc containing <c>localMoment</c> —
/// see <see cref="SwissEphemerisInterpreter.GetSunEvents"/> for the arc-selection rule. No
/// day/night classification here — that's a caller-domain interpretation of these three
/// instants, not an ephemeris concern.
/// </summary>
public sealed record SunEvents(
    DateTimeOffset Sunrise,
    DateTimeOffset Sunset,
    DateTimeOffset NextSunrise);
