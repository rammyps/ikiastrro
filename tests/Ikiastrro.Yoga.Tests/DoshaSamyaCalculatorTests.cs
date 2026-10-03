using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>DoshaSamyaCalculator against Vasudev, "The Art of Matching Charts", Ch. V (pp.51-61):
/// the afflictions the book lists for its Chart 9 (girl) and Chart 10 (boy), the severity tiers and the
/// balancing rule.</summary>
public class DoshaSamyaCalculatorTests
{
    private static DoshaChart Chart(ZodiacName lagna, ZodiacName moon, ZodiacName venus, ZodiacName mars,
        ZodiacName sun = ZodiacName.Leo, ZodiacName saturn = ZodiacName.Leo, ZodiacName rahu = ZodiacName.Leo, ZodiacName ketu = ZodiacName.Aquarius) =>
        new(lagna, new Dictionary<PlanetName, ZodiacName>
        {
            [PlanetName.Sun] = sun, [PlanetName.Moon] = moon, [PlanetName.Mars] = mars, [PlanetName.Mercury] = ZodiacName.Gemini,
            [PlanetName.Jupiter] = ZodiacName.Gemini, [PlanetName.Venus] = venus, [PlanetName.Saturn] = saturn,
            [PlanetName.Rahu] = rahu, [PlanetName.Ketu] = ketu,
        });

    private static (PlanetName, DoshaReference, int) Key(DoshaAffliction a) => (a.Malefic, a.From, a.House);

    // Book p.55-56, Chart 9 (girl): Mars in the 8th from Lagna and from the Moon, Rahu in the 4th from
    // Lagna and from the Moon, Mars in the 7th from Venus, the Sun in the 12th from Venus. Placed here with
    // Lagna, Sun and Moon in Aries, Venus in Taurus, Mars in Scorpio and Rahu in Cancer.
    private static readonly DoshaChart Chart9 = Chart(ZodiacName.Aries, ZodiacName.Aries, ZodiacName.Taurus,
        mars: ZodiacName.Scorpio, sun: ZodiacName.Aries, rahu: ZodiacName.Cancer, saturn: ZodiacName.Leo);

    // Book p.56, Chart 10 (boy): Mars and Saturn in the 2nd from Lagna, the Moon and Venus (all three in
    // one sign, Mars and Saturn in the next).
    private static readonly DoshaChart Chart10 = Chart(ZodiacName.Aries, ZodiacName.Aries, ZodiacName.Aries,
        mars: ZodiacName.Taurus, saturn: ZodiacName.Taurus, sun: ZodiacName.Leo, rahu: ZodiacName.Leo);

    [Fact]
    public void Chart_9_afflictions_are_the_ones_the_book_lists()
    {
        var keys = DoshaSamyaCalculator.Read(Chart9).Afflictions.Select(Key).ToHashSet();

        Assert.Contains((PlanetName.Mars, DoshaReference.Lagna, 8), keys);
        Assert.Contains((PlanetName.Rahu, DoshaReference.Lagna, 4), keys);
        Assert.Contains((PlanetName.Mars, DoshaReference.Moon, 8), keys);
        Assert.Contains((PlanetName.Rahu, DoshaReference.Moon, 4), keys);
        Assert.Contains((PlanetName.Mars, DoshaReference.Venus, 7), keys);
        Assert.Contains((PlanetName.Sun, DoshaReference.Venus, 12), keys);
    }

    [Fact]
    public void Chart_10_afflictions_are_the_six_in_the_2nd_house()
    {
        var keys = DoshaSamyaCalculator.Read(Chart10).Afflictions.Select(Key).ToHashSet();

        foreach (var malefic in new[] { PlanetName.Mars, PlanetName.Saturn })
            foreach (var from in Enum.GetValues<DoshaReference>())
                Assert.Contains((malefic, from, 2), keys);
        Assert.Equal(6, keys.Count);
    }

    [Fact]
    public void Severity_follows_the_books_tiers()
    {
        var chart9 = DoshaSamyaCalculator.Read(Chart9).Afflictions;
        Assert.Equal(DoshaWeight.Severe, chart9.Single(a => a.Malefic == PlanetName.Mars && a.From == DoshaReference.Lagna).Weight);
        Assert.Equal(DoshaWeight.Notable, chart9.First(a => a.Malefic == PlanetName.Rahu).Weight);
        Assert.Equal(DoshaWeight.Untiered, chart9.Single(a => a.Malefic == PlanetName.Sun).Weight);

        var chart10 = DoshaSamyaCalculator.Read(Chart10).Afflictions;
        Assert.Equal(DoshaWeight.Lesser, chart10.First(a => a.Malefic == PlanetName.Mars).Weight);
        Assert.Equal(DoshaWeight.Untiered, chart10.First(a => a.Malefic == PlanetName.Saturn).Weight);
    }

    [Fact]
    public void Mars_in_the_first_is_conditional_and_flagged_inimical_in_the_books_signs()
    {
        // Mars in Taurus with the Lagna in Taurus (p.55).
        var inimical = DoshaSamyaCalculator.Read(Chart(ZodiacName.Taurus, ZodiacName.Leo, ZodiacName.Leo, ZodiacName.Taurus))
            .Afflictions.Single(a => a.Malefic == PlanetName.Mars && a.From == DoshaReference.Lagna);
        Assert.Equal(DoshaWeight.Conditional, inimical.Weight);
        Assert.Contains("highly inimical", inimical.Note);

        var plain = DoshaSamyaCalculator.Read(Chart(ZodiacName.Aries, ZodiacName.Leo, ZodiacName.Leo, ZodiacName.Aries))
            .Afflictions.Single(a => a.Malefic == PlanetName.Mars && a.From == DoshaReference.Lagna);
        Assert.DoesNotContain("highly inimical", plain.Note);
    }

