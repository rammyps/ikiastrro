using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>
/// Golden record from PVR Integrated Approach Sec. 6.6, Example 27 (Bill Cosby, Jupiter,
/// 12 Jul 1937 00:30, TZ 5:00W, 75W10 39N57). The book gives every varga sign directly, so
/// this test reproduces its own worked arithmetic rather than the ephemeris.
/// </summary>
public class AmsabalaCalculatorTests
{
    [Fact]
    public void MatchesPvrWorkedExample27_JupiterBillCosby()
    {
        var charts = new[]
        {
            Chart("D1", "Sagittarius"), Chart("D2", "Cancer"), Chart("D3", "Libra"),
            Chart("D4", "Virgo"), Chart("D7", "Gemini"), Chart("D9", "Sagittarius"),
            Chart("D10", "Virgo"), Chart("D12", "Scorpio"), Chart("D16", "Pisces"),
            Chart("D20", "Pisces"), Chart("D24", "Cancer"), Chart("D27", "Gemini"),
            Chart("D30", "Libra"), Chart("D40", "Cancer"), Chart("D45", "Leo"), Chart("D60", "Scorpio")
        };

        var results = AmsabalaCalculator.Calculate(PlanetName.Jupiter, charts, Groups(), Names())
            .ToDictionary(r => r.SchemeCode);

        // Jupiter owns Sagittarius/Pisces and is exalted in Cancer -- any of those 3 signs counts good.
        Assert.Equal(3, results["SHADVARGA"].GoodCount);
        Assert.Equal("Vyanjanamsa", results["SHADVARGA"].AmsaName);

        Assert.Equal(3, results["SAPTAVARGA"].GoodCount);
        Assert.Equal("Vyanjanamsa", results["SAPTAVARGA"].AmsaName);

        Assert.Equal(4, results["DASAVARGA"].GoodCount);
        Assert.Equal("Gopuramsa", results["DASAVARGA"].AmsaName);

        Assert.Equal(7, results["SHODASAVARGA"].GoodCount);
        Assert.Equal("Kalpavrikshamsa", results["SHODASAVARGA"].AmsaName);
    }

    [Fact]
    public void ReturnsNullAmsaNameBelowCountTwo()
    {
        // Jupiter in Aries everywhere: neutral, not own/moolatrikona/exalted anywhere -- 0 good.
        var charts = new[] { Chart("D1", "Aries"), Chart("D2", "Aries"), Chart("D3", "Aries"),
            Chart("D9", "Aries"), Chart("D12", "Aries"), Chart("D30", "Aries") };

        var result = AmsabalaCalculator.Calculate(PlanetName.Jupiter, charts, Groups(), Names())
            .Single(r => r.SchemeCode == "SHADVARGA");

        Assert.Equal(0, result.GoodCount);
        Assert.Null(result.AmsaName);
    }

    private static ChartAnalysisInput Chart(string type, string jupiterSign) =>
        new(type, ZodiacName.Aries, new List<PlanetPosition>
        {
            new() { Planet = "Jupiter", Sign = jupiterSign, NirayanaLongitudeDegrees = ((int)Enum.Parse<ZodiacName>(jupiterSign) * 30) + 15 }
        });

    private static IReadOnlyList<AmsabalaGroupMember> Groups() => new[]
    {
        new AmsabalaGroupMember("SHADVARGA", "D1"), new AmsabalaGroupMember("SHADVARGA", "D2"),
        new AmsabalaGroupMember("SHADVARGA", "D3"), new AmsabalaGroupMember("SHADVARGA", "D9"),
        new AmsabalaGroupMember("SHADVARGA", "D12"), new AmsabalaGroupMember("SHADVARGA", "D30"),

        new AmsabalaGroupMember("SAPTAVARGA", "D1"), new AmsabalaGroupMember("SAPTAVARGA", "D2"),
        new AmsabalaGroupMember("SAPTAVARGA", "D3"), new AmsabalaGroupMember("SAPTAVARGA", "D7"),
        new AmsabalaGroupMember("SAPTAVARGA", "D9"), new AmsabalaGroupMember("SAPTAVARGA", "D12"),
        new AmsabalaGroupMember("SAPTAVARGA", "D30"),

        new AmsabalaGroupMember("DASAVARGA", "D1"), new AmsabalaGroupMember("DASAVARGA", "D2"),
        new AmsabalaGroupMember("DASAVARGA", "D3"), new AmsabalaGroupMember("DASAVARGA", "D7"),
        new AmsabalaGroupMember("DASAVARGA", "D9"), new AmsabalaGroupMember("DASAVARGA", "D10"),
        new AmsabalaGroupMember("DASAVARGA", "D12"), new AmsabalaGroupMember("DASAVARGA", "D16"),
        new AmsabalaGroupMember("DASAVARGA", "D30"), new AmsabalaGroupMember("DASAVARGA", "D60"),

        new AmsabalaGroupMember("SHODASAVARGA", "D1"), new AmsabalaGroupMember("SHODASAVARGA", "D2"),
        new AmsabalaGroupMember("SHODASAVARGA", "D3"), new AmsabalaGroupMember("SHODASAVARGA", "D4"),
        new AmsabalaGroupMember("SHODASAVARGA", "D7"), new AmsabalaGroupMember("SHODASAVARGA", "D9"),
        new AmsabalaGroupMember("SHODASAVARGA", "D10"), new AmsabalaGroupMember("SHODASAVARGA", "D12"),
        new AmsabalaGroupMember("SHODASAVARGA", "D16"), new AmsabalaGroupMember("SHODASAVARGA", "D20"),
        new AmsabalaGroupMember("SHODASAVARGA", "D24"), new AmsabalaGroupMember("SHODASAVARGA", "D27"),
        new AmsabalaGroupMember("SHODASAVARGA", "D30"), new AmsabalaGroupMember("SHODASAVARGA", "D40"),
        new AmsabalaGroupMember("SHODASAVARGA", "D45"), new AmsabalaGroupMember("SHODASAVARGA", "D60")
    };

