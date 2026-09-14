using Ikiastrro.Core.Models;

namespace Ikiastrro.Core.Presentation;

/// <summary>One planet's row for the visual chart display — everything a UI needs to render it.</summary>
public record PlanetRow(
    string Planet,
    string Sign,
    string DegreesInSignDisplay,
    string? Nakshatra,
    int? NakshatraPada,
    string? NakshatraLordPlanet,
    bool? IsRetrograde,
    bool? IsCombust,
    decimal? DistanceFromSunDegrees,
    decimal? CombustionOrbUsedDegrees,
    int HouseFromLagna,
    int HouseFromMoon,
    string? DignityStatus,
    string? RulesHouseNumbers,
    string? AspectsCast,
    string? AspectedBy,
    string? CharaKaraka,
    string? NakshatraSubLordPlanet,
    double? SpeedLongitudeDegPerDay,
    double? EclipticLatitudeDegrees,
    double VargaLongitudeDegrees,
    string PointKind);

/// <summary>One graha's row for Key Inference step 2.2 "About Planets" — dignity, Chara Kāraka,
/// its classical Uchcha Bindu (deep-exaltation point), and how close its D1 longitude sits to
/// that exact point. <see cref="ExaltationPointDisplay"/>/<see cref="DeltaFromExaltationDegrees"/>/
/// <see cref="ClosenessPercent"/> are null for Rahu/Ketu — no classical exaltation point for the
/// nodes (docs/ui/components/key-inference.md § New fields — sourcing status).</summary>
public record ExaltationRow(
    string Planet,
    string? DignityStatus,
    string? CharaKaraka,
    string? ExaltationPointDisplay,
    double? DeltaFromExaltationDegrees,
    int? ClosenessPercent);

