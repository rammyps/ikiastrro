using Bunit;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.DivisionalCharts;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts;
using Ikiastrro.Web.Components.Charts.SindUni;
using Ikiastrro.Web.Components.Workspace;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>The Spl Lagnas chart's special points: the catalogue (upagrahas, Bhṛgu Bindu/Varṇada, 14 sphuṭas, 36 sahams,
/// projected onto any varga), the ChartMenu dropdown, and both renderers (South Indian grid chips, wheel ring).</summary>
public sealed class SpecialPointsChartTests : BunitContext
{
    private static ChartKeyDetail Point(string kind, string name, double lon, string sign = "Aries") => new()
    {
        PointKind = kind, Planet = name, NirayanaLongitudeDegrees = lon, Sign = sign,
    };

    // 1_Ramakrishnan's D1 (JHora's longitudes) with Gulika, Māndi (the point JHora calls Gulika), Dhūma and Hora Lagna.
    private static readonly ChartKeyDetail[] D1 =
    [
        Point("Graha", "Ascendant", 0.6574), Point("Graha", "Sun", 8.2019), Point("Graha", "Moon", 217.2088, "Scorpio"),
        Point("Graha", "Mars", 3.9433), Point("Graha", "Mercury", 1.8326), Point("Graha", "Jupiter", 158.7238, "Virgo"),
        Point("Graha", "Venus", 11.9832), Point("Graha", "Saturn", 160.9595, "Virgo"), Point("Graha", "Rahu", 102.9146, "Cancer"),
        Point("Graha", "Ketu", 282.9146, "Capricornus"), Point("Upagraha", "Maandi", 198.1023, "Libra"), Point("Upagraha", "Gulika", 207.4, "Scorpio"),
        Point("Upagraha", "Dhuma", 351.3, "Pisces"), Point("SpecialLagna", "HL", 354.9145, "Pisces"),
    ];

    // ---- catalogue ----

    [Fact]
    public void BuildListsEveryFamily()
    {
        var points = PointCatalog.Build(D1, isNightBirth: true);

        Assert.Equal(3, points.Count(p => p.Group == PointGroup.Upagraha));
        Assert.Equal(13, points.Count(p => p.Group == PointGroup.Sensitive));          // Bhṛgu Bindu + V1–V12
        Assert.Equal(14, points.Count(p => p.Group == PointGroup.Sphuta));
        Assert.Equal(10, points.Count(p => p.Group == PointGroup.SahamKey));
        Assert.Equal(26, points.Count(p => p.Group == PointGroup.Saham));
        Assert.Equal(points.Count, points.Select(p => p.Code).Distinct().Count());     // codes are unique
        Assert.All(PointCatalog.Essentials, code => Assert.Contains(points, p => p.Code == code));
    }

    [Fact]
    public void BuildWithoutTheLagnaOrAGrahaGivesOnlyUpagrahas()
    {
        var points = PointCatalog.Build(D1.Where(k => k.Planet != "Saturn").ToList(), isNightBirth: false);
        Assert.All(points, p => Assert.Equal(PointGroup.Upagraha, p.Group));
    }

    [Fact]
    public void ShortCodesAreUniqueWithinAFamilyOfTheSameColour()
    {
        var points = PointCatalog.Build(D1, true);
        foreach (var group in points.GroupBy(p => PointCatalog.ColorVar(p.Group)))
            Assert.Equal(group.Count(), group.Select(p => p.Short).Distinct().Count());
    }

    [Fact]
    public void PlaceOnD1UsesTheLongitudeAndOnNavamsaTheVargaRule()
    {
        var bb = PointCatalog.Build(D1, true).Single(p => p.Code == "BB");
        var d1 = PointCatalog.Place([bb], null, ZodiacName.Aries).Single();
        Assert.Equal(AstroMath.GetSignAtLongitude(bb.Longitude), d1.Sign);
        Assert.Equal((int)d1.Sign + 1, d1.House);                                       // Aries Lagna

        var d9 = PointCatalog.Place([bb], new NavamsaD9SignRule(), ZodiacName.Taurus).Single();
        Assert.Equal(new NavamsaD9SignRule().SignFor(bb.Longitude), d9.Sign);
        Assert.Equal(((int)d9.Sign - (int)ZodiacName.Taurus + 12) % 12 + 1, d9.House);
    }

    // ---- dropdown ----

    private static readonly IReadOnlyList<ChartMenu.MenuGroup> Groups =
    [
        new("Sphuṭas", [new("SP_PRANA", "Prana Sphuta", "Pr"), new("SP_DEHA", "Deha Sphuta", "De")]),
        new("Sahams", [new("SH_PUNYA", "Punya saham", "Pu")]),
    ];

