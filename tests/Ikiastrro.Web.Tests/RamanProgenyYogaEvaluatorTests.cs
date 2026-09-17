using Ikiastrro.Core.Engines.Astronomy;using Ikiastrro.Core.Engines.Yoga;using Ikiastrro.Core.Models;using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class RamanProgenyYogaEvaluatorTests
{
 [Fact]public void Tracks_Every_Number_220_Through_244(){var r=RamanProgenyYogaEvaluator.Evaluate(Bundle(Chart()));Assert.Equal(25,r.Count);Assert.Equal(Enumerable.Range(220,25).Select(n=>$"RAMAN_300_{n:000}"),r.Select(x=>x.SourceVariantCode));}
 [Fact]public void Combination_220_Needs_Rahu_In_Fifth_Outside_Saturns_Navamsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Rahu","Leo",5)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Rahu","Aries",1)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1,d9)),220).Present);
 }
 [Fact]public void Combination_221_Follows_A_Seventh_Lord_Associates_Navamsa_Dispositor()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Venus","Libra",7),P("Mars","Libra",7),P("Moon","Aries",1)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Mars","Cancer",1)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1,d9)),221).Present);
 }
 [Fact]public void Combination_222_Needs_Mars_Saturn_In_Fifth_And_Lagna_Lord_In_Mercurys_Sign()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Virgo",5),P("Saturn","Virgo",5),P("Mercury","Virgo",5)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),222).Present);
 }
 [Fact]public void Combination_223_Needs_Seventh_Lord_In_Eleventh_And_Fifth_Lord_With_A_Benefic()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Venus","Aquarius",11),P("Sun","Leo",5),P("Jupiter","Leo",5),P("Mars","Leo",5)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),223).Present);
 }
 [Fact]public void Combination_224_Needs_Fifth_Lord_In_A_Dusthana(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Virgo",6)]);Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),224).Present);}
 [Fact]public void Combination_225_Needs_Fifth_Lord_In_Kendra_Or_Trikona(){var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Leo",1)]);Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),225).Present);}
 [Fact]public void Combination_226_Needs_Jupiter_As_Fifth_Lord_And_A_Favourable_Sun()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Leo,[P("Sun","Leo",1)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),226).Present);
 }
 [Fact]public void Combination_227_Needs_Jupiter_In_Fifth_And_Fifth_Lord_With_Venus()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Leo",5),P("Sun","Leo",9),P("Venus","Leo",9)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),227).Present);
 }
 [Fact]public void Combination_228_Needs_Jupiter_Ninth_Venus_Ninth_From_Jupiter_With_Lagna_Lord()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Sagittarius",9),P("Venus","Leo",5),P("Mars","Leo",5)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),228).Present);
 }
 [Fact]public void Combination_229_Needs_Rahu_Fifth_Afflicted_Fifth_Lord_And_Debilitated_Jupiter()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Rahu","Taurus",5),P("Sun","Leo",5),P("Saturn","Leo",5),P("Jupiter","Capricornus",1)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),229).Present);
 }
 [Fact]public void Combination_230_Needs_A_Malefic_Fifth_From_Jupiter_And_From_Lagna()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Aries",1),P("Saturn","Leo",5)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),230).Present);
 }
 [Fact]public void Combination_231_Needs_A_Benefic_In_Fifth_And_Fifth_Lord_In_Association()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Leo",5),P("Sun","Leo",5)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),231).Present);
 }
 [Fact]public void Combination_232_Needs_A_Benefic_Navamsa_Dispositor_Aspected_By_A_Benefic()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Moon","Leo",4),P("Jupiter","Leo",4)]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Sun","Cancer",1)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1,d9)),232).Present);
 }
 [Fact]public void Combination_233_Needs_Lagna_Lord_Afflicted_And_Saturn_In_Fifth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Leo",3),P("Saturn","Leo",5)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),233).Present);
 }
 [Fact]public void Combination_234_Needs_Jupiter_In_His_Own_Navamsa_And_Mrudwamsa()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[new PlanetPosition{Planet="Jupiter",Sign="Sagittarius",HouseNumber=1,NirayanaLongitudeDegrees=249}]);
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Jupiter","Sagittarius",1)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1,d9)),234).Present);
 }
 [Fact]public void Combination_235_Needs_Jupiter_And_Venus_In_Fifth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Leo",5),P("Venus","Leo",5)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),235).Present);
 }
 [Fact]public void Combination_236_Needs_Tenth_Second_And_Seventh_Lords_All_In_Tenth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Saturn","Capricornus",10),P("Venus","Capricornus",10)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),236).Present);
 }
 [Fact]public void Combination_237_Needs_Powerful_Fifth_And_Seventh_Lords_Joined_To_The_Sixth_Lord()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Gemini,[P("Venus","Pisces",10),P("Jupiter","Pisces",10),P("Mars","Pisces",10),P("Mercury","Pisces",10)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),237).Present);
 }
 [Fact]public void Combination_238_Needs_Lagna_Lord_And_Seventh_Lord_In_Association()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Mars","Leo",1),P("Venus","Leo",7)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),238).Present);
 }
 [Fact]public void Combination_239_Needs_Seventh_Lord_Or_Venus_With_Jupiter_Or_Mercury()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Venus","Leo",7),P("Jupiter","Leo",7)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),239).Present);
 }
 [Fact]public void Combination_240_Needs_Seventh_Lord_In_Fourth_With_Venus()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Cancer,[P("Saturn","Libra",4),P("Venus","Libra",4)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),240).Present);
 }
 [Fact]public void Combination_241_Needs_A_Strong_Benefic_In_Lagna_Third_Or_Fifth_Aspecting_The_Ninth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Sagittarius,[P("Jupiter","Sagittarius",1)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),241).Present);
 }
 [Fact]public void Combination_242_Needs_The_Sun_In_A_Dusthana_With_Specific_Lord_Placements()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Libra",8),P("Mars","Sagittarius",9),P("Jupiter","Aries",1),P("Mercury","Leo",5)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),242).Present);
 }
 [Fact]public void Combination_243_Needs_An_Exalted_Aspected_Ninth_Lord_And_A_Benefic_In_The_Ninth()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Jupiter","Cancer",3),P("Mercury","Cancer",3),P("Moon","Sagittarius",9)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),243).Present);
 }
 [Fact]public void Combination_244_Needs_Sun_And_Saturn_In_Tenth_Aspected_By_A_Malefic()
 {
  var d1=new ChartAnalysisInput("D1",ZodiacName.Aries,[P("Sun","Capricornus",10),P("Saturn","Capricornus",10)]);
  Assert.True(Row(RamanProgenyYogaEvaluator.Evaluate(Bundle(d1)),244).Present);
 }
 private static ContextualYogaResult Row(IEnumerable<ContextualYogaResult>r,int n)=>r.Single(x=>x.SourceVariantCode==$"RAMAN_300_{n:000}");
 private static ChartBundle Bundle(params ChartAnalysisInput[]c)=>new(new BirthDetails(),new SiderealPositions(0,new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),0,0),new SunTimes(default,default,default,false),c,new Dictionary<string,string>(),[]);
 private static ChartAnalysisInput Chart(params PlanetPosition[]p)=>new("D1",ZodiacName.Aries,p.ToList());
 private static PlanetPosition P(string p,string s,int h)=>new(){Planet=p,Sign=s,HouseNumber=h};
}
