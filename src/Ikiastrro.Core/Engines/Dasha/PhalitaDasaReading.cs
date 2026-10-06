using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Dasha;

/// <summary>The special points the rasi-dasa interpretations refer to, as the signs they fall in. Any may be null when the
/// chart does not carry it.</summary>
public sealed record DasaReferencePoints(
    ZodiacName? HoraLagna, ZodiacName? GhatiLagna, ZodiacName? ArudhaLagna, ZodiacName? Upapada, ZodiacName? RajyaPada,
    PlanetName? AtmaKaraka);

/// <summary>One thing PVR says to look for in a dasa: the condition, whether it holds in this chart, what PVR says follows
/// when it does, and what was found.</summary>
public sealed record PvrPoint(string Statement, bool Holds, string Result, string Detail);

/// <summary>What PVR reads in one running rasi dasa, from his own interpretation rules. <see cref="Context"/> names the
/// reference points used (dasa lagna, paaka rasi, ...); <see cref="Points"/> are the rules, each marked as holding or not;
/// <see cref="NotEvaluated"/> lists the rules PVR gives that the app cannot yet test; <see cref="Source"/> is the citation.</summary>
public sealed record PhalitaReading(
    string System, string Source, IReadOnlyList<string> Context, IReadOnlyList<PvrPoint> Points, IReadOnlyList<string> NotEvaluated);

