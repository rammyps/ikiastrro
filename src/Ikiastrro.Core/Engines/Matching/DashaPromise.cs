using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.Matching;

public enum DashaLevel { Maha, Antar, Pratyantar }

/// <summary>The lords running for one person: Mahadasha, Antardasha and Pratyantardasha.</summary>
public sealed record RunningDasha(PlanetName Maha, PlanetName Antar, PlanetName Pratyantar);

/// <summary>
/// One running lord against one life area, in the area's varga. <see cref="Reference"/> is the point the level
/// is judged from (Sun for the Mahadasha, Moon for the Antardasha, Lagna for the Pratyantar); the house numbers
/// <see cref="HouseFromReference"/> and <see cref="RulesFromReference"/> count from it. <see cref="LinksFromLagna"/>
/// and <see cref="LinksFromReference"/> say whether the lord rules or occupies the area's focus houses, counted
/// from the varga Lagna and from the reference, or is a karaka of the area; empty means no link. For the
/// Pratyantar the reference is the Lagna, so the second list is left empty (it would repeat the first).
/// </summary>
public sealed record DashaAreaReading(
    SimilarityArea Area, DashaLevel Level, PlanetName Lord, string Reference,
    int HouseFromReference, IReadOnlyList<int> RulesFromReference,
    IReadOnlyList<string> LinksFromLagna, IReadOnlyList<string> LinksFromReference)
{
    public bool IsLinked => LinksFromLagna.Count > 0 || LinksFromReference.Count > 0;
}

/// <summary>
/// What each running dasha lord signifies for each life area, from P.V.R. Narasimha Rao, <i>Vedic Astrology:
/// An Integrated Approach</i> (<c>SRC_PVR_INTEGRATED</c>) Ch. 16.5: a planet gives the results promised by its
/// positions in the divisional charts (16.5.1, e.g. the 5th lord in D7 for children, the 7th lord in D9 for
/// marriage), and under the "tripod of life" the Mahadasha is judged from the Sun, the Antardasha from the Moon
/// and the Pratyantar from the Lagna (16.5.3). Facts only: the book says a placement "can give" a result, and
/// gives no scale for how strongly, so no good or bad label is produced. The area list is the one used by
/// <see cref="LifeMatterSimilarity"/>, whose house and karaka choice is a working one, not a cited rule. Areas
/// with no house to read (Moon and mind) and areas whose varga is not supplied are left out. Pure; no I/O.
/// </summary>
public static class DashaPromiseReader
{
    public static string ReferenceName(DashaLevel level) =>
        level switch { DashaLevel.Maha => "Sun", DashaLevel.Antar => "Moon", _ => "Lagna" };

    public static IReadOnlyList<DashaAreaReading> Read(IReadOnlyDictionary<string, DoshaChart> vargas, RunningDasha running)
    {
        var result = new List<DashaAreaReading>();
        foreach (var area in LifeMatterSimilarity.Areas.Where(a => a.Houses.Count > 0))
        {
            if (!vargas.TryGetValue(area.Varga, out var chart)) continue;
            foreach (var (level, lord) in new[]
                     { (DashaLevel.Maha, running.Maha), (DashaLevel.Antar, running.Antar), (DashaLevel.Pratyantar, running.Pratyantar) })
                result.Add(ReadOne(area, chart, level, lord));
        }
        return result;
    }

    private static DashaAreaReading ReadOne(SimilarityArea area, DoshaChart chart, DashaLevel level, PlanetName lord)
    {
        var refSign = level switch
        {
            DashaLevel.Maha => chart.Signs[PlanetName.Sun],
            DashaLevel.Antar => chart.Signs[PlanetName.Moon],
            _ => chart.Lagna,
        };
        var lordSign = chart.Signs[lord];
        var ruled = Enum.GetValues<ZodiacName>().Where(s => HouseEngine.GetSignLord(s) == lord.ToString()).ToList();

        var fromLagna = Links(area, ruled, lordSign, chart.Lagna, "");
        if (area.Karakas.Contains(lord)) fromLagna.Add("karaka");

        var fromRef = level == DashaLevel.Pratyantar
            ? []
            : Links(area, ruled, lordSign, refSign, $" from {ReferenceName(level)}");

        return new(area, level, lord, ReferenceName(level),
            HouseFrom(refSign, lordSign), ruled.Select(s => HouseFrom(refSign, s)).Order().ToList(), fromLagna, fromRef);
    }

    // Focus houses are numbered from the base sign; the lord links when it owns or sits in one of them.
    private static List<string> Links(SimilarityArea area, List<ZodiacName> ruled, ZodiacName lordSign,
        ZodiacName baseSign, string suffix)
    {
        var links = new List<string>();
        foreach (var h in area.Houses)
        {
            var sign = HouseEngine.GetHouseSign(baseSign, h);
            if (ruled.Contains(sign)) links.Add($"rules {Ordinal(h)}{suffix}");
            if (lordSign == sign) links.Add($"in {Ordinal(h)}{suffix}");
        }
        return links;
    }

    private static int HouseFrom(ZodiacName from, ZodiacName sign) => ((int)sign - (int)from + 12) % 12 + 1;
    public static string Ordinal(int n) => n switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{n}th" };
}

/// <summary>Planets (Sun to Saturn) in the quadrants (1st, 4th, 7th, 10th) from a reference point, in whole signs.</summary>
public sealed record KendraOccupancy(string From, int PlanetsInQuadrants, int QuadrantsOccupied)
{
    public bool AllQuadrantsOccupied => QuadrantsOccupied == 4;
}

/// <summary>
/// The caution PVR gives against Vimshottari (Ch. 16.7, repeated in 19.1): when the quadrants from the stronger
/// of Lagna and Moon are occupied by planets (16.7: all four; 19.1: four planets in them), Kendradi Graha Dasa
/// suits better. The book gives no rule for which of Lagna and Moon is stronger ("no clear guidelines"), so both
/// are reported and none is chosen. The seven planets Sun to Saturn count; the Moon counts when read from the
/// Lagna and from itself (whole-sign occupancy). Nodes are not counted: the source speaks of planets.
/// </summary>
public static class KendradiGrahaDasaCheck
{
    private static readonly PlanetName[] Planets =
        [PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn];

    public static (KendraOccupancy FromLagna, KendraOccupancy FromMoon) Evaluate(DoshaChart d1) =>
        (From("Lagna", d1.Lagna, d1), From("Moon", d1.Signs[PlanetName.Moon], d1));

    private static KendraOccupancy From(string name, ZodiacName baseSign, DoshaChart d1)
    {
        var quadrants = new[] { 1, 4, 7, 10 }.Select(h => HouseEngine.GetHouseSign(baseSign, h)).ToList();
        var inQuadrants = Planets.Count(p => quadrants.Contains(d1.Signs[p]));
        var occupied = quadrants.Count(q => Planets.Any(p => d1.Signs[p] == q));
        return new(name, inQuadrants, occupied);
    }
}
