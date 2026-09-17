using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Yoga;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class PvrChapter11YogaEvaluatorTests
{
 private static readonly string[] Codes=
 [
  "YOGA_MAALA","YOGA_SUBHA","YOGA_ASUBHA","YOGA_GURU_MANGALA","YOGA_CHAMARA",
  "YOGA_KHADGA","YOGA_LAGNAADHI","YOGA_SAARADA","YOGA_DHARMA_KARMADHIPATI"
 ];

 [Fact]public void Tracks_All_Nine_P0_Codes()
 {
  var r=PvrChapter11YogaEvaluator.Evaluate(Chart());
  Assert.Equal(9,r.Count);
  Assert.Equal(Codes,r.Select(x=>x.YogaCode));
  Assert.All(r,x=>{Assert.Equal("EVALUATED",x.EvaluationStatus);Assert.Equal("SRC_PVR_INTEGRATED",x.SourceRefCode);});
 }

 [Fact]public void Maala_Forms_When_Three_Kendras_Hold_Natural_Benefics()
 {
  var c=Chart(P("Moon","Aries",1),P("Mercury","Cancer",4),P("Jupiter","Libra",7));
  Assert.True(Result(c,"YOGA_MAALA").Present);
 }

 [Fact]public void Subha_Forms_When_A_Benefic_Occupies_Lagna()
 {
  var c=Chart(P("Moon","Aries",1));
  Assert.True(Result(c,"YOGA_SUBHA").Present);
 }

 [Fact]public void Asubha_Forms_When_A_Malefic_Occupies_Lagna()
 {
  var c=Chart(P("Sun","Aries",1));
  Assert.True(Result(c,"YOGA_ASUBHA").Present);
 }

 [Fact]public void Guru_Mangala_Forms_When_Jupiter_And_Mars_Are_Conjoined()
 {
  var c=Chart(P("Jupiter","Aries",1),P("Mars","Aries",1));
  Assert.True(Result(c,"YOGA_GURU_MANGALA").Present);
 }

 [Fact]public void Chamara_Forms_When_Two_Benefics_Join_In_The_Seventh()
 {
  var c=Chart(P("Moon","Libra",7),P("Mercury","Libra",7));
  Assert.True(Result(c,"YOGA_CHAMARA").Present);
 }

 [Fact]public void Khadga_Forms_When_Second_And_Ninth_Lords_Exchange_With_Lagna_Lord_In_Kendra()
 {
  var c=Chart(P("Venus","Sagittarius",9),P("Jupiter","Taurus",2),P("Mars","Aries",1));
  Assert.True(Result(c,"YOGA_KHADGA").Present);
 }

 [Fact]public void Lagnaadhi_Forms_When_Seventh_And_Eighth_Hold_Unafflicted_Benefics()
 {
  var c=Chart(P("Moon","Libra",7),P("Mercury","Scorpio",8));
  Assert.True(Result(c,"YOGA_LAGNAADHI").Present);
 }

 [Fact]public void Saarada_Forms_When_All_Five_Conditions_Are_Met()
 {
  var c=Chart(P("Saturn","Leo",5),P("Sun","Leo",5),P("Mercury","Aries",1),P("Moon","Aries",1),P("Mars","Aquarius",11));
  Assert.True(Result(c,"YOGA_SAARADA").Present);
 }

 [Fact]public void Dharma_Karmadhipati_Forms_When_Ninth_And_Tenth_Lords_Conjoin()
 {
  var c=Chart(P("Jupiter","Aries",1),P("Saturn","Aries",1));
  Assert.True(Result(c,"YOGA_DHARMA_KARMADHIPATI").Present);
 }

 private static ContextualYogaResult Result(ChartAnalysisInput c,string code)=>PvrChapter11YogaEvaluator.Evaluate(c).Single(x=>x.YogaCode==code);
 private static ChartAnalysisInput Chart(params PlanetPosition[] p)=>new("D1",ZodiacName.Aries,p.ToList());
 private static PlanetPosition P(string planet,string sign,int house)=>new(){Planet=planet,Sign=sign,HouseNumber=house};
}
