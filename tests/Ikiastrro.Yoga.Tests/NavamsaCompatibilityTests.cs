using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>NavamsaCompatibility is lookup and sign arithmetic over stored positions; these tests pin the
/// arithmetic (the 7th from the D9 Lagna, its lord, occupants, vargottama) on a hand-built chart.</summary>
public class NavamsaCompatibilityTests
{
    private static VargaPoint P(ZodiacName sign, int house, string? dignity = null) => new(sign, house, dignity);

    // D1 Lagna Aries (7th = Libra, lord Venus). D9 Lagna Virgo (7th = Pisces, lord Jupiter).
    private static NavamsaChart Chart(PlanetName? dk = PlanetName.Jupiter) => new(
        ZodiacName.Aries,
        new Dictionary<PlanetName, ZodiacName>
        {
            [PlanetName.Sun] = ZodiacName.Leo, [PlanetName.Moon] = ZodiacName.Cancer, [PlanetName.Mars] = ZodiacName.Aries,
            [PlanetName.Mercury] = ZodiacName.Gemini, [PlanetName.Jupiter] = ZodiacName.Cancer, [PlanetName.Venus] = ZodiacName.Taurus,
            [PlanetName.Saturn] = ZodiacName.Aquarius, [PlanetName.Rahu] = ZodiacName.Gemini, [PlanetName.Ketu] = ZodiacName.Sagittarius,
        },
        ZodiacName.Virgo,
        new Dictionary<PlanetName, VargaPoint>
        {
            [PlanetName.Sun] = P(ZodiacName.Pisces, 7), [PlanetName.Moon] = P(ZodiacName.Virgo, 1, "Great Friend"),
            [PlanetName.Mars] = P(ZodiacName.Cancer, 11, "Debilitated"), [PlanetName.Mercury] = P(ZodiacName.Cancer, 11),
            [PlanetName.Jupiter] = P(ZodiacName.Cancer, 11, "Exalted"), [PlanetName.Venus] = P(ZodiacName.Taurus, 9),
            [PlanetName.Saturn] = P(ZodiacName.Cancer, 11), [PlanetName.Rahu] = P(ZodiacName.Gemini, 10), [PlanetName.Ketu] = P(ZodiacName.Sagittarius, 4),
        },
        dk);

    [Fact]
    public void The_d9_7th_is_counted_from_the_d9_lagna_and_its_lord_is_read_from_d9()
    {
        var r = NavamsaCompatibility.Read(Chart());
        Assert.Equal(ZodiacName.Pisces, r.SeventhSign);
        Assert.Equal(PlanetName.Jupiter, r.SeventhLord);
        Assert.Equal(ZodiacName.Cancer, r.SeventhLordPlacement.Sign);
        Assert.Equal("Exalted", r.SeventhLordPlacement.Dignity);
        Assert.Equal(PlanetName.Mercury, r.D9LagnaLord);
    }

    [Fact]
    public void Occupants_of_the_d9_7th_are_the_grahas_in_that_sign()
    {
        Assert.Equal([PlanetName.Sun], NavamsaCompatibility.Read(Chart()).SeventhOccupants);
    }

    [Fact]
    public void Same_7th_lord_compares_the_d1_and_d9_7th_lords()
    {
        var r = NavamsaCompatibility.Read(Chart());
        Assert.Equal(PlanetName.Venus, r.D1SeventhLord);
        Assert.False(r.SameSeventhLord);
    }

    [Fact]
    public void Vargottama_lists_only_points_in_the_same_sign_in_d1_and_d9()
    {
        var r = NavamsaCompatibility.Read(Chart());
        // Venus is Taurus in both and is the D1 7th lord; Jupiter is Cancer in both and is the Darakaraka.
        Assert.Contains(("Venus", PlanetName.Venus), r.Vargottama);
        Assert.Contains(("D1 7th lord", PlanetName.Venus), r.Vargottama);
        Assert.Contains(("Jupiter", PlanetName.Jupiter), r.Vargottama);
        Assert.Contains(("Darakaraka", PlanetName.Jupiter), r.Vargottama);
    }

    [Fact]
    public void The_darakaraka_is_reported_with_its_d1_and_d9_signs_and_is_optional()
    {
        var r = NavamsaCompatibility.Read(Chart(PlanetName.Mars));
        Assert.Equal(PlanetName.Mars, r.Darakaraka);
        Assert.Equal(ZodiacName.Aries, r.DarakarakaD1Sign);
        Assert.Equal(ZodiacName.Cancer, r.DarakarakaD9!.Sign);
        Assert.DoesNotContain(r.Vargottama, v => v.Point == "Darakaraka");

        var none = NavamsaCompatibility.Read(Chart(null));
        Assert.Null(none.Darakaraka);
        Assert.Null(none.DarakarakaD9);
    }
}
