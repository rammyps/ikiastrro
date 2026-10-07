using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Dasha;

/// <summary>A divisional chart with the special points the dasha-matter rules refer to (AL, A3, HL, GL ...) by name.</summary>
public sealed record DashaMatterChart(ChartAnalysisInput Chart, IReadOnlyDictionary<string, ZodiacName> Points);

/// <summary>A planet that meets a rule, and the placement that makes it meet it.</summary>
public sealed record DashaMatterPlanet(PlanetName Planet, string Why);

public enum DashaMatterScopeKind { Example, ChartTheme, House, NaturalKaraka }

/// <summary>Machine-readable scope used to join a rule to a LifeMatter without matching prose.</summary>
public sealed record DashaMatterScope(DashaMatterScopeKind Kind, int? House = null, PlanetName? Karaka = null);

/// <summary>
/// One of PVR's nine examples. <see cref="Planets"/> are the planets whose dasas and antardasas can bring the
/// <see cref="Result"/>; empty with a null <see cref="NotEvaluated"/> means the chart has no planet meeting the rule.
/// </summary>
public sealed record DashaMatterRule(
    int Number, string Varga, string Statement, string Result,
    IReadOnlyList<DashaMatterPlanet> Planets, string? NotEvaluated, string? Note, string Source = "",
    DashaMatterScope? Scope = null);

/// <summary>
/// P.V.R. Narasimha Rao, <i>Vedic Astrology: An Integrated Approach</i> sec.16.5.1, printed p.214
/// (<c>SRC_PVR_INTEGRATED</c>): "A planet gives the results promised by its positions in various divisional
/// charts", with nine examples. Each example is a rule here; for a chart, the planets meeting it are the ones whose
/// dasas can bring the stated result. The book calls these "just a few examples", gives no scale of strength and
/// no verdict on good or bad, so none is produced. Readings the book leaves open are worked as follows and said in
/// the row's note: "exalted" is by sign and degree as the repo's DignityEngine reads it; "12th from AK in D-9" counts
/// from the sign the Atma Karaka (taken from the rasi chart) holds in D-9; "aspect" is rasi drishti (sign aspect);
/// co-lords of Scorpio and Aquarius resolve to the stronger one. Pure; no I/O.
/// </summary>
public static class DashaMatters
{
    private static readonly string[] WellDisposed = ["Exalted", "Own Sign", "Moolatrikona", "Great Friend", "Friend"];

    public static IReadOnlyList<DashaMatterRule> Evaluate(IReadOnlyDictionary<string, DashaMatterChart> charts, PlanetName? atmaKaraka)
    {
        var rules = new List<DashaMatterRule>
        {
            Rule(charts, 1, "D7", "The 5th lord in D-7", "can give a child", c => LordOf(c, 5)),
            Rule(charts, 2, "D1", "The 8th lord in the rasi chart", "can give some troubles and frustration", c => LordOf(c, 8)),
            Rule(charts, 3, "D10", "A planet exalted in GL in D-10", "can give power and authority in career", ExaltedIn("GL")),
            Rule(charts, 4, "D9", "An exalted planet in the 12th from AK in D-9", "can give serious thoughts related to spiritual liberation",
                c => ExaltedIn12FromAk(c, atmaKaraka, charts), atmaKaraka is null ? "Needs the Atma Karaka." : null,
                "The 12th is counted from the sign the Atma Karaka (from the rasi chart) holds in D-9."),
            Rule(charts, 5, "D9", "The 7th lord in D-9", "can give marriage", c => LordOf(c, 7)),
            Rule(charts, 6, "D4", "A planet with Rahu in the 9th in D-4", "can give foreign residence", WithRahuIn9th),
            Rule(charts, 7, "D1", "An exalted planet aspecting HL from the 11th from AL in the rasi chart", "can give a lot of wealth", ExaltedAspectingHl,
                null, "Aspect is by sign (rasi drishti)."),
            Rule(charts, 8, "D30", "A planet joined by Moon and Saturn in the 8th house in D-30",
                "may give serious psychological problems and suicidal tendencies", JoinedByMoonAndSaturnIn8th),
            Rule(charts, 9, "D10", "A well-disposed planet aspecting A3 in D-10", "may make one write some books", WellDisposedAspecting("A3"), null,
                "PVR does not define well-disposed; here it means exalted, in its own sign or Moolatrikona, or in a friend's or great friend's sign. Aspect is by sign (rasi drishti)."),
        };
        return rules;
    }

