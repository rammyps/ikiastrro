using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Relationships;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Core.Engines.Yoga;

/// <summary>Raman 245–263, the "Raja Yogas" cluster — one catalogued group in
/// RamanFinalHundredCatalog.cs (YOGA_RAJA), the first slice of that catalog to be
/// transcribed (bvyoga.md §4 phasing).</summary>
public static class RamanRajaYogaEvaluator
{
 private static readonly PlanetName[] M=[PlanetName.Sun,PlanetName.Mars,PlanetName.Saturn];
 private static readonly PlanetName[] Classical=
  [PlanetName.Sun,PlanetName.Moon,PlanetName.Mars,PlanetName.Mercury,PlanetName.Jupiter,PlanetName.Venus,PlanetName.Saturn];
 private static readonly int[] KendraTrikona=[1,4,5,7,9,10];
 private static readonly Dictionary<PlanetName,int> DigbalaHouse=new()
 {
  [PlanetName.Sun]=10,[PlanetName.Mars]=10,[PlanetName.Moon]=4,[PlanetName.Venus]=4,
  [PlanetName.Mercury]=1,[PlanetName.Jupiter]=1,[PlanetName.Saturn]=7
 };
 private static readonly Dictionary<ZodiacName,PlanetName> ExaltsIn=
  AstroMath.DeepExaltationPoints.ToDictionary(kv=>kv.Value.Sign,kv=>kv.Key);

 public static IReadOnlyList<ContextualYogaResult> Evaluate(ChartBundle bundle)
 {
  var c=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D1",StringComparison.OrdinalIgnoreCase));if(c is null)return[];
  var d9=bundle.Charts.FirstOrDefault(x=>x.ChartType.Equals("D9",StringComparison.OrdinalIgnoreCase));
  var charts=bundle.Charts;
  bool?[] v=
  [
   Y245(c),Y246(c),Y247(c),Y248(c,d9),Y249(c),Y250(c),Y251(c),Y252(c,d9),Y253(c),
   Y254(c),Y255(c),Y256(c,d9,charts),Y257(c),Y258(c,d9),Y259(c),Y260(c,d9),Y261(c),Y262(c),Y263(c)
  ];
  return Enumerable.Range(245,19)
   .Zip(v,(n,r)=>r is null?Missing(n,"Required divisional or qualification evidence is unavailable."):Row(n,r.Value))
   .ToList();
 }

 // 245: "Three or more planets should be in exaltation or own house occupying kendras."
 // Remarks broaden this: "these three or more planets in swakshetra or uchcha need not
 // necessarily be in kendras. Even thrikonas will do" — read as kendra-or-trikona per Raman's
 // own note.
 private static bool Y245(ChartAnalysisInput c)=>
  Classical.Count(p=>Find(c,p) is{}x&&KendraTrikona.Contains(x.HouseNumber)&&Dignity(c,p) is "EXALTED" or "OWN" or "MOOLATRIKONA")>=3;

 // 246: "When a planet is in debilitation but with bright rays, or retrograde, and occupies
 // favourable positions [not a dusthana], Raja Yoga will be caused." The Sun can never be
 // combust relative to itself, so it trivially satisfies "bright rays."
 private static bool Y246(ChartAnalysisInput c)=>Classical.Any(p=>
 {
  var x=Find(c,p);if(x is null||Dignity(c,p)!="DEBILITATED"||x.HouseNumber is 6 or 8 or 12)return false;
  return p==PlanetName.Sun||x.IsRetrograde==true||!IsCombust(c,p);
 });

 // 247: "Two, three or four planets should possess Digbala." Digbala houses per PVR/Raman:
 // Sun & Mars->10th, Moon & Venus->4th, Mercury & Jupiter->Lagna, Saturn->7th (same table as
 // ShadbalaCalculator.DigBalaHouses).
 private static bool Y247(ChartAnalysisInput c)=>DigbalaHouse.Count(kv=>Find(c,kv.Key)?.HouseNumber==kv.Value) is 2 or 3 or 4;

