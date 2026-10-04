using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Core.LifeMatters.Promise;
using Ikiastrro.Core.Models;
using Ikiastrro.Data;
using Ikiastrro.Data.Statistics;

namespace Ikiastrro.Web.Tests;

/// <summary>The adapter that turns a life matter and a person's persisted D1 facts into the promise
/// engine's input (docs/architecture/key_inference_promise.md §10). Aries Lagna, career at the 10th.</summary>
public sealed class LifeMatterPromiseAdapterTests
{
    private static ChartKeyDetail G(string planet, string sign, string? dignity = null, bool combust = false,
        string? star = null, string? chara = null) =>
        new() { Planet = planet, Sign = sign, PointKind = "Graha", DignityStatus = dignity, IsCombust = combust,
                NakshatraLordPlanet = star, CharaKaraka = chara };

    private static List<ChartKeyDetail> Grahas() =>
    [
        G("Ascendant", "Aries"),
        G("Sun", "Aries", "Exalted", chara: "AK"), G("Moon", "Virgo"), G("Mercury", "Gemini"), G("Jupiter", "Gemini"),
        G("Venus", "Pisces"), G("Mars", "Pisces", combust: true, star: "Saturn"), G("Saturn", "Libra", "Exalted"),
        G("Rahu", "Gemini"), G("Ketu", "Gemini"),
    ];

    private static ShadbalaSummaryRow Sb(string planet, decimal? percent) =>
        new(planet, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, percent);

    private static List<ShadbalaSummaryRow> Shadbala() =>
        [Sb("Sun", 130), Sb("Moon", 90), Sb("Mars", 40), Sb("Mercury", 100), Sb("Jupiter", 85), Sb("Venus", 120), Sb("Saturn", 105)];

    private static LifeMatterStepRow Step(string code = "CAREER_STATUS_07") =>
        new(7, code, "CAREER_STATUS", "Career and status", 7, "Career and actions", "D10", "10th", "Sun, Saturn", "PVR", null);

    private static ResolvedLifeMatterFocus Focus(IEnumerable<(int House, string Ref, int Priority)> houses, params string[] karakas) =>
        new(7, null,
            karakas.Select((k, i) => new LifeMatterKarakaRule(i + 1, 1, 7, 1, k, i + 1)).ToList(),
            houses.Select((h, i) => new LifeMatterFocusRule(i + 1, 1, 7, LifeMatterFocusKind.House, h.Ref, h.House, null, h.Priority)).ToList());

    private static LifeMatterStatistics Stats(List<ShadbalaSummaryRow>? shadbala = null) =>
        new("D1", "Aries", [], [], shadbala ?? Shadbala(), []);

    [Theory]
    [InlineData(StrengthBand.Strong, Capacity.Strong)]
    [InlineData(StrengthBand.Middle, Capacity.Moderate)]
    [InlineData(StrengthBand.Weak, Capacity.Weak)]
    [InlineData(StrengthBand.None, Capacity.Unknown)]
    public void Strength_bands_map_to_capacity(StrengthBand band, Capacity expected) =>
        Assert.Equal(expected, LifeMatterPromiseAdapter.ToCapacity(band));

    [Fact]
    public void Planet_facts_carry_dignity_combustion_nakshatra_lord_and_the_shadbala_band()
    {
        var facts = LifeMatterPromiseAdapter.PlanetFacts(Grahas().Where(g => g.Planet != "Ascendant"), Shadbala());
        Assert.Equal(9, facts.Count);
        Assert.Equal("Exalted", facts[PlanetName.Sun].Dignity);
        Assert.Equal(Capacity.Strong, facts[PlanetName.Sun].Capacity);      // 130 % of the minimum
        Assert.Equal(Capacity.Weak, facts[PlanetName.Mars].Capacity);       // 40 %
        Assert.True(facts[PlanetName.Mars].IsCombust);
        Assert.Equal(PlanetName.Saturn, facts[PlanetName.Mars].NakshatraLord);
        Assert.Equal(Capacity.Unknown, facts[PlanetName.Rahu].Capacity);    // the nodes have no Shadbala
        Assert.Equal(ZodiacName.Libra, facts[PlanetName.Saturn].Sign);
    }

    [Fact]
    public void Karakas_resolve_a_fixed_graha_and_a_chara_karaka_role()
    {
        var focus = Focus([(10, "LAGNA", 1)], "GRAHA_SATURN", "KARAKA_AK", "GRAHA_NOBODY");
        var planets = LifeMatterPromiseAdapter.KarakaPlanets(Grahas(), focus);
        Assert.Equal([PlanetName.Saturn, PlanetName.Sun], planets);   // AK is the Sun here; an unknown code is dropped
    }

