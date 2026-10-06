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

/// <summary>
/// One of PVR's nine examples. <see cref="Planets"/> are the planets whose dasas and antardasas can bring the
/// <see cref="Result"/>; empty with a null <see cref="NotEvaluated"/> means the chart has no planet meeting the rule.
/// </summary>
public sealed record DashaMatterRule(
    int Number, string Varga, string Statement, string Result,
    IReadOnlyList<DashaMatterPlanet> Planets, string? NotEvaluated, string? Note);

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

    private static DashaMatterRule Rule(IReadOnlyDictionary<string, DashaMatterChart> charts, int number, string varga, string statement,
        string result, Func<DashaMatterChart, IEnumerable<DashaMatterPlanet>> find, string? blocked = null, string? note = null)
    {
        if (blocked is not null) return new(number, varga, statement, result, [], blocked, note);
        if (!charts.TryGetValue(varga, out var chart)) return new(number, varga, statement, result, [], $"Needs the {varga} chart.", note);
        if (Signs(chart.Chart).Count < 9) return new(number, varga, statement, result, [], $"The {varga} chart is incomplete.", note);
        return new(number, varga, statement, result, find(chart).ToList(), null, note);
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
