using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Core.Engines.Yoga;

/// <summary>Raman 220–244 — progeny, intelligence, spouses and fortune. Cluster B of the
/// 201–300 catalog's phased build-out (bvyoga.md §4), transcribed out of
/// RamanFinalHundredCatalog.cs the same way Cluster C (245–263) was.</summary>
public static class RamanProgenyYogaEvaluator
{
 private static readonly PlanetName[] B=[PlanetName.Moon,PlanetName.Mercury,PlanetName.Jupiter,PlanetName.Venus];
 private static readonly PlanetName[] M=[PlanetName.Sun,PlanetName.Mars,PlanetName.Saturn];
 private static readonly PlanetName[] Classical=
  [PlanetName.Sun,PlanetName.Moon,PlanetName.Mars,PlanetName.Mercury,PlanetName.Jupiter,PlanetName.Venus,PlanetName.Saturn];
 private static readonly int[] KendraTrikona=[1,4,5,7,9,10];
 private static readonly Dictionary<int,string> Codes=new()
 {
  [220]="YOGA_BAHUPUTRA",[221]="YOGA_BAHUPUTRA",[222]="YOGA_DATTAPUTRA",[223]="YOGA_DATTAPUTRA",
  [224]="YOGA_APUTRA",[225]="YOGA_EKAPUTRA",[226]="YOGA_SUPUTRA",
  [227]="YOGA_KALANIRDESAT_PUTRA",[228]="YOGA_KALANIRDESAT_PUTRA",
  [229]="YOGA_KALANIRDESAT_PUTRANASA",[230]="YOGA_KALANIRDESAT_PUTRANASA",
  [231]="YOGA_BUDDHIMATURYA",[232]="YOGA_THEEVRABUDDHI",[233]="YOGA_BUDDHI_JADA",[234]="YOGA_THRIKALAGNANA",
  [235]="YOGA_PUTRA_SUKHA",[236]="YOGA_JARA",[237]="YOGA_JARAJA_PUTRA",[238]="YOGA_BAHU_STREE",
  [239]="YOGA_SATKALATRA",[240]="YOGA_BHAGA_CHUMBANA",[241]="YOGA_BHAGYA",
  [242]="YOGA_JANANAT_PURVAM_PITRU_MARANA",[243]="YOGA_DHATRUTWA",[244]="YOGA_APAKEERTI"
 };

 public static IReadOnlyList<ContextualYogaResult> Evaluate(ChartBundle bundle)
 {
  var c=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D1",StringComparison.OrdinalIgnoreCase));if(c is null)return[];
  var d9=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D9",StringComparison.OrdinalIgnoreCase));
  var charts=bundle.Charts;
  bool? v(int n)=>n switch
  {
   220=>Y220(c,d9),221=>Y221(c,d9),
   222=>Y222(c),223=>Y223(c),
   224=>Y224(c),225=>Y225(c),226=>Y226(c),
   227=>Y227(c),228=>Y228(c),
   229=>Y229(c),230=>Y230(c),
   231=>Y231(c),232=>Y232(c,d9),233=>Y233(c),234=>Y234(c,d9,charts),
   235=>Y235(c),236=>Y236(c),237=>Y237(c),238=>Y238(c),239=>Y239(c),
   240=>Y240(c,d9),241=>Y241(c),242=>Y242(c),243=>Y243(c),244=>Y244(c,d9),
   _=>null
  };
  return Enumerable.Range(220,25)
   .Select(n=>v(n) is{}r?Row(n,r):Missing(n,"Required divisional or qualification evidence is unavailable."))
   .ToList();
 }

 // 220 (Bahuputra, 1st form): "Rahu in the 5th house, in a Navamsa other than that of Saturn."
 private static bool? Y220(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(Find(c,PlanetName.Rahu)?.HouseNumber!=5)return false;
  if(d9 is null)return null;
  return NavamsaLord(d9,PlanetName.Rahu)!=PlanetName.Saturn;
 }

