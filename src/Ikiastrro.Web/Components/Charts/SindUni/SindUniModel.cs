using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Relationships;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Presentation;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.Workspace;

namespace Ikiastrro.Web.Components.Charts.SindUni;

public enum SindUniView { Compact, Reading, Micro }

public enum SindUniLens { Off, Aspects, Argala }

/// <summary>One graha in a sign. <see cref="Label"/> is the chip text: "JU" direct, "(JU)" retrograde,
/// and always "(RA)" / "(KE)".</summary>
public sealed record SindUniPlanet(
    string Planet, string Code, string Label, string DignityToken, string? DignityStatus,
    bool IsRetrograde, bool IsCombust, string? Degree, int? NakshatraId, int? Pada, string? Karaka);

/// <summary>Planets acting on a sign by Argala (or obstructing it, Virodhargala) from
/// <see cref="Offset"/> signs away.</summary>
public sealed record SindUniArgalaSource(int Offset, bool IsArgala, IReadOnlyList<string> Planets, bool CountedAntiZodiacally = false)
{
    /// <summary>The sign the planets sit in, counted from the target (backwards when the Argala
    /// is counted anti-zodiacally, PVR 10.6).</summary>
    public ZodiacName SourceSign(ZodiacName target) =>
        CountedAntiZodiacally ? (ZodiacName)(((int)target - (Offset - 1) + 24) % 12) : SindUniChart.Nth(target, Offset);
}

public sealed record SindUniSign(
    ZodiacName Sign,
    int HouseFromLagna,
    IReadOnlyList<SindUniPlanet> Planets,
    string? Verdict,
    string? VerdictWhy,
    int? Sav,
    StrengthTier SavTier,
    IReadOnlyList<string> Arudhas,
    IReadOnlyList<string> SpecialLagnas,
    string? ArgalaNet,
    IReadOnlyList<SindUniArgalaSource> ArgalaSources,
    IReadOnlyList<string> Aspectors);

/// <summary>Everything a SIND-UNI chart draws, built once per chart from data the pages already load.</summary>
public sealed record SindUniChart(
    string ChartType,
    ZodiacName Ascendant,
    string? AscendantDegree,
    int? AscendantNakshatraId,
    int? AscendantPada,
    IReadOnlyDictionary<ZodiacName, SindUniSign> Signs,
    IReadOnlyDictionary<string, ZodiacName> PlanetSigns)
{
    public SindUniSign this[ZodiacName sign] => Signs[sign];

    /// <summary>Whole-sign house of <paramref name="sign"/> counted from <paramref name="from"/>.</summary>
    public static int HouseFrom(ZodiacName from, ZodiacName sign) => ((int)sign - (int)from + 12) % 12 + 1;

    public static ZodiacName Nth(ZodiacName from, int n) => (ZodiacName)(((int)from + n - 1) % 12);
}

/// <summary>
/// Builds a <see cref="SindUniChart"/> from a <see cref="LoadedChart"/> plus the optional per-chart
/// facts the page has: Sarvashtakavarga bindus and Argala rows. The benefic / malefic verdict is
/// <see cref="HouseBeneficMaleficCalculator"/> (B.V. Raman) — the same one Astro Facts → About Houses
/// shows; sign-level aspects use <see cref="RelationshipEngine.AspectsSign"/>, as that calculator does.
/// </summary>
public static class SindUniBuilder
{
    private static readonly int[] ArgalaOffsets = [2, 4, 11, 5];

