using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Core.Engines.Yoga;

/// <summary>Raman 264–300 — affliction, disease, deformity, manner of death, loss of status.
/// Cluster D, the last of the 201–300 catalog's phased build-out (bvyoga.md §4), transcribed
/// out of RamanFinalHundredCatalog.cs the same way Clusters A/B/C were. Per the project's
/// sensitive-content policy (yoga-corpus.md), every predicate below quotes Raman's own
/// wording rather than adding editorial framing.</summary>
public static class RamanAfflictionYogaEvaluator
{
 private static readonly PlanetName[] B=[PlanetName.Moon,PlanetName.Mercury,PlanetName.Jupiter,PlanetName.Venus];
 private static readonly PlanetName[] M=[PlanetName.Sun,PlanetName.Mars,PlanetName.Saturn];
 private static readonly int[] KendraTrikona=[1,4,5,7,9,10];
 private static readonly Dictionary<int,string> Codes=new()
 {
  [264]="YOGA_GALAKARNA",[265]="YOGA_VRANA",[266]="YOGA_SISNAVYADHI",[267]="YOGA_KALATRASHANDA",
  [268]="YOGA_KUSHTAROGA",[269]="YOGA_KUSHTAROGA",[270]="YOGA_KSHAYAROGA",
  [271]="YOGA_BANDHANA",[272]="YOGA_KARASCHEDA",[273]="YOGA_SIRACHCHEDA",[274]="YOGA_DURMARANA",
  [275]="YOGA_YUDDHE_MARANA",[276]="YOGA_SANGHATAKA_MARANA",[277]="YOGA_SANGHATAKA_MARANA",
  [278]="YOGA_PEENASAROGA",[279]="YOGA_PITTAROGA",[280]="YOGA_VIKALANGA_PATNI",
  [281]="YOGA_PUTRA_KALATRA_HEENA",[282]="YOGA_BHARYASAHA_VYABHICHARA",[283]="YOGA_VAMSACHEDA",
  [284]="YOGA_GUHYAROGA",[285]="YOGA_ANGAHEENA",[286]="YOGA_SWETAKUSHTA",[287]="YOGA_PISACHA_GRASTHA",
  [288]="YOGA_ANDHA",[289]="YOGA_ANDHA",[290]="YOGA_VATHAROGA",
  [291]="YOGA_MATIBHRAMANA",[292]="YOGA_MATIBHRAMANA",[293]="YOGA_MATIBHRAMANA",[294]="YOGA_MATIBHRAMANA",
  [295]="YOGA_KHALWATA",[296]="YOGA_NISHTURABHASHI",[297]="YOGA_RAJABHRASHTA",
  [298]="YOGA_RAJA_BHANGA",[299]="YOGA_RAJA_BHANGA",[300]="YOGA_GOHANTA"
 };

 public static IReadOnlyList<ContextualYogaResult> Evaluate(ChartBundle bundle)
 {
  var c=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D1",StringComparison.OrdinalIgnoreCase));if(c is null)return[];
  var d9=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D9",StringComparison.OrdinalIgnoreCase));
  var charts=bundle.Charts;
  bool? v(int n)=>n switch
  {
   264=>Y264(c),265=>Y265(c),266=>Y266(c),267=>Y267(c),
   268=>Y268(c),269=>Y269(c),270=>Y270(c),
   271=>Y271(c),272=>Y272(c),273=>Y273(c),274=>Y274(c),
   275=>Y275(c,d9,charts),
   276=>Y276(c,d9),277=>Y277(c),
   278=>Y278(c,d9),279=>Y279(c),280=>Y280(c),
   281=>Y281(c),282=>Y282(c),283=>Y283(c),
   284=>Y284(c,d9),285=>Y285(c),286=>Y286(c),287=>Y287(c),
   288=>Y288(c),289=>Y289(c),290=>Y290(c),
   291=>Y291(c),292=>Y292(c),293=>Y293(c),294=>Y294(c),
   295=>Y295(c),296=>Y296(c),297=>Y297(c),
   298=>Y298(c,d9),299=>Y299(c),300=>Y300(c),
   _=>null
  };
  return Enumerable.Range(264,37)
   .Select(n=>v(n) is{}r?Row(n,r):Missing(n,"Required divisional or qualification evidence is unavailable."))
   .ToList();
 }

