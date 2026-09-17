using Ikiastrro.Core.Engines.Astronomy;using Ikiastrro.Core.Engines.Yoga;using Ikiastrro.Core.Models;using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class RamanYogaBatchEightTests
{
 [Fact]public void Tracks_Every_Number_151_Through_200(){var r=RamanYogaBatchEightEvaluator.Evaluate(Bundle(Chart()));Assert.Equal(50,r.Count);Assert.Equal(Enumerable.Range(151,50).Select(n=>$"RAMAN_300_{n:000}"),r.Select(x=>x.SourceVariantCode));}
 [Fact]public void Unsupported_Amsa_And_Strength_Rules_Are_Unevaluated(){var r=RamanYogaBatchEightEvaluator.Evaluate(Bundle(Chart()));foreach(var n in new[]{178,179,183,185})Assert.Equal("NOT_EVALUATED",Row(r,n).EvaluationStatus);}
 [Fact]public void Combination_155_Needs_Deep_Exaltation_Parvatamsa_And_A_Benefics_Simhasanamsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Capricornus,[new PlanetPosition{Planet="Saturn",Sign="Libra",HouseNumber=1,NirayanaLongitudeDegrees=200}]);
  var charts=new[]{d1}.Concat(OwnSignCharts(PlanetName.Saturn,"Aquarius",6,0)).Concat(OwnSignCharts(PlanetName.Jupiter,"Sagittarius",5,6)).ToArray();
  Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(charts)),155).Present);
 }
 [Fact]public void Combination_156_Follows_The_Suns_Navamsa_Dispositor_To_Vaiseshikamsa_In_The_Second()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Capricornus,[P("Mars","Aquarius",2)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Capricornus,[P("Sun","Aries",1)]);
  var marsCharts=new[]{"D2","D3","D4","D7","D10","D12","D16","D20","D24","D27","D30","D40","D45"}
   .Select(t=>new ChartAnalysisInput(t,ZodiacName.Aries,[P("Mars","Aries",1)])).ToArray();
  var charts=new[]{d1,d9}.Concat(marsCharts).ToArray();
  Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(charts)),156).Present);
 }
 [Fact]public void Combination_167_Needs_The_Kendra_Signs_Lord_To_Attain_Gopuramsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Sun","Leo",1)]);
  var charts=new[]{d1}.Concat(OwnSignCharts(PlanetName.Sun,"Leo",4,0)).ToArray();
  Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(charts)),167).Present);
 }
 [Fact]public void Combination_170_Needs_The_Second_Lord_In_Vaiseshikamsa_Aspected_By_A_Benefic()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Sun","Leo",1),P("Venus","Leo",1)]);
  var charts=new[]{d1}.Concat(OwnSignCharts(PlanetName.Sun,"Leo",13,0)).ToArray();
  Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(charts)),170).Present);
 }
 [Fact]public void Combination_171_Needs_The_Second_Lord_In_Vaiseshikamsa_With_Both_Jupiter_And_Mercury()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Sun","Leo",1),P("Jupiter","Leo",1),P("Mercury","Leo",1)]);
  var charts=new[]{d1}.Concat(OwnSignCharts(PlanetName.Sun,"Leo",13,0)).ToArray();
  Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(charts)),171).Present);
 }
 [Fact]public void Sarpaganda_Uses_Rahu_And_Gulika_In_Second(){var c=Chart(P("Rahu","Taurus",2),P("Gulika","Taurus",2));Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(c)),174).Present);}
 [Fact]public void Combination_175_Reads_Cruel_Navamsa_From_The_Malefic_Second_Lord(){var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Sun","Aquarius",8)]);var d9=new ChartAnalysisInput("D9",ZodiacName.Cancer,[P("Sun","Aries",1)]);Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(d1,d9)),175).Present);}
 [Fact]public void Combination_176_Needs_Malefic_Affliction_On_Both_Second_House_And_Its_Cruel_Navamsa_Lord(){var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Mars","Leo",2),P("Sun","Sagittarius",6),P("Saturn","Sagittarius",6)]);var d9=new ChartAnalysisInput("D9",ZodiacName.Cancer,[P("Sun","Capricornus",1)]);Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(d1,d9)),176).Present);}
 [Fact]public void Combination_177_Needs_No_Strength_Threshold_Beyond_The_Standard_Dignity_Ladder(){var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Mercury","Virgo",3),P("Jupiter","Virgo",3)]);Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(d1)),177).Present);}
 [Fact]public void Combination_182_Reads_Benefic_Navamsa_And_Mars_Sign_Lordship(){var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Mercury","Virgo",3),P("Jupiter","Virgo",3),P("Mars","Taurus",8)]);var d9=new ChartAnalysisInput("D9",ZodiacName.Cancer,[P("Mercury","Gemini",3)]);Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(d1,d9)),182).Present);}
 [Fact]public void Combination_196_Needs_No_Vaiseshikamsa_Only_Exaltation_And_Strength(){var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Jupiter","Libra",4),P("Venus","Pisces",9),P("Moon","Cancer",1)]);Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(d1)),196).Present);}
 [Fact]public void Combination_197_Follows_The_Navamsa_Dispositor_To_A_Dual_Kendra(){var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Mars","Aries",1),P("Moon","Gemini",1)]);var d9=new ChartAnalysisInput("D9",ZodiacName.Cancer,[P("Venus","Aries",1)]);Assert.True(Row(RamanYogaBatchEightEvaluator.Evaluate(Bundle(d1,d9)),197).Present);}
 private static ContextualYogaResult Row(IEnumerable<ContextualYogaResult>r,int n)=>r.Single(x=>x.SourceVariantCode==$"RAMAN_300_{n:000}");
 private static ChartBundle Bundle(params ChartAnalysisInput[]c)=>new(new BirthDetails(),new SiderealPositions(0,new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),0,0),new SunTimes(default,default,default,false),c,new Dictionary<string,string>(),[]);
 private static ChartAnalysisInput Chart(params PlanetPosition[]p)=>new("D1",ZodiacName.Aries,p.ToList());private static PlanetPosition P(string p,string s,int h)=>new(){Planet=p,Sign=s,HouseNumber=h};
 // Builds up to 15 additional Shodasa Varga chart entries (D2..D60, never D1 — callers supply
 // their own D1) placing `planet` in `ownSign`, starting at pool index `skip` so two planets'
 // slices in the same test never collide on the same chart type.
 private static ChartAnalysisInput[] OwnSignCharts(PlanetName planet,string ownSign,int count,int skip)=>
  new[]{"D2","D3","D4","D7","D9","D10","D12","D16","D20","D24","D27","D30","D40","D45","D60"}
   .Skip(skip).Take(count).Select(t=>new ChartAnalysisInput(t,ZodiacName.Aries,[P(planet.ToString(),ownSign,1)])).ToArray();
}
