using SwissEphNet;

namespace SwissEphemeris.Interpreter;

/// <summary>
/// A thin, domain-agnostic wrapper over SwissEphNet — a direct managed C# port of Astrodienst's
/// Swiss Ephemeris (the same precision source behind JHora, Parashara's Light, and Jyotish
/// Dashboard) — using its Moshier analytical mode (SEFLG_MOSEPH): no external ephemeris data
/// files to bundle or configure, ~1 arcsecond accuracy.
///
/// Extracted out of ikiastrro's Ikiastrro.Core (decision 003 Part C) so this wrapper can be
/// taken as a dependency by another .NET project as-is — no <c>Ikiastrro.*</c> types appear
/// anywhere in this project's public surface, only plain records/primitives (see
/// <see cref="EphemerisSnapshot"/>, <see cref="SunEvents"/>, <see cref="PlanetPosition"/>).
/// Pure extraction, not a rebuild: every flag and formula below is unchanged from
/// ikiastrro's <c>SwissEphemerisProvider</c> as it stood before this split — see this
/// project's README.md and ikiastrro's own <c>decisions/003-...md</c> for the full rationale.
/// </summary>
public static class SwissEphemerisInterpreter
{
    /// <summary>
    /// Sidereal longitudes are requested directly via SEFLG_SIDEREAL + the caller's
    /// <see cref="AyanamsaDefinition.SwissSiderealMode"/> — Swiss Ephemeris subtracts the
    /// ayanamsha internally before returning xx[0], no separate correction step needed.
    ///
    /// Rahu is the MEAN lunar node (SE_MEAN_NODE) — ikiastrro's cross-check against Prokerala/
    /// AstroSage on its established test chart (22 Apr 1981, Chennai) found the mean node
    /// within 0.6 arcmin of both, while the astronomically "truer" oscillating true node
    /// (SE_TRUE_NODE) was off by ~6.8 arcmin — mainstream Vedic tools key off the mean node.
    /// Ketu is derived as Rahu + 180°: Swiss Ephemeris has no separate Ketu body, since it is
    /// not a real celestial body.
    ///
    /// Only <see cref="HouseSystem.WholeSign"/> is implemented (swe_houses_ex house-system
    /// char <c>'W'</c>) — see <see cref="HouseSystem"/>.
    /// </summary>
    public static EphemerisSnapshot GetPositions(
        DateTimeOffset localMoment, double latitudeDeg, double longitudeDeg,
        AyanamsaDefinition ayanamsa, HouseSystem houseSystem = HouseSystem.WholeSign)
    {
        if (houseSystem != HouseSystem.WholeSign)
            throw new NotSupportedException($"House system '{houseSystem}' is not implemented yet — only WholeSign.");
        if (!ayanamsa.IsImplemented)
            throw new NotSupportedException($"Ayanamsa '{ayanamsa.DisplayName}' is catalogued but has no calculation formula yet.");

        using var sweph = new SwissEph();
        if (!ayanamsa.IsTropical)
            sweph.swe_set_sid_mode(ayanamsa.SwissSiderealMode!.Value, 0, 0);

        var utc = localMoment.ToUniversalTime();
        var utHours = utc.Hour + utc.Minute / 60.0 + utc.Second / 3600.0;
        var jd = sweph.swe_julday(utc.Year, utc.Month, utc.Day, utHours, SwissEph.SE_GREG_CAL);

        var flags = SwissEph.SEFLG_MOSEPH | SwissEph.SEFLG_SPEED;
        if (!ayanamsa.IsTropical) flags |= SwissEph.SEFLG_SIDEREAL;

        // xx[0] = longitude, xx[1] = ecliptic latitude (deg), xx[3] = daily motion speed in
        // longitude (deg/day) — negative means retrograde. Returned together since callers
        // need the longitude and the latitude/speed come free from the same swe_calc_ut call
        // (SEFLG_SPEED is already set).
        (double Longitude, double Latitude, double Speed) GetPosition(int body, string label)
        {
            var xx = new double[6];
            var serr = "";
            var result = sweph.swe_calc_ut(jd, body, flags, xx, ref serr);
            if (result < 0)
                throw new InvalidOperationException($"Swiss Ephemeris calculation failed for {label}: {serr}");
            return (xx[0], xx[1], xx[3]);
        }

        var planets = new List<PlanetPosition>(9);
        void AddPosition(string body, int swissBody)
        {
            var (longitude, latitude, speed) = GetPosition(swissBody, body);
            planets.Add(new PlanetPosition(body, longitude, latitude, speed));
        }

        AddPosition("Sun", SwissEph.SE_SUN);
        AddPosition("Moon", SwissEph.SE_MOON);
        AddPosition("Mars", SwissEph.SE_MARS);
        AddPosition("Mercury", SwissEph.SE_MERCURY);
        AddPosition("Jupiter", SwissEph.SE_JUPITER);
        AddPosition("Venus", SwissEph.SE_VENUS);
        AddPosition("Saturn", SwissEph.SE_SATURN);

        var (rahuLongitude, rahuLatitude, rahuSpeed) = GetPosition(SwissEph.SE_MEAN_NODE, "Rahu");
        planets.Add(new PlanetPosition("Rahu", rahuLongitude, rahuLatitude, rahuSpeed));
        // Ketu is a computed point 180° from Rahu, not a separately-tracked body — it moves
        // exactly as Rahu does, so its retrograde status is Rahu's speed sign, unchanged by
        // the 180° offset. The mean node lies on the ecliptic (latitude ~0); Ketu takes the
        // opposite-signed latitude.
        var ketuLongitude = Normalize(rahuLongitude + 180);
        planets.Add(new PlanetPosition("Ketu", ketuLongitude, -rahuLatitude, rahuSpeed));

        var cusps = new double[13];
        var ascmc = new double[10];
        var houseErr = "";
        var houseFlags = SwissEph.SEFLG_MOSEPH | (ayanamsa.IsTropical ? 0 : SwissEph.SEFLG_SIDEREAL);
        var houseResult = sweph.swe_houses_ex(jd, houseFlags, latitudeDeg, longitudeDeg, 'W', cusps, ascmc);
        if (houseResult < 0)
            throw new InvalidOperationException($"Swiss Ephemeris house/ascendant calculation failed: {houseErr}");
        var ascendantLongitude = ascmc[0];

        // ascmc[2] = ARMC (Right Ascension of the MC) in degrees = the local apparent
        // sidereal time; /15 -> hours, normalised to [0,24).
        var lst = (ascmc[2] / 15.0) % 24.0;
        if (lst < 0) lst += 24.0;
        var ayanamshaDegrees = ayanamsa.IsTropical ? 0 : sweph.swe_get_ayanamsa_ut(jd);

        return new EphemerisSnapshot(ascendantLongitude, planets, ayanamshaDegrees, lst);
    }