    [Fact]
    public void MenuShowsItsCountAndOpensIntoGroupedCheckboxes()
    {
        var cut = Render<ChartMenu>(p => p.Add(x => x.Label, "Points").Add(x => x.Groups, Groups)
            .Add(x => x.Selected, new HashSet<string> { "SP_PRANA", "OTHER" }));

        Assert.Equal("1", cut.Find(".cm-count").TextContent.Trim());                    // OTHER is not in this menu
        Assert.Empty(cut.FindAll(".cm-panel"));
        cut.Find(".cm-btn").Click();
        Assert.Equal(3, cut.FindAll(".cm-item").Count);
        Assert.Equal(2, cut.FindAll(".cm-group").Count);
        Assert.Contains("is-on", cut.FindAll(".cm-item")[0].ClassList);
    }

    [Fact]
    public void MenuTogglesAndPresetsKeepSelectionsFromOtherMenus()
    {
        IReadOnlySet<string>? latest = null;
        var cut = Render<ChartMenu>(p => p.Add(x => x.Label, "Points").Add(x => x.Groups, Groups)
            .Add(x => x.Presets, [new("Pair", new HashSet<string> { "SP_PRANA", "SP_DEHA", "NOT_HERE" })])
            .Add(x => x.Selected, new HashSet<string> { "KEEP" })
            .Add(x => x.SelectedChanged, s => latest = s));
        cut.Find(".cm-btn").Click();

        cut.FindAll(".cm-item input")[2].Change(true);
        Assert.Equal(new[] { "KEEP", "SH_PUNYA" }, latest!.Order());

        cut.FindAll(".cm-chip").Single(b => b.TextContent == "Pair").Click();
        Assert.Equal(new[] { "KEEP", "SP_DEHA", "SP_PRANA" }, latest!.Order());          // NOT_HERE is not in this menu

        cut.FindAll(".cm-chip").Single(b => b.TextContent == "All").Click();
        Assert.Equal(4, latest!.Count);
        cut.FindAll(".cm-chip").Single(b => b.TextContent == "None").Click();
        Assert.Equal(new[] { "KEEP" }, latest!.Order());
    }

    [Fact]
    public void MenuSearchFiltersAndEscapeCloses()
    {
        var cut = Render<ChartMenu>(p => p.Add(x => x.Label, "Points").Add(x => x.Groups, Groups).Add(x => x.Searchable, true));
        cut.Find(".cm-btn").Click();

        cut.Find(".cm-search").Input("deha");
        Assert.Single(cut.FindAll(".cm-item"));
        Assert.Contains("Deha", cut.Find(".cm-item").TextContent);

        cut.Find(".cm").KeyDown("Escape");
        Assert.Empty(cut.FindAll(".cm-panel"));
    }

    // ---- grid ----

    private static LoadedChart Chart() => new(
        "D1", "Rasi · D1", "Rasi", null, "Aries", "Scorpio", "Anuradha",
        [
            new() { Planet = "Ascendant", Sign = "Aries", PointKind = "Graha", DegreesInSignDisplay = "0°38'", NakshatraId = 1, NakshatraPada = 1, HouseNumberFromLagna = 1 },
            new() { Planet = "Sun", Sign = "Aries", PointKind = "Graha", DegreesInSignDisplay = "8°11'", NakshatraId = 1, NakshatraPada = 3, DignityStatus = "Exalted", HouseNumberFromLagna = 1 },
            new() { Planet = "HL", Sign = "Pisces", PointKind = "SpecialLagna" },
        ],
        [], [], [], [], [], null, null, "test");

    private static IReadOnlyList<PlacedPoint> Placed(int inAries, int inGemini)
    {
        PlacedPoint P(string code, ZodiacName sign) =>
            new(new PointDef(code, code, code[^2..], PointGroup.Sphuta, (int)sign * 30 + 5, "test point"), sign, ((int)sign) + 1);
        return Enumerable.Range(0, inAries).Select(i => P($"A{i:00}", ZodiacName.Aries))
            .Concat(Enumerable.Range(0, inGemini).Select(i => P($"G{i:00}", ZodiacName.Gemini))).ToList();
    }