    public static SindUniChart Build(
        LoadedChart chart,
        IReadOnlyDictionary<ZodiacName, int>? savBySign = null,
        IReadOnlyList<ArgalaFactRow>? argalaFacts = null)
    {
        var asc = Enum.Parse<ZodiacName>(chart.AscendantSign);
        var grahas = chart.Grahas.Where(k => k.Planet != "Ascendant" && Enum.TryParse<PlanetName>(k.Planet, out _)).ToList();
        var ascRow = chart.Grahas.FirstOrDefault(k => k.Planet == "Ascendant");
        var planetSigns = grahas.ToDictionary(k => k.Planet, k => Enum.Parse<ZodiacName>(k.Sign));

        var verdicts = HouseBeneficMaleficCalculator.ComputeAll(asc, chart.KeyDetails).ToDictionary(v => v.Sign);
        var facts = argalaFacts ?? ArgalaFacts.Live(chart.ChartType, chart.AscendantSign, chart.Grahas);
        var factsByHouse = facts.Where(f => f.TargetKind == "House").ToLookup(f => (int)f.TargetHouseNumber);
        var dignityByPlanet = grahas.ToDictionary(k => Enum.Parse<PlanetName>(k.Planet), k => k.DignityStatus);

        var signs = new Dictionary<ZodiacName, SindUniSign>();
        foreach (var sign in Enum.GetValues<ZodiacName>())
        {
            var house = SindUniChart.HouseFrom(asc, sign);
            var planets = grahas
                .Where(k => k.Sign == sign.ToString())
                .OrderBy(k => SindUniGlyphs.PlanetOrder.ToList().IndexOf(k.Planet))
                .Select(ToPlanet)
                .ToList();

            var verdict = verdicts.GetValueOrDefault(sign);
            var signFacts = factsByHouse[house].ToList();
            var sources = signFacts
                .GroupBy(f => (f.RelationTypeCode, f.HouseOffset, f.CountedAntiZodiacally))
                .Select(g => new SindUniArgalaSource(g.Key.HouseOffset, g.Key.RelationTypeCode == "ARGALA",
                    g.Select(f => f.OccupantPlanet).OrderBy(p => SindUniGlyphs.PlanetOrder.ToList().IndexOf(p)).ToList(),
                    g.Key.CountedAntiZodiacally))
                .OrderBy(s => s.IsArgala ? 0 : 1).ThenBy(s => Array.IndexOf(ArgalaOffsets, s.Offset) is var i && i < 0 ? 9 : i)
                .ToList();
            string? net = null;
            if (signFacts.Count > 0)
            {
                var compare = ArgalaCalculator.Compare(
                    signFacts.Where(f => f.RelationTypeCode == "ARGALA").Select(f => Enum.Parse<PlanetName>(f.OccupantPlanet)).ToList(),
                    signFacts.Where(f => f.RelationTypeCode == "VIRODHARGALA").Select(f => Enum.Parse<PlanetName>(f.OccupantPlanet)).ToList(),
                    p => ChartViewModel.DignityScore(dignityByPlanet.GetValueOrDefault(p)));
                var label = compare.Dominant switch
                {
                    ArgalaCalculator.RelationType.Argala => "Argala",
                    ArgalaCalculator.RelationType.Virodhargala => "Virodhargala",
                    _ => "Tie",
                };
                net = $"{label} {compare.ArgalaCount}–{compare.VirodhargalaCount}";
            }

            var aspectors = grahas
                .Where(k => RelationshipEngine.AspectsSign(k.Planet, Enum.Parse<ZodiacName>(k.Sign), sign))
                .Select(k => k.Planet)
                .OrderBy(p => SindUniGlyphs.PlanetOrder.ToList().IndexOf(p))
                .ToList();

            int? sav = savBySign is not null && savBySign.TryGetValue(sign, out var b) ? b : null;

            signs[sign] = new SindUniSign(
                sign, house, planets,
                verdict is null ? null : VerdictCode(verdict.Verdict),
                verdict is null ? null : WhyText(verdict),
                sav, StrengthBands.SarvaAshtakavargaBindus.Classify(sav),
                chart.KeyDetails.Where(k => k.PointKind == "Arudha" && k.Sign == sign.ToString()).Select(k => k.Planet).ToList(),
                SpecialLagnaCodes(chart, sign, asc, planetSigns),
                net, sources, aspectors);
        }

        return new SindUniChart(
            chart.ChartType, asc, ShortDegree(ascRow?.DegreesInSignDisplay),
            ascRow?.NakshatraId, ascRow?.NakshatraPada, signs, planetSigns);

        SindUniPlanet ToPlanet(ChartKeyDetail k)
        {
            var code = SindUniGlyphs.PlanetCode(k.Planet);
            var retro = k.Planet is "Rahu" or "Ketu" || k.IsRetrograde == true;
            return new SindUniPlanet(
                k.Planet, code, retro ? $"({code})" : code,
                SindUniGlyphs.DignityToken(k.DignityStatus), k.DignityStatus,
                retro, k.IsCombust == true, ShortDegree(k.DegreesInSignDisplay),
                k.NakshatraId, k.NakshatraPada, k.CharaKaraka);
        }
    }

