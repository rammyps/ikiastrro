using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
namespace Ikiastrro.Web.Tests;
public sealed class ShashtiamsaDeityTableTests
{
 [Fact] public void NumberFor_Uses_The_Traditional_Parashari_Rule()
 {
  Assert.Equal(1,ShashtiamsaDeityTable.NumberFor(0));
  Assert.Equal(17,ShashtiamsaDeityTable.NumberFor(8.2));
  Assert.Equal(60,ShashtiamsaDeityTable.NumberFor(29.99));
 }
 [Fact] public void Odd_Sign_Reads_The_List_Forward()
 {
  // Reference export worked example (d60-shashtiamsa.md §9): Sun 8°12' Aries -> part 17 -> Amrita (B).
  var deity=ShashtiamsaDeityTable.Lookup(ZodiacName.Aries,8.2);
  Assert.Equal(17,deity.Number);
  Assert.Equal("Amrita",deity.Name);
  Assert.False(deity.IsMalefic);
 }
 [Fact] public void Odd_Sign_Part_Two_Is_Rakshasa()
 {
  // Reference export worked example: D60 Lagna 0°38'50" Aries -> part 2 -> Rakshasa (M).
  var deity=ShashtiamsaDeityTable.Lookup(ZodiacName.Aries,0.647);
  Assert.Equal(2,deity.Number);
  Assert.Equal("Rakshasa",deity.Name);
  Assert.True(deity.IsMalefic);
 }
 [Fact] public void Even_Sign_Reverses_The_List()
 {
  // d60-shashtiamsa.md §4 note: "part 1 of an even sign = name 60" (Karaala-damshtra/Ghora, M).
  var deity=ShashtiamsaDeityTable.Lookup(ZodiacName.Taurus,0);
  Assert.Equal(1,deity.Number);
  Assert.Equal("Karaala-damshtra",deity.Name);
  Assert.True(deity.IsMalefic);
 }
}
