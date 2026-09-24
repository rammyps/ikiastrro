using Bunit;
using Ikiastrro.Core.Engines.PlanetaryStates;
using Ikiastrro.Data;
using Ikiastrro.Web.Components.Charts;
using Xunit;

namespace Ikiastrro.Web.Tests;

public sealed class PlanetaryStateTableTests : BunitContext
{
    [Fact]
    public void RendersAllThreeMeaningsAndThePlanetSpecificReading()
    {
        var rows = new[]
        {
            new PlanetaryStateFact
            {
                Planet = "Moon", PlanetId = 2, RuleSetId = 1,
                AgeStateId = 1, AgeEffectFraction = 0.25m,
                WakefulnessStateId = 6, PostureStateId = 20,
            },
        };
        IReadOnlyDictionary<byte, PlanetaryStateRow> states = new Dictionary<byte, PlanetaryStateRow>
        {
            [1] = new(1, "Baaladi", "Baala", 1, "Infant — quarter effect"),
            [6] = new(6, "Jagradadi", "Jagrat", 1, "Awake — full result"),
            [20] = new(20, "Sayanadi", "Kautuka", 11, "Being eager"),
        };
        IReadOnlyDictionary<(byte, byte), PostureStateInterpretationRow> interpretations =
            new Dictionary<(byte, byte), PostureStateInterpretationRow>
            {
                [(20, 2)] = new(1, 1, 20, 2,
                    "The source associates this eager mode with status and prosperity.",
                    "Read together with lunar phase.", "SRC_PVR_INTEGRATED", "§15.4.4, pp.193-199"),
            };

        var cut = Render<PlanetaryStateTable>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.StateNames, states)
            .Add(x => x.PostureInterpretations, interpretations));

        Assert.Contains("Infant — quarter effect", cut.Markup);
        Assert.Contains("Awake — full result", cut.Markup);
        Assert.Contains("Being eager", cut.Markup);
        Assert.Contains("status and prosperity", cut.Markup);
        Assert.Contains("Conditional:", cut.Markup);
        Assert.Contains("SRC_PVR_INTEGRATED", cut.Markup);
    }

    [Fact]
    public void MissingInterpretationDegradesToDash()
    {
        var rows = new[]
        {
            new PlanetaryStateFact { Planet = "Sun", PlanetId = 1, RuleSetId = 1, PostureStateId = 9 },
        };

        var cut = Render<PlanetaryStateTable>(p => p
            .Add(x => x.Rows, rows)
            .Add(x => x.StateNames, new Dictionary<byte, PlanetaryStateRow>
            {
                [9] = new(9, "Sayanadi", "Sayana", 1, "Lying down, resting"),
            })
            .Add(x => x.PostureInterpretations,
                new Dictionary<(byte, byte), PostureStateInterpretationRow>()));

        Assert.Contains("Sayana", cut.Markup);
        Assert.Empty(cut.FindAll(".pst-source"));
    }
}