    /// <summary>One extended matter: the lord of <paramref name="House"/> in <paramref name="Varga"/>, plus any planets
    /// sitting in that house, or a fixed natural karaka when <paramref name="Karaka"/> is set (House is then 0).</summary>
    private sealed record Spec(string Varga, int House, string Matter, PlanetName? Karaka = null);

    /// <summary>The varga each house or karaka is read in follows the usual Parashari divisional-chart themes
    /// (D2 wealth, D3 siblings, D4 property, D7 children, D9 marriage and dharma, D10 career, D12 parents, D16 vehicles,
    /// D20 spiritual practice, D24 learning). Extension of the section 16.5.1 principle, not PVR's own examples.</summary>
    private static readonly Spec[] Extended =
    [
        new("D1", 2, "wealth and family"), new("D1", 5, "children and intelligence"), new("D1", 7, "marriage and partnership"),
        new("D1", 9, "fortune and dharma"), new("D1", 10, "career and status"), new("D1", 11, "gains"), new("D1", 12, "losses and foreign matters"),
        new("D2", 2, "stored wealth"), new("D2", 11, "income and gains"),
        new("D3", 3, "siblings and courage"), new("D3", 0, "siblings", PlanetName.Mars),
        new("D4", 4, "property and home"), new("D4", 0, "property", PlanetName.Mars),
        new("D7", 0, "children", PlanetName.Jupiter), new("D7", 9, "fortune through children"),
        new("D9", 9, "fortune and dharma"),
        new("D10", 10, "career and status"), new("D10", 6, "service and daily work"), new("D10", 0, "authority", PlanetName.Sun),
        new("D12", 4, "the mother"), new("D12", 9, "the father"), new("D12", 0, "the father", PlanetName.Sun), new("D12", 0, "the mother", PlanetName.Moon),
        new("D16", 4, "vehicles and comforts"), new("D16", 0, "vehicles", PlanetName.Venus),
        new("D20", 5, "spiritual practice"), new("D20", 9, "spiritual practice and guidance"),
        new("D24", 4, "formal education"), new("D24", 5, "learning and intellect"), new("D24", 0, "learning", PlanetName.Mercury),
    ];

    /// <summary>PVR Table 11: the sphere of life each divisional chart shows. Each chart's own lagna lord and the planets
    /// in its lagna stand for that sphere, so their dasas can bring it.</summary>
    private static readonly (string Varga, string Theme)[] Table11 =
    [
        ("D1", "physical existence"), ("D2", "wealth"), ("D3", "siblings"), ("D4", "property and fortune"), ("D5", "fame and power"),
        ("D6", "health and troubles"), ("D7", "children"), ("D8", "sudden troubles"), ("D9", "marriage and dharma"), ("D10", "career"),
        ("D11", "death and destruction"), ("D12", "parents"), ("D16", "vehicles and comforts"), ("D20", "religion and spirituality"),
        ("D24", "education"), ("D27", "innate nature"), ("D30", "evils and punishment"), ("D40", "auspicious events"),
        ("D45", "all matters"), ("D60", "past-life karma"),
    ];

    private const string ConventionSource = "Parashari house and karaka significations, applied per PVR 16.5.1";
    private static string PvrSource(int number) => number < 10 ? $"PVR §16.5.1 example {number} (p.214)" : "";

