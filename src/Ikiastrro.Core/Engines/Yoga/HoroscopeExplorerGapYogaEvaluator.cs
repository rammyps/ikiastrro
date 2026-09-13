using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Yoga;

/// <summary>Source-separated evaluation of the four Horoscope Explorer comparison gaps.</summary>
public static class HoroscopeExplorerGapYogaEvaluator
{
    private static readonly string[] NineGrahas = ["Sun","Moon","Mars","Mercury","Jupiter","Venus","Saturn","Rahu","Ketu"];
    public static IReadOnlyList<ContextualYogaResult> Evaluate(ChartAnalysisInput d1) =>
    [
        new("YOGA_VIPAREETA_RAJA", VipareetaRaja(d1), "EVALUATED", "SRC_PVR_INTEGRATED", "PVR_CH11_VIPAREETA_RAJA", "ch.11 §11.7.1; printed pp.134-135", "Broad PVR form: at least one lord of 6, 8 or 12 occupies a dusthana; the ideal form is stricter."),
        new("YOGA_ANIVAHUPPU", Anivahuppu(d1), "EVALUATED", "SRC_HOROSCOPE_EXPLORER", "HE_ANIVAHUPPU_STRUCTURAL", "Horoscope Explorer comparison report; Ram_HorExp_Yogas1.png", "Nine-graha structural rule: an anchor is alone in its sign, with four grahas in each open semicircle around it."),
        new("YOGA_VIDYA", null, "NOT_EVALUATED", "SRC_HOROSCOPE_EXPLORER", "HE_VIDYA_NATAL", "Horoscope Explorer comparison report; Ram_HorExp_Yogas1.png", "The report supplies effects but no natal predicate. Raman's located Vidya Yoga is a muhurta rule and must not be substituted."),
        new("YOGA_ARISHTA", null, "NOT_EVALUATED", "SRC_HOROSCOPE_EXPLORER", "HE_ARISHTA_GENERIC", "Horoscope Explorer comparison report; Ram_HorExp_Yogas2.png", "Arishta is a class of adverse combinations; the report does not identify which predicate fired.")
    ];
    private static bool VipareetaRaja(ChartAnalysisInput c) => new[] { 6, 8, 12 }.Select(h => Find(c, Lord(c, h))).Any(p => p?.HouseNumber is 6 or 8 or 12);
    private static bool Anivahuppu(ChartAnalysisInput c)
    {
        var planets=c.Planets.Where(p=>NineGrahas.Contains(p.Planet)).ToArray();
        if(planets.Length!=9)return false;
        foreach(var anchor in planets){if(planets.Count(p=>p.Sign==anchor.Sign)!=1)continue;var ahead=0;var behind=0;var opposite=0;var a=(int)Enum.Parse<ZodiacName>(anchor.Sign);
            foreach(var other in planets.Where(p=>p!=anchor)){var delta=((int)Enum.Parse<ZodiacName>(other.Sign)-a+12)%12;if(delta==6)opposite++;else if(delta<6)ahead++;else behind++;}
            if((ahead==4&&behind+opposite==4)||(behind==4&&ahead+opposite==4))return true;}
        return false;
    }
    private static PlanetName Lord(ChartAnalysisInput c,int house)=>Enum.Parse<PlanetName>(HouseEngine.GetSignLord(HouseEngine.GetHouseSign(c.AscendantSign,house)));
    private static PlanetPosition? Find(ChartAnalysisInput c,PlanetName planet)=>c.Planets.SingleOrDefault(p=>p.Planet==planet.ToString());
}
