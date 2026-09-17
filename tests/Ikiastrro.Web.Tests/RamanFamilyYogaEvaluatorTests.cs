using Ikiastrro.Core.Engines.Astronomy;using Ikiastrro.Core.Engines.Yoga;using Ikiastrro.Core.Models;using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class RamanFamilyYogaEvaluatorTests
{
 [Fact]public void Tracks_Every_Number_201_Through_219(){var r=RamanFamilyYogaEvaluator.Evaluate(Bundle(Chart()));Assert.Equal(19,r.Count);Assert.Equal(Enumerable.Range(201,19).Select(n=>$"RAMAN_300_{n:000}"),r.Select(x=>x.SourceVariantCode));}
 [Fact]public void Combination_201_Needs_Seventh_Lord_And_Venus_In_Fourth_Afflicted_By_A_Malefic()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Saturn","Libra",4),P("Venus","Libra",4),P("Mars","Libra",4)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),201).Present);
 }
 [Fact]public void Combination_202_Needs_A_Malefic_In_Fourth_And_An_Afflicted_Fourth_Lord()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Cancer",4),P("Moon","Leo",2),P("Saturn","Leo",2)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),202).Present);
 }
 [Fact]public void Combination_203_Needs_Saturn_Mars_Rahu_In_Fourth_And_An_Afflicted_Malefic_Tenth_Lord()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Saturn","Cancer",4),P("Mars","Cancer",4),P("Rahu","Cancer",4)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),203).Present);
 }
 [Fact]public void Combination_204_Needs_Fourth_Lord_Joined_To_Saturn_Maandi_And_Rahu()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Leo",4),P("Saturn","Leo",4),new PlanetPosition{Planet="Maandi",Sign="Leo",HouseNumber=4},P("Rahu","Leo",4)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),204).Present);
 }
 [Fact]public void Combination_205_Needs_A_Benefic_In_Fourth(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Sagittarius",4)]);Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),205).Present);}
 [Fact]public void Combination_206_Needs_Lagna_Lord_In_Fourth_With_A_Benefic()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Leo",4),P("Jupiter","Leo",4)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),206).Present);
 }
 [Fact]public void Combination_207_Needs_Mercury_As_Lagna_And_Fourth_Lord_Afflicted()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Gemini,[P("Mercury","Leo",1),P("Saturn","Leo",1)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),207).Present);
 }
 [Fact]public void Combination_208_Needs_A_Common_Lord_Between_Lagna_And_Fourth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Gemini,[]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),208).Present);
 }
 [Fact]public void Combination_209_Needs_Lagna_Lord_In_Fourth_Ninth_Or_Eleventh(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Leo",4)]);Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),209).Present);}
 [Fact]public void Combination_210_Needs_An_Exalted_Fourth_Lord_And_Its_Exaltation_Signs_Lord_In_Kendra()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[new PlanetPosition{Planet="Moon",Sign="Taurus",HouseNumber=5,NirayanaLongitudeDegrees=31},P("Venus","Leo",1)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),210).Present);
 }
 [Fact]public void Combination_211_Needs_Jupiter_And_Three_Lords_All_Weak()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),211).Present);
 }
 [Fact]public void Combination_212_Needs_Rahu_In_Fifth_Aspected_By_Mars()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Rahu","Leo",5),P("Mars","Leo",5)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),212).Present);
 }
 [Fact]public void Combination_213_Needs_Fifth_Lord_With_Rahu_And_Saturn_In_Fifth_With_Moon()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Cancer",4),P("Rahu","Cancer",4),P("Saturn","Leo",5),P("Moon","Leo",5)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),213).Present);
 }
 [Fact]public void Combination_214_Needs_Jupiter_With_Mars_Rahu_In_Lagna_And_Fifth_Lord_In_Dusthana()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Cancer",2),P("Mars","Cancer",2),P("Rahu","Aries",1),P("Sun","Virgo",6)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),214).Present);
 }
 [Fact]public void Combination_215_Needs_A_Mars_Sign_Fifth_With_Rahu_And_Mercury()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Sagittarius,[P("Rahu","Aries",5),P("Mercury","Aries",5)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),215).Present);
 }
 [Fact]public void Combination_216_Needs_A_Debilitated_Sun_In_The_Fifth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Libra",5)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),216).Present);
 }
 [Fact]public void Combination_217_Needs_Fifth_And_Eighth_Lords_Exchanged_With_Moon_In_Sixth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Capricornus",5),P("Sun","Capricornus",8),P("Moon","Capricornus",6)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),217).Present);
 }
 [Fact]public void Combination_218_Needs_Lagna_And_Fifth_Lords_In_Eighth_With_Third_Lord_Joined_To_Mars_And_Rahu()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Leo",8),P("Sun","Capricornus",8),P("Mercury","Leo",5),P("Rahu","Leo",9)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),218).Present);
 }
 [Fact]public void Combination_219_Needs_Sun_Saturn_Fifth_Weak_Moon_Seventh_Rahu_Lagna_Jupiter_Twelfth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Capricornus",5),P("Saturn","Capricornus",5),P("Moon","Gemini",7),P("Rahu","Aries",1),P("Jupiter","Virgo",12)]);
  Assert.True(Row(RamanFamilyYogaEvaluator.Evaluate(Bundle(d1)),219).Present);
 }
 private static ContextualYogaResult Row(IEnumerable<ContextualYogaResult>r,int n)=>r.Single(x=>x.SourceVariantCode==$"RAMAN_300_{n:000}");
 private static ChartBundle Bundle(params ChartAnalysisInput[]c)=>new(new BirthDetails(),new SiderealPositions(0,new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),0,0),new SunTimes(default,default,default,false),c,new Dictionary<string,string>(),[]);
 private static ChartAnalysisInput Chart(params PlanetPosition[]p)=>new("D1",ZodiacName.Aries,p.ToList());
 private static PlanetPosition P(string p,string s,int h)=>new(){Planet=p,Sign=s,HouseNumber=h};
}