    /// <summary>Further dasa-matters beyond PVR's nine: first every divisional chart's Table 11 sphere (its lagna lord and
    /// occupants), then the lords and occupants of the houses that govern each theme and the natural karakas. Numbered
    /// from 10 so they never collide with PVR's examples. Facts only, no verdict on good or bad.</summary>
    public static IReadOnlyList<DashaMatterRule> EvaluateExtended(IReadOnlyDictionary<string, DashaMatterChart> charts)
    {
        const string? note = null;   // the Source column already says where each row comes from
        var rules = new List<DashaMatterRule>();
        var n = 10;
        foreach (var (varga, theme) in Table11)
            rules.Add(Rule(charts, n++, varga, "The lagna lord, and planets in the lagna", $"can bring matters of {theme}",
                c => LordAndOccupants(c, 1), null, note, $"PVR Table 11 ({varga}: {theme})",
                new(DashaMatterScopeKind.ChartTheme)));
        foreach (var spec in Extended)
        {
            var statement = spec.Karaka is { } k
                ? $"{k}, natural karaka of {spec.Matter}"
                : $"The {Ordinal(spec.House)} lord, and planets in the {Ordinal(spec.House)}";
            var result = $"can bring matters of {spec.Matter}";
            rules.Add(Rule(charts, n++, spec.Varga, statement, result,
                c => spec.Karaka is { } kk ? KarakaIn(c, kk, spec.Varga) : LordAndOccupants(c, spec.House), null, note, ConventionSource,
                spec.Karaka is { } karaka
                    ? new(DashaMatterScopeKind.NaturalKaraka, Karaka: karaka)
                    : new(DashaMatterScopeKind.House, House: spec.House)));
        }
        return rules;
    }

    private static IEnumerable<DashaMatterPlanet> KarakaIn(DashaMatterChart c, PlanetName karaka, string varga)
    {
        var signs = Signs(c.Chart);
        yield return new(karaka, $"Natural karaka, in {Label(signs[karaka])} in {varga}");
    }

    private static IEnumerable<DashaMatterPlanet> LordAndOccupants(DashaMatterChart c, int house)
    {
        var sign = HouseEngine.GetHouseSign(c.Chart.AscendantSign, house);
        var lord = StrongerCoLord.For(sign, c.Chart);
        var signs = Signs(c.Chart);
        yield return new(lord, $"Lord of the {Ordinal(house)} ({Label(sign)}), in {Label(signs[lord])}");
        foreach (var kv in signs.Where(kv => kv.Value == sign && kv.Key != lord))
            yield return new(kv.Key, $"In the {Ordinal(house)} ({Label(sign)})");
    }

    private static DashaMatterRule Rule(IReadOnlyDictionary<string, DashaMatterChart> charts, int number, string varga, string statement,
        string result, Func<DashaMatterChart, IEnumerable<DashaMatterPlanet>> find, string? blocked = null, string? note = null, string? source = null,
        DashaMatterScope? scope = null)
    {
        if (blocked is not null) return new(number, varga, statement, result, [], blocked, note, source ?? PvrSource(number), scope);
        if (!charts.TryGetValue(varga, out var chart)) return new(number, varga, statement, result, [], $"Needs the {varga} chart.", note, source ?? PvrSource(number), scope);
        if (Signs(chart.Chart).Count < 9) return new(number, varga, statement, result, [], $"The {varga} chart is incomplete.", note, source ?? PvrSource(number), scope);
        return new(number, varga, statement, result, find(chart).ToList(), null, note, source ?? PvrSource(number), scope);
    }

