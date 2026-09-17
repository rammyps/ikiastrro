using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Core.Engines.Yoga;

/// <summary>Raman 201–219 — siblings, deceit/curses on parents &amp; children, vehicles,
/// childlessness. Cluster A of the 201–300 catalog's phased build-out (bvyoga.md §4),
/// transcribed out of RamanFinalHundredCatalog.cs the same way Clusters B/C were.</summary>
public static class RamanFamilyYogaEvaluator
{
 private static readonly PlanetName[] B=[PlanetName.Moon,PlanetName.Mercury,PlanetName.Jupiter,PlanetName.Venus];
 private static readonly PlanetName[] M=[PlanetName.Sun,PlanetName.Mars,PlanetName.Saturn];
 private static readonly PlanetName[] Classical=
  [PlanetName.Sun,PlanetName.Moon,PlanetName.Mars,PlanetName.Mercury,PlanetName.Jupiter,PlanetName.Venus,PlanetName.Saturn];
 private static readonly int[] KendraTrikona=[1,4,5,7,9,10];
 private static readonly Dictionary<int,string> Codes=new()
 {
  [201]="YOGA_SAHODAREE_SANGAMA",
  [202]="YOGA_KAPATA",[203]="YOGA_KAPATA",[204]="YOGA_KAPATA",
  [205]="YOGA_NISHKAPATA",[206]="YOGA_NISHKAPATA",
  [207]="YOGA_MATRU_SATRUTWA",[208]="YOGA_MATRU_SNEHA",
  [209]="YOGA_VAHANA",[210]="YOGA_VAHANA",
  [211]="YOGA_ANAPATHYA",
  [212]="YOGA_SARPASAPA",[213]="YOGA_SARPASAPA",[214]="YOGA_SARPASAPA",[215]="YOGA_SARPASAPA",
  [216]="YOGA_PITRUSAPA_SUTAKSHAYA",[217]="YOGA_MATRUSAPA_SUTAKSHAYA",
  [218]="YOGA_BHRATRUSAPA_SUTAKSHAYA",[219]="YOGA_PRETASAPA"
 };

 public static IReadOnlyList<ContextualYogaResult> Evaluate(ChartBundle bundle)
 {
  var c=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D1",StringComparison.OrdinalIgnoreCase));if(c is null)return[];
  var d9=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D9",StringComparison.OrdinalIgnoreCase));
  var charts=bundle.Charts;
  bool? v(int n)=>n switch
  {
   201=>Y201(c),
   202=>Y202(c),203=>Y203(c),204=>Y204(c),
   205=>Y205(c),206=>Y206(c,charts),
   207=>Y207(c),208=>Y208(c),
   209=>Y209(c),210=>Y210(c),
   211=>Y211(c),
   212=>Y212(c),213=>Y213(c),214=>Y214(c),215=>Y215(c),
   216=>Y216(c,d9),
   217=>Y217(c),218=>Y218(c),219=>Y219(c),
   _=>null
  };
  return Enumerable.Range(201,19)
   .Select(n=>v(n) is{}r?Row(n,r):Missing(n,"Required divisional or qualification evidence is unavailable."))
   .ToList();
 }

 // 201 (Sahodareesangama): "If the lord of the 7th and Venus are in conjunction in the 4th
 // house and are aspected by or associated with malefics or are in cruel shashtiamsas."
 private static bool Y201(ChartAnalysisInput c)
 {
  var l7=Lord(c,7);
  if(Find(c,l7)?.HouseNumber!=4||Find(c,PlanetName.Venus)?.HouseNumber!=4)return false;
  return M.Any(m=>Influenced(c,l7,m)||Influenced(c,PlanetName.Venus,m))||CruelShashtiamsa(c,l7)||CruelShashtiamsa(c,PlanetName.Venus);
 }

