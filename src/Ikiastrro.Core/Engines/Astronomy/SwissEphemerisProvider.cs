using Ikiastrro.Core.Models;
using SwissEphemeris.Interpreter;

namespace Ikiastrro.Core.Engines.Astronomy;

/// <summary>
/// Sidereal (nirayana, Lahiri) longitudes for the Ascendant and all 9 planets at one moment/place,
/// plus each planet's ecliptic latitude (deg) and daily motion speed in longitude (deg/day —
/// negative means retrograde). The Ascendant has no latitude/speed/retrograde concept (it's a
/// house-circle point, not an orbiting body), so PlanetLatitudes/PlanetSpeeds only cover the 9
/// planets, same set as PlanetLongitudes.
/// </summary>
public record SiderealPositions(
    double AscendantLongitude,
    IReadOnlyDictionary<PlanetName, double> PlanetLongitudes,
    IReadOnlyDictionary<PlanetName, double> PlanetLatitudes,
    IReadOnlyDictionary<PlanetName, double> PlanetSpeeds,
    double AyanamshaDegrees,
    double LocalSiderealTimeHours)
{
    public string AyanamsaCode { get; init; } = AyanamsaDefinition.Default.Code;
}

/// <summary>
/// The three sunrise/sunset instants that frame the Vedic day the birth falls in, plus a
/// night-birth flag. All are local-offset <see cref="DateTimeOffset"/>s built from
/// <c>BirthDetails.UtcOffset</c>.
///
/// <see cref="Sunrise"/> opens that Vedic day, <see cref="Sunset"/> splits it from its
/// night, <see cref="NextSunrise"/> closes it. So the <b>day arc</b> is
/// [<see cref="Sunrise"/>, <see cref="Sunset"/>] and the <b>night arc</b> is
/// [<see cref="Sunset"/>, <see cref="NextSunrise"/>] — Gulika / Maandi (Task 5) divide
/// whichever arc the birth is in into eight.
///
/// For a "night birth" — birth after midnight but before that calendar day's sunrise, as
/// 1_Ramakrishnan's 05:30 vs 05:56 sunrise is — the Vedic day began at the PREVIOUS
/// calendar day's sunrise, so <see cref="Sunrise"/> / <see cref="Sunset"/> are that prior
/// day's pair and <see cref="NextSunrise"/> is the birth calendar date's sunrise. This
/// matches JHora, which prints "Sunrise: 5:56:39 (April 21)" for this 22-Apr-1981 birth.
/// Hora Lagna's reference sunrise is always <see cref="Sunrise"/>.
/// </summary>
public record SunTimes(
    DateTimeOffset Sunrise,
    DateTimeOffset Sunset,
    DateTimeOffset NextSunrise,
    bool IsNightBirth);

/// <summary>
/// ikiastrro's astro-domain adapter over <c>SwissEphemeris.Interpreter</c> (decision 003 Part C —
/// the actual Swiss Ephemeris calls, flags, and formulas live there now; this class only maps
/// its plain records to ikiastrro's <see cref="PlanetName"/>/<see cref="BirthDetails"/> types).
/// Pure extraction: every public signature and every returned value here is unchanged from
/// before the split — same flags, same node/Ketu handling, same rise/set bias, byte-identical
/// output on ikiastrro's existing CLI <c>verify-*</c> checks.
/// </summary>
public static class SwissEphemerisProvider
{
    public static SiderealPositions GetSiderealPositions(
        DateTimeOffset localMoment, double latitude, double longitude,
        AyanamsaDefinition? ayanamsa = null)
    {
        ayanamsa ??= AyanamsaDefinition.Default;
        if (!ayanamsa.IsImplemented)
            throw new NotSupportedException($"Ayanamsa '{ayanamsa.DisplayName}' is catalogued but has no calculation formula yet.");

        var snapshot = SwissEphemerisInterpreter.GetPositions(localMoment, latitude, longitude, ayanamsa);

        var planetLongitudes = new Dictionary<PlanetName, double>();
        var planetLatitudes = new Dictionary<PlanetName, double>();
        var planetSpeeds = new Dictionary<PlanetName, double>();
        foreach (var p in snapshot.Planets)
        {
            // PlanetName's member names ("Sun", ..., "Rahu", "Ketu") were chosen to match
            // the interpreter's plain body identifiers exactly — see PlanetName.cs.
            var planet = Enum.Parse<PlanetName>(p.Body);
            planetLongitudes[planet] = p.LongitudeDeg;
            planetLatitudes[planet] = p.LatitudeDeg;
            planetSpeeds[planet] = p.SpeedDegPerDay;
        }

        return new SiderealPositions(
            snapshot.AscendantLongitudeDeg, planetLongitudes, planetLatitudes, planetSpeeds,
            snapshot.AyanamshaDegrees, snapshot.LocalSiderealTimeHours)
        { AyanamsaCode = ayanamsa.Code };
    }

    /// <summary>Convenience overload for callers (e.g. ChartGenerationService in the
    /// Data layer) that hold a BirthDetails but cannot reach the internal
    /// BirthMomentFactory.</summary>
    public static SiderealPositions GetSiderealPositions(BirthDetails birthDetails, AyanamsaDefinition? ayanamsa = null) =>
        GetSiderealPositions(
            BirthMomentFactory.Create(birthDetails),
            birthDetails.Latitude,
            birthDetails.Longitude, ayanamsa);

    /// <summary>
    /// Sunrise / sunset for a person's birth date &amp; place — see <see cref="SunTimes"/>.
    /// <see cref="SunTimes.IsNightBirth"/> is ikiastrro's own domain read of the interpreter's
    /// plain <see cref="SunEvents"/> triple (birth precedes that day's sunrise, or falls past
    /// its sunset) — the interpreter itself carries no "night birth" concept.
    /// </summary>
    public static SunTimes GetSunTimes(BirthDetails birthDetails)
    {
        var moment = BirthMomentFactory.Create(birthDetails);
        var events = SwissEphemerisInterpreter.GetSunEvents(moment, birthDetails.Latitude, birthDetails.Longitude);
        var isNightBirth = moment < events.Sunrise || moment >= events.Sunset;
        return new SunTimes(events.Sunrise, events.Sunset, events.NextSunrise, isNightBirth);
    }
}
