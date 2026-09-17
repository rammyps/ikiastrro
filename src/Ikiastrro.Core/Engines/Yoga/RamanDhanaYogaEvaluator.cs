using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Yoga;

/// <summary>Raman combinations 118–143; unsupported source-strength qualifiers remain explicit.</summary>
public static class RamanDhanaYogaEvaluator
{
    public static IReadOnlyList<ContextualYogaResult> Evaluate(ChartBundle bundle)
    {
        var c=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D1",StringComparison.OrdinalIgnoreCase));
        if(c is null)return [];
        var d9=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D9",StringComparison.OrdinalIgnoreCase));
        var isNightBirth=bundle.SunTimes.IsNightBirth;
        return
        [
            Row(118,135,147,Dhana118(c)),Row(119,135,147,Dhana119(c)),Row(120,135,147,Dhana120(c)),
            Row(121,135,147,Dhana121(c)),Row(122,136,148,Dhana122(c)),
            Row(123,140,152,OwnLagnaInfluenced(c,PlanetName.Sun,PlanetName.Mars,PlanetName.Jupiter)),
            Row(124,140,152,OwnLagnaInfluenced(c,PlanetName.Moon,PlanetName.Jupiter,PlanetName.Mars)),
            Row(125,140,152,OwnLagnaInfluenced(c,PlanetName.Mars,PlanetName.Moon,PlanetName.Venus,PlanetName.Saturn)),
            Row(126,140,152,OwnLagnaInfluenced(c,PlanetName.Mercury,PlanetName.Saturn,PlanetName.Venus)),
            Row(127,140,152,OwnLagnaInfluenced(c,PlanetName.Jupiter,PlanetName.Mercury,PlanetName.Mars)),
            Row(128,140,152,OwnLagnaInfluenced(c,PlanetName.Venus,PlanetName.Saturn,PlanetName.Mercury)),
            Named("YOGA_BAHUDRAVYARJANA",129,143,155,Bahudravyarjana(c)),
            Row(130,144,156,Dhana130(c,bundle)),
            d9 is null?Missing(131,144,156,"D9 placement is required."):Row(131,144,156,Dhana131(c,d9)),
            Dhana132(c),
            Row(133,146,158,Dhana133(c,isNightBirth)),
            Row(134,147,159,Dhana134(c)),
            d9 is null?Missing(135,147,159,"D9 placement is required."):Row(135,147,159,Dhana135(c,d9)),
            Row(136,148,160,Dhana136(c)),
            Row(137,148,160,Dhana137(c,bundle)),
            Named("YOGA_MATRUMOOLA_DHANA",138,149,161,Associated(c,Lord(c,2),Lord(c,4))),
            Row(139,149,161,Dhana139(c,bundle)),
            Row(140,151,163,Dhana140(c,bundle)),
            Row(141,151,163,Dhana141(c)),
            Row(142,151,163,Dhana142(c)),
            Named("YOGA_AYATNA_DHANA_LABHA",143,152,164,Exchange(c,Lord(c,1),1,Lord(c,2),2))
        ];
    }
    private static bool Dhana118(ChartAnalysisInput c)=>Lord(c,5)==PlanetName.Venus&&Find(c,PlanetName.Venus)?.HouseNumber==5&&Find(c,PlanetName.Saturn)?.HouseNumber==11;
    private static bool Dhana119(ChartAnalysisInput c)=>Lord(c,5)==PlanetName.Mercury&&Find(c,PlanetName.Mercury)?.HouseNumber==5&&Find(c,PlanetName.Moon)?.HouseNumber==11&&Find(c,PlanetName.Mars)?.HouseNumber==11;
    private static bool Dhana120(ChartAnalysisInput c)=>Lord(c,5)==PlanetName.Saturn&&Find(c,PlanetName.Saturn)?.HouseNumber==5&&Find(c,PlanetName.Mercury)?.HouseNumber==11&&Find(c,PlanetName.Mars)?.HouseNumber==11;
    private static bool Dhana121(ChartAnalysisInput c)=>Lord(c,5)==PlanetName.Sun&&Find(c,PlanetName.Sun)?.HouseNumber==5&&Find(c,PlanetName.Jupiter)?.HouseNumber==11&&Find(c,PlanetName.Moon)?.HouseNumber==11;
    private static bool Dhana122(ChartAnalysisInput c)=>Lord(c,5)==PlanetName.Jupiter&&Find(c,PlanetName.Jupiter)?.HouseNumber==5&&Find(c,PlanetName.Mars)?.HouseNumber==11&&Find(c,PlanetName.Moon)?.HouseNumber==11;
    private static bool OwnLagnaInfluenced(ChartAnalysisInput c,PlanetName occupant,params PlanetName[] influences)
    {
        var p=Find(c,occupant);if(p?.HouseNumber!=1||Lord(c,1)!=occupant)return false;
        return influences.All(x=>Influences(c,x,p.Sign));
    }
    private static bool Bahudravyarjana(ChartAnalysisInput c)=>Find(c,Lord(c,1))?.HouseNumber==2&&Find(c,Lord(c,2))?.HouseNumber==11&&Find(c,Lord(c,11))?.HouseNumber==1;
    private static ContextualYogaResult Dhana132(ChartAnalysisInput c)
    {
        var l1=Find(c,Lord(c,1));var l2=Find(c,Lord(c,2));
        if(l1 is not null&&l2 is not null&&Distance(l1.Sign,l2.Sign) is 1 or 4 or 5 or 7 or 9 or 10)return Row(132,144,156,true);
        var planet=Lord(c,2);
        if(IsNaturalBenefic(planet)&&DeeplyExalted(c,planet))return Row(132,144,156,true);
        return Missing(132,144,156,"If the positional alternative is absent, source-qualified benefic/strength evidence is required.");
    }

