using Bunit;
using Microsoft.AspNetCore.Components;
using Ikiastrro.Core.Engines.Astronomy;
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
    public void ShowsBasisCausingPlanetsAndTheirShadbala()
    {
        var rows = new[]
        {
            new YogaEvaluationRow("SRC_RAMAN_300_COMBINATIONS", "YOGA_AYATNA_DHANA_LABHA", true, "EVALUATED", "LAGNA",
                "Lagna lord and 2nd lord exchange houses", null),
        };
        var lords = new Dictionary<int, PlanetName> { [1] = PlanetName.Mars, [2] = PlanetName.Venus };
        var shadbala = new[]
        {
            new ShadbalaSummaryRow("Mars", 0, 0, 0, 0, 0, 0, 0, 0, 6.5m, 5m, 130m),
            new ShadbalaSummaryRow("Venus", 0, 0, 0, 0, 0, 0, 0, 0, 3m, 5.5m, 55m),
        };

        var cut = Render<YogaEvaluationTable>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.HouseLords, lords)
            .Add(x => x.Shadbala, shadbala));

        Assert.Contains("Lagna based", cut.Markup);
        Assert.Equal(new[] { "Mars", "Venus" }, cut.FindAll(".yg-pstrength b").Select(b => b.TextContent));
        Assert.Contains("yg-tier-strong", cut.Markup);
        Assert.Contains("yg-tier-weak", cut.Markup);
        Assert.Contains("130%", cut.Markup);
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
    public void VargaSelectorMarksTheSelectedChartAndRaisesTheChange()
    {
        var rows = new[] { new YogaEvaluationRow("SRC_PVR_INTEGRATED", "YOGA_RUCHAKA", true, "EVALUATED", "LAGNA", "Mars own in a kendra", null, ChartType: "D9") };
        string? picked = null;
        var cut = Render<YogaEvaluationTable>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.Varga, "D9")
            .Add(x => x.Vargas, new[] { "D1", "D2", "D3", "D9", "D12", "D30" })
            .Add(x => x.OnVargaChanged, EventCallback.Factory.Create<string>(this, v => picked = v)));

        var buttons = cut.FindAll(".yg-varga");
        Assert.Equal(6, buttons.Count);
        Assert.Equal("D9 Navamsa", cut.Find(".yg-varga.is-on").Normalized());
        Assert.Contains("Ṣaḍbala is the natal (D1) strength", cut.Find(".yg-varga-note").Normalized());

        buttons.Single(b => b.TextContent.Contains("D12")).Click();
        Assert.Equal("D12", picked);
    }

    [Fact]
    public void ShowsTheVargaOfEachRowAndHidesTheSelectorWithASingleChart()
    {
        var rows = new[] { new YogaEvaluationRow("SRC_PVR_INTEGRATED", "YOGA_RUCHAKA", true, "EVALUATED", "LAGNA", "Mars own in a kendra", null, ChartType: "D30") };
        var withSelector = Render<YogaEvaluationTable>(p => p.Add(x => x.Rows, rows).Add(x => x.Varga, "D30")
            .Add(x => x.Vargas, new[] { "D1", "D30" }));
        Assert.Equal("D30", withSelector.Find(".yg-varga-cell").Normalized());

        var plain = Render<YogaEvaluationTable>(p => p.Add(x => x.Rows, rows));
        Assert.Empty(plain.FindAll(".yg-varga"));
    }

    [Fact]
    public void VargaPlacementsDriveTheCausingPlanetsAndNaturalShadbalaStays()
    {
        // Mars is the lagna lord in the selected varga; the Ṣaḍbala column still reads the natal strength.
        var rows = new[] { new YogaEvaluationRow("SRC_RAMAN_300_COMBINATIONS", "YOGA_X", true, "EVALUATED", "LAGNA",
            "Lagna lord and 2nd lord exchange houses", null, ChartType: "D9") };
        var cut = Render<YogaEvaluationTable>(p => p.Add(x => x.Rows, rows).Add(x => x.Varga, "D9")
            .Add(x => x.Vargas, new[] { "D1", "D9" })
            .Add(x => x.HouseLords, new Dictionary<int, PlanetName> { [1] = PlanetName.Mars, [2] = PlanetName.Venus })
            .Add(x => x.Shadbala, new[] { new ShadbalaSummaryRow("Mars", 0, 0, 0, 0, 0, 0, 0, 0, 6.5m, 5m, 130m) }));
        Assert.Equal(new[] { "Mars", "Venus" }, cut.FindAll(".yg-pstrength b").Select(b => b.TextContent));
        Assert.Contains("130%", cut.Markup);
    }
}
