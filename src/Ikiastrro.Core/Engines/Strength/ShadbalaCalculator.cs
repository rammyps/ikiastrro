using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Panchanga;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Strength;

/// <summary>
/// PVR-first Shadbala calculator. PVR defines the six strength sources and refers detailed
/// arithmetic to Raman's Graha and Bhava Balas; this class keeps every intermediate component
/// visible so later JHora reconciliation can change one convention without hiding the delta.
/// </summary>
public static class ShadbalaCalculator
{
    private static readonly PlanetName[] ClassicalPlanets =
    {
        PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury,
        PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn
    };

    /// <summary>Deep-exaltation point as an absolute 0-360 longitude, from the shared
    /// AstroMath.DeepExaltationPoints (previously an independent hardcoded copy here — 2026-09-11
    /// rule-mapping audit).</summary>
    private static double DeepExaltationLongitude(PlanetName planet)
    {
        var (sign, degree) = AstroMath.DeepExaltationPoints[planet];
        return (int)sign * 30 + degree;
    }

    private static readonly IReadOnlyDictionary<PlanetName, double> Naisargika =
        new Dictionary<PlanetName, double>
        {
            [PlanetName.Sun] = 60.00,
            [PlanetName.Moon] = 51.43,
            [PlanetName.Venus] = 42.86,
            [PlanetName.Jupiter] = 34.29,
            [PlanetName.Mercury] = 25.71,
            [PlanetName.Mars] = 17.14,
            [PlanetName.Saturn] = 8.57
        };

    private static readonly IReadOnlyDictionary<PlanetName, int> DigBalaHouses =
        new Dictionary<PlanetName, int>
        {
            [PlanetName.Sun] = 10, [PlanetName.Mars] = 10,
            [PlanetName.Moon] = 4, [PlanetName.Venus] = 4,
            [PlanetName.Mercury] = 1, [PlanetName.Jupiter] = 1,
            [PlanetName.Saturn] = 7
        };

    /// <summary>Graha Yuddha (planetary war) participants and orb — tbl_Rule_PlanetaryWar
    /// (migration 072, SRC_RAMAN_GRAHA_BHAVA_BALAS). Sun/Moon/nodes never take part.</summary>
    private static readonly PlanetName[] WarParticipants =
        { PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn };
    private const double WarOrbDegrees = 1.00;