/// <summary>
/// PVR's own interpretation rules for the rasi dasas, applied to one running period, <i>Vedic Astrology: An Integrated
/// Approach</i> (<c>SRC_PVR_INTEGRATED</c>): Narayana (sec.18.4, pp.238-240), Lagna Kendradi (sec.19.4 and Example 76,
/// pp.262-263), Sudasa (sec.20.3, p.265) and Drigdasa (sec.21.3, p.269). Each rule is the book's, with its stated result;
/// the engine only tests whether the condition holds in the chart. No score or verdict is added. Rules that depend on
/// something the app does not compute (raja and dhana yogas from the dasa lagna, parivraja yogas, AmK argala) are listed in
/// <see cref="PhalitaReading.NotEvaluated"/> rather than guessed. Pure; no I/O.
/// </summary>
public static class PhalitaDasaReader
{
    // ---------------------------------------------------------------------------------------------
    // Narayana
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// The Narayana reading of <paramref name="dasaRasi"/>. The dasa lagna is the dasa rasi when the dasas start from the
    /// Lagna and the 7th from it when they start from the 7th; the paaka rasi is the rasi holding the dasa lagna's lord.
    /// <paramref name="antardasaRasi"/>, when given, adds the antardasa rules (aspected by GL, aspecting the upapada).
    /// </summary>
    public static PhalitaReading? ReadNarayana(
        ChartAnalysisInput chart, ZodiacName dasaRasi, bool dasasStartFromLagna, DasaReferencePoints refs, ZodiacName? antardasaRasi = null)
    {
        var ctx = Ctx.From(chart);
        if (ctx is null) return null;

        var dasaLagna = dasasStartFromLagna ? dasaRasi : HouseEngine.GetHouseSign(dasaRasi, 7);
        var lordOfDasaLagna = StrongerCoLord.For(dasaLagna, chart);
        var paaka = ctx.Sign[lordOfDasaLagna];
        string From(int h) => Label(HouseEngine.GetHouseSign(dasaLagna, h));
        IReadOnlyList<PlanetName> In(int h) => ctx.Occupants(HouseEngine.GetHouseSign(dasaLagna, h));

        var context = new List<string>
        {
            $"Dasa rasi {Label(dasaRasi)}; dasa lagna {Label(dasaLagna)}{(dasasStartFromLagna ? "" : " (the 7th from it, as the dasas start from the 7th)")}.",
            $"Lord of the dasa lagna: {lordOfDasaLagna}, in {Label(paaka)} (the paaka rasi).",
        };
        var points = new List<PvrPoint>();

        // Natural malefics / benefics in houses from the dasa lagna.
        PvrPoint Natures(string label, int[] houses, bool malefic, string result)
        {
            var found = houses.SelectMany(h => In(h).Where(p => ctx.IsMalefic(p) == malefic).Select(p => $"{p} in the {Ordinal(h)}")).ToList();
            return new(label, found.Count > 0, result, found.Count > 0 ? string.Join(", ", found) : "none");
        }
        points.Add(Natures("Natural malefics in the 3rd and 6th from the dasa lagna", [3, 6], true, "success in ventures"));
        points.Add(Natures("Natural benefics in the 3rd and 6th from the dasa lagna", [3, 6], false, "failures"));
        points.Add(Natures("Natural benefics in the trines (1st, 5th, 9th) and the 8th from the dasa lagna", [1, 5, 9, 8], false, "happiness and success"));
        points.Add(Natures("Natural malefics in the trines (1st, 5th, 9th) and the 8th from the dasa lagna", [1, 5, 9, 8], true, "failures, obstructions and unhappiness"));

        var in11 = In(11);
        points.Add(new("A planet, benefic or malefic, in the 11th from the dasa lagna", in11.Count > 0, "gains are ensured",
            in11.Count > 0 ? string.Join(", ", in11) + " in the 11th" : "none"));

        var rahuHouses = new[] { 8, 12 }.Where(h => In(h).Contains(PlanetName.Rahu)).ToList();
        points.Add(new("Rahu in the 8th or 12th from the dasa lagna", rahuHouses.Count > 0, "constant fear",
            rahuHouses.Count > 0 ? "Rahu in the " + string.Join(" and ", rahuHouses.Select(Ordinal)) : "no"));

        // Lords of the dasa lagna, trines and quadrants.
        string LordLine(int h)
        {
            var lord = StrongerCoLord.For(HouseEngine.GetHouseSign(dasaLagna, h), chart);
            return $"{lord} ({Ordinal(h)}) {ctx.Dignity(lord).ToLowerInvariant()}";
        }
        var strongLords = new[] { 1, 4, 5, 7, 9, 10 }.Where(h => ctx.IsStrong(StrongerCoLord.For(HouseEngine.GetHouseSign(dasaLagna, h), chart))).Select(LordLine).ToList();
        points.Add(new("The lord of the dasa lagna, or of a trine or quadrant from it, exalted or in its own house", strongLords.Count > 0,
            "excellent results", strongLords.Count > 0 ? string.Join("; ", strongLords) : "none"));
        var weakLords = new[] { 1, 4, 5, 7, 9, 10 }.Where(h => ctx.Dignity(StrongerCoLord.For(HouseEngine.GetHouseSign(dasaLagna, h), chart)) == "Debilitated").Select(LordLine).ToList();
        points.Add(new("The lord of the dasa lagna, or of a trine or quadrant from it, debilitated", weakLords.Count > 0,
            "bad results", weakLords.Count > 0 ? string.Join("; ", weakLords) : "none"));
        var dusthanaDebil = new[] { 6, 8, 12 }.Where(h => ctx.Dignity(StrongerCoLord.For(HouseEngine.GetHouseSign(dasaLagna, h), chart)) == "Debilitated").Select(LordLine).ToList();
        points.Add(new("The lord of a dusthana (6th, 8th, 12th) from the dasa lagna debilitated", dusthanaDebil.Count > 0,
            "good results", dusthanaDebil.Count > 0 ? string.Join("; ", dusthanaDebil) : "none"));

        points.Add(Natures("Natural malefics in the 4th from the dasa lagna", [4], true, "discomfort and lack of happiness"));
        points.Add(Natures("Natural benefics in the 4th from the dasa lagna", [4], false, "happiness, well-being and pleasures"));
        points.Add(Natures("Natural benefics in the 2nd and 5th from the dasa lagna", [2, 5], false, "good name, fame and favours from authorities"));
        points.Add(Natures("Natural malefics in the 2nd and 5th from the dasa lagna", [2, 5], true, "bad results in the same areas"));

        var afflicted7 = new List<string>();
        foreach (var (name, sign) in new[] { ("dasa lagna", dasaLagna), ("paaka rasi", paaka) })
        {
            var seventh = HouseEngine.GetHouseSign(sign, 7);
            var bad = ctx.Occupants(seventh).Where(ctx.IsMalefic).ToList();
            if (bad.Count > 0) afflicted7.Add($"{string.Join(", ", bad)} in the 7th from the {name} ({Label(seventh)})");
        }
        points.Add(new("The 7th from the dasa lagna or the paaka rasi afflicted by malefics", afflicted7.Count > 0, "troubles in marriage",
            afflicted7.Count > 0 ? string.Join("; ", afflicted7) : "none"));

        var good = new List<string>();
        foreach (var (name, sign) in new[] { ("dasa lagna", dasaLagna), ("paaka rasi", paaka) })
            foreach (var p in ctx.Occupants(sign).Where(ctx.IsStrong))
                good.Add($"{p} {ctx.Dignity(p).ToLowerInvariant()} in the {name} ({Label(sign)})");
        points.Add(new("The dasa lagna or the paaka rasi associated with an exalted planet or a planet in its own house", good.Count > 0,
            "all-round success and accumulation of wealth in the dasa", good.Count > 0 ? string.Join("; ", good) : "none"));

        // Natal references for the dasa rasi itself.
        if (refs.RajyaPada is { } a10)
            points.Add(new("The dasa rasi contains the raajya pada (A10)", dasaRasi == a10, "success in career", dasaRasi == a10 ? $"A10 is in {Label(a10)}" : $"A10 is in {Label(a10)}"));
        if (refs.Upapada is { } ul)
        {
            points.Add(new("The dasa rasi is the upapada (UL)", dasaRasi == ul, "may bring marriage", $"UL is in {Label(ul)}"));
            var second = HouseEngine.GetHouseSign(ul, 2);
            var seventh = HouseEngine.GetHouseSign(ul, 7);
            points.Add(new("The dasa rasi is the 2nd or the 7th from the upapada", dasaRasi == second || dasaRasi == seventh,
                "may bring troubles in marriage", $"2nd from UL is {Label(second)}, 7th is {Label(seventh)}"));
        }
        if (refs.GhatiLagna is { } gl)
            points.Add(new("The dasa rasi contains Ghati Lagna (GL)", dasaRasi == gl, "may bring power", $"GL is in {Label(gl)}"));

        if (antardasaRasi is { } ad)
        {
            if (refs.GhatiLagna is { } gl2)
                points.Add(new("The antardasa rasi is aspected by GL", RasiDrishtiCalculator.Aspects(gl2, ad), "may bring promotions",
                    $"antardasa {Label(ad)}, GL in {Label(gl2)}"));
            if (refs.Upapada is { } ul2)
                points.Add(new("The antardasa rasi aspects the upapada", RasiDrishtiCalculator.Aspects(ad, ul2), "may bring marriage",
                    $"antardasa {Label(ad)}, UL in {Label(ul2)}"));
            var houseOfAntar = ((int)ad - (int)dasaRasi + 12) % 12 + 1;
            context.Add($"Antardasa rasi {Label(ad)} is the {Ordinal(houseOfAntar)} house from the dasa rasi (PVR also judges an antardasa by this house).");
        }

        return new("Narayana", "PVR §18.4, pp.238-240", context, points, new[]
        {
            "Raja and dhana yogas with respect to the dasa lagna (bring success).",
            "Arudha padas aspected by the antardasa rasi.",
            "PVR's thirds: the rasi dominates the first third of a dasa, its lord the second, and its occupants and aspecters the third.",
        });
    }

