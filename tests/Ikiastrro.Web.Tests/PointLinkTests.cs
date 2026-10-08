using Bunit;
using Ikiastrro.Web.Components.Charts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class PointLinkTests : BunitContext
{
    private static readonly PointFocus Gulika = new("Capricornus", "Gulika");
    private static readonly PointFocus Mandi = new("Aries", "Māndi");

    [Fact]
    public void State_HoverPreviewsPinSticksAndClearResets()
    {
        var s = new PointLinkState();
        var changes = 0;
        s.Changed += () => changes++;

        s.TogglePin(Gulika);
        Assert.Equal(Gulika, s.Current);

        s.SetHover(Mandi);                      // hover wins over the pin while it lasts
        Assert.Equal(Mandi, s.Current);
        s.SetHover(null);
        Assert.Equal(Gulika, s.Current);        // and falls back to the pin

        s.TogglePin(Gulika);                    // same row again unpins
        Assert.Null(s.Current);

        s.SetHover(Mandi); s.Clear();
        Assert.Null(s.Current);
        Assert.True(changes >= 5);
    }

    [Fact]
    public void SignOf_UsesTheInternalZodiacSpelling() =>
        Assert.Equal("Capricornus", PointFocus.SignOf(285.5));

    private IRenderedComponent<LinkedRow> Row(PointLinkState state, PointFocus focus) =>
        Render<LinkedRow>(p => p
            .AddCascadingValue(state)
            .Add(x => x.Focus, focus)
            .Add(x => x.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0, "<td>Gulika</td>"))));

    [Fact]
    public void Row_HoverMarksItLinkedAndClickPinsIt()
    {
        var state = new PointLinkState();
        var cut = Row(state, Gulika);
        var tr = cut.Find("tr");

        tr.MouseEnter();
        Assert.Contains("is-linked", cut.Find("tr").ClassList);
        Assert.DoesNotContain("is-pinned", cut.Find("tr").ClassList);

        cut.Find("tr").Click();
        Assert.Contains("is-pinned", cut.Find("tr").ClassList);
        Assert.Equal("true", cut.Find("tr").GetAttribute("aria-pressed"));

        cut.Find("tr").MouseLeave();
        Assert.Contains("is-linked", cut.Find("tr").ClassList);   // still shown: it is pinned
    }

    [Fact]
    public void Row_KeyboardEnterPins_AndAnotherRowStaysQuiet()
    {
        var state = new PointLinkState();
        var a = Row(state, Gulika);
        var b = Row(state, Mandi);

        a.Find("tr").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal(Gulika, state.Pinned);
        Assert.Contains("is-pinned", a.Find("tr").ClassList);
        Assert.DoesNotContain("is-linked", b.Find("tr").ClassList);
    }

    [Fact]
    public void Row_WithoutAStateRendersAsAPlainRow()
    {
        var cut = Render<LinkedRow>(p => p
            .Add(x => x.Focus, Gulika)
            .Add(x => x.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0, "<td>x</td>"))));
        Assert.DoesNotContain("pl-row", cut.Find("tr").ClassList);
        Assert.Null(cut.Find("tr").GetAttribute("tabindex"));
    }
}

public sealed class SindUniDtlGridHighlightTests : BunitContext
{
    [Fact]
    public void HighlightSign_MarksThatCellAndPrintsTheLabel()
    {
        var cut = Render<SindUniDtlGrid>(ps => ps
            .Add(p => p.AscendantSign, "Aries")
            .Add(p => p.PlanetsBySign, ChartFixture.GridGlyphs)
            .Add(p => p.CenterTitle, "D1")
            .Add(p => p.CenterMeta, "<span>x</span>")
            .Add(p => p.HighlightSign, "Capricornus")
            .Add(p => p.HighlightLabel, "Gulika"));

        var linked = cut.FindAll(".cell.link-hl");
        Assert.Single(linked);
        Assert.Equal("Capricornus", linked[0].GetAttribute("data-sign"));
        Assert.Contains("Gulika", linked[0].QuerySelector(".link-label")!.TextContent);
    }

    [Fact]
    public void NoHighlightSign_MarksNothing()
    {
        var cut = Render<SindUniDtlGrid>(ps => ps
            .Add(p => p.AscendantSign, "Aries")
            .Add(p => p.PlanetsBySign, ChartFixture.GridGlyphs)
            .Add(p => p.CenterTitle, "D1")
            .Add(p => p.CenterMeta, "<span>x</span>"));
        Assert.Empty(cut.FindAll(".cell.link-hl"));
    }
}
