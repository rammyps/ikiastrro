using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Yoga;

/// <summary>PVR chapter 11's "P0" named-yoga gap set (yoga-corpus.md "Next implementation
/// slice" step 1): 9 formations from SRC_PVR_INTEGRATED §11.5–11.7.1 that were previously
/// untracked by any YogaCode — not even a catalog placeholder. Their pair/sibling formations
/// (Sarpa, Gaja-Kesari, Vipareeta Raja, Kalpadruma/Parijata, Hari-Hara-Brahma, ...) are already
/// shipped elsewhere; only the confirmed gaps live here.</summary>
public static class PvrChapter11YogaEvaluator
{
    private static readonly PlanetName[] B = [PlanetName.Moon, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus];
    private static readonly PlanetName[] M = [PlanetName.Sun, PlanetName.Mars, PlanetName.Saturn];
    private static readonly int[] Kendra = [1, 4, 7, 10];
    private static readonly int[] KendraTrikona = [1, 4, 5, 7, 9, 10];

    public static IReadOnlyList<ContextualYogaResult> Evaluate(ChartAnalysisInput d1, ChartAnalysisInput? d9 = null) =>
    [
        Row("YOGA_MAALA", Maala(d1), "PVR_CH11_MAALA", "ch.11 §11.5.2 Dala Yogas; printed pp.119-120",
            "Maalaa Yoga: 3 of the 4 kendras occupied by natural benefics. A malefic also present in one of those kendras weakens, but per PVR's own wording does not negate, the yoga."),
        Row("YOGA_SARPA", Sarpa(d1), "PVR_CH11_SARPA", "ch.11 §11.5.2 Dala Yogas; printed p.120",
            "Sarpa Yoga: 3 of the 4 kendras occupied by natural malefics (Sun, Mars, Saturn, Rahu, Ketu — PVR's own example uses Mars, Rahu and Ketu). A benefic also present in one of those kendras weakens, but per PVR's own wording does not negate, the yoga."),
        Row("YOGA_MRIDANGA", Mridanga(d1), "PVR_CH11_MRIDANGA", "ch.11 §11.6; printed p.126",
            "Mridanga Yoga: one or more planets in own or exaltation sign in a kendra or trikona, and the lagna lord strong (own, exaltation or moolatrikona — PVR §3.3)."),
        Row("YOGA_SUBHA", Subha(d1), "PVR_CH11_SUBHA", "ch.11 §11.6; printed p.124",
            "Subha Yoga: lagna occupied by a natural benefic, or subha kartari (natural benefics in both the 2nd and 12th)."),
        Row("YOGA_ASUBHA", Asubha(d1), "PVR_CH11_ASUBHA", "ch.11 §11.6; printed p.124",
            "Asubha Yoga: lagna occupied by a natural malefic, or paapa kartari (natural malefics in both the 2nd and 12th)."),
        Row("YOGA_GURU_MANGALA", GuruMangala(d1), "PVR_CH11_GURU_MANGALA", "ch.11 §11.6; printed p.125",
            "Guru-Mangala Yoga: Jupiter and Mars conjoined or in mutual 7th houses."),
        Row("YOGA_CHAMARA", Chamara(d1), "PVR_CH11_CHAMARA", "ch.11 §11.6; printed pp.125-126",
            "Chaamara Yoga: lagna lord exalted in a kendra with Jupiter's aspect, or two natural benefics joined in the 7th, 9th or 10th."),
        Row("YOGA_KHADGA", Khadga(d1), "PVR_CH11_KHADGA", "ch.11 §11.6; printed pp.126-127",
            "Khadga Yoga: 2nd lord in the 9th, 9th lord in the 2nd, and lagna lord in a kendra or trikona."),
        Row("YOGA_LAGNAADHI", Lagnaadhi(d1), "PVR_CH11_LAGNAADHI", "ch.11 §11.6; printed p.129",
            "Lagnaadhi Yoga: the 7th and 8th from lagna both occupied by natural benefics, with no malefic conjoining or aspecting those houses. Adhi Yoga's Lagna-reckoned counterpart (classical Adhi is Moon-reckoned, houses 6/7/8)."),
        Row("YOGA_SAARADA", Saarada(d1), "PVR_CH11_SAARADA", "ch.11 §11.6; printed p.131",
            "Saarada Yoga: 10th lord in the 5th, Mercury in a kendra, Sun in Leo (own sign = strong), Mercury or Jupiter in a trine from Moon, and Mars in the 11th."),
        Row("YOGA_DHARMA_KARMADHIPATI", DharmaKarmadhipati(d1), "PVR_CH11_DHARMA_KARMADHIPATI", "ch.11 §11.7.1; printed p.134",
            "Dharma-Karmadhipati Yoga: the special case of Basic Raaja Yoga formed by the 9th and 10th lords — conjunction, mutual graha drishti, or parivartana (exchange)."),
        Row("YOGA_BASIC_RAJA", BasicRaja(d1), "PVR_CH11_BASIC_RAJA", "ch.11 §11.7.1; printed pp.133-134",
            "Basic Raaja Yoga: a distinct kendra lord and trikona lord are conjoined, mutually aspect each other, or exchange signs; Lagna counts as both kendra and trikona."),
        Row("YOGA_HARI", Hari(d1), "PVR_CH11_HARI", "ch.11 §11.6; printed p.129",
            "Hari Yoga: natural benefics occupy the 2nd, 12th and 8th signs counted from the 2nd lord."),
        Row("YOGA_HARA", Hara(d1), "PVR_CH11_HARA", "ch.11 §11.6; printed p.129",
            "Hara Yoga: natural benefics occupy the 4th, 9th and 8th signs counted from the 7th lord."),
        Row("YOGA_BRAHMA_TRIMURTI", BrahmaTrimurti(d1), "PVR_CH11_BRAHMA_TRIMURTI", "ch.11 §11.6; printed pp.129-130",
            "Brahma (Trimurti) Yoga: natural benefics occupy the 4th, 10th and 11th signs counted from the Lagna lord; distinct from PVR's second Brahma variation."),
        d9 is null
            ? Missing("YOGA_PARIJATHA", "PVR_CH11_KALPADRUMA", "ch.11 §11.6; printed pp.127-129", "D9 is required to identify the Navamsa dispositor in PVR's four-link Kalpadruma definition.")
            : Row("YOGA_PARIJATHA", Kalpadruma(d1, d9), "PVR_CH11_KALPADRUMA", "ch.11 §11.6; printed pp.127-129",
                "Kalpadruma (Parijata) Yoga: Lagna lord, its dispositor, that planet's Rasi dispositor and its Navamsa dispositor are each in a kendra, trikona or exaltation sign in D1."),
    ];

