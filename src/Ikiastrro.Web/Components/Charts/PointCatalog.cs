using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.DivisionalCharts;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Engines.KeyInfo;
using Ikiastrro.Core.Models;

namespace Ikiastrro.Web.Components.Charts;

/// <summary>The families of special points the Spl Lagnas chart can show (the special lagnas themselves keep
/// their own dropdown, driven by <c>SindUniGlyphs.SpecialLagnas</c>).</summary>
public enum PointGroup { Upagraha, Sensitive, Sphuta, SahamKey, Saham }

/// <summary>One special point with its D1 sidereal longitude. <see cref="Short"/> is the 2–3 letter chip text;
/// <see cref="Meaning"/> is the hover line (what it stands for / how it is made).</summary>
public sealed record PointDef(string Code, string Name, string Short, PointGroup Group, double Longitude, string Meaning);

/// <summary>A point placed on the selected chart: its sign in that chart and the house from that chart's Lagna.</summary>
public sealed record PlacedPoint(PointDef Def, ZodiacName Sign, int House);

/// <summary>
/// Every special point the Spl Lagnas chart offers beyond the special lagnas — the upagrahas, Bhṛgu Bindu and Varṇada
/// lagnas, the 14 sphuṭas and the 36 sahams — worked from the D1 key details (the same engines the Astro Facts
/// tables use) and projected onto any divisional chart. Special points are reference points projected into every
/// varga, not charts of their own (PVR §7.1): a point keeps its D1 longitude and takes the sign that longitude falls
/// in under the chosen varga's rule.
/// </summary>
public static class PointCatalog
{
    public static readonly IReadOnlyList<PointGroup> Groups = Enum.GetValues<PointGroup>();

    public static string GroupLabel(PointGroup g) => g switch
    {
        PointGroup.Upagraha => "Upagrahas",
        PointGroup.Sensitive => "Bhṛgu Bindu & Varṇada",
        PointGroup.Sphuta => "Sphuṭas",
        PointGroup.SahamKey => "Sahams — life areas",
        _ => "Sahams — others",
    };

    /// <summary>Colour token per group (tokens.css, light and dark).</summary>
    public static string ColorVar(PointGroup g) => g switch
    {
        PointGroup.Upagraha => "--pg-upagraha",
        PointGroup.Sensitive => "--pg-sensitive",
        PointGroup.Sphuta => "--pg-sphuta",
        _ => "--pg-saham",
    };

    /// <summary>The first set shown by the "Essentials" preset: the two Gulika points, Bhṛgu Bindu, Prāṇa, Deha and
    /// Mṛtyu sphuṭas, and the Punya, Vivaha and Karma sahams.</summary>
    public static readonly IReadOnlySet<string> Essentials = new HashSet<string>
    {
        "UP_GULIKA", "UP_MAANDI", "BB", "SP_PRANA", "SP_DEHA", "SP_MRITYU", "SH_PUNYA", "SH_VIVAHA", "SH_KARMA",
    };

    private static readonly HashSet<string> KeySahams = new()
    {
        "PUNYA", "VIDYA", "PUTRA", "VIVAHA", "KARMA", "ARTHA", "MATRU", "PITRU", "BHRATRU", "MRITYU",
    };

    private static readonly (string Key, string Name, string Short, string Meaning)[] Upagrahas =
    {
        ("Gulika", "Gulika", "Gk", "Saturn's son — hidden troubles in the house it falls in"),
        ("Maandi", "Māndi", "Md", "Saturn's other portion — obstacles and delays"),
        ("Dhuma", "Dhūma", "Dh", "Sun-derived smoke point"),
        ("Vyatipata", "Vyatīpāta", "Vy", "Sun-derived point"),
        ("Parivesha", "Pariveṣa", "Pv", "Sun-derived halo point"),
        ("Indrachapa", "Indra Chāpa", "IC", "Sun-derived rainbow point"),
        ("Upaketu", "Upaketu", "Uk", "Sun-derived point"),
        ("Kaala", "Kāla", "Kl", "Time upagraha (Sun's portion)"),
        ("Mrityu", "Mṛtyu (upagraha)", "Mt", "Death upagraha (Mars's portion)"),
        ("Ardhaprahara", "Ardhaprahara", "AP", "Mercury's portion"),
        ("Yamaghantaka", "Yamaghaṇṭaka", "YG", "Jupiter's portion"),
    };