    [Fact]
    public void Chart_9_girl_against_chart_10_boy_is_not_balanced()
    {
        // The book: the girl's afflictions on the 4th, 7th and 8th far outweigh the boy's, which sit round the 2nd.
        var balance = DoshaSamyaCalculator.Balance(
            boy: DoshaSamyaCalculator.Read(Chart10), girl: DoshaSamyaCalculator.Read(Chart9));
        Assert.Equal(DoshaBalanceStatus.NotBalanced, balance.Status);
    }

    [Fact]
    public void Both_charts_with_mars_in_the_8th_are_balanced_and_girl_8th_with_boy_7th_is_partly()
    {
        var eighth = Chart(ZodiacName.Aries, ZodiacName.Leo, ZodiacName.Leo, ZodiacName.Scorpio);
        var seventh = Chart(ZodiacName.Aries, ZodiacName.Leo, ZodiacName.Leo, ZodiacName.Libra);

        Assert.Equal(DoshaBalanceStatus.Balanced,
            DoshaSamyaCalculator.Balance(DoshaSamyaCalculator.Read(eighth), DoshaSamyaCalculator.Read(eighth)).Status);
        Assert.Equal(DoshaBalanceStatus.PartlyBalanced,
            DoshaSamyaCalculator.Balance(DoshaSamyaCalculator.Read(seventh), DoshaSamyaCalculator.Read(eighth)).Status);
        // Girl with Mars in the 7th: the boy's 7th or 8th is fine (p.57).
        Assert.Equal(DoshaBalanceStatus.Balanced,
            DoshaSamyaCalculator.Balance(DoshaSamyaCalculator.Read(eighth), DoshaSamyaCalculator.Read(seventh)).Status);
    }

    [Fact]
    public void Neither_chart_with_a_severe_dosha_is_reported_as_such()
    {
        var calm = DoshaSamyaCalculator.Read(Chart(ZodiacName.Aries, ZodiacName.Leo, ZodiacName.Leo, ZodiacName.Taurus));
        Assert.Equal(DoshaBalanceStatus.NoSevereDosha, DoshaSamyaCalculator.Balance(calm, calm).Status);
    }

    // The two saved charts used in the Astro Facts audit (RamakrishnanP and RameshwariS), hand-derived.
    private static readonly DoshaChart Husband = new(ZodiacName.Aries, new Dictionary<PlanetName, ZodiacName>
    {
        [PlanetName.Sun] = ZodiacName.Aries, [PlanetName.Moon] = ZodiacName.Scorpio, [PlanetName.Mars] = ZodiacName.Aries,
        [PlanetName.Mercury] = ZodiacName.Aries, [PlanetName.Jupiter] = ZodiacName.Virgo, [PlanetName.Venus] = ZodiacName.Aries,
        [PlanetName.Saturn] = ZodiacName.Virgo, [PlanetName.Rahu] = ZodiacName.Cancer, [PlanetName.Ketu] = ZodiacName.Capricornus,
    });

    private static readonly DoshaChart Wife = new(ZodiacName.Gemini, new Dictionary<PlanetName, ZodiacName>
    {
        [PlanetName.Sun] = ZodiacName.Scorpio, [PlanetName.Moon] = ZodiacName.Capricornus, [PlanetName.Mars] = ZodiacName.Virgo,
        [PlanetName.Mercury] = ZodiacName.Sagittarius, [PlanetName.Jupiter] = ZodiacName.Scorpio, [PlanetName.Venus] = ZodiacName.Libra,
        [PlanetName.Saturn] = ZodiacName.Libra, [PlanetName.Rahu] = ZodiacName.Taurus, [PlanetName.Ketu] = ZodiacName.Scorpio,
    });

    [Fact]
    public void Mars_is_counted_from_lagna_moon_and_venus_for_the_manglik_test()
    {
        var husband = DoshaSamyaCalculator.Read(Husband);
        Assert.Equal(1, husband.MarsHouses[DoshaReference.Lagna]);
        Assert.Equal(6, husband.MarsHouses[DoshaReference.Moon]);
        Assert.Equal(1, husband.MarsHouses[DoshaReference.Venus]);
        Assert.True(husband.ClassicalKuja);

        var wife = DoshaSamyaCalculator.Read(Wife);
        Assert.Equal(4, wife.MarsHouses[DoshaReference.Lagna]);
        Assert.Equal(9, wife.MarsHouses[DoshaReference.Moon]);
        Assert.Equal(12, wife.MarsHouses[DoshaReference.Venus]);
        Assert.True(wife.ClassicalKuja);
    }

    [Fact]
    public void Only_the_wife_has_a_severe_affliction_rahu_in_the_8th_from_venus()
    {
        Assert.Empty(DoshaSamyaCalculator.Read(Husband).Severe);
        var severe = DoshaSamyaCalculator.Read(Wife).Severe.Single();
        Assert.Equal((PlanetName.Rahu, DoshaReference.Venus, 8), Key(severe));

        var balance = DoshaSamyaCalculator.Balance(DoshaSamyaCalculator.Read(Husband), DoshaSamyaCalculator.Read(Wife));
        Assert.Equal(DoshaBalanceStatus.NotBalanced, balance.Status);
    }
}
