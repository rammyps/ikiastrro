using Bunit;
using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;
using Ikiastrro.Web.Components.Charts.SindUni;
using Ikiastrro.Web.Components.Workspace;

namespace Ikiastrro.Web.Tests;

/// <summary>SIND-UNI grid chart (docs/ui/components/spec_SIND-UNI_GridChart.md), on Ramakrishnan's
/// stored D1 (tbl_Chart_KeyDetails, person 4): Aries Lagna with Sun, Mars, Mercury, Venus in it.</summary>
public sealed class SindUniTests : BunitContext
{
    private static ChartKeyDetail G(string planet, string sign, string deg, int nak, int pada, string? dignity,
        bool retro = false, bool combust = false, int house = 1) => new()
    {
        Planet = planet, Sign = sign, PointKind = "Graha", DegreesInSignDisplay = deg,
        NakshatraId = (byte)nak, NakshatraPada = pada, DignityStatus = dignity,
        IsRetrograde = planet == "Ascendant" ? null : retro, IsCombust = combust, HouseNumberFromLagna = house,
    };

    private static LoadedChart Ramakrishnan() => new(
        "D1", "Rasi · D1", "Rasi", null, "Aries", "Scorpio", "Anuradha",
        [
            G("Ascendant", "Aries", "0°38'26\"", 1, 1, null),
            G("Sun", "Aries", "8°11'2\"", 1, 3, "Exalted"),
            G("Moon", "Scorpio", "7°16'38\"", 17, 2, "Debilitated", house: 8),
            G("Mars", "Aries", "3°55'27\"", 1, 2, "Moolatrikona", combust: true),
            G("Mercury", "Aries", "1°48'42\"", 1, 1, "Enemy", combust: true),
            G("Jupiter", "Virgo", "8°42'49\"", 12, 4, "Great Enemy", retro: true, house: 6),
            G("Venus", "Aries", "11°57'52\"", 1, 4, "Enemy", combust: true),
            G("Saturn", "Virgo", "10°56'39\"", 13, 1, "Neutral", retro: true, house: 6),
            G("Rahu", "Cancer", "13°2'37\"", 8, 3, "Neutral", retro: true, house: 4),
            G("Ketu", "Capricornus", "13°2'37\"", 22, 1, "Neutral", retro: true, house: 10),
            new() { Planet = "AL", Sign = "Capricornus", PointKind = "Arudha" },
            new() { Planet = "HL", Sign = "Pisces", PointKind = "SpecialLagna" },
            new() { Planet = "GL", Sign = "Pisces", PointKind = "SpecialLagna" },
        ],
        [], [], [], [], [], null, null, "test");

    [Fact]
    public void Retrograde_planets_and_the_nodes_are_bracketed()
    {
        var chart = SindUniBuilder.Build(Ramakrishnan());
        var label = (string p) => chart.Signs.Values.SelectMany(s => s.Planets).Single(x => x.Planet == p).Label;
        Assert.Equal("SU", label("Sun"));
        Assert.Equal("(JU)", label("Jupiter"));
        Assert.Equal("(RA)", label("Rahu"));
        Assert.Equal("(KE)", label("Ketu"));
    }

    [Fact]
    public void Dignity_status_maps_to_its_own_token()
    {
        var chart = SindUniBuilder.Build(Ramakrishnan());
        var aries = chart[ZodiacName.Aries].Planets.ToDictionary(p => p.Planet, p => p.DignityToken);
        Assert.Equal("exalted", aries["Sun"]);
        Assert.Equal("moolatrikona", aries["Mars"]);
        Assert.Equal("enemy", aries["Venus"]);
    }

    [Fact]
    public void A_sign_holds_nine_padas()
    {
        Assert.Equal([(1, 1), (1, 2), (1, 3), (1, 4), (2, 1), (2, 2), (2, 3), (2, 4), (3, 1)], SindUniGlyphs.PadasIn(ZodiacName.Aries));
        Assert.Equal((27, 4), SindUniGlyphs.PadasIn(ZodiacName.Pisces)[^1]);
    }

    [Fact]
    public void Verdicts_come_from_Ramans_count()
    {
        // Same values Astro Facts → About Houses shows for this chart.
        var chart = SindUniBuilder.Build(Ramakrishnan());
        Assert.Equal("BEN", chart[ZodiacName.Aries].Verdict);
        Assert.Equal("MAL", chart[ZodiacName.Gemini].Verdict);
        Assert.Equal("MIX", chart[ZodiacName.Scorpio].Verdict);
    }

    [Fact]
    public void Gemini_has_argala_seven_to_none()
    {
        var gemini = SindUniBuilder.Build(Ramakrishnan())[ZodiacName.Gemini];
        Assert.Equal("Argala 7–0", gemini.ArgalaNet);
        var eleventh = gemini.ArgalaSources.Single(s => s.Offset == 11);
        Assert.True(eleventh.IsArgala);
        Assert.Equal(ZodiacName.Aries, eleventh.SourceSign(ZodiacName.Gemini));
        Assert.Equal(["Sun", "Mars", "Mercury", "Venus"], eleventh.Planets);
    }

