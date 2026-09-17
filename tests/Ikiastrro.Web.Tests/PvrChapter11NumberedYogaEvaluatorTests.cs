using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Engines.Yoga;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class PvrChapter11NumberedYogaEvaluatorTests
{
 [Fact]public void Tracks_All_59_Numbered_Items()
 {
  var r=PvrChapter11NumberedYogaEvaluator.Evaluate(Bundle(Chart(ZodiacName.Aries)));
  Assert.Equal(59,r.Count);
  Assert.Equal(18,r.Count(x=>x.YogaCode=="YOGA_RAJA_ADVANCED"));
  Assert.Equal(15,r.Count(x=>x.YogaCode=="YOGA_RAJA_SAMBANDHA"));
  Assert.Equal(13,r.Count(x=>x.YogaCode=="YOGA_DHANA"));
  Assert.Equal(13,r.Count(x=>x.YogaCode=="YOGA_DARIDRA"));
  Assert.All(r,x=>Assert.Equal("SRC_PVR_INTEGRATED",x.SourceRefCode));
 }

 // ---------------- §11.7.3 More Raja Yogas ----------------
 [Fact]public void RajaAdv01_PK_AK_Conjoined_And_Lagna_Fifth_Lords_Conjoined()
 {
  var b=Bundle(new Dictionary<string,string>{["Moon"]="AK",["Mercury"]="PK"},Chart(ZodiacName.Aries,P("Mars","Aries",1),P("Sun","Aries",1),P("Moon","Cancer",4),P("Mercury","Cancer",4)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_01").Present);
 }
 [Fact]public void RajaAdv02_Exchange_With_AK_PK_In_Lagna_Or_Fifth_Qualified()
 {
  var b=Bundle(new Dictionary<string,string>{["Mercury"]="AK",["Venus"]="PK"},Chart(ZodiacName.Aries,P("Sun","Aries",1),P("Mars","Leo",5),P("Mercury","Aries",1),P("Jupiter","Aries",1),P("Venus","Leo",5),P("Moon","Leo",5)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_02").Present);
 }
 [Fact]public void RajaAdv03_Ninth_Lord_And_AK_In_Lagna_Fifth_Or_Seventh_Aspected_By_Benefics()
 {
  var b=Bundle(new Dictionary<string,string>{["Mercury"]="AK"},Chart(ZodiacName.Aries,P("Jupiter","Libra",7),P("Venus","Libra",7),P("Mercury","Aries",1),P("Moon","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_03").Present);
 }
 [Fact]public void RajaAdv04_Second_Fourth_Fifth_From_Lagna_Lord_And_AK_Hold_Benefics()
 {
  var b=Bundle(new Dictionary<string,string>{["Mars"]="AK"},Chart(ZodiacName.Aries,P("Mars","Aries",1),P("Moon","Taurus",2),P("Mercury","Cancer",4),P("Jupiter","Leo",5)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_04").Present);
 }
 [Fact]public void RajaAdv05_Third_Sixth_From_Lagna_Lord_And_AK_Hold_Malefics()
 {
  var b=Bundle(new Dictionary<string,string>{["Mars"]="AK"},Chart(ZodiacName.Aries,P("Mars","Aries",1),P("Sun","Gemini",3),P("Saturn","Virgo",6)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_05").Present);
 }
 [Fact]public void RajaAdv06_Lagna_HL_GL_Owned_By_Same_Planet()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("HL","Aries",1),P("GL","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_06").Present);
 }
 [Fact]public void RajaAdv07_Same_Planet_Aspects_Lagna_In_All_Six_Shadvarga_Charts()
 {
  var six=new[]{"D1","D2","D3","D9","D12","D30"}.Select(t=>new ChartAnalysisInput(t,ZodiacName.Aries,new List<PlanetPosition>{P("Saturn","Libra",7)})).ToArray();
  var b=Bundle(six);
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_07").Present);
 }
 [Fact]public void RajaAdv08_Lagna_HL_GL_Each_Occupied_By_A_Strong_Planet()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Aries",1),P("HL","Cancer",4),P("Moon","Cancer",4),P("GL","Leo",5),P("Sun","Leo",5)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_08").Present);
 }
 [Fact]public void RajaAdv09_Lagna_In_Rasi_Navamsa_Drekkana_Each_Occupied_By_A_Strong_Planet()
 {
  var b=Bundle(
   Chart(ZodiacName.Aries,P("Mars","Aries",1)),
   new ChartAnalysisInput("D9",ZodiacName.Aries,new List<PlanetPosition>{P("Mars","Aries",1)}),
   new ChartAnalysisInput("D3",ZodiacName.Aries,new List<PlanetPosition>{P("Mars","Aries",1)}));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_09").Present);
 }
 [Fact]public void RajaAdv10_One_Or_Two_Debilitated_Planets_In_3_6_8_With_Strong_Aspecting_Lagna_Lord()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Capricornus",10),P("Moon","Scorpio",8)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_10").Present);
 }
 [Fact]public void RajaAdv11_Sixth_Eighth_Twelfth_Lords_Afflicted_With_Strong_Aspecting_Lagna_Lord()
 {
  var b=Bundle(Chart(ZodiacName.Pisces,P("Jupiter","Cancer",5),P("Sun","Libra",8),P("Venus","Virgo",7),P("Saturn","Aries",2)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_11").Present);
 }
 [Fact]public void RajaAdv12_Fifth_And_Ninth_Lords_Conjoined()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Sun","Leo",5),P("Jupiter","Leo",5)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_12").Present);
 }
 [Fact]public void RajaAdv13_Fourth_Tenth_Lords_Exchange_Aspected_By_Fifth_Or_Ninth_Lord()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Moon","Capricornus",10),P("Saturn","Cancer",4),P("Jupiter","Cancer",4),P("Sun","Capricornus",10)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_13").Present);
 }
 [Fact]public void RajaAdv14_Fifth_Lord_In_Kendra_Joined_By_Lagna_Or_Ninth_Lord()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Sun","Aries",1),P("Mars","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_14").Present);
 }
 [Fact]public void RajaAdv15_Moon_Strong_Vargottama_Aspected_By_Four_Or_More_Planets()
 {
  var chart=Chart(ZodiacName.Aries,P("Moon","Taurus",2),P("Sun","Scorpio",8),P("Mercury","Scorpio",8),P("Venus","Scorpio",8),P("Saturn","Scorpio",8));
  var b=BundleV(new(),[new VargottamaResult("Moon","Taurus","Taurus",true)],chart);
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_15").Present);
 }
 [Fact]public void RajaAdv16_Four_Or_More_Planets_In_Moolatrikona_Or_Exaltation()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Sun","Aries",1),P("Mars","Capricornus",10),P("Jupiter","Cancer",4),P("Saturn","Libra",7)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_16").Present);
 }
 [Fact]public void RajaAdv17_Benefics_Confined_To_Kendras_Malefics_Confined_To_3_6_11()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Moon","Aries",1),P("Mercury","Cancer",4),P("Sun","Gemini",3),P("Mars","Virgo",6)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_17").Present);
 }
 [Fact]public void RajaAdv18_AL_And_Darapada_Not_In_Mutual_2_12_Or_6_8()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("AL","Aries",1),P("A7","Cancer",4)));
  Assert.True(Result(b,"PVR_CH11_RAJA_ADV_18").Present);
 }

 // ---------------- §11.8 Raaja Sambandha Yogas ----------------
 [Fact]public void Sambandha01_Tenth_Lord_Conjoined_By_AmK()
 {
  var b=Bundle(new Dictionary<string,string>{["Moon"]="AmK"},Chart(ZodiacName.Aries,P("Saturn","Capricornus",10),P("Moon","Capricornus",10)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_01").Present);
 }
 [Fact]public void Sambandha02_Eleventh_Lord_Aspects_Eleventh_Unafflicted()
 {
  var b=Bundle(Chart(ZodiacName.Cancer,P("Venus","Scorpio",5)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_02").Present);
 }
 [Fact]public void Sambandha03_AK_AmK_Conjoin()
 {
  var b=Bundle(new Dictionary<string,string>{["Sun"]="AK",["Moon"]="AmK"},Chart(ZodiacName.Aries,P("Sun","Aries",1),P("Moon","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_03").Present);
 }
 [Fact]public void Sambandha04_AmK_Strong_In_Own_Sign()
 {
  var b=Bundle(new Dictionary<string,string>{["Venus"]="AmK"},Chart(ZodiacName.Aries,P("Venus","Libra",7)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_04").Present);
 }
 [Fact]public void Sambandha05_AmK_In_Trine_From_Lagna()
 {
  var b=Bundle(new Dictionary<string,string>{["Mercury"]="AmK"},Chart(ZodiacName.Aries,P("Mercury","Leo",5)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_05").Present);
 }
 [Fact]public void Sambandha06_AmK_In_Kendra_Or_Trikona_From_AK()
 {
  var b=Bundle(new Dictionary<string,string>{["Sun"]="AK",["Moon"]="AmK"},Chart(ZodiacName.Aries,P("Sun","Aries",1),P("Moon","Cancer",4)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_06").Present);
 }
 [Fact]public void Sambandha07_Malefics_In_3rd_6th_From_Lagna_AL_And_AK()
 {
  var b=Bundle(new Dictionary<string,string>{["Mars"]="AK"},Chart(ZodiacName.Aries,P("Mars","Aries",1),P("AL","Aries",1),P("Sun","Gemini",3),P("Saturn","Virgo",6)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_07").Present);
 }
 [Fact]public void Sambandha08_AK_Strong_In_Kendra_Trikona_And_Ninth_Lord_Joins_AK()
 {
  var b=Bundle(new Dictionary<string,string>{["Mars"]="AK"},Chart(ZodiacName.Aries,P("Mars","Aries",1),P("Jupiter","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_08").Present);
 }
 [Fact]public void Sambandha09_AK_Is_Moons_Dispositor_In_Lagna_With_A_Benefic()
 {
  var b=Bundle(new Dictionary<string,string>{["Venus"]="AK"},Chart(ZodiacName.Aries,P("Moon","Taurus",2),P("Venus","Aries",1),P("Mercury","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_09").Present);
 }
 [Fact]public void Sambandha10_AK_In_5_7_9_10_With_A_Benefic()
 {
  var b=Bundle(new Dictionary<string,string>{["Sun"]="AK"},Chart(ZodiacName.Aries,P("Sun","Leo",5),P("Jupiter","Leo",5)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_10").Present);
 }
 [Fact]public void Sambandha11_AK_In_Ninth()
 {
  var b=Bundle(new Dictionary<string,string>{["Sun"]="AK"},Chart(ZodiacName.Aries,P("Sun","Sagittarius",9)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_11").Present);
 }
 [Fact]public void Sambandha12_Eleventh_Lord_In_Eleventh_Unafflicted_And_AK_With_Benefic()
 {
  var b=Bundle(new Dictionary<string,string>{["Mercury"]="AK"},Chart(ZodiacName.Cancer,P("Venus","Taurus",11),P("Mercury","Cancer",1),P("Moon","Cancer",1)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_12").Present);
 }
 [Fact]public void Sambandha13_Lagna_Tenth_Lords_Exchange()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Capricornus",10),P("Saturn","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_13").Present);
 }
 [Fact]public void Sambandha14_Moon_Venus_In_Fourth_From_AK()
 {
  var b=Bundle(new Dictionary<string,string>{["Sun"]="AK"},Chart(ZodiacName.Aries,P("Sun","Aries",1),P("Moon","Cancer",4),P("Venus","Cancer",4)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_14").Present);
 }
 [Fact]public void Sambandha15_Lagna_Lord_Or_AK_Conjoins_Fifth_Lord_In_Kendra_Trikona()
 {
  var b=Bundle(new Dictionary<string,string>{["Mars"]="AK"},Chart(ZodiacName.Aries,P("Mars","Leo",5),P("Sun","Leo",5)));
  Assert.True(Result(b,"PVR_CH11_RAJA_SAMBANDHA_15").Present);
 }

 // ---------------- §11.9 Dhana Yogas ----------------
 [Fact]public void DhanaBasic_Exalted_Benefic_In_Second()
 {
  var b=Bundle(Chart(ZodiacName.Gemini,P("Jupiter","Cancer",2)));
  Assert.True(Result(b,"PVR_CH11_DHANA_BASIC").Present);
 }
 [Fact]public void DhanaAries_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Aries",1),P("Mercury","Aries",1),P("Venus","Aries",1),P("Saturn","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_ARIES").Present);
 }
 [Fact]public void DhanaTaurus_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Taurus,P("Venus","Taurus",1),P("Mercury","Taurus",1),P("Saturn","Taurus",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_TAURUS").Present);
 }
 [Fact]public void DhanaGemini_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Gemini,P("Mercury","Gemini",1),P("Jupiter","Gemini",1),P("Saturn","Gemini",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_GEMINI").Present);
 }
 [Fact]public void DhanaCancer_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Cancer,P("Moon","Cancer",1),P("Mercury","Cancer",1),P("Jupiter","Cancer",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_CANCER").Present);
 }
 [Fact]public void DhanaLeo_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Leo,P("Sun","Leo",1),P("Mars","Leo",1),P("Jupiter","Leo",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_LEO").Present);
 }
 [Fact]public void DhanaVirgo_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Virgo,P("Mercury","Virgo",1),P("Jupiter","Virgo",1),P("Saturn","Virgo",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_VIRGO").Present);
 }
 [Fact]public void DhanaLibra_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Libra,P("Venus","Libra",1),P("Mercury","Libra",1),P("Saturn","Libra",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_LIBRA").Present);
 }
 [Fact]public void DhanaScorpio_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Scorpio,P("Mars","Scorpio",1),P("Mercury","Scorpio",1),P("Venus","Scorpio",1),P("Saturn","Scorpio",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_SCORPIO").Present);
 }
 [Fact]public void DhanaSagittarius_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Sagittarius,P("Jupiter","Sagittarius",1),P("Mars","Sagittarius",1),P("Mercury","Sagittarius",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_SAGITTARIUS").Present);
 }
 [Fact]public void DhanaCapricornus_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Capricornus,P("Saturn","Capricornus",1),P("Mars","Capricornus",1),P("Jupiter","Capricornus",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_CAPRICORNUS").Present);
 }
 [Fact]public void DhanaAquarius_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Aquarius,P("Saturn","Aquarius",1),P("Mars","Aquarius",1),P("Jupiter","Aquarius",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_AQUARIUS").Present);
 }
 [Fact]public void DhanaPisces_AltInLagna_With_Associates()
 {
  var b=Bundle(Chart(ZodiacName.Pisces,P("Jupiter","Pisces",1),P("Mars","Pisces",1),P("Mercury","Pisces",1)));
  Assert.True(Result(b,"PVR_CH11_DHANA_PISCES").Present);
 }

 // ---------------- §11.10 Daridra Yogas ----------------
 [Fact]public void Daridra01_Lagna_Twelfth_Lords_Exchange_With_Maraka()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Pisces",12),P("Jupiter","Aries",1),P("Venus","Pisces",12)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_01").Present);
 }
 [Fact]public void Daridra02_Lagna_Sixth_Lords_Exchange_With_Maraka()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Virgo",6),P("Mercury","Aries",1),P("Venus","Virgo",6)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_02").Present);
 }
 [Fact]public void Daridra03_Moon_With_Ketu_Lagna_Lord_In_Eighth_With_Maraka()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Moon","Taurus",2),P("Ketu","Taurus",2),P("Mars","Scorpio",8),P("Venus","Scorpio",8)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_03").Present);
 }
 [Fact]public void Daridra04_Lagna_Lord_With_Malefic_In_Dusthana_Second_Lord_Debilitated()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Virgo",6),P("Saturn","Virgo",6),P("Venus","Virgo",6)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_04").Present);
 }
 [Fact]public void Daridra05_Fifth_Lord_Sixth_Ninth_Lord_Twelfth_With_Maraka_Aspect()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Sun","Virgo",6),P("Jupiter","Pisces",12),P("Venus","Pisces",12)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_05").Present);
 }
 [Fact]public void Daridra06_Malefic_In_Lagna_Without_Ninth_Tenth_Lord_Maraka_Influences()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Sun","Aries",1),P("Venus","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_06").Present);
 }
 [Fact]public void Daridra07_Dispositors_Of_Dusthana_Lords_In_Dusthana_With_Malefics()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mercury","Leo",5),P("Mars","Cancer",4),P("Jupiter","Taurus",2),P("Sun","Scorpio",8),P("Moon","Scorpio",8),P("Venus","Scorpio",8),P("Saturn","Scorpio",8)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_07").Present);
 }
 [Fact]public void Daridra08_Navamsa_Moon_Dispositor_Occupies_Maraka_House()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Taurus",2)),new ChartAnalysisInput("D9",ZodiacName.Aries,new List<PlanetPosition>{P("Moon","Aries",1)}));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_08").Present);
 }
 [Fact]public void Daridra09_Lords_Of_Lagna_In_Rasi_And_Navamsa_Maraka_Afflicted()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Aries",1),P("Venus","Aries",1),P("Moon","Aries",1)),new ChartAnalysisInput("D9",ZodiacName.Cancer,new List<PlanetPosition>()));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_09").Present);
 }
 [Fact]public void Daridra10_Ashtakavarga_Forward_Reference_Is_Not_Evaluated()
 {
  var r=Result(Bundle(Chart(ZodiacName.Aries)),"PVR_CH11_DARIDRA_10");
  Assert.Null(r.Present);Assert.Equal("NOT_EVALUATED",r.EvaluationStatus);
 }
 [Fact]public void Daridra11_Planet_Conjoined_With_Dusthana_Lord_Unhelped_By_Trine_Lord()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mercury","Aries",1),P("Venus","Aries",1)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_11").Present);
 }
 [Fact]public void Daridra12_Mars_Saturn_In_Second_Unaspected_By_Mercury()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Mars","Taurus",2),P("Saturn","Taurus",2)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_12").Present);
 }
 [Fact]public void Daridra13_Sun_In_Second_Aspected_By_Saturn()
 {
  var b=Bundle(Chart(ZodiacName.Aries,P("Sun","Taurus",2),P("Saturn","Scorpio",8)));
  Assert.True(Result(b,"PVR_CH11_DARIDRA_13").Present);
 }

 private static ContextualYogaResult Result(ChartBundle b,string variant)=>PvrChapter11NumberedYogaEvaluator.Evaluate(b).Single(x=>x.SourceVariantCode==variant);
 private static PlanetPosition P(string planet,string sign,int house)=>new(){Planet=planet,Sign=sign,HouseNumber=house};
 private static ChartAnalysisInput Chart(ZodiacName asc,params PlanetPosition[] p)=>new("D1",asc,p.ToList());
 private static ChartBundle Bundle(params ChartAnalysisInput[] charts)=>Bundle(new Dictionary<string,string>(),charts);
 private static ChartBundle Bundle(Dictionary<string,string> karakas,params ChartAnalysisInput[] charts)=>
  new(new BirthDetails(),new SiderealPositions(0,new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),new Dictionary<PlanetName,double>(),0,0),new SunTimes(default,default,default,false),charts,karakas,[]);
 private static ChartBundle BundleV(Dictionary<string,string> karakas,IReadOnlyList<VargottamaResult> vg,params ChartAnalysisInput[] charts)=>
  Bundle(karakas,charts) with { Vargottama=vg };
}
