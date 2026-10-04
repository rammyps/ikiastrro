using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.LifeMatters.Promise;

namespace Ikiastrro.Yoga.Tests;

/// <summary>Phase 2 of docs/architecture/key_inference_promise.md on synthetic charts. Aries Lagna, the
/// career matter read at the 10th (Capricorn, lord Saturn, karaka Sun). Filler planets sit in dual signs
/// (Gemini, Virgo, Pisces): a dual sign's rāśi dṛṣṭi reaches only dual signs, and none of them aspects
/// Capricorn by graha dṛṣṭi, so unless a test places a planet on purpose nothing acts on the target.</summary>
public sealed class D1PromiseEngineTests
{
    private static PlanetFact F(ZodiacName sign, string? dignity = null, Capacity cap = Capacity.Strong,
        bool combust = false, PlanetName? star = null) => new(sign, dignity, combust, cap, star);

    private static Dictionary<PlanetName, PlanetFact> Chart(PlanetFact? sun = null, PlanetFact? saturn = null,
        params (PlanetName Planet, PlanetFact Fact)[] overrides)
    {
        var d = new Dictionary<PlanetName, PlanetFact>
        {
            [PlanetName.Sun] = sun ?? F(ZodiacName.Aries, "Exalted"),
            [PlanetName.Saturn] = saturn ?? F(ZodiacName.Libra, "Exalted"),
            [PlanetName.Moon] = F(ZodiacName.Virgo),
            [PlanetName.Mercury] = F(ZodiacName.Gemini),
            [PlanetName.Jupiter] = F(ZodiacName.Gemini),
            [PlanetName.Venus] = F(ZodiacName.Pisces),
            [PlanetName.Mars] = F(ZodiacName.Pisces),
            [PlanetName.Rahu] = F(ZodiacName.Gemini, cap: Capacity.Unknown),
            [PlanetName.Ketu] = F(ZodiacName.Gemini, cap: Capacity.Unknown),
        };
        foreach (var (p, f) in overrides) d[p] = f;
        return d;
    }

    private static MatterPromiseInput Career(IReadOnlyDictionary<PlanetName, PlanetFact> planets,
        IReadOnlyList<PlanetName>? karakas = null, Capacity target = Capacity.Strong, Capacity sav = Capacity.Unknown,
        Capacity bav = Capacity.Unknown, IReadOnlyList<PlanetName>? argala = null,
        IReadOnlyList<PlanetName>? virodha = null, IReadOnlyList<YogaFact>? yogas = null) =>
        new("CAREER", ZodiacName.Aries, "10th house", ZodiacName.Capricornus, planets,
            karakas ?? [PlanetName.Sun], target, sav, bav, argala, virodha, yogas);

    // ------------------------------------------------------------------ vocabulary

    [Theory]
    [InlineData(1, Direction.Supportive)] [InlineData(2, Direction.Neutral)] [InlineData(3, Direction.Supportive)]
    [InlineData(4, Direction.Supportive)] [InlineData(5, Direction.Supportive)] [InlineData(6, Direction.Mixed)]
    [InlineData(7, Direction.Supportive)] [InlineData(8, Direction.Obstructive)] [InlineData(9, Direction.Supportive)]
    [InlineData(10, Direction.Supportive)] [InlineData(11, Direction.Supportive)] [InlineData(12, Direction.Obstructive)]
    public void Placement_from_the_target_follows_PVR_step_5(int house, Direction expected) =>
        Assert.Equal(expected, D1PromiseEngine.PlacementDirection(house));

    [Theory]
    [InlineData("Exalted", Direction.Supportive)] [InlineData("Own Sign", Direction.Supportive)]
    [InlineData("Friend", Direction.Supportive)] [InlineData("Neutral", Direction.Neutral)]
    [InlineData("Enemy", Direction.Obstructive)] [InlineData("Debilitated", Direction.Obstructive)]
    [InlineData(null, Direction.Neutral)]
    public void Dignity_gives_a_direction(string? dignity, Direction expected) =>
        Assert.Equal(expected, D1PromiseEngine.DignityDirection(dignity));