    private static IReadOnlyList<AmsabalaNameEntry> Names() => new[]
    {
        new AmsabalaNameEntry("SHADVARGA", 2, "Kimsukamsa"), new AmsabalaNameEntry("SHADVARGA", 3, "Vyanjanamsa"),
        new AmsabalaNameEntry("SHADVARGA", 4, "Chaamaramsa"), new AmsabalaNameEntry("SHADVARGA", 5, "Chatramsa"),
        new AmsabalaNameEntry("SHADVARGA", 6, "Kundalamsa"),

        new AmsabalaNameEntry("SAPTAVARGA", 2, "Kimsukamsa"), new AmsabalaNameEntry("SAPTAVARGA", 3, "Vyanjanamsa"),
        new AmsabalaNameEntry("SAPTAVARGA", 4, "Chaamaramsa"), new AmsabalaNameEntry("SAPTAVARGA", 5, "Chatramsa"),
        new AmsabalaNameEntry("SAPTAVARGA", 6, "Kundalamsa"), new AmsabalaNameEntry("SAPTAVARGA", 7, "Mukutamsa"),

        new AmsabalaNameEntry("DASAVARGA", 2, "Paarijaatamsa"), new AmsabalaNameEntry("DASAVARGA", 3, "Uttamamsa"),
        new AmsabalaNameEntry("DASAVARGA", 4, "Gopuramsa"), new AmsabalaNameEntry("DASAVARGA", 5, "Simhaasanamsa"),
        new AmsabalaNameEntry("DASAVARGA", 6, "Paaraavatamsa"), new AmsabalaNameEntry("DASAVARGA", 7, "Devalokamsa"),
        new AmsabalaNameEntry("DASAVARGA", 8, "Brahmalokamsa"), new AmsabalaNameEntry("DASAVARGA", 9, "Airaavatamsa"),
        new AmsabalaNameEntry("DASAVARGA", 10, "Sreedhaamamsa"),

        new AmsabalaNameEntry("SHODASAVARGA", 2, "Bhedakamsa"), new AmsabalaNameEntry("SHODASAVARGA", 3, "Kusumamsa"),
        new AmsabalaNameEntry("SHODASAVARGA", 4, "Nagapurushamsa"), new AmsabalaNameEntry("SHODASAVARGA", 5, "Kandukamsa"),
        new AmsabalaNameEntry("SHODASAVARGA", 6, "Keralamsa"), new AmsabalaNameEntry("SHODASAVARGA", 7, "Kalpavrikshamsa"),
        new AmsabalaNameEntry("SHODASAVARGA", 8, "Chandanavanamsa"), new AmsabalaNameEntry("SHODASAVARGA", 9, "Poornachandramsa"),
        new AmsabalaNameEntry("SHODASAVARGA", 10, "Uchchaisravamsa"), new AmsabalaNameEntry("SHODASAVARGA", 11, "Dhanvantaryamsa"),
        new AmsabalaNameEntry("SHODASAVARGA", 12, "Sooryakaantamsa"), new AmsabalaNameEntry("SHODASAVARGA", 13, "Vidrumamsa"),
        new AmsabalaNameEntry("SHODASAVARGA", 14, "Indraasanamsa"), new AmsabalaNameEntry("SHODASAVARGA", 15, "Golokamsa"),
        new AmsabalaNameEntry("SHODASAVARGA", 16, "SreeVallabhamsa")
    };
}
