using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Pipeline;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// The Arudha pada of each of the 12 houses (Parashara). For house H with whole-sign lord L:
/// let n = signs counted from H's sign to L's sign (inclusive, 1..12); the pada is n signs
/// on from L's sign (inclusive). If the pada lands on H's own sign or the 7th from it, take
/// the 10th sign from the pada. The pada is placed at the natal Lagna's degree-in-sign so it
/// has a longitude for varga projection. A1 is emitted under the code "AL".
///
/// SRC_PVR_INTEGRATED sec.9.2 "Computation of Bhava Arudhas" (verified against the raw book
/// extract). Mirrors <c>tbl_Rule_ArudhaFormula</c> (migration 085) — cited there but not read
/// from there; CLI <c>verify-jaimini</c> asserts that row exists and cites the right source.
/// PVR's Aquarius/Scorpio "take the stronger co-lord" rule (Saturn-vs-Rahu, Mars-vs-Ketu) is
/// not modeled — <see cref="Houses.HouseEngine.GetSignLord"/> always returns the classical sole
/// lord (Saturn, Mars); the strength-comparison exception is a known, documented simplification.
/// </summary>
public static class ArudhaCalculator
{
    public static IReadOnlyList<SpecialPointSeed> Compute(ChartAnalysisInput d1)
    {
        var lagnaSign = d1.AscendantSign;
        var asc = d1.Planets.First(p => p.Planet == "Ascendant");
        var lagnaDegInSign = (asc.NirayanaLongitudeDegrees ?? 0) % 30.0;

        var planetSign = d1.Planets
            .Where(p => p.Planet != "Ascendant")
            .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet), p => Enum.Parse<ZodiacName>(p.Sign));

        int SignIndex(ZodiacName z) => (int)z;
        ZodiacName Add(ZodiacName z, int n) => (ZodiacName)(((SignIndex(z) + n) % 12 + 12) % 12);

        var seeds = new List<SpecialPointSeed>();
        for (var house = 1; house <= 12; house++)
        {
            var houseSign = Add(lagnaSign, house - 1);
            var lord = Enum.Parse<PlanetName>(HouseEngine.GetSignLord(houseSign));
            var pada = PadaOf(houseSign, planetSign[lord]);
            var longitude = SignIndex(pada) * 30.0 + lagnaDegInSign;
            var code = house == 1 ? "AL" : $"A{house}";
            seeds.Add(new SpecialPointSeed(code, "Arudha", AstroMath.Normalize(longitude)));
        }
        return seeds;
    }

    /// <summary>The Arudha pada of the house in <paramref name="houseSign"/> whose lord sits in
    /// <paramref name="lordSign"/> — the same rule as <see cref="Compute"/>, for any sign counted
    /// from any lagna (Life Matters reads the pada of a house counted from the Moon, AL, …).</summary>
    public static ZodiacName PadaOf(ZodiacName houseSign, ZodiacName lordSign)
    {
        static ZodiacName Add(ZodiacName z, int n) => (ZodiacName)((((int)z + n) % 12 + 12) % 12);
        static int CountInclusive(ZodiacName from, ZodiacName to) => (((int)to - (int)from) % 12 + 12) % 12 + 1;

        var pada = Add(lordSign, CountInclusive(houseSign, lordSign) - 1);
        // exception: pada == house's own sign (1st) or the 7th from it -> the 10th sign from the pada
        var fromHouse = CountInclusive(houseSign, pada);
        return fromHouse is 1 or 7 ? Add(pada, 9) : pada;
    }
}