/// <summary>
/// Builds display-ready chart data from the raw rows already stored per-BirthDetail (KeyDetails,
/// HouseLords, Conjunctions) — the same rows <c>vw_Chart_Consolidated</c> derives its RulesHouseNumbers
/// and Aspects columns from. Doing it here in C# instead of re-querying the SQL view keeps the Blazor UI
/// working from the same repository methods already used elsewhere, with no separate view dependency.
/// KeyDetails/HouseLords/Aspects are the shared, chart-type-generic rows (ChartKeyDetail etc.) — callers
/// pass the chart-type-scoped subset (already filtered by ChartResultId).
/// </summary>
public static class ChartViewModel
{
    /// <summary>
    /// Classical Uchcha Bindu (deep-exaltation degree) per BPHS/PVR, as an absolute nirayana degree
    /// (Aries 0° = 0). The 7 classical grahas only — Rahu/Ketu have no classical exaltation point.
    /// Not yet a DB column (key-inference.md's round-2 spec: a real build adds a small
    /// <c>tbl_Rule_Exaltation</c> instead of hard-coding these 7 constants here).
    /// </summary>
    private static readonly IReadOnlyDictionary<string, double> ExaltationAbsoluteDegrees =
        new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["Sun"] = 10,      // Aries 10°
            ["Moon"] = 33,     // Taurus 3°
            ["Mars"] = 298,    // Capricorn 28°
            ["Mercury"] = 165, // Virgo 15°
            ["Jupiter"] = 95,  // Cancer 5°
            ["Venus"] = 357,   // Pisces 27°
            ["Saturn"] = 200,  // Libra 20°
        };

    private static readonly string[] ZodiacSignsFromAries =
        { "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio",
          "Sagittarius", "Capricorn", "Aquarius", "Pisces" };

    /// <summary>"10° Aries" style label for a planet's classical exaltation point — null if the
    /// planet has none (Rahu/Ketu, or an unrecognised name).</summary>
    public static string? ExaltationPointDisplay(string planet)
    {
        if (!ExaltationAbsoluteDegrees.TryGetValue(planet, out var absolute)) return null;
        var sign = ZodiacSignsFromAries[(int)(absolute / 30)];
        var degree = absolute % 30;
        return $"{degree:0}° {sign}";
    }

    /// <summary>Shortest angular distance between two absolute degrees, 0–180°.</summary>
    private static double AngularDistance(double a, double b)
    {
        var d = Math.Abs(a - b) % 360;
        return d > 180 ? 360 - d : d;
    }

    /// <summary>
    /// Builds one row per graha (Sun…Ketu — the Ascendant and any special points are excluded) for
    /// Key Inference's "About Planets" chart + table: dignity, Chara Kāraka, exaltation point, Δ
    /// from that point, and closeness% (<c>round((1 − Δ/180) × 100)</c> — 100% = exact exaltation,
    /// 0% = exact debilitation). Order follows classical Sun…Ketu, not KeyDetails' storage order.
    /// </summary>
    public static IReadOnlyList<ExaltationRow> BuildExaltationRows(IReadOnlyList<ChartKeyDetail> keyDetails) =>
        keyDetails
            .Where(k => k.PointKind == "Graha" && k.Planet != "Ascendant")
            .OrderBy(k => Array.IndexOf(PlanetOrder, k.Planet) is var i && i >= 0 ? i : int.MaxValue)
            .Select(k =>
            {
                if (!ExaltationAbsoluteDegrees.TryGetValue(k.Planet, out var exaltAbsolute))
                    return new ExaltationRow(k.Planet, k.DignityStatus, k.CharaKaraka, null, null, null);

                var delta = AngularDistance(k.NirayanaLongitudeDegrees, exaltAbsolute);
                var closeness = (int)Math.Round((1 - delta / 180.0) * 100);
                return new ExaltationRow(k.Planet, k.DignityStatus, k.CharaKaraka,
                    ExaltationPointDisplay(k.Planet), delta, closeness);
            })
            .ToList();

    public static IReadOnlyList<PlanetRow> BuildPlanetRows(
        IReadOnlyList<ChartKeyDetail> keyDetails,
        IReadOnlyList<ChartHouseLord> houseLords,
        IReadOnlyList<ChartAspect> aspects)
    {
        return keyDetails.Select(k =>
        {
            var rules = houseLords
                .Where(h => h.LordPlanet == k.Planet)
                .OrderBy(h => h.HouseNumber)
                .Select(h => h.HouseNumber.ToString())
                .ToList();

            var castAspects = aspects
                .Where(a => a.AspectingPlanet == k.Planet)
                .Select(a => $"{a.AspectedTarget} ({a.AspectType})")
                .ToList();

            return new PlanetRow(
                k.Planet,
                k.Sign,
                k.DegreesInSignDisplay ?? "",
                k.Nakshatra,
                k.NakshatraPada,
                k.NakshatraLordPlanet,
                k.IsRetrograde,
                k.IsCombust,
                k.DistanceFromSunDegrees,
                k.CombustionOrbUsedDegrees,
                k.HouseNumberFromLagna,
                k.HouseNumberFromMoon,
                k.DignityStatus,
                rules.Count > 0 ? string.Join(", ", rules) : null,
                castAspects.Count > 0 ? string.Join(", ", castAspects) : null,
                k.AspectingPlanets,
                k.CharaKaraka,
                k.NakshatraSubLordPlanet,
                k.SpeedLongitudeDegPerDay,
                k.EclipticLatitudeDegrees,
                k.VargaLongitudeDegrees,
                k.PointKind);
        }).ToList();
    }

    /// <summary>
    /// Classical order grahas are conventionally listed in — used only to keep each cell's
    /// "Aspected by" strip in a stable, predictable order rather than whatever order
    /// tbl_Chart_Aspects rows happen to come back in.
    /// </summary>
    private static readonly string[] PlanetOrder =
        { "Sun", "Moon", "Mars", "Mercury", "Jupiter", "Venus", "Saturn", "Rahu", "Ketu" };

    /// <summary>
    /// Reduces tbl_Chart_Aspects rows to, for each sign that receives at least one aspect, the
    /// distinct glyphs of the planets casting one there — what the South Indian grid's "Aspected by"
    /// strip renders for that cell (rendered once per cell, not once per aspect, so a house aspected
    /// by the same planet under more than one rule still shows just one glyph). Same-sign aspects are
    /// dropped (already conjunct in that one cell — nothing to call out). The aspected target can be
    /// another graha or "Ascendant"; KeyDetails already carries an Ascendant row with its Sign, so no
    /// separate lookup is needed for that case. Works for any chart type (D1, D9, ...) — callers pass
    /// that chart's own KeyDetails/Aspects, already scoped by ChartResultId.
    ///
    /// EXTENSION POINT: adding a new divisional chart (D2, D10, ...)? Call this the same way the
    /// workspace and varga pages do (its own KeyDetails + Aspects in, its own SouthIndianGrid_Detailed's
    /// AspectedByGlyphs out) — see VargaView.razor for the pattern to copy.
    /// </summary>
    /// <summary>
    /// Chip label for one aspecting planet, e.g. "Ma(a)" for the universal 7th-house aspect (the
    /// default, so no number shown) or "Ma(a)-8" for a planet's classical special aspect (2026-08-28,
    /// rammyps's explicit format). <paramref name="aspectType"/> is the label RelationshipEngine
    /// already produces ("7th", "8th", ...) — just strips the ordinal suffix to get the bare number.
    /// </summary>
    private static string FormatAspectChip(string aspectingPlanet, string aspectType)
    {
        var glyph = PlanetGlyph(aspectingPlanet);
        var numeral = new string(aspectType.TakeWhile(char.IsDigit).ToArray());
        return numeral == "7" ? $"{glyph}(a)" : $"{glyph}(a)-{numeral}";
    }

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildAspectedByGlyphs(
        IReadOnlyList<ChartKeyDetail> keyDetails,
        IReadOnlyList<ChartAspect> aspects) =>
        BuildAspectedBy(keyDetails, aspects, FormatAspectChip);

    /// <summary>Plain "Ju-3" style label (no "(a)", the house number always shown, unlike
    /// FormatAspectChip's default-7th omission) for the South Indian chart template's "ASP:" row —
    /// same per-sign grouping as BuildAspectedByGlyphs above, just a different label shape.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildAspectedByPlain(
        IReadOnlyList<ChartKeyDetail> keyDetails,
        IReadOnlyList<ChartAspect> aspects) =>
        BuildAspectedBy(keyDetails, aspects, FormatAspectPlain);

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildAspectedBy(
        IReadOnlyList<ChartKeyDetail> keyDetails,
        IReadOnlyList<ChartAspect> aspects,
        Func<string, string, string> formatChip)
    {
        var signByPlanet = keyDetails.ToDictionary(k => k.Planet, k => k.Sign);

        return aspects
            .Where(a => signByPlanet.ContainsKey(a.AspectingPlanet) && signByPlanet.ContainsKey(a.AspectedTarget))
            .Where(a => signByPlanet[a.AspectingPlanet] != signByPlanet[a.AspectedTarget]) // already conjunct — nothing to call out
            .GroupBy(a => signByPlanet[a.AspectedTarget])
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g
                    .GroupBy(a => a.AspectingPlanet) // same aspecting planet can hit >1 target in this sign — one chip each, not one per target
                    .OrderBy(pg => Array.IndexOf(PlanetOrder, pg.Key))
                    .Select(pg => formatChip(pg.Key, pg.First().AspectType)) // same source offset -> same AspectType for every target in the group
                    .ToList());
    }

    private static string FormatAspectPlain(string aspectingPlanet, string aspectType) =>
        $"{PlanetGlyph(aspectingPlanet)}-{new string(aspectType.TakeWhile(char.IsDigit).ToArray())}";

    /// <summary>"(D)"/"(R)" direction label — null (the Ascendant; no retrograde concept) renders as "—".</summary>
    public static string DirectionLabel(bool? isRetrograde) => isRetrograde switch
    {
        true => "(R)",
        false => "(D)",
        null => "—"
    };

    /// <summary>Combustion label for the planetary-positions table — "—" when not combust or not applicable (Sun/Rahu/Ketu/Ascendant).</summary>
    public static string CombustionLabel(bool? isCombust, decimal? distanceFromSun, decimal? orbUsed) =>
        isCombust == true ? $"🔥 Combust — {distanceFromSun:0.##}° (orb {orbUsed:0.#}°)" : "—";

    /// <summary>Maps a classical DignityStatus label to one of the 7 dignity CSS tokens the chart UI uses.</summary>
    public static string DignityToken(string? dignityStatus) => dignityStatus switch
    {
        "Exalted" => "exalted",
        "Moolatrikona" or "Own Sign" or "Great Friend" => "good",
        "Friend" => "friend",
        "Neutral" => "neutral",
        "Enemy" => "enemy",
        "Great Enemy" => "great-enemy",
        "Debilitated" => "debilitated",
        _ => "neutral"
    };

    /// <summary>Maps a classical DignityStatus label to a signed strength score on a symmetric,
    /// evenly-spaced -4..+4 scale across the same 9 tiers DignityToken collapses to 7 CSS tokens —
    /// Exalted at the top (+4) down to Debilitated at the bottom (-4). Backs the "Dg(+N)" label
    /// D1TemplateGrid prints after each planet's combust icon (rammyps's decision, 2026-09-05).</summary>
    public static int DignityScore(string? dignityStatus) => dignityStatus switch
    {
        "Exalted" => 4,
        "Moolatrikona" => 3,
        "Own Sign" => 2,
        "Great Friend" => 1,
        "Friend" => 0,
        "Neutral" => -1,
        "Enemy" => -2,
        "Great Enemy" => -3,
        "Debilitated" => -4,
        _ => 0
    };

    /// <summary>Two-letter classical-dignity code — the compact alternative to the "Dg(+N)" score for
    /// dense grids (the all-charts gallery). Same 9 tiers as <see cref="DignityScore"/>: Ex(alted),
    /// MT (Moolatrikona), Ow(n sign), GF (Great Friend), Fr(iend), Ne(utral), En(emy), GE (Great
    /// Enemy), De(bilitated). Empty string when unknown/not applicable (so nothing renders).</summary>
    public static string DignityShortCode(string? dignityStatus) => dignityStatus switch
    {
        "Exalted" => "Ex",
        "Moolatrikona" => "MT",
        "Own Sign" => "Ow",
        "Great Friend" => "GF",
        "Friend" => "Fr",
        "Neutral" => "Ne",
        "Enemy" => "En",
        "Great Enemy" => "GE",
        "Debilitated" => "De",
        _ => ""
    };

    /// <summary>Short glyph for the South Indian grid cells (2 letters, Sanskrit-flavored for Jupiter/Rahu/Ketu per this project's convention).</summary>
    public static string PlanetGlyph(string planet) => planet switch
    {
        "Ascendant" => "As",
        "Sun" => "Su",
        "Moon" => "Mo",
        "Mars" => "Ma",
        "Mercury" => "Me",
        "Jupiter" => "Ju",
        "Venus" => "Ve",
        "Saturn" => "Sa",
        "Rahu" => "Ra",
        "Ketu" => "Ke",
        _ => planet.Length >= 2 ? planet[..2] : planet
    };

    /// <summary>Formats a stored <c>BirthDetails.UtcOffset</c> string ("05:30:00", "-08:00:00") for display
    /// as "UTC+05:30" / "UTC-08:00". Returns "—" when blank, or the raw string if it doesn't parse.</summary>
    public static string FormatUtcOffset(string? utcOffset)
    {
        if (string.IsNullOrWhiteSpace(utcOffset)) return "—";
        if (!TimeSpan.TryParse(utcOffset.TrimStart('+'), out var offset)) return utcOffset;
        var abs = offset.Duration();
        return $"UTC{(offset < TimeSpan.Zero ? "-" : "+")}{abs.Hours:00}:{abs.Minutes:00}";
    }

    /// <summary>"Śukla"/"Kṛṣṇa" display for <c>vw_ChartMoonContext.PakshaCode</c> ("SHUKLA"/"KRISHNA").
    /// "—" for anything else (null included).</summary>
    public static string PakshaDisplay(string? pakshaCode) => pakshaCode?.ToUpperInvariant() switch
    {
        "SHUKLA" => "Śukla",
        "KRISHNA" => "Kṛṣṇa",
        _ => "—"
    };

    private static readonly string[] TithiNames =
    {
        "Pratipadā", "Dvitīyā", "Tṛtīyā", "Caturthī", "Pañcamī", "Ṣaṣṭhī", "Saptamī",
        "Aṣṭamī", "Navamī", "Daśamī", "Ekādaśī", "Dvādaśī", "Trayodaśī", "Caturdaśī"
    };

    /// <summary>"9 · Śukla Navamī" style label from <c>vw_ChartMoonContext.TithiNumber</c> (1–30,
    /// Shukla then Krishna back-to-back) + <c>PakshaCode</c> — the 15th of each paksha is Pūrṇimā
    /// (Shukla) / Amāvāsyā (Krishna) rather than a numbered tithi name. "—" when tithiNumber is null.</summary>
    public static string TithiDisplay(byte? tithiNumber, string? pakshaCode)
    {
        if (tithiNumber is null) return "—";
        var withinPaksha = ((tithiNumber.Value - 1) % 15) + 1; // 1..15
        var isShukla = string.Equals(pakshaCode, "SHUKLA", StringComparison.OrdinalIgnoreCase);
        var name = withinPaksha == 15 ? (isShukla ? "Pūrṇimā" : "Amāvāsyā") : TithiNames[withinPaksha - 1];
        return $"{withinPaksha} · {PakshaDisplay(pakshaCode)} {name}";
    }
}
