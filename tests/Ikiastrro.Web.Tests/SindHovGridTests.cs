using Bunit;
using Ikiastrro.Web.Components.Charts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Ikiastrro.Web.Tests;

public sealed class SindHovGridTests : BunitContext
{
    [Theory]
    [InlineData("Aries", 7, "Libra")]
    [InlineData("Scorpio", 7, "Taurus")]
    public void HouseFocus_IsTranslatedAgainstDisplayedChartsOwnAscendant(
        string ascendant,
        int house,
        string expectedSign)
    {
        var cut = Render<SindHovGrid>(parameters => parameters
            .Add(p => p.AscendantSign, ascendant)
            .Add(p => p.HouseFoci, [new(ascendant, house)]));

        var expectedCell = cut.Find($"[data-sign='{expectedSign}']");
        Assert.Contains("relevant", expectedCell.ClassList);
        Assert.Equal(house.ToString(), expectedCell.GetAttribute("data-house"));
    }

    [Fact]
    public void CanonicalSpecialPointFocus_HighlightsContainingSign()
    {
        var points = new Dictionary<string, IReadOnlyList<string>>
        {
            ["Pisces"] = ["A12"]
        };

        var cut = Render<SindHovGrid>(parameters => parameters
            .Add(p => p.AscendantSign, "Aries")
            .Add(p => p.SpecialPointsBySign, points)
            .Add(p => p.SpecialPointFocusCodes, ["A12"]));

        Assert.Contains("relevant", cut.Find("[data-sign='Pisces']").ClassList);
    }

    [Fact]
    public void HoverPreview_DoesNotReplacePinnedSelection()
    {
        string? preview = null;
        var cut = Render<SindHovGrid>(parameters => parameters
            .Add(p => p.AscendantSign, "Aries")
            .Add(p => p.PinnedSign, "Libra")
            .Add(p => p.PreviewSignChanged,
                EventCallback.Factory.Create<string?>(this, value => preview = value)));

        cut.Find("[data-sign='Taurus']").MouseEnter();

        Assert.Equal("Taurus", preview);
        Assert.Contains("pinned", cut.Find("[data-sign='Libra']").ClassList);

        cut.Find("[data-sign='Taurus']").MouseLeave();
        Assert.Null(preview);
        Assert.Contains("pinned", cut.Find("[data-sign='Libra']").ClassList);
    }

    [Fact]
    public void EnterAndSpace_PinKeyboardFocusedCell()
    {
        string? pinned = null;
        var cut = Render<SindHovGrid>(parameters => parameters
            .Add(p => p.AscendantSign, "Aries")
            .Add(p => p.PinnedSignChanged,
                EventCallback.Factory.Create<string?>(this, value => pinned = value)));

        var cell = cut.Find("[data-sign='Cancer']");
        cell.KeyDown(new KeyboardEventArgs { Key = "Enter" });
        Assert.Equal("Cancer", pinned);

        cell.KeyDown(new KeyboardEventArgs { Key = " " });
        Assert.Equal("Cancer", pinned);
    }

    [Fact]
    public void Escape_ClearsPreviewFirstThenPin()
    {
        string? preview = "unset";
        string? pinned = "unset";
        var cut = Render<SindHovGrid>(parameters => parameters
            .Add(p => p.AscendantSign, "Aries")
            .Add(p => p.PinnedSign, "Libra")
            .Add(p => p.PreviewSignChanged, EventCallback.Factory.Create<string?>(this, v => preview = v))
            .Add(p => p.PinnedSignChanged, EventCallback.Factory.Create<string?>(this, v => pinned = v)));

        cut.Find("[data-sign='Taurus']").MouseEnter();
        Assert.Equal("Taurus", preview);

        var grid = cut.Find("[role='grid']");
        grid.KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.Null(preview);
        Assert.Equal("unset", pinned);

        grid.KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.Null(pinned);
    }

    [Fact]
    public void EverySignCell_IsAKeyboardOperableGridCellWithNonColorMarker()
    {
        var cut = Render<SindHovGrid>(parameters => parameters
            .Add(p => p.AscendantSign, "Aries")
            .Add(p => p.HouseFoci, [new("Aries", 7)]));

        Assert.Equal(12, cut.FindAll("button[role='gridcell']").Count);
        Assert.NotNull(cut.Find("[data-sign='Libra'] .selection-mark"));
        Assert.Equal("South Indian horoscope grid", cut.Find("[role='grid']").GetAttribute("aria-label"));
    }
}