    [Fact]
    public void Families_resolve_conflict_to_Mixed_and_ignore_Neutral()
    {
        Assert.Equal(Direction.Supportive, PromiseFamilies.Resolve([Direction.Supportive, Direction.Neutral]));
        Assert.Equal(Direction.Mixed, PromiseFamilies.Resolve([Direction.Supportive, Direction.Obstructive]));
        Assert.Equal(Direction.Mixed, PromiseFamilies.Resolve([Direction.Mixed, Direction.Supportive]));
        Assert.Equal(Direction.Neutral, PromiseFamilies.Resolve([Direction.Neutral]));
        Assert.Equal(Capacity.Weak, PromiseFamilies.Weakest([Capacity.Strong, Capacity.Weak, Capacity.Unknown]));
        Assert.Equal(Capacity.Unknown, PromiseFamilies.Weakest([Capacity.Unknown]));
    }

    // ------------------------------------------------------------------ verdicts

    [Fact]
    public void Confirmed_promise_is_strong_positive()
    {
        // Exalted Saturn in Libra (10th from the target), exalted Sun in Aries (4th): four supportive families.
        var p = D1PromiseEngine.Read(Career(Chart()));
        Assert.Equal(PromiseVerdict.StrongPositive, p.Verdict);
        Assert.Empty(p.Negative);
        Assert.Empty(p.MissingEvidence);
        Assert.Equal(Confidence.Medium, p.Confidence); // cannot be High until a varga confirms
        Assert.Contains("Saturn", p.DominantSupport);
        Assert.Equal("", p.DominantObstruction);
    }

    [Fact]
    public void Damaged_promise_stays_positive_but_conditional()
    {
        // Saturn's placement and dignity are as good as before, but it is combust and weak: the promise
        // is present, its capacity is not.
        var chart = Chart(saturn: F(ZodiacName.Libra, "Exalted", Capacity.Weak, combust: true));
        var p = D1PromiseEngine.Read(Career(chart));
        Assert.Equal(PromiseVerdict.PositiveConditional, p.Verdict);
        var saturn = p.D1.Testimonies.Where(t => t.Subject == "Saturn" && t.Role == TestimonyRole.Lord).ToList();
        Assert.All(saturn, t => Assert.Equal(Direction.Supportive, t.Direction));
        Assert.All(saturn, t => Assert.Equal(Capacity.Weak, t.Capacity));
        Assert.Contains("combust", saturn[0].Explanation);
    }

    [Fact]
    public void Combustion_lowers_capacity_one_step_and_never_flips_direction()
    {
        var p = D1PromiseEngine.Read(Career(Chart(saturn: F(ZodiacName.Libra, "Exalted", Capacity.Strong, combust: true))));
        var t = p.D1.Testimonies.First(x => x.Family == PromiseFamilies.LordshipPlacement(PlanetName.Saturn));
        Assert.Equal(Capacity.Moderate, t.Capacity);
        Assert.Equal(Direction.Supportive, t.Direction);
    }

    [Fact]
    public void A_strong_malefic_placement_is_strongly_obstructive_not_strongly_positive()
    {
        // Saturn, strong, in Leo: the 8th from the target. Strength says how hard it acts, not which way.
        var strongSaturn = D1PromiseEngine.Read(Career(Chart(saturn: F(ZodiacName.Leo, "Enemy", Capacity.Strong))));
        var weakSaturn = D1PromiseEngine.Read(Career(Chart(saturn: F(ZodiacName.Leo, "Enemy", Capacity.Weak))));
        var strong = strongSaturn.D1.Testimonies.First(x => x.Family == PromiseFamilies.LordshipPlacement(PlanetName.Saturn));
        var weak = weakSaturn.D1.Testimonies.First(x => x.Family == PromiseFamilies.LordshipPlacement(PlanetName.Saturn));
        Assert.Equal(Direction.Obstructive, strong.Direction);
        Assert.Equal(Direction.Obstructive, weak.Direction);
        Assert.Equal(Capacity.Strong, strong.Capacity);
        Assert.Equal(Capacity.Weak, weak.Capacity);
    }

    [Fact]
    public void Obstructive_lord_and_karaka_make_an_adverse_promise()
    {
        var chart = Chart(sun: F(ZodiacName.Sagittarius), saturn: F(ZodiacName.Leo, "Enemy", Capacity.Strong));
        var p = D1PromiseEngine.Read(Career(chart));
        Assert.Equal(PromiseVerdict.Adverse, p.Verdict);
        Assert.NotEmpty(p.DominantObstruction);
        Assert.Equal("", p.DominantSupport);
    }