 // 248: "The Lagna must be Kumbha with Sukra in it and four planets should be exalted without
 // occupying evil navamsas or shastiamsas." "Evil navamsa" = navamsa sign ruled by a natural
 // malefic (same reading as combos 175/176); "evil shashtiamsa" = a malefic-natured
 // shashtiamsha per ShashtiamsaDeityTable.
 private static bool? Y248(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(c.AscendantSign!=ZodiacName.Aquarius||Find(c,PlanetName.Venus)?.HouseNumber!=1)return false;
  if(d9 is null)return null;
  var cleanExalted=Classical.Count(p=>
  {
   if(Dignity(c,p)!="EXALTED")return false;
   var nl=NavamsaLord(d9,p);if(nl is null||M.Contains(nl.Value))return false;
   var x=Find(c,p);if(x?.NirayanaLongitudeDegrees is not double lon)return false;
   return !ShashtiamsaDeityTable.Lookup(Enum.Parse<ZodiacName>(x.Sign),((lon%30)+30)%30).IsMalefic;
  });
  return cleanExalted>=4;
 }

 // 249: "The Moon must be in Lagna, Jupiter in the 4th, Venus in the 10th and Saturn exalted
 // or in his own house."
 private static bool Y249(ChartAnalysisInput c)=>
  Find(c,PlanetName.Moon)?.HouseNumber==1&&Find(c,PlanetName.Jupiter)?.HouseNumber==4&&
  Find(c,PlanetName.Venus)?.HouseNumber==10&&Strong(c,PlanetName.Saturn);

 // 250: "The lord of the sign a planet is debilitated in, or the planet who would be exalted
 // there, should be in a kendra from the Moon or Lagna." Raman's own worked example ("The Sun
 // is neecha, and Saturn, the planet who is to get exalted in the Rasi occupied by the Sun, is
 // in a kendra from Lagna") settles the "thaduchchanatha" controversy in favour of the second
 // reading — implemented as an OR of both, matching the Definition's own wording.
 private static bool Y250(ChartAnalysisInput c)
 {
  foreach(var p in Classical)
  {
   if(Dignity(c,p)!="DEBILITATED")continue;
   var sign=Enum.Parse<ZodiacName>(Find(c,p)!.Sign);
   var l1=Enum.Parse<PlanetName>(HouseEngine.GetSignLord(sign));
   if(KendraFromMoonOrLagna(c,l1))return true;
   if(ExaltsIn.TryGetValue(sign,out var l2)&&KendraFromMoonOrLagna(c,l2))return true;
  }
  return false;
 }

 // 251: "The Moon must be in a kendra other than Lagna and aspected by Jupiter, and otherwise
 // powerful."
 private static bool Y251(ChartAnalysisInput c)
 {
  var moon=Find(c,PlanetName.Moon);
  if(moon is null||moon.HouseNumber is not(4 or 7 or 10)||!Influenced(c,PlanetName.Moon,PlanetName.Jupiter))return false;
  return Strong(c,PlanetName.Moon);
 }

 // 252: "Planets in debilitated Rasis should occupy exalted Navamsas."
 private static bool? Y252(ChartAnalysisInput c,ChartAnalysisInput? d9)=>
  d9 is null?null:Classical.Any(p=>Dignity(c,p)=="DEBILITATED"&&Dignity(d9,p)=="EXALTED");

 // 253: "Jupiter in Lagna and Mercury in a kendra must be aspected respectively by the lords
 // of the 9th and 11th." Raman's own worked example treats "Mercury aspected by the 11th
 // lord" as satisfied when Mercury himself IS the 11th lord ("Since Mercury himself happens to
 // be lord of the 11th, the yoga can be assumed to be present").
 private static bool Y253(ChartAnalysisInput c)
 {
  if(Find(c,PlanetName.Jupiter)?.HouseNumber!=1)return false;
  if(Find(c,PlanetName.Mercury)?.HouseNumber is not(1 or 4 or 7 or 10))return false;
  if(!Influenced(c,PlanetName.Jupiter,Lord(c,9)))return false;
  var eleventhLord=Lord(c,11);
  return eleventhLord==PlanetName.Mercury||Influenced(c,PlanetName.Mercury,eleventhLord);
 }

