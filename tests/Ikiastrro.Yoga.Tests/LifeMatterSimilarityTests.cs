using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>Pins the per-area similarity arithmetic on small hand-built charts: shared facts, weights,
/// the slow-planet discount, the nakshatra dedupe and the tie band.</summary>
public class LifeMatterSimilarityTests
{
    private static DoshaChart Sign(ZodiacName lagna, ZodiacName all) =>
        new(lagna, Enum.GetValues<PlanetName>().ToDictionary(p => p, _ => all));

    private static MatchPerson Moon(ZodiacName sign, int nak, string gana = "Deva", string yoni = "Horse", string nadi = "Vata") =>
        new(sign, nak, gana, yoni, "Male", nadi);

    private static LifeMatterChart Person(DoshaChart d1, MatchPerson moon, int pada = 1) =>
        new(new Dictionary<string, DoshaChart> { ["D1"] = d1 }, moon, pada);

    private static AreaSimilarity Area(IReadOnlyList<AreaSimilarity> r, string code) => r.Single(a => a.Area.Code == code);

    [Fact]
    public void Identical_charts_score_one_in_every_area_that_has_its_varga()
    {
        var p = Person(Sign(ZodiacName.Aries, ZodiacName.Leo), Moon(ZodiacName.Leo, 10));
        var r = LifeMatterSimilarity.Compare(p, p);
        Assert.All(r.Where(a => a.Area.Varga == "D1"), a => Assert.Equal(1.0, a.Score!.Value, 9));
    }

    [Fact]
    public void An_area_whose_varga_is_missing_has_no_score()
    {
        var p = Person(Sign(ZodiacName.Aries, ZodiacName.Leo), Moon(ZodiacName.Leo, 10));
        var r = LifeMatterSimilarity.Compare(p, p);
        Assert.Null(Area(r, "marriage").Score);   // D9 not supplied
        Assert.Equal(SimilarityLean.Unknown, LifeMatterSimilarity.Lean(Area(r, "marriage"), Area(r, "marriage")));
    }

    [Fact]
    public void Health_shares_only_what_matches_and_a_slow_karaka_counts_less()
    {
        // Health reads the 6th sign, 6th lord's sign, Mars sign and Mars house (Mars is fast).
        var a = Person(Sign(ZodiacName.Aries, ZodiacName.Leo), Moon(ZodiacName.Leo, 10));
        var b = Person(Sign(ZodiacName.Taurus, ZodiacName.Leo), Moon(ZodiacName.Leo, 10));
        var h = Area(LifeMatterSimilarity.Compare(a, b), "health");
        // Lagna differs, so the 6th sign (Virgo vs Scorpio) differs; all grahas are in Leo, so the lord's
        // sign and Mars's sign match; Mars's house from Lagna differs (5th vs 4th).
        Assert.Equal(["6th lord in", "Mars sign"], h.Shared.Select(s => s.Key).Order().ToArray());
        Assert.Equal(2 * Math.Log(12), h.SharedWeight, 9);
        Assert.Equal(5 * Math.Log(12), h.TotalWeight, 9);   // 6th sign, 6th lord in, 6th lord house, Mars sign, Mars house

        // Longevity reads Saturn, which is slow: its two facts are weighted 0.25.
        var l = Area(LifeMatterSimilarity.Compare(a, a), "longevity");
        Assert.Equal(Math.Log(12) * (1 + 1 + 0.25), l.TotalWeight, 9);   // same Lagna drops the house facts: 8th sign, 8th lord in (Mars), Saturn sign (+ Saturn house and 8th lord house would be dropped)
    }

    [Fact]
    public void A_lord_in_the_same_house_matches_across_different_lagnas_even_when_the_signs_differ()
    {
        // Aries Lagna: 6th sign Virgo, lord Mercury. Taurus Lagna: 6th sign Libra, lord Venus.
        // Each lord sits in its own 1st house, so "6th lord in the 1st" is shared; the signs (Aries, Taurus) are not.
        DoshaChart With(ZodiacName lagna, PlanetName p, ZodiacName at)
        {
            var d = Sign(lagna, ZodiacName.Leo).Signs.ToDictionary(x => x.Key, x => x.Value);
            d[p] = at;
            return new(lagna, d);
        }
        var a = Person(With(ZodiacName.Aries, PlanetName.Mercury, ZodiacName.Aries), Moon(ZodiacName.Leo, 10));
        var b = Person(With(ZodiacName.Taurus, PlanetName.Venus, ZodiacName.Taurus), Moon(ZodiacName.Leo, 11));
        var h = Area(LifeMatterSimilarity.Compare(a, b), "health");
        Assert.Contains(h.Shared, s => s.Key == "6th lord house" && s.Value == "1");
        Assert.DoesNotContain(h.Shared, s => s.Key == "6th lord in");
    }

    [Fact]
    public void House_facts_are_dropped_when_both_charts_share_a_lagna()
    {
        var a = Person(Sign(ZodiacName.Aries, ZodiacName.Leo), Moon(ZodiacName.Leo, 10));
        var h = Area(LifeMatterSimilarity.Compare(a, a), "health");
        Assert.DoesNotContain(h.Shared, s => s.Key.EndsWith(" house"));
        Assert.Equal(1.0, h.Score!.Value, 9);
    }

    [Fact]
    public void A_shared_nakshatra_replaces_the_moon_sign_gana_yoni_and_nadi_facts()
    {
        var a = Person(Sign(ZodiacName.Aries, ZodiacName.Leo), Moon(ZodiacName.Leo, 10, "Rakshasa", "Rat", "Kapha"), 2);
        var b = Person(Sign(ZodiacName.Aries, ZodiacName.Leo), Moon(ZodiacName.Leo, 10, "Rakshasa", "Rat", "Kapha"), 3);
        var m = Area(LifeMatterSimilarity.Compare(a, b), "moon");
        Assert.Equal(["Nakshatra"], m.Shared.Select(s => s.Key).ToArray());
        Assert.Equal(Math.Log(27) + Math.Log(108), m.TotalWeight, 9);
        Assert.Equal(Math.Log(27) / m.TotalWeight, m.Score!.Value, 9);
    }

    [Fact]
    public void Different_nakshatras_compare_gana_yoni_and_nadi_by_their_own_chance()
    {
        var a = Person(Sign(ZodiacName.Aries, ZodiacName.Leo), Moon(ZodiacName.Pisces, 26, "Manushya", "Cow", "Pitta"));
        var b = Person(Sign(ZodiacName.Aries, ZodiacName.Leo), Moon(ZodiacName.Scorpio, 17, "Deva", "Deer", "Pitta"));
        var m = Area(LifeMatterSimilarity.Compare(a, b), "moon");
        Assert.Equal(["Nadi"], m.Shared.Select(s => s.Key).ToArray());
        Assert.Equal(Math.Log(3), m.SharedWeight, 9);
    }

    [Fact]
    public void Lean_is_a_tie_inside_the_band_and_otherwise_names_the_nearer_relative()
    {
        SimilarityArea area = LifeMatterSimilarity.Areas[0];
        AreaSimilarity S(double? s) => new(area, s, 0, 0, []);
        Assert.Equal(SimilarityLean.Tie, LifeMatterSimilarity.Lean(S(0.30), S(0.20)));
        Assert.Equal(SimilarityLean.First, LifeMatterSimilarity.Lean(S(0.40), S(0.10)));
        Assert.Equal(SimilarityLean.Second, LifeMatterSimilarity.Lean(S(0.0), S(0.25)));
        Assert.Equal(SimilarityLean.Unknown, LifeMatterSimilarity.Lean(S(null), S(0.25)));
    }
}
