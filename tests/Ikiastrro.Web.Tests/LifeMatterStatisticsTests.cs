using Ikiastrro.Data;
using Ikiastrro.Web.Components.LifeMatters;

namespace Ikiastrro.Web.Tests;

public sealed class LifeMatterStatisticsTests
{
    private static ArgalaFactRow Fact(int house, string relation, int offset, string planet, bool exception = false) =>
        new("D1", "House", house.ToString(), (byte)house, relation, (byte)offset, offset != 5 && offset != 9, planet, exception, false, "SRC_PVR_INTEGRATED");

    private static LifeMatterStatistics Build(IEnumerable<ArgalaFactRow>? argala = null) => new(
        "Aries",
        [
            // SAV is read per sign; one recipient row per sign is enough (SarvaBindus repeats per recipient).
            new AshtakavargaRow("D1", "SUN", 1, 1, 5, 30),
            new AshtakavargaRow("D1", "SUN", 7, 7, 2, 17),
            new AshtakavargaRow("D1", "SUN", 11, 11, 6, 43),
            new AshtakavargaRow("D9", "SUN", 7, 7, 6, 40),
        ],
        [
            new BhavaBalaSummaryRow(1, "Aries", "Mars", 1, "Aries", "Moolatrikona", 638.6m, 10.64m),
            new BhavaBalaSummaryRow(7, "Libra", "Venus", 1, "Aries", "Enemy", 384.5m, 6.41m),
            new BhavaBalaSummaryRow(11, "Aquarius", "Saturn", 6, "Virgo", "Neutral", 285m, 4.75m),
        ],
        [
            new ShadbalaSummaryRow("Mars", 0, 0, 0, 0, 0, 0, 0, 608m, 10.13m, 5m, 202.68m),
            new ShadbalaSummaryRow("Venus", 0, 0, 0, 0, 0, 0, 0, 384m, 6.40m, 5.5m, 116.35m),
            new ShadbalaSummaryRow("Saturn", 0, 0, 0, 0, 0, 0, 0, 400m, 4.2m, 5m, 84m),
        ],
        argala ?? []);

    [Fact]
    public void ForSign_ReadsD1FactsForThatSignAsAHouseFromTheD1Lagna()
    {
        var stats = Build().ForSign("Libra");

        Assert.Equal(7, stats.HouseFromLagna);
        Assert.Equal(17, stats.SavBindus);          // D1 row, not the D9 row for the same sign
        Assert.Equal(6.41m, stats.BhavaBalaRupas);
        Assert.Equal("Venus", stats.LordPlanet);
        Assert.Equal(116.35m, stats.LordShadbalaPercent);
    }

    [Theory]
    [InlineData(31, StrengthBand.Strong)]
    [InlineData(30, StrengthBand.Middle)]
    [InlineData(25, StrengthBand.Middle)]
    [InlineData(24, StrengthBand.Weak)]
    public void SavBand_UsesTheCitedAbove30Below25Rule(int bindus, StrengthBand expected) =>
        Assert.Equal(expected, LifeMatterStatistics.SavBand(bindus));

    [Theory]
    [InlineData(7.0, StrengthBand.Strong)]
    [InlineData(6.99, StrengthBand.Middle)]
    [InlineData(5.0, StrengthBand.Middle)]
    [InlineData(4.99, StrengthBand.Weak)]
    public void BhavaBand_MatchesHouseStrengthChart(double rupas, StrengthBand expected) =>
        Assert.Equal(expected, LifeMatterStatistics.BhavaBand((decimal)rupas));

    [Theory]
    [InlineData(110.0, StrengthBand.Strong)]
    [InlineData(109.9, StrengthBand.Middle)]
    [InlineData(90.0, StrengthBand.Middle)]
    [InlineData(89.9, StrengthBand.Weak)]
    public void ShadbalaBand_MatchesPlanetaryStateTableStrongAndWeak(double percent, StrengthBand expected) =>
        Assert.Equal(expected, LifeMatterStatistics.ShadbalaBand((decimal)percent));