    // Combination 131: Lagna lord's D9 sign-lord's D1 sign-lord must be strong (own/moolatrikona/
    // exalted/favourable relationship) and either in a kendra/trikona from the 2nd lord or strictly
    // in his own/exaltation sign. Raman: "lord of the sign in which the lord of the Navamsa occupied
    // by the Ascendant lord is, should be strong, and join a quadrant or a trine from the 2nd lord or
    // should occupy his own or exaltation sign." "Strong" is read broadly (Favoured) so the
    // kendra/trikona-vs-own/exaltation alternative remains meaningful rather than redundant.
    private static bool Dhana131(ChartAnalysisInput c,ChartAnalysisInput d9)
    {
        var lagnaLordD9=Find(d9,Lord(c,1));if(lagnaLordD9 is null)return false;
        var navamsaLord=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(lagnaLordD9.Sign)));
        var navamsaLordD1=Find(c,navamsaLord);if(navamsaLordD1 is null)return false;
        var finalLord=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(navamsaLordD1.Sign)));
        var finalPosition=Find(c,finalLord);if(finalPosition is null)return false;
        if(!Favoured(c,finalLord))return false;
        var secondLordSign=Find(c,Lord(c,2))?.Sign;
        var kendraTrikona=secondLordSign is not null&&Distance(secondLordSign,finalPosition.Sign) is 1 or 4 or 5 or 7 or 9 or 10;
        return kendraTrikona||Strong(c,finalLord);
    }

    // Combination 133 (Madhya Vayasi Dhana Yoga): the 2nd lord, positioned in a kendra/trikona,
    // joins or is aspected by the lords of Lagna and the 11th, and is aspected by a benefic, while
    // itself "possessing Kalabala". Raman's own remarks give an operational definition of Kalabala
    // here rather than a numeric threshold: Moon/Mars/Saturn hold it by night, Sun/Jupiter/Venus by
    // day, Mercury always — used directly instead of the still-unbuilt numeric Kalabala component.
    private static bool Dhana133(ChartAnalysisInput c,bool isNightBirth)
    {
        var secondLord=Lord(c,2);
        if(!HasKalabala(secondLord,isNightBirth))return false;
        var position=Find(c,secondLord);
        if(position is null||position.HouseNumber is not(1 or 4 or 5 or 7 or 9 or 10))return false;
        if(!JoinedOrAspected(c,secondLord,Lord(c,1))||!JoinedOrAspected(c,secondLord,Lord(c,11)))return false;
        return AspectedByBenefic(c,secondLord);
    }
    private static bool HasKalabala(PlanetName planet,bool isNightBirth)=>planet switch
    {
        PlanetName.Mercury=>true,
        PlanetName.Moon or PlanetName.Mars or PlanetName.Saturn=>isNightBirth,
        PlanetName.Sun or PlanetName.Jupiter or PlanetName.Venus=>!isNightBirth,
        _=>false
    };

    // Combination 134 (Anthya Vayasi Dhana Yoga): the Lagna lord, 2nd lord and a (third, natural
    // benefic) planet share a sign; that sign's lord must be strong and posted in Lagna. Raman:
    // "the planets owning the sign in which the lords of the 2nd and 1st together with a natural
    // benefic are placed, should be strongly disposed in Lagna."
    private static bool Dhana134(ChartAnalysisInput c)
    {
        var lagnaLord=Lord(c,1);var secondLord=Lord(c,2);
        var lagnaLordPosition=Find(c,lagnaLord);var secondLordPosition=Find(c,secondLord);
        if(lagnaLordPosition is null||secondLordPosition is null||lagnaLordPosition.Sign!=secondLordPosition.Sign)return false;
        var hasThirdBenefic=c.Planets.Any(p=>Enum.TryParse<PlanetName>(p.Planet,out var pn)&&IsNaturalBenefic(pn)&&pn!=lagnaLord&&pn!=secondLord&&p.Sign==lagnaLordPosition.Sign);
        if(!hasThirdBenefic)return false;
        var signLord=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(lagnaLordPosition.Sign)));
        var signLordPosition=Find(c,signLord);
        return signLordPosition?.HouseNumber==1&&Strong(c,signLord);
    }

    private static bool Dhana135(ChartAnalysisInput c,ChartAnalysisInput d9)
    {
        var l1=Lord(c,1);var l2=Lord(c,2);var l10=Lord(c,10);var p1=Find(c,l1);var p2=Find(c,l2);var p10=Find(c,l10);
        if(p1 is null||p2 is null||p10 is null||p2.Sign!=p10.Sign||p2.HouseNumber is not(1 or 4 or 7 or 10))return false;
        var l1d9=Find(d9,l1);if(l1d9 is null)return false;
        var nl=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(l1d9.Sign)));var np=Find(c,nl);
        return np is not null&&Aspects(nl,np.Sign,p2.Sign);
    }
    private static bool Dhana136(ChartAnalysisInput c)
    {
        var l1=Find(c,Lord(c,1));var l2=Find(c,Lord(c,2));if(l1?.HouseNumber!=3||l2?.HouseNumber!=3)return false;
        return new[]{PlanetName.Moon,PlanetName.Mercury,PlanetName.Jupiter,PlanetName.Venus}.Any(p=>Influences(c,p,l1.Sign));
    }

    // Combination 141 (Kalatramooladdhana Yoga): the strong 2nd lord joins or is aspected by the
    // 7th lord and Venus, and the Lagna lord is powerful. Raman: "the strong lord of the 2nd should
    // join or be aspected by the 7th lord and Venus and the lord of Lagna must be powerful." No
    // Vaiseshikamsa qualifier here — "strong"/"powerful" read as own/moolatrikona/exalted, the same
    // convention used throughout this project's other Raman evaluators.
    private static bool Dhana141(ChartAnalysisInput c)
    {
        var secondLord=Lord(c,2);
        if(!Strong(c,secondLord))return false;
        if(!JoinedOrAspected(c,secondLord,Lord(c,7))||!JoinedOrAspected(c,secondLord,PlanetName.Venus))return false;
        return Strong(c,Lord(c,1));
    }

    // Combination 142 (Amaranantha Dhana Yoga): several planets occupy the 2nd house, and the
    // wealth-giving planets (2nd lord and Jupiter) are strong. Raman: "if a number of planets occupy
    // the 2nd house and the wealth-giving ones are strong or occupy own or exaltation signs..."
    private static bool Dhana142(ChartAnalysisInput c)
    {
        var occupantsInSecond=c.Planets.Count(p=>Enum.TryParse<PlanetName>(p.Planet,out var pn)&&pn is not PlanetName.Rahu and not PlanetName.Ketu&&p.HouseNumber==2);
        if(occupantsInSecond<2)return false;
        return Strong(c,Lord(c,2))&&Strong(c,PlanetName.Jupiter);
    }

    // Combination 130 (Swaveerya Dhana Yoga): the Lagna lord must be the strongest planet in the
    // horoscope, occupy a kendra, and be conjoined with Jupiter; the 2nd lord must have attained
    // Vaiseshikamsa. Raman: "the ascendant lord must be the strongest planet in the horoscope and
    // he should be in a kendra in conjunction with Jupiter and 2nd lord in Vaiseshikamsa." Strength
    // is compared via each graha's PVR dignity score (the only source-registered per-planet
    // strength figure available at this layer) rather than full Shadbala.
    private static bool Dhana130(ChartAnalysisInput c,ChartBundle bundle)
    {
        var lagnaLord=Lord(c,1);
        if(!IsStrongestPlanet(c,lagnaLord))return false;
        if(Find(c,lagnaLord)?.HouseNumber is not(1 or 4 or 7 or 10))return false;
        if(!Conjunct(c,lagnaLord,PlanetName.Jupiter))return false;
        return VaiseshikamsaCalculator.HasVaiseshikamsa(bundle.Charts,Lord(c,2));
    }

    // Combination 137 (Bhratrumooladdhanaprapti Yoga, 2nd form): the 3rd lord joins Jupiter in the
    // 2nd, and is aspected by or conjoined with the Lagna lord, who must have attained
    // Vaiseshikamsa. Raman: "the lord of the 3rd should be in the 2nd with Jupiter and aspected by
    // or conjoined with the lord of Lagna who should have attained Vaiseshikamsa."
    private static bool Dhana137(ChartAnalysisInput c,ChartBundle bundle)
    {
        var thirdLord=Lord(c,3);
        if(Find(c,thirdLord)?.HouseNumber!=2||!Conjunct(c,thirdLord,PlanetName.Jupiter))return false;
        if(!JoinedOrAspected(c,thirdLord,Lord(c,1)))return false;
        return VaiseshikamsaCalculator.HasVaiseshikamsa(bundle.Charts,Lord(c,1));
    }

    // Combination 139 (Putramooladdhana Yoga): the strong 2nd lord joins the 5th lord or Jupiter,
    // and the Lagna lord is in Vaiseshikamsa. Raman: "if the strong lord of the 2nd is in
    // conjunction with the 5th lord or Jupiter and if the lord of Lagna is in Vaiseshikamsa."
    private static bool Dhana139(ChartAnalysisInput c,ChartBundle bundle)
    {
        var secondLord=Lord(c,2);
        if(!Strong(c,secondLord))return false;
        if(!Conjunct(c,secondLord,Lord(c,5))&&!Conjunct(c,secondLord,PlanetName.Jupiter))return false;
        return VaiseshikamsaCalculator.HasVaiseshikamsa(bundle.Charts,Lord(c,1));
    }

    // Combination 140 (Satrumooladdhana Yoga): the strong 2nd lord joins (or, per Raman's own
    // remarks, is aspected by) the 6th lord or Mars, and the powerful Lagna lord is in
    // Vaiseshikamsa. Raman: "the strong lord of the 2nd should join the lord of the 6th or Mars
    // and the powerful lord of Lagna should be in Vaiseshikamsa."
    private static bool Dhana140(ChartAnalysisInput c,ChartBundle bundle)
    {
        var secondLord=Lord(c,2);
        if(!Strong(c,secondLord))return false;
        if(!JoinedOrAspected(c,secondLord,Lord(c,6))&&!JoinedOrAspected(c,secondLord,PlanetName.Mars))return false;
        return VaiseshikamsaCalculator.HasVaiseshikamsa(bundle.Charts,Lord(c,1));
    }

    private static readonly PlanetName[] AllGrahas=
    [
        PlanetName.Sun,PlanetName.Moon,PlanetName.Mars,PlanetName.Mercury,
        PlanetName.Jupiter,PlanetName.Venus,PlanetName.Saturn,PlanetName.Rahu,PlanetName.Ketu
    ];
    private static bool IsStrongestPlanet(ChartAnalysisInput c,PlanetName p)=>AllGrahas.All(x=>Dignity(c,x).DignityScore<=Dignity(c,p).DignityScore);
    private static bool Conjunct(ChartAnalysisInput c,PlanetName a,PlanetName b){var pa=Find(c,a);var pb=Find(c,b);return pa is not null&&pb is not null&&pa.Sign==pb.Sign;}
    private static bool Associated(ChartAnalysisInput c,PlanetName a,PlanetName b){var x=Find(c,a);var y=Find(c,b);return x is not null&&y is not null&&(x.Sign==y.Sign||Aspects(b,y.Sign,x.Sign));}
    private static bool Exchange(ChartAnalysisInput c,PlanetName a,int ah,PlanetName b,int bh)=>a!=b&&Find(c,a)?.HouseNumber==bh&&Find(c,b)?.HouseNumber==ah;
    private static bool Influences(ChartAnalysisInput c,PlanetName p,string target){var x=Find(c,p);return x is not null&&(x.Sign==target||Aspects(p,x.Sign,target));}
    private static bool JoinedOrAspected(ChartAnalysisInput c,PlanetName a,PlanetName b)
    {
        var pa=Find(c,a);var pb=Find(c,b);if(pa is null||pb is null)return false;
        return pa.Sign==pb.Sign||Aspects(a,pa.Sign,pb.Sign)||Aspects(b,pb.Sign,pa.Sign);
    }
    private static bool AspectedByBenefic(ChartAnalysisInput c,PlanetName target)
    {
        var position=Find(c,target);if(position is null)return false;
        return new[]{PlanetName.Moon,PlanetName.Mercury,PlanetName.Jupiter,PlanetName.Venus}
            .Where(p=>p!=target).Any(p=>Find(c,p) is{}pos&&Aspects(p,pos.Sign,position.Sign));
    }
    private static bool Aspects(PlanetName p,string a,string b){var d=Distance(a,b);return d==7||p==PlanetName.Mars&&d is 4 or 8||p==PlanetName.Jupiter&&d is 5 or 9||p==PlanetName.Saturn&&d is 3 or 10;}
    private static int Distance(string a,string b)=>(((int)Enum.Parse<ZodiacName>(b)-(int)Enum.Parse<ZodiacName>(a)+12)%12)+1;
    private static bool IsNaturalBenefic(PlanetName p)=>p is PlanetName.Moon or PlanetName.Mercury or PlanetName.Jupiter or PlanetName.Venus;
    // Was a fourth independent hardcoded exaltation-degree dictionary (found alongside the three
    // named in the 2026-09-11 rule-mapping audit); now reads AstroMath.DeepExaltationPoints.
    private static bool DeeplyExalted(ChartAnalysisInput c,PlanetName p)
    {
        var (sign,degree)=AstroMath.DeepExaltationPoints[p];
        var exact=(int)sign*30+degree;
        var x=Find(c,p);return x?.NirayanaLongitudeDegrees is double lon&&Math.Abs(lon-exact)<.000001;
    }
    private static PvrDignityResult Dignity(ChartAnalysisInput c,PlanetName p)
    {
        var x=Find(c,p);if(x is null)return new("MISSING",0,"",null,null,0);
        var lon=x.VargaLongitudeDegrees??x.NirayanaLongitudeDegrees;
        return PvrDignityEvaluator.Evaluate(p,Enum.Parse<ZodiacName>(x.Sign),lon is null?15:((lon.Value%30)+30)%30);
    }
    private static bool Strong(ChartAnalysisInput c,PlanetName p)=>Dignity(c,p).DignityTypeCode is "OWN" or "MOOLATRIKONA" or "EXALTED";
    private static bool Favoured(ChartAnalysisInput c,PlanetName p){var d=Dignity(c,p);return d.DignityTypeCode is "OWN" or "MOOLATRIKONA" or "EXALTED"||d.RelationshipScore>0;}
    private static PlanetName Lord(ChartAnalysisInput c,int h)=>Enum.Parse<PlanetName>(HouseEngine.GetSignLord(HouseEngine.GetHouseSign(c.AscendantSign,h)));
    private static PlanetPosition? Find(ChartAnalysisInput c,PlanetName p)=>c.Planets.SingleOrDefault(x=>x.Planet==p.ToString());
    private static ContextualYogaResult Row(int n,int printed,int scan,bool present)=>Named("YOGA_DHANA",n,printed,scan,present);
    private static ContextualYogaResult Named(string code,int n,int printed,int scan,bool present)=>new(code,present,"EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{printed}; scan p.{scan}");
    private static ContextualYogaResult Missing(int n,int printed,int scan,string note)=>new("YOGA_DHANA",null,"NOT_EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{printed}; scan p.{scan}",note);
}