    private static string VerdictCode(HouseBeneficMaleficVerdict v) => v switch
    {
        HouseBeneficMaleficVerdict.Benefic => "BEN",
        HouseBeneficMaleficVerdict.Malefic => "MAL",
        HouseBeneficMaleficVerdict.Mixed => "MIX",
        _ => null!,
    };

    private static string WhyText(HouseBeneficMaleficResult r)
    {
        static string List(IReadOnlyList<string> inSign, IReadOnlyList<string> aspecting)
        {
            var all = inSign.Select(p => $"{p} (in)").Concat(aspecting.Select(p => $"{p} (asp)")).ToList();
            return all.Count == 0 ? "—" : string.Join(", ", all);
        }
        return $"Lord {r.LordPlanet} · {r.LordFunctionalNature}; benefics {List(r.BeneficOccupants, r.BeneficAspectors)}; " +
               $"malefics {List(r.MaleficOccupants, r.MaleficAspectors)}";
    }

    /// <summary>"8°11'2\"" → "8°11′".</summary>
    public static string? ShortDegree(string? display)
    {
        if (string.IsNullOrWhiteSpace(display)) return null;
        var parts = display.Split('°', '\'', '"');
        return parts.Length >= 2 ? $"{parts[0]}°{parts[1].PadLeft(2, '0')}′" : display;
    }

    /// <summary>Codes of the special lagnas that fall in <paramref name="sign"/>, matching
    /// <see cref="SindUniGlyphs.SpecialLagnas"/>.</summary>
    private static IReadOnlyList<string> SpecialLagnaCodes(LoadedChart chart, ZodiacName sign, ZodiacName asc,
        IReadOnlyDictionary<string, ZodiacName> planetSigns)
    {
        var codes = new List<string>();
        foreach (var kind in SindUniGlyphs.SpecialLagnas)
        {
            var here = kind.Code switch
            {
                "ASC" => asc == sign,
                "MOON" => planetSigns.TryGetValue("Moon", out var m) && m == sign,
                "SUN" => planetSigns.TryGetValue("Sun", out var s) && s == sign,
                "AL" => chart.KeyDetails.Any(k => k.PointKind == "Arudha" && k.Planet == "AL" && k.Sign == sign.ToString()),
                "GK" => chart.KeyDetails.Any(k => k.PointKind == "Upagraha" && k.Planet is "Gulika" or "Maandi" && k.Sign == sign.ToString()),
                _ => chart.KeyDetails.Any(k => k.PointKind == "SpecialLagna" && k.Planet == kind.Code && k.Sign == sign.ToString()),
            };
            if (here) codes.Add(kind.Code);
        }
        return codes;
    }

    /// <summary>Sign → Sarvashtakavarga bindus for one chart type, from the repository rows.</summary>
    public static IReadOnlyDictionary<ZodiacName, int> SavFrom(IEnumerable<AshtakavargaRow> rows, string chartType) =>
        rows.Where(r => r.ChartType == chartType && r.SarvaBindus is not null)
            .GroupBy(r => r.SignNumber)
            .ToDictionary(g => (ZodiacName)(g.Key - 1), g => (int)g.Max(r => r.SarvaBindus!.Value));
}
