using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Yoga;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

public class YogaPlanetResolverTests
{
    private static readonly Dictionary<int, PlanetName> Lords = new()
    {
        [1] = PlanetName.Mars, [2] = PlanetName.Venus, [5] = PlanetName.Sun, [7] = PlanetName.Venus,
        [11] = PlanetName.Saturn,
    };

    private static IReadOnlyList<PlanetName> Resolve(string code, string? rule,
        Dictionary<PlanetName, YogaPlacement>? placements = null)
        => YogaPlanetResolver.Resolve(code, rule, Lords, placements ?? new());

    [Fact]
    public void Names_in_the_rule_are_planets()
        => Assert.Equal(new[] { PlanetName.Sun, PlanetName.Moon, PlanetName.Mars },
            Resolve("YOGA_THRILOCHANA", "Sun, Moon & Mars in mutual trine"));

    [Fact]
    public void House_lords_resolve_to_this_charts_planet()
        => Assert.Equal(new[] { PlanetName.Mars, PlanetName.Venus },
            Resolve("YOGA_AYATNA_DHANA_LABHA", "Lagna lord and 2nd lord exchange houses"));

    [Fact]
    public void A_list_of_lords_resolves_each()
        => Assert.Equal(new[] { PlanetName.Mars, PlanetName.Venus, PlanetName.Saturn },
            Resolve("YOGA_BAHUDRAVYARJANA", "Lagna, 2nd & 11th lords in a 3-way mutual house exchange"));

    [Fact]
    public void Lords_of_a_list_resolve_and_mix_with_named_planets()
        => Assert.Equal(new[] { PlanetName.Sun, PlanetName.Mars, PlanetName.Jupiter, PlanetName.Venus },
            Resolve("YOGA_ANAPATHYA", "Jupiter and the lords of Lagna, 5th and 7th are all weak"));

    [Fact]
    public void Parenthetical_exclusions_are_not_planets()
        => Assert.Equal(new[] { PlanetName.Moon },
            Resolve("YOGA_X", "A planet (not Sun) in the 12th from Moon"));

    [Fact]
    public void All_seven_grahas_rule_names_the_classical_seven()
        => Assert.Equal(7, Resolve("YOGA_KUTA", "All 7 classical grahas fill houses 4-10").Count);

    [Fact]
    public void Sun_based_occupancy_lists_planets_standing_there_and_skips_the_moon()
    {
        var placements = new Dictionary<PlanetName, YogaPlacement>
        {
            [PlanetName.Mars] = new(3, 2, 5),
            [PlanetName.Moon] = new(4, 2, 1),
            [PlanetName.Venus] = new(9, 6, 8),
        };
        Assert.Equal(new[] { PlanetName.Mars }, Resolve("YOGA_VESI", "A planet (not Moon) in the 2nd from Sun", placements));
    }

    [Fact]
    public void Adhi_lists_the_benefics_in_6_7_8_from_the_moon_and_the_moon()
    {
        var placements = new Dictionary<PlanetName, YogaPlacement>
        {
            [PlanetName.Jupiter] = new(9, 6, 7),
            [PlanetName.Mercury] = new(2, 6, 3),
        };
        Assert.Equal(new[] { PlanetName.Moon, PlanetName.Jupiter },
            Resolve("YOGA_ADHI", "Jupiter, Mercury or Venus in the 6th, 7th or 8th from Moon", placements));
    }

    [Fact]
    public void No_rule_means_no_planets()
        => Assert.Empty(Resolve("YOGA_X", null));
}
