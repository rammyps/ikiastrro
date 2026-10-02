using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Web.Components.LifeMatters;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>PVR step 5 influences on a target sign. Fixture: Lagna Aries, target Cancer (the 4th);
/// Cancer is movable, so its bādhaka sthāna is the 11th from it (Taurus) and Venus the bādhaka.</summary>
public class TargetInfluencesTests
{
    private static readonly Dictionary<PlanetName, ZodiacName> Chart = new()
    {
        [PlanetName.Sun] = ZodiacName.Taurus,        // 11th from Cancer, in the bādhaka sthāna
        [PlanetName.Moon] = ZodiacName.Taurus,       // lord, 11th, rāśi dṛṣṭi (fixed → movable)
        [PlanetName.Mars] = ZodiacName.Capricornus,  // 7th: graha dṛṣṭi, quadrant
        [PlanetName.Mercury] = ZodiacName.Leo,       // 2nd: no class; Leo is the sign behind Cancer, so no rāśi dṛṣṭi
        [PlanetName.Jupiter] = ZodiacName.Scorpio,   // 5th: trine, 9th aspect lands on Cancer, rāśi dṛṣṭi
        [PlanetName.Venus] = ZodiacName.Sagittarius, // 6th: upachaya + dusthāna, and the bādhaka
        [PlanetName.Saturn] = ZodiacName.Cancer,     // occupies: quadrant + trine
        [PlanetName.Rahu] = ZodiacName.Aquarius,     // 8th: dusthāna
        [PlanetName.Ketu] = ZodiacName.Leo,          // 2nd
    };

    private static TargetInfluenceReading Read() =>
        TargetInfluences.Read(ZodiacName.Aries, Chart, ZodiacName.Cancer, karakas: [PlanetName.Jupiter],
            argalaPlanets: [PlanetName.Mercury], virodhargalaPlanets: [PlanetName.Venus]);

    private static PlanetInfluence Row(PlanetName p) => Read().Planets.Single(r => r.Planet == p);

    [Fact]
    public void Lord_and_baadhaka_are_read_off_the_target()
    {
        var r = Read();
        Assert.Equal(PlanetName.Moon, r.Lord);
        Assert.Equal(11, r.LordHouseFromTarget);
        Assert.Equal(TargetPosition.Upachaya, r.LordPosition);
        Assert.Equal(ZodiacName.Taurus, r.Baadhaka.SthaanaSign);
        Assert.Equal("Venus", r.Baadhaka.Baadhaka);
    }

    [Fact]
    public void Lists_lord_then_karakas_then_planets_touching_the_target()
    {
        var order = Read().Planets.Select(p => p.Planet).ToList();
        Assert.Equal(9, order.Count);
        Assert.Equal([PlanetName.Moon, PlanetName.Jupiter], order.Take(2));
        Assert.Equal(PlanetName.Ketu, order[^1]); // the only planet that neither is a role nor touches Cancer
    }

    [Fact]
    public void Links_occupation_graha_and_rasi_drishti_and_argala()
    {
        Assert.Equal(InfluenceLink.Occupies, Row(PlanetName.Saturn).Links);
        Assert.Equal(InfluenceLink.GrahaDrishti, Row(PlanetName.Mars).Links);
        Assert.Equal(InfluenceLink.GrahaDrishti | InfluenceLink.RasiDrishti, Row(PlanetName.Jupiter).Links);
        Assert.Equal(InfluenceLink.RasiDrishti, Row(PlanetName.Moon).Links);
        Assert.Equal(InfluenceLink.Argala, Row(PlanetName.Mercury).Links);
        Assert.Equal(InfluenceLink.Virodhargala, Row(PlanetName.Venus).Links);
        Assert.False(Row(PlanetName.Ketu).Touches);
    }

    [Fact]
    public void Leans_follow_position_from_the_target_and_the_baadhaka()
    {
        Assert.Equal(InfluenceLean.Supports, Row(PlanetName.Saturn).Lean);   // quadrant + trine
        Assert.Equal(InfluenceLean.Supports, Row(PlanetName.Mars).Lean);     // quadrant
        Assert.Equal(InfluenceLean.Obstructs, Row(PlanetName.Rahu).Lean);    // 8th
        Assert.Equal(InfluenceLean.Mixed, Row(PlanetName.Venus).Lean);       // 6th, and the bādhaka
        Assert.Equal(InfluenceLean.Mixed, Row(PlanetName.Sun).Lean);         // 11th, but in the bādhaka sthāna
        Assert.Equal(InfluenceLean.Neutral, Row(PlanetName.Ketu).Lean);      // 2nd
        Assert.True(Row(PlanetName.Venus).IsBaadhaka);
        Assert.True(Row(PlanetName.Sun).InBaadhakaSthaana);
    }

    [Fact]
    public void Functional_nature_is_reported_for_the_seven_planets_only()
    {
        Assert.Null(Row(PlanetName.Rahu).Functional);
        Assert.Equal(LagnaFunctionalNature.For(ZodiacName.Aries, PlanetName.Jupiter).Nature, Row(PlanetName.Jupiter).Functional);
        Assert.True(Row(PlanetName.Rahu).IsNaturalMalefic);
        Assert.False(Row(PlanetName.Jupiter).IsNaturalMalefic);
    }

    [Theory]
    [InlineData(1, TargetPosition.Quadrant | TargetPosition.Trine)]
    [InlineData(2, TargetPosition.None)]
    [InlineData(6, TargetPosition.Upachaya | TargetPosition.Dusthana)]
    [InlineData(10, TargetPosition.Quadrant | TargetPosition.Upachaya)]
    [InlineData(12, TargetPosition.Dusthana)]
    public void Position_classes_overlap_where_the_texts_overlap(int house, TargetPosition expected) =>
        Assert.Equal(expected, TargetInfluences.PositionOf(house));

    [Theory]
    [InlineData(ZodiacName.Taurus, ZodiacName.Gemini, ZodiacName.Cancer)] // 2 signs on from the lord
    [InlineData(ZodiacName.Aries, ZodiacName.Cancer, ZodiacName.Cancer)]  // lands in the 7th → 10th from it
    [InlineData(ZodiacName.Aries, ZodiacName.Aries, ZodiacName.Capricornus)] // lord in the house → 10th
    public void PadaOf_follows_the_arudha_rule_and_its_exception(ZodiacName house, ZodiacName lordSign, ZodiacName expected) =>
        Assert.Equal(expected, ArudhaCalculator.PadaOf(house, lordSign));

    [Fact]
    public void Answer_card_names_acting_planets_only_with_the_baadhaka_first_among_hinderers()
    {
        var helpers = TargetInfluenceText.Helpers(Read());
        var hinderers = TargetInfluenceText.Hinderers(Read());
        Assert.Equal(TargetInfluenceText.Named, helpers.Count);
        Assert.All(helpers, h => Assert.True(h.Lean == InfluenceLean.Supports && (h.Touches || h.IsLord)));
        Assert.Equal(PlanetName.Venus, hinderers[0].Planet);
        Assert.Equal("its bādhaka, blocks an intervention, 6th from it (grows, obstacles)", TargetInfluenceText.Why(hinderers[0]));
    }

    [Fact]
    public void An_occupant_is_described_as_sitting_in_it_without_a_position() =>
        Assert.Equal("sits in it", TargetInfluenceText.Why(Row(PlanetName.Saturn)));
}