    [Fact]
    public void GridDrawsPointChipsWithAPlusNWhenACellIsFull()
    {
        var cut = Render<SindUni3Grid>(p => p
            .Add(x => x.Chart, SindUniBuilder.Build(Chart())).Add(x => x.ShowSpecialLagnas, true)
            .Add(x => x.ExtraPoints, Placed(inAries: 12, inGemini: 2)));

        Assert.Equal(2, cut.FindAll(".su-cell[data-sign='Gemini'] .tag.xp").Count);
        var aries = cut.FindAll(".su-cell[data-sign='Aries'] .tag.xp");
        Assert.True(aries.Count < 12);                                                    // capped by the planets already in the cell
        Assert.StartsWith("+", aries[^1].TextContent.Trim());
        Assert.Equal(12, aries.Take(aries.Count - 1).Count() + int.Parse(aries[^1].TextContent.Trim()[1..]));
    }

    [Fact]
    public void GridTakesItsLagnasFromTheDropdownAndCanHideItsOwnRow()
    {
        var own = Render<SindUni3Grid>(p => p.Add(x => x.Chart, SindUniBuilder.Build(Chart())).Add(x => x.ShowSpecialLagnas, true));
        Assert.NotEmpty(own.FindAll(".su-slchip"));
        Assert.Single(own.FindAll(".su-cell[data-sign='Pisces'] .bars i"));                // HL on by default

        var driven = Render<SindUni3Grid>(p => p.Add(x => x.Chart, SindUniBuilder.Build(Chart())).Add(x => x.ShowSpecialLagnas, true)
            .Add(x => x.HideLagnaToolbar, true).Add(x => x.ActiveLagnas, new HashSet<string>()));
        Assert.Empty(driven.FindAll(".su-slchip"));
        Assert.Empty(driven.FindAll(".su-cell[data-sign='Pisces'] .bars i"));              // none selected
    }

    [Fact]
    public void PinnedSignCardListsEveryPointHere()
    {
        var cut = Render<SindUni3Grid>(p => p
            .Add(x => x.Chart, SindUniBuilder.Build(Chart())).Add(x => x.ShowSpecialLagnas, true)
            .Add(x => x.ExtraPoints, Placed(inAries: 0, inGemini: 11)).Add(x => x.PinnedSign, ZodiacName.Gemini)
            .Add(x => x.PinnedSignChanged, _ => { }));        // bound = interactive, so the card shows

        Assert.Equal(11, cut.FindAll(".su-center .xp-row").Count);                         // the card lists all, not just the chip cap
    }

    // ---- wheel ----

    private static readonly IReadOnlyList<PolarGridLagnaSelect.Sector> Sectors =
        Enumerable.Range(1, 12).Select(h => new PolarGridLagnaSelect.Sector(h, ((ZodiacName)(h - 1)).ToString())).ToList();

    private static readonly IReadOnlyList<PolarGridLagnaSelect.ReferencePoint> References =
    [
        new("LAGNA", "Sign Lagna", "Aries"), new("ARUDHA_LAGNA", "Arudha Lagna", "Pisces"),
        new("HORA_LAGNA", "Hora Lagna", "Pisces"),
    ];

    [Fact]
    public void WheelDrawsAPointsRingOnlyWhenGivenPoints()
    {
        var without = Render<PolarGridLagnaSelect>(p => p.Add(x => x.Sectors, Sectors).Add(x => x.ReferencePoints, References));
        Assert.Empty(without.FindAll(".pgls-points"));

        var with = Render<PolarGridLagnaSelect>(p => p.Add(x => x.Sectors, Sectors).Add(x => x.ReferencePoints, References)
            .Add(x => x.ExtraPoints, Placed(inAries: 4, inGemini: 30)));
        Assert.Equal(12, with.FindAll(".pgls-points").Count);
        Assert.Equal(4, with.FindAll(".pgls-points")[0].QuerySelectorAll(".pgls-pt").Length);
        var gemini = with.FindAll(".pgls-points")[2].QuerySelectorAll(".pgls-pt");
        Assert.True(gemini.Length < 30);
        Assert.Contains("pgls-pt-more", gemini[^1].ClassName);
    }

    [Fact]
    public void WheelRingsFollowTheDropdownAndItsOwnCheckboxesCanBeHidden()
    {
        var cut = Render<PolarGridLagnaSelect>(p => p.Add(x => x.Sectors, Sectors).Add(x => x.ReferencePoints, References)
            .Add(x => x.HideControls, true).Add(x => x.SelectedReferenceCodes, new HashSet<string> { "LAGNA", "HORA_LAGNA" }));

        Assert.Empty(cut.FindAll(".pgls-lagnas"));
        Assert.Equal(24, cut.FindAll(".pgls-house").Count);                                // two rings x 12 house numbers
    }
}