 // 202 (Kapata, 1st form): "The 4th house must be joined by a malefic and the 4th lord must be
 // associated with or aspected by malefics or be hemmed in between malefics."
 private static bool Y202(ChartAnalysisInput c)
 {
  var l4=Lord(c,4);
  if(!M.Any(m=>Find(c,m)?.HouseNumber==4))return false;
  return M.Any(m=>Influenced(c,l4,m))||HemmedByMalefics(c,Find(c,l4)?.HouseNumber??0);
 }

 // 203 (Kapata, 2nd form): "The 4th must be occupied by Sani, Kuja, Rahu and the malefic 10th
 // lord, who in his turn should be aspected by malefics."
 private static bool Y203(ChartAnalysisInput c)
 {
  if(Find(c,PlanetName.Saturn)?.HouseNumber!=4||Find(c,PlanetName.Mars)?.HouseNumber!=4||Find(c,PlanetName.Rahu)?.HouseNumber!=4)return false;
  var l10=Lord(c,10);
  return M.Contains(l10)&&M.Any(m=>Influenced(c,l10,m));
 }

 // 204 (Kapata, 3rd form): "The 4th lord must join Saturn, Mandi and Rahu and aspected by
 // malefics." ("Mandi" = the engine's "Maandi" upagraha.)
 private static bool Y204(ChartAnalysisInput c)
 {
  var l4=Lord(c,4);
  if(!Conjunct(c,l4,PlanetName.Saturn)||!Conjunct(c,l4,"Maandi")||!Conjunct(c,l4,PlanetName.Rahu))return false;
  return M.Any(m=>Influenced(c,l4,m));
 }