 // 254: "Saturn in exaltation or Moolathrikona should occupy a kendra or thrikona aspected by
 // the lord of the 10th."
 private static bool Y254(ChartAnalysisInput c)
 {
  var pos=Find(c,PlanetName.Saturn);
  if(pos is null||Dignity(c,PlanetName.Saturn) is not("EXALTED" or "MOOLATRIKONA")||!KendraTrikona.Contains(pos.HouseNumber))return false;
  return Influenced(c,PlanetName.Saturn,Lord(c,10));
 }

 // 255: "The Moon should join Mars in the 2nd or 3rd and Rahu must occupy the 5th."
 private static bool Y255(ChartAnalysisInput c)
 {
  var moon=Find(c,PlanetName.Moon);var mars=Find(c,PlanetName.Mars);
  if(moon is null||mars is null||moon.Sign!=mars.Sign||moon.HouseNumber is not(2 or 3))return false;
  return Find(c,PlanetName.Rahu)?.HouseNumber==5;
 }

 // 256: "The lord of the 10th should occupy an exalted or friendly navamsa in the 9th having
 // attained Uttamamsa [3 of 16 Shodasa Varga own-sign charts]." Raman's own worked example uses
 // the 10th lord's OWN navamsa ("occupying his own (instead of exalted or friendly) Navamsa")
 // as satisfying this, so "exalted or friendly" is read broadly here (own/moolatrikona/exalted,
 // or a natural-friend relationship), matching the project's Favoured() convention.
 private static bool? Y256(ChartAnalysisInput c,ChartAnalysisInput? d9,IReadOnlyList<ChartAnalysisInput> charts)
 {
  var tenthLord=Lord(c,10);
  if(Find(c,tenthLord)?.HouseNumber!=9)return false;
  if(d9 is null)return null;
  if(!Favoured(d9,tenthLord))return false;
  return VaiseshikamsaCalculator.HasAttained(charts,tenthLord,3);
 }

 // 257: "Jupiter must be in the 5th from Lagna and in a kendra from the Moon and the Lagna
 // being a fixed sign the lord should occupy the 10th."
 private static bool Y257(ChartAnalysisInput c)
 {
  var jup=Find(c,PlanetName.Jupiter);if(jup is null||jup.HouseNumber!=5)return false;
  var moon=Find(c,PlanetName.Moon);if(moon is null)return false;
  if(HouseDistance(jup.HouseNumber,moon.HouseNumber) is not(1 or 4 or 7 or 10))return false;
  if(!Fixed(c.AscendantSign.ToString()))return false;
  return Find(c,Lord(c,1))?.HouseNumber==10;
 }

 // 258: "The lord of the navamsa occupied by the Moon should be disposed in a quadrant or
 // trine either from Lagna or Mercury."
 private static bool? Y258(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(d9 is null)return null;
  var nl=NavamsaLord(d9,PlanetName.Moon);if(nl is null)return null;
  var pos=Find(c,nl.Value);if(pos is null)return false;
  if(KendraTrikona.Contains(pos.HouseNumber))return true;
  var mercPos=Find(c,PlanetName.Mercury);if(mercPos is null)return false;
  return KendraTrikona.Contains(HouseDistance(pos.HouseNumber,mercPos.HouseNumber));
 }

 // 259: "The Lagna being Taurus with the Moon in it, Saturn, the Sun and Jupiter must occupy
 // the 10th, 4th and 7th houses respectively."
 private static bool Y259(ChartAnalysisInput c)=>
  c.AscendantSign==ZodiacName.Taurus&&Find(c,PlanetName.Moon)?.HouseNumber==1&&
  Find(c,PlanetName.Saturn)?.HouseNumber==10&&Find(c,PlanetName.Sun)?.HouseNumber==4&&Find(c,PlanetName.Jupiter)?.HouseNumber==7;

 // 260: "The lord of the Navamsa occupied by a debilitated planet should join a quadrant or
 // trine from Lagna which should be a movable sign and the lord of the Lagna should also be in
 // a movable sign."
 private static bool? Y260(ChartAnalysisInput c,ChartAnalysisInput? d9)
 {
  if(d9 is null)return null;
  if(!Movable(Find(c,Lord(c,1))?.Sign))return false;
  foreach(var p in Classical)
  {
   if(Dignity(c,p)!="DEBILITATED")continue;
   var nl=NavamsaLord(d9,p);if(nl is null)continue;
   var pos=Find(c,nl.Value);
   if(pos is not null&&KendraTrikona.Contains(pos.HouseNumber)&&Movable(pos.Sign))return true;
  }
  return false;
 }

