using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Xunit;

namespace Ikiastrro.Yoga.Tests;

/// <summary>DignityEngine.EvaluatePairRelationship — the planet-to-planet (not just planet-to-own-
/// sign-lord) Panchadha Maitri generalization added for Lajjitadi's "conjoined/aspected by a friend/
/// enemy" conditions (PVR sec 15.4.3).</summary>
public class DignityEnginePairRelationshipTests
{
    [Fact]
    public void Natural_friend_at_a_temporary_friend_distance_is_a_great_friend()
    {
        // Sun and Moon are natural friends of each other; Aries -> Gemini is 3 signs apart, one of
        // the temporary-friend distances (2/3/4/10/11/12) -> naturalFriend + temporaryFriend -> Great Friend.
        var tier = DignityEngine.EvaluatePairRelationship("Sun", "Moon", ZodiacName.Aries, ZodiacName.Gemini);
        Assert.Equal("Great Friend", tier);
    }

    [Fact]
    public void Natural_enemies_far_apart_are_a_great_enemy()
    {
        // Sun and Saturn are natural enemies; Sun in Aries, Saturn in Libra (7 signs apart -> not
        // one of the temporary-friend distances 2/3/4/10/11/12) -> Great Enemy.
        var tier = DignityEngine.EvaluatePairRelationship("Sun", "Saturn", ZodiacName.Aries, ZodiacName.Libra);
        Assert.Equal("Great Enemy", tier);
    }

    [Fact]
    public void Rahu_or_ketu_as_subject_has_no_naisargika_table_and_returns_null()
    {
        Assert.Null(DignityEngine.EvaluatePairRelationship("Rahu", "Sun", ZodiacName.Gemini, ZodiacName.Aries));
        Assert.Null(DignityEngine.EvaluatePairRelationship("Ketu", "Sun", ZodiacName.Sagittarius, ZodiacName.Aries));
    }

    [Fact]
    public void Rahu_or_ketu_as_associated_planet_falls_through_to_natural_neutral()
    {
        // Jupiter has no friend/enemy entry for Rahu (nodes aren't in the Naisargika table at all),
        // so it falls to the temporary-friendship-only branch: Sagittarius -> Aquarius is 3 signs
        // apart (a temporary-friend distance) -> Friend, not an error.
        var tier = DignityEngine.EvaluatePairRelationship("Jupiter", "Rahu", ZodiacName.Sagittarius, ZodiacName.Aquarius);
        Assert.Equal("Friend", tier);
    }
}