    // ---------------------------------------------------------------------------------------------
    // Sudasa
    // ---------------------------------------------------------------------------------------------

    /// <summary>PVR sec.20.3 (p.265): Sudasa is read for money, power and status from HL, GL and AL.</summary>
    public static PhalitaReading? ReadSudasa(ChartAnalysisInput chart, ZodiacName dasaRasi, DasaReferencePoints refs)
    {
        var ctx = Ctx.From(chart);
        if (ctx is null) return null;
        var lord = StrongerCoLord.For(dasaRasi, chart);
        var context = new List<string> { $"Dasa rasi {Label(dasaRasi)}; its lord {lord} is in {Label(ctx.Sign[lord])}." };
        var points = new List<PvrPoint>();

        void Prosperity(string point, ZodiacName? sign, string result)
        {
            if (sign is not { } s) return;
            var lordOfPoint = StrongerCoLord.For(s, chart);
            var is7th = dasaRasi == HouseEngine.GetHouseSign(s, 7);
            points.Add(new($"The dasa rasi is {point} itself", dasaRasi == s, result, $"{point} is in {Label(s)}"));
            points.Add(new($"The dasa rasi is the 7th from {point}", is7th, result, $"7th from {point} is {Label(HouseEngine.GetHouseSign(s, 7))}"));
            points.Add(new($"The dasa rasi aspects {point}", RasiDrishtiCalculator.Aspects(dasaRasi, s), result, $"{Label(dasaRasi)} to {Label(s)}"));
            var lordLinks = new List<string>();
            if (ctx.Sign[lord] == s) lordLinks.Add($"{lord}, lord of the dasa rasi, occupies {point}");
            else if (RasiDrishtiCalculator.Aspects(ctx.Sign[lord], s)) lordLinks.Add($"{lord}, lord of the dasa rasi, aspects {point}");
            points.Add(new($"The lord of the dasa rasi occupies or aspects {point}", lordLinks.Count > 0, result + " (improves the chance)",
                lordLinks.Count > 0 ? string.Join("; ", lordLinks) : $"{lord} is in {Label(ctx.Sign[lord])}"));
            var reverse = new List<string>();
            if (ctx.Sign[lordOfPoint] == dasaRasi) reverse.Add($"{lordOfPoint}, lord of {point}, occupies the dasa rasi");
            else if (RasiDrishtiCalculator.Aspects(ctx.Sign[lordOfPoint], dasaRasi)) reverse.Add($"{lordOfPoint}, lord of {point}, aspects the dasa rasi");
            points.Add(new($"The lord of {point} occupies or aspects the dasa rasi", reverse.Count > 0, result + " (improves the chance)",
                reverse.Count > 0 ? string.Join("; ", reverse) : $"{lordOfPoint} is in {Label(ctx.Sign[lordOfPoint])}"));
        }
        Prosperity("Hora Lagna (HL)", refs.HoraLagna, "financial prosperity");
        Prosperity("Ghati Lagna (GL)", refs.GhatiLagna, "power and authority");

        if (refs.ArudhaLagna is { } al)
        {
            var house = ((int)dasaRasi - (int)al + 12) % 12 + 1;
            var upachaya = house is 3 or 6 or 10 or 11;
            points.Add(new("The dasa rasi is an upachaya (3rd, 6th, 10th, 11th) from the arudha lagna (AL)", upachaya, "growth of status",
                $"{Label(dasaRasi)} is the {Ordinal(house)} from AL ({Label(al)})"));
            points.Add(new("The dasa rasi is the 11th from the arudha lagna", house == 11, "particularly favourable", $"the {Ordinal(house)} from AL"));
            points.Add(new("The dasa rasi is the 8th or 12th from the arudha lagna", house is 8 or 12, "setbacks to status; the dasa can be unfavourable", $"the {Ordinal(house)} from AL"));
        }
        return new("Sudasa", "PVR §20.3, p.265 (and §20.1)", context, points, new[]
        {
            "PVR: check Narayana dasa along with Sudasa for power; Sudasa gives prosperity.",
        });
    }