    [Fact]
    public void Promise_and_denial_on_principal_roles_read_mixed_with_a_contradiction()
    {
        // Saturn exalted in the 10th, the Sun karaka in the 12th: they disagree.
        var p = D1PromiseEngine.Read(Career(Chart(sun: F(ZodiacName.Sagittarius))));
        Assert.Equal(PromiseVerdict.Mixed, p.Verdict);
        var c = Assert.Single(p.Contradictions);
        Assert.Equal((TestimonyRole.Lord, TestimonyRole.Karaka), (c.RoleA, c.RoleB));
    }

    [Fact]
    public void Support_that_is_all_weak_cannot_sustain_the_promise()
    {
        var chart = Chart(sun: F(ZodiacName.Aries, "Exalted", Capacity.Weak), saturn: F(ZodiacName.Libra, "Exalted", Capacity.Weak));
        Assert.Equal(PromiseVerdict.WeakLimited, D1PromiseEngine.Read(Career(chart)).Verdict);
    }

    [Fact]
    public void A_missing_lord_is_indeterminate_with_low_confidence()
    {
        var chart = Chart();
        chart.Remove(PlanetName.Saturn);
        var p = D1PromiseEngine.Read(Career(chart));
        Assert.Equal(PromiseVerdict.Indeterminate, p.Verdict);
        Assert.Equal(Confidence.Low, p.Confidence);
        Assert.Contains(p.MissingEvidence, m => m.Contains("Saturn"));
    }

    [Fact]
    public void Evidence_that_says_nothing_either_way_is_indeterminate()
    {
        // Lord in the 2nd from the target (no class), no dignity data, no karaka: nothing directional.
        var chart = Chart(saturn: F(ZodiacName.Aquarius));
        var p = D1PromiseEngine.Read(Career(chart, karakas: []));
        Assert.Equal(PromiseVerdict.Indeterminate, p.Verdict);
        Assert.Equal(Confidence.Low, p.Confidence);
    }

    // ------------------------------------------------------------------ independence

    [Fact]
    public void A_yoga_formed_by_the_lords_conjunction_is_the_same_family_and_counts_once()
    {
        var chart = Chart(saturn: F(ZodiacName.Libra, "Exalted"), overrides: (PlanetName.Rahu, F(ZodiacName.Libra, cap: Capacity.Unknown)));
        var without = D1PromiseEngine.Read(Career(chart));
        var yoga = new YogaFact("Shrapit", Direction.Obstructive, [PlanetName.Saturn, PlanetName.Rahu]);
        var with = D1PromiseEngine.Read(Career(chart, yogas: [yoga]));

        var family = with.D1.Testimonies.Where(t => t.Family == PromiseFamilies.Conjunction(PlanetName.Saturn)).ToList();
        Assert.Equal(2, family.Count);                                   // the conjunction and the yoga, both visible
        Assert.Contains(family, t => t.Role == TestimonyRole.Yoga);
        Assert.DoesNotContain(with.D1.Testimonies, t => t.Family == PromiseFamilies.Yoga("Shrapit"));
        Assert.Equal(without.Verdict, with.Verdict);                     // and the verdict is unchanged by the repeat
    }

    [Fact]
    public void A_lord_that_aspects_the_target_is_not_also_counted_as_an_influence()
    {
        // Saturn in Cancer is 7th from Capricorn: its placement and its aspect are one cause.
        var p = D1PromiseEngine.Read(Career(Chart(saturn: F(ZodiacName.Cancer))));
        Assert.DoesNotContain(p.D1.Testimonies, t => t.Family == PromiseFamilies.Influence(PlanetName.Saturn));
        var placement = p.D1.Testimonies.First(t => t.Family == PromiseFamilies.LordshipPlacement(PlanetName.Saturn));
        Assert.Contains("aspects it", placement.Explanation);
    }

