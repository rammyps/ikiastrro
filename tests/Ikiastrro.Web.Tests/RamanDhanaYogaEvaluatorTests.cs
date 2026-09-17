using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Yoga;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class RamanDhanaYogaEvaluatorTests
{
 [Fact] public void Returns_118_Through_143(){var r=RamanDhanaYogaEvaluator.Evaluate(Bundle(Chart()));Assert.Equal(26,r.Count);Assert.Equal("RAMAN_300_118",r.First().SourceVariantCode);Assert.Equal("RAMAN_300_143",r.Last().SourceVariantCode);}
 [Fact] public void Combination_121_Uses_Aries_Sun_Fifth_And_Moon_Jupiter_Eleventh(){var c=Chart(P("Sun","Leo",5),P("Moon","Aquarius",11),P("Jupiter","Aquarius",11));Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(c)),121).Present);}
 [Fact] public void Combinations_130_137_139_140_Are_Evaluated_Not_Missing()
 {
  var r=RamanDhanaYogaEvaluator.Evaluate(Bundle(Chart()));
  foreach(var n in new[]{130,137,139,140})
  {
   var row=Row(r,n);
   Assert.Equal("EVALUATED",row.EvaluationStatus);
   Assert.False(row.Present);
  }
 }
 [Fact] public void Combination_130_Needs_Strongest_Lagna_Lord_In_Kendra_With_Jupiter_And_Second_Lord_Vaiseshikamsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Leo,[P("Sun","Leo",1),P("Jupiter","Leo",1)]);
  var charts=new[]{d1}.Concat(OwnSignCharts(PlanetName.Mercury,"Virgo",13)).ToArray();
  Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(charts)),130).Present);
 }
 [Fact] public void Combination_137_Follows_Third_Lord_To_Lagna_Lord_In_Vaiseshikamsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Mercury","Leo",2),P("Jupiter","Leo",2),P("Moon","Leo",1)]);
  var charts=new[]{d1}.Concat(OwnSignCharts(PlanetName.Moon,"Cancer",13)).ToArray();
  Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(charts)),137).Present);
 }
 [Fact] public void Combination_139_Needs_Strong_Second_Lord_And_Lagna_Lord_Vaiseshikamsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Sun","Leo",2),P("Jupiter","Leo",2)]);
  var charts=new[]{d1}.Concat(OwnSignCharts(PlanetName.Moon,"Cancer",13)).ToArray();
  Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(charts)),139).Present);
 }
 [Fact] public void Combination_140_Needs_Strong_Second_Lord_Joined_To_Mars_And_Lagna_Lord_Vaiseshikamsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Sun","Leo",2),P("Mars","Leo",2)]);
  var charts=new[]{d1}.Concat(OwnSignCharts(PlanetName.Moon,"Cancer",13)).ToArray();
  Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(charts)),140).Present);
 }
 [Fact] public void Combination_139_Is_Absent_With_Only_Twelve_Of_Sixteen_Own_Sign_Charts()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Sun","Leo",2),P("Jupiter","Leo",2)]);
  var charts=new[]{d1}.Concat(OwnSignCharts(PlanetName.Moon,"Cancer",12)).ToArray();
  Assert.False(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(charts)),139).Present);
 }
 [Fact] public void Combination_131_Follows_The_Navamsa_Dispositor_Chain_To_A_Strong_Final_Lord()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Moon","Cancer",1),P("Mars","Capricornus",7),P("Saturn","Capricornus",7)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Cancer,[P("Moon","Aries",1)]);
  Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(d1,d9)),131).Present);
 }
 [Fact] public void Combination_131_Is_Not_Evaluated_Without_D9()
 {
  var row=Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(Chart())),131);
  Assert.Equal("NOT_EVALUATED",row.EvaluationStatus);
  Assert.Contains("D9",row.Notes);
 }
 [Fact] public void Combination_133_Reads_Kalabala_From_Raman_Own_Day_Night_Rule()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,
  [P("Moon","Cancer",1),P("Sun","Cancer",1),P("Venus","Cancer",1),P("Jupiter","Pisces",9)]);
  Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(d1)),133).Present);
 }
 [Fact] public void Combination_134_Requires_A_Third_Benefic_Sharing_The_Lords_Sign()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,
  [P("Moon","Scorpio",5),P("Sun","Scorpio",5),P("Mercury","Scorpio",5),P("Mars","Aries",1)]);
  Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(d1)),134).Present);
 }
 [Fact] public void Combination_141_Needs_No_Vaiseshikamsa_Only_Strength_And_Aspect()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,
  [P("Sun","Leo",2),P("Saturn","Leo",7),P("Venus","Leo",11),P("Moon","Taurus",1)]);
  Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(d1)),141).Present);
 }
 [Fact] public void Combination_142_Needs_Multiple_Second_House_Occupants_And_Strong_Wealth_Planets()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,
  [P("Sun","Leo",2),P("Mercury","Gemini",2),P("Jupiter","Sagittarius",6)]);
  Assert.True(Row(RamanDhanaYogaEvaluator.Evaluate(Bundle(d1)),142).Present);
 }
 private static ContextualYogaResult Row(IEnumerable<ContextualYogaResult> r,int n)=>r.Single(x=>x.SourceVariantCode==$"RAMAN_300_{n:000}");
 private static ChartBundle Bundle(params ChartAnalysisInput[] c)=>new(new BirthDetails(),new SiderealPositions(0,new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),0,0),new SunTimes(default,default,default,false),c,new Dictionary<string,string>(),[]);
 private static ChartAnalysisInput Chart(params PlanetPosition[] p)=>new("D1",ZodiacName.Aries,p.ToList());
 private static PlanetPosition P(string p,string s,int h)=>new(){Planet=p,Sign=s,HouseNumber=h};
 // Builds up to 15 additional Shodasa Varga chart entries (D2..D60, never D1 — callers supply
 // their own D1) placing `planet` in `ownSign`, to drive VaiseshikamsaCalculator past a threshold.
 private static ChartAnalysisInput[] OwnSignCharts(PlanetName planet,string ownSign,int count)=>
  new[]{"D2","D3","D4","D7","D9","D10","D12","D16","D20","D24","D27","D30","D40","D45","D60"}
   .Take(count).Select(t=>new ChartAnalysisInput(t,ZodiacName.Aries,[P(planet.ToString(),ownSign,1)])).ToArray();
}