    // ---------------------------------------------------------------------------------------------
    // Lagna Kendradi Rasi
    // ---------------------------------------------------------------------------------------------

    /// <summary>PVR sec.19.4 and Example 76 (pp.262-263): a phalita dasa for material success; the book's reasons a rasi gave
    /// success. Sudasa is the superior dasa.</summary>
    public static PhalitaReading? ReadLagnaKendradi(ChartAnalysisInput chart, ZodiacName dasaRasi, DasaReferencePoints refs)
    {
        var ctx = Ctx.From(chart);
        if (ctx is null) return null;
        var lagnaLord = StrongerCoLord.For(chart.AscendantSign, chart);
        var context = new List<string> { $"Dasa rasi {Label(dasaRasi)}." };
        var points = new List<PvrPoint>();
        if (refs.AtmaKaraka is { } ak)
            points.Add(new("The dasa rasi contains the Atma Karaka", ctx.Sign[ak] == dasaRasi, "usually gives success", $"{ak} (AK) is in {Label(ctx.Sign[ak])}"));
        if (refs.GhatiLagna is { } gl)
            points.Add(new("The dasa rasi contains Ghati Lagna (GL), the seat of power", dasaRasi == gl, "power and authority", $"GL is in {Label(gl)}"));
        points.Add(new("The dasa rasi contains the Lagna lord", ctx.Sign[lagnaLord] == dasaRasi,
            "connects the Lagna to the dasa rasi (PVR's Example 76 notes this beside GL)", $"{lagnaLord} (Lagna lord) is in {Label(ctx.Sign[lagnaLord])}"));
        return new("Lagna Kendradi Rasi", "PVR §19.4 and Example 76, pp.262-263", context, points, new[]
        {
            "A strong argala of the Amatya Karaka on the rasi (shows capable advisors, and political power).",
            "PVR: this dasa shows material success; use it with Sudasa, which is the superior dasa.",
        });
    }

