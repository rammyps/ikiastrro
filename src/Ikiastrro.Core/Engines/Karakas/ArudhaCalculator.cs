using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Pipeline;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Engines.Karakas;

/// <summary>
/// The Arudha pada of each of the 12 houses (Parashara). For house H with lord L:
/// let n = signs counted from H's sign to L's sign (inclusive, 1..12); the pada is n signs
/// on from L's sign (inclusive). If the pada lands on H's own sign or the 7th from it, take
/// the 10th sign from the pada. The pada is placed at the chart's Lagna degree-in-sign so it
/// has a longitude. A1 is emitted under the code "AL".
///
/// Computed inside each chart from that chart's own placements (D1 and every varga alike —
/// sec.9.2: "in all the divisional charts"; VargaChartComputer calls this per varga rather than
/// projecting the D1 padas, since 2026-10-01). For Scorpio and Aquarius, L is the stronger
/// co-lord (<see cref="StrongerCoLord"/>, sec.15.5.1: Mars vs Ketu, Saturn vs Rahu).
///
/// SRC_PVR_INTEGRATED sec.9.2 "Computation of Bhava Arudhas" (verified against the raw book
/// extract). Mirrors <c>tbl_Rule_ArudhaFormula</c> (migration 085) — cited there but not read
/// from there; CLI <c>verify-jaimini</c> asserts that row exists and cites the right source.
/// </summary>
public static class ArudhaCalculator
{
    /// <param name="chart">D1 or any divisional chart: PVR sec.9.2 defines the padas of "all the
    /// 12 houses in all the divisional charts" from that chart's own placements. The returned
    /// longitudes are in the chart's own zodiac (its Lagna's degree-in-sign).</param>
    public static IReadOnlyList<SpecialPointSeed> Compute(ChartAnalysisInput chart)
    {
        var lagnaSign = chart.AscendantSign;
        var asc = chart.Planets.First(p => p.Planet == "Ascendant");
        var lagnaDegInSign = (asc.VargaLongitudeDegrees ?? asc.NirayanaLongitudeDegrees ?? 0) % 30.0;

        var planetSign = chart.Planets
            .Where(p => p.Planet != "Ascendant")
            .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet), p => Enum.Parse<ZodiacName>(p.Sign));

        int SignIndex(ZodiacName z) => (int)z;
        ZodiacName Add(ZodiacName z, int n) => (ZodiacName)(((SignIndex(z) + n) % 12 + 12) % 12);

        var seeds = new List<SpecialPointSeed>();
        for (var house = 1; house <= 12; house++)
        {
            var houseSign = Add(lagnaSign, house - 1);
            var lord = StrongerCoLord.For(houseSign, chart);
            var pada = PadaOf(houseSign, planetSign[lord]);
            var longitude = SignIndex(pada) * 30.0 + lagnaDegInSign;
            var code = house == 1 ? "AL" : $"A{house}";
            seeds.Add(new SpecialPointSeed(code, "Arudha", AstroMath.Normalize(longitude)));
        }
        return seeds;
    }

    /// <summary>The Arudha pada of the house in <paramref name="houseSign"/> whose lord sits in
    /// <paramref name="lordSign"/> — the same rule as <see cref="Compute"/>, for any sign counted
    /// from any lagna (Key Inference reads the pada of a house counted from the Moon, AL, …).</summary>
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
