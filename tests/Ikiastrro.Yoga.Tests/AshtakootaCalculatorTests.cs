using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Matching;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>AshtakootaCalculator against the worked examples and one-line rules in Vasudev, "The Art
/// of Matching Charts", Ch. VI (SRC_VASUDEV_MATCHING_CHARTS). Boy = groom, girl = bride.</summary>
public class AshtakootaCalculatorTests
{
    private static MatchPerson P(ZodiacName sign, int nakshatra = 1, string gana = "Deva", string animal = "Horse", string nadi = "Vata") =>
        new(sign, nakshatra, gana, animal, "Male", nadi);

    private static int Score(AshtakootaResult r, string code) => r.Scored.Single(k => k.Code == code).Score!.Value;

    // Book p.81-83: Jyeshta boy and Anuradha girl, both Scorpio. 1+0+3+4+5+0+7+8 = 28 of 36.
    private static readonly MatchPerson Jyeshta = P(ZodiacName.Scorpio, 18, "Rakshasa", "Deer", "Vata");
    private static readonly MatchPerson Anuradha = P(ZodiacName.Scorpio, 17, "Deva", "Deer", "Pitta");

    [Fact]
    public void Worked_example_jyeshta_boy_anuradha_girl_scores_28_of_36()
    {
        var r = AshtakootaCalculator.Score(Jyeshta, Anuradha);

        Assert.Equal(1, Score(r, "VARNA"));
        Assert.Equal(0, Score(r, "VASHYA"));
        Assert.Equal(3, Score(r, "DINA"));
        Assert.Equal(4, Score(r, "YONI"));
        Assert.Equal(5, Score(r, "GRAHAMAITRA"));
        Assert.Equal(0, Score(r, "GANA"));
        Assert.Equal(7, Score(r, "RASI"));
        Assert.Equal(8, Score(r, "NADI"));
        Assert.Equal(28, r.TotalMin);
        Assert.True(r.IsComplete);
        Assert.True(r.Passes);
        Assert.True(r.SameSignLord);
    }

    [Fact]
    public void Worked_example_rajju_groups_differ_and_the_unscored_factors_are_reported()
    {
        var r = AshtakootaCalculator.Score(Jyeshta, Anuradha);
        // Anuradha is Ooru, Jyeshta is Pada: "different Rajjus is favourable".
        Assert.Equal(KutaStatus.Present, r.Additional.Single(k => k.Code == "RAJJU").Status);
        Assert.All(r.Additional, k => Assert.Null(k.Score));
    }

    [Theory]
    [InlineData(17, 4, 0)]  // boy Anuradha, girl Rohini: count 14, remainder 5, not good (p.68)
    [InlineData(18, 11, 3)] // boy Jyeshta, girl Purva Phalguni: count 8, good (p.68)
    [InlineData(18, 17, 3)] // boy Jyeshta, girl Anuradha: count 2, good (p.82)
    public void Dina_counts_from_the_girls_star_and_wants_remainder_2_4_6_8_or_0(int boyStar, int girlStar, int expected)
    {
        var r = AshtakootaCalculator.Score(P(ZodiacName.Aries, boyStar), P(ZodiacName.Aries, girlStar));
        Assert.Equal(expected, Score(r, "DINA"));
    }

    [Theory]
    [InlineData("Deva", "Deva", 6)]
    [InlineData("Manushya", "Manushya", 6)]
    [InlineData("Rakshasa", "Rakshasa", 6)]
    [InlineData("Deva", "Manushya", 4)]      // Daivika boy, Manushya girl
    [InlineData("Manushya", "Deva", 4)]      // Daivika girl, Manushya boy
    [InlineData("Manushya", "Rakshasa", 2)]  // Manushya boy, Rakshasa girl (p.72)
    [InlineData("Rakshasa", "Manushya", 2)]
    [InlineData("Rakshasa", "Deva", 0)]      // Jyeshta boy, Anuradha girl
    [InlineData("Deva", "Rakshasa", 0)]
    public void Gana_scores_follow_the_books_table(string boyGana, string girlGana, int expected)
    {
        var r = AshtakootaCalculator.Score(P(ZodiacName.Aries, gana: boyGana), P(ZodiacName.Aries, gana: girlGana));
        Assert.Equal(expected, Score(r, "GANA"));
    }

