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
  // d60-shashtiamsa.md §4 note: "part 1 of an even sign = name 60" (Chandra-rekha, B).
  var deity=ShashtiamsaDeityTable.Lookup(ZodiacName.Taurus,0);
  Assert.Equal(1,deity.Number);
  Assert.Equal("Chandra-rekha",deity.Name);
  Assert.False(deity.IsMalefic);
 }

 // Jagannatha Hora, Basics -> Amsa rulers -> Shashtyamsa (D-60), RamakrishnanP: every list index
 // its export shows, with JHora's own spelling on the right (decisions/006).
 [Theory]
 [InlineData(2,"Rakshasa")] [InlineData(3,"Deva")] [InlineData(4,"Kubera")] [InlineData(5,"Yaksha")]
 [InlineData(7,"Bhrashta")] [InlineData(8,"Kulaghna")] [InlineData(10,"Vahni")] [InlineData(11,"Maaya")]
 [InlineData(13,"Apampati")] [InlineData(14,"Marut")] [InlineData(15,"Kaala")] [InlineData(16,"Sarpa")]
 [InlineData(17,"Amrita")] [InlineData(23,"Vishnu")] [InlineData(24,"Maheshwara")] [InlineData(29,"Kamalaakara")]
 [InlineData(30,"Gulika")] [InlineData(35,"Yama")] [InlineData(37,"Sudha")] [InlineData(38,"Amrita")]
 [InlineData(39,"Poorna-Chandra")] [InlineData(40,"Visha-dagdha")] [InlineData(41,"Kulanaasha")] [InlineData(43,"Utpaata")]
 [InlineData(44,"Kaala")] [InlineData(46,"Komala")] [InlineData(47,"Sheetala")] [InlineData(51,"Kaala-paavaka")]
 [InlineData(59,"Bhramana")]
 public void List_Index_Matches_JHora(int index,string name) =>
  // An odd sign reads the list forward, so part N of Aries is list index N.
  Assert.Equal(name,ShashtiamsaDeityTable.Lookup(ZodiacName.Aries,(index-1)/2.0+0.1).Name);

 [Fact]
 public void Even_Sign_Placements_Match_JHora()
 {
  // RamakrishnanP: Moon 7 Sc 12' (division 15, index 46), Jupiter 8 Vi 43' (18 -> 43), Saturn 10 Vi 57' (22 -> 39).
  Assert.Equal("Komala",ShashtiamsaDeityTable.Lookup(ZodiacName.Scorpio,7.209).Name);
  Assert.Equal("Utpaata",ShashtiamsaDeityTable.Lookup(ZodiacName.Virgo,8.724).Name);
  Assert.Equal("Poorna-Chandra",ShashtiamsaDeityTable.Lookup(ZodiacName.Virgo,10.959).Name);
 }
}
