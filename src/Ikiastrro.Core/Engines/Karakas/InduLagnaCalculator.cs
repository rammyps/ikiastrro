using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// Indu Lagna (IL, "Chandra Kalasha") — B.V. Raman method (tbl_Rule_SpecialLagnaKala, db/150).
/// No dignity modifier: verified against 3+ secondary sources and against the vendored
/// PyJHora reference (_research/PyJHora/src/jhora/panchanga/drik.py:2176 `indu_lagna`,
/// `il_factors`) — there is no exaltation/own-sign/debilitation multiplier in the standard
/// method, contradicting an earlier assumption that one existed.
///
/// Rule: Kala values Sun 30, Moon 16, Mars 6, Mercury 8, Jupiter 10, Venus 12, Saturn 1. Sum the
/// Kala of the lord of the 9th house from the Lagna and the lord of the 9th house from the Moon;
/// reduce mod 12 (a remainder of 0 is treated as 12); count that many signs forward from the
/// Moon's own sign, inclusive (the Moon's sign itself counts as 1).
///
/// Degree-within-sign is an engineering choice classical texts don't make (they define only the
/// resulting Rasi) — resolved by direct evidence, not assumed: PyJHora's own indu_lagna() returns
/// (indu_rasi, moon_long), i.e. it reuses the Moon's own degree-within-sign rather than 0°, and
/// docs/artifacts/reference-charts/Rammy_Jagannatha.txt confirms it exactly — Moon 7 Sc 17'33.70",
/// Indu Lagna 7 Sg 17'33.70" (identical minutes/seconds, sign shifted by the Kala-count offset).
/// See InduLagnaCalculatorTests for the hand-worked confirmation against that chart.
/// </summary>
public static class InduLagnaCalculator
{
    private static readonly IReadOnlyDictionary<string, double> KalaValues = new Dictionary<string, double>
    {
        ["Sun"] = 30, ["Moon"] = 16, ["Mars"] = 6, ["Mercury"] = 8,
        ["Jupiter"] = 10, ["Venus"] = 12, ["Saturn"] = 1
    };

    public static SpecialPointSeed Compute(double lagnaLongitude, double moonLongitude)
    {
        var lagnaSign = AstroMath.GetSignAtLongitude(lagnaLongitude);
        var moonSign = AstroMath.GetSignAtLongitude(moonLongitude);

        var ninthFromLagnaLord = HouseEngine.GetSignLord(HouseEngine.GetHouseSign(lagnaSign, 9));
        var ninthFromMoonLord = HouseEngine.GetSignLord(HouseEngine.GetHouseSign(moonSign, 9));

        var kalaSum = KalaValues[ninthFromLagnaLord] + KalaValues[ninthFromMoonLord];
        var reduced = kalaSum % 12;
        if (reduced == 0) reduced = 12;

        var signsForward = (int)reduced - 1; // inclusive count: the Moon's own sign is position 1
        var longitude = AstroMath.Normalize(moonLongitude + signsForward * 30.0);
        return new SpecialPointSeed("IL", "SpecialLagna", longitude);
    }
}
