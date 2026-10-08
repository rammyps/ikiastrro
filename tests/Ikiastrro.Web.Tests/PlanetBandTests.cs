using Ikiastrro.Core.Presentation;
using Xunit;

namespace Ikiastrro.Web.Tests;

public class PlanetBandTests
{
    [Theory]
    [InlineData(-9, "n4")] [InlineData(-4, "n4")] [InlineData(-1, "n1")] [InlineData(0, "z")]
    [InlineData(1, "p1")] [InlineData(4, "p4")] [InlineData(9, "p4")]
    public void Suffix_NamesTheNineStopsAndClamps(int stop, string expected) =>
        Assert.Equal(expected, PlanetBand.Suffix(stop));

    [Fact]
    public void Class_UsesThePlanetRampOrFallsBackToGolden()
    {
        Assert.Equal("pb pb-sun-p3", PlanetBand.Class("Sun", 3));
        Assert.Equal("pb pb-saturn-z", PlanetBand.Class(" saturn ", 0));
        Assert.Equal("pb pb-golden-n2", PlanetBand.Class("Lagna", -2));
        Assert.Equal("pb pb-golden-p1", PlanetBand.Class(null, 1));
        Assert.Equal("var(--pb-moon-n4)", PlanetBand.Var("Moon", -4));
    }

    [Theory]
    [InlineData("Exalted", 4)] [InlineData("Moolatrikona", 3)] [InlineData("Own Sign", 2)]
    [InlineData("Great Friend", 1)] [InlineData("Friend", 0)] [InlineData("Neutral", -1)]
    [InlineData("Enemy", -2)] [InlineData("Great Enemy", -3)] [InlineData("Debilitated", -4)]
    public void Dignity_MapsOntoTheSharedAxisAndBack(string status, int stop)
    {
        Assert.Equal(stop, PlanetBand.FromDignity(status));
        Assert.Equal(status, PlanetBand.DignityName(stop));
    }

    [Theory]
    [InlineData(0, -4)] [InlineData(50, -2)] [InlineData(100, 0)] [InlineData(129, 1)]
    [InlineData(150, 2)] [InlineData(195, 4)] [InlineData(400, 4)]
    public void Shadbala_ParIsOneHundredPercent(decimal percent, int stop) =>
        Assert.Equal(stop, PlanetBand.FromShadbalaPercent(percent));

    [Theory]
    [InlineData(0, -4)] [InlineData(4, 0)] [InlineData(8, 4)]
    public void Bindus_ParIsFour(int bindus, int stop) => Assert.Equal(stop, PlanetBand.FromBindus(bindus));

    [Theory]
    [InlineData(16, -4)] [InlineData(25, -1)] [InlineData(28, 0)] [InlineData(31, 1)] [InlineData(40, 4)]
    public void SarvaTotal_ParIsTwentyEight(int total, int stop) => Assert.Equal(stop, PlanetBand.FromSarvaTotal(total));
}

public class PlanetBandTokenTests
{
    [Theory]
    [InlineData("exalted", 4)] [InlineData("moolatrikona", 3)] [InlineData("good", 2)] [InlineData("own", 2)]
    [InlineData("great-friend", 1)] [InlineData("friend", 0)] [InlineData("neutral", -1)] [InlineData("enemy", -2)]
    [InlineData("great-enemy", -3)] [InlineData("debilitated", -4)] [InlineData(null, 0)] [InlineData("zzz", 0)]
    public void FromToken_MapsDignityTokensOntoTheAxis(string? token, int stop) =>
        Assert.Equal(stop, PlanetBand.FromToken(token));
}

public class PlanetBandScaleTests
{
    [Theory]
    [InlineData(2, -4)] [InlineData(5, -1)] [InlineData(6, 0)] [InlineData(7, 1)] [InlineData(10, 4)] [InlineData(14, 4)]
    public void BhavaRupas_SixIsPar(decimal rupas, int stop) => Assert.Equal(stop, PlanetBand.FromBhavaRupas(rupas));

    [Theory]
    [InlineData(0, -4)] [InlineData(10, 0)] [InlineData(15, 2)] [InlineData(20, 4)]
    public void Vimsopaka_TenIsPar(decimal score, int stop) => Assert.Equal(stop, PlanetBand.FromVimsopaka(score));
}