    [Fact]
    public void MissingStatistics_ReadAsNone()
    {
        var stats = Build().ForSign("Gemini"); // lord Mercury has no Ṣaḍbala row in this fixture

        Assert.Null(stats.SavBindus);
        Assert.Equal([StrengthBand.None, StrengthBand.None, StrengthBand.None, StrengthBand.None], stats.Bands);
    }

    [Fact]
    public void Argala_PairsEachInterventionWithItsObstruction()
    {
        // House 7 for Ramakrishnan: Moon (2nd) and Ketu (4th) give argala; Jupiter + Saturn (12th)
        // obstruct the 2nd, Rahu (10th) obstructs the 4th.
        var stats = Build(
        [
            Fact(7, "ARGALA", 2, "Moon"), Fact(7, "ARGALA", 4, "Ketu"),
            Fact(7, "VIRODHARGALA", 12, "Jupiter"), Fact(7, "VIRODHARGALA", 12, "Saturn"),
            Fact(7, "VIRODHARGALA", 10, "Rahu"),
        ]).ForSign("Libra");

        var second = stats.Argala.Pairs.Single(p => p.ArgalaOffset == 2);
        var fourth = stats.Argala.Pairs.Single(p => p.ArgalaOffset == 4);
        Assert.Equal(ArgalaVerdict.Obstructed, second.Verdict);
        Assert.Equal(["Jupiter", "Saturn"], second.ObstructingPlanets);
        Assert.Equal(ArgalaVerdict.Contested, fourth.Verdict);
        Assert.Equal(-1, stats.Argala.Net);
        Assert.Equal(StrengthBand.Weak, stats.ArgalaBand);
    }

    [Fact]
    public void Argala_HoldsWhenIntervenersOutnumber_AndObstructionAloneHasNoVerdict()
    {
        var summary = LifeMatterStatistics.BuildArgala(
        [
            Fact(1, "ARGALA", 11, "Jupiter"), Fact(1, "ARGALA", 11, "Venus"), Fact(1, "VIRODHARGALA", 3, "Mars"),
            Fact(1, "VIRODHARGALA", 9, "Sun"),
        ]);

        Assert.Equal(ArgalaVerdict.Holds, summary.Pairs.Single(p => p.ArgalaOffset == 11).Verdict);
        Assert.Null(summary.Pairs.Single(p => p.ArgalaOffset == 5).Verdict);
        Assert.Equal(1, summary.Net);
    }

    [Fact]
    public void Argala_MaleficThirdIsAddedOnlyWhenPresent()
    {
        Assert.DoesNotContain(LifeMatterStatistics.BuildArgala([]).Pairs, p => p.ArgalaOffset == 3);

        var summary = LifeMatterStatistics.BuildArgala([Fact(4, "ARGALA", 3, "Saturn", exception: true)]);
        var third = summary.Pairs.Single(p => p.ArgalaOffset == 3);
        Assert.Equal(11, third.ObstructionOffset);
        Assert.True(third.ExceptionApplied);
        Assert.Equal(ArgalaVerdict.Holds, third.Verdict);
    }

    [Fact]
    public void Argala_FactsFromOtherChartsAndGrahaTargetsAreIgnored()
    {
        var stats = Build(
        [
            new ArgalaFactRow("D9", "House", "1", 1, "ARGALA", 2, true, "Moon", false, false, null),
            new ArgalaFactRow("D1", "Graha", "Sun", 1, "ARGALA", 4, true, "Rahu", false, false, null),
        ]).ForSign("Aries");

        Assert.False(stats.Argala.Any);
        Assert.Equal(StrengthBand.None, stats.ArgalaBand);
    }