    /// <summary>All points that can be built from these D1 key details. Empty when the Lagna or a graha is missing.
    /// <paramref name="isNightBirth"/> flips the saham formulas that reverse by night.</summary>
    public static IReadOnlyList<PointDef> Build(IReadOnlyList<ChartKeyDetail> d1KeyDetails, bool isNightBirth)
    {
        var inputs = KeyInfoInputs.From(d1KeyDetails);
        var list = new List<PointDef>();

        foreach (var (key, name, shortCode, meaning) in Upagrahas)
        {
            var row = d1KeyDetails.FirstOrDefault(k => k.PointKind == "Upagraha" && k.Planet == key);
            if (row is not null)
                list.Add(new($"UP_{key.ToUpperInvariant()}", name, shortCode, PointGroup.Upagraha, row.NirayanaLongitudeDegrees, meaning));
        }

        if (!inputs.Complete) return list;
        var g = inputs.Grahas;

        list.Add(new("BB", "Bhṛgu Bindu", "BB", PointGroup.Sensitive,
            SensitivePoints.BhriguBindu(g[PlanetName.Rahu], g[PlanetName.Moon]), "Rahu–Moon midpoint — triggers events in transit"));
        var hora = d1KeyDetails.FirstOrDefault(k => k.PointKind == "SpecialLagna" && k.Planet == "HL")?.NirayanaLongitudeDegrees;
        if (hora is { } hl)
            for (var h = 1; h <= 12; h++)
                list.Add(new($"V{h}", $"Varṇada V{h}", $"V{h}", PointGroup.Sensitive, SensitivePoints.Varnada(inputs.Lagna, hl, h),
                    h == 1 ? "Varṇada lagna — work, status and community" : $"Varṇada of the {KeyInfoFormat.Ordinal(h)} house"));

        if (inputs.Gulika is { } gulika)
            foreach (var s in Sphutas.Compute(inputs.Lagna, g[PlanetName.Sun], g[PlanetName.Moon], g[PlanetName.Mars],
                         g[PlanetName.Jupiter], g[PlanetName.Venus], g[PlanetName.Rahu], gulika))
                list.Add(new($"SP_{s.Code}", s.Name, SphutaShort(s.Code), PointGroup.Sphuta, s.Longitude, "Sphuṭa — " + s.Name));

        foreach (var s in SahamTable.Compute(g, inputs.Lagna, isNightBirth))
            list.Add(new($"SH_{s.Code}", s.Name.Replace(" Saham", " saham"), SahamShort(s.Code),
                KeySahams.Contains(s.Code) ? PointGroup.SahamKey : PointGroup.Saham, s.LongitudeDegrees, s.Meaning));
        return list;
    }

    /// <summary>Place points on a chart. <paramref name="rule"/> is that varga's sign rule (null for D1);
    /// <paramref name="ascendant"/> is that chart's Lagna sign, for the house number.</summary>
    public static IReadOnlyList<PlacedPoint> Place(IEnumerable<PointDef> points, IVargaSignRule? rule, ZodiacName ascendant) =>
        points.Select(p =>
        {
            var sign = rule is null ? AstroMath.GetSignAtLongitude(p.Longitude) : rule.SignFor(p.Longitude);
            return new PlacedPoint(p, sign, ((int)sign - (int)ascendant + 12) % 12 + 1);
        }).ToList();

    private static string SphutaShort(string code) => code switch
    {
        "PRANA" => "Pr", "DEHA" => "De", "MRITYU" => "Mr", "SOOKSHMA_TRI" => "ST", "TITHI" => "Ti", "YOGA_SUN_MOON" => "Yo",
        "RAHU_TITHI" => "RT", "KSHETRA" => "Ks", "BEEJA" => "Bj", "TRI" => "Tr", "CHATUS" => "Ct", "PANCHA" => "Pn",
        "YOGI" => "Yg", "AVAYOGI" => "Ay", _ => code[..Math.Min(2, code.Length)],
    };

    private static string SahamShort(string code) => code switch
    {
        "PUNYA" => "Pu", "VIDYA" => "Vd", "YASAS" => "Ya", "MITRA" => "Mi", "MAHATMYA" => "Mh", "ASHA" => "As",
        "SAMARTHA" => "Sm", "BHRATRU" => "Bh", "GAURAVA" => "Ga", "PITRU" => "Pi", "RAJYA" => "Rj", "MATRU" => "Ma",
        "PUTRA" => "Pt", "JEEVA" => "Je", "KARMA" => "Ka", "ROGA" => "Ro", "KALI" => "Ki", "SASTRA" => "Sh",
        "BANDHU" => "Bn", "MRITYU" => "Mu", "PARADESA" => "Pd", "ARTHA" => "Ar", "PARADARA" => "Pa", "VANIK" => "Vn",
        "KARYASIDDHI" => "Kd", "VIVAHA" => "Vi", "SANTAPA" => "Sn", "SRADDHA" => "Sr", "PREETI" => "Pe", "JADYA" => "Jd",
        "VYAPARA" => "Vp", "SATRU" => "Sa", "JALAPATANA" => "Jp", "BANDHANA" => "Bd", "APAMRITYU" => "Am", "LABHA" => "La",
        _ => code[..Math.Min(2, code.Length)],
    };
}