 // 205 (Nishkapata, 1st form): "The 4th house must be occupied by a benefic, or a planet in
 // exaltation, friendly or own house, or the 4th house must be a benefic sign."
 private static bool Y205(ChartAnalysisInput c)
 {
  if(B.Any(p=>Find(c,p)?.HouseNumber==4))return true;
  if(Classical.Any(p=>Find(c,p)?.HouseNumber==4&&Favoured(c,p)))return true;
  return B.Contains(Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(HouseSign(c,4)))));
 }

 // 206 (Nishkapata, 2nd form): "Lord of Lagna should join the 4th in conjunction with or
 // aspected by a benefic or occupy Parvata or Uttamamsa." Parvatamsa (6) subsumes Uttamamsa (3)
 // in the Vaiseshikamsa ladder, so only the lower threshold needs checking.
 private static bool? Y206(ChartAnalysisInput c,IReadOnlyList<ChartAnalysisInput> charts)
 {
  var l1=Lord(c,1);
  if(Find(c,l1)?.HouseNumber!=4)return false;
  return B.Any(p=>Influenced(c,l1,p))||VaiseshikamsaCalculator.HasAttained(charts,l1,3);
 }

 // 207 (Matru Satrutwa): "Mercury, being lord of Lagna and the 4th, must join with or be
 // aspected by a malefic."
 private static bool Y207(ChartAnalysisInput c)=>
  Lord(c,1)==PlanetName.Mercury&&Lord(c,4)==PlanetName.Mercury&&M.Any(m=>Influenced(c,PlanetName.Mercury,m));

 // 208 (Matru Sneha): "The 1st and 4th houses must have a common lord, or the lords of the 1st
 // and 4th must be temporal or natural friends or aspected by benefics."
 private static bool Y208(ChartAnalysisInput c)
 {
  var l1=Lord(c,1);var l4=Lord(c,4);
  if(l1==l4)return true;
  if(PvrDignityEvaluator.IsNaturalFriend(l1,l4))return true;
  var p1=Find(c,l1);var p4=Find(c,l4);
  if(p1 is not null&&p4 is not null&&TemporalFriendDistance(p1.Sign,p4.Sign))return true;
  return B.Any(p=>Influenced(c,l1,p)||Influenced(c,l4,p));
 }

 // 209 (Vahana, 1st form): "The lord of Lagna must join the 4th, 11th or the 9th."
 private static bool Y209(ChartAnalysisInput c)=>Find(c,Lord(c,1))?.HouseNumber is 4 or 9 or 11;

 // 210 (Vahana, 2nd form): "The 4th lord must be exalted and the lord of the exaltation sign
 // must occupy a kendra or trikona."
 private static bool Y210(ChartAnalysisInput c)
 {
  var l4=Lord(c,4);var pos=Find(c,l4);
  if(pos is null||Dignity(c,l4)!="EXALTED")return false;
  var exaltSignLord=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(pos.Sign)));
  return KendraTrikona.Contains(Find(c,exaltSignLord)?.HouseNumber??0);
 }

 // 211 (Anapathya): "If Jupiter and the lords of Lagna, the 7th and the 5th are weak."
 private static bool Y211(ChartAnalysisInput c)=>
  new[]{PlanetName.Jupiter,Lord(c,1),Lord(c,7),Lord(c,5)}.All(p=>!Strong(c,p));

 // 212 (Sarpasapa, 1st form): "The 5th should be occupied by Rahu and aspected by Kuja or the
 // 5th house being a sign of Mars, should be occupied by Rahu."
 private static bool Y212(ChartAnalysisInput c)
 {
  if(Find(c,PlanetName.Rahu)?.HouseNumber!=5)return false;
  return Influenced(c,PlanetName.Rahu,PlanetName.Mars)||Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(HouseSign(c,5))))==PlanetName.Mars;
 }

 // 213 (Sarpasapa, 2nd form): "If the 5th lord is in conjunction with Rahu, and Saturn is in
 // the 5th house aspected by or associated with the Moon."
 private static bool Y213(ChartAnalysisInput c)=>
  Conjunct(c,Lord(c,5),PlanetName.Rahu)&&Find(c,PlanetName.Saturn)?.HouseNumber==5&&Influenced(c,PlanetName.Saturn,PlanetName.Moon);

 // 214 (Sarpasapa, 3rd form): "The karaka of children in association with Mars, Rahu in Lagna,
 // and the 5th lord in a dusthana."
 private static bool Y214(ChartAnalysisInput c)=>
  Influenced(c,PlanetName.Jupiter,PlanetName.Mars)&&Find(c,PlanetName.Rahu)?.HouseNumber==1&&Find(c,Lord(c,5))?.HouseNumber is 6 or 8 or 12;

 // 215 (Sarpasapa, 4th form): "The 5th house, being a sign of Mars, must be conjoined by Rahu
 // and aspected by or associated with Mercury."
 private static bool Y215(ChartAnalysisInput c)=>
  Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(HouseSign(c,5))))==PlanetName.Mars&&
  Find(c,PlanetName.Rahu)?.HouseNumber==5&&Influenced(c,PlanetName.Rahu,PlanetName.Mercury);

 // 216 (Pitrusapa Sutakshaya): six alternative dispositions, all quoted from Raman directly.
 // The elaborate degree-arc arithmetic in his Remarks for form (b) is only how to locate the
 // Sun's navamsa by hand — the engine already has the D9 chart, so it reads the navamsa sign
 // directly instead.
 private static bool Y216(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  var sun=Find(c,PlanetName.Sun);
  // (a) "The Sun must occupy the 5th house which should be his place of debilitation."
  if(sun is not null&&sun.HouseNumber==5&&Dignity(c,PlanetName.Sun)=="DEBILITATED")return true;
  // (b) "...or the amsas of Makara and Kumbha" (Sun in the 5th, Navamsa Capricornus/Aquarius).
  if(sun is not null&&sun.HouseNumber==5&&d9 is not null&&Find(d9,PlanetName.Sun)?.Sign is "Capricornus" or "Aquarius")return true;
  // (c) "...or in between malefics" — Sun in the 5th, hemmed by malefics.
  if(sun is not null&&sun.HouseNumber==5&&HemmedByMalefics(c,5))return true;
  // "The following dispositions of planets also constitute Yoga No. 216":
  // (a2) The Sun, being lord of the 5th, should occupy a trine, and be hemmed in between or
  // aspected by malefics.
  if(Lord(c,5)==PlanetName.Sun&&sun is not null&&sun.HouseNumber is 1 or 5 or 9&&
     (HemmedByMalefics(c,sun.HouseNumber)||M.Any(m=>Influenced(c,PlanetName.Sun,m))))return true;
  // (b2) Jupiter's disposition in Leo, the association of the 5th lord with the Sun and the 5th
  // and Lagna being occupied by malefics.
  if(Find(c,PlanetName.Jupiter)?.Sign=="Leo"&&Influenced(c,Lord(c,5),PlanetName.Sun)&&
     M.Any(m=>Find(c,m)?.HouseNumber==5)&&M.Any(m=>Find(c,m)?.HouseNumber==1))return true;
  // (c2) If Mars as lord of the 9th joins the 5th lord and malefics occupy Lagna and the
  // trikonas.
  if(Lord(c,9)==PlanetName.Mars&&Conjunct(c,PlanetName.Mars,Lord(c,5))&&
     M.Any(m=>Find(c,m)?.HouseNumber==1)&&M.Any(m=>Find(c,m)?.HouseNumber is 5 or 9))return true;
  return false;
 }

 // 217 (Matrusapa Sutakshaya): "If the 8th lord is in the 5th, the 5th lord is in the 8th and
 // the Moon and the 4th lord join the 6th."
 private static bool Y217(ChartAnalysisInput c)=>
  Find(c,Lord(c,8))?.HouseNumber==5&&Find(c,Lord(c,5))?.HouseNumber==8&&Find(c,PlanetName.Moon)?.HouseNumber==6&&Find(c,Lord(c,4))?.HouseNumber==6;

 // 218 (Bhratrusapa Sutakshaya): "The lords of Lagna and the 5th must join the 8th and the lord
 // of the 3rd should combine with Mars and Rahu in the 5th."
 private static bool Y218(ChartAnalysisInput c)
 {
  if(Find(c,Lord(c,1))?.HouseNumber!=8||Find(c,Lord(c,5))?.HouseNumber!=8)return false;
  var l3=Lord(c,3);
  return Conjunct(c,l3,PlanetName.Mars)&&Conjunct(c,l3,PlanetName.Rahu)&&Find(c,l3)?.HouseNumber==5;
 }

 // 219 (Pretasapa): "The Sun and Saturn in the 5th, weak Moon in the 7th, Rahu in Lagna and
 // Jupiter in the 12th."
 private static bool Y219(ChartAnalysisInput c)=>
  Find(c,PlanetName.Sun)?.HouseNumber==5&&Find(c,PlanetName.Saturn)?.HouseNumber==5&&
  Find(c,PlanetName.Moon)?.HouseNumber==7&&!Strong(c,PlanetName.Moon)&&
  Find(c,PlanetName.Rahu)?.HouseNumber==1&&Find(c,PlanetName.Jupiter)?.HouseNumber==12;

 private static bool HemmedByMalefics(ChartAnalysisInput c,int house)
 {
  if(house<1)return false;
  var prev=((house+10)%12)+1;var next=(house%12)+1;
  return M.Any(m=>Find(c,m)?.HouseNumber==prev)&&M.Any(m=>Find(c,m)?.HouseNumber==next);
 }
 private static bool CruelShashtiamsa(ChartAnalysisInput c,PlanetName p)
 {
  var x=Find(c,p);if(x?.NirayanaLongitudeDegrees is not double lon)return false;
  return ShashtiamsaDeityTable.Lookup(Enum.Parse<ZodiacName>(x.Sign),((lon%30)+30)%30).IsMalefic;
 }
 private static bool TemporalFriendDistance(string a,string b)
 {
  var d=Distance(a,b);return d is 2 or 3 or 4 or 10 or 11 or 12;
 }
 private static bool Strong(ChartAnalysisInput c,PlanetName p)=>Dignity(c,p) is "OWN" or "MOOLATRIKONA" or "EXALTED";
 private static bool Favoured(ChartAnalysisInput c,PlanetName p){var d=FullDignity(c,p);return d.DignityTypeCode is "OWN" or "MOOLATRIKONA" or "EXALTED"||d.RelationshipScore>0;}
 private static PvrDignityResult FullDignity(ChartAnalysisInput c,PlanetName p)
 {
  var x=Find(c,p);if(x is null)return new("MISSING",0,"",null,null,0);
  var lon=x.VargaLongitudeDegrees??x.NirayanaLongitudeDegrees??15;
  var signs=c.Planets.Where(v=>Enum.TryParse<PlanetName>(v.Planet,out _)).ToDictionary(v=>v.Planet,v=>Enum.Parse<ZodiacName>(v.Sign),StringComparer.OrdinalIgnoreCase);
  return PvrDignityEvaluator.Evaluate(p,Enum.Parse<ZodiacName>(x.Sign),((lon%30)+30)%30,signs);
 }
 private static bool Conjunct(ChartAnalysisInput c,PlanetName a,PlanetName b){var pa=Find(c,a);var pb=Find(c,b);return pa is not null&&pb is not null&&pa.Sign==pb.Sign;}
 private static bool Conjunct(ChartAnalysisInput c,PlanetName a,string b){var pa=Find(c,a);var pb=Find(c,b);return pa is not null&&pb is not null&&pa.Sign==pb.Sign;}
 private static bool Influenced(ChartAnalysisInput c,PlanetName target,PlanetName source)=>Find(c,target) is{}x&&Find(c,source) is{}y&&(x.Sign==y.Sign||Aspects(source,y.Sign,x.Sign));
 private static bool Aspects(PlanetName p,string a,string b){var d=Distance(a,b);return d==7||p==PlanetName.Mars&&d is 4 or 8||p==PlanetName.Jupiter&&d is 5 or 9||p==PlanetName.Saturn&&d is 3 or 10;}
 private static int Distance(string? a,string? b)=>a is null||b is null?0:(((int)Enum.Parse<ZodiacName>(b)-(int)Enum.Parse<ZodiacName>(a)+12)%12)+1;
 private static string HouseSign(ChartAnalysisInput c,int h)=>HouseEngine.GetHouseSign(c.AscendantSign,h).ToString();
 private static string Dignity(ChartAnalysisInput c,PlanetName p){var x=Find(c,p);if(x is null)return"MISSING";var l=x.VargaLongitudeDegrees??x.NirayanaLongitudeDegrees??15;return PvrDignityEvaluator.Evaluate(p,Enum.Parse<ZodiacName>(x.Sign),((l%30)+30)%30).DignityTypeCode;}
 private static PlanetName Lord(ChartAnalysisInput c,int h)=>Enum.Parse<PlanetName>(HouseEngine.GetSignLord(HouseEngine.GetHouseSign(c.AscendantSign,h)));
 private static PlanetPosition? Find(ChartAnalysisInput c,PlanetName p)=>c.Planets.SingleOrDefault(x=>x.Planet==p.ToString());
 private static PlanetPosition? Find(ChartAnalysisInput c,string p)=>c.Planets.SingleOrDefault(x=>x.Planet.Equals(p,StringComparison.OrdinalIgnoreCase));
 private static ContextualYogaResult Row(int n,bool present){var scan=Scan(n);return new(Codes[n],present,"EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{scan-12}; scan p.{scan}");}
 private static ContextualYogaResult Missing(int n,string note){var scan=Scan(n);return new(Codes[n],null,"NOT_EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{scan-12}; scan p.{scan}",note);}
 private static int Scan(int n)=>n switch
 {
  201 or 202 or 203 or 204=>204,205 or 206=>206,207 or 208=>207,209 or 210=>208,211=>209,
  212 or 213 or 214 or 215=>210,216=>214,217=>218,218=>219,219=>220,
  _=>204
 };
}
