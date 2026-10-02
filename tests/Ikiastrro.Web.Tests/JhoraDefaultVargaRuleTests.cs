using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.DivisionalCharts;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>D2/D3/D7 (migration 164) and D10/D16/D24/D60 (migration 166) on Jagannatha Hora's schemes. Expected signs are
/// RamakrishnanP's JHora "Rasis occupied in all vargas" export (2026-09-23), longitudes from its
/// "Natal longitudes" view.</summary>
public class JhoraDefaultVargaRuleTests
{
    [Theory]
    [InlineData(8.2019, ZodiacName.Aries)]        // Sun 8 Ar 12'
    [InlineData(217.2088, ZodiacName.Pisces)]     // Moon 7 Sc 12'
    [InlineData(11.9832, ZodiacName.Taurus)]      // Venus 11 Ar 58'
    [InlineData(160.9596, ZodiacName.Leo)]        // Saturn 10 Vi 57'
    [InlineData(102.9146, ZodiacName.Aquarius)]   // Rahu 12 Cn 54'
    [InlineData(282.9146, ZodiacName.Leo)]        // Ketu 12 Cp 54'
    [InlineData(354.9145, ZodiacName.Capricornus)] // Hora Lagna 24 Pi 54'
    [InlineData(141.5352, ZodiacName.Gemini)]     // Dhooma 21 Le 32'
    [InlineData(329.1490, ZodiacName.Sagittarius)] // Yama Ghantaka 29 Aq 08'
    public void D3UmaShambuMatchesJhora(double longitude, ZodiacName expected) =>
        Assert.Equal(expected, new DrekkanaD3UmaShambuSignRule().SignFor(longitude));

    [Theory]
    [InlineData(8.2019, ZodiacName.Taurus)]       // Sun
    [InlineData(217.2088, ZodiacName.Aries)]      // Moon
    [InlineData(158.7238, ZodiacName.Capricornus)] // Jupiter 8 Vi 43'
    [InlineData(11.9832, ZodiacName.Gemini)]      // Venus
    [InlineData(160.9596, ZodiacName.Capricornus)] // Saturn
    [InlineData(102.9146, ZodiacName.Libra)]      // Rahu
    [InlineData(282.9146, ZodiacName.Aries)]      // Ketu
    public void D7EvenReverseMatchesJhora(double longitude, ZodiacName expected) =>
        Assert.Equal(expected, new SaptamsaD7EvenReverseSignRule().SignFor(longitude));

    [Theory]
    [InlineData(8.2019, ZodiacName.Aries)]        // Sun
    [InlineData(217.2088, ZodiacName.Cancer)]     // Moon
    [InlineData(158.7238, ZodiacName.Pisces)]     // Jupiter
    [InlineData(354.9145, ZodiacName.Aquarius)]   // Hora Lagna
    public void D2UmaShambuMatchesJhora(double longitude, ZodiacName expected) =>
        Assert.Equal(expected, new HoraD2UmaShambuSignRule().SignFor(longitude));

    // D10: JHora "D-10 (5-8)" (migration 166, decision 007).
    [Theory]
    [InlineData(217.2088, ZodiacName.Capricornus)] // Moon 7 Sc
    [InlineData(158.7238, ZodiacName.Scorpio)] // Jupiter 8 Vi
    [InlineData(160.9595, ZodiacName.Libra)] // Saturn 10 Vi
    [InlineData(354.9145, ZodiacName.Scorpio)] // Hora Lagna 24 Pi
    [InlineData(334.9834, ZodiacName.Gemini)] // Ghati Lagna 4 Pi
    [InlineData(235.3279, ZodiacName.Cancer)] // Vighati Lagna 25 Sc
    [InlineData(330.6574, ZodiacName.Cancer)] // Varnada Lagna 0 Pi
    [InlineData(105.2938, ZodiacName.Gemini)] // Sree Lagna 15 Cn
    [InlineData(235.3279, ZodiacName.Cancer)] // Pranapada Lagna 25 Sc
    [InlineData(0.6574, ZodiacName.Aries)] // Lagna 0 Ar (unchanged)
    [InlineData(8.2019, ZodiacName.Gemini)] // Sun 8 Ar (unchanged)
    [InlineData(3.9433, ZodiacName.Taurus)] // Mars 3 Ar (unchanged)
    public void D10EvenReverseMatchesJhora(double longitude, ZodiacName expected) =>
        Assert.Equal(expected, new DasamsaD10EvenReverseSignRule().SignFor(longitude));

