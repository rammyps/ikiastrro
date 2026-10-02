using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.DivisionalCharts;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>D2/D3/D7 on Jagannatha Hora's default schemes (migration 164). Expected signs are
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

    [Fact]
    public void FactoryResolvesTheNewKeys()
    {
        Assert.IsType<DrekkanaD3UmaShambuSignRule>(VargaSignRuleFactory.For("DrekkanaD3UmaShambu", 3));
        Assert.IsType<SaptamsaD7EvenReverseSignRule>(VargaSignRuleFactory.For("SaptamsaD7EvenReverse", 7));
    }
}