    [Fact]
    public void Argala_planets_are_those_that_hold_or_are_contested_and_every_obstructor()
    {
        var summary = new ArgalaSummary(
        [
            new ArgalaPair(2, 12, "Primary", ["Jupiter"], ["Mars"], false, ArgalaVerdict.Obstructed),
            new ArgalaPair(4, 10, "Primary", ["Venus"], ["Saturn"], false, ArgalaVerdict.Holds),
            new ArgalaPair(11, 3, "Primary", ["Mercury"], ["Moon"], false, ArgalaVerdict.Contested),
        ]);
        Assert.Equal([PlanetName.Venus, PlanetName.Mercury], LifeMatterPromiseAdapter.ArgalaPlanets(summary, held: true));
        Assert.Equal([PlanetName.Mars, PlanetName.Saturn, PlanetName.Moon], LifeMatterPromiseAdapter.ArgalaPlanets(summary, held: false));
    }

    [Fact]
    public void A_matter_is_read_at_each_of_its_lagna_houses_in_focus_order()
    {
        var focus = Focus([(10, "LAGNA", 2), (3, "LAGNA", 1), (6, "CHANDRA_LAGNA", 1)], "GRAHA_SUN");
        var reading = LifeMatterPromiseAdapter.Read(Step(), focus, Grahas(), "Aries", Stats(), Shadbala());

        Assert.Null(reading.Note);
        Assert.Equal([3, 10], reading.Targets.Select(t => t.House));            // priority first; the Moon-lagna house is not a Lagna target
        Assert.Equal(["3rd house", "10th house"], reading.Targets.Select(t => t.Label));
        Assert.Equal(ZodiacName.Gemini, reading.Targets[0].Sign);
        Assert.Equal(ZodiacName.Capricornus, reading.Targets[1].Sign);
        Assert.Equal(reading.Targets[0], reading.Primary);
        Assert.All(reading.Targets, t => Assert.Equal("D1", t.Promise.D1.Testimonies[0].Chart));
        Assert.Equal("CAREER_STATUS_07", reading.Targets[0].Promise.MatterCode);
    }

    [Fact]
    public void The_engine_gets_the_chart_it_was_given()
    {
        // Career at the 10th: Saturn (lord) exalted in Libra, the Sun karaka exalted in Aries — the engine's confirmed-promise chart.
        var focus = Focus([(10, "LAGNA", 1)], "GRAHA_SUN");
        var reading = LifeMatterPromiseAdapter.Read(Step(), focus, Grahas(), "Aries", Stats(), Shadbala());
        var promise = reading.Primary!.Promise;

        var lord = promise.D1.Testimonies.First(t => t.Family == PromiseFamilies.LordshipPlacement(PlanetName.Saturn));
        Assert.Equal(Direction.Supportive, lord.Direction);
        Assert.Equal(Capacity.Strong, lord.Capacity);                          // 105 % of the minimum
        Assert.Contains(promise.D1.Testimonies, t => t.Family == PromiseFamilies.LordshipPlacement(PlanetName.Sun));
        Assert.Empty(promise.MissingEvidence);
    }

    [Fact]
    public void A_matter_with_no_lagna_house_has_no_target_and_says_why()
    {
        var focus = Focus([(10, "ARUDHA_LAGNA", 1)], "GRAHA_SUN");
        var reading = LifeMatterPromiseAdapter.Read(Step("CAREER_STATUS_13"), focus, Grahas(), "Aries", Stats(), Shadbala());
        Assert.Empty(reading.Targets);
        Assert.Null(reading.Primary);
        Assert.Contains("No Lagna house focus", reading.Note);
    }

    [Fact]
    public void A_chart_missing_a_planet_is_flagged_not_guessed()
    {
        var grahas = Grahas().Where(g => g.Planet != "Saturn").ToList();      // Saturn is the 10th's lord
        var reading = LifeMatterPromiseAdapter.Read(Step(), Focus([(10, "LAGNA", 1)], "GRAHA_SUN"), grahas, "Aries", Stats(), Shadbala());
        var promise = reading.Primary!.Promise;
        Assert.Equal(PromiseVerdict.Indeterminate, promise.Verdict);
        Assert.Contains(promise.MissingEvidence, m => m.Contains("Saturn"));
    }
}