    [Theory]
    [InlineData(ZodiacName.Taurus, ZodiacName.Aquarius, 5)]    // Venus and Saturn are friends (p.70)
    [InlineData(ZodiacName.Aries, ZodiacName.Cancer, 4)]       // Mars neutral / Moon friendly (p.70)
    [InlineData(ZodiacName.Virgo, ZodiacName.Sagittarius, 0)]  // Jupiter neutral, Mercury inimical (p.70)
    [InlineData(ZodiacName.Leo, ZodiacName.Taurus, 0)]         // Sun and Venus both inimical (p.71)
    [InlineData(ZodiacName.Scorpio, ZodiacName.Scorpio, 5)]    // same ruler
    public void Grahamaitra_reads_each_rulers_natural_attitude_to_the_other(ZodiacName boySign, ZodiacName girlSign, int expected)
    {
        var r = AshtakootaCalculator.Score(P(boySign), P(girlSign));
        Assert.Equal(expected, Score(r, "GRAHAMAITRA"));
    }

    [Fact]
    public void Varna_fails_when_the_girl_is_higher_unless_the_sign_rulers_make_it_up()
    {
        // Boy Aries (Kshatriya), girl Scorpio (Brahmin): higher, but both rulers are Mars, a Kshatriya (p.67).
        Assert.Equal(1, Score(AshtakootaCalculator.Score(P(ZodiacName.Aries), P(ZodiacName.Scorpio)), "VARNA"));
        // Boy Gemini (Sudra, ruler Mercury: Sudra), girl Cancer (Brahmin, ruler Moon: Vaisya): higher and not made up.
        Assert.Equal(0, Score(AshtakootaCalculator.Score(P(ZodiacName.Gemini), P(ZodiacName.Cancer)), "VARNA"));
        // Girl lower or equal passes.
        Assert.Equal(1, Score(AshtakootaCalculator.Score(P(ZodiacName.Scorpio), P(ZodiacName.Taurus)), "VARNA"));
    }

    [Theory]
    [InlineData(ZodiacName.Leo, ZodiacName.Aries, 2)]    // p.68: boy Leo, girl Aries: present
    [InlineData(ZodiacName.Leo, ZodiacName.Taurus, 0)]   // p.68: boy Leo, girl Taurus: none
    [InlineData(ZodiacName.Scorpio, ZodiacName.Scorpio, 0)] // p.68: same sign: lacking
    public void Vashya_follows_the_books_examples(ZodiacName boySign, ZodiacName girlSign, int expected)
    {
        Assert.Equal(expected, Score(AshtakootaCalculator.Score(P(boySign), P(girlSign)), "VASHYA"));
    }

    [Theory]
    [InlineData(ZodiacName.Aries, ZodiacName.Taurus, 7)]         // girl's sign 2nd from boy's: long life
    [InlineData(ZodiacName.Gemini, ZodiacName.Taurus, 0)]        // boy's sign 2nd from girl's, odd sign: fatal
    [InlineData(ZodiacName.Leo, ZodiacName.Gemini, 0)]           // boy 3rd from girl: misery
    [InlineData(ZodiacName.Leo, ZodiacName.Libra, 7)]            // girl 3rd from boy: happiness
    [InlineData(ZodiacName.Aries, ZodiacName.Capricornus, 0)]    // boy 4th from girl: poverty
    [InlineData(ZodiacName.Cancer, ZodiacName.Libra, 7)]         // girl 4th from boy: prosperity
    [InlineData(ZodiacName.Scorpio, ZodiacName.Cancer, 0)]       // boy 5th from girl: widowhood
    [InlineData(ZodiacName.Sagittarius, ZodiacName.Aries, 7)]    // girl 5th from boy: long married life
    [InlineData(ZodiacName.Aquarius, ZodiacName.Virgo, 0)]       // boy 6th from girl: loss of progeny
    [InlineData(ZodiacName.Aquarius, ZodiacName.Cancer, 7)]      // girl 6th from boy: birth of children
    [InlineData(ZodiacName.Virgo, ZodiacName.Aries, 7)]          // 6th-from exception pair (girl Aries, boy Virgo)
    [InlineData(ZodiacName.Cancer, ZodiacName.Capricornus, 7)]   // opposite signs: happy
    [InlineData(ZodiacName.Scorpio, ZodiacName.Scorpio, 7)]      // same sign (p.82)
    public void Rasi_follows_the_books_directional_verdicts(ZodiacName boySign, ZodiacName girlSign, int expected)
    {
        Assert.Equal(expected, Score(AshtakootaCalculator.Score(P(boySign), P(girlSign)), "RASI"));
    }

