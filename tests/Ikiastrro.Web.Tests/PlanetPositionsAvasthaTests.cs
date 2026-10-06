using Bunit;
using Ikiastrro.Core.Engines.PlanetaryStates;
using Ikiastrro.Core.Presentation;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

/// <summary>The Planet positions table's optional avasthas column: the five states with PVR 15.4's meanings, the final
/// Sayanadi value as n/12, and the house results PVR names.</summary>
public sealed class PlanetPositionsAvasthaTests : BunitContext
{
    private static PlanetRow Row(string planet, int house) => new(
        planet, "Leo", "10°00'", "Magha", 1, "Ketu", false, false, null, null, house, house, "Own Sign", null, null, null, null,
        "Venus", "Moon", 0, 0, 130, "Graha", 5, 10, "Sun", null);

    private static PlanetaryStateRow S(byte id, string system, string name, byte order, string? meaning = null) => new(id, system, name, order, meaning);

    [Fact]
    public void AvasthaColumnShowsStatesMeaningsFinalValueAndHouseResults()
    {
        var names = new[]
        {
            S(1, "Baaladi", "Kumara", 2), S(2, "Jagradadi", "Swapna", 2), S(3, "Deeptadi", "Mudita", 3),
            S(4, "Lajjitadi", "Lajjita", 1), S(5, "Sayanadi", "Gamana", 5, "Going (on the move)"),
        }.ToDictionary(x => x.Id);
        var fact = new PlanetaryStateFact
        {
            Planet = "Sun", PlanetId = 1, AgeStateId = 1, AgeEffectFraction = 0.5m, WakefulnessStateId = 2, PostureStateId = 5,
            DeeptadiStateIds = [3], LajjitadiStateIds = [4],
        };
        var interpretations = new Dictionary<(byte, byte), PostureStateInterpretationRow>
        {
            [(5, 1)] = new(1, 1, 5, 1, "The source associates this mode with travel.", null, "SRC_PVR_INTEGRATED", "A15.4.4, pp.193-199"),
        };

        var cut = Render<PlanetPositionsTable>(p => p
            .Add(x => x.Rows, [Row("Sun", 5)]).Add(x => x.LagnaSign, "Aries").Add(x => x.ShowAnalysis, false)
            .Add(x => x.ShowAvastha, true).Add(x => x.States, [fact]).Add(x => x.StateNames, names)
            .Add(x => x.PostureInterpretations, interpretations));

        Assert.Contains("Avasthas", cut.Markup);
        var cell = cut.Find("td.ppt-avastha").Normalized();
        Assert.Contains("Kumara", cell);
        Assert.Contains("gives half of its results", cell);          // PVR 15.4.1
        Assert.Contains("Swapna", cell);
        Assert.Contains("gives medium results", cell);               // PVR 15.4.2
        Assert.Contains("Mudita", cell);
        Assert.Contains("Lajjita", cell);
        Assert.Contains("Gamana · 5/12", cell);                  // final Sayanadi value
        Assert.Contains("travel", cell);                             // PVR 15.4.4 result for this planet
        Assert.Contains("losses related to progeny", cell);          // Lajjita in the 5th
    }

    [Fact]
    public void AvasthaColumnIsOffByDefault()
    {
        var cut = Render<PlanetPositionsTable>(p => p
            .Add(x => x.Rows, [Row("Sun", 5)]).Add(x => x.LagnaSign, "Aries").Add(x => x.ShowAnalysis, false));
        Assert.DoesNotContain("Avasthas", cut.Markup);
    }
}