    [Fact]
    public void Ashtakavarga_is_one_family_even_with_two_readings()
    {
        var p = D1PromiseEngine.Read(Career(Chart(), sav: Capacity.Strong, bav: Capacity.Weak));
        var rows = p.D1.Testimonies.Where(t => t.Family == PromiseFamilies.Ashtakavarga).ToList();
        Assert.Equal(2, rows.Count);
        Assert.Equal(Direction.Mixed, PromiseFamilies.Resolve(rows.Select(r => r.Direction)));
        Assert.Equal(PromiseVerdict.StrongPositive, p.Verdict); // a Mixed secondary family neither adds nor removes
    }

    // ------------------------------------------------------------------ influences, argala, refinements

    [Fact]
    public void A_malefic_in_the_target_obstructs()
    {
        var p = D1PromiseEngine.Read(Career(Chart(overrides: (PlanetName.Rahu, F(ZodiacName.Capricornus, cap: Capacity.Unknown)))));
        var rahu = p.D1.Testimonies.Single(t => t.Family == PromiseFamilies.Influence(PlanetName.Rahu));
        Assert.Equal(Direction.Obstructive, rahu.Direction);
        Assert.Equal(TestimonyRole.Influence, rahu.Role);
        Assert.Contains("occupies", rahu.Explanation);
    }

    [Fact]
    public void The_baadhaka_touching_the_target_obstructs()
    {
        // Capricorn is movable: its baadhaka is the lord of Scorpio, Mars, and Scorpio's rāśi dṛṣṭi reaches it.
        var p = D1PromiseEngine.Read(Career(Chart(overrides: (PlanetName.Mars, F(ZodiacName.Scorpio, "Own Sign")))));
        var mars = p.D1.Testimonies.Single(t => t.Family == PromiseFamilies.Influence(PlanetName.Mars));
        Assert.Equal(Direction.Obstructive, mars.Direction);
        Assert.Contains("bādhaka", mars.Explanation);
    }

    [Fact]
    public void Argala_blocked_by_virodhargala_is_neutral_and_unblocked_follows_the_planet_nature()
    {
        var blocked = D1PromiseEngine.Read(Career(Chart(), argala: [PlanetName.Venus], virodha: [PlanetName.Mars]));
        Assert.Equal(Direction.Neutral, blocked.D1.Testimonies.Single(t => t.Family == PromiseFamilies.Intervention).Direction);

        var open = D1PromiseEngine.Read(Career(Chart(), argala: [PlanetName.Jupiter]));
        Assert.Equal(D1PromiseEngine.NatureDirection(ZodiacName.Aries, PlanetName.Jupiter),
            open.D1.Testimonies.Single(t => t.Family == PromiseFamilies.Intervention).Direction);
    }

    [Fact]
    public void Nakshatra_and_dispositor_refine_without_changing_the_verdict()
    {
        var plain = D1PromiseEngine.Read(Career(Chart()));
        var chart = Chart(saturn: F(ZodiacName.Libra, "Exalted", star: PlanetName.Rahu),
            overrides: (PlanetName.Venus, F(ZodiacName.Virgo, "Debilitated")));
        var p = D1PromiseEngine.Read(Career(chart));
        Assert.Contains(p.Refinements, r => r.Contains("dispositor Venus is debilitated"));
        Assert.Contains(p.Refinements, r => r.Contains("nakshatra of Rahu"));
        Assert.Equal(plain.Verdict, p.Verdict);
    }

    [Fact]
    public void Fewer_than_nine_placements_flags_the_influence_step_as_missing()
    {
        var chart = Chart();
        chart.Remove(PlanetName.Ketu);
        var p = D1PromiseEngine.Read(Career(chart));
        Assert.Contains(p.MissingEvidence, m => m.Contains("all nine placements"));
        Assert.Equal(Confidence.Low, p.Confidence);
    }

    // ------------------------------------------------------------------ the sources

    [Fact]
    public void Every_testimony_carries_a_source_chart_and_family()
    {
        var p = D1PromiseEngine.Read(Career(Chart(), sav: Capacity.Strong, argala: [PlanetName.Jupiter]));
        Assert.All(p.D1.Testimonies, t =>
        {
            Assert.StartsWith("SRC_", t.SourceCode);
            Assert.Equal("D1", t.Chart);
            Assert.False(string.IsNullOrWhiteSpace(t.Family));
            Assert.False(string.IsNullOrWhiteSpace(t.Explanation));
        });
    }
}