    [Fact]
    public void Nadi_scores_8_only_when_the_nadis_differ()
    {
        Assert.Equal(8, Score(AshtakootaCalculator.Score(P(ZodiacName.Aries, nadi: "Vata"), P(ZodiacName.Aries, nadi: "Pitta")), "NADI"));
        Assert.Equal(0, Score(AshtakootaCalculator.Score(P(ZodiacName.Aries, nadi: "Kapha"), P(ZodiacName.Aries, nadi: "Kapha")), "NADI"));
    }

    [Fact]
    public void Different_yoni_animals_are_left_unscored_and_widen_the_total_range()
    {
        var r = AshtakootaCalculator.Score(P(ZodiacName.Scorpio, animal: "Deer"), P(ZodiacName.Scorpio, animal: "Monkey"));
        var yoni = r.Scored.Single(k => k.Code == "YONI");
        Assert.Null(yoni.Score);
        Assert.Equal(KutaStatus.Unscored, yoni.Status);
        Assert.False(r.IsComplete);
        Assert.Equal(r.TotalMin + 4, r.TotalMax);
    }

    [Theory]
    [InlineData(16, 4, true)]    // Visakha boy, Rohini girl: 13th, beyond the 9th (p.77)
    [InlineData(5, 4, false)]    // within the 9th
    public void Stree_deergha_wants_the_boys_star_beyond_the_9th(int boyStar, int girlStar, bool present)
    {
        var r = AshtakootaCalculator.Score(P(ZodiacName.Aries, boyStar), P(ZodiacName.Aries, girlStar));
        Assert.Equal(present, r.Additional.Single(k => k.Code == "STREE_DEERGHA").Status == KutaStatus.Present);
    }

    [Fact]
    public void Mahendra_is_present_when_the_boys_star_is_the_4th_from_the_girls()
    {
        // Boy Ardra (6), girl Krittika (3): the 4th (p.77).
        var r = AshtakootaCalculator.Score(P(ZodiacName.Aries, 6), P(ZodiacName.Aries, 3));
        Assert.Equal(KutaStatus.Present, r.Additional.Single(k => k.Code == "MAHENDRA").Status);
    }

    [Fact]
    public void Anuradha_boy_and_shravana_girl_are_scored_by_hand_derived_values()
    {
        // Boy: Anuradha (17), Scorpio, Deva, Deer, Pitta. Girl: Shravana (22), Capricorn, Deva, Monkey, Kapha.
        // Varna 1 (girl Vaisya is below the boy's Brahmin); Vashya 0 (Capricorn's list is Aquarius, Aries);
        // Dina 0 (girl's star to boy's counts 23, remainder 5); Yoni unscored (Deer vs Monkey);
        // Grahamaitra 0 (Mars regards Saturn neutral, Saturn regards Mars an enemy); Gana 6; Rasi 7
        // (the girl's sign is 3rd from the boy's); Nadi 8.
        var boy = P(ZodiacName.Scorpio, 17, "Deva", "Deer", "Pitta");
        var girl = P(ZodiacName.Capricornus, 22, "Deva", "Monkey", "Kapha");

        var r = AshtakootaCalculator.Score(boy, girl);

        Assert.Equal(1, Score(r, "VARNA"));
        Assert.Equal(0, Score(r, "VASHYA"));
        Assert.Equal(0, Score(r, "DINA"));
        Assert.Null(r.Scored.Single(k => k.Code == "YONI").Score);
        Assert.Equal(0, Score(r, "GRAHAMAITRA"));
        Assert.Equal(6, Score(r, "GANA"));
        Assert.Equal(7, Score(r, "RASI"));
        Assert.Equal(8, Score(r, "NADI"));
        Assert.Equal(22, r.TotalMin);
        Assert.Equal(26, r.TotalMax);
        Assert.True(r.Passes);
        Assert.False(r.SameSignLord);
        Assert.Equal(KutaStatus.Present, r.Additional.Single(k => k.Code == "RAJJU").Status);
        Assert.Equal(KutaStatus.Present, r.Additional.Single(k => k.Code == "STREE_DEERGHA").Status);
        Assert.Equal(KutaStatus.Absent, r.Additional.Single(k => k.Code == "MAHENDRA").Status);
    }

    [Fact]
    public void Every_nakshatra_has_a_rajju_group_so_a_star_against_itself_is_always_the_same_group()
    {
        for (var n = 1; n <= 27; n++)
        {
            var r = AshtakootaCalculator.Score(P(ZodiacName.Aries, n), P(ZodiacName.Aries, n));
            Assert.Equal(KutaStatus.Absent, r.Additional.Single(k => k.Code == "RAJJU").Status);
        }
    }
}