 // 264 (Galakarna): "The 3rd house must be occupied by Mandi and Rahu or by Mars in the
 // shashtiamsa of Preta Puriha." Remarks generalize the 2nd branch: "Rahu's disposition in the
 // 3rd house in a cruel shashtiamsa is a sure indication" — the named "Preta Puriha" division
 // isn't in the registered 60-name table, so that generalized reading is used instead of
 // inventing a match.
 private static bool Y264(ChartAnalysisInput c)
 {
  if(Find(c,PlanetName.Rahu)?.HouseNumber==3&&Find(c,"Maandi")?.HouseNumber==3)return true;
  if(Find(c,PlanetName.Rahu)?.HouseNumber==3&&CruelShashtiamsa(c,PlanetName.Rahu))return true;
  return Find(c,PlanetName.Mars)?.HouseNumber==3&&CruelShashtiamsa(c,PlanetName.Mars);
 }

 // 265 (Vrana): "The 6th lord, being a malefic, should occupy the Lagna, 8th or 10th."
 private static bool Y265(ChartAnalysisInput c){var l6=Lord(c,6);return M.Contains(l6)&&Find(c,l6)?.HouseNumber is 1 or 8 or 10;}

 // 266 (Sisnavyadhi): "Mercury should join Lagna in association with the lords of the 6th and
 // 8th."
 private static bool Y266(ChartAnalysisInput c)=>
  Find(c,PlanetName.Mercury)?.HouseNumber==1&&Conjunct(c,PlanetName.Mercury,Lord(c,6))&&Conjunct(c,PlanetName.Mercury,Lord(c,8));

 // 267 (Kalatrashanda): "The lord of the 7th should join the 6th with Venus."
 private static bool Y267(ChartAnalysisInput c){var l7=Lord(c,7);return Find(c,l7)?.HouseNumber==6&&Conjunct(c,l7,PlanetName.Venus);}

 // 268 (Kushtaroga, 1st form): "The lord of Lagna must join the 4th or 12th in conjunction with
 // Mars and Mercury."
 private static bool Y268(ChartAnalysisInput c){var l1=Lord(c,1);return Find(c,l1)?.HouseNumber is 4 or 12&&Conjunct(c,l1,PlanetName.Mars)&&Conjunct(c,l1,PlanetName.Mercury);}

 // 269 (Kushtaroga, 2nd form): "Jupiter should occupy the 6th in association with Saturn and
 // the Moon."
 private static bool Y269(ChartAnalysisInput c)=>
  Find(c,PlanetName.Jupiter)?.HouseNumber==6&&Conjunct(c,PlanetName.Jupiter,PlanetName.Saturn)&&Conjunct(c,PlanetName.Jupiter,PlanetName.Moon);

 // 270 (Kshayaroga): "Rahu in the 6th, Mandi in a kendra from Lagna, and the lord of Lagna in
 // the 8th."
 private static bool Y270(ChartAnalysisInput c)=>
  Find(c,PlanetName.Rahu)?.HouseNumber==6&&Find(c,"Maandi")?.HouseNumber is 1 or 4 or 7 or 10&&Find(c,Lord(c,1))?.HouseNumber==8;

 // 271 (Bandhana): "If the lord of the Lagna and the 6th join a kendra or thrikona with Saturn,
 // Rahu or Kethu."
 private static bool Y271(ChartAnalysisInput c)
 {
  var l1=Lord(c,1);var l6=Lord(c,6);
  var afflictors=new[]{PlanetName.Saturn,PlanetName.Rahu,PlanetName.Ketu};
  return KendraTrikona.Contains(Find(c,l1)?.HouseNumber??0)&&KendraTrikona.Contains(Find(c,l6)?.HouseNumber??0)&&
   afflictors.Any(p=>Conjunct(c,l1,p))&&afflictors.Any(p=>Conjunct(c,l6,p));
 }