    // Maalaa Yoga: "If three quadrants are occupied by natural benefics, this yoga is formed."
    private static bool Maala(ChartAnalysisInput c) =>
        Kendra.Count(h => OccupiedBy(c, h, B)) >= 3;

    // Sarpa Yoga: "If three quadrants are occupied by natural malefics, this yoga is formed."
    // PVR's worked example places Mars, Rahu and Ketu in the quadrants, so the nodes count as malefics.
    private static readonly PlanetName[] SarpaMalefics =
        [PlanetName.Sun, PlanetName.Mars, PlanetName.Saturn, PlanetName.Rahu, PlanetName.Ketu];
    private static bool Sarpa(ChartAnalysisInput c) =>
        Kendra.Count(h => OccupiedBy(c, h, SarpaMalefics)) >= 3;

    // Mridanga Yoga: "(1) there are planets in own and exaltation signs in quadrants and trines and
    // (2) lagna lord is strong". Strong = own rasi, exaltation rasi or moolatrikona (PVR §3.3).
    private static readonly PlanetName[] Classical7 =
        [PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn];
    private static bool IsStrong(ChartAnalysisInput c, PlanetName p) => Dignity(c, p) is "OWN" or "MOOLATRIKONA" or "EXALTED";
    private static bool Mridanga(ChartAnalysisInput c) =>
        IsStrong(c, Lord(c, 1)) &&
        Classical7.Any(p => Dignity(c, p) is "OWN" or "EXALTED" && Find(c, p) is { } x && KendraTrikona.Contains(x.HouseNumber));

    // Subha Yoga: "If lagna has benefics or has subha kartari - benefics in 12th and 2nd."
    private static bool Subha(ChartAnalysisInput c) =>
        OccupiedBy(c, 1, B) || (OccupiedBy(c, 2, B) && OccupiedBy(c, 12, B));

    // Asubha Yoga: the malefic mirror of Subha Yoga.
    private static bool Asubha(ChartAnalysisInput c) =>
        OccupiedBy(c, 1, M) || (OccupiedBy(c, 2, M) && OccupiedBy(c, 12, M));

    // Guru-Mangala Yoga: "If Jupiter and Mars are together or in the 7th house from each other."
    private static bool GuruMangala(ChartAnalysisInput c)
    {
        var ju = Find(c, PlanetName.Jupiter); var ma = Find(c, PlanetName.Mars);
        if (ju is null || ma is null) return false;
        return ju.Sign == ma.Sign || Distance(ju.Sign, ma.Sign) == 7;
    }

