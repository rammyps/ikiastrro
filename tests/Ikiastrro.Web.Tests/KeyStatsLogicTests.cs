using Bunit;
using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Data;
using Ikiastrro.Data.Statistics;
using Ikiastrro.Web.Components.LifeMatters;
using Ikiastrro.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class KeyStatsLogicTests : BunitContext
{
    private static PopulationComparison Row(byte house, decimal? percentile, decimal? z, string sufficiency = "SUFFICIENT") =>
        new(house, "KI_D1_HOUSE_SUPPORT_V2", 50m, 48, 48, 0m, 50m, 40m, 60m, z, percentile, 47m, 53m, sufficiency);

    private static PopulationEvidenceSnapshot Snap(string code, int eligible, long? run) =>
        new(code, "ELIGIBLE", "RESEARCH", eligible, run, "D", 1, "v2", 1, null, []);

    [Fact]
    public void Filter_NotableIsAbsoluteZOfAtLeastOne_ThinIsInsufficientOrIncomplete()
    {
        PopulationComparison[] rows =
        [
            Row(1, 14m, -1.4m), Row(2, 50m, 0.2m), Row(3, 96m, 2.3m), Row(4, null, null, "INSUFFICIENT"), Row(5, 55m, 1.0m, "INCOMPLETE")
        ];
        Assert.Equal([1, 3, 5], KeyStatsLogic.Filter(rows, "notable").Select(r => (int)r.HouseFromLagna));
        Assert.Equal([4, 5], KeyStatsLogic.Filter(rows, "thin").Select(r => (int)r.HouseFromLagna));
        Assert.Equal(5, KeyStatsLogic.Filter(rows, "all").Count());
    }

    [Fact]
    public void Extreme_IgnoresThinAndUnpublishedHouses()
    {
        PopulationComparison[] rows = [Row(1, 14m, -1m), Row(2, 99m, 2m, "INSUFFICIENT"), Row(3, 80m, 1m), Row(4, null, null)];
        Assert.Equal(3, KeyStatsLogic.Extreme(rows, highest: true)!.HouseFromLagna);
        Assert.Equal(1, KeyStatsLogic.Extreme(rows, highest: false)!.HouseFromLagna);
        Assert.Null(KeyStatsLogic.Extreme([Row(2, 99m, 2m, "INSUFFICIENT")], highest: true));
    }

    [Fact]
    public void UnlockSteps_RunStepOnlyCountsOnceTheCohortIsLargeEnough()
    {
        var notEnrolled = KeyStatsLogic.UnlockSteps(Snap("NOT_ENROLLED", 0, 7));
        Assert.Equal([false, false, false], notEnrolled.Select(s => s.Done));

        var small = KeyStatsLogic.UnlockSteps(Snap("NO_COMPARISONS", 12, 7));
        Assert.Equal([true, false, false], small.Select(s => s.Done));
        Assert.Contains("now 12", small[1].Text);

        var ready = KeyStatsLogic.UnlockSteps(Snap("NO_COMPARISONS", 30, 7));
        Assert.Equal([true, true, true], ready.Select(s => s.Done));
        Assert.False(KeyStatsLogic.UnlockSteps(Snap("NO_COMPARISONS", 30, null))[2].Done);
    }

    private static LifeMatterStepRow Step(int id, string code, string category) =>
        new(id, code, category, category, id, code, "", "", "", "", null);

    private static LifeMatterFocusRule House(int id, int matter, int house, int priority, string? reference = null, bool active = true) =>
        new(id, 1, matter, LifeMatterFocusKind.House, reference, house, null, priority, active);

    [Fact]
    public void BestMatterByHouse_PrefersPrimaryThenMostSpecificNotAlphabeticalCategory()
    {
        // CAREER sorts before SELF alphabetically and lists house 1 as a secondary house.
        LifeMatterStepRow[] steps = [Step(1, "CAREER_01", "CAREER"), Step(2, "SELF_01", "SELF"), Step(3, "SELF_02", "SELF"), Step(4, "SELF_03", "SELF")];
        LifeMatterFocusRule[] foci =
        [
            House(1, 1, 10, 1), House(2, 1, 1, 2),                 // career: primary 10th, secondary 1st
            House(3, 2, 1, 1), House(9, 4, 1, 1),                  // self 01 and 03: only the 1st; ties go to the earlier question
            House(4, 3, 1, 1), House(5, 3, 6, 2), House(6, 3, 8, 3), // self 02: primary 1st but reads from 3 houses
            House(7, 2, 7, 1, "CHANDRA_LAGNA"),                    // not Lagna-counted: ignored
            House(8, 1, 12, 1, active: false)                      // inactive: ignored
        ];

        var best = KeyStatsLogic.BestMatterByHouse(steps, foci,
            [new LifeMatterCategoryRow("SELF", "Self", 1), new LifeMatterCategoryRow("CAREER", "Career", 2)]);

        Assert.Equal("SELF_01", best[1]);
        Assert.Equal("CAREER_01", best[10]);
        Assert.Equal("SELF_02", best[6]);
        Assert.False(best.ContainsKey(7));
        Assert.False(best.ContainsKey(12));
    }

    [Fact]
    public void BestMatterByHouse_TiesFollowTheAppAreaOrderNotTheAlphabet()
    {
        LifeMatterStepRow[] steps = [Step(1, "CAREER_01", "CAREER"), Step(2, "SELF_01", "SELF")];
        LifeMatterFocusRule[] foci = [House(1, 1, 1, 1), House(2, 2, 1, 1)];

        var best = KeyStatsLogic.BestMatterByHouse(steps, foci,
            [new LifeMatterCategoryRow("SELF", "Self", 1), new LifeMatterCategoryRow("CAREER", "Career", 2)]);

        Assert.Equal("SELF_01", best[1]);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("stats", "STATS")]
    [InlineData("Population", "STATS")]
    [InlineData("TIMING", "STATISTICAL")]
    [InlineData("reading", "READING")]
    [InlineData("nonsense", "READING")]
    public void KeyInferenceView_Parse(string? input, string? expected) =>
        Assert.Equal(expected, KeyInferenceView.Parse(input));

    [Fact]
    public void LegacyKeyStatsRoute_RedirectsToThePopulationView()
    {
        Render<KeyStats>(p => p.Add(x => x.Id, 3));
        Assert.EndsWith("/key-inference/3?view=stats", Services.GetRequiredService<NavigationManager>().Uri);
    }

    private static LifeMatterPopulationComparison Matter(string code, int focus, string lens, string feature = "KI_LM_SUPPORT_V1",
        decimal? pct = 60m, string sufficiency = "SUFFICIENT") =>
        new(1, focus, code, code, lens, "D9", null, feature, 50m, 48, 48, 0m, 50m, 40m, 60m, 0m, pct, 47m, 53m, sufficiency, null);

    [Fact]
    public void MatterGroups_KeepOnlyOverallSupport_PromiseBeforeVarga_ThinRowsKept()
    {
        LifeMatterPopulationComparison[] rows =
        [
            Matter("MARRIAGE", 2, "VARGA_CONFIRMATION"), Matter("MARRIAGE", 1, "D1_PROMISE"),
            Matter("MARRIAGE", 1, "D1_PROMISE", "KI_LM_CAPACITY_V1"), Matter("CAREER", 3, "D1_PROMISE", pct: null, sufficiency: "INSUFFICIENT")
        ];
        var groups = KeyStatsLogic.MatterGroups(rows);
        Assert.Equal(["MARRIAGE", "CAREER"], groups.Select(g => g.Code));
        Assert.Equal(["D1_PROMISE", "VARGA_CONFIRMATION"], groups[0].Rows.Select(r => r.EvidenceLensCode));
        Assert.True(KeyStatsLogic.IsThin(groups[1].Rows[0]));
    }

    [Fact]
    public void Reliability_NamesMeasuredCountAndMedianInterval()
    {
        Assert.Equal("48 of 48 measured · median 50% (95% CI 47–53)", KeyStatsLogic.Reliability(Matter("M", 1, "D1_PROMISE")));
    }

    private static DashaMatterPopulationComparison Dasha(int rule, string scope, string feature, decimal? personal, decimal? mean,
        string sufficiency = "SUFFICIENT") =>
        new(rule, "D9", scope, null, null, feature, personal, 48, 48, 0m, 1m, mean, null, 0m, 1m, sufficiency, null);

    [Fact]
    public void DashaGroups_UseCoreWording_GroupByScope_AndShowPrevalence()
    {
        DashaMatterPopulationComparison[] rows =
        [
            Dasha(5, "EXAMPLE", "KI_DM_TARGET_COUNT_V1", 1m, 1m), Dasha(5, "EXAMPLE", "KI_DM_PRESENT_V1", 100m, 62.5m),
            Dasha(10, "CHART_THEME", "KI_DM_TARGET_COUNT_V1", null, 1m, "INCOMPLETE"),
            Dasha(10, "CHART_THEME", "KI_DM_PRESENT_V1", null, 40m, "INCOMPLETE")
        ];
        var groups = KeyStatsLogic.DashaGroups(rows);
        Assert.Equal(["PVR examples", "Divisional chart themes"], groups.Select(g => g.Heading));
        var seventh = groups[0].Items.Single();
        Assert.Equal("The 7th lord in D-9", seventh.Statement);
        Assert.Equal(1m, seventh.PlanetCount);
        Assert.Equal(62.5m, seventh.PresentPercent);
        var theme = groups[1].Items.Single();
        Assert.Null(theme.PlanetCount);
        Assert.True(theme.IsThin);
    }
}
