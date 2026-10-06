using Ikiastrro.Core.Engines.PlanetaryStates;

namespace Ikiastrro.Yoga.Tests;

/// <summary>PVR sec.15.4.1-15.4.3: the meaning of each age, alertness and mood state, and the house results the
/// book names for Lajjita, Kshobhita and Kshudhita.</summary>
public sealed class AvasthaPvrNotesTests
{
    [Theory]
    [InlineData("Baaladi", "Baala", "one quarter")]
    [InlineData("Baaladi", "Kumara", "half")]
    [InlineData("Baaladi", "Yuva", "all of its results")]
    [InlineData("Baaladi", "Mrita", "none")]
    [InlineData("Jagradadi", "Jagrat", "full results")]
    [InlineData("Jagradadi", "Swapna", "medium")]
    [InlineData("Jagradadi", "Sushupti", "negligible")]
    [InlineData("Deeptadi", "Kopita", "joined closely by the Sun")]
    [InlineData("Lajjitadi", "Kshobhita", "shaken, agitated")]
    public void Meaning_follows_the_book(string system, string state, string expectedFragment) =>
        Assert.Contains(expectedFragment, AvasthaPvrNotes.Meaning(system, state));

    [Fact]
    public void Every_baaladi_jagradadi_deeptadi_and_lajjitadi_state_has_a_meaning()
    {
        string[] baaladi = ["Baala", "Kumara", "Yuva", "Vriddha", "Mrita"];
        string[] jagradadi = ["Jagrat", "Swapna", "Sushupti"];
        string[] deeptadi = ["Deepta", "Swastha", "Mudita", "Saanta", "Deena", "Duhkhita", "Vikala", "Khala", "Kopita"];
        string[] lajjitadi = ["Lajjita", "Garvita", "Kshudhita", "Trishita", "Mudita", "Kshobhita"];
        foreach (var s in baaladi) Assert.NotNull(AvasthaPvrNotes.Meaning("Baaladi", s));
        foreach (var s in jagradadi) Assert.NotNull(AvasthaPvrNotes.Meaning("Jagradadi", s));
        foreach (var s in deeptadi) Assert.NotNull(AvasthaPvrNotes.Meaning("Deeptadi", s));
        foreach (var s in lajjitadi) Assert.NotNull(AvasthaPvrNotes.Meaning("Lajjitadi", s));
        Assert.Null(AvasthaPvrNotes.Meaning("Sayanadi", "Sayana"));   // Sayanadi results are per planet, in the database
    }

    [Fact]
    public void House_results_are_the_ones_the_book_names()
    {
        Assert.Contains(AvasthaPvrNotes.HouseResults(["Lajjita"], 5), r => r.Contains("progeny"));
        Assert.DoesNotContain(AvasthaPvrNotes.HouseResults(["Lajjita"], 6), r => r.Contains("progeny"));
        Assert.Contains(AvasthaPvrNotes.HouseResults(["Kshobhita"], 7), r => r.Contains("loss of spouse"));
        Assert.DoesNotContain(AvasthaPvrNotes.HouseResults(["Kshobhita"], 5), r => r.Contains("loss of spouse"));
        Assert.Contains(AvasthaPvrNotes.HouseResults(["Kshudhita"], 3), r => r.Contains("significations of that house"));
        Assert.Empty(AvasthaPvrNotes.HouseResults(["Garvita", "Mudita"], 5));
    }
}