    // Chaamara Yoga: "If the lagna lord is exalted in a quadrant with Jupiter's aspect or two
    // benefics join in 7th, 9th or 10th."
    private static bool Chamara(ChartAnalysisInput c)
    {
        var lagnaLord = Lord(c, 1); var llPos = Find(c, lagnaLord);
        if (llPos is not null && Dignity(c, lagnaLord) == "EXALTED" && Kendra.Contains(llPos.HouseNumber) &&
            Influenced(c, lagnaLord, PlanetName.Jupiter)) return true;
        return new[] { 7, 9, 10 }.Any(h =>
            c.Planets.Count(p => Enum.TryParse<PlanetName>(p.Planet, out var pn) && B.Contains(pn) && p.HouseNumber == h) >= 2);
    }

    // Khadga Yoga: "(1) the 2nd lord is in the 9th house, (2) the 9th lord is in the 2nd house,
    // and, (3) lagna lord is in a quadrant or a trine."
    private static bool Khadga(ChartAnalysisInput c) =>
        Find(c, Lord(c, 2))?.HouseNumber == 9 && Find(c, Lord(c, 9))?.HouseNumber == 2 &&
        KendraTrikona.Contains(Find(c, Lord(c, 1))?.HouseNumber ?? -1);

    // Lagnaadhi Yoga: "(1) the 7th and 8th houses from lagna are occupied by benefics and (2) no
    // malefics conjoin or aspect these planets."
    private static bool Lagnaadhi(ChartAnalysisInput c)
    {
        foreach (var h in new[] { 7, 8 })
        {
            if (!OccupiedBy(c, h, B)) return false;
            if (OccupiedBy(c, h, M)) return false;
            var sign = HouseSign(c, h);
            if (M.Any(m => InfluencesSign(c, m, sign))) return false;
        }
        return true;
    }

    // Saarada Yoga: "(1) the 10th lord is in the 5th house, (2) Mercury is in a quadrant, (3) Sun
    // is strong in Leo, (4) Mercury or Jupiter is in a trine from Moon, and, (5) Mars is in 11th."
    private static bool Saarada(ChartAnalysisInput c)
    {
        if (Find(c, Lord(c, 10))?.HouseNumber != 5) return false;
        if (Find(c, PlanetName.Mercury)?.HouseNumber is not (1 or 4 or 7 or 10)) return false;
        if (Find(c, PlanetName.Sun)?.Sign != ZodiacName.Leo.ToString()) return false;
        var moon = Find(c, PlanetName.Moon); if (moon is null) return false;
        bool TrineFromMoon(PlanetName p) { var x = Find(c, p); return x is not null && HouseDistance(x.HouseNumber, moon.HouseNumber) is 1 or 5 or 9; }
        if (!TrineFromMoon(PlanetName.Mercury) && !TrineFromMoon(PlanetName.Jupiter)) return false;
        return Find(c, PlanetName.Mars)?.HouseNumber == 11;
    }

    // Dharma-Karmadhipati Yoga: the 9th and 10th lords in a Raaja Yoga association — conjunction,
    // mutual graha drishti, or parivartana (exchange) per §11.7.1's 3-way "association" definition.
    private static bool DharmaKarmadhipati(ChartAnalysisInput c) => RajaAssociation(c, Lord(c, 9), Lord(c, 10));

    private static bool BasicRaja(ChartAnalysisInput c)
    {
        var kendras = new[] { 1, 4, 7, 10 }.Select(h => Lord(c, h)).Distinct();
        var trines = new[] { 1, 5, 9 }.Select(h => Lord(c, h)).Distinct();
        return kendras.Any(k => trines.Any(t => k != t && RajaAssociation(c, k, t)));
    }

    private static bool Hari(ChartAnalysisInput c) => BeneficsFillFrom(c, Lord(c, 2), 2, 12, 8);
    private static bool Hara(ChartAnalysisInput c) => BeneficsFillFrom(c, Lord(c, 7), 4, 9, 8);
    private static bool BrahmaTrimurti(ChartAnalysisInput c) => BeneficsFillFrom(c, Lord(c, 1), 4, 10, 11);

    private static bool BeneficsFillFrom(ChartAnalysisInput c, PlanetName reference, params int[] distances)
    {
        var origin = Find(c, reference);
        return origin is not null && distances.All(d => OccupiedBySign(c, RelativeSign(origin.Sign, d), B));
    }

