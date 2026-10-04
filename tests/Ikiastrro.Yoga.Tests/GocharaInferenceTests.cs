using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Transits;

namespace Ikiastrro.Yoga.Tests;

/// <summary>The net tier and the slow/fast reading (docs/architecture/transit_gochara_inference.md).
/// Natal Moon in Pisces, the same fixture as <see cref="GocharaReadingTests"/>.</summary>
public sealed class GocharaInferenceTests
{
    private static readonly GocharaVedhaRule[] Rules =
    [
        new(PlanetName.Mercury, 4, 3, PlanetName.Moon),
        new(PlanetName.Jupiter, 5, 4),
        new(PlanetName.Sun, 3, 9, PlanetName.Saturn),
        new(PlanetName.Saturn, 11, 5, PlanetName.Sun),
        new(PlanetName.Saturn, 3, 12, PlanetName.Sun),
        new(PlanetName.Mars, 6, 9),
    ];

    private static readonly Dictionary<PlanetName, ZodiacName> Transits = new()
    {
        [PlanetName.Mercury] = ZodiacName.Gemini,
        [PlanetName.Mars] = ZodiacName.Taurus,
        [PlanetName.Jupiter] = ZodiacName.Cancer,
        [PlanetName.Sun] = ZodiacName.Aries,
        [PlanetName.Saturn] = ZodiacName.Pisces,
        [PlanetName.Rahu] = ZodiacName.Capricornus,
        [PlanetName.Ketu] = ZodiacName.Cancer,
    };

    private static GocharaInference Build(Func<PlanetName, ZodiacName, int?>? bindus = null,
        IReadOnlyDictionary<PlanetName, GocharaIngress>? ingress = null) =>
        GocharaInferenceBuilder.Build(
            GocharaReading.Read(ZodiacName.Pisces, Transits, Rules, bindus ?? ((p, _) => p == PlanetName.Mercury ? 5 : 2)),
            ingress ?? new Dictionary<PlanetName, GocharaIngress>(), Rules);

    [Theory]
    [InlineData(GocharaVerdict.Good, 8, GocharaTier.Supportive)]
    [InlineData(GocharaVerdict.Good, 5, GocharaTier.Supportive)]
    [InlineData(GocharaVerdict.Good, 4, GocharaTier.Mild)]
    [InlineData(GocharaVerdict.Good, 3, GocharaTier.Mixed)]
    [InlineData(GocharaVerdict.Good, 0, GocharaTier.Mixed)]
    [InlineData(GocharaVerdict.Good, null, GocharaTier.Mild)]
    [InlineData(GocharaVerdict.Blocked, 8, GocharaTier.Obstructed)]
    [InlineData(GocharaVerdict.Blocked, 2, GocharaTier.Obstructed)]
    [InlineData(GocharaVerdict.Blocked, null, GocharaTier.Obstructed)]
    [InlineData(GocharaVerdict.NotFavourable, 7, GocharaTier.Mixed)]
    [InlineData(GocharaVerdict.NotFavourable, 4, GocharaTier.Unfavourable)]
    [InlineData(GocharaVerdict.NotFavourable, 1, GocharaTier.Unfavourable)]
    [InlineData(GocharaVerdict.NotFavourable, null, GocharaTier.Unfavourable)]
    public void Tier_combines_the_verdict_with_the_bindus_band(GocharaVerdict verdict, int? bindus, GocharaTier expected) =>
        Assert.Equal(expected, GocharaInferenceBuilder.TierOf(verdict, bindus));

    [Fact]
    public void Slow_planets_are_split_from_fast_and_Saturn_leads()
    {
        var inf = Build();
        Assert.Equal([PlanetName.Saturn, PlanetName.Jupiter, PlanetName.Rahu, PlanetName.Ketu],
            inf.Slow.Select(r => r.Row.Planet));
        Assert.Equal(3, inf.Fast.Count); // the fixture transits only Mercury, Mars and Sun of the fast five
        Assert.DoesNotContain(inf.Fast, r => GocharaInferenceBuilder.SlowPlanets.Contains(r.Row.Planet));
        Assert.Equal(SaturnPhase.SadeSatiPeak, inf.Saturn);
        Assert.Empty(inf.Qualifiers);
        Assert.Equal(GocharaInferenceBuilder.Caveat, inf.Caveat);
    }

    [Fact]
    public void Obstructed_planet_names_its_obstructor_and_ignores_strong_bindus()
    {
        var inf = Build((_, _) => 8);
        var jupiter = inf.Slow.Single(r => r.Row.Planet == PlanetName.Jupiter);
        Assert.Equal(GocharaTier.Obstructed, jupiter.Tier); // Mercury in its Vedha house
        Assert.Contains("Mercury", jupiter.Line);
        Assert.Contains("8/8", jupiter.Line);
    }

    [Fact]
    public void Nodes_are_read_by_their_proxy_and_carry_no_bindus()
    {
        var rahu = Build().Slow.Single(r => r.Row.Planet == PlanetName.Rahu);
        Assert.Contains("read as Saturn", rahu.Line);
        Assert.DoesNotContain("bindus", rahu.Line);
    }

    [Fact]
    public void Next_crossing_gives_house_and_verdict_before_Vedha()
    {
        var when = new DateTime(2027, 6, 2, 0, 0, 0, DateTimeKind.Utc);
        var inf = Build(ingress: new Dictionary<PlanetName, GocharaIngress>
        {
            [PlanetName.Saturn] = new(when, ZodiacName.Aries, false),     // 2nd from Pisces: not listed
            [PlanetName.Rahu] = new(when, ZodiacName.Aquarius, true),     // 11th: Saturn's row
        });
        Assert.Equal([PlanetName.Saturn, PlanetName.Rahu], inf.Changes.Select(c => c.Planet));
        var saturn = inf.Changes[0];
        Assert.Equal(2, saturn.HouseFromMoon);
        Assert.Equal(GocharaVerdict.NotFavourable, saturn.VerdictBeforeVedha);
        Assert.Equal(SaturnPhase.SadeSatiSetting, saturn.SaturnPhaseThen);
        var rahu = inf.Changes[1];
        Assert.Equal(12, rahu.HouseFromMoon);
        Assert.True(rahu.IsRetrograde);
        Assert.Equal(SaturnPhase.None, rahu.SaturnPhaseThen);
    }
}