    [Fact]
    public void Anti_zodiacal_sources_count_backwards()
    {
        var src = new SindUniArgalaSource(2, true, ["Moon"], CountedAntiZodiacally: true);
        Assert.Equal(ZodiacName.Sagittarius, src.SourceSign(ZodiacName.Capricornus));
    }

    [Fact]
    public void Special_lagnas_land_in_their_signs()
    {
        var chart = SindUniBuilder.Build(Ramakrishnan());
        Assert.Contains("HL", chart[ZodiacName.Pisces].SpecialLagnas);
        Assert.Contains("GL", chart[ZodiacName.Pisces].SpecialLagnas);
        Assert.Contains("ASC", chart[ZodiacName.Aries].SpecialLagnas);
        Assert.Contains("MOON", chart[ZodiacName.Scorpio].SpecialLagnas);
        Assert.Contains("AL", chart[ZodiacName.Capricornus].SpecialLagnas);
    }

    [Fact]
    public void Sav_bands_follow_the_strength_scale()
    {
        var sav = new Dictionary<ZodiacName, int> { [ZodiacName.Aquarius] = 43, [ZodiacName.Gemini] = 29, [ZodiacName.Libra] = 17 };
        var chart = SindUniBuilder.Build(Ramakrishnan(), sav);
        Assert.Equal(Core.Engines.Strength.StrengthTier.Strong, chart[ZodiacName.Aquarius].SavTier);
        Assert.Equal(Core.Engines.Strength.StrengthTier.Moderate, chart[ZodiacName.Gemini].SavTier);
        Assert.Equal(Core.Engines.Strength.StrengthTier.Weak, chart[ZodiacName.Libra].SavTier);
    }

    [Fact]
    public void Reading_view_counts_shared_padas_and_counts_houses_from_the_moon()
    {
        var cut = Render<SindUni2Grid>(p => p
            .Add(x => x.Chart, SindUniBuilder.Build(Ramakrishnan()))
            .Add(x => x.Title, "D1"));

        var aries = cut.Find(".su-cell[data-sign='Aries']");
        Assert.Equal("BEN", aries.GetAttribute("data-nature"));
        // Ashwini 1 holds the Lagna and Mercury.
        Assert.Equal("2", aries.QuerySelector(".dots i.n")!.TextContent);
        // Aries is the 6th from the Moon in Scorpio.
        Assert.EndsWith("6", aries.QuerySelector(".hn.m")!.TextContent);
        Assert.Equal(12, cut.FindAll(".su-cell").Count);
    }

    [Fact]
    public void Argala_lens_marks_the_source_signs()
    {
        var cut = Render<SindUni2Grid>(p => p
            .Add(x => x.Chart, SindUniBuilder.Build(Ramakrishnan()))
            .Add(x => x.PinnedSign, ZodiacName.Gemini));

        cut.FindAll(".su-seg button").Single(b => b.TextContent == "Argala").Click();

        Assert.Equal("+11", cut.Find(".su-cell[data-sign='Aries'] .lensmark").TextContent);
        Assert.Contains("target", cut.Find(".su-cell[data-sign='Gemini']").ClassList);
        Assert.NotEmpty(cut.FindAll("svg.lens line"));
    }

    [Fact]
    public void Micro_with_special_lagnas_draws_bars_and_pins_when_bound()
    {
        ZodiacName? pinned = null;
        var cut = Render<SindUni3Grid>(p => p
            .Add(x => x.Chart, SindUniBuilder.Build(Ramakrishnan()))
            .Add(x => x.ShowSpecialLagnas, true)
            .Add(x => x.PinnedSignChanged, s => pinned = s));

        // Pisces holds HL and GL, both on by default.
        Assert.Equal(2, cut.FindAll(".su-cell[data-sign='Pisces'] .bars i").Count);
        cut.FindAll(".su-slchip").Single(b => b.TextContent.Trim().EndsWith("GL")).Click();
        Assert.Single(cut.FindAll(".su-cell[data-sign='Pisces'] .bars i"));

        cut.Find(".su-cell[data-sign='Gemini']").Click();
        Assert.Equal(ZodiacName.Gemini, pinned);
    }

    [Fact]
    public void Static_micro_shows_no_sign_card()
    {
        var cut = Render<SindUni3Grid>(p => p.Add(x => x.Chart, SindUniBuilder.Build(Ramakrishnan())).Add(x => x.Title, "D1"));
        cut.Find(".su-cell[data-sign='Gemini']").Focus();
        Assert.Empty(cut.FindAll(".su-center .card"));
    }
}
