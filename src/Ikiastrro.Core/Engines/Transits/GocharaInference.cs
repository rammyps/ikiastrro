using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;

namespace Ikiastrro.Core.Engines.Transits;

/// <summary>Net reading of one transiting planet from the natal Moon: PVR's verdict
/// (<see cref="GocharaVerdict"/>) combined with its own BAV bindus band. A project heuristic —
/// PVR gives no way to combine the two (docs/architecture/transit_gochara_inference.md §3).</summary>
public enum GocharaTier { Supportive, Mild, Mixed, Obstructed, Unfavourable }

public sealed record GocharaPlanetReading(GocharaRow Row, GocharaTier Tier, string Line);

/// <summary>A slow planet's next sign crossing: where it enters, which house that is from the natal
/// Moon, and the verdict before Vedha (the other planets' future signs are not known).</summary>
public sealed record GocharaSlowChange(
    PlanetName Planet, DateTime AtUtc, ZodiacName Sign, int HouseFromMoon, GocharaVerdict VerdictBeforeVedha,
    bool IsRetrograde, SaturnPhase SaturnPhaseThen);

/// <summary>A slow planet's next crossing as the transit table records it.</summary>
public sealed record GocharaIngress(DateTime AtUtc, ZodiacName Sign, bool IsRetrograde);

/// <param name="Slow">Saturn, Jupiter, Rahu, Ketu — the long-running influences, Saturn first.</param>
/// <param name="Fast">Moon, Venus, Mars, Mercury, Sun — days to weeks; shown behind an expand control.</param>
/// <param name="Qualifiers">Natal-promise and dasha lines; empty until those slices are built.</param>
public sealed record GocharaInference(
    SaturnPhase Saturn,
    IReadOnlyList<GocharaPlanetReading> Slow,
    IReadOnlyList<GocharaPlanetReading> Fast,
    IReadOnlyList<GocharaSlowChange> Changes,
    IReadOnlyList<string> Qualifiers,
    string Caveat);

/// <summary>
/// Reads a <see cref="GocharaReadingResult"/> as a whole: the net tier per planet, the slow planets
/// split from the fast, and each slow planet's next crossing. Pure.
/// </summary>
public static class GocharaInferenceBuilder
{
    public const string Caveat =
        "Moon-reference reading only. It confirms or tempers the natal promise and the running dasha; it is not a prediction.";

    public static readonly IReadOnlyList<PlanetName> SlowPlanets =
        [PlanetName.Saturn, PlanetName.Jupiter, PlanetName.Rahu, PlanetName.Ketu];

    /// <summary>Verdict × bindus band. Blocked is always Obstructed (PVR: the planet "cannot give his
    /// good results"); a strong bindu count lifts an unlisted house only to Mixed; Rahu and Ketu have
    /// no BAV, so the verdict alone decides.</summary>
    public static GocharaTier TierOf(GocharaVerdict verdict, int? bindus)
    {
        var band = StrengthBands.BhinnaAshtakavargaBindus.Classify(bindus);
        return verdict switch
        {
            GocharaVerdict.Blocked => GocharaTier.Obstructed,
            GocharaVerdict.Good => band switch
            {
                StrengthTier.Strong => GocharaTier.Supportive,
                StrengthTier.Weak => GocharaTier.Mixed,
                _ => GocharaTier.Mild,
            },
            _ => band == StrengthTier.Strong ? GocharaTier.Mixed : GocharaTier.Unfavourable,
        };
    }

    public static GocharaInference Build(
        GocharaReadingResult reading,
        IReadOnlyDictionary<PlanetName, GocharaIngress> ingress,
        IReadOnlyList<GocharaVedhaRule> rules)
    {
        var readings = reading.Rows.Select(r => new GocharaPlanetReading(r, TierOf(r.Verdict, r.Bindus), Describe(r))).ToList();
        var slow = readings.Where(r => SlowPlanets.Contains(r.Row.Planet)).ToList();
        var fast = readings.Where(r => !SlowPlanets.Contains(r.Row.Planet)).ToList();

        var changes = new List<GocharaSlowChange>();
        foreach (var planet in SlowPlanets)
        {
            if (!ingress.TryGetValue(planet, out var next)) continue;
            // Alone in the sky, so no Vedha: the verdict of the house itself.
            var alone = GocharaReading.Read(reading.NatalMoonSign,
                new Dictionary<PlanetName, ZodiacName> { [planet] = next.Sign }, rules);
            var row = alone.Rows.Single();
            var saturnPhase = planet == PlanetName.Saturn ? GocharaReading.PhaseOf(row.HouseFromMoon) : SaturnPhase.None;
            changes.Add(new GocharaSlowChange(planet, next.AtUtc, next.Sign, row.HouseFromMoon, row.Verdict,
                next.IsRetrograde, saturnPhase));
        }

        return new GocharaInference(reading.Saturn, slow, fast, changes, Array.Empty<string>(), Caveat);
    }

    /// <summary>One line from the row's own facts — no prose from the books.</summary>
    private static string Describe(GocharaRow r)
    {
        var read = r.ReadAs is { } proxy ? $"{r.Planet} (read as {proxy})" : r.Planet.ToString();
        var house = $"{Ordinal(r.HouseFromMoon)} from the Moon";
        var head = r.Verdict switch
        {
            GocharaVerdict.Good => $"{house} is an auspicious house for {read}, and nothing obstructs it",
            GocharaVerdict.Blocked =>
                $"{house} is an auspicious house for {read}, but {string.Join(", ", r.Vedha.Obstructors)} in its Vedha house ({Ordinal(r.Vedha.VedhaHouse ?? 0)}) obstructs it",
            _ => $"{house} is not an auspicious house for {read}",
        };
        var tail = r.Bindus is { } b
            ? $"; its own bindus here: {b}/8 ({BandWord(b)})"
            : string.Empty;
        return head + tail + ".";
    }

    private static string BandWord(int bindus) => StrengthBands.BhinnaAshtakavargaBindus.Classify(bindus) switch
    {
        StrengthTier.Strong => "strong",
        StrengthTier.Weak => "weak",
        _ => "moderate",
    };

    private static string Ordinal(int n) =>
        n + (n % 100 is 11 or 12 or 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" });
}