    [Fact]
    public void SortKey_IsStrongMinusWeak() =>
        Assert.Equal(1, LifeMatterStatistics.SortKey([StrengthBand.Strong, StrengthBand.Strong, StrengthBand.Weak, StrengthBand.None]));

    [Fact]
    public void ForSign_InAVarga_ReadsThatVargasSavAndLeavesBhavaBalaEmpty()
    {
        var d1 = Build();
        var d9 = new LifeMatterStatistics("D9", "Aries",
            [new AshtakavargaRow("D1", "SUN", 7, 7, 2, 17), new AshtakavargaRow("D9", "SUN", 7, 7, 6, 40)],
            [new BhavaBalaSummaryRow(7, "Libra", "Venus", 1, "Aries", "Enemy", 384.5m, 6.41m)],
            [new ShadbalaSummaryRow("Venus", 0, 0, 0, 0, 0, 0, 0, 384m, 6.40m, 5.5m, 116.35m)],
            []).ForSign("Libra");

        Assert.Equal(17, d1.ForSign("Libra").SavBindus);
        Assert.Equal(40, d9.SavBindus);
        Assert.Null(d9.BhavaBalaRupas);                 // Bhava Bala is a D1 house computation
        Assert.Equal(StrengthBand.None, d9.BhavaBand);
        Assert.Equal(116.35m, d9.LordShadbalaPercent);  // one Ṣaḍbala per planet
    }

    [Fact]
    public void LiveArgala_BuildsHouseFactsFromTheChartsOwnPlacements()
    {
        // Aries lagna; Jupiter in Taurus is in the 2nd from house 1, Saturn in Pisces the 12th.
        var grahas = new[]
        {
            new Ikiastrro.Core.Models.ChartKeyDetail { Planet = "Ascendant", Sign = "Aries", PointKind = "Graha" },
            new Ikiastrro.Core.Models.ChartKeyDetail { Planet = "Jupiter", Sign = "Taurus", PointKind = "Graha" },
            new Ikiastrro.Core.Models.ChartKeyDetail { Planet = "Saturn", Sign = "Pisces", PointKind = "Graha" },
        };

        var facts = LifeMatterStatistics.LiveArgala("D10", "Aries", grahas);
        var house1 = new LifeMatterStatistics("D10", "Aries", [], [], [], facts).ForSign("Aries").Argala;

        Assert.All(facts, f => Assert.Equal("D10", f.ChartType));
        var pair = Assert.Single(house1.Pairs, p => p.ArgalaOffset == 2);
        Assert.Equal(["Jupiter"], pair.ArgalaPlanets);
        Assert.Equal(["Saturn"], pair.ObstructingPlanets);
        Assert.Equal(ArgalaVerdict.Contested, pair.Verdict);
    }

    [Theory]
    [InlineData(StrengthLean.Strong, StrengthBand.Strong, StrengthBand.Strong, StrengthBand.Middle, StrengthBand.None)]
    [InlineData(StrengthLean.LeansStrong, StrengthBand.Strong, StrengthBand.Middle, StrengthBand.Middle, StrengthBand.None)]
    [InlineData(StrengthLean.Mixed, StrengthBand.Strong, StrengthBand.Weak, StrengthBand.Middle, StrengthBand.Middle)]
    [InlineData(StrengthLean.LeansWeak, StrengthBand.Weak, StrengthBand.Middle, StrengthBand.None, StrengthBand.None)]
    [InlineData(StrengthLean.Weak, StrengthBand.Weak, StrengthBand.Weak, StrengthBand.Weak, StrengthBand.Strong)]
    [InlineData(StrengthLean.None, StrengthBand.None, StrengthBand.None, StrengthBand.None, StrengthBand.None)]
    public void Lean_SummarisesTheFourSignalsAsOneReading(StrengthLean expected, StrengthBand a, StrengthBand b, StrengthBand c, StrengthBand d) =>
        Assert.Equal(expected, LifeMatterStatistics.Lean([a, b, c, d]));
}
