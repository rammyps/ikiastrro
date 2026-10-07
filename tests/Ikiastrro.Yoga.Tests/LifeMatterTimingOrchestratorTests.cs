using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Engines.Transits;
using Ikiastrro.Core.LifeMatters;
using Ikiastrro.Core.LifeMatters.Activation;
using Ikiastrro.Core.LifeMatters.Promise;

namespace Ikiastrro.Yoga.Tests;

public sealed class LifeMatterTimingOrchestratorTests
{
    [Fact]
    public void Produces_complete_ordered_trigger_result()
    {
        var result = LifeMatterTimingOrchestrator.Evaluate(new(
            Promise(PromiseVerdict.PositiveConditional,
                new DomainConfirmation("D10", PromiseVerdict.StrongPositive, "Career varga supports.")),
            Focus("D10", 10),
            [Rule(10, DashaMatterScopeKind.ChartTheme), Rule(11, DashaMatterScopeKind.House, 10)],
            new GocharaDashaLords(PlanetName.Sun, null, null),
            [Transit(PlanetName.Saturn, PlanetName.Sun, foreground: true)]));

        Assert.True(result.HasCompleteDashaMapping);
        Assert.Equal(VargaSupportState.Supports, result.Activation.VargaSupport);
        Assert.Equal(DashaActivationState.Active, result.Activation.State);
        Assert.Equal(TransitManifestationState.TriggerPresent, result.Manifestation.State);
        Assert.Equal([10, 11], result.Selection.Rules.Select(rule => rule.Number));
    }

    [Fact]
    public void Mapping_gap_propagates_without_guessing_or_activation()
    {
        var result = LifeMatterTimingOrchestrator.Evaluate(new(
            Promise(PromiseVerdict.StrongPositive),
            Focus("D10", 10),
            [],
            new GocharaDashaLords(PlanetName.Sun, null, null),
            [Transit(PlanetName.Sun, PlanetName.Sun, foreground: true)]));

        Assert.False(result.HasCompleteDashaMapping);
        Assert.Equal(DashaActivationState.NotEvaluated, result.Activation.State);
        Assert.Equal(TransitManifestationState.AwaitingDasha, result.Manifestation.State);
        Assert.Empty(result.Manifestation.RelevantTransits);
    }

    [Fact]
    public void Foreground_transit_cannot_bypass_inactive_dasha()
    {
        var result = LifeMatterTimingOrchestrator.Evaluate(new(
            Promise(PromiseVerdict.StrongPositive),
            Focus("D10", 10),
            [Rule(10, DashaMatterScopeKind.ChartTheme)],
            new GocharaDashaLords(PlanetName.Saturn, null, null),
            [Transit(PlanetName.Sun, PlanetName.Sun, foreground: true)]));

        Assert.Equal(DashaActivationState.Inactive, result.Activation.State);
        Assert.Equal(TransitManifestationState.AwaitingDasha, result.Manifestation.State);
    }

    [Fact]
    public void Adverse_d1_direction_is_preserved_through_active_timing_window()
    {
        var result = LifeMatterTimingOrchestrator.Evaluate(new(
            Promise(PromiseVerdict.Adverse,
                new DomainConfirmation("D10", PromiseVerdict.Adverse, "Career varga obstructs.")),
            Focus("D10", 10),
            [Rule(10, DashaMatterScopeKind.ChartTheme)],
            new GocharaDashaLords(PlanetName.Sun, null, null),
            [Transit(PlanetName.Sun, PlanetName.Sun, foreground: true)]));

        Assert.Equal(PromiseVerdict.Adverse, result.Activation.D1Verdict);
        Assert.Equal(VargaSupportState.Obstructs, result.Activation.VargaSupport);
        Assert.Equal(TransitManifestationState.TriggerPresent, result.Manifestation.State);
    }

    private static LifeMatterPromise Promise(
        PromiseVerdict verdict, DomainConfirmation? domain = null)
    {
        var d1 = new D1Foundation(verdict, Confidence.Medium,
            new Dictionary<TestimonyRole, Direction>(), []);
        return new LifeMatterPromise(
            "CAREER_STATUS_07", d1, domain, [], [], [], [], verdict, Confidence.Medium,
            [], [], "", "", null);
    }

    private static ResolvedLifeMatterFocus Focus(string chart, int house)
    {
        const int matterId = 7;
        return new ResolvedLifeMatterFocus(
            matterId,
            new LifeMatterSubjectRule(1, 1, matterId, "CAREER_STATUS", chart),
            [],
            [new LifeMatterFocusRule(1, 1, matterId, LifeMatterFocusKind.House,
                "LAGNA", house, null, 1)]);
    }

    private static DashaMatterRule Rule(
        int number, DashaMatterScopeKind kind, int? house = null) =>
        new(number, "D10", "structured", "career result",
            [new DashaMatterPlanet(PlanetName.Sun, "career significator")],
            null, null, "test", new DashaMatterScope(kind, house));

    private static GocharaPlanetQualifier Transit(
        PlanetName planet, PlanetName houseLord, bool foreground)
    {
        var links = foreground
            ? new[] { new DashaLink("Mahādaśā", houseLord, DashaLinkKind.LordRulesHouse) }
            : [];
        return new GocharaPlanetQualifier(
            planet, ZodiacName.Aries, 1, houseLord, [], [], null,
            StrengthTier.Moderate, StrengthTier.Moderate, StrengthTier.Moderate,
            GocharaPromise.Promised, links, foreground, []);
    }
}
