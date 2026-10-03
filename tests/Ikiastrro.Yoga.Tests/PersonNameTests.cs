using Ikiastrro.Core.Models;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

public class PersonNameTests
{
    [Theory]
    [InlineData("Ananya", "R", "Ananya R")]
    [InlineData("  GowriShankar ", " C ", "GowriShankar C")]
    [InlineData("Mary Ann", "Smith", "Mary Ann Smith")]
    [InlineData("Ramakrishnan", "", "Ramakrishnan")]
    [InlineData(null, "P", "P")]
    public void Compose_joins_first_and_last_with_one_space(string? first, string? last, string expected) =>
        Assert.Equal(expected, PersonName.Compose(first, last));

    [Theory]
    [InlineData("Ananya R", "Ananya", "R")]
    [InlineData("GowriShankar C", "GowriShankar", "C")]
    [InlineData("Mary Ann Smith", "Mary Ann", "Smith")]
    [InlineData("Ananya", "Ananya", "")]
    [InlineData("  ", "", "")]
    [InlineData(null, "", "")]
    public void Split_takes_the_last_word_as_the_last_name(string? text, string first, string last) =>
        Assert.Equal((first, last), PersonName.Split(text));

    [Theory]
    [InlineData("AnanyaR", "ananya r", true)]
    [InlineData("Ananya R", "AnanyaR", true)]
    [InlineData("RamakrishnanP", "krishnan", true)]
    [InlineData("RamakrishnanP", "xyz", false)]
    [InlineData("RamakrishnanP", "   ", false)]
    public void Matches_ignores_case_and_spaces(string name, string query, bool expected) =>
        Assert.Equal(expected, PersonName.Matches(name, query));
}
