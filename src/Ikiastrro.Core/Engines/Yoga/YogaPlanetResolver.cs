using System.Text.RegularExpressions;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Yoga;

/// <summary>A graha's house from the Lagna, from the Sun and from the Moon (1-12).</summary>
public sealed record YogaPlacement(int FromLagna, int FromSun, int FromMoon);

/// <summary>
/// The planets that make a yoga in one chart, for the Yoga tab's "Planets" column. The engines only
/// record Present/Absent, so this reads the yoga's own one-line rule (<c>tbl_Rule_Yoga.ShortFormationRule</c>):
/// planets the rule names, plus every house lord it names ("Lagna lord", "5th &amp; 6th lords") resolved to
/// this chart's planet. "From the Sun / Moon" yogas (Vesi, Sunapha, Adhi ...) instead list the planets
/// that actually stand in the counted houses. Where a rule offers alternatives the list is every planet
/// the rule mentions, not only the branch that fired.
/// </summary>
public static class YogaPlanetResolver
{
    private static readonly PlanetName[] Classical =
    {
        PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury,
        PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn,
    };

    private const string HouseToken = @"(?:Lagna|\d{1,2}(?:st|nd|rd|th))";
    private const string HouseList = HouseToken + @"(?:\s*(?:,|&|/|and|or)\s*" + HouseToken + ")*";

    private static readonly Regex PlanetWord = new(@"\b(Sun|Moon|Mars|Mercury|Jupiter|Venus|Saturn|Rahu|Ketu)\b", RegexOptions.Compiled);
    private static readonly Regex LordsBefore = new(@"\b(" + HouseList + @")\s+lords?\b", RegexOptions.Compiled);
    private static readonly Regex LordsOf = new(@"\blords? of (" + HouseList + ")", RegexOptions.Compiled);
    private static readonly Regex HouseWord = new(HouseToken, RegexOptions.Compiled);
    private static readonly Regex Exclusion = new(@"\((?:[^)]*\b(?:not|excluded)\b[^)]*)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex AllSeven = new(@"\b(?:all\s+)?7 classical grahas\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>Yogas read off the planets standing in houses counted from the Sun or Moon.</summary>
    private sealed record Occupancy(bool FromMoon, int[] Houses, PlanetName[] Candidates, bool IncludeReference);

    private static readonly PlanetName[] NotMoon = Classical.Where(p => p != PlanetName.Moon).ToArray();
    private static readonly PlanetName[] NotSun = Classical.Where(p => p != PlanetName.Sun).ToArray();

    private static readonly Dictionary<string, Occupancy> Occupancies = new()
    {
        ["YOGA_VESI"] = new(false, new[] { 2 }, NotMoon, false),
        ["YOGA_VASI"] = new(false, new[] { 12 }, NotMoon, false),
        ["YOGA_UBHAYACHARI"] = new(false, new[] { 2, 12 }, NotMoon, false),
        ["YOGA_SUNAPHA"] = new(true, new[] { 2 }, NotSun, false),
        ["YOGA_ANAPHA"] = new(true, new[] { 12 }, NotSun, false),
        ["YOGA_DURADHARA"] = new(true, new[] { 2, 12 }, NotSun, false),
        ["YOGA_ADHI"] = new(true, new[] { 6, 7, 8 }, new[] { PlanetName.Jupiter, PlanetName.Mercury, PlanetName.Venus }, true),
        ["YOGA_SAKATA_LUNAR"] = new(true, new[] { 6, 8, 12 }, new[] { PlanetName.Jupiter }, true),
        ["YOGA_GAJAKESARI"] = new(true, new[] { 1, 4, 7, 10 }, new[] { PlanetName.Jupiter }, true),
    };

    public static IReadOnlyList<PlanetName> Resolve(
        string yogaCode, string? rule,
        IReadOnlyDictionary<int, PlanetName> houseLords,
        IReadOnlyDictionary<PlanetName, YogaPlacement> placements)
    {
        if (Occupancies.TryGetValue(yogaCode, out var occ))
        {
            var found = occ.Candidates.Where(p => placements.TryGetValue(p, out var at)
                && occ.Houses.Contains(occ.FromMoon ? at.FromMoon : at.FromSun)).ToList();
            if (occ.IncludeReference) found.Add(occ.FromMoon ? PlanetName.Moon : PlanetName.Sun);
            return Ordered(found);
        }
        if (string.IsNullOrWhiteSpace(rule)) return Array.Empty<PlanetName>();

        var text = Exclusion.Replace(rule, "");
        var planets = new HashSet<PlanetName>();
        if (AllSeven.IsMatch(text)) planets.UnionWith(Classical);
        foreach (Match m in PlanetWord.Matches(text)) planets.Add(Enum.Parse<PlanetName>(m.Value));
        foreach (var pattern in new[] { LordsBefore, LordsOf })
            foreach (Match m in pattern.Matches(text))
                foreach (Match h in HouseWord.Matches(m.Groups[1].Value))
                {
                    var house = h.Value == "Lagna" ? 1 : int.Parse(h.Value[..^2]);
                    if (houseLords.TryGetValue(house, out var lord)) planets.Add(lord);
                }
        return Ordered(planets);
    }

    private static IReadOnlyList<PlanetName> Ordered(IEnumerable<PlanetName> planets)
        => planets.Distinct().OrderBy(p => (int)p).ToList();
}
