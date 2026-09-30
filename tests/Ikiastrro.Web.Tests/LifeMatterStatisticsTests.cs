using Ikiastrro.Data;
using Ikiastrro.Data.Statistics;

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
    [InlineData(1.0, StrengthBand.Strong)]   // one SD above the chart's own house mean
    [InlineData(0.99, StrengthBand.Middle)]
    [InlineData(-1.0, StrengthBand.Middle)]
    [InlineData(-1.01, StrengthBand.Weak)]
    public void IndependentBhavaBand_IsOneStandardDeviationEitherSide(double z, StrengthBand expected) =>
        Assert.Equal(expected, LifeMatterStatistics.IndependentBhavaBand(z));

    [Theory]
    [InlineData(5, StrengthBand.Strong)]     // PVR: 5+ good, 3 or fewer bad
    [InlineData(4, StrengthBand.Middle)]
    [InlineData(3, StrengthBand.Weak)]
    public void BavBand_UsesTheCitedFivePlusThreeOrFewerRule(int bindus, StrengthBand expected) =>
        Assert.Equal(expected, LifeMatterStatistics.BavBand(bindus));

    [Theory]
    [InlineData(100.0, StrengthBand.Strong)]   // Parāśara's required minimum (SRC_BPHS_27)
    [InlineData(99.9, StrengthBand.Middle)]
    [InlineData(80.0, StrengthBand.Middle)]
    [InlineData(79.9, StrengthBand.Weak)]
    public void ShadbalaBand_MatchesPlanetStrengthChart(double percent, StrengthBand expected) =>
        Assert.Equal(expected, LifeMatterStatistics.ShadbalaBand((decimal)percent));

    [Fact]
    public void MissingStatistics_ReadAsNone()
    {
        var stats = Build().ForSign("Gemini"); // lord Mercury has no Ṣaḍbala row in this fixture

        Assert.Null(stats.SavBindus);
        Assert.All(stats.Bands, b => Assert.Equal(StrengthBand.None, b));
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
        Assert.Null(d9.IndependentBhavaZ);
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

        var facts = ArgalaFacts.Live("D10", "Aries", grahas);
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

    [Theory]
    [InlineData(28, 50)]   // the average SAV sits at the middle
    [InlineData(56, 100)]
    [InlineData(17, 30)]
    public void SavIndex_IsBindusOutOf56(int bindus, int expected) =>
        Assert.Equal(expected, LifeMatterStatistics.SavIndex(bindus));

    [Theory]
    [InlineData(100.0, 50)] // exactly the required minimum is the middle
    [InlineData(250.0, 100)] // capped
    public void ShadbalaIndex_IsPercentOfMinimumOutOf200(double percent, int expected) =>
        Assert.Equal(expected, LifeMatterStatistics.ShadbalaIndex((decimal)percent));

    // Independent Bhava Bala (Dig + Drik) per house: houses 1-6 at 1.0 Rupas, 7-12 at 0.0 — mean
    // 0.5, SD 0.5, so every house sits exactly one SD either side.
    private static IEnumerable<BhavaBalaComponentRow> Components()
    {
        for (var h = 1; h <= 12; h++)
        {
            var rupas = h <= 6 ? 1.0m : 0.0m;
            yield return new BhavaBalaComponentRow((byte)h, "BHAVADHIPATI_BALA", 400m);   // excluded
            yield return new BhavaBalaComponentRow((byte)h, "BHAVA_DIG_BALA", rupas * 60m);
            yield return new BhavaBalaComponentRow((byte)h, "BHAVA_DRIK_BALA", 0m);
        }
    }

    private static LifeMatterStatistics BuildFull() => new("D1", "Aries",
        [new AshtakavargaRow("D1", "VENUS", 7, 7, 5, 28), new AshtakavargaRow("D1", "SUN", 7, 7, 2, 28)],
        [new BhavaBalaSummaryRow(7, "Libra", "Venus", 1, "Aries", "Enemy", 384.5m, 6.41m)],
        [
            new ShadbalaSummaryRow("Venus", 0, 0, 0, 0, 0, 0, 0, 384m, 6.40m, 5.5m, 100m),
            new ShadbalaSummaryRow("Jupiter", 0, 0, 0, 0, 0, 0, 0, 400m, 6.66m, 6.5m, 200m),
        ],
        [],
        Components(),
        [
            new AmsabalaRow("VENUS", "SHODASAVARGA", 16, 4, null, ""),
            new AmsabalaRow("VENUS", "SHADVARGA", 6, 6, null, ""),     // other schemes are ignored
            new AmsabalaRow("JUPITER", "SHODASAVARGA", 16, 12, null, ""),
        ]);

    [Fact]
    public void BhavaBala_ExcludesTheLordsShadbalaAndIsScoredAgainstTheChartsOwnHouses()
    {
        var stats = BuildFull();

        var libra = stats.ForSign("Libra");     // house 7: one SD below the mean
        Assert.Equal(0m, libra.IndependentBhavaRupas);
        Assert.Equal(-1.0, libra.IndependentBhavaZ!.Value, 6);
        Assert.Equal(40, LifeMatterStatistics.IndependentBhavaIndex(libra.IndependentBhavaZ));
        Assert.Equal(6.41m, libra.BhavaBalaRupas);   // the raw total is kept for display only

        var aries = stats.ForSign("Aries");     // house 1: one SD above
        Assert.Equal(StrengthBand.Strong, aries.BhavaBand);
        Assert.Equal(60, LifeMatterStatistics.IndependentBhavaIndex(aries.IndependentBhavaZ));
    }

    [Fact]
    public void Axes_CapacityAndConsistencyReadTheLordAndKarakas_ContextReadsTheSign()
    {
        var libra = BuildFull().ForSign("Libra", ["Jupiter", "Venus"]);   // Venus is the lord and a kāraka: counted once

        Assert.Equal(5, libra.LordBavBindus);          // Venus's own BAV in Libra, not the Sun's
        Assert.Equal(25, libra.LordAmsabalaPercent);   // 4 of 16 Shodasavarga
        Assert.Equal(["Venus", "Jupiter"], libra.Planets.Select(p => p.Planet));
        Assert.Equal(75, libra.Capacity);              // Venus 100% → 50, Jupiter 200% → 100
        Assert.Equal(50, libra.Consistency);           // 25% and 75%
        Assert.Equal([50, 63, 40, null], libra.ContextParts);   // SAV 28, BAV 5, z −1, no Argala
        Assert.Equal(51, libra.Context);
        Assert.Equal(59, libra.StrengthPercent);       // mean of 75, 50, 51 — each axis counts once
    }

    [Fact]
    public void StrengthPercent_AveragesOnlyTheReadableAxes()
    {
        var libra = Build().ForSign("Libra"); // no Amsabala or Bhava components in this fixture

        Assert.Null(libra.Consistency);
        Assert.Equal(58, libra.Capacity);     // Venus 116.35% → 58
        Assert.Equal(30, libra.Context);      // SAV 17 → 30; no BAV for Venus, no Bhava components, no Argala
        Assert.Equal(44, libra.StrengthPercent);
        Assert.Null(Build().ForSign("Gemini").StrengthPercent);
    }

    [Fact]
    public void ArgalaIndex_IsTheShareOfPairsThatHold()
    {
        // House 1: Moon on the 2nd holds (nothing on the 12th); Jupiter on the 4th is obstructed by Saturn + Rahu on the 10th.
        var stats = Build(
        [
            Fact(1, "ARGALA", 2, "Moon"),
            Fact(1, "ARGALA", 4, "Jupiter"),
            Fact(1, "VIRODHARGALA", 10, "Saturn"),
            Fact(1, "VIRODHARGALA", 10, "Rahu"),
        ]).ForSign("Aries");

        Assert.Equal(50, LifeMatterStatistics.ArgalaIndex(stats.Argala));
    }
}