    // D16: JHora "D-16 (Rev)" (migration 166, decision 007).
    [Theory]
    [InlineData(217.2088, ZodiacName.Leo)] // Moon 7 Sc
    [InlineData(158.7238, ZodiacName.Scorpio)] // Jupiter 8 Vi
    [InlineData(160.9595, ZodiacName.Libra)] // Saturn 10 Vi
    [InlineData(102.9146, ZodiacName.Capricornus)] // Rahu 12 Cn
    [InlineData(282.9146, ZodiacName.Capricornus)] // Ketu 12 Cp
    [InlineData(354.9145, ZodiacName.Aquarius)] // Hora Lagna 24 Pi
    [InlineData(334.9834, ZodiacName.Capricornus)] // Ghati Lagna 4 Pi
    [InlineData(235.3279, ZodiacName.Libra)] // Vighati Lagna 25 Sc
    [InlineData(330.6574, ZodiacName.Pisces)] // Varnada Lagna 0 Pi
    [InlineData(0.6574, ZodiacName.Aries)] // Lagna 0 Ar (unchanged)
    [InlineData(8.2019, ZodiacName.Leo)] // Sun 8 Ar (unchanged)
    [InlineData(3.9433, ZodiacName.Gemini)] // Mars 3 Ar (unchanged)
    public void D16EvenReverseMatchesJhora(double longitude, ZodiacName expected) =>
        Assert.Equal(expected, new ShodasamsaD16EvenReverseSignRule().SignFor(longitude));

    // D24: JHora "D-24 (Rev)" (migration 166, decision 007).
    [Theory]
    [InlineData(217.2088, ZodiacName.Aquarius)] // Moon 7 Sc
    [InlineData(160.9595, ZodiacName.Scorpio)] // Saturn 10 Vi
    [InlineData(102.9146, ZodiacName.Virgo)] // Rahu 12 Cn
    [InlineData(282.9146, ZodiacName.Virgo)] // Ketu 12 Cp
    [InlineData(354.9145, ZodiacName.Sagittarius)] // Hora Lagna 24 Pi
    [InlineData(334.9834, ZodiacName.Aries)] // Ghati Lagna 4 Pi
    [InlineData(235.3279, ZodiacName.Scorpio)] // Vighati Lagna 25 Sc
    [InlineData(235.3279, ZodiacName.Scorpio)] // Pranapada Lagna 25 Sc
    [InlineData(160.0617, ZodiacName.Scorpio)] // Bhrigu Bindu 10 Vi
    [InlineData(0.6574, ZodiacName.Leo)] // Lagna 0 Ar (unchanged)
    [InlineData(8.2019, ZodiacName.Aquarius)] // Sun 8 Ar (unchanged)
    [InlineData(3.9433, ZodiacName.Scorpio)] // Mars 3 Ar (unchanged)
    public void D24EvenReverseMatchesJhora(double longitude, ZodiacName expected) =>
        Assert.Equal(expected, new SiddhamsaD24EvenReverseSignRule().SignFor(longitude));

    // D60: JHora "D-60 (RvAr)" (migration 166, decision 007).
    [Theory]
    [InlineData(158.7238, ZodiacName.Libra)] // Jupiter 8 Vi
    [InlineData(102.9146, ZodiacName.Aquarius)] // Rahu 12 Cn
    [InlineData(198.1023, ZodiacName.Aries)] // Maandi 18 Li
    [InlineData(187.7294, ZodiacName.Cancer)] // Gulika 7 Li
    [InlineData(354.9145, ZodiacName.Aquarius)] // Hora Lagna 24 Pi
    [InlineData(334.9834, ZodiacName.Gemini)] // Ghati Lagna 4 Pi
    [InlineData(330.6574, ZodiacName.Aquarius)] // Varnada Lagna 0 Pi
    [InlineData(105.2938, ZodiacName.Virgo)] // Sree Lagna 15 Cn
    [InlineData(247.2088, ZodiacName.Gemini)] // Indu Lagna 7 Sg
    [InlineData(0.6574, ZodiacName.Taurus)] // Lagna 0 Ar (unchanged)
    [InlineData(8.2019, ZodiacName.Leo)] // Sun 8 Ar (unchanged)
    [InlineData(217.2088, ZodiacName.Capricornus)] // Moon 7 Sc (unchanged)
    public void D60EvenReverseFromAriesMatchesJhora(double longitude, ZodiacName expected) =>
        Assert.Equal(expected, new ShashtyamsaD60EvenReverseFromAriesSignRule().SignFor(longitude));

    [Fact]
    public void FactoryResolvesTheNewKeys()
    {
        Assert.IsType<DrekkanaD3UmaShambuSignRule>(VargaSignRuleFactory.For("DrekkanaD3UmaShambu", 3));
        Assert.IsType<SaptamsaD7EvenReverseSignRule>(VargaSignRuleFactory.For("SaptamsaD7EvenReverse", 7));
        Assert.IsType<DasamsaD10EvenReverseSignRule>(VargaSignRuleFactory.For("DasamsaD10EvenReverse", 10));
        Assert.IsType<ShodasamsaD16EvenReverseSignRule>(VargaSignRuleFactory.For("ShodasamsaD16EvenReverse", 16));
        Assert.IsType<SiddhamsaD24EvenReverseSignRule>(VargaSignRuleFactory.For("SiddhamsaD24EvenReverse", 24));
        Assert.IsType<ShashtyamsaD60EvenReverseFromAriesSignRule>(VargaSignRuleFactory.For("ShashtyamsaD60EvenReverseFromAries", 60));
    }
}
