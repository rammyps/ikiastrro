using Bunit;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class YogaEvaluationTableTests : BunitContext
{
    [Fact]
    public void RendersNotesOnlyWhenPresent()
    {
        var rows = new[]
        {
            new YogaEvaluationRow("SRC_PVR_INTEGRATED", "YOGA_BUDHA_ADITYA", true, "EVALUATED", "SUN",
                "Sun and Mercury conjunct (same sign)", "Present, but Mercury is combust."),
            new YogaEvaluationRow("SRC_RAMAN_300_COMBINATIONS", "YOGA_GAJAKESARI", true, "EVALUATED", "MOON",
                "Jupiter in a kendra from Moon", null),
        };

        var cut = Render<YogaEvaluationTable>(p => p.Add(x => x.Rows, rows));

        Assert.Contains("Present, but Mercury is combust.", cut.Markup);
        Assert.Single(cut.FindAll(".yg-notes"));
    }

    [Fact]
    public void RendersLagnaLordOnlyForLagnaTypedRows()
    {
        var rows = new[]
        {
            new YogaEvaluationRow("SRC_PVR_INTEGRATED", "YOGA_SUBHA", true, "EVALUATED", "LAGNA",
                "Lagna occupied by a natural benefic", null),
            new YogaEvaluationRow("SRC_RAMAN_300_COMBINATIONS", "YOGA_GAJAKESARI", true, "EVALUATED", "MOON",
                "Jupiter in a kendra from Moon", null),
        };

        var cut = Render<YogaEvaluationTable>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.LagnaLordPlanet, "Mercury"));

        Assert.Contains("Lagna lord: Mercury", cut.Markup);
        Assert.Single(cut.FindAll(".yg-lagnalord"));
    }

    [Fact]
    public void ResolvesInterpretationBySourceThenFallsBackToGeneric()
    {
        var rows = new[]
        {
            new YogaEvaluationRow("SRC_RAMAN_300_COMBINATIONS", "YOGA_BUDHA_ADITYA", true, "EVALUATED", "SUN",
                "Sun and Mercury conjunct, more than 10 degrees apart", null),
            new YogaEvaluationRow("SRC_PVR_INTEGRATED", "YOGA_GAJAKESARI", true, "EVALUATED", "MOON",
                "Jupiter in a kendra from Moon", null),
        };
        var interpretations = new[]
        {
            new InterpretationRow(1, "YOGA", "YOGA_BUDHA_ADITYA", "SRC_RAMAN_300_COMBINATIONS", "Raman-specific standard text.", "Raman short."),
            new InterpretationRow(2, "YOGA", "YOGA_GAJAKESARI", null, "Generic fallback standard text.", "Generic short."),
        };

        var cut = Render<YogaEvaluationTable>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.Interpretations, interpretations));

        Assert.Contains("Raman-specific standard text.", cut.Markup);
        Assert.Contains("Generic fallback standard text.", cut.Markup);
    }

    [Fact]
    public void EditConfirmSaveRoundTripsThroughCallbackWithoutTouchingARepository()
    {
        var rows = new[]
        {
            new YogaEvaluationRow("SRC_PVR_INTEGRATED", "YOGA_BUDHA_ADITYA", true, "EVALUATED", "SUN",
                "Sun and Mercury conjunct (same sign)", null),
        };
        YogaEvaluationTable.InterpretationEditRequest? saved = null;

        var cut = Render<YogaEvaluationTable>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.OnSaveInterpretation, (YogaEvaluationTable.InterpretationEditRequest r) => saved = r));

        cut.Find(".edit-icon-btn").Click();
        cut.FindAll("textarea")[0].Input("A conservative interpretation.");
        cut.FindAll("textarea")[1].Input("Short form.");
        cut.Find(".yg-edit-actions .btn-confirm").Click();

        // Save is confirm-gated: the write hasn't happened yet, the ConfirmDialog is now open.
        Assert.Null(saved);
        Assert.Contains("Save this interpretation for", cut.Markup);

        cut.Find(".dialog-actions .btn-confirm").Click();

        Assert.NotNull(saved);
        Assert.Equal("YOGA_BUDHA_ADITYA", saved!.SubjectCode);
        Assert.Equal("SRC_PVR_INTEGRATED", saved.SourceRefCode);
        Assert.Equal("A conservative interpretation.", saved.StandardText);
        Assert.Equal("Short form.", saved.ShortText);
        // Optimistic local update shows the new text immediately, without waiting on the page to re-fetch.
        Assert.Contains("A conservative interpretation.", cut.Markup);
    }

    [Fact]
    public void RendersSevenRankedLifeMatterPathsForTheMatchingVariant()
    {
        var rows = new[]
        {
            new YogaEvaluationRow("SRC_PVR_INTEGRATED", "YOGA_DHANA", true, "EVALUATED", "LAGNA",
                "Wealth combination", null, "PVR_CH11_DHANA_ARIES")
        };
        var paths = Enumerable.Range(1, 7).Select(rank => new YogaLifeMatterPathRow(
            "SRC_PVR_INTEGRATED", "PVR_CH11_DHANA_ARIES", "YOGA_DHANA", (byte)rank,
            "Wealth and financial matters", $"Wealth path {rank}", "Wealth", "House",
            "LAGNA", "House 2", "Jupiter", .8m - rank / 100m, "PROPOSED")).ToArray();

        var cut = Render<YogaEvaluationTable>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.LifeMatterPaths, paths));

        Assert.Contains("7 ranked paths", cut.Markup);
        Assert.Equal(7, cut.FindAll(".yg-path").Count);
        Assert.Contains("Wealth path 1", cut.Markup);
        Assert.Contains("Jupiter", cut.Markup);
        Assert.DoesNotContain("Wealth path 8", cut.Markup);
    }
}