    private static bool Kalpadruma(ChartAnalysisInput d1, ChartAnalysisInput d9)
    {
        var lagnaLord = Lord(d1, 1);
        var lagnaLordPosition = Find(d1, lagnaLord); if (lagnaLordPosition is null) return false;
        var firstDispositor = SignLord(lagnaLordPosition.Sign);
        var firstPosition = Find(d1, firstDispositor); if (firstPosition is null) return false;
        var rasiDispositor = SignLord(firstPosition.Sign);
        var firstInD9 = Find(d9, firstDispositor); if (firstInD9 is null) return false;
        var navamsaDispositor = SignLord(firstInD9.Sign);
        return new[] { lagnaLord, firstDispositor, rasiDispositor, navamsaDispositor }
            .All(p => Find(d1, p) is { } x && (KendraTrikona.Contains(x.HouseNumber) || Dignity(d1, p) == "EXALTED"));
    }

    private static bool RajaAssociation(ChartAnalysisInput c, PlanetName a, PlanetName b)
    {
        var pa = Find(c, a); var pb = Find(c, b);
        if (pa is null || pb is null) return false;
        if (pa.Sign == pb.Sign) return true;
        if (Aspects(a, pa.Sign, pb.Sign) || Aspects(b, pb.Sign, pa.Sign)) return true;
        var signLordOfA = Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(pa.Sign)));
        var signLordOfB = Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(pb.Sign)));
        return signLordOfA == b && signLordOfB == a;
    }

    private static bool OccupiedBy(ChartAnalysisInput c, int house, PlanetName[] set) =>
        c.Planets.Any(p => Enum.TryParse<PlanetName>(p.Planet, out var pn) && set.Contains(pn) && p.HouseNumber == house);
    private static bool OccupiedBySign(ChartAnalysisInput c, string sign, PlanetName[] set) =>
        c.Planets.Any(p => Enum.TryParse<PlanetName>(p.Planet, out var pn) && set.Contains(pn) && p.Sign == sign);
    private static bool Influenced(ChartAnalysisInput c, PlanetName target, PlanetName source) =>
        Find(c, target) is { } x && Find(c, source) is { } y && (x.Sign == y.Sign || Aspects(source, y.Sign, x.Sign));
    private static bool InfluencesSign(ChartAnalysisInput c, PlanetName source, string sign) =>
        Find(c, source) is { } x && (x.Sign == sign || Aspects(source, x.Sign, sign));
    private static bool Aspects(PlanetName p, string a, string b)
    {
        var d = Distance(a, b);
        return d == 7 || (p == PlanetName.Mars && d is 4 or 8) || (p == PlanetName.Jupiter && d is 5 or 9) || (p == PlanetName.Saturn && d is 3 or 10);
    }
    private static int Distance(string? a, string? b) =>
        a is null || b is null ? 0 : (((int)Enum.Parse<ZodiacName>(b) - (int)Enum.Parse<ZodiacName>(a) + 12) % 12) + 1;
    private static int HouseDistance(int a, int b) => ((a - b + 12) % 12) + 1;
    private static string HouseSign(ChartAnalysisInput c, int h) => HouseEngine.GetHouseSign(c.AscendantSign, h).ToString();
    private static string RelativeSign(string sign, int distance) => ((ZodiacName)(((int)Enum.Parse<ZodiacName>(sign) + distance - 1) % 12)).ToString();
    private static PlanetName SignLord(string sign) => Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(sign)));
    private static string Dignity(ChartAnalysisInput c, PlanetName p)
    {
        var x = Find(c, p); if (x is null) return "MISSING";
        var l = x.VargaLongitudeDegrees ?? x.NirayanaLongitudeDegrees ?? 15;
        return PvrDignityEvaluator.Evaluate(p, Enum.Parse<ZodiacName>(x.Sign), ((l % 30) + 30) % 30).DignityTypeCode;
    }
    private static PlanetName Lord(ChartAnalysisInput c, int h) => Enum.Parse<PlanetName>(HouseEngine.GetSignLord(HouseEngine.GetHouseSign(c.AscendantSign, h)));
    private static PlanetPosition? Find(ChartAnalysisInput c, PlanetName p) => c.Planets.SingleOrDefault(x => x.Planet == p.ToString());
    private static ContextualYogaResult Row(string code, bool present, string variant, string locator, string notes) =>
        new(code, present, "EVALUATED", "SRC_PVR_INTEGRATED", variant, locator, notes);
    private static ContextualYogaResult Missing(string code, string variant, string locator, string notes) =>
        new(code, null, "NOT_EVALUATED", "SRC_PVR_INTEGRATED", variant, locator, notes);
}
