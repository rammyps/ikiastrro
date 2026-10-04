using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Engines.Transits;

namespace Ikiastrro.Yoga.Tests;

/// <summary>Natal promise and dasha links for a slow planet's transit
/// (docs/architecture/transit_gochara_inference.md §9). Aries Lagna: Mars 1/8, Venus 2/7, Mercury 3/6,
/// Moon 4, Sun 5, Jupiter 9/12, Saturn 10/11.</summary>
public sealed class GocharaQualifierTests
{
    private static readonly Dictionary<int, PlanetName> Lords = new()
    {
        [1] = PlanetName.Mars, [2] = PlanetName.Venus, [3] = PlanetName.Mercury, [4] = PlanetName.Moon,
        [5] = PlanetName.Sun, [6] = PlanetName.Mercury, [7] = PlanetName.Venus, [8] = PlanetName.Mars,
        [9] = PlanetName.Jupiter, [10] = PlanetName.Saturn, [11] = PlanetName.Saturn, [12] = PlanetName.Jupiter,
    };

    private static GocharaNatalContext Natal(
        StrengthTier jupiter = StrengthTier.Strong, StrengthTier house12 = StrengthTier.Moderate) => new(
        ZodiacName.Aries,
        Lords,
        new Dictionary<PlanetName, ZodiacName>
        {
            [PlanetName.Moon] = ZodiacName.Pisces,
            [PlanetName.Saturn] = ZodiacName.Capricornus,
            [PlanetName.Jupiter] = ZodiacName.Leo,
            [PlanetName.Rahu] = ZodiacName.Gemini,
        },
        new Dictionary<PlanetName, StrengthTier>
        {
            [PlanetName.Jupiter] = jupiter, [PlanetName.Saturn] = StrengthTier.Weak, [PlanetName.Moon] = StrengthTier.Moderate,
        },
        new Dictionary<int, StrengthTier> { [12] = house12 });

    [Theory]
    [InlineData(StrengthTier.Strong, StrengthTier.Strong, GocharaPromise.Promised)]
    [InlineData(StrengthTier.Moderate, StrengthTier.Moderate, GocharaPromise.Promised)]
    [InlineData(StrengthTier.Strong, StrengthTier.Weak, GocharaPromise.Mixed)]
    [InlineData(StrengthTier.Weak, StrengthTier.Moderate, GocharaPromise.Mixed)]
    [InlineData(StrengthTier.Weak, StrengthTier.Weak, GocharaPromise.Weak)]
    [InlineData(StrengthTier.Weak, StrengthTier.None, GocharaPromise.Weak)]   // only the known one counts
    [InlineData(StrengthTier.Strong, StrengthTier.None, GocharaPromise.Promised)]
    [InlineData(StrengthTier.None, StrengthTier.None, GocharaPromise.Unknown)]
    public void Promise_needs_the_lord_and_the_house_not_both_weak(StrengthTier lord, StrengthTier house, GocharaPromise expected) =>
        Assert.Equal(expected, GocharaQualifierBuilder.PromiseOf(lord, house));

    [Fact]
    public void Transit_house_lord_occupants_and_the_planets_own_role_are_read_from_the_natal_chart()
    {
        var q = GocharaQualifierBuilder.Qualify(PlanetName.Saturn, ZodiacName.Pisces, Natal(), new GocharaDashaLords(null, null, null));
        Assert.Equal(12, q.HouseFromLagna);
        Assert.Equal(PlanetName.Jupiter, q.HouseLord);
        Assert.Equal([PlanetName.Moon], q.NatalOccupants);
        Assert.Equal([10, 11], q.PlanetLordships);
        Assert.Equal(10, q.PlanetNatalHouse);
        Assert.Equal(StrengthTier.Weak, q.PlanetStrength);
        Assert.Equal(GocharaPromise.Promised, q.Promise); // Jupiter strong, house 12 moderate
        Assert.Contains("12th house (Pisces)", q.Lines[0]);
    }

    [Fact]
    public void A_planet_is_never_its_own_natal_occupant()
    {
        var q = GocharaQualifierBuilder.Qualify(PlanetName.Saturn, ZodiacName.Capricornus, Natal(), new GocharaDashaLords(null, null, null));
        Assert.DoesNotContain(PlanetName.Saturn, q.NatalOccupants);
    }

