using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>PVR's three longevity categories: short 0-36 years, middle 36-72, long 72-108.</summary>
public enum LongevityCategory { Short, Middle, Long }

/// <summary>One of the three pairs: the two points compared, the modality of the sign each occupies, and the
/// category Table 33 gives for that combination.</summary>
public sealed record LongevityPair(
    string Name, string FirstLabel, ZodiacName FirstSign, string SecondLabel, ZodiacName SecondSign, LongevityCategory Category);

/// <summary>The Method of Three Pairs. <see cref="Category"/> is the combined category;
/// <see cref="Paramaayush"/> is Table 34's maximum longevity in years when two pairs agree (null when all three
/// differ, as the table has no cell for that case); <see cref="Why"/> says how the three results were combined.</summary>
public sealed record ThreePairsReading(
    IReadOnlyList<LongevityPair> Pairs, LongevityCategory Category, int? Paramaayush, string Why);

/// <summary>The Eighth Lord Method: the stronger of the Lagna and the 7th is the reference; the category follows
/// where the 8th lord from it sits, counted from it.</summary>
public sealed record EighthLordReading(
    ZodiacName Reference, string ReferenceWhy, ZodiacName EighthSign, PlanetName EighthLord,
    ZodiacName LordSign, int HouseFromReference, LongevityCategory Category);

/// <summary>
/// The two longevity-category methods of P.V.R. Narasimha Rao, <i>Vedic Astrology: An Integrated Approach</i>,
/// sec.14.4 and 14.5, printed pp.183-185 (<c>SRC_PVR_INTEGRATED</c>, checked against the raw extract and the book's
/// Example 47 and Exercise 23).
/// <list type="bullet">
/// <item><b>Three Pairs</b>: (1) the Lagna lord and the 8th lord (the 8th from PVR's Table 32), (2) the Moon and
/// Saturn, (3) the Lagna and the Hora Lagna. Each pair is read from the modality of the signs the two occupy
/// (Table 33: Fixed+Dual or Movable+Movable is long, Movable+Fixed or Dual+Dual middle, Movable+Dual or Fixed+Fixed
/// short). All three agreeing gives that category; two against one, the two; all three different, pair 3, unless
/// the Moon is in the Lagna or the 7th, when pair 2. Table 34 gives the paramaayush (maximum longevity) from the
/// category the two pairs agree on and the category of the third pair.</item>
/// <item><b>Eighth Lord</b>: from the stronger of the Lagna and the 7th, the 8th lord in a quadrant is long life,
/// in a panaphara middle, in an apoklima short. "Stronger" is the sec.15.5.2 stronger-rasi cascade
/// (<see cref="StrongerRasiComparator"/>); the book gives no rule for it in this chapter.</item>
/// </list>
/// The book calls both methods not infallible and says not to be biased by them or to scare anyone with them; they
/// are shown as categories, not as a prediction. Pure; no I/O.
/// </summary>
public static class LongevityMethods
{
    // PVR Table 34 "Paramaayush Reckoner": [third pair][two pairs] in years, categories Short, Middle, Long.
    private static readonly int[,] Paramaayush =
    {
        { 32, 64, 96 },     // third pair short
        { 36, 72, 108 },    // third pair middle
        { 40, 80, 120 },    // third pair long
    };

    /// <summary>Table 33: the category for two signs' modalities, in either order.</summary>
    public static LongevityCategory PairCategory(ZodiacName a, ZodiacName b)
    {
        var (x, y) = (BaadhakaCalculator.GetModality(a), BaadhakaCalculator.GetModality(b));
        bool Is(SignModality m, SignModality n) => (x == m && y == n) || (x == n && y == m);
        if (Is(SignModality.Fixed, SignModality.Dual) || (x == SignModality.Movable && y == SignModality.Movable))
            return LongevityCategory.Long;
        if (Is(SignModality.Movable, SignModality.Fixed) || (x == SignModality.Dual && y == SignModality.Dual))
            return LongevityCategory.Middle;
        return LongevityCategory.Short;   // Movable+Dual or Fixed+Fixed
    }

