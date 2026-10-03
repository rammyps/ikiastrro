using Ikiastrro.Core.Engines.Matching;
using Ikiastrro.Core.Models;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>SadeSatiCompatibility over stored Saturn periods: retrograde re-entries merge into one spell,
/// and the overlap is where both people are inside a spell at once.</summary>
public class SadeSatiCompatibilityTests
{
    private static SadeSatiPeriod P(string type, string from, string to) =>
        new(type, 0, DateTime.Parse(from), DateTime.Parse(to), "Libra");

    // One Sade Sati spell from the real shape of the stored data (a Rising interval split by a retrograde
    // gap, then Peak, then Setting): 2011-11 to 2020-01.
    private static readonly SadeSatiPeriod[] SpellA =
    [
        P("SadeSati_Dhaiya1_Rising", "2011-11-15", "2012-05-15"),
        P("SadeSati_Dhaiya1_Rising", "2012-08-04", "2014-11-02"),
        P("SadeSati_Dhaiya2_Peak", "2014-11-02", "2017-01-26"),
        P("SadeSati_Dhaiya2_Peak", "2017-06-20", "2017-10-26"),
        P("SadeSati_Dhaiya3_Setting", "2017-10-26", "2020-01-24"),
    ];

    [Fact]
    public void Retrograde_gaps_inside_a_spell_merge_into_one_spell()
    {
        var spells = SadeSatiCompatibility.Spells(SpellA);
        var spell = Assert.Single(spells);
        Assert.Equal(SadeSatiCompatibility.SadeSati, spell.Category);
        Assert.Equal(new DateTime(2011, 11, 15), spell.StartUtc);
        Assert.Equal(new DateTime(2020, 1, 24), spell.EndUtc);
    }

    [Fact]
    public void Standing_reports_the_current_spell_and_its_phase()
    {
        var result = SadeSatiCompatibility.Compare(SpellA, [], new DateTime(2015, 6, 1));
        var standing = result.First.Single(s => s.Category == SadeSatiCompatibility.SadeSati);
        Assert.NotNull(standing.Current);
        Assert.Equal("Peak", standing.Phase);
        Assert.Null(standing.Next);
    }

    [Fact]
    public void Two_spells_that_overlap_give_the_shared_window()
    {
        var a = new[] { P("SadeSati_Dhaiya1_Rising", "2040-01-01", "2042-01-01"), P("SadeSati_Dhaiya2_Peak", "2042-01-01", "2047-01-01") };
        var b = new[] { P("SadeSati_Dhaiya1_Rising", "2043-01-01", "2045-01-01"), P("SadeSati_Dhaiya3_Setting", "2045-01-01", "2050-01-01") };

        var result = SadeSatiCompatibility.Compare(a, b, new DateTime(2026, 10, 3), horizonYears: 30);

        var overlap = Assert.Single(result.Overlaps);
        Assert.Equal(new DateTime(2043, 1, 1), overlap.StartUtc);
        Assert.Equal(new DateTime(2047, 1, 1), overlap.EndUtc);
        Assert.True(result.SadeSatiOverlaps);
    }

    [Fact]
    public void Spells_that_never_coincide_are_reported_as_staggered()
    {
        var b = new[] { P("SadeSati_Dhaiya1_Rising", "2041-01-01", "2044-01-01"), P("SadeSati_Dhaiya3_Setting", "2044-01-01", "2049-01-01") };

        var result = SadeSatiCompatibility.Compare(SpellA, b, new DateTime(2026, 10, 3));

        Assert.Empty(result.Overlaps);
        Assert.False(result.SadeSatiOverlaps);
        Assert.Equal(new DateTime(2041, 1, 1), result.Second.Single(s => s.Category == SadeSatiCompatibility.SadeSati).Next!.StartUtc);
    }

    [Fact]
    public void Overlaps_in_the_past_or_beyond_the_horizon_are_left_out()
    {
        var a = new[] { P("SadeSati_Dhaiya2_Peak", "2000-01-01", "2005-01-01"), P("SadeSati_Dhaiya2_Peak", "2090-01-01", "2095-01-01") };
        var b = new[] { P("SadeSati_Dhaiya2_Peak", "2001-01-01", "2004-01-01"), P("SadeSati_Dhaiya2_Peak", "2091-01-01", "2094-01-01") };

        Assert.Empty(SadeSatiCompatibility.Compare(a, b, new DateTime(2026, 10, 3), horizonYears: 30).Overlaps);
    }

    [Fact]
    public void Kantaka_and_ashtama_periods_are_compared_as_their_own_categories()
    {
        var a = new[] { P("KantakaShani", "2030-01-01", "2032-06-01"), P("AshtamaShani", "2034-01-01", "2036-06-01") };
        var b = new[] { P("KantakaShani", "2031-01-01", "2033-06-01") };

        var result = SadeSatiCompatibility.Compare(a, b, new DateTime(2026, 10, 3));

        var overlap = Assert.Single(result.Overlaps);
        Assert.Equal(SadeSatiCompatibility.Kantaka, overlap.Category);
        Assert.False(result.SadeSatiOverlaps);
    }
}