    [Fact]
    public void Dasha_links_cover_the_planet_itself_the_house_lord_and_lords_in_the_sign()
    {
        var q = GocharaQualifierBuilder.Qualify(PlanetName.Saturn, ZodiacName.Pisces, Natal(),
            new GocharaDashaLords(Maha: PlanetName.Jupiter, Antar: PlanetName.Saturn, Pratyantar: PlanetName.Moon));
        Assert.True(q.Foreground);
        Assert.Contains(new DashaLink("Mahādaśā", PlanetName.Jupiter, DashaLinkKind.LordRulesHouse), q.DashaLinks);
        Assert.Contains(new DashaLink("Antardaśā", PlanetName.Saturn, DashaLinkKind.TransitPlanetIsLord), q.DashaLinks);
        Assert.Contains(new DashaLink("Pratyantardaśā", PlanetName.Moon, DashaLinkKind.LordOccupiesSign), q.DashaLinks);
        Assert.Equal(3, q.DashaLinks.Count);
    }

    [Fact]
    public void An_unlinked_transit_is_background_not_unimportant()
    {
        var q = GocharaQualifierBuilder.Qualify(PlanetName.Saturn, ZodiacName.Pisces, Natal(),
            new GocharaDashaLords(PlanetName.Venus, PlanetName.Mars, PlanetName.Mercury));
        Assert.False(q.Foreground);
        Assert.Contains("background to the dasha", q.Lines[^1]);
    }

    [Fact]
    public void No_dasha_selected_reads_no_link()
    {
        var q = GocharaQualifierBuilder.Qualify(PlanetName.Rahu, ZodiacName.Pisces, Natal(), new GocharaDashaLords(null, null, null));
        Assert.False(q.Foreground);
        Assert.Contains("No dasha selected", q.Lines[^1]);
        Assert.Empty(q.PlanetLordships);                  // Rāhu rules no house
        Assert.Contains("rules no house", q.Lines[2]);
    }

    [Fact]
    public void Weak_lord_and_house_show_as_weakly_promised()
    {
        var q = GocharaQualifierBuilder.Qualify(PlanetName.Saturn, ZodiacName.Pisces,
            Natal(jupiter: StrengthTier.Weak, house12: StrengthTier.Weak), new GocharaDashaLords(null, null, null));
        Assert.Equal(GocharaPromise.Weak, q.Promise);
        Assert.Contains("weakly promised", q.Lines[1]);
    }

    [Fact]
    public void Inference_carries_qualifiers_and_a_dasha_headline_for_slow_planets_only()
    {
        var rules = new GocharaVedhaRule[] { new(PlanetName.Saturn, 11, 5, PlanetName.Sun) };
        var transits = new Dictionary<PlanetName, ZodiacName>
        {
            [PlanetName.Saturn] = ZodiacName.Pisces, [PlanetName.Jupiter] = ZodiacName.Cancer, [PlanetName.Sun] = ZodiacName.Aries,
        };
        var reading = GocharaReading.Read(ZodiacName.Pisces, transits, rules);
        var inf = GocharaInferenceBuilder.Build(reading, new Dictionary<PlanetName, GocharaIngress>(), rules, Natal(),
            new GocharaDashaLords(PlanetName.Venus, PlanetName.Saturn, null));
        Assert.Equal([PlanetName.Saturn, PlanetName.Jupiter], inf.PlanetQualifiers.Select(q => q.Planet));
        Assert.Equal("Running dasha: Venus / Saturn.", inf.Qualifiers[0]);
        Assert.Equal("Linked to the dasha: Saturn — read these first.", inf.Qualifiers[1]);
    }

    [Fact]
    public void Headline_says_so_when_every_slow_planet_is_linked_or_none_is()
    {
        var rules = new GocharaVedhaRule[] { new(PlanetName.Saturn, 11, 5, PlanetName.Sun) };
        var reading = GocharaReading.Read(ZodiacName.Pisces,
            new Dictionary<PlanetName, ZodiacName> { [PlanetName.Saturn] = ZodiacName.Pisces }, rules);
        GocharaInference Build(GocharaDashaLords d) => GocharaInferenceBuilder.Build(
            reading, new Dictionary<PlanetName, GocharaIngress>(), rules, Natal(), d);
        Assert.Equal("Every slow planet touches the running dasha lords, so weigh them by tier and house promise.",
            Build(new GocharaDashaLords(PlanetName.Saturn, null, null)).Qualifiers[1]);
        Assert.StartsWith("No slow planet touches", Build(new GocharaDashaLords(PlanetName.Venus, null, null)).Qualifiers[1]);
    }

    [Fact]
    public void Without_natal_facts_nothing_is_qualified()
    {
        var reading = GocharaReading.Read(ZodiacName.Pisces, new Dictionary<PlanetName, ZodiacName> { [PlanetName.Saturn] = ZodiacName.Pisces }, []);
        var inf = GocharaInferenceBuilder.Build(reading, new Dictionary<PlanetName, GocharaIngress>(), []);
        Assert.Empty(inf.PlanetQualifiers);
        Assert.Empty(inf.Qualifiers);
    }
}