    public static ThreePairsReading? ReadThreePairs(ChartAnalysisInput chart, ZodiacName horaLagna)
    {
        var signs = SignsOf(chart);
        if (signs is null) return null;

        var lagna = chart.AscendantSign;
        var lagnaLord = StrongerCoLord.For(lagna, chart);
        var eighthSign = RudraMaheswara.RudraEighth(lagna);
        var eighthLord = StrongerCoLord.For(eighthSign, chart);

        var pairs = new List<LongevityPair>
        {
            Pair("Lagna lord and 8th lord", $"Lagna lord {lagnaLord}", signs[lagnaLord], $"8th lord {eighthLord}", signs[eighthLord]),
            Pair("Moon and Saturn", "Moon", signs[PlanetName.Moon], "Saturn", signs[PlanetName.Saturn]),
            Pair("Lagna and Hora Lagna", "Lagna", lagna, "Hora Lagna", horaLagna),
        };

        var counts = pairs.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.Count());
        LongevityCategory category;
        int? years = null;
        string why;
        if (counts.Count == 1)
        {
            category = pairs[0].Category;
            years = Paramaayush[(int)category, (int)category];
            why = "All three pairs give the same category.";
        }
        else if (counts.Count == 2)
        {
            category = counts.Single(kv => kv.Value == 2).Key;
            var third = pairs.Single(p => p.Category != category).Category;
            years = Paramaayush[(int)third, (int)category];
            why = $"Two pairs give {Name(category).ToLowerInvariant()} life and the third gives {Name(third).ToLowerInvariant()}, so the two dominate.";
        }
        else
        {
            var moonSign = signs[PlanetName.Moon];
            var moonInLagnaOr7th = moonSign == lagna || moonSign == HouseEngine.GetHouseSign(lagna, 7);
            var chosen = moonInLagnaOr7th ? pairs[1] : pairs[2];
            category = chosen.Category;
            why = moonInLagnaOr7th
                ? "All three differ and the Moon is in the Lagna or the 7th, so Moon and Saturn is preferred."
                : "All three differ, so Lagna and Hora Lagna is preferred.";
        }
        return new(pairs, category, years, why);
    }

    public static EighthLordReading? ReadEighthLord(ChartAnalysisInput chart)
    {
        var signs = SignsOf(chart);
        if (signs is null) return null;

        var lagna = chart.AscendantSign;
        var seventh = HouseEngine.GetHouseSign(lagna, 7);
        var reference = StrongerRasiComparator.Compare(lagna, seventh, chart);
        var eighthSign = HouseEngine.GetHouseSign(reference, 8);
        var lord = StrongerCoLord.For(eighthSign, chart);
        var house = ((int)signs[lord] - (int)reference + 12) % 12 + 1;
        var category = house switch
        {
            1 or 4 or 7 or 10 => LongevityCategory.Long,
            2 or 5 or 8 or 11 => LongevityCategory.Middle,
            _ => LongevityCategory.Short,
        };
        var which = reference == lagna ? "Lagna" : "7th house";
        return new(reference, $"The {which} ({Label(reference)}) is stronger than the {(reference == lagna ? "7th" : "Lagna")} ({Label(reference == lagna ? seventh : lagna)}).",
            eighthSign, lord, signs[lord], house, category);
    }

    private static LongevityPair Pair(string name, string first, ZodiacName firstSign, string second, ZodiacName secondSign) =>
        new(name, first, firstSign, second, secondSign, PairCategory(firstSign, secondSign));

    private static string Name(LongevityCategory c) => c.ToString();
    private static string Label(ZodiacName s) => s == ZodiacName.Capricornus ? "Capricorn" : s.ToString();

    private static Dictionary<PlanetName, ZodiacName>? SignsOf(ChartAnalysisInput chart)
    {
        var signs = chart.Planets
            .Where(p => p.Planet != "Ascendant" && Enum.TryParse<PlanetName>(p.Planet, out _))
            .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet), p => Enum.Parse<ZodiacName>(p.Sign));
        return signs.Count == 9 ? signs : null;
    }
}
