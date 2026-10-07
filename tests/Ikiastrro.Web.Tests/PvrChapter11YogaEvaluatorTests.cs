using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Yoga;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class PvrChapter11YogaEvaluatorTests
{
 private static readonly string[] Codes=
 [
  "YOGA_MAALA","YOGA_SARPA","YOGA_MRIDANGA","YOGA_SUBHA","YOGA_ASUBHA","YOGA_GURU_MANGALA","YOGA_CHAMARA",
  "YOGA_KHADGA","YOGA_LAGNAADHI","YOGA_SAARADA","YOGA_DHARMA_KARMADHIPATI",
  "YOGA_BASIC_RAJA","YOGA_HARI","YOGA_HARA","YOGA_BRAHMA_TRIMURTI","YOGA_PARIJATHA"
 ];

 [Fact]public void Tracks_All_Nine_P0_And_Five_Identity_Codes()
 {
  var r=PvrChapter11YogaEvaluator.Evaluate(Chart());
  Assert.Equal(16,r.Count);
  Assert.Equal(Codes,r.Select(x=>x.YogaCode));
  Assert.All(r,x=>Assert.Equal("SRC_PVR_INTEGRATED",x.SourceRefCode));
  Assert.All(r.Take(15),x=>Assert.Equal("EVALUATED",x.EvaluationStatus));
  Assert.Equal("NOT_EVALUATED",r[15].EvaluationStatus);
 }

 [Fact]public void Sarpa_Forms_When_Three_Kendras_Hold_Natural_Malefics_Including_The_Nodes()
 {
  // RamakrishnanP (Aries lagna): Sun/Mars in Aries, Rahu in Cancer, Ketu in Capricornus — JHora lists Sarpa here.
  var c=Chart(P("Sun","Aries",1),P("Mars","Aries",1),P("Rahu","Cancer",4),P("Ketu","Capricornus",10),P("Saturn","Virgo",6));
  Assert.True(Result(c,"YOGA_SARPA").Present);
 }

 [Fact]public void Sarpa_Needs_Three_Kendras_Not_Just_Three_Malefics()
 {
  var c=Chart(P("Sun","Aries",1),P("Mars","Aries",1),P("Saturn","Aries",1),P("Rahu","Cancer",4));
  Assert.False(Result(c,"YOGA_SARPA").Present);
 }

 [Fact]public void Mridanga_Forms_With_A_Strong_Lagna_Lord_And_A_Strong_Planet_In_Kendra_Or_Trikona()
 {
  // RamakrishnanP: Mars (lagna lord) own/moolatrikona in Aries, Sun exalted in Aries.
  var c=Chart(P("Mars","Aries",1),P("Sun","Aries",1),P("Moon","Scorpio",8));
  Assert.True(Result(c,"YOGA_MRIDANGA").Present);
 }

 [Fact]public void Mridanga_Fails_When_The_Lagna_Lord_Is_Not_Strong()
 {
  var c=Chart(P("Mars","Cancer",4),P("Sun","Aries",1));
  Assert.False(Result(c,"YOGA_MRIDANGA").Present);
 }

 [Fact]public void Basic_Raja_Forms_For_Distinct_Kendra_And_Trine_Lords_In_Conjunction()
 {
  var c=Chart(P("Moon","Aries",1),P("Sun","Aries",1));
  Assert.True(Result(c,"YOGA_BASIC_RAJA").Present);
 }

 [Fact]public void Hari_Forms_From_Second_Lord_When_All_Three_Relative_Signs_Hold_Benefics()
 {
  var c=Chart(P("Venus","Libra",7),P("Moon","Scorpio",8),P("Mercury","Virgo",6),P("Jupiter","Taurus",2));
  Assert.True(Result(c,"YOGA_HARI").Present);
 }

 [Fact]public void Hara_Forms_From_Seventh_Lord_When_All_Three_Relative_Signs_Hold_Benefics()
 {
  var c=Chart(P("Venus","Aries",1),P("Moon","Cancer",4),P("Jupiter","Sagittarius",9),P("Mercury","Scorpio",8));
  Assert.True(Result(c,"YOGA_HARA").Present);
 }

 [Fact]public void Brahma_Trimurti_Forms_From_Lagna_Lord_When_All_Three_Relative_Signs_Hold_Benefics()
 {
  var c=Chart(P("Mars","Aries",1),P("Moon","Cancer",4),P("Jupiter","Capricornus",10),P("Mercury","Aquarius",11));
  Assert.True(Result(c,"YOGA_BRAHMA_TRIMURTI").Present);
 }

 [Fact]public void Kalpadruma_Is_Not_Evaluated_Without_D9()
 {
 Assert.Equal("NOT_EVALUATED",Result(Chart(P("Mars","Aries",1)),"YOGA_PARIJATHA").EvaluationStatus);
 }

 [Fact]public void Kalpadruma_Forms_When_All_Four_Links_Are_Strong()
 {
  var d1=Chart(P("Mars","Aries",1));
  var d9=new ChartAnalysisInput("D9",ZodiacName.Aries,[P("Mars","Aries",1)]);
  Assert.True(PvrChapter11YogaEvaluator.Evaluate(d1,d9).Single(x=>x.SourceVariantCode=="PVR_CH11_KALPADRUMA").Present);
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
