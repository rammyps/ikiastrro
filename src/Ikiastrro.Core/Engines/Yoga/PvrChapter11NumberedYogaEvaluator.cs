using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Relationships;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Yoga;

/// <summary>PVR chapter 11's 4 unnamed numbered blocks (yoga-corpus.md "Unnamed numbered
/// combinations" table): §11.7.3 "More Raja Yogas" (18), §11.8 Raaja Sambandha Yogas (15),
/// §11.9 Dhana Yogas (12 per-lagna + 1 basic principle = 13), §11.10 Daridra Yogas (13). 59
/// items total. These had no per-item transcription anywhere — only the generic YOGA_DHANA/
/// YOGA_DARIDRA catch-alls existed for the last two blocks, and the Raja/Sambandha blocks had
/// no home at all. Per the plan, §11.7.3 and §11.8 get their own umbrella YogaCodes
/// (YOGA_RAJA_ADVANCED / YOGA_RAJA_SAMBANDHA); the Dhana/Daridra items are added as extra
/// SourceVariantCode rows under the existing YOGA_DHANA/YOGA_DARIDRA codes (source variants are
/// never collapsed — see ProductionYogaEngine's own doc comment).</summary>
public static class PvrChapter11NumberedYogaEvaluator
{
    private static readonly PlanetName[] Classical7 =
        [PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn];
    private static readonly PlanetName[] B = [PlanetName.Moon, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus];
    private static readonly PlanetName[] M = [PlanetName.Sun, PlanetName.Mars, PlanetName.Saturn];
    private static readonly int[] Kendra = [1, 4, 7, 10];
    private static readonly int[] KendraTrikona = [1, 4, 5, 7, 9, 10];
    private static readonly int[] Dusthana = [6, 8, 12];
    private const string RajaAdvLocator = "ch.11 §11.7.3 \"More Raja Yogas\"; printed pp.137-139";
    private const string SambandhaLocator = "ch.11 §11.8 Raaja Sambandha Yogas; printed pp.139-141";
    private const string DhanaLocator = "ch.11 §11.9 Dhana Yogas; printed pp.141-142";
    private const string DaridraLocator = "ch.11 §11.10 Daridra Yogas; printed pp.142-144";

    public static IReadOnlyList<ContextualYogaResult> Evaluate(ChartBundle bundle)
    {
        var c = bundle.Charts.FirstOrDefault(x => x.ChartType.Equals("D1", StringComparison.OrdinalIgnoreCase));
        if (c is null) return [];
        var d9 = bundle.Charts.FirstOrDefault(x => x.ChartType.Equals("D9", StringComparison.OrdinalIgnoreCase));
        var d3 = bundle.Charts.FirstOrDefault(x => x.ChartType.Equals("D3", StringComparison.OrdinalIgnoreCase));
        var ak = KarakaPlanet(bundle, "AK");
        var amk = KarakaPlanet(bundle, "AmK");

        var rows = new List<ContextualYogaResult>();
        rows.AddRange(RajaAdvanced(c, d9, d3, bundle, ak, amk));
        rows.AddRange(RajaSambandha(c, ak, amk));
        rows.AddRange(Dhana(c));
        rows.AddRange(Daridra(c, d9));
        return rows;
    }

    // ---------------- §11.7.3 More Raja Yogas (18) ----------------
    private static IEnumerable<ContextualYogaResult> RajaAdvanced(
        ChartAnalysisInput c, ChartAnalysisInput? d9, ChartAnalysisInput? d3, ChartBundle bundle, PlanetName? ak, PlanetName? amk)
    {
        var l1 = Lord(c, 1); var l5 = Lord(c, 5); var l6 = Lord(c, 6); var l8 = Lord(c, 8);
        var l9 = Lord(c, 9); var l10 = Lord(c, 10); var l12 = Lord(c, 12);
        var pk = KarakaPlanet(bundle, "PK");
        var lagnaSign = HouseSign(c, 1);

        // (1) "PK and AK conjoined" and "lagna and 5th lords conjoin."
        yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_01", RajaAdvLocator,
            "PK and AK conjoined, and lagna lord and 5th lord conjoined.",
            ak is null || pk is null ? null : Conjunct(c, ak.Value, pk.Value) && Conjunct(c, l1, l5));

