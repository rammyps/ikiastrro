using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Transits;

/// <summary>How a transit reads by PVR ch.26.3's table: an auspicious house that no planet
/// blocks (Good), an auspicious house blocked by Vedha (Blocked), or a house the table does not
/// list as auspicious (NotFavourable).</summary>
public enum GocharaVerdict { Good, Blocked, NotFavourable }

/// <summary>Saturn counted from the natal Moon: Sāḍe Sātī (12th, 1st, 2nd — its three
/// Dhaiyas), Kaṇṭaka (4th) and Aṣṭama (8th) Śani. The same offsets as
/// tvf_Chart_SadeSatiPeriods (docs/cli/calculations.md §9).</summary>
public enum SaturnPhase { None, SadeSatiRising, SadeSatiPeak, SadeSatiSetting, Kantaka, Ashtama }

/// <param name="ReadAs">The planet whose table row a node is read by — Saturn for Rahu, Mars
/// for Ketu (PVR ch.25, after Table 59); null for the seven planets, read by their own row.</param>
/// <param name="Bindus">The planet's own Bhinnāṣṭakavarga bindus in the sign it is crossing
/// (natal D1); null for Rahu/Ketu, which have no BAV.</param>
public sealed record GocharaRow(
    PlanetName Planet,
    ZodiacName Sign,
    int HouseFromMoon,
    GocharaVerdict Verdict,
    GocharaVedhaResult Vedha,
    int? Bindus,
    PlanetName? ReadAs = null);

public sealed record GocharaReadingResult(ZodiacName NatalMoonSign, IReadOnlyList<GocharaRow> Rows, SaturnPhase Saturn);

/// <summary>
/// Every transiting planet read from the natal Moon (PVR ch.26): its house from the Moon, whether
/// that house is auspicious for it and whether another transiting planet blocks it by Vedha
/// (<see cref="GocharaVedhaCalculator"/>, tbl_Rule_GocharaVedha), and its own BAV bindus in that
/// sign. The bindus are reported beside the verdict, not folded into it. Rahu is read by Saturn's
/// row and Ketu by Mars's ("Rahu's behavior is similar to that of Saturn's and Ketu's behavior to
/// Mars's", PVR ch.25); the two nodes never cause Vedha on each other — they are always 7th apart,
/// so otherwise each would block the other's 11th-house transit every time. Rows run slowest
/// first: Saturn, Jupiter, Rahu, Ketu, Moon, Venus, Mars, Mercury, Sun. Pure.
/// </summary>
public static class GocharaReading
{
    public static readonly IReadOnlyList<PlanetName> RowOrder =
    [
        PlanetName.Saturn, PlanetName.Jupiter, PlanetName.Rahu, PlanetName.Ketu, PlanetName.Moon,
        PlanetName.Venus, PlanetName.Mars, PlanetName.Mercury, PlanetName.Sun,
    ];

    /// <summary>The planet whose PVR row each node borrows.</summary>
    public static PlanetName? NodeProxy(PlanetName node) => node switch
    {
        PlanetName.Rahu => PlanetName.Saturn,
        PlanetName.Ketu => PlanetName.Mars,
        _ => null,
    };

    public static GocharaReadingResult Read(
        ZodiacName natalMoonSign,
        IReadOnlyDictionary<PlanetName, ZodiacName> transitSigns,
        IReadOnlyList<GocharaVedhaRule> rules,
        Func<PlanetName, ZodiacName, int?>? bavBindus = null)
    {
        var allRules = rules.Concat(NodeRules(PlanetName.Rahu, rules)).Concat(NodeRules(PlanetName.Ketu, rules)).ToList();
        var rows = RowOrder
            .Where(transitSigns.ContainsKey)
            .Select(planet =>
            {
                var sign = transitSigns[planet];
                var house = AstroMath.CountFromSignToSign(natalMoonSign, sign);
                var vedha = GocharaVedhaCalculator.Evaluate(planet, natalMoonSign, transitSigns, allRules);
                var verdict = !vedha.IsAuspicious ? GocharaVerdict.NotFavourable
                    : vedha.IsObstructed ? GocharaVerdict.Blocked
                    : GocharaVerdict.Good;
                var proxy = NodeProxy(planet);
                return new GocharaRow(planet, sign, house, verdict, vedha,
                    proxy is null ? bavBindus?.Invoke(planet, sign) : null, proxy);
            })
            .ToList();

        var saturn = rows.FirstOrDefault(r => r.Planet == PlanetName.Saturn);
        return new GocharaReadingResult(natalMoonSign, rows, saturn is null ? SaturnPhase.None : PhaseOf(saturn.HouseFromMoon));
    }

    /// <summary>The proxy planet's rows re-keyed to the node. The proxy's own exclusion (Sun–Saturn)
    /// is a pair rule between those two planets, so it is replaced by the other node.</summary>
    private static IEnumerable<GocharaVedhaRule> NodeRules(PlanetName node, IReadOnlyList<GocharaVedhaRule> rules) =>
        rules.Where(r => r.TransitPlanet == NodeProxy(node))
             .Select(r => r with
             {
                 TransitPlanet = node,
                 ExcludedObstructorPlanet = node == PlanetName.Rahu ? PlanetName.Ketu : PlanetName.Rahu,
             });

    public static SaturnPhase PhaseOf(int saturnHouseFromMoon) => saturnHouseFromMoon switch
    {
        12 => SaturnPhase.SadeSatiRising,
        1 => SaturnPhase.SadeSatiPeak,
        2 => SaturnPhase.SadeSatiSetting,
        4 => SaturnPhase.Kantaka,
        8 => SaturnPhase.Ashtama,
        _ => SaturnPhase.None,
    };
}