 // 220 (Bahuputra, 2nd form/221): "The same yoga arises if the lord of the Navamsa occupied by
 // a planet who is in association with the 7th lord is in the 1st, 2nd or 5th house."
 private static bool? Y221(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(d9 is null)return null;
  var seventhLord=Lord(c,7);
  foreach(var x in Classical)
  {
   if(x==seventhLord||!Influenced(c,x,seventhLord))continue;
   var nl=NavamsaLord(d9,x);if(nl is null)continue;
   if(Find(c,nl.Value)?.HouseNumber is 1 or 2 or 5)return true;
  }
  return false;
 }

 // 222 (Dattaputra, 1st form): "Mars and Saturn should occupy the 5th house and the lord of
 // Lagna should be in a sign of Mercury, aspected by or in association with the same planet."
 private static bool Y222(ChartAnalysisInput c)
 {
  if(Find(c,PlanetName.Mars)?.HouseNumber!=5||Find(c,PlanetName.Saturn)?.HouseNumber!=5)return false;
  var lagnaLord=Lord(c,1);var pos=Find(c,lagnaLord);if(pos is null)return false;
  if(Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(pos.Sign)))!=PlanetName.Mercury)return false;
  return Influenced(c,lagnaLord,PlanetName.Mercury);
 }

 // 223 (Dattaputra, 2nd form): "The lord of the 7th must be posited in the 11th, the 5th lord
 // must join a benefic and the 5th house must be occupied by Mars or Saturn."
 private static bool Y223(ChartAnalysisInput c)
 {
  if(Find(c,Lord(c,7))?.HouseNumber!=11)return false;
  var fifthLord=Lord(c,5);
  if(!B.Any(p=>Conjunct(c,fifthLord,p)))return false;
  return Find(c,PlanetName.Mars)?.HouseNumber==5||Find(c,PlanetName.Saturn)?.HouseNumber==5;
 }

 // 224 (Aputra): "The lord of the 5th house should occupy a dusthana."
 private static bool Y224(ChartAnalysisInput c)=>Find(c,Lord(c,5))?.HouseNumber is 6 or 8 or 12;

 // 225 (Ekaputra): "Lord of the 5th house should join a kendra or trikona."
 private static bool Y225(ChartAnalysisInput c)=>KendraTrikona.Contains(Find(c,Lord(c,5))?.HouseNumber??0);

 // 226 (Suputra): "If Jupiter is lord of the 5th house and the Sun occupies a favourable
 // position, this yoga is caused." "Favourable" read as the project's Strong() convention or a
 // kendra/trikona placement.
 private static bool Y226(ChartAnalysisInput c)=>
  Lord(c,5)==PlanetName.Jupiter&&(Strong(c,PlanetName.Sun)||KendraTrikona.Contains(Find(c,PlanetName.Sun)?.HouseNumber??0));

 // 227 (Kalanirdesat Putra, 1st form): "Jupiter should be in the 5th house and the lord of the
 // 5th should join Venus."
 private static bool Y227(ChartAnalysisInput c)=>Find(c,PlanetName.Jupiter)?.HouseNumber==5&&Conjunct(c,Lord(c,5),PlanetName.Venus);

 // 228 (Kalanirdesat Putra, 2nd form): "Jupiter must also occupy the 9th from Lagna and Venus
 // should be in the 9th from Jupiter, in conjunction with the lord of Lagna."
 private static bool Y228(ChartAnalysisInput c)
 {
  var jup=Find(c,PlanetName.Jupiter);if(jup is null||jup.HouseNumber!=9)return false;
  var venus=Find(c,PlanetName.Venus);if(venus is null||HouseDistance(venus.HouseNumber,jup.HouseNumber)!=9)return false;
  return Conjunct(c,PlanetName.Venus,Lord(c,1));
 }

 // 229 (Kalanirdesat Putranasa, 1st form): "Rahu must occupy the 5th house, the lord of the 5th
 // must be in conjunction with a malefic and Jupiter should be debilitated." Remarks: "an
 // affliction by way of a malefic... by implication an aspect is also meant" — read broadly.
 private static bool Y229(ChartAnalysisInput c)=>
  Find(c,PlanetName.Rahu)?.HouseNumber==5&&M.Any(m=>Influenced(c,Lord(c,5),m))&&Dignity(c,PlanetName.Jupiter)=="DEBILITATED";

 // 230 (Kalanirdesat Putranasa, 2nd form): "Malefics should be disposed in the 5th from Jupiter
 // and Lagna."
 private static bool Y230(ChartAnalysisInput c)
 {
  var jup=Find(c,PlanetName.Jupiter);if(jup is null)return false;
  return M.Any(m=>Find(c,m) is{}x&&HouseDistance(x.HouseNumber,jup.HouseNumber)==5)&&M.Any(m=>Find(c,m)?.HouseNumber==5);
 }

 // 231 (Buddhimaturya): Raman's own broadened reading (the literal one "would be preposterous"
 // per his Remarks): "the 5th house... is occupied by benefics and 5th lord is in association
 // with Jupiter, Mercury and Venus."
 private static bool Y231(ChartAnalysisInput c)
 {
  if(!B.Any(p=>Find(c,p)?.HouseNumber==5))return false;
  return B.Any(p=>Influenced(c,Lord(c,5),p));
 }

 // 232 (Theevrabuddhi): "the lord of the Navamsa occupied by the 5th lord should receive the
 // aspect of a benefic" — and (per Raman's own note) the 5th lord's navamsa should itself be
 // benefic, regardless of the 5th lord's own natural classification.
 private static bool? Y232(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(d9 is null)return null;
  var navLord=NavamsaLord(d9,Lord(c,5));
  if(navLord is null||!B.Contains(navLord.Value))return false;
  return B.Any(p=>p!=navLord.Value&&Influenced(c,navLord.Value,p));
 }

 // 233 (Buddhi Jada): "If the lord of the Ascendant is conjoined with or aspected by evil
 // planets, Saturn occupies the 5th and the lord of Lagna is aspected by Saturn."
 private static bool Y233(ChartAnalysisInput c)
 {
  var lagnaLord=Lord(c,1);
  if(!M.Any(m=>Influenced(c,lagnaLord,m)))return false;
  if(Find(c,PlanetName.Saturn)?.HouseNumber!=5)return false;
  return Influenced(c,lagnaLord,PlanetName.Saturn);
 }

 // 234 (Thrikalagnana): "Jupiter should occupy Mrudwamsa in his own navamsa, or Gopuramsa and
 // be aspected by a benefic planet." Raman's own worked description of Mrudwamsa (19th
 // shashtiamsha part in an odd sign, 42nd in an even sign) matches ShashtiamsaDeityTable's
 // "Mridu" entry exactly under its odd/even reversal rule — cross-confirms that table.
 private static bool? Y234(ChartAnalysisInput c,ChartAnalysisInput? d9,IReadOnlyList<ChartAnalysisInput> charts)
 {
  if(d9 is null)return null;
  var jupiterD1=Find(c,PlanetName.Jupiter);
  var ownNavamsa=NavamsaLord(d9,PlanetName.Jupiter)==PlanetName.Jupiter;
  var mrudwamsa=ownNavamsa&&jupiterD1?.NirayanaLongitudeDegrees is double lon&&
   ShashtiamsaDeityTable.Lookup(Enum.Parse<ZodiacName>(jupiterD1.Sign),((lon%30)+30)%30).Name=="Mridu";
  if(mrudwamsa)return true;
  if(!VaiseshikamsaCalculator.HasAttained(charts,PlanetName.Jupiter,4))return false;
  return B.Any(p=>p!=PlanetName.Jupiter&&Influenced(c,PlanetName.Jupiter,p));
 }

 // 235 (Putra Sukha): "When the 5th is occupied by Jupiter and Venus, or when Mercury joins the
 // 5th, or the 5th happening to be the sign of a benefic, is occupied by benefics."
 private static bool Y235(ChartAnalysisInput c)
 {
  if(Find(c,PlanetName.Jupiter)?.HouseNumber==5&&Find(c,PlanetName.Venus)?.HouseNumber==5)return true;
  if(Find(c,PlanetName.Mercury)?.HouseNumber==5)return true;
  var fifthSign=HouseSign(c,5);
  return B.Contains(Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(fifthSign))))&&B.Any(p=>Find(c,p)?.HouseNumber==5);
 }

 // 236 (Jara): "The 10th house must be occupied by the lords of the 10th, 2nd and 7th."
 private static bool Y236(ChartAnalysisInput c)=>
  new[]{Lord(c,10),Lord(c,2),Lord(c,7)}.Distinct().All(p=>Find(c,p)?.HouseNumber==10);

 // 237 (Jarajaputra): "Powerful lords of the 5th and the 7th must join with the lord of the 6th
 // and be aspected by benefics."
 private static bool Y237(ChartAnalysisInput c)
 {
  var l5=Lord(c,5);var l6=Lord(c,6);var l7=Lord(c,7);
  if(!Strong(c,l5)||!Strong(c,l7)||!Conjunct(c,l5,l6)||!Conjunct(c,l7,l6))return false;
  return B.Any(p=>Influenced(c,l5,p))&&B.Any(p=>Influenced(c,l7,p));
 }

 // 238 (Bahu Stree): "If the lords of the Lagna and the 7th are in conjunction or aspect with
 // each other" — or (Raman's own alternate) "the lord of the 9th is in the 7th, the lord of the
 // 7th is in the 4th and the lord of Lagna or the lord of the 11th is in a kendra."
 private static bool Y238(ChartAnalysisInput c)
 {
  if(JoinedOrAspected(c,Lord(c,1),Lord(c,7)))return true;
  if(Find(c,Lord(c,9))?.HouseNumber!=7||Find(c,Lord(c,7))?.HouseNumber!=4)return false;
  return Kendra(c,Lord(c,1))||Kendra(c,Lord(c,11));
 }

 // 239 (Satkalatra): "The lord of the 7th or Venus should join or be aspected by Jupiter or
 // Mercury."
 private static bool Y239(ChartAnalysisInput c)=>
  JoinedOrAspected(c,Lord(c,7),PlanetName.Jupiter)||JoinedOrAspected(c,Lord(c,7),PlanetName.Mercury)||
  JoinedOrAspected(c,PlanetName.Venus,PlanetName.Jupiter)||JoinedOrAspected(c,PlanetName.Venus,PlanetName.Mercury);

 // 240 (Bhaga Chumbana): "If the lord of the 7th is in the 4th in conjunction with Venus" — or
 // (Remarks' own alternate) "Lagna lord is debilitated in the Rasi or in the Navamsa."
 private static bool Y240(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(Find(c,Lord(c,7))?.HouseNumber==4&&Conjunct(c,Lord(c,7),PlanetName.Venus))return true;
  if(Dignity(c,Lord(c,1))=="DEBILITATED")return true;
  return d9 is not null&&Dignity(d9,Lord(c,1))=="DEBILITATED";
 }

 // 241 (Bhagya): "A strong benefic should be in Lagna, the 3rd or 5th, simultaneously aspecting
 // the 9th."
 private static bool Y241(ChartAnalysisInput c)=>
  B.Any(p=>Strong(c,p)&&Find(c,p)?.HouseNumber is 1 or 3 or 5&&InfluencesSign(c,p,HouseSign(c,9)));

 // 242 (Jananatpurvam Pitru Marana): "The Sun must be in the 6th, 8th or 12th; lord of the 8th
 // must be in the 9th; lord of the 12th in Lagna and the lord of the 6th in the 5th."
 private static bool Y242(ChartAnalysisInput c)=>
  Find(c,PlanetName.Sun)?.HouseNumber is 6 or 8 or 12&&Find(c,Lord(c,8))?.HouseNumber==9&&
  Find(c,Lord(c,12))?.HouseNumber==1&&Find(c,Lord(c,6))?.HouseNumber==5;

 // 243 (Dhatrutwa): "The lord of the 9th should be exalted, and aspected by a benefic, and the
 // 9th should be occupied by a benefic."
 private static bool Y243(ChartAnalysisInput c)
 {
  var l9=Lord(c,9);
  if(Dignity(c,l9)!="EXALTED"||!B.Any(p=>Influenced(c,l9,p)))return false;
  return B.Any(p=>Find(c,p)?.HouseNumber==9);
 }

 // 244 (Apakeerti): "the 10th house should be occupied by the Sun and Saturn and aspected by
 // malefics, or the Sun and Saturn should occupy malefic navamsas."
 private static bool Y244(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(Find(c,PlanetName.Sun)?.HouseNumber==10&&Find(c,PlanetName.Saturn)?.HouseNumber==10&&M.Any(m=>InfluencesSign(c,m,HouseSign(c,10))))return true;
  return d9 is not null&&M.Contains(NavamsaLord(d9,PlanetName.Sun)??PlanetName.Moon)&&M.Contains(NavamsaLord(d9,PlanetName.Saturn)??PlanetName.Moon);
 }

 private static int HouseDistance(int a,int b)=>((a-b+12)%12)+1;
 private static bool Kendra(ChartAnalysisInput c,PlanetName p)=>Find(c,p)?.HouseNumber is 1 or 4 or 7 or 10;
 private static PlanetName? NavamsaLord(ChartAnalysisInput d9,PlanetName p){var x=Find(d9,p);return x is null?null:Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(x.Sign)));}
 private static bool Strong(ChartAnalysisInput c,PlanetName p)=>Dignity(c,p) is "OWN" or "MOOLATRIKONA" or "EXALTED";
 private static bool Conjunct(ChartAnalysisInput c,PlanetName a,PlanetName b){var pa=Find(c,a);var pb=Find(c,b);return pa is not null&&pb is not null&&pa.Sign==pb.Sign;}
 private static bool JoinedOrAspected(ChartAnalysisInput c,PlanetName a,PlanetName b)
 {
  var pa=Find(c,a);var pb=Find(c,b);if(pa is null||pb is null)return false;
  return pa.Sign==pb.Sign||Aspects(a,pa.Sign,pb.Sign)||Aspects(b,pb.Sign,pa.Sign);
 }
 private static bool Influenced(ChartAnalysisInput c,PlanetName target,PlanetName source)=>Find(c,target) is{}x&&Find(c,source) is{}y&&(x.Sign==y.Sign||Aspects(source,y.Sign,x.Sign));
 private static bool InfluencesSign(ChartAnalysisInput c,PlanetName source,string sign)=>Find(c,source) is{}x&&(x.Sign==sign||Aspects(source,x.Sign,sign));
 private static bool Aspects(PlanetName p,string a,string b){var d=Distance(a,b);return d==7||p==PlanetName.Mars&&d is 4 or 8||p==PlanetName.Jupiter&&d is 5 or 9||p==PlanetName.Saturn&&d is 3 or 10;}
 private static int Distance(string? a,string? b)=>a is null||b is null?0:(((int)Enum.Parse<ZodiacName>(b)-(int)Enum.Parse<ZodiacName>(a)+12)%12)+1;
 private static string HouseSign(ChartAnalysisInput c,int h)=>HouseEngine.GetHouseSign(c.AscendantSign,h).ToString();
 private static string Dignity(ChartAnalysisInput c,PlanetName p){var x=Find(c,p);if(x is null)return"MISSING";var l=x.VargaLongitudeDegrees??x.NirayanaLongitudeDegrees??15;return PvrDignityEvaluator.Evaluate(p,Enum.Parse<ZodiacName>(x.Sign),((l%30)+30)%30).DignityTypeCode;}
 private static PlanetName Lord(ChartAnalysisInput c,int h)=>Enum.Parse<PlanetName>(HouseEngine.GetSignLord(HouseEngine.GetHouseSign(c.AscendantSign,h)));
 private static PlanetPosition? Find(ChartAnalysisInput c,PlanetName p)=>c.Planets.SingleOrDefault(x=>x.Planet==p.ToString());
 private static ContextualYogaResult Row(int n,bool present){var scan=Scan(n);return new(Codes[n],present,"EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{scan-12}; scan p.{scan}");}
 private static ContextualYogaResult Missing(int n,string note){var scan=Scan(n);return new(Codes[n],null,"NOT_EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{scan-12}; scan p.{scan}",note);}
 private static int Scan(int n)=>n switch
 {
  220=>222,221=>223,222 or 223=>224,224=>225,225=>226,226=>227,227=>228,228=>229,
  229=>228,230=>229,231=>230,232=>233,233=>234,234=>236,235 or 236=>238,237 or 238=>240,
  239=>242,240 or 241=>244,242=>245,243=>246,244=>248,
  _=>222
 };
}
