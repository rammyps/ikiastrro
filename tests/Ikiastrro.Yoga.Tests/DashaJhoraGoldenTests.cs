using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Models;
using Ikiastrro.Core.Pipeline;
using Xunit.Abstractions;

namespace Ikiastrro.Yoga.Tests;

/// <summary>Dasa systems against JHora's "Copy complete calculations" for 1_Ramakrishnan (birth 1981-04-22, Lagna
/// 28 Pi, Moon 4 Sc 59), clipboard export 2026-09-06. JHora counts years a little differently from this repo
/// (maha boundaries differ by up to 3 days), so dates are compared to within 4 days.</summary>
public sealed class DashaJhoraGoldenTests(ITestOutputHelper output)
{
    private static readonly DateTime Birth = new(1981, 4, 22);

    private static ChartAnalysisInput Golden()
    {
        PlanetPosition P(string name, ZodiacName sign, double deg) => new()
        {
            Planet = name, Sign = sign.ToString(), NirayanaLongitudeDegrees = (int)sign * 30 + deg,
        };
        return new ChartAnalysisInput("D1", ZodiacName.Pisces,
        [
            P("Ascendant", ZodiacName.Pisces, 28.485), P("Sun", ZodiacName.Aries, 8.019), P("Moon", ZodiacName.Scorpio, 4.988),
            P("Mars", ZodiacName.Aries, 3.802), P("Mercury", ZodiacName.Aries, 1.452), P("Jupiter", ZodiacName.Virgo, 8.742),
            P("Venus", ZodiacName.Aries, 11.752), P("Saturn", ZodiacName.Virgo, 10.971),
            P("Rahu", ZodiacName.Cancer, 12.951), P("Ketu", ZodiacName.Capricornus, 12.951),
        ]);
    }

    [Fact]
    public void AshtottariMahaDatesMatchJhora()
    {
        var birth = new DateTimeOffset(1981, 4, 22, 5, 30, 0, TimeSpan.FromHours(5.5));
        var periods = AshtottariDashaCalculator.ComputeFromMoonLongitude(birth, 214.988756, 108);
        // JHora: Merc dasa began 1980-08-11 (before birth); the rest start on these dates.
        var expected = new (PlanetName, DateTime)[]
        {
            (PlanetName.Saturn, new(1997, 8, 11)), (PlanetName.Jupiter, new(2007, 8, 11)),
            (PlanetName.Rahu, new(2026, 8, 11)), (PlanetName.Venus, new(2038, 8, 11)),
            (PlanetName.Sun, new(2059, 8, 11)), (PlanetName.Moon, new(2065, 8, 10)), (PlanetName.Mars, new(2080, 8, 10)),
        };
        Assert.Equal(PlanetName.Mercury, periods[0].Lord);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Item1, periods[i + 1].Lord);
            Assert.InRange(Math.Abs((periods[i + 1].StartDate.DateTime - expected[i].Item2).TotalDays), 0, 4);
        }
    }

    [Fact]
    public void NarayanaMahaOrderAndStartsMatchJhora()
    {
        var plan = RasiDasa.Compute(RasiDasaSystem.Narayana, Golden(), Birth)!;
        foreach (var p in plan.Periods.Take(12)) output.WriteLine($"{p.Rasi} {p.Start:yyyy-MM-dd} {p.Years}");
        var expected = new (ZodiacName, DateTime)[]
        {
            (ZodiacName.Virgo, new(1981, 4, 22)), (ZodiacName.Libra, new(1986, 4, 22)), (ZodiacName.Scorpio, new(1992, 4, 21)),
            (ZodiacName.Sagittarius, new(1997, 4, 22)), (ZodiacName.Capricornus, new(2006, 4, 22)), (ZodiacName.Aquarius, new(2010, 4, 22)),
            (ZodiacName.Pisces, new(2015, 4, 22)), (ZodiacName.Aries, new(2021, 4, 21)), (ZodiacName.Taurus, new(2033, 4, 21)),
            (ZodiacName.Gemini, new(2044, 4, 21)), (ZodiacName.Cancer, new(2054, 4, 21)), (ZodiacName.Leo, new(2061, 4, 21)),
        };
        for (var i = 0; i < 12; i++)
        {
            Assert.Equal(expected[i].Item1, plan.Periods[i].Rasi);
            Assert.InRange(Math.Abs((plan.Periods[i].Start - expected[i].Item2).TotalDays), 0, 4);
        }
    }

    /// <summary>JHora starts this chart's Sudasa in Scorpio (the 7th from the Sree Lagna sign, which holds the Moon);
    /// PVR ch.20.2 (this repo's source) starts it in the Sree Lagna sign, Taurus. Both walk the same kendra cycle
    /// (Ta Le Sc Aq) and both take the first dasa's balance from the Sree Lagna's advancement, (30 − 13.18)/30 = 0.56.
    /// Recorded as a known divergence — the engine keeps PVR.</summary>
    [Fact]
    public void SudasaFollowsPvrAndSharesJhorasKendraCycleAndBalance()
    {
        var plan = RasiDasa.Compute(RasiDasaSystem.Sudasa, Golden(), Birth, sreeLagnaLongitude: 43.1815)!;
        Assert.Equal(ZodiacName.Taurus, plan.Seed);
        Assert.Equal(
            new[] { ZodiacName.Scorpio, ZodiacName.Leo, ZodiacName.Taurus, ZodiacName.Aquarius }.Order(),
            plan.Sequence.Take(4).Order());
        var fraction = plan.Periods[0].Years / 11;      // Taurus's first-cycle length is 11
        Assert.Equal(0.5606, fraction, 3);
    }
}
