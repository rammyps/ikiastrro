using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Transits;

/// <summary>How a transit reads by PVR ch.26.3's table: an auspicious house that no planet
/// blocks (Good), an auspicious house blocked by Vedha (Blocked), a house the table does not list
/// as auspicious (NotFavourable), or a planet the table does not cover — Rahu/Ketu (NotTabled).</summary>
public enum GocharaVerdict { Good, Blocked, NotFavourable, NotTabled }

/// <summary>Saturn counted from the natal Moon: Sāḍe Sātī (12th, 1st, 2nd — its three
/// Dhaiyas), Kaṇṭaka (4th) and Aṣṭama (8th) Śani. The same offsets as
/// tvf_Chart_SadeSatiPeriods (docs/cli/calculations.md §9).</summary>
public enum SaturnPhase { None, SadeSatiRising, SadeSatiPeak, SadeSatiSetting, Kantaka, Ashtama }

/// <param name="Bindus">The planet's own Bhinnāṣṭakavarga bindus in the sign it is crossing
/// (natal D1); null for Rahu/Ketu, which have no BAV.</param>
/// <param name="Vedha">Null for Rahu/Ketu.</param>
public sealed record GocharaRow(
    PlanetName Planet,
    ZodiacName Sign,
    int HouseFromMoon,
    GocharaVerdict Verdict,
    GocharaVedhaResult? Vedha,
    int? Bindus);

public sealed record GocharaReadingResult(ZodiacName NatalMoonSign, IReadOnlyList<GocharaRow> Rows, SaturnPhase Saturn);

/// <summary>
/// Every transiting planet read from the natal Moon (PVR ch.26): its house from the Moon, whether
/// that house is auspicious for it and whether another transiting planet blocks it by Vedha
/// (<see cref="GocharaVedhaCalculator"/>, tbl_Rule_GocharaVedha), and its own BAV bindus in that
/// sign. The bindus are reported beside the verdict, not folded into it. Pure.
/// </summary>
public static class GocharaReading
{
    public static GocharaReadingResult Read(
        ZodiacName natalMoonSign,
        IReadOnlyDictionary<PlanetName, ZodiacName> transitSigns,
        IReadOnlyList<GocharaVedhaRule> rules,
        Func<PlanetName, ZodiacName, int?>? bavBindus = null)
    {
        var rows = PlanetNames.All9
            .Where(transitSigns.ContainsKey)
            .Select(planet =>
            {
                var sign = transitSigns[planet];
                var house = AstroMath.CountFromSignToSign(natalMoonSign, sign);
                if (planet is PlanetName.Rahu or PlanetName.Ketu)
                    return new GocharaRow(planet, sign, house, GocharaVerdict.NotTabled, null, null);

                var vedha = GocharaVedhaCalculator.Evaluate(planet, natalMoonSign, transitSigns, rules);
                var verdict = !vedha.IsAuspicious ? GocharaVerdict.NotFavourable
                    : vedha.IsObstructed ? GocharaVerdict.Blocked
                    : GocharaVerdict.Good;
                return new GocharaRow(planet, sign, house, verdict, vedha, bavBindus?.Invoke(planet, sign));
            })
            .ToList();

        var saturn = rows.FirstOrDefault(r => r.Planet == PlanetName.Saturn);
        return new GocharaReadingResult(natalMoonSign, rows, saturn is null ? SaturnPhase.None : PhaseOf(saturn.HouseFromMoon));
    }

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