    private static Dictionary<PlanetName, ZodiacName> Signs(ChartAnalysisInput chart) =>
        chart.Planets
            .Where(p => p.Planet != "Ascendant" && Enum.TryParse<PlanetName>(p.Planet, out _))
            .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet), p => Enum.Parse<ZodiacName>(p.Sign));

    private static IEnumerable<DashaMatterPlanet> LordOf(DashaMatterChart c, int house)
    {
        var sign = HouseEngine.GetHouseSign(c.Chart.AscendantSign, house);
        var lord = StrongerCoLord.For(sign, c.Chart);
        yield return new(lord, $"Lord of the {Ordinal(house)} ({Label(sign)})");
    }

    private static bool IsExalted(DashaMatterChart c, PlanetName planet, IReadOnlyDictionary<PlanetName, ZodiacName> signs) =>
        DignityEngine.Evaluate(planet.ToString(), signs[planet], DegreeInSign(c.Chart, planet), signs.ToDictionary(k => k.Key.ToString(), k => k.Value))
            .DignityStatus == "Exalted";

    private static double? DegreeInSign(ChartAnalysisInput chart, PlanetName planet)
    {
        var p = chart.Planets.First(x => x.Planet == planet.ToString());
        var longitude = p.VargaLongitudeDegrees ?? p.NirayanaLongitudeDegrees;
        return longitude is null ? null : ((longitude.Value % 30) + 30) % 30;
    }

    private static Func<DashaMatterChart, IEnumerable<DashaMatterPlanet>> ExaltedIn(string point) => c =>
    {
        if (!c.Points.TryGetValue(point, out var sign)) return [];
        var signs = Signs(c.Chart);
        return signs.Where(kv => kv.Value == sign && IsExalted(c, kv.Key, signs))
            .Select(kv => new DashaMatterPlanet(kv.Key, $"Exalted in {point} ({Label(sign)})"));
    };

    private static IEnumerable<DashaMatterPlanet> ExaltedIn12FromAk(DashaMatterChart c, PlanetName? ak, IReadOnlyDictionary<string, DashaMatterChart> charts)
    {
        var signs = Signs(c.Chart);
        var twelfth = (ZodiacName)(((int)signs[ak!.Value] + 11) % 12);
        return signs.Where(kv => kv.Value == twelfth && IsExalted(c, kv.Key, signs))
            .Select(kv => new DashaMatterPlanet(kv.Key, $"Exalted in {Label(twelfth)}, the 12th from AK {ak} ({Label(signs[ak.Value])})"));
    }

    private static IEnumerable<DashaMatterPlanet> WithRahuIn9th(DashaMatterChart c)
    {
        var signs = Signs(c.Chart);
        var ninth = HouseEngine.GetHouseSign(c.Chart.AscendantSign, 9);
        if (signs[PlanetName.Rahu] != ninth) return [];
        return signs.Where(kv => kv.Key != PlanetName.Rahu && kv.Value == ninth)
            .Select(kv => new DashaMatterPlanet(kv.Key, $"With Rahu in the 9th ({Label(ninth)})"));
    }

    private static IEnumerable<DashaMatterPlanet> ExaltedAspectingHl(DashaMatterChart c)
    {
        if (!c.Points.TryGetValue("AL", out var al) || !c.Points.TryGetValue("HL", out var hl)) return [];
        var signs = Signs(c.Chart);
        var eleventh = HouseEngine.GetHouseSign(al, 11);
        return signs.Where(kv => kv.Value == eleventh && IsExalted(c, kv.Key, signs) && RasiDrishtiCalculator.Aspects(kv.Value, hl))
            .Select(kv => new DashaMatterPlanet(kv.Key, $"Exalted in {Label(eleventh)}, the 11th from AL ({Label(al)}), aspecting HL ({Label(hl)})"));
    }

    private static IEnumerable<DashaMatterPlanet> JoinedByMoonAndSaturnIn8th(DashaMatterChart c)
    {
        var signs = Signs(c.Chart);
        var eighth = HouseEngine.GetHouseSign(c.Chart.AscendantSign, 8);
        if (signs[PlanetName.Moon] != eighth || signs[PlanetName.Saturn] != eighth) return [];
        return signs.Where(kv => kv.Value == eighth && kv.Key is not (PlanetName.Moon or PlanetName.Saturn))
            .Select(kv => new DashaMatterPlanet(kv.Key, $"In the 8th ({Label(eighth)}) with Moon and Saturn"));
    }

    private static Func<DashaMatterChart, IEnumerable<DashaMatterPlanet>> WellDisposedAspecting(string point) => c =>
    {
        if (!c.Points.TryGetValue(point, out var target)) return [];
        var signs = Signs(c.Chart);
        var byName = signs.ToDictionary(k => k.Key.ToString(), k => k.Value);
        return signs.Where(kv => RasiDrishtiCalculator.Aspects(kv.Value, target))
            .Select(kv => (Planet: kv.Key, Status: DignityEngine.Evaluate(kv.Key.ToString(), kv.Value, DegreeInSign(c.Chart, kv.Key), byName).DignityStatus))
            .Where(x => x.Status is not null && WellDisposed.Contains(x.Status))
            .Select(x => new DashaMatterPlanet(x.Planet, $"{x.Status} in {Label(signs[x.Planet])}, aspecting {point} ({Label(target)})"));
    };

    private static string Label(ZodiacName s) => s == ZodiacName.Capricornus ? "Capricorn" : s.ToString();
    private static string Ordinal(int n) => n switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{n}th" };
}