    // ---------------------------------------------------------------------------------------------
    // Drigdasa
    // ---------------------------------------------------------------------------------------------

    /// <summary>PVR sec.21.3 (p.269): Drigdasa shows spiritual vision and growth.</summary>
    public static PhalitaReading? ReadDrig(ChartAnalysisInput chart, ZodiacName dasaRasi, DasaReferencePoints refs)
    {
        var lagna = chart.AscendantSign;
        var context = new List<string> { $"Dasa rasi {Label(dasaRasi)}." };
        var points = new List<PvrPoint>();
        if (refs.ArudhaLagna is { } al)
        {
            points.Add(new("The dasa rasi is the arudha lagna (AL)", dasaRasi == al, "can bring renunciation if there are parivraja yogas in the chart", $"AL is in {Label(al)}"));
            points.Add(new("The dasa rasi aspects the arudha lagna", RasiDrishtiCalculator.Aspects(dasaRasi, al),
                "external activities important for one's spiritual evolution", $"{Label(dasaRasi)} to AL in {Label(al)}"));
        }
        points.Add(new("The dasa rasi is the Lagna or the 7th house", dasaRasi == lagna || dasaRasi == HouseEngine.GetHouseSign(lagna, 7),
            "internal awakening and self-realization", $"Lagna {Label(lagna)}, 7th {Label(HouseEngine.GetHouseSign(lagna, 7))}"));
        return new("Drigdasa", "PVR §21.1 and §21.3, pp.267-269", context, points, new[]
        {
            "Parivraja (renunciation) yogas, which the AL dasa result depends on.",
        });
    }

    // ---------------------------------------------------------------------------------------------

    private static string Label(ZodiacName s) => s == ZodiacName.Capricornus ? "Capricorn" : s.ToString();
    private static string Ordinal(int n) => n switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{n}th" };

    private sealed class Ctx
    {
        public required Dictionary<PlanetName, ZodiacName> Sign { get; init; }
        private Dictionary<string, ZodiacName> _names = null!;
        private Dictionary<PlanetName, double?> _degree = null!;
        private IReadOnlyDictionary<ZodiacName, IReadOnlyList<PlanetName>> _occupancy = null!;

        public static Ctx? From(ChartAnalysisInput chart)
        {
            var planets = chart.Planets
                .Where(p => p.Planet != "Ascendant" && Enum.TryParse<PlanetName>(p.Planet, out _))
                .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet));
            if (planets.Count != 9) return null;
            var c = new Ctx { Sign = planets.ToDictionary(kv => kv.Key, kv => Enum.Parse<ZodiacName>(kv.Value.Sign)) };
            c._names = c.Sign.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
            c._degree = planets.ToDictionary(kv => kv.Key, kv => kv.Value.VargaLongitudeDegrees is null
                ? (double?)((((kv.Value.NirayanaLongitudeDegrees ?? 0) % 30) + 30) % 30) : null);
            c._occupancy = Enum.GetValues<ZodiacName>().ToDictionary(s => s,
                s => (IReadOnlyList<PlanetName>)c.Sign.Where(kv => kv.Value == s).Select(kv => kv.Key).ToList());
            return c;
        }

        public IReadOnlyList<PlanetName> Occupants(ZodiacName sign) => _occupancy[sign];
        public bool IsMalefic(PlanetName p) => ArgalaCalculator.IsNaturalMalefic(p, _occupancy);
        public string Dignity(PlanetName p) => DignityEngine.Evaluate(p.ToString(), Sign[p], _degree[p], _names).DignityStatus ?? "Neutral";
        public bool IsStrong(PlanetName p) => Dignity(p) is "Exalted" or "Own Sign" or "Moolatrikona";
    }
}