    /// <summary>Calculates the seven classical planets from the D1 and available varga inputs.
    /// <paramref name="panchanga"/> supplies the already-verified Vedic weekday and Hora Lord
    /// (<see cref="PanchangaCalculator"/>) so Dina/Hora Bala reuse that layer instead of
    /// re-deriving it.</summary>
    public static IReadOnlyList<PlanetaryStrengthResult> Calculate(
        IReadOnlyList<ChartAnalysisInput> charts,
        SiderealPositions positions,
        SunTimes sunTimes,
        PanchangaResult panchanga)
    {
        var d1 = charts.FirstOrDefault(c => c.ChartType.Equals("D1", StringComparison.OrdinalIgnoreCase))
                  ?? charts.FirstOrDefault()
                  ?? throw new ArgumentException("At least one chart input is required.", nameof(charts));

        var provisional = new List<(PlanetName Planet, List<ShadbalaComponentResult> Components,
            double Sthana, double Dig, double Kala, double Cheshta, double Naisargika, double Drik,
            double TotalBeforeWar)>(ClassicalPlanets.Length);

        foreach (var planet in ClassicalPlanets)
        {
            var name = planet.ToString();
            var p = d1.Planets.FirstOrDefault(x => x.Planet.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (p is null || p.NirayanaLongitudeDegrees is null) continue;

            var components = new List<ShadbalaComponentResult>();
            AddSthana(components, planet, p, charts);
            AddDig(components, planet, p);
            AddKala(components, planet, p, positions, sunTimes, panchanga);
            AddCheshta(components, planet, p, positions);
            AddNaisargika(components, planet);
            AddDrik(components, planet, d1);

            var sthana = Sum(components, "STHANA_BALA");
            var dig = Sum(components, "DIG_BALA");
            var kala = Sum(components, "KALA_BALA");
            var cheshta = Sum(components, "CHESTA_BALA");
            var naisargika = Sum(components, "NAISARGIKA_BALA");
            var drik = Sum(components, "DRIK_BALA");
            provisional.Add((planet, components, sthana, dig, kala, cheshta, naisargika, drik,
                sthana + dig + kala + cheshta + naisargika + drik));
        }

        // Yuddha Bala (Graha Yuddha) is not one of the six sources; it adjusts the six-fold
        // total afterwards, per tbl_Rule_PlanetaryWar (migration 072).
        var yuddha = ComputeYuddha(d1, positions);

        var results = new List<PlanetaryStrengthResult>(provisional.Count);
        foreach (var row in provisional)
        {
            var components = row.Components;
            var yuddhaVirupas = 0.0;
            if (yuddha.TryGetValue(row.Planet, out var yuddhaRow))
            {
                components.Add(yuddhaRow);
                yuddhaVirupas = yuddhaRow.ValueVirupas;
            }

            var total = row.TotalBeforeWar + yuddhaVirupas;
            var uchcha = components.First(x => x.SubComponentCode == "UCHCHA_BALA").ValueVirupas;
            var ishta = Math.Sqrt(Math.Max(0, uchcha * row.Cheshta));

            results.Add(new PlanetaryStrengthResult(
                row.Planet.ToString(), components, Round(row.Sthana), Round(row.Dig), Round(row.Kala),
                Round(row.Cheshta), Round(row.Naisargika), Round(row.Drik), Round(yuddhaVirupas),
                Round(total), Round(total / 60.0), Round(ishta), Round(60.0 - ishta)));
        }
        return results;
    }

    private static void AddSthana(List<ShadbalaComponentResult> rows, PlanetName planet,
        PlanetPosition p, IReadOnlyList<ChartAnalysisInput> charts)
    {
        var longitude = p.NirayanaLongitudeDegrees!.Value;
        var exalt = DeepExaltationLongitude(planet);
        var distanceFromDebilitation = AngularDistance(longitude, (exalt + 180) % 360);
        rows.Add(Row("STHANA_BALA", "UCHCHA_BALA", distanceFromDebilitation / 3.0,
            "DEBILITATION_DISTANCE", "Raman/PVR: distance from the deep-debilitation point."));

        var saptaTypes = new[] { "D1", "D2", "D3", "D7", "D9", "D12", "D30" };
        var saptaDetails = charts.Where(c => saptaTypes.Contains(c.ChartType, StringComparer.OrdinalIgnoreCase))
            .Select(c => (Chart: c, Position: c.Planets.FirstOrDefault(x => x.Planet.Equals(p.Planet, StringComparison.OrdinalIgnoreCase))))
            .Where(x => x.Position is not null)
            .Select(x =>
            {
                var position = x.Position!;
                var chartLongitude = position.VargaLongitudeDegrees ?? position.NirayanaLongitudeDegrees ?? longitude;
                var degreeInSign = ((chartLongitude % 30) + 30) % 30;
                var sign = Enum.TryParse<ZodiacName>(position.Sign, true, out var parsed) ? parsed : ZodiacName.Aries;
                var signs = x.Chart.Planets
                    .Where(v => Enum.TryParse<PlanetName>(v.Planet, out _))
                    .ToDictionary(v => v.Planet, v => Enum.Parse<ZodiacName>(v.Sign), StringComparer.OrdinalIgnoreCase);
                var dignity = PvrDignityEvaluator.Evaluate(planet, sign, degreeInSign, signs);
                return (ChartType: x.Chart.ChartType, Sign: position.Sign, DegreeInSign: degreeInSign, Dignity: dignity);
            }).ToList();
        var dignityPoints = saptaDetails.Sum(x => x.Dignity.SaptavargajaPoints);
        rows.Add(Row("STHANA_BALA", "SAPTAVARGAJA_BALA", dignityPoints,
            "SAPTA_VARGA_DIGNITY", string.Join("; ", saptaDetails.Select(x =>
                $"{x.ChartType}:{x.Sign} {x.DegreeInSign:0.###}° {x.Dignity.DignityTypeCode} " +
                $"lord={x.Dignity.SignLord} rel={x.Dignity.CompoundRelationshipCode ?? "—"} " +
                $"({x.Dignity.SaptavargajaPoints:0.###})"))));

        var odd = SignIndex(p.Sign) % 2 == 0;
        var sexMatches = planet is PlanetName.Sun or PlanetName.Mars or PlanetName.Jupiter or PlanetName.Saturn
            ? odd : !odd;
        rows.Add(Row("STHANA_BALA", "OJHA_YUGMA_RASYAMSA_BALA", sexMatches ? 30 : 0,
            "ODD_EVEN_D1_D9", "Raman/PVR odd-even sign and planetary sex contribution."));

        rows.Add(Row("STHANA_BALA", "KENDRADI_BALA", p.HouseNumber is 1 or 4 or 7 or 10 ? 60 :
            p.HouseNumber is 2 or 5 or 8 or 11 ? 30 : 15,
            "KENDRA_PANAPHARA_APOKLIMA", "Kendra, panaphara, and apoklima house contribution."));
        rows.Add(Row("STHANA_BALA", "DREKKANA_BALA", (int)((longitude % 30) / 10) switch
        {
            0 when planet is PlanetName.Sun or PlanetName.Mars or PlanetName.Jupiter => 15,
            1 when planet is PlanetName.Moon or PlanetName.Venus => 15,
            2 when planet is PlanetName.Mercury or PlanetName.Saturn => 15,
            _ => 0
        }, "DREKKANA_GROUP", "Drekkana sex/group contribution."));
    }

    private static void AddDig(List<ShadbalaComponentResult> rows, PlanetName planet, PlanetPosition p)
    {
        var houseDistance = ((p.HouseNumber - DigBalaHouses[planet]) % 12 + 12) % 12;
        var fraction = Math.Min(houseDistance, 12 - houseDistance) / 6.0;
        rows.Add(Row("DIG_BALA", "DIG_BALA", 60.0 * (1.0 - fraction),
            "FULL_STRENGTH_HOUSE_DISTANCE", "Directional strength measured from the full-strength house."));
    }

    private static void AddKala(List<ShadbalaComponentResult> rows, PlanetName planet, PlanetPosition p,
        SiderealPositions positions, SunTimes sunTimes, PanchangaResult panchanga)
    {
        var dayStrong = planet is PlanetName.Sun or PlanetName.Jupiter or PlanetName.Venus;
        var nathonnata = sunTimes.IsNightBirth == dayStrong ? 0 : 60;
        rows.Add(Row("KALA_BALA", "NATHONNATA_BALA", nathonnata,
            "DAY_NIGHT_ARC", "Temporal strength from the birth day/night arc."));

        var sun = positions.PlanetLongitudes[PlanetName.Sun];
        var moon = positions.PlanetLongitudes[PlanetName.Moon];
        var phase = AngularDistance(sun, moon);
        var paksha = planet == PlanetName.Moon ? Math.Abs(180 - phase) / 3.0 : 0;
        rows.Add(Row("KALA_BALA", "PAKSHA_BALA", 60 - paksha,
            "MOON_PHASE", "Lunar-phase component; additional calendrical components are added in the next slice."));

        // Dina (Vara) Bala -- 45 virupas to the lord of the birth weekday (sunrise-to-sunrise).
        // Reuses PanchangaCalculator's own weekday-lord mapping (VedicWeekdayId 1=Sunday..7=Saturday
        // -> PlanetName ordinal) rather than re-deriving it -- verify-panchanga already confirms
        // Tuesday for 1_Ramakrishnan.
        var weekdayLord = (PlanetName)(panchanga.VedicWeekdayId - 1);
        rows.Add(Row("KALA_BALA", "DINA_BALA", planet == weekdayLord ? 45 : 0,
            "WEEKDAY_LORD", $"Vara Bala: full strength only to the birth weekday's lord ({weekdayLord})."));

        // Hora Bala -- 60 virupas to the lord of the planetary hour running at birth. Reuses
        // PanchangaCalculator's own Hora Lord (verify-panchanga: Venus for 1_Ramakrishnan).
        var horaLord = AstroIds.PlanetFromId(panchanga.HoraLordPlanetId);
        rows.Add(Row("KALA_BALA", "HORA_BALA", planet == horaLord ? 60 : 0,
            "HORA_LORD", $"Hora Bala: full strength only to the running planetary-hour lord ({horaLord})."));

        // Tribhaga Bala -- day (sunrise-sunset) splits into 3 parts ruled Mercury/Sun/Saturn;
        // night (sunset-next sunrise) into 3 parts ruled Moon/Venus/Mars. 60 virupas to the part's
        // lord; Jupiter is classically exempt and always scores the full 60 regardless of the part.
        // Elapsed-since-sunrise reuses panchanga.JanmaGhatis (already verified) rather than
        // re-deriving the birth moment.
        var (tribhagaValue, tribhagaNarrative) = TribhagaValue(planet, sunTimes, panchanga);
        rows.Add(Row("KALA_BALA", "TRIBHAGA_BALA", tribhagaValue, "DAY_NIGHT_THIRD", tribhagaNarrative));
    }

    private static readonly PlanetName[] DayTribhagaLords = { PlanetName.Mercury, PlanetName.Sun, PlanetName.Saturn };
    private static readonly PlanetName[] NightTribhagaLords = { PlanetName.Moon, PlanetName.Venus, PlanetName.Mars };

    private static (double Value, string Narrative) TribhagaValue(
        PlanetName planet, SunTimes sunTimes, PanchangaResult panchanga)
    {
        var dayLengthMinutes = (sunTimes.Sunset - sunTimes.Sunrise).TotalMinutes;
        var nightLengthMinutes = (sunTimes.NextSunrise - sunTimes.Sunset).TotalMinutes;
        var elapsedSinceSunrise = panchanga.JanmaGhatis * 24.0;

        PlanetName partLord;
        string phase;
        if (elapsedSinceSunrise <= dayLengthMinutes)
        {
            var third = Math.Min(2, (int)(elapsedSinceSunrise / (dayLengthMinutes / 3.0)));
            partLord = DayTribhagaLords[third];
            phase = $"day-third #{third + 1}";
        }
        else
        {
            var intoNight = elapsedSinceSunrise - dayLengthMinutes;
            var third = Math.Min(2, (int)(intoNight / (nightLengthMinutes / 3.0)));
            partLord = NightTribhagaLords[third];
            phase = $"night-third #{third + 1}";
        }

        if (planet == PlanetName.Jupiter)
            return (60, $"Jupiter is classically exempt and always scores full Tribhaga Bala " +
                        $"(birth fell in the {phase}, ruled by {partLord}).");

        return (planet == partLord ? 60 : 0,
            $"Birth fell in the {phase} (ruled by {partLord}); full Tribhaga Bala goes only to that lord.");
    }

    private static void AddCheshta(List<ShadbalaComponentResult> rows, PlanetName planet, PlanetPosition p,
        SiderealPositions positions)
    {
        var value = planet is PlanetName.Sun or PlanetName.Moon ? 0 :
            (p.IsRetrograde == true || positions.PlanetSpeeds.GetValueOrDefault(planet) < 0 ? 60 : 30);
        rows.Add(Row("CHESTA_BALA", "CHESTA_BALA", value,
            "MOTION_AND_RETROGRADE", "Motional strength; mean-motion refinements are retained for JHora reconciliation."));
    }

    private static void AddNaisargika(List<ShadbalaComponentResult> rows, PlanetName planet) =>
        rows.Add(Row("NAISARGIKA_BALA", "NAISARGIKA_BALA", Naisargika[planet],
            "FIXED_PLANET_VALUE", "Permanent natural strength."));

    private static void AddDrik(List<ShadbalaComponentResult> rows, PlanetName planet, ChartAnalysisInput d1)
    {
        var target = d1.Planets.FirstOrDefault(p => p.Planet.Equals(planet.ToString(), StringComparison.OrdinalIgnoreCase));
        if (target?.NirayanaLongitudeDegrees is null) return;
        var total = 0.0;
        foreach (var source in d1.Planets.Where(p => !p.Planet.Equals("Ascendant", StringComparison.OrdinalIgnoreCase)
                                                    && !p.Planet.Equals(planet.ToString(), StringComparison.OrdinalIgnoreCase)
                                                    && Enum.TryParse<PlanetName>(p.Planet, out _)))
        {
            var sourcePlanet = Enum.Parse<PlanetName>(source.Planet);
            var angle = AngularDistance(source.NirayanaLongitudeDegrees ?? 0, target.NirayanaLongitudeDegrees.Value);
            var strength = AspectStrength(angle, sourcePlanet);
            total += IsNaturalBenefic(sourcePlanet) ? strength : -strength;
        }
        rows.Add(Row("DRIK_BALA", "DRIK_BALA", total / 4.0,
            "SPUTA_DRISHTI", "Aspect strength from benefic and malefic planetary influence."));
    }

    /// <summary>
    /// Graha Yuddha (planetary war) among the five tara grahas (tbl_Rule_PlanetaryWar, migration
    /// 072): within <see cref="WarOrbDegrees"/> of D1 longitude, the planet with the more
    /// northern ecliptic latitude wins. The winner/loser detection and criterion are cited
    /// (SRC_RAMAN_GRAHA_BHAVA_BALAS via SRC_PVR_INTEGRATED); the delta MAGNITUDE (the seeded
    /// diameter-based formula) is deliberately left at 0 virupas here -- the Raman edition that
    /// carries the exact apparent-diameter coefficients has no text extract available, so
    /// fabricating a number for it would violate this engine's source-honesty rule. 1_Ramakrishnan
    /// has no war among his five tara grahas (Mars/Mercury/Venus are all >2° apart in Aries;
    /// Jupiter/Saturn are 2°14' apart in Virgo), so this never fires for the golden record.
    /// </summary>
    private static IReadOnlyDictionary<PlanetName, ShadbalaComponentResult> ComputeYuddha(
        ChartAnalysisInput d1, SiderealPositions positions)
    {
        var longitude = new Dictionary<PlanetName, double>();
        foreach (var planet in WarParticipants)
        {
            var pos = d1.Planets.FirstOrDefault(x => x.Planet.Equals(planet.ToString(), StringComparison.OrdinalIgnoreCase));
            if (pos?.NirayanaLongitudeDegrees is { } lon) longitude[planet] = lon;
        }

        var result = new Dictionary<PlanetName, ShadbalaComponentResult>();
        for (var i = 0; i < WarParticipants.Length; i++)
        for (var j = i + 1; j < WarParticipants.Length; j++)
        {
            var a = WarParticipants[i];
            var b = WarParticipants[j];
            if (!longitude.TryGetValue(a, out var lonA) || !longitude.TryGetValue(b, out var lonB)) continue;
            var orb = AngularDistance(lonA, lonB);
            if (orb > WarOrbDegrees) continue;

            var latA = positions.PlanetLatitudes.GetValueOrDefault(a);
            var latB = positions.PlanetLatitudes.GetValueOrDefault(b);
            var (winner, loser) = latA >= latB ? (a, b) : (b, a);

            result[winner] = Row("YUDDHA_BALA", "YUDDHA_BALA", 0, "GRAHA_YUDDHA_LATITUDE",
                $"Graha Yuddha vs {loser}: orb {orb:0.###}°, {winner} wins on more northern latitude " +
                $"({positions.PlanetLatitudes.GetValueOrDefault(winner):0.###}° vs " +
                $"{positions.PlanetLatitudes.GetValueOrDefault(loser):0.###}°). Magnitude deliberately left " +
                "at 0 -- the diameter-based delta in tbl_Rule_PlanetaryWar needs the cited Raman edition " +
                "(SRC_RAMAN_GRAHA_BHAVA_BALAS), which has no text extract available.");
            result[loser] = Row("YUDDHA_BALA", "YUDDHA_BALA", 0, "GRAHA_YUDDHA_LATITUDE",
                $"Graha Yuddha vs {winner}: orb {orb:0.###}°, {loser} loses on more southern latitude. " +
                $"Magnitude deferred -- see {winner}'s row.");
        }
        return result;
    }

    private static ShadbalaComponentResult Row(string bala, string sub, double value, string method, string narrative) =>
        new(bala, sub, Round(value), method, narrative);

    private static double Sum(IEnumerable<ShadbalaComponentResult> rows, string bala) => rows.Where(x => x.BalaCode == bala).Sum(x => x.ValueVirupas);
    private static double Round(double value) => Math.Round(value, 3, MidpointRounding.AwayFromZero);
    private static int SignIndex(string sign) => Enum.TryParse<ZodiacName>(sign, true, out var z) ? (int)z : 0;
    private static double AngularDistance(double a, double b)
    {
        var d = Math.Abs(((a - b) % 360 + 360) % 360);
        return d > 180 ? 360 - d : d;
    }

    private static bool IsNaturalBenefic(PlanetName planet) => planet is PlanetName.Moon or PlanetName.Mercury or PlanetName.Jupiter or PlanetName.Venus;

    private static double AspectStrength(double angle, PlanetName source)
    {
        var special = source switch
        {
            PlanetName.Mars => new[] { 90d, 210d },
            PlanetName.Jupiter => new[] { 120d, 240d },
            PlanetName.Saturn => new[] { 60d, 270d },
            _ => Array.Empty<double>()
        };
        if (special.Any(x => AngularDistance(angle, x) <= 8)) return 60;
        if (Math.Abs(angle - 180) <= 8) return 60;
        if (angle < 30 || angle > 150) return 0;
        return angle <= 90 ? (angle - 30) : (150 - angle);
    }
}
