using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
namespace Ikiastrro.Web.Tests;
public sealed class VaiseshikamsaCalculatorTests
{
 private static readonly string[] AllSixteen=["D1","D2","D3","D4","D7","D9","D10","D12","D16","D20","D24","D27","D30","D40","D45","D60"];
 [Fact] public void Counts_Only_Charts_Where_The_Planet_Owns_The_Sign()
 {
  var charts=AllSixteen.Take(5).Select(t=>Chart(t,PlanetName.Moon,"Cancer")).Concat(AllSixteen.Skip(5).Take(3).Select(t=>Chart(t,PlanetName.Sun,"Leo"))).ToArray();
  Assert.Equal(5,VaiseshikamsaCalculator.SwavargaCount(charts,PlanetName.Moon));
 }
 [Fact] public void Ignores_Charts_The_Planet_Is_Absent_From()
 {
  var charts=AllSixteen.Take(4).Select(t=>Chart(t,PlanetName.Moon,"Cancer")).ToArray();
  Assert.Equal(4,VaiseshikamsaCalculator.SwavargaCount(charts,PlanetName.Moon));
  Assert.Equal(0,VaiseshikamsaCalculator.SwavargaCount(charts,PlanetName.Sun));
 }
 [Fact] public void Grade_Maps_Two_Through_Thirteen_By_Ramans_Own_Names()
 {
  Assert.Null(VaiseshikamsaCalculator.Grade(1));
  Assert.Equal("Parijatamsa",VaiseshikamsaCalculator.Grade(2));
  Assert.Equal("Gopuramsa",VaiseshikamsaCalculator.Grade(4));
  Assert.Equal("Vaiseshikamsa",VaiseshikamsaCalculator.Grade(13));
  Assert.Equal("Vaiseshikamsa",VaiseshikamsaCalculator.Grade(16));
 }
 [Fact] public void HasVaiseshikamsa_Requires_Thirteen_Not_Twelve()
 {
  var twelve=AllSixteen.Take(12).Select(t=>Chart(t,PlanetName.Moon,"Cancer")).ToArray();
  var thirteen=AllSixteen.Take(13).Select(t=>Chart(t,PlanetName.Moon,"Cancer")).ToArray();
  Assert.False(VaiseshikamsaCalculator.HasVaiseshikamsa(twelve,PlanetName.Moon));
  Assert.True(VaiseshikamsaCalculator.HasVaiseshikamsa(thirteen,PlanetName.Moon));
 }
 [Fact] public void HasAttained_Reads_An_Arbitrary_Named_Grade_Threshold()
 {
  var six=AllSixteen.Take(6).Select(t=>Chart(t,PlanetName.Jupiter,"Sagittarius")).ToArray();
  Assert.True(VaiseshikamsaCalculator.HasAttained(six,PlanetName.Jupiter,6));
  Assert.False(VaiseshikamsaCalculator.HasAttained(six,PlanetName.Jupiter,7));
 }
 private static ChartAnalysisInput Chart(string type,PlanetName planet,string ownSign)=>new(type,ZodiacName.Cancer,[new PlanetPosition{Planet=planet.ToString(),Sign=ownSign,HouseNumber=1}]);
}
