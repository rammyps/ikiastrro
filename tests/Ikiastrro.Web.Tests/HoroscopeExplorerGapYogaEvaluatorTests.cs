using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Yoga;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class HoroscopeExplorerGapYogaEvaluatorTests
{
 [Fact]public void Vipareeta_Raja_Forms_When_Dusthana_Lord_Occupies_Dusthana(){var c=Chart(P("Jupiter","Virgo",6,160));Assert.True(Result(c,"YOGA_VIPAREETA_RAJA").Present);}
 [Fact]public void Anivahuppu_Requires_Alone_Anchor_And_Four_Grahas_On_Each_Side(){var c=Chart(P("Sun","Aries",1,0),P("Moon","Taurus",2,30),P("Mars","Gemini",3,60),P("Mercury","Cancer",4,90),P("Jupiter","Leo",5,120),P("Venus","Libra",7,210),P("Saturn","Scorpio",8,240),P("Rahu","Sagittarius",9,270),P("Ketu","Capricornus",10,300));Assert.True(Result(c,"YOGA_ANIVAHUPPU").Present);}
 [Fact]public void Anivahuppu_Assigns_Opposite_Node_As_The_Remaining_Planet(){var c=Chart(P("Sun","Aries",1,8),P("Mercury","Aries",1,1),P("Mars","Aries",1,3),P("Venus","Aries",1,11),P("Rahu","Cancer",4,103),P("Jupiter","Virgo",6,158),P("Saturn","Virgo",6,160),P("Moon","Scorpio",8,217),P("Ketu","Capricornus",10,283));Assert.True(Result(c,"YOGA_ANIVAHUPPU").Present);}
 [Fact]public void Unsupported_Natal_Predicates_Are_Not_Evaluated(){var r=HoroscopeExplorerGapYogaEvaluator.Evaluate(Chart());Assert.All(r.Where(x=>x.YogaCode is "YOGA_VIDYA" or "YOGA_ARISHTA"),x=>{Assert.Null(x.Present);Assert.Equal("NOT_EVALUATED",x.EvaluationStatus);});}
 private static ContextualYogaResult Result(ChartAnalysisInput c,string code)=>HoroscopeExplorerGapYogaEvaluator.Evaluate(c).Single(x=>x.YogaCode==code);
 private static ChartAnalysisInput Chart(params PlanetPosition[] p)=>new("D1",ZodiacName.Aries,p.ToList());
 private static PlanetPosition P(string planet,string sign,int house,double longitude)=>new(){Planet=planet,Sign=sign,HouseNumber=house,NirayanaLongitudeDegrees=longitude};
}
