using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>What the life-matter similarity reads for one person: the stored sign charts by varga code
/// ("D1", "D3", "D7", "D9", "D10"), the Moon's nakshatra attributes and the Moon's pada.</summary>
public sealed record LifeMatterChart(
    IReadOnlyDictionary<string, DoshaChart> Vargas, MatchPerson Moon, int MoonPada);

/// <summary>One life area and the chart facts read for it: the varga, the houses (from that varga's Lagna)
/// whose sign and lord are read, the karaka grahas, and whether the Moon's nakshatra facts are included.
/// The fact selection is a working choice, not a cited rule.</summary>
public sealed record SimilarityArea(
    string Code, string Name, string Varga, IReadOnlyList<int> Houses, IReadOnlyList<PlanetName> Karakas, bool MoonNature);

/// <summary>One fact both people share, with the weight it contributed.</summary>
public sealed record SharedAreaFact(string Key, string Value, double Weight);

/// <summary>One area's result for a pair. <see cref="Score"/> is the shared weight over the total weight
/// of the probe's facts in the area (0 to 1); null when either chart lacks the area's varga.</summary>
public sealed record AreaSimilarity(
    SimilarityArea Area, double? Score, double SharedWeight, double TotalWeight, IReadOnlyList<SharedAreaFact> Shared);

public enum SimilarityLean { First, Second, Tie, Unknown }

/// <summary>
/// Family similarity per life area. Each person is flattened into small facts per area (the sign and lord
/// placement of the area's houses, the karakas' sign and house, and for the Moon areas the Moon's sign,
/// nakshatra, pada, Gana, Yoni and Nadi). A fact both people share adds its weight, <c>-ln(chance)</c> for the
/// chance that two random charts agree on it, scaled by <see cref="SlowPlanetFactor"/> for Jupiter, Saturn
/// and the nodes (their signs follow the birth years, not the family). The score is shared weight over the
/// probe's total weight. A graha's house from the Lagna is compared as well as its sign (so "7th lord in the
/// 1st" matches across different Lagnas); when the two Lagnas are the same the house fact is dropped, as the
/// sign already decides it. A shared nakshatra already implies the Moon's sign, Gana, Yoni and Nadi, so those
/// are left out when it matches. Chances are flat (1/12 per sign or house) until measured base rates exist
/// (docs/architecture/compatibility_similarity.md section 5.3). Facts only: how alike two charts are is not
/// presented as a reading of character or inheritance. Pure; no I/O.
/// </summary>
public static class LifeMatterSimilarity
{
    public const double SlowPlanetFactor = 0.25;
    /// <summary>Two scores closer than this are a tie.</summary>
    public const double LeanBand = 0.15;

    public static readonly IReadOnlyList<SimilarityArea> Areas =
    [
        new("self", "Self", "D1", [1], [PlanetName.Sun], false),
        new("wealth", "Wealth", "D1", [2, 11], [PlanetName.Jupiter], false),
        new("siblings", "Siblings", "D3", [3], [PlanetName.Mars], false),
        new("mother", "Mother and home", "D1", [4], [PlanetName.Moon], true),
        new("children", "Children", "D7", [5], [PlanetName.Jupiter], false),
        new("health", "Health", "D1", [6], [PlanetName.Mars], false),
        new("marriage", "Marriage", "D9", [7], [PlanetName.Venus], false),
        new("longevity", "Longevity", "D1", [8], [PlanetName.Saturn], false),
        new("father", "Father and dharma", "D1", [9], [PlanetName.Sun], false),
        new("career", "Career", "D10", [10], [PlanetName.Saturn, PlanetName.Sun], false),
        new("loss", "Loss and moksha", "D1", [12], [PlanetName.Ketu], false),
        new("moon", "Moon and mind", "D1", [], [], true),
    ];

    private sealed record Fact(string Key, string Value, double Weight);

    /// <summary>Each area's similarity between the probe and one relative, in <see cref="Areas"/> order.</summary>
    public static IReadOnlyList<AreaSimilarity> Compare(LifeMatterChart probe, LifeMatterChart relative) =>
        Areas.Select(a => CompareArea(a, probe, relative)).ToList();