 // 272 (Karascheda): "Saturn and Jupiter should be in the 9th and the 3rd" — plus three
 // alternates Raman states directly constitute the same yoga: "(a) ... in the 8th and 12th, (b)
 // ... the Moon in the 7th or 8th in association with Mars, (c) ... Rahu, Saturn and Mercury in
 // conjunction in the 10th."
 private static bool Y272(ChartAnalysisInput c)=>
  (Find(c,PlanetName.Saturn)?.HouseNumber==9&&Find(c,PlanetName.Jupiter)?.HouseNumber==3)||
  (Find(c,PlanetName.Saturn)?.HouseNumber==8&&Find(c,PlanetName.Jupiter)?.HouseNumber==12)||
  (Find(c,PlanetName.Moon)?.HouseNumber is 7 or 8&&Conjunct(c,PlanetName.Moon,PlanetName.Mars))||
  (Find(c,PlanetName.Rahu)?.HouseNumber==10&&Conjunct(c,PlanetName.Rahu,PlanetName.Saturn)&&Conjunct(c,PlanetName.Rahu,PlanetName.Mercury));

 // 273 (Sirachcheda): "The lord of the 6th must be in conjunction with Venus while the Sun or
 // Saturn should join Rahu in a cruel shashtiamsa." Raman resolves his own quoted-Sanskrit
 // ambiguity directly: "the Sun or Saturn being in conjunction with Rahu should occupy a cruel
 // shashtiamsa" — the conjoining planet itself, not Rahu.
 private static bool Y273(ChartAnalysisInput c)
 {
  if(!Conjunct(c,Lord(c,6),PlanetName.Venus))return false;
  return new[]{PlanetName.Sun,PlanetName.Saturn}.Any(p=>Conjunct(c,p,PlanetName.Rahu)&&CruelShashtiamsa(c,p));
 }

 // 274 (Durmarana): "The Moon being aspected by lord of Lagna should occupy the 6th, 8th or
 // 12th in association with Saturn, Mandi or Rahu."
 private static bool Y274(ChartAnalysisInput c)
 {
  if(!Influenced(c,PlanetName.Moon,Lord(c,1))||Find(c,PlanetName.Moon)?.HouseNumber is not(6 or 8 or 12))return false;
  return Conjunct(c,PlanetName.Moon,PlanetName.Saturn)||Conjunct(c,PlanetName.Moon,PlanetName.Rahu)||Conjunct(c,PlanetName.Moon,"Maandi");
 }

