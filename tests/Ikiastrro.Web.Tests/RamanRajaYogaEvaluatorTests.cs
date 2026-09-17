using Ikiastrro.Core.Engines.Astronomy;using Ikiastrro.Core.Engines.Yoga;using Ikiastrro.Core.Models;using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class RamanRajaYogaEvaluatorTests
{
 [Fact]public void Tracks_Every_Number_245_Through_263(){var r=RamanRajaYogaEvaluator.Evaluate(Bundle(Chart()));Assert.Equal(19,r.Count);Assert.Equal(Enumerable.Range(245,19).Select(n=>$"RAMAN_300_{n:000}"),r.Select(x=>x.SourceVariantCode));}
 [Fact]public void Combination_245_Needs_Three_Strong_Planets_In_Kendra_Or_Trikona()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Aries",1),P("Jupiter","Cancer",4),P("Mars","Capricornus",10)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),245).Present);
 }
 [Fact]public void Combination_246_Needs_A_Non_Combust_Non_Dusthana_Debilitated_Planet()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Saturn","Aries",1)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),246).Present);
 }
 [Fact]public void Combination_247_Needs_Two_To_Four_Planets_With_Digbala()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mercury","Aries",1),P("Jupiter","Aries",1)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),247).Present);
 }
 [Fact]public void Combination_248_Needs_Kumbha_Lagna_Venus_And_Four_Clean_Exaltations()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aquarius,
  [
   P("Venus","Aquarius",1),
   new PlanetPosition{Planet="Sun",Sign="Aries",HouseNumber=2,NirayanaLongitudeDegrees=1.0},
   new PlanetPosition{Planet="Moon",Sign="Taurus",HouseNumber=3,NirayanaLongitudeDegrees=30.5},
   new PlanetPosition{Planet="Mars",Sign="Capricornus",HouseNumber=5,NirayanaLongitudeDegrees=270.5},
   new PlanetPosition{Planet="Mercury",Sign="Virgo",HouseNumber=6,NirayanaLongitudeDegrees=150.5}
  ]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aquarius,[P("Sun","Cancer",1),P("Moon","Cancer",1),P("Mars","Cancer",1),P("Mercury","Cancer",1)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1,d9)),248).Present);
 }
 [Fact]public void Combination_249_Needs_Moon_Jupiter_Venus_In_Place_And_Saturn_Strong()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Aries",1),P("Jupiter","Cancer",4),P("Venus","Capricornus",10),P("Saturn","Libra",7)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),249).Present);
 }
 [Fact]public void Combination_250_Follows_Ramans_Own_Reading_Of_Thaduchchanatha()
 {
  // Raman's own worked example: Sun neecha in Libra, Saturn (who exalts in Libra) in a kendra.
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Libra",7),P("Saturn","Aries",1)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),250).Present);
 }
 [Fact]public void Combination_251_Needs_A_Strong_Moon_In_A_Non_Lagna_Kendra_Aspected_By_Jupiter()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Cancer",4),P("Jupiter","Cancer",4)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),251).Present);
 }
 [Fact]public void Combination_252_Needs_A_Debilitated_Planet_Exalted_In_Navamsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Saturn","Aries",1)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Saturn","Libra",1)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1,d9)),252).Present);
 }
 [Fact]public void Combination_252_Is_Not_Evaluated_Without_D9()
 {
  var row=Row(RamanRajaYogaEvaluator.Evaluate(Bundle(Chart())),252);
  Assert.Equal("NOT_EVALUATED",row.EvaluationStatus);
 }
 [Fact]public void Combination_253_Follows_Ramans_Own_Self_Lordship_Reading()
 {
  // Mercury is himself the 11th lord, so "aspected by the 11th lord" is auto-satisfied (Raman's own note).
  var d1=new ChartAnalysisInput("D1",ZodiacName.Leo,[P("Jupiter","Leo",1),P("Mercury","Leo",1),P("Mars","Leo",1)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),253).Present);
 }
 [Fact]public void Combination_254_Needs_Exalted_Saturn_In_Kendra_Or_Trikona_Aspected_By_Tenth_Lord()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Saturn","Libra",4),P("Mars","Libra",4)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),254).Present);
 }
 [Fact]public void Combination_255_Needs_Moon_Mars_Conjunction_In_Second_Or_Third_With_Rahu_In_Fifth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Taurus",2),P("Mars","Taurus",2),P("Rahu","Leo",5)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),255).Present);
 }
 [Fact]public void Combination_256_Needs_Tenth_Lord_In_Ninth_Favoured_Navamsa_And_Uttamamsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Saturn","Capricornus",9)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Saturn","Aquarius",1)]);
  var extra=new[]{"D2","D3","D4"}.Select(t=>new ChartAnalysisInput(t,ZodiacName.Aries,[P("Saturn","Aquarius",1)]));
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle([d1,d9,..extra])),256).Present);
 }
 [Fact]public void Combination_257_Needs_Jupiter_In_Fifth_Kendra_From_Moon_Fixed_Lagna_And_Lord_In_Tenth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Leo,[P("Jupiter","Sagittarius",5),P("Moon","Sagittarius",5),P("Sun","Gemini",10)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),257).Present);
 }
 [Fact]public void Combination_258_Follows_The_Moons_Navamsa_Dispositor_To_A_Kendra()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Leo",1)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Moon","Leo",1)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1,d9)),258).Present);
 }
 [Fact]public void Combination_259_Needs_The_Exact_Vrishabha_Lagna_Placement()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Taurus,[P("Moon","Taurus",1),P("Saturn","Aquarius",10),P("Sun","Leo",4),P("Jupiter","Scorpio",7)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),259).Present);
 }
 [Fact]public void Combination_260_Needs_A_Movable_Lagna_Lord_And_A_Movable_Navamsa_Dispositor_In_Kendra()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Moon","Aries",3),P("Mars","Cancer",1)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Cancer,[P("Mars","Aries",1)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1,d9)),260).Present);
 }
 [Fact]public void Combination_261_Needs_Lagna_Lord_With_Debilitated_Planet_And_Rahu_Saturn_In_Tenth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Capricornus",10),P("Jupiter","Capricornus",10),P("Rahu","Capricornus",10),P("Saturn","Capricornus",10)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),261).Present);
 }
 [Fact]public void Combination_262_Needs_A_Kendra_From_Moon_And_Jupiters_Wealth_Lordship()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Leo,[P("Moon","Leo",3),P("Mercury","Leo",3)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),262).Present);
 }
 [Fact]public void Combination_263_Needs_A_Natural_Friend_Joining_The_Ninth_Free_Of_Combustion()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Sagittarius",9),P("Moon","Sagittarius",9)]);
  Assert.True(Row(RamanRajaYogaEvaluator.Evaluate(Bundle(d1)),263).Present);
 }
 private static ContextualYogaResult Row(IEnumerable<ContextualYogaResult>r,int n)=>r.Single(x=>x.SourceVariantCode==$"RAMAN_300_{n:000}");
 private static ChartBundle Bundle(params ChartAnalysisInput[]c)=>new(new BirthDetails(),new SiderealPositions(0,new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),0,0),new SunTimes(default,default,default,false),c,new Dictionary<string,string>(),[]);
 private static ChartAnalysisInput Chart(params PlanetPosition[]p)=>new("D1",ZodiacName.Aries,p.ToList());
 private static PlanetPosition P(string p,string s,int h)=>new(){Planet=p,Sign=s,HouseNumber=h};
}