    /// <summary>
    /// Sunrise / sunset / next-sunrise for one place framing the day arc that contains
    /// <paramref name="localMoment"/> — see <see cref="SunEvents"/>.
    ///
    /// Path taken: SwissEphNet 2.8.0.2 DOES expose <c>swe_rise_trans</c> plus the
    /// <c>SE_CALC_RISE</c> / <c>SE_CALC_SET</c> / <c>SE_BIT_DISC_CENTER</c> /
    /// <c>SE_BIT_NO_REFRACTION</c> constants (verified by reflecting the shipped
    /// <c>SwissEphNet.dll</c>). Signature used:
    /// <c>int swe_rise_trans(double tjd_ut, int ipl, string starname, int epheflag,
    /// int rsmi, double[] geopos, double atpress, double attemp, ref double tret,
    /// ref string serr)</c>.
    ///
    /// Rise/set flag combination: <c>SE_BIT_DISC_CENTER | SE_BIT_NO_REFRACTION</c> — i.e.
    /// geometric centre of the Sun's disc crossing the true horizon, no atmospheric
    /// refraction. This matches JHora / Parashara's Light's default. SwissEphNet ships no
    /// <c>.se1</c> files, so <c>swe_rise_trans</c> falls back to the Moshier analytic theory
    /// rather than the full Swiss Ephemeris JHora uses, and delta-T handling differs
    /// slightly — a small, fixed early bias (ikiastrro's own golden-record check found ~2s
    /// on a 1981 Chennai birth). Toggling either bit off shifts sunrise by ~1–2 min
    /// (refraction) or ~2.5 min (disc), which is a real break; the couple-of-seconds Moshier
    /// gap is not.
    ///
    /// Arc-selection rule: the returned <see cref="SunEvents.Sunrise"/> is whichever sunrise
    /// opens the sun-day containing <paramref name="localMoment"/> — the same calendar day's
    /// sunrise if <paramref name="localMoment"/> falls on/after it, otherwise the previous
    /// calendar day's. <see cref="SunEvents.Sunset"/> splits that arc from its night;
    /// <see cref="SunEvents.NextSunrise"/> closes it.
    /// </summary>
    public static SunEvents GetSunEvents(DateTimeOffset localMoment, double latitudeDeg, double longitudeDeg)
    {
        var offset = localMoment.Offset;
        using var sweph = new SwissEph();

        var geopos = new[] { longitudeDeg, latitudeDeg, 0.0 };

        double JdOf(DateTimeOffset t)
        {
            var u = t.ToUniversalTime();
            return sweph.swe_julday(u.Year, u.Month, u.Day,
                u.Hour + u.Minute / 60.0 + u.Second / 3600.0, SwissEph.SE_GREG_CAL);
        }

        DateTimeOffset LocalOf(double jdUt)
        {
            // SwissEphNet's swe_revjul takes ref (not out) parameters.
            int y = 0, mo = 0, d = 0;
            double h = 0;
            sweph.swe_revjul(jdUt, SwissEph.SE_GREG_CAL, ref y, ref mo, ref d, ref h);
            var whole = (int)h;
            var min = (int)((h - whole) * 60);
            var sec = (int)Math.Round(((h - whole) * 60 - min) * 60);
            var utc = new DateTimeOffset(y, mo, d, whole, min, 0, TimeSpan.Zero).AddSeconds(sec);
            return utc.ToOffset(offset);
        }

        double NextEvent(double fromJd, int rsmi)
        {
            double tret = 0;
            string serr = "";
            var rc = sweph.swe_rise_trans(fromJd, SwissEph.SE_SUN, null,
                SwissEph.SEFLG_MOSEPH, rsmi, geopos, 0.0, 0.0, ref tret, ref serr);
            if (rc < 0) throw new InvalidOperationException($"swe_rise_trans failed: {serr}");
            return tret;
        }

        const int riseFlag = SwissEph.SE_CALC_RISE | SwissEph.SE_BIT_DISC_CENTER | SwissEph.SE_BIT_NO_REFRACTION;
        const int setFlag = SwissEph.SE_CALC_SET | SwissEph.SE_BIT_DISC_CENTER | SwissEph.SE_BIT_NO_REFRACTION;

        // Search anchor: local midnight of localMoment's calendar date, expressed in UT.
        var localMidnight = new DateTimeOffset(localMoment.Date, offset);
        var midnightJd = JdOf(localMidnight);

        var calendarDateSunriseJd = NextEvent(midnightJd, riseFlag);
        var calendarDateSunrise = LocalOf(calendarDateSunriseJd);
        var precedesCalendarDateSunrise = localMoment < calendarDateSunrise;

        var arcSunriseJd = precedesCalendarDateSunrise ? NextEvent(midnightJd - 1.0, riseFlag) : calendarDateSunriseJd;
        var arcSunsetJd = NextEvent(arcSunriseJd, setFlag);
        var sunrise = LocalOf(arcSunriseJd);
        var sunset = LocalOf(arcSunsetJd);
        var nextSunrise = LocalOf(NextEvent(arcSunsetJd, riseFlag));

        return new SunEvents(sunrise, sunset, nextSunrise);
    }

    /// <summary>Normalizes any longitude into [0, 360).</summary>
    private static double Normalize(double longitudeDegrees)
    {
        var d = longitudeDegrees % 360.0;
        return d < 0 ? d + 360.0 : d;
    }
}
