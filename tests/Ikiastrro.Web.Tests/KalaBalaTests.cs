using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>Nathonnata, Abda and Māsa Bala — 1_Ramakrishnan figures from JHora (2026-09-23).</summary>
public sealed class KalaBalaTests
{
    private static readonly TimeSpan Ist = TimeSpan.FromHours(5.5);

    // JHora's own times for the birth: sunrise 05:56:39 and sunset 18:18:53 on 21 Apr, next sunrise ~05:56.
    private static readonly SunTimes Ramakrishnan = new(
        new DateTimeOffset(1981, 4, 21, 5, 56, 39, Ist), new DateTimeOffset(1981, 4, 21, 18, 18, 53, Ist),
        new DateTimeOffset(1981, 4, 22, 5, 56, 20, Ist), IsNightBirth: true);

    [Fact]
    public void NathonnataIsGradedByTheHourFromApparentMidnight()
    {
        var birth = new DateTimeOffset(1981, 4, 22, 5, 30, 1, Ist);

        // Midnight 00:07:36, birth 5.37 h later → 26.87 (JHora 27.49).
        Assert.InRange(NathonnataBala.Compute(PlanetName.Sun, birth, Ramakrishnan), 26.8, 26.95);
        Assert.Equal(60 - NathonnataBala.Compute(PlanetName.Sun, birth, Ramakrishnan),
            NathonnataBala.Compute(PlanetName.Saturn, birth, Ramakrishnan), 6);
        Assert.Equal(60, NathonnataBala.Compute(PlanetName.Mercury, birth, Ramakrishnan));
    }

    [Fact]
    public void NathonnataPeaksAtNoonForDayStrongPlanets()
    {
        var day = Ramakrishnan with { IsNightBirth = false };
        var noon = new DateTimeOffset(1981, 4, 21, 12, 7, 46, Ist);   // midpoint of sunrise and sunset

        Assert.InRange(NathonnataBala.Compute(PlanetName.Jupiter, noon, day), 59.9, 60);
        Assert.InRange(NathonnataBala.Compute(PlanetName.Moon, noon, day), 0, 0.1);
    }

    [Fact]
    public void AbdaAndMasaLordsMatchJhoraForRamakrishnan()
    {
        var date = new DateOnly(1981, 4, 22);
        Assert.Equal(PlanetName.Mars, AbdaMasaLords.AbdaLord(date));
        Assert.Equal(PlanetName.Saturn, AbdaMasaLords.MasaLord(date));
    }

    [Fact]
    public void AYearLaterTheAbdaLordMovesThreeWeekdays()
    {
        // 360 days = 51 weeks + 3 days, so the next Abda starts three weekdays on — also before 1951.
        var start = new DateOnly(1918, 10, 16);
        var order = new[] { PlanetName.Mars, PlanetName.Mercury, PlanetName.Jupiter, PlanetName.Venus,
            PlanetName.Saturn, PlanetName.Sun, PlanetName.Moon };
        var a = Array.IndexOf(order, AbdaMasaLords.AbdaLord(start));
        var b = Array.IndexOf(order, AbdaMasaLords.AbdaLord(start.AddDays(360)));
        Assert.Equal((a + 3) % 7, b);
    }
}