 // 261: "The lord of Lagna should join a debilitated planet and Rahu and Saturn should occupy
 // the 10th, aspected by the lord of the 9th."
 private static bool Y261(ChartAnalysisInput c)
 {
  var lagnaLord=Lord(c,1);var llPos=Find(c,lagnaLord);if(llPos is null)return false;
  if(!Classical.Any(p=>p!=lagnaLord&&Dignity(c,p)=="DEBILITATED"&&Find(c,p)?.Sign==llPos.Sign))return false;
  if(Find(c,PlanetName.Rahu)?.HouseNumber!=10||Find(c,PlanetName.Saturn)?.HouseNumber!=10)return false;
  return InfluencesSign(c,Lord(c,9),HouseSign(c,10));
 }

 // 262: "Out of the lords of the 11th, the 9th and the 2nd houses, at least one planet should
 // be in a kendra from the Moon and Jupiter must be lord of the 2nd, 5th or the 11th house."
 private static bool Y262(ChartAnalysisInput c)
 {
  var moon=Find(c,PlanetName.Moon);if(moon is null)return false;
  bool KendraFromMoon(PlanetName p){var x=Find(c,p);return x is not null&&HouseDistance(x.HouseNumber,moon.HouseNumber) is 1 or 4 or 7 or 10;}
  if(!new[]{Lord(c,11),Lord(c,9),Lord(c,2)}.Any(KendraFromMoon))return false;
  return new[]{Lord(c,2),Lord(c,5),Lord(c,11)}.Contains(PlanetName.Jupiter);
 }

 // 263: "Jupiter, Mercury, Venus or the Moon should join the 9th, free from combustion and be
 // aspected by or associated with friendly planets" — "friendly" read as a natural Parashari
 // friend (PvrDignityEvaluator.IsNaturalFriend), not merely a natural benefic.
 private static bool Y263(ChartAnalysisInput c)
 {
  foreach(var x in new[]{PlanetName.Jupiter,PlanetName.Mercury,PlanetName.Venus,PlanetName.Moon})
  {
   var pos=Find(c,x);if(pos is null||pos.HouseNumber!=9||IsCombust(c,x))continue;
   var hasFriendlyAssociation=c.Planets.Any(pp=>
    Enum.TryParse<PlanetName>(pp.Planet,out var other)&&other!=x&&PvrDignityEvaluator.IsNaturalFriend(x,other)&&
    (pp.Sign==pos.Sign||Aspects(other,pp.Sign,pos.Sign)));
   if(hasFriendlyAssociation)return true;
  }
  return false;
 }