        // (2) lagna lord/5th lord exchange; AK and PK both in lagna or 5th; those two in own/
        // exaltation or aspected by a benefic.
        yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_02", RajaAdvLocator,
            "Lagna lord in 5th, 5th lord in lagna (exchange); AK and PK both in lagna or 5th; AK and PK each in own/exaltation sign or aspected by a benefic.",
            ak is null || pk is null ? null :
            InHouse(c, l1, 5) && InHouse(c, l5, 1) &&
            Find(c, ak.Value)?.HouseNumber is 1 or 5 && Find(c, pk.Value)?.HouseNumber is 1 or 5 &&
            Qualifies(c, ak.Value) && Qualifies(c, pk.Value));

        // (3) 9th lord and AK both in lagna, 5th or 7th, aspected by benefics.
        yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_03", RajaAdvLocator,
            "9th lord and AK are each in lagna, 5th or 7th, and each aspected by a benefic.",
            ak is null ? null :
            Find(c, l9)?.HouseNumber is 1 or 5 or 7 && Find(c, ak.Value)?.HouseNumber is 1 or 5 or 7 &&
            AspectedByBenefic(c, l9) && AspectedByBenefic(c, ak.Value));

        // (4) 2nd/4th/5th houses from lagna lord AND from AK occupied by benefics.
        yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_04", RajaAdvLocator,
            "The 2nd, 4th and 5th houses reckoned from both lagna lord and AK are occupied by benefics.",
            ak is null ? null : BeneficsInRelative(c, l1, 2, 4, 5) && BeneficsInRelative(c, ak.Value, 2, 4, 5));

        // (5) 3rd/6th houses from lagna lord AND from AK occupied or aspected by malefics.
        yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_05", RajaAdvLocator,
            "The 3rd and 6th houses reckoned from both lagna lord and AK are occupied or aspected by malefics.",
            ak is null ? null : MalefictedRelative(c, l1, 3, 6) && MalefictedRelative(c, ak.Value, 3, 6));

        var hlSign = SpecialSign(c, "HL"); var glSign = SpecialSign(c, "GL");

        // (6) lagna, HL and GL joined, aspected or owned by the same planet.
        yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_06", RajaAdvLocator,
            "Lagna, HL and GL are joined, aspected or owned by the same planet.",
            hlSign is null || glSign is null ? null :
            Classical7.Any(x => Relates(c, x, lagnaSign) && Relates(c, x, hlSign) && Relates(c, x, glSign)));

        // (7) same planet aspects lagna in all 6 Shad Varga charts.
        {
            var sixTypes = VaiseshikamsaCalculator.Shadvarga;
            var sixCharts = sixTypes.Select(t => bundle.Charts.FirstOrDefault(x => x.ChartType.Equals(t, StringComparison.OrdinalIgnoreCase))).ToArray();
            yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_07", RajaAdvLocator,
                "The same planet aspects lagna in all 6 Shad Varga charts (Rasi, Hora, Drekkana, Navamsa, Dwadasamsa, Trimsamsa).",
                sixCharts.Any(x => x is null) ? null :
                Classical7.Any(x => sixCharts.All(chart => Find(chart!, x) is { } pos && Aspects(x, pos.Sign, chart!.AscendantSign.ToString()))));
        }

        // (8) lagna, HL and GL each occupied by a planet (any) in own or exaltation sign.
        yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_08", RajaAdvLocator,
            "Lagna, HL and GL are each occupied by a (possibly different) planet in its own or exaltation sign.",
            hlSign is null || glSign is null ? null :
            OccupiedByStrongPlanet(c, lagnaSign) && OccupiedByStrongPlanet(c, hlSign) && OccupiedByStrongPlanet(c, glSign));

        // (9) lagna in Rasi, Navamsa and Drekkana each occupied by a planet in own/exaltation.
        yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_09", RajaAdvLocator,
            "Lagna in D1, D9 and D3 is each occupied by a (possibly different) planet in its own or exaltation sign in that chart.",
            d9 is null || d3 is null ? null :
            OccupiedByStrongPlanetInChart(c, c.AscendantSign.ToString()) && OccupiedByStrongPlanetInChart(d9, d9.AscendantSign.ToString()) && OccupiedByStrongPlanetInChart(d3, d3.AscendantSign.ToString()));

        // (10) 1 or 2 debilitated planets in 3rd/6th/8th; lagna lord in own/exaltation and aspects lagna.
        yield return Row("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_10", RajaAdvLocator,
            "1 or 2 debilitated planets occupy the 3rd, 6th or 8th houses; lagna lord is in own or exaltation sign and aspects lagna.",
            Classical7.Count(p => Dignity(c, p) == "DEBILITATED" && Find(c, p)?.HouseNumber is 3 or 6 or 8) is 1 or 2 &&
            Dignity(c, l1) is "OWN" or "MOOLATRIKONA" or "EXALTED" && AspectsOnly(c, l1, lagnaSign));

        // (11) 6th/8th/12th lords debilitated, combust or in an inimical sign; lagna lord strong and aspects lagna.
        yield return Row("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_11", RajaAdvLocator,
            "The 6th, 8th and 12th lords are each debilitated, combust or in an inimical (Shatru/Adhishatru) sign; lagna lord is in own or exaltation sign and aspects lagna.",
            new[] { l6, l8, l12 }.All(p => Dignity(c, p) == "DEBILITATED" || IsCombust(c, p) || CompoundRelationship(c, p) is "SHATRU" or "ADHISHATRU") &&
            Dignity(c, l1) is "OWN" or "MOOLATRIKONA" or "EXALTED" && AspectsOnly(c, l1, lagnaSign));

        // (12) 5th and 9th lords in conjunction or mutual aspect.
        yield return Row("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_12", RajaAdvLocator,
            "The 5th and 9th lords are in conjunction or mutual aspect.",
            Conjunct(c, l5, l9) || (Find(c, l5) is { } p5 && Find(c, l9) is { } p9 && (Aspects(l5, p5.Sign, p9.Sign) || Aspects(l9, p9.Sign, p5.Sign))));

        // (13) 4th lord in 10th, 10th lord in 4th (exchange); both aspected by 5th or 9th lord.
        yield return Row("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_13", RajaAdvLocator,
            "4th lord in the 10th, 10th lord in the 4th (exchange), and both are aspected by the 5th lord or the 9th lord.",
            InHouse(c, Lord(c, 4), 10) && InHouse(c, l10, 4) &&
            (AspectsOnly(c, l5, Find(c, Lord(c, 4))!.Sign) || AspectsOnly(c, l9, Find(c, Lord(c, 4))!.Sign)) &&
            (AspectsOnly(c, l5, Find(c, l10)!.Sign) || AspectsOnly(c, l9, Find(c, l10)!.Sign)));

        // (14) 5th lord in 1st/4th/10th; lagna lord or 9th lord joins him.
        yield return Row("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_14", RajaAdvLocator,
            "5th lord is in the 1st, 4th or 10th house, and lagna lord or the 9th lord conjoins him.",
            Find(c, l5)?.HouseNumber is 1 or 4 or 10 && (Conjunct(c, l5, l1) || Conjunct(c, l5, l9)));

        // (15) Moon strong and vargottama; 4+ planets aspect Moon.
        yield return Row("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_15", RajaAdvLocator,
            "Moon is strong and vargottama, and 4 or more planets aspect Moon.",
            Strong(c, PlanetName.Moon) && bundle.Vargottama.Any(v => v.Planet == "Moon" && v.IsVargottama) &&
            Classical7.Count(p => p != PlanetName.Moon && AspectsOnly(c, p, Find(c, PlanetName.Moon)!.Sign)) >= 4);

        // (16) 4+ planets in moolatrikona or exaltation.
        yield return Row("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_16", RajaAdvLocator,
            "4 or more planets occupy moolatrikona or exaltation signs.",
            Classical7.Count(p => Dignity(c, p) is "MOOLATRIKONA" or "EXALTED") >= 4);

        // (17) benefics confined to quadrants, malefics confined to 3rd/6th/11th.
        {
            var benefics = c.Planets.Where(pl => Enum.TryParse<PlanetName>(pl.Planet, out var pn) && B.Contains(pn)).ToList();
            var malefics = c.Planets.Where(pl => Enum.TryParse<PlanetName>(pl.Planet, out var pn) && M.Contains(pn)).ToList();
            yield return Row("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_17", RajaAdvLocator,
                "Every natural benefic present occupies a kendra, and every natural malefic present occupies the 3rd, 6th or 11th.",
                benefics.Count > 0 && malefics.Count > 0 && benefics.All(pl => Kendra.Contains(pl.HouseNumber)) && malefics.All(pl => new[] { 3, 6, 11 }.Contains(pl.HouseNumber)));
        }

        // (18) AL and darapada (A7) NOT in mutual 2/12 or 6/8.
        {
            var alSign = SpecialSign(c, "AL"); var a7Sign = SpecialSign(c, "A7");
            yield return RowOrMissing("YOGA_RAJA_ADVANCED", "PVR_CH11_RAJA_ADV_18", RajaAdvLocator,
                "Arudha Lagna and darapada (A7) are not in a mutual 2nd/12th or 6th/8th relationship — makes any Raja Yoga in the chart more effective, per PVR's own wording, rather than forming one on its own.",
                alSign is null || a7Sign is null ? null : !(Distance(alSign, a7Sign) is 2 or 12 or 6 or 8));
        }
    }

    // ---------------- §11.8 Raaja Sambandha Yogas (15) ----------------
    private static IEnumerable<ContextualYogaResult> RajaSambandha(ChartAnalysisInput c, PlanetName? ak, PlanetName? amk)
    {
        var l9 = Lord(c, 9); var l10 = Lord(c, 10); var l11 = Lord(c, 11); var l1 = Lord(c, 1); var l5 = Lord(c, 5);

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_01", SambandhaLocator,
            "10th lord is conjoined or aspected by AmK or AmK's dispositor.",
            amk is null ? null : Influenced(c, l10, amk.Value) || (Dispositor(c, amk.Value) is { } amkDisp && Influenced(c, l10, amkDisp)));

        yield return Row("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_02", SambandhaLocator,
            "11th lord aspects the 11th house, and no malefic joins or aspects the 10th or 11th house.",
            AspectsOnly(c, l11, HouseSign(c, 11)) && !M.Any(m => InfluencesSign(c, m, HouseSign(c, 10)) || InfluencesSign(c, m, HouseSign(c, 11))));

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_03", SambandhaLocator,
            "AK and AmK conjoin.", ak is null || amk is null ? null : Conjunct(c, ak.Value, amk.Value));

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_04", SambandhaLocator,
            "AmK is strong in own or exaltation sign.", amk is null ? null : Dignity(c, amk.Value) is "OWN" or "MOOLATRIKONA" or "EXALTED");

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_05", SambandhaLocator,
            "AmK is in a trine from lagna.", amk is null ? null : Find(c, amk.Value)?.HouseNumber is 1 or 5 or 9);

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_06", SambandhaLocator,
            "AmK is in a quadrant or trine from AK.",
            ak is null || amk is null ? null : KendraTrikona.Contains(HouseDistance(Find(c, amk.Value)!.HouseNumber, Find(c, ak.Value)!.HouseNumber)));

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_07", SambandhaLocator,
            "Malefics occupy the 3rd and 6th houses reckoned from lagna, AL and AK.",
            ak is null || SpecialHouse(c, "AL") is not int alHouse ? null :
            MalefictsIn36From(c, 1) && MalefictsIn36From(c, alHouse) && MalefictsIn36From(c, Find(c, ak.Value)!.HouseNumber));

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_08", SambandhaLocator,
            "AK is in own or exaltation sign in a kendra or trikona, and the 9th lord is conjoined or aspected by AK.",
            ak is null ? null : Dignity(c, ak.Value) is "OWN" or "MOOLATRIKONA" or "EXALTED" && KendraTrikona.Contains(Find(c, ak.Value)!.HouseNumber) && Influenced(c, l9, ak.Value));

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_09", SambandhaLocator,
            "AK is Moon's D1 dispositor and occupies lagna together with a benefic.",
            ak is null ? null : ak.Value == Dispositor(c, PlanetName.Moon) && InHouse(c, ak.Value, 1) && B.Any(b => b != ak.Value && Conjunct(c, ak.Value, b)));

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_10", SambandhaLocator,
            "AK is in the 5th, 7th, 9th or 10th with a benefic.",
            ak is null ? null : Find(c, ak.Value)?.HouseNumber is 5 or 7 or 9 or 10 && B.Any(b => b != ak.Value && Conjunct(c, ak.Value, b)));

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_11", SambandhaLocator,
            "Bhagyapada (A9) is in lagna, or AK is in the 9th.",
            ak is null ? null : SpecialHouse(c, "A9") == 1 || Find(c, ak.Value)?.HouseNumber == 9);

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_12", SambandhaLocator,
            "11th lord is in the 11th without any malefic's aspect, and AK is with a benefic.",
            ak is null ? null : InHouse(c, l11, 11) && !M.Any(m => InfluencesSign(c, m, HouseSign(c, 11))) && B.Any(b => b != ak.Value && Conjunct(c, ak.Value, b)));

        yield return Row("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_13", SambandhaLocator,
            "Lagna lord is in the 10th and 10th lord is in lagna (exchange).",
            InHouse(c, l1, 10) && InHouse(c, l10, 1));

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_14", SambandhaLocator,
            "Moon and Venus are both in the 4th house reckoned from AK.",
            ak is null ? null : Find(c, PlanetName.Moon)?.HouseNumber == RelativeHouse(Find(c, ak.Value)!.HouseNumber, 4) && Find(c, PlanetName.Venus)?.HouseNumber == RelativeHouse(Find(c, ak.Value)!.HouseNumber, 4));

        yield return RowOrMissing("YOGA_RAJA_SAMBANDHA", "PVR_CH11_RAJA_SAMBANDHA_15", SambandhaLocator,
            "Lagna lord or AK conjoins the 5th lord in a kendra or trikona.",
            ak is null ? null : (Conjunct(c, l1, l5) || Conjunct(c, ak.Value, l5)) && KendraTrikona.Contains(Find(c, l5)?.HouseNumber ?? -1));
    }

    // ---------------- §11.9 Dhana Yogas: basic principle + 12 per-lagna (13) ----------------
    private sealed record DhanaRow(ZodiacName Lagna, PlanetName In5, PlanetName[] In11, PlanetName AltInLagna, PlanetName[] AltAssociates, string VariantSuffix);
    private static readonly DhanaRow[] DhanaTable =
    [
        new(ZodiacName.Aries, PlanetName.Sun, [PlanetName.Saturn, PlanetName.Moon, PlanetName.Jupiter], PlanetName.Mars, [PlanetName.Mercury, PlanetName.Venus, PlanetName.Saturn], "ARIES"),
        new(ZodiacName.Taurus, PlanetName.Mercury, [PlanetName.Moon, PlanetName.Mars, PlanetName.Jupiter], PlanetName.Venus, [PlanetName.Mercury, PlanetName.Saturn], "TAURUS"),
        new(ZodiacName.Gemini, PlanetName.Venus, [PlanetName.Mars], PlanetName.Mercury, [PlanetName.Jupiter, PlanetName.Saturn], "GEMINI"),
        new(ZodiacName.Cancer, PlanetName.Mars, [PlanetName.Venus], PlanetName.Moon, [PlanetName.Mercury, PlanetName.Jupiter], "CANCER"),
        new(ZodiacName.Leo, PlanetName.Jupiter, [PlanetName.Mercury], PlanetName.Sun, [PlanetName.Mars, PlanetName.Jupiter], "LEO"),
        new(ZodiacName.Virgo, PlanetName.Saturn, [PlanetName.Sun, PlanetName.Moon], PlanetName.Mercury, [PlanetName.Jupiter, PlanetName.Saturn], "VIRGO"),
        new(ZodiacName.Libra, PlanetName.Saturn, [PlanetName.Sun, PlanetName.Moon], PlanetName.Venus, [PlanetName.Mercury, PlanetName.Saturn], "LIBRA"),
        new(ZodiacName.Scorpio, PlanetName.Jupiter, [PlanetName.Mercury], PlanetName.Mars, [PlanetName.Mercury, PlanetName.Venus, PlanetName.Saturn], "SCORPIO"),
        new(ZodiacName.Sagittarius, PlanetName.Mars, [PlanetName.Venus], PlanetName.Jupiter, [PlanetName.Mars, PlanetName.Mercury], "SAGITTARIUS"),
        new(ZodiacName.Capricornus, PlanetName.Venus, [PlanetName.Mars], PlanetName.Saturn, [PlanetName.Mars, PlanetName.Jupiter], "CAPRICORNUS"),
        new(ZodiacName.Aquarius, PlanetName.Mercury, [PlanetName.Moon, PlanetName.Mars, PlanetName.Jupiter], PlanetName.Saturn, [PlanetName.Mars, PlanetName.Jupiter], "AQUARIUS"),
        new(ZodiacName.Pisces, PlanetName.Moon, [], PlanetName.Jupiter, [PlanetName.Mars, PlanetName.Mercury], "PISCES"),
    ];

    private static IEnumerable<ContextualYogaResult> Dhana(ChartAnalysisInput c)
    {
        yield return Row("YOGA_DHANA", "PVR_CH11_DHANA_BASIC", DhanaLocator,
            "Basic Principle: Moon, Mercury, Jupiter or Venus exalted in the 2nd house makes the native very rich.",
            B.Any(p => Find(c, p)?.HouseNumber == 2 && Dignity(c, p) == "EXALTED"));

        foreach (var row in DhanaTable)
        {
            var present = c.AscendantSign == row.Lagna &&
                ((InHouse(c, row.In5, 5) && row.In11.Length > 0 && row.In11.All(p => InHouse(c, p, 11))) ||
                 (InHouse(c, row.AltInLagna, 1) && row.AltAssociates.All(p => Influenced(c, row.AltInLagna, p))));
            var notes = row.Lagna == ZodiacName.Pisces
                ? "For Pi lagna: the printed 5th-house/11th-house clause names only \"Moon...and in the 11th house\" with no second planet — an apparent OCR/print gap in the source. Only the second (lagna) alternative is coded; the first is left unimplemented rather than guessed."
                : $"For {row.Lagna} lagna: {row.In5} in the 5th with {string.Join('/', row.In11)} in the 11th, OR {row.AltInLagna} in lagna conjoined/aspected by {string.Join('/', row.AltAssociates)}.";
            yield return Row("YOGA_DHANA", $"PVR_CH11_DHANA_{row.VariantSuffix}", DhanaLocator, notes, present);
        }
    }

    // ---------------- §11.10 Daridra Yogas (13) ----------------
    private static IEnumerable<ContextualYogaResult> Daridra(ChartAnalysisInput c, ChartAnalysisInput? d9)
    {
        var l1 = Lord(c, 1); var l2 = Lord(c, 2); var l5 = Lord(c, 5); var l6 = Lord(c, 6);
        var l8 = Lord(c, 8); var l9 = Lord(c, 9); var l10 = Lord(c, 10); var l12 = Lord(c, 12);
        var marakas = MarakaSet(c);

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_01", DaridraLocator,
            "Lagna lord in 12th, 12th lord in lagna (exchange), conjoined or aspected by a maraka.",
            InHouse(c, l1, 12) && InHouse(c, l12, 1) && marakas.Any(mk => Influenced(c, l1, mk) || Influenced(c, l12, mk)));

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_02", DaridraLocator,
            "Lagna lord in 6th, 6th lord in lagna (exchange), conjoined or aspected by a maraka.",
            InHouse(c, l1, 6) && InHouse(c, l6, 1) && marakas.Any(mk => Influenced(c, l1, mk) || Influenced(c, l6, mk)));

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_03", DaridraLocator,
            "Lagna or Moon is with Ketu; lagna lord is in the 8th; a maraka conjoins or aspects lagna lord.",
            (InHouse(c, PlanetName.Ketu, 1) || Conjunct(c, PlanetName.Moon, PlanetName.Ketu)) && InHouse(c, l1, 8) && marakas.Any(mk => Influenced(c, l1, mk)));

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_04", DaridraLocator,
            "Lagna lord with a malefic in a dusthana, and 2nd lord debilitated or in an inimical sign.",
            Dusthana.Contains(Find(c, l1)?.HouseNumber ?? -1) && M.Any(m => m != l1 && Conjunct(c, l1, m)) &&
            (Dignity(c, l2) == "DEBILITATED" || CompoundRelationship(c, l2) is "SHATRU" or "ADHISHATRU"));

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_05", DaridraLocator,
            "5th lord in 6th and 9th lord in 12th, with aspects from marakas.",
            InHouse(c, l5, 6) && InHouse(c, l9, 12) && marakas.Any(mk => AspectsOnly(c, mk, Find(c, l5)!.Sign) || AspectsOnly(c, mk, Find(c, l9)!.Sign)));

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_06", DaridraLocator,
            "Malefics occupy lagna without the 9th or 10th lord there, aspected or conjoined by marakas.",
            M.Any(m => InHouse(c, m, 1)) && !InHouse(c, l9, 1) && !InHouse(c, l10, 1) && marakas.Any(mk => InfluencesSign(c, mk, HouseSign(c, 1))));

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_07", DaridraLocator,
            "The dispositors of the 6th/8th/12th lords are each themselves in a dusthana, conjoined or aspected by malefics.",
            new[] { l6, l8, l12 }.All(p => Dispositor(c, p) is { } disp && Dusthana.Contains(Find(c, disp)?.HouseNumber ?? -1) && M.Any(m => Influenced(c, disp, m))));

        yield return RowOrMissing("YOGA_DARIDRA", "PVR_CH11_DARIDRA_08", DaridraLocator,
            "Dispositor of Moon's navamsa sign is with a maraka or occupies a maraka house (2nd/7th).",
            d9 is null ? null : NavamsaLord(d9, PlanetName.Moon) is { } nl && (InHouse(c, nl, 2) || InHouse(c, nl, 7) || marakas.Any(mk => Conjunct(c, nl, mk))));

        yield return RowOrMissing("YOGA_DARIDRA", "PVR_CH11_DARIDRA_09", DaridraLocator,
            "Lords of lagna in Rasi and Navamsa are each conjoined or aspected by marakas.",
            d9 is null ? null : marakas.Any(mk => Influenced(c, l1, mk)) && marakas.Any(mk => Influenced(c, Enum.Parse<PlanetName>(HouseEngine.GetSignLord(d9.AscendantSign)), mk)));

        yield return Missing("YOGA_DARIDRA", "PVR_CH11_DARIDRA_10", DaridraLocator,
            "\"Benefics are in malefic houses and malefics are in benefic houses\" — PVR's own text forward-references the Bhinnashtakavarga benefic/malefic house tables introduced in the immediately following chapter (§12.2), without restating an operational definition here. Not guessed.");

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_11", DaridraLocator,
            "A planet conjoined with a 6th/8th/12th lord, itself not conjoined or aspected by a trine (1st/5th/9th) lord, risks loss of wealth in its dasa.",
            c.Planets.Any(pl => Enum.TryParse<PlanetName>(pl.Planet, out var pn) && Classical7.Contains(pn) &&
                new[] { l6, l8, l12 }.Any(dl => dl != pn && Conjunct(c, pn, dl)) &&
                !new[] { l1, l5, l9 }.Any(tl => Influenced(c, pn, tl))));

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_12", DaridraLocator,
            "Mars and Saturn are in the 2nd, and Mercury does not aspect them.",
            InHouse(c, PlanetName.Mars, 2) && InHouse(c, PlanetName.Saturn, 2) && !AspectsOnly(c, PlanetName.Mercury, HouseSign(c, 2)));

        yield return Row("YOGA_DARIDRA", "PVR_CH11_DARIDRA_13", DaridraLocator,
            "Sun in the 2nd is aspected by Saturn.",
            InHouse(c, PlanetName.Sun, 2) && AspectsOnly(c, PlanetName.Saturn, HouseSign(c, 2)));
    }

    // ---------------- shared helpers ----------------
    private static bool Qualifies(ChartAnalysisInput c, PlanetName p) =>
        Dignity(c, p) is "OWN" or "MOOLATRIKONA" or "EXALTED" || AspectedByBenefic(c, p);
    private static bool AspectedByBenefic(ChartAnalysisInput c, PlanetName target) =>
        B.Any(b => b != target && Influenced(c, target, b));
    private static bool BeneficsInRelative(ChartAnalysisInput c, PlanetName reference, params int[] relatives)
    {
        var refHouse = Find(c, reference)?.HouseNumber; if (refHouse is null) return false;
        return relatives.All(n => OccupiedBy(c, RelativeHouse(refHouse.Value, n), B));
    }
    private static bool MalefictedRelative(ChartAnalysisInput c, PlanetName reference, params int[] relatives)
    {
        var refHouse = Find(c, reference)?.HouseNumber; if (refHouse is null) return false;
        return relatives.All(n =>
        {
            var h = RelativeHouse(refHouse.Value, n);
            return OccupiedBy(c, h, M) || M.Any(m => InfluencesSign(c, m, HouseSign(c, h)));
        });
    }
    private static bool MalefictsIn36From(ChartAnalysisInput c, int refHouse) =>
        OccupiedBy(c, RelativeHouse(refHouse, 3), M) && OccupiedBy(c, RelativeHouse(refHouse, 6), M);
    private static bool Relates(ChartAnalysisInput c, PlanetName x, string sign)
    {
        var pos = Find(c, x);
        return (pos is not null && (pos.Sign == sign || Aspects(x, pos.Sign, sign))) ||
               Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(sign))) == x;
    }
    private static bool OccupiedByStrongPlanet(ChartAnalysisInput c, string sign) =>
        c.Planets.Any(p => Enum.TryParse<PlanetName>(p.Planet, out var pn) && Classical7.Contains(pn) && p.Sign == sign && Dignity(c, pn) is "OWN" or "MOOLATRIKONA" or "EXALTED");
    private static bool OccupiedByStrongPlanetInChart(ChartAnalysisInput chart, string sign) =>
        chart.Planets.Any(p => Enum.TryParse<PlanetName>(p.Planet, out var pn) && Classical7.Contains(pn) && p.Sign == sign &&
            PvrDignityEvaluator.Evaluate(pn, Enum.Parse<ZodiacName>(p.Sign), ((p.NirayanaLongitudeDegrees ?? 15) % 30 + 30) % 30).DignityTypeCode is "OWN" or "MOOLATRIKONA" or "EXALTED");
    private static bool IsMaraka(ChartAnalysisInput c, PlanetName p)
    {
        var l2 = Lord(c, 2); var l7 = Lord(c, 7);
        if (p == l2 || p == l7) return true;
        if (M.Contains(p) && Find(c, p) is { } x && x.HouseNumber is 2 or 7) return true;
        if (M.Contains(p) && (Influenced(c, l2, p) || Influenced(c, l7, p))) return true;
        return false;
    }
    private static PlanetName[] MarakaSet(ChartAnalysisInput c) => Classical7.Where(p => IsMaraka(c, p)).ToArray();
    private static PlanetName? Dispositor(ChartAnalysisInput c, PlanetName p) =>
        Find(c, p) is { } x ? Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(x.Sign))) : null;
    private static PlanetName? NavamsaLord(ChartAnalysisInput d9, PlanetName p)
    {
        var x = Find(d9, p); return x is null ? null : Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(x.Sign)));
    }
    private static PlanetName? KarakaPlanet(ChartBundle bundle, string code)
    {
        var kv = bundle.CharaKarakaByPlanet.FirstOrDefault(x => x.Value == code);
        return kv.Key is null ? null : Enum.Parse<PlanetName>(kv.Key);
    }
    private static int RelativeHouse(int refHouse, int n) => ((refHouse - 1 + n - 1) % 12) + 1;
    private static int HouseDistance(int a, int b) => ((a - b + 12) % 12) + 1;
    private static bool InHouse(ChartAnalysisInput c, PlanetName p, int h) => Find(c, p)?.HouseNumber == h;
    private static bool OccupiedBy(ChartAnalysisInput c, int house, PlanetName[] set) =>
        c.Planets.Any(p => Enum.TryParse<PlanetName>(p.Planet, out var pn) && set.Contains(pn) && p.HouseNumber == house);
    private static bool Conjunct(ChartAnalysisInput c, PlanetName a, PlanetName b) => Find(c, a) is { } x && Find(c, b) is { } y && x.Sign == y.Sign;
    private static bool Influenced(ChartAnalysisInput c, PlanetName target, PlanetName source) =>
        Find(c, target) is { } x && Find(c, source) is { } y && (x.Sign == y.Sign || Aspects(source, y.Sign, x.Sign));
    private static bool InfluencesSign(ChartAnalysisInput c, PlanetName source, string sign) =>
        Find(c, source) is { } x && (x.Sign == sign || Aspects(source, x.Sign, sign));
    private static bool AspectsOnly(ChartAnalysisInput c, PlanetName source, string sign) => Find(c, source) is { } x && Aspects(source, x.Sign, sign);
    private static bool Aspects(PlanetName p, string a, string b)
    {
        var d = Distance(a, b);
        return d == 7 || (p == PlanetName.Mars && d is 4 or 8) || (p == PlanetName.Jupiter && d is 5 or 9) || (p == PlanetName.Saturn && d is 3 or 10);
    }
    private static int Distance(string? a, string? b) =>
        a is null || b is null ? 0 : (((int)Enum.Parse<ZodiacName>(b) - (int)Enum.Parse<ZodiacName>(a) + 12) % 12) + 1;
    private static bool IsCombust(ChartAnalysisInput c, PlanetName p)
    {
        if (!CombustionEngine.IsApplicable(p.ToString())) return false;
        var planet = Find(c, p); var sun = Find(c, PlanetName.Sun);
        if (planet?.NirayanaLongitudeDegrees is not double pl || sun?.NirayanaLongitudeDegrees is not double sl) return false;
        return CombustionEngine.Evaluate(p.ToString(), pl, sl, planet.IsRetrograde).IsCombust;
    }
    private static bool Strong(ChartAnalysisInput c, PlanetName p) => Dignity(c, p) is "OWN" or "MOOLATRIKONA" or "EXALTED";
    private static string Dignity(ChartAnalysisInput c, PlanetName p)
    {
        var x = Find(c, p); if (x is null) return "MISSING";
        var l = x.VargaLongitudeDegrees ?? x.NirayanaLongitudeDegrees ?? 15;
        return PvrDignityEvaluator.Evaluate(p, Enum.Parse<ZodiacName>(x.Sign), ((l % 30) + 30) % 30).DignityTypeCode;
    }
    private static PvrDignityResult FullDignity(ChartAnalysisInput c, PlanetName p)
    {
        var x = Find(c, p); if (x is null) return new("MISSING", 0, "", null, null, 0);
        var lon = x.VargaLongitudeDegrees ?? x.NirayanaLongitudeDegrees ?? 15;
        var signs = c.Planets.Where(v => Enum.TryParse<PlanetName>(v.Planet, out _)).ToDictionary(v => v.Planet, v => Enum.Parse<ZodiacName>(v.Sign), StringComparer.OrdinalIgnoreCase);
        return PvrDignityEvaluator.Evaluate(p, Enum.Parse<ZodiacName>(x.Sign), ((lon % 30) + 30) % 30, signs);
    }
    private static string CompoundRelationship(ChartAnalysisInput c, PlanetName p) => FullDignity(c, p).CompoundRelationshipCode ?? "NEUTRAL";
    private static PlanetName Lord(ChartAnalysisInput c, int h) => Enum.Parse<PlanetName>(HouseEngine.GetSignLord(HouseEngine.GetHouseSign(c.AscendantSign, h)));
    private static string HouseSign(ChartAnalysisInput c, int h) => HouseEngine.GetHouseSign(c.AscendantSign, h).ToString();
    private static int HouseFromSign(ChartAnalysisInput c, string sign) =>
        ((int)Enum.Parse<ZodiacName>(sign) - (int)c.AscendantSign + 12) % 12 + 1;
    private static string? SpecialSign(ChartAnalysisInput c, string specialPoint) => Find(c, specialPoint)?.Sign;
    private static int? SpecialHouse(ChartAnalysisInput c, string specialPoint) =>
        SpecialSign(c, specialPoint) is { } sign ? HouseFromSign(c, sign) : null;
    private static PlanetPosition? Find(ChartAnalysisInput c, PlanetName p) => c.Planets.SingleOrDefault(x => x.Planet == p.ToString());
    private static PlanetPosition? Find(ChartAnalysisInput c, string name) => c.Planets.SingleOrDefault(x => x.Planet == name);
    private static ContextualYogaResult Row(string code, string variant, string locator, string notes, bool present) =>
        new(code, present, "EVALUATED", "SRC_PVR_INTEGRATED", variant, locator, notes);
    private static ContextualYogaResult Missing(string code, string variant, string locator, string notes) =>
        new(code, null, "NOT_EVALUATED", "SRC_PVR_INTEGRATED", variant, locator, notes);
    private static ContextualYogaResult RowOrMissing(string code, string variant, string locator, string notes, bool? present) =>
        present is null ? Missing(code, variant, locator, notes + " (missing a required input.)") : Row(code, variant, locator, notes, present.Value);
}