    /// <summary>Which of two relatives' results is nearer to the probe in one area; a gap under
    /// <see cref="LeanBand"/> is a tie, and a missing score is Unknown.</summary>
    public static SimilarityLean Lean(AreaSimilarity first, AreaSimilarity second)
    {
        if (first.Score is not { } f || second.Score is not { } s) return SimilarityLean.Unknown;
        var gap = f - s;
        return gap > LeanBand ? SimilarityLean.First : gap < -LeanBand ? SimilarityLean.Second : SimilarityLean.Tie;
    }

    private static AreaSimilarity CompareArea(SimilarityArea area, LifeMatterChart probe, LifeMatterChart relative)
    {
        if (!probe.Vargas.TryGetValue(area.Varga, out var pv) || !relative.Vargas.TryGetValue(area.Varga, out var rv))
            return new(area, null, 0, 0, []);

        var a = Facts(area, pv, probe);
        var b = Facts(area, rv, relative).ToDictionary(f => f.Key);
        var sameNakshatra = probe.Moon.NakshatraNumber == relative.Moon.NakshatraNumber;
        var sameLagna = pv.Lagna == rv.Lagna;

        double total = 0, shared = 0;
        var hits = new List<SharedAreaFact>();
        foreach (var f in a)
        {
            if (sameNakshatra && f.Key is "Moon sign" or "Gana" or "Yoni" or "Nadi") continue;
            if (sameLagna && f.Key.EndsWith(" house")) continue;   // same Lagna: a shared sign already gives the same house
            total += f.Weight;
            if (b.TryGetValue(f.Key, out var other) && other.Value == f.Value)
            {
                shared += f.Weight;
                hits.Add(new(f.Key, f.Value, f.Weight));
            }
        }
        return new(area, total == 0 ? null : shared / total, shared, total, hits);
    }

    private static List<Fact> Facts(SimilarityArea area, DoshaChart chart, LifeMatterChart person)
    {
        var facts = new List<Fact>();
        foreach (var h in area.Houses)
        {
            var sign = HouseEngine.GetHouseSign(chart.Lagna, h);
            var lord = Enum.Parse<PlanetName>(HouseEngine.GetSignLord(sign));
            facts.Add(new($"{Ordinal(h)} sign", Label(sign), Weight(1.0 / 12, 1)));
            facts.Add(new($"{Ordinal(h)} lord in", Label(chart.Signs[lord]), Weight(1.0 / 12, Slow(lord))));
            facts.Add(new($"{Ordinal(h)} lord house", HouseFrom(chart.Lagna, chart.Signs[lord]).ToString(), Weight(1.0 / 12, Slow(lord))));
        }
        foreach (var k in area.Karakas)
        {
            var sign = chart.Signs[k];
            if (!(k == PlanetName.Moon && area.MoonNature))   // the Moon-nature facts already carry the Moon's sign
                facts.Add(new($"{k} sign", Label(sign), Weight(1.0 / 12, Slow(k))));
            facts.Add(new($"{k} house", HouseFrom(chart.Lagna, sign).ToString(), Weight(1.0 / 12, Slow(k))));
        }
        if (area.MoonNature)
        {
            var m = person.Moon;
            facts.Add(new("Moon sign", Label(m.MoonSign), Weight(1.0 / 12, 1)));
            facts.Add(new("Nakshatra", m.NakshatraNumber.ToString(), Weight(1.0 / 27, 1)));
            facts.Add(new("Pada", $"{m.NakshatraNumber}-{person.MoonPada}", Weight(1.0 / 108, 1)));
            facts.Add(new("Gana", m.Gana, Weight(1.0 / 3, 1)));
            facts.Add(new("Yoni", m.YoniAnimal, Weight(1.0 / 14, 1)));
            facts.Add(new("Nadi", m.Nadi, Weight(1.0 / 3, 1)));
        }
        return facts;
    }

    private static double Weight(double chance, double factor) => -Math.Log(chance) * factor;

    private static double Slow(PlanetName p) =>
        p is PlanetName.Jupiter or PlanetName.Saturn or PlanetName.Rahu or PlanetName.Ketu ? SlowPlanetFactor : 1;

    private static int HouseFrom(ZodiacName lagna, ZodiacName sign) => ((int)sign - (int)lagna + 12) % 12 + 1;

    private static string Label(ZodiacName s) => SignLabels.For(s);

    private static string Ordinal(int n) => n switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{n}th" };
}