 // 275 (Yuddhe Marana): Raman's own broadened Remarks reading — "the 6th or 8th lord in
 // conjunction with the 3rd lord, and Saturn, Rahu or Mandi occupies cruel amsas" — plus his
 // separately-stated alternate: "the lord of the drekkana occupied by Saturn is in a Rasi or
 // Navamsa of Mars or is aspected by Mars."
 private static bool Y275(ChartAnalysisInput c,ChartAnalysisInput? d9,IReadOnlyList<ChartAnalysisInput> charts)
 {
  var l3=Lord(c,3);
  var primary=(Conjunct(c,Lord(c,6),l3)||Conjunct(c,Lord(c,8),l3))&&
   (CruelShashtiamsa(c,PlanetName.Rahu)||CruelShashtiamsa(c,PlanetName.Saturn));
  if(primary)return true;
  var d3=charts.FirstOrDefault(x=>x.ChartType.Equals("D3",StringComparison.OrdinalIgnoreCase));
  var satD3Sign=d3 is null?null:Find(d3,PlanetName.Saturn)?.Sign;
  if(satD3Sign is null)return false;
  var drekkanaLord=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(satD3Sign)));
  var d1Pos=Find(c,drekkanaLord);
  var inMarsRasi=d1Pos is not null&&Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(d1Pos.Sign)))==PlanetName.Mars;
  var inMarsNavamsa=d9 is not null&&Find(d9,drekkanaLord) is{}nd&&Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(nd.Sign)))==PlanetName.Mars;
  return inMarsRasi||inMarsNavamsa||Influenced(c,drekkanaLord,PlanetName.Mars);
 }

 // 276 (Sanghataka Marana, 1st form): "If there are many evil planets in the 8th occupying
 // martian Rasi or Navamsa and joining evil sub-divisions."
 private static bool? Y276(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  var maleficsInEighth=M.Where(m=>Find(c,m)?.HouseNumber==8).ToList();
  if(maleficsInEighth.Count<2)return false;
  bool Martian(PlanetName m)
  {
   var x=Find(c,m)!;
   var d1Mars=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(x.Sign)))==PlanetName.Mars;
   var d9Mars=d9 is not null&&Find(d9,m) is{}y&&Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(y.Sign)))==PlanetName.Mars;
   return d1Mars||d9Mars;
  }
  if(!maleficsInEighth.Any(Martian))return false;
  return maleficsInEighth.Any(m=>CruelShashtiamsa(c,m));
 }

 // 277 (Sanghataka Marana, 2nd form): "The Sun, Rahu and Saturn being aspected by the 8th lord
 // should join evil amsas."
 private static bool Y277(ChartAnalysisInput c)
 {
  var l8=Lord(c,8);
  var trio=new[]{PlanetName.Sun,PlanetName.Rahu,PlanetName.Saturn};
  return trio.All(p=>Influenced(c,p,l8))&&trio.All(p=>CruelShashtiamsa(c,p));
 }

 // 278 (Peenasaroga): "The Moon, Saturn and a malefic should be in the 6th, 8th and 12th
 // respectively and Lagnadhipathi should join malefic Navamsa."
 private static bool? Y278(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(Find(c,PlanetName.Moon)?.HouseNumber!=6||Find(c,PlanetName.Saturn)?.HouseNumber!=8)return false;
  if(!M.Any(m=>m!=PlanetName.Saturn&&Find(c,m)?.HouseNumber==12))return false;
  if(d9 is null)return null;
  var l1=Lord(c,1);
  return NavamsaLord(d9,l1) is{}nl&&M.Contains(nl);
 }

 // 279 (Pittaroga): "The 6th house must be occupied by the Sun in conjunction with a malefic
 // and further aspected by another malefic."
 private static bool Y279(ChartAnalysisInput c)
 {
  if(Find(c,PlanetName.Sun)?.HouseNumber!=6)return false;
  var malefics=M.Where(m=>m!=PlanetName.Sun&&Find(c,m) is not null).ToList();
  return malefics.Any(m1=>Conjunct(c,PlanetName.Sun,m1)&&malefics.Any(m2=>m2!=m1&&Influenced(c,PlanetName.Sun,m2)));
 }

 // 280 (Vikalangapatni): "Venus and Sun should occupy the 7th, 9th or 5th house."
 private static bool Y280(ChartAnalysisInput c)
 {
  var venus=Find(c,PlanetName.Venus);var sun=Find(c,PlanetName.Sun);
  return venus is not null&&sun is not null&&venus.HouseNumber==sun.HouseNumber&&venus.HouseNumber is 5 or 7 or 9;
 }

 // 281 (Putrakalatraheena): "When the waning Moon is in the 5th and malefics occupy the 12th,
 // 7th and Lagna." Raman's three lettered variants are just permutations of Sun/Mars/Saturn
 // across those three houses, so the predicate checks the set, not a fixed assignment.
 private static bool Y281(ChartAnalysisInput c)
 {
  var moon=Find(c,PlanetName.Moon);
  if(moon is null||moon.HouseNumber!=5||!IsWaning(c))return false;
  int?[] houses=[Find(c,PlanetName.Sun)?.HouseNumber,Find(c,PlanetName.Mars)?.HouseNumber,Find(c,PlanetName.Saturn)?.HouseNumber];
  return houses.All(h=>h.HasValue)&&new HashSet<int>(houses.Select(h=>h!.Value)).SetEquals([1,7,12]);
 }

 // 282 (Bharyasahavyabhichara): "Venus, Saturn and Mars must join the Moon in the 7th house."
 private static bool Y282(ChartAnalysisInput c)
 {
  var moon=Find(c,PlanetName.Moon);
  return moon is not null&&moon.HouseNumber==7&&Conjunct(c,PlanetName.Moon,PlanetName.Venus)&&Conjunct(c,PlanetName.Moon,PlanetName.Saturn)&&Conjunct(c,PlanetName.Moon,PlanetName.Mars);
 }

 // 283 (Vamsacheda): "The 10th, 7th and 4th must be occupied by the Moon, Venus and malefics
 // respectively."
 private static bool Y283(ChartAnalysisInput c)=>
  Find(c,PlanetName.Moon)?.HouseNumber==10&&Find(c,PlanetName.Venus)?.HouseNumber==7&&M.Any(m=>Find(c,m)?.HouseNumber==4);

 // 284 (Guhyaroga): "The Moon should join malefics in the Navamsa of Cancer or Scorpio."
 private static bool? Y284(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(d9 is null)return null;
  if(Find(d9,PlanetName.Moon)?.Sign is not("Cancer" or "Scorpio"))return false;
  return M.Any(m=>Conjunct(c,PlanetName.Moon,m));
 }

 // 285 (Angaheena): "When the Moon is in the 10th, Mars in the 7th and Saturn in the 2nd from
 // the Sun."
 private static bool Y285(ChartAnalysisInput c)
 {
  if(Find(c,PlanetName.Moon)?.HouseNumber!=10||Find(c,PlanetName.Mars)?.HouseNumber!=7)return false;
  var sun=Find(c,PlanetName.Sun);var saturn=Find(c,PlanetName.Saturn);
  return sun is not null&&saturn is not null&&Distance(sun.Sign,saturn.Sign)==2;
 }

 // 286 (Swetakushta): "If Mars and Saturn are in the 2nd and 12th, the Moon in Lagna and the
 // Sun in the 7th."
 private static bool Y286(ChartAnalysisInput c)=>
  Find(c,PlanetName.Mars)?.HouseNumber==2&&Find(c,PlanetName.Saturn)?.HouseNumber==12&&Find(c,PlanetName.Moon)?.HouseNumber==1&&Find(c,PlanetName.Sun)?.HouseNumber==7;

 // 287 (Pisacha Grastha): "When Rahu is in Lagna in conjunction with the Moon and the malefics
 // join trines."
 private static bool Y287(ChartAnalysisInput c)=>
  Find(c,PlanetName.Rahu)?.HouseNumber==1&&Conjunct(c,PlanetName.Rahu,PlanetName.Moon)&&M.All(p=>Find(c,p)?.HouseNumber is 1 or 5 or 9);

 // 288 (Andha, 1st form): "The Sun must rise in Lagna in conjunction with Rahu and malefics
 // should be disposed in thrikonas."
 private static bool Y288(ChartAnalysisInput c)=>
  Find(c,PlanetName.Sun)?.HouseNumber==1&&Conjunct(c,PlanetName.Sun,PlanetName.Rahu)&&new[]{PlanetName.Mars,PlanetName.Saturn}.All(p=>Find(c,p)?.HouseNumber is 5 or 9);

 // 289 (Andha, 2nd form): "Mars, the Moon, Saturn and the Sun should respectively occupy the
 // 2nd, 6th, 12th and 8th."
 private static bool Y289(ChartAnalysisInput c)=>
  Find(c,PlanetName.Mars)?.HouseNumber==2&&Find(c,PlanetName.Moon)?.HouseNumber==6&&Find(c,PlanetName.Saturn)?.HouseNumber==12&&Find(c,PlanetName.Sun)?.HouseNumber==8;

 // 290 (Vatharoga): "When Jupiter is in Lagna and Saturn in the 7th house."
 private static bool Y290(ChartAnalysisInput c)=>Find(c,PlanetName.Jupiter)?.HouseNumber==1&&Find(c,PlanetName.Saturn)?.HouseNumber==7;

 // 291 (Matibhramana, 1st form): "Jupiter and Mars should occupy the Lagna and the 7th
 // respectively."
 private static bool Y291(ChartAnalysisInput c)=>Find(c,PlanetName.Jupiter)?.HouseNumber==1&&Find(c,PlanetName.Mars)?.HouseNumber==7;

 // 292 (Matibhramana, 2nd form): "Saturn must be in Lagna and Mars should join the 9th, 5th or
 // 7th."
 private static bool Y292(ChartAnalysisInput c)=>Find(c,PlanetName.Saturn)?.HouseNumber==1&&Find(c,PlanetName.Mars)?.HouseNumber is 5 or 7 or 9;

 // 293 (Matibhramana, 3rd form): "Saturn must occupy the 12th with the waning Moon."
 private static bool Y293(ChartAnalysisInput c)=>Find(c,PlanetName.Saturn)?.HouseNumber==12&&Conjunct(c,PlanetName.Saturn,PlanetName.Moon)&&IsWaning(c);

 // 294 (Matibhramana, 4th form): "The Moon and Mercury should be in a kendra, aspected by or
 // conjoined with any other planet."
 private static bool Y294(ChartAnalysisInput c)
 {
  var moon=Find(c,PlanetName.Moon);var merc=Find(c,PlanetName.Mercury);
  if(moon is null||merc is null||moon.HouseNumber!=merc.HouseNumber||moon.HouseNumber is not(1 or 4 or 7 or 10))return false;
  var others=new[]{PlanetName.Sun,PlanetName.Mars,PlanetName.Jupiter,PlanetName.Venus,PlanetName.Saturn,PlanetName.Rahu,PlanetName.Ketu};
  return others.Any(p=>Influenced(c,PlanetName.Moon,p));
 }

 // 295 (Khalwata): "The ascendant must be a malefic sign or Sagittarius or Taurus aspected by
 // malefic planets."
 private static bool Y295(ChartAnalysisInput c)
 {
  var malefic=new[]{ZodiacName.Aries,ZodiacName.Leo,ZodiacName.Scorpio,ZodiacName.Capricornus,ZodiacName.Aquarius,ZodiacName.Sagittarius,ZodiacName.Taurus};
  return malefic.Contains(c.AscendantSign)&&M.Any(m=>InfluencesSign(c,m,c.AscendantSign.ToString()));
 }

 // 296 (Nishturabhashi): "The Moon must be in conjunction with Saturn."
 private static bool Y296(ChartAnalysisInput c)=>Conjunct(c,PlanetName.Moon,PlanetName.Saturn);

 // 297 (Rajabhrashta): "The lords of Aroodha Lagna and Aroodha Dwadasa should be in
 // conjunction." Arudha Lagna and the 12th-house Arudha are already computed as the "AL"/"A12"
 // special points (gap-and-coverage.md's delivered "AL + 12 Arudhas").
 private static bool Y297(ChartAnalysisInput c)
 {
  var al=Find(c,"AL");var a12=Find(c,"A12");
  if(al is null||a12 is null)return false;
  var alLord=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(al.Sign)));
  var a12Lord=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(a12.Sign)));
  return Conjunct(c,alLord,a12Lord);
 }

 // 298 (Raja Yoga Bhanga, 1st form): "The ascendant being Leo, Saturn must be in exaltation
 // occupying a debilitated Navamsa or aspected by benefics."
 private static bool? Y298(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(c.AscendantSign!=ZodiacName.Leo||Dignity(c,PlanetName.Saturn)!="EXALTED")return false;
  if(B.Any(p=>Influenced(c,PlanetName.Saturn,p)))return true;
  return d9 is null?null:Dignity(d9,PlanetName.Saturn)=="DEBILITATED";
 }

 // 299 (Raja Yoga Bhanga, 2nd form): "The Sun must occupy the 10th degree of Libra."
 private static bool Y299(ChartAnalysisInput c)
 {
  var sun=Find(c,PlanetName.Sun);
  if(sun?.NirayanaLongitudeDegrees is not double lon||sun.Sign!="Libra")return false;
  var degreeInSign=((lon%30)+30)%30;
  return Math.Abs(degreeInSign-10)<0.000001;
 }

 // 300 (Gohanta): "A malefic devoid of benefic aspect in a kendra and Jupiter in the 8th house."
 private static bool Y300(ChartAnalysisInput c)=>
  Find(c,PlanetName.Jupiter)?.HouseNumber==8&&M.Any(m=>Find(c,m)?.HouseNumber is 1 or 4 or 7 or 10&&!B.Any(b=>Influenced(c,m,b)));

 private static bool IsWaning(ChartAnalysisInput c)
 {
  var moon=Find(c,PlanetName.Moon);var sun=Find(c,PlanetName.Sun);
  if(moon?.NirayanaLongitudeDegrees is not double ml||sun?.NirayanaLongitudeDegrees is not double sl)return false;
  return (((ml-sl)%360)+360)%360>180;
 }
 private static bool CruelShashtiamsa(ChartAnalysisInput c,PlanetName p)
 {
  var x=Find(c,p);if(x?.NirayanaLongitudeDegrees is not double lon)return false;
  return ShashtiamsaDeityTable.Lookup(Enum.Parse<ZodiacName>(x.Sign),((lon%30)+30)%30).IsMalefic;
 }
 private static PlanetName? NavamsaLord(ChartAnalysisInput d9,PlanetName p){var x=Find(d9,p);return x is null?null:Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(x.Sign)));}
 private static bool Conjunct(ChartAnalysisInput c,PlanetName a,PlanetName b){var pa=Find(c,a);var pb=Find(c,b);return pa is not null&&pb is not null&&pa.Sign==pb.Sign;}
 private static bool Conjunct(ChartAnalysisInput c,PlanetName a,string b){var pa=Find(c,a);var pb=Find(c,b);return pa is not null&&pb is not null&&pa.Sign==pb.Sign;}
 private static bool Influenced(ChartAnalysisInput c,PlanetName target,PlanetName source)=>Find(c,target) is{}x&&Find(c,source) is{}y&&(x.Sign==y.Sign||Aspects(source,y.Sign,x.Sign));
 private static bool InfluencesSign(ChartAnalysisInput c,PlanetName source,string sign)=>Find(c,source) is{}x&&(x.Sign==sign||Aspects(source,x.Sign,sign));
 private static bool Aspects(PlanetName p,string a,string b){var d=Distance(a,b);return d==7||p==PlanetName.Mars&&d is 4 or 8||p==PlanetName.Jupiter&&d is 5 or 9||p==PlanetName.Saturn&&d is 3 or 10;}
 private static int Distance(string? a,string? b)=>a is null||b is null?0:(((int)Enum.Parse<ZodiacName>(b)-(int)Enum.Parse<ZodiacName>(a)+12)%12)+1;
 private static string Dignity(ChartAnalysisInput c,PlanetName p){var x=Find(c,p);if(x is null)return"MISSING";var l=x.VargaLongitudeDegrees??x.NirayanaLongitudeDegrees??15;return PvrDignityEvaluator.Evaluate(p,Enum.Parse<ZodiacName>(x.Sign),((l%30)+30)%30).DignityTypeCode;}
 private static PlanetName Lord(ChartAnalysisInput c,int h)=>Enum.Parse<PlanetName>(HouseEngine.GetSignLord(HouseEngine.GetHouseSign(c.AscendantSign,h)));
 private static PlanetPosition? Find(ChartAnalysisInput c,PlanetName p)=>c.Planets.SingleOrDefault(x=>x.Planet==p.ToString());
 private static PlanetPosition? Find(ChartAnalysisInput c,string p)=>c.Planets.SingleOrDefault(x=>x.Planet.Equals(p,StringComparison.OrdinalIgnoreCase));
 private static ContextualYogaResult Row(int n,bool present){var scan=Scan(n);return new(Codes[n],present,"EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{scan-12}; scan p.{scan}");}
 private static ContextualYogaResult Missing(int n,string note){var scan=Scan(n);return new(Codes[n],null,"NOT_EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{scan-12}; scan p.{scan}",note);}
 private static int Scan(int n)=>n switch
 {
  264 or 265=>272,266=>275,267 or 268 or 269=>277,270=>279,271=>286,272=>286,273=>287,274=>289,
  275=>290,276 or 277=>291,278=>292,279=>293,280=>294,281 or 282=>295,283=>297,284=>298,
  285=>299,286=>300,287=>301,288 or 289=>302,290=>303,291 or 292 or 293 or 294=>305,
  295=>312,296 or 297=>313,298 or 299=>314,300=>316,
  _=>272
 };
}
