using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.KeyInfo;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>JHora gap step 2 against JHora's own figures for 1_Ramakrishnan
/// (explorer_output/jhora/basics-views, 2026-09-23), fed JHora's longitudes.</summary>
public sealed class KeyInfoTests
{
    private const double Lagna = 0.6574, Sun = 8.2019, Moon = 217.2088, Mars = 3.9433, Mercury = 1.8326,
        Jupiter = 158.7238, Venus = 11.9832, Saturn = 160.9595, Rahu = 102.9146, Ketu = 282.9146,
        Maandi = 198.1023, HoraLagna = 354.9145;

    [Fact]
    public void YogiSahayogiAvayogi()
    {
        var r = YogiAvayogi.Compute(Sun, Moon);
        Assert.Equal(318.744, r.YogiPoint, 2);     // 18°44′ Aquarius
        Assert.Equal(PlanetName.Rahu, r.Yogi);
        Assert.Equal(PlanetName.Saturn, r.Sahayogi);
        Assert.Equal(145.411, r.AvayogiPoint, 2);  // 25°24′ Leo
        Assert.Equal(PlanetName.Venus, r.Avayogi);
    }

    [Fact]
    public void NavaTarasFromMoonAndLagna()
    {
        var moon = NavaTara.FromLongitude(Moon);
        Assert.Equal(PlanetName.Saturn, moon[0].Lord);   // Janma: Anurādhā, U.Bhādrapada, Puṣya
        Assert.Equal(new[] { ConstellationName.Anuradha, ConstellationName.Uttarabhadra, ConstellationName.Pushyami },
            moon[0].Nakshatras);
        Assert.Equal(PlanetName.Jupiter, moon[8].Lord);  // Parama Mitra

        var lagna = NavaTara.FromLongitude(Lagna);
        Assert.Equal(PlanetName.Ketu, lagna[0].Lord);
        Assert.Equal(PlanetName.Mercury, lagna[8].Lord);
    }

    [Theory]
    [InlineData("Sun", Sun, 11.2981, false)]
    [InlineData("Mars", Mars, 14.5567, false)]
    [InlineData("Mercury", Mercury, 12.6674, false)]
    [InlineData("Jupiter", Jupiter, 5.2238, false)]
    [InlineData("Venus", Venus, 15.5168, false)]
    [InlineData("Saturn", Saturn, 4.5405, false)]
    [InlineData("Rahu", Rahu, 2.4146, false)]
    [InlineData("Ketu", Ketu, 1.4146, false)]
    [InlineData("Maandi", Maandi, 10.6023, false)]
    [InlineData("Lagna", Lagna, 0.1574, true)]
    public void MrityuBhagaMatchesJhora(string body, double longitude, double distance, bool inside)
    {
        var r = MrityuPushkara.For(body, longitude)!;
        Assert.Equal(distance, r.FromMrityuBhagaCentre, 0.001);
        Assert.Equal(inside, r.InMrityuBhaga);
    }

    [Theory]
    [InlineData("Sun", Sun, false, false, 12.2981)]
    [InlineData("Moon", Moon, true, false, 3.2912)]
    [InlineData("Jupiter", Jupiter, true, true, 0.2238)]
    [InlineData("Saturn", Saturn, false, false, 2.4595)]
    [InlineData("Ketu", Ketu, false, false, 0.5854)]
    [InlineData("Maandi", Maandi, true, false, 5.3977)]
    [InlineData("Lagna", Lagna, false, false, 19.8426)]
    public void PushkaraMatchesJhora(string body, double longitude, bool amsa, bool bhaga, double distance)
    {
        var r = MrityuPushkara.For(body, longitude)!;
        Assert.Equal(amsa, r.InPushkaramsa);
        Assert.Equal(bhaga, r.InPushkaraBhaga);
        Assert.Equal(distance, r.FromPushkaraBhagaCentre, 0.001);
    }

    [Fact]
    public void BhriguBinduIsTheRahuToMoonMidpoint() =>
        Assert.Equal(160.0617, SensitivePoints.BhriguBindu(Rahu, Moon), 3);   // 10°03′ Virgo

    [Theory]
    [InlineData(1, ZodiacName.Pisces)]
    [InlineData(2, ZodiacName.Gemini)]
    [InlineData(3, ZodiacName.Scorpio)]
    [InlineData(4, ZodiacName.Libra)]
    [InlineData(5, ZodiacName.Cancer)]
    [InlineData(6, ZodiacName.Aquarius)]
    [InlineData(7, ZodiacName.Pisces)]
    [InlineData(8, ZodiacName.Aquarius)]
    [InlineData(9, ZodiacName.Cancer)]
    [InlineData(10, ZodiacName.Libra)]
    [InlineData(11, ZodiacName.Scorpio)]
    [InlineData(12, ZodiacName.Gemini)]
    public void VarnadaLagnasMatchJhora(int house, ZodiacName sign)
    {
        var v = SensitivePoints.Varnada(Lagna, HoraLagna, house);
        Assert.Equal(sign, (ZodiacName)(int)(v / 30));
        Assert.Equal(Lagna, v % 30, 4);
    }
}