 private static bool KendraFromMoonOrLagna(ChartAnalysisInput c,PlanetName p)
 {
  var x=Find(c,p);if(x is null)return false;
  if(x.HouseNumber is 1 or 4 or 7 or 10)return true;
  var moon=Find(c,PlanetName.Moon);
  return moon is not null&&HouseDistance(x.HouseNumber,moon.HouseNumber) is 1 or 4 or 7 or 10;
 }
 private static int HouseDistance(int a,int b)=>((a-b+12)%12)+1;
 private static bool IsCombust(ChartAnalysisInput c,PlanetName p)
 {
  if(!CombustionEngine.IsApplicable(p.ToString()))return false;
  var planet=Find(c,p);var sun=Find(c,PlanetName.Sun);
  if(planet?.NirayanaLongitudeDegrees is not double pl||sun?.NirayanaLongitudeDegrees is not double sl)return false;
  return CombustionEngine.Evaluate(p.ToString(),pl,sl,planet.IsRetrograde).IsCombust;
 }
 private static PlanetName? NavamsaLord(ChartAnalysisInput d9,PlanetName p){var x=Find(d9,p);return x is null?null:Enum.Parse<PlanetName>(HouseEngine.GetSignLord(Enum.Parse<ZodiacName>(x.Sign)));}
 private static bool Strong(ChartAnalysisInput c,PlanetName p)=>Dignity(c,p) is "OWN" or "MOOLATRIKONA" or "EXALTED";
 private static bool Favoured(ChartAnalysisInput c,PlanetName p){var d=FullDignity(c,p);return d.DignityTypeCode is "OWN" or "MOOLATRIKONA" or "EXALTED"||d.RelationshipScore>0;}
 private static PvrDignityResult FullDignity(ChartAnalysisInput c,PlanetName p)
 {
  var x=Find(c,p);if(x is null)return new("MISSING",0,"",null,null,0);
  var lon=x.VargaLongitudeDegrees??x.NirayanaLongitudeDegrees??15;
  var signs=c.Planets.Where(v=>Enum.TryParse<PlanetName>(v.Planet,out _)).ToDictionary(v=>v.Planet,v=>Enum.Parse<ZodiacName>(v.Sign),StringComparer.OrdinalIgnoreCase);
  return PvrDignityEvaluator.Evaluate(p,Enum.Parse<ZodiacName>(x.Sign),((lon%30)+30)%30,signs);
 }
 private static bool Influenced(ChartAnalysisInput c,PlanetName target,PlanetName source)=>Find(c,target) is{}x&&Find(c,source) is{}y&&(x.Sign==y.Sign||Aspects(source,y.Sign,x.Sign));
 private static bool InfluencesSign(ChartAnalysisInput c,PlanetName source,string sign)=>Find(c,source) is{}x&&(x.Sign==sign||Aspects(source,x.Sign,sign));
 private static bool Aspects(PlanetName p,string a,string b){var d=Distance(a,b);return d==7||p==PlanetName.Mars&&d is 4 or 8||p==PlanetName.Jupiter&&d is 5 or 9||p==PlanetName.Saturn&&d is 3 or 10;}
 private static int Distance(string? a,string? b)=>a is null||b is null?0:(((int)Enum.Parse<ZodiacName>(b)-(int)Enum.Parse<ZodiacName>(a)+12)%12)+1;
 private static bool Movable(string? s)=>s is not null&&Enum.Parse<ZodiacName>(s) is ZodiacName.Aries or ZodiacName.Cancer or ZodiacName.Libra or ZodiacName.Capricornus;
 private static bool Fixed(string? s)=>s is not null&&Enum.Parse<ZodiacName>(s) is ZodiacName.Taurus or ZodiacName.Leo or ZodiacName.Scorpio or ZodiacName.Aquarius;
 private static string HouseSign(ChartAnalysisInput c,int h)=>HouseEngine.GetHouseSign(c.AscendantSign,h).ToString();
 private static string Dignity(ChartAnalysisInput c,PlanetName p){var x=Find(c,p);if(x is null)return"MISSING";var l=x.VargaLongitudeDegrees??x.NirayanaLongitudeDegrees??15;return PvrDignityEvaluator.Evaluate(p,Enum.Parse<ZodiacName>(x.Sign),((l%30)+30)%30).DignityTypeCode;}
 private static PlanetName Lord(ChartAnalysisInput c,int h)=>Enum.Parse<PlanetName>(HouseEngine.GetSignLord(HouseEngine.GetHouseSign(c.AscendantSign,h)));
 private static PlanetPosition? Find(ChartAnalysisInput c,PlanetName p)=>c.Planets.SingleOrDefault(x=>x.Planet==p.ToString());
 private static ContextualYogaResult Row(int n,bool present){var scan=Scan(n);return new("YOGA_RAJA",present,"EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{scan-12}; scan p.{scan}");}
 private static ContextualYogaResult Missing(int n,string note){var scan=Scan(n);return new("YOGA_RAJA",null,"NOT_EVALUATED","SRC_RAMAN_300_COMBINATIONS",$"RAMAN_300_{n:000}",$"combination {n}; printed p.{scan-12}; scan p.{scan}",note);}
 private static int Scan(int n)=>n switch
 {
  245 or 246=>249,247=>250,
  248 or 249 or 250=>254,
  251 or 252 or 253=>257,
  254=>261,
  255 or 256 or 257 or 258=>262,
  259 or 260 or 261=>267,
  262 or 263=>270,
  _=>249
 };
}
