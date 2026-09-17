using Ikiastrro.Core.Engines.Astronomy;using Ikiastrro.Core.Engines.Yoga;using Ikiastrro.Core.Models;using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class RamanAfflictionYogaEvaluatorTests
{
 [Fact]public void Tracks_Every_Number_264_Through_300(){var r=RamanAfflictionYogaEvaluator.Evaluate(Bundle(Chart()));Assert.Equal(37,r.Count);Assert.Equal(Enumerable.Range(264,37).Select(n=>$"RAMAN_300_{n:000}"),r.Select(x=>x.SourceVariantCode));}
 [Fact]public void Combination_264_Needs_Maandi_And_Rahu_In_Third(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[new PlanetPosition{Planet="Maandi",Sign="Leo",HouseNumber=3},P("Rahu","Leo",3)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),264).Present);}
 [Fact]public void Combination_265_Needs_A_Malefic_Sixth_Lord_In_Lagna_Eighth_Or_Tenth(){var d1=new ChartAnalysisInput("D1",ZodiacName.Leo,[P("Saturn","Scorpio",1)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),265).Present);}
 [Fact]public void Combination_266_Needs_Mercury_In_Lagna_With_Sixth_And_Eighth_Lords(){var d1=new ChartAnalysisInput("D1",ZodiacName.Leo,[P("Mercury","Scorpio",1),P("Saturn","Scorpio",6),P("Jupiter","Scorpio",8)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),266).Present);}
 [Fact]public void Combination_267_Needs_Seventh_Lord_In_Sixth_With_Venus(){var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Saturn","Pisces",6),P("Venus","Pisces",7)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),267).Present);}
 [Fact]public void Combination_268_Needs_Lagna_Lord_In_Fourth_Or_Twelfth_With_Mars_And_Mercury(){var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Moon","Leo",4),P("Mars","Leo",1),P("Mercury","Leo",5)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),268).Present);}
 [Fact]public void Combination_269_Needs_Jupiter_In_Sixth_With_Saturn_And_Moon(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Leo",6),P("Saturn","Leo",1),P("Moon","Leo",4)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),269).Present);}
 [Fact]public void Combination_270_Needs_Rahu_Sixth_Maandi_Kendra_Lagna_Lord_Eighth(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Rahu","Leo",6),new PlanetPosition{Planet="Maandi",Sign="Leo",HouseNumber=1},P("Mars","Leo",8)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),270).Present);}
 [Fact]public void Combination_271_Needs_Lagna_And_Sixth_Lords_In_Kendra_Or_Trikona_With_An_Afflictor(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Leo",1),P("Saturn","Leo",1),P("Mercury","Scorpio",5),P("Rahu","Scorpio",5)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),271).Present);}
 [Fact]public void Combination_272_Needs_Saturn_Ninth_Jupiter_Third(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Saturn","Leo",9),P("Jupiter","Leo",3)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),272).Present);}
 [Fact]public void Combination_273_Needs_Sixth_Lord_With_Venus_And_Saturn_Rahu_In_A_Cruel_Shashtiamsha()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,
  [P("Mercury","Leo",3),P("Venus","Leo",3),new PlanetPosition{Planet="Saturn",Sign="Aries",HouseNumber=9,NirayanaLongitudeDegrees=0},P("Rahu","Aries",9)]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),273).Present);
 }
 [Fact]public void Combination_274_Needs_Moon_Afflicted_By_Lagna_Lord_And_Saturn_In_A_Dusthana(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Leo",6),P("Mars","Leo",2),P("Saturn","Leo",9)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),274).Present);}
 [Fact]public void Combination_275_Needs_Sixth_Or_Eighth_Lord_With_Third_Lord_And_A_Cruel_Shashtiamsha()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Jupiter","Scorpio",6),P("Mercury","Scorpio",3),new PlanetPosition{Planet="Rahu",Sign="Aries",HouseNumber=9,NirayanaLongitudeDegrees=0}]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),275).Present);
 }
 [Fact]public void Combination_276_Needs_Two_Martian_Cruel_Malefics_In_Eighth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,
  [P("Mars","Scorpio",8),new PlanetPosition{Planet="Saturn",Sign="Scorpio",HouseNumber=8,NirayanaLongitudeDegrees=210}]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),276).Present);
 }
 [Fact]public void Combination_277_Needs_Sun_Rahu_Saturn_Aspected_By_Eighth_Lord_All_In_Cruel_Amsas()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,
  [new PlanetPosition{Planet="Mars",Sign="Leo",HouseNumber=8},
   new PlanetPosition{Planet="Sun",Sign="Leo",HouseNumber=2,NirayanaLongitudeDegrees=120},
   new PlanetPosition{Planet="Rahu",Sign="Leo",HouseNumber=3,NirayanaLongitudeDegrees=120},
   new PlanetPosition{Planet="Saturn",Sign="Leo",HouseNumber=4,NirayanaLongitudeDegrees=120}]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),277).Present);
 }
 [Fact]public void Combination_278_Needs_Moon_Saturn_Malefic_In_Sixth_Eighth_Twelfth_And_Malefic_Navamsa_Lagna_Lord()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Cancer",6),P("Saturn","Cancer",8),P("Mars","Cancer",12)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Mars","Leo",1)]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1,d9)),278).Present);
 }
 [Fact]public void Combination_279_Needs_Sun_Sixth_With_One_Malefic_Aspected_By_Another(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Leo",6),P("Mars","Leo",6),P("Saturn","Leo",6)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),279).Present);}
 [Fact]public void Combination_280_Needs_Venus_And_Sun_Together_In_Fifth_Seventh_Or_Ninth(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Venus","Leo",5),P("Sun","Leo",5)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),280).Present);}
 [Fact]public void Combination_281_Needs_Waning_Moon_Fifth_And_Malefics_Across_Lagna_Seventh_Twelfth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,
  [new PlanetPosition{Planet="Moon",Sign="Leo",HouseNumber=5,NirayanaLongitudeDegrees=200},
   new PlanetPosition{Planet="Sun",Sign="Aries",HouseNumber=1,NirayanaLongitudeDegrees=0},
   P("Mars","Leo",7),P("Saturn","Leo",12)]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),281).Present);
 }
 [Fact]public void Combination_282_Needs_Venus_Saturn_Mars_Joining_Moon_In_Seventh(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Leo",7),P("Venus","Leo",7),P("Saturn","Leo",7),P("Mars","Leo",7)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),282).Present);}
 [Fact]public void Combination_283_Needs_Moon_Tenth_Venus_Seventh_Malefic_Fourth(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Leo",10),P("Venus","Leo",7),P("Mars","Leo",4)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),283).Present);}
 [Fact]public void Combination_284_Needs_Moon_In_Cancer_Or_Scorpio_Navamsa_With_A_Malefic()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Leo",3),P("Mars","Leo",3)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Moon","Cancer",1)]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1,d9)),284).Present);
 }
 [Fact]public void Combination_285_Needs_Moon_Tenth_Mars_Seventh_Saturn_Second_From_Sun()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Leo",10),P("Mars","Leo",7),P("Sun","Aries",3),P("Saturn","Taurus",9)]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),285).Present);
 }
 [Fact]public void Combination_286_Needs_Mars_Second_Saturn_Twelfth_Moon_Lagna_Sun_Seventh(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Leo",2),P("Saturn","Leo",12),P("Moon","Leo",1),P("Sun","Leo",7)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),286).Present);}
 [Fact]public void Combination_287_Needs_Rahu_Moon_Lagna_And_Malefics_In_Trikonas(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Rahu","Leo",1),P("Moon","Leo",1),P("Sun","Leo",5),P("Mars","Leo",9),P("Saturn","Leo",1)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),287).Present);}
 [Fact]public void Combination_288_Needs_Sun_Lagna_With_Rahu_And_Malefics_In_Fifth_Ninth(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Leo",1),P("Rahu","Leo",1),P("Mars","Leo",5),P("Saturn","Leo",9)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),288).Present);}
 [Fact]public void Combination_289_Needs_Mars_Second_Moon_Sixth_Saturn_Twelfth_Sun_Eighth(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Leo",2),P("Moon","Leo",6),P("Saturn","Leo",12),P("Sun","Leo",8)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),289).Present);}
 [Fact]public void Combination_290_Needs_Jupiter_Lagna_Saturn_Seventh(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Leo",1),P("Saturn","Leo",7)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),290).Present);}
 [Fact]public void Combination_291_Needs_Jupiter_Lagna_Mars_Seventh(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Leo",1),P("Mars","Leo",7)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),291).Present);}
 [Fact]public void Combination_292_Needs_Saturn_Lagna_Mars_Fifth_Seventh_Or_Ninth(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Saturn","Leo",1),P("Mars","Leo",5)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),292).Present);}
 [Fact]public void Combination_293_Needs_Saturn_Twelfth_With_Waning_Moon()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,
  [new PlanetPosition{Planet="Saturn",Sign="Leo",HouseNumber=12},
   new PlanetPosition{Planet="Moon",Sign="Leo",HouseNumber=12,NirayanaLongitudeDegrees=200},
   new PlanetPosition{Planet="Sun",Sign="Aries",HouseNumber=1,NirayanaLongitudeDegrees=0}]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),293).Present);
 }
 [Fact]public void Combination_294_Needs_Moon_And_Mercury_In_A_Kendra_With_Any_Other_Planet(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Leo",1),P("Mercury","Leo",1),P("Jupiter","Leo",1)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),294).Present);}
 [Fact]public void Combination_295_Needs_A_Malefic_Ascendant_Aspected_By_A_Malefic(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Aries",5)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),295).Present);}
 [Fact]public void Combination_296_Needs_Moon_Conjunct_Saturn(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Leo",1),P("Saturn","Leo",1)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),296).Present);}
 [Fact]public void Combination_297_Needs_Aroodha_Lagna_And_Aroodha_Dwadasa_Lords_Conjunct()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,
  [new PlanetPosition{Planet="AL",Sign="Leo",HouseNumber=5},new PlanetPosition{Planet="A12",Sign="Scorpio",HouseNumber=8},
   P("Sun","Cancer",1),P("Mars","Cancer",1)]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),297).Present);
 }
 [Fact]public void Combination_298_Needs_Leo_Lagna_Exalted_Saturn_Aspected_By_A_Benefic()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Leo,[P("Saturn","Libra",3),P("Jupiter","Libra",3)]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),298).Present);
 }
 [Fact]public void Combination_299_Needs_The_Sun_At_The_Tenth_Degree_Of_Libra()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[new PlanetPosition{Planet="Sun",Sign="Libra",HouseNumber=4,NirayanaLongitudeDegrees=190}]);
  Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),299).Present);
 }
 [Fact]public void Combination_300_Needs_An_Unaspected_Malefic_In_Kendra_And_Jupiter_Eighth(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Leo",8),P("Mars","Cancer",1)]);Assert.True(Row(RamanAfflictionYogaEvaluator.Evaluate(Bundle(d1)),300).Present);}
 private static ContextualYogaResult Row(IEnumerable<ContextualYogaResult>r,int n)=>r.Single(x=>x.SourceVariantCode==$"RAMAN_300_{n:000}");
 private static ChartBundle Bundle(params ChartAnalysisInput[]c)=>new(new BirthDetails(),new SiderealPositions(0,new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),0,0),new SunTimes(default,default,default,false),c,new Dictionary<string,string>(),[]);
 private static ChartAnalysisInput Chart(params PlanetPosition[]p)=>new("D1",ZodiacName.Aries,p.ToList());
 private static PlanetPosition P(string p,string s,int h)=>new(){Planet=p,Sign=s,HouseNumber=h};
}
