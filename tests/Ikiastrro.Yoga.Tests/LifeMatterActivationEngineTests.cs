using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Engines.Strength;
using Ikiastrro.Core.Engines.Transits;
using Ikiastrro.Core.LifeMatters.Activation;
using Ikiastrro.Core.LifeMatters.Promise;

namespace Ikiastrro.Yoga.Tests;

public sealed class LifeMatterActivationEngineTests
{
    [Fact]
    public void Indeterminate_d1_blocks_matching_dasha_and_transit()
    {
        var activation = LifeMatterActivationEngine.Activate(
            Promise(PromiseVerdict.Indeterminate),
            [Rule(PlanetName.Jupiter)],
            new GocharaDashaLords(PlanetName.Jupiter, null, null));

        var manifestation = LifeMatterActivationEngine.QualifyTransits(
            activation,
            [Transit(PlanetName.Jupiter, PlanetName.Jupiter, foreground: true)]);

        Assert.Equal(DashaActivationState.BlockedByNatalPromise, activation.State);
        Assert.Single(activation.Matches); // the factual match remains visible
        Assert.Equal(TransitManifestationState.BlockedByNatalPromise, manifestation.State);
        Assert.Empty(manifestation.RelevantTransits);
    }

    [Theory]
    [InlineData(PromiseVerdict.StrongPositive)]
    [InlineData(PromiseVerdict.Mixed)]
    [InlineData(PromiseVerdict.Adverse)]
    public void Established_d1_can_be_activated_without_changing_its_direction(PromiseVerdict verdict)
    {
        var activation = LifeMatterActivationEngine.Activate(
            Promise(verdict),
            [Rule(PlanetName.Venus)],
            new GocharaDashaLords(null, PlanetName.Venus, null));

        Assert.Equal(DashaActivationState.Active, activation.State);
        Assert.Equal(verdict, activation.D1Verdict);
        Assert.Equal("Antardaśā", Assert.Single(activation.Matches).Level);
    }

    [Fact]
    public void Varga_support_is_separate_and_does_not_override_d1_gate()
    {
        var domain = new DomainConfirmation("D7", PromiseVerdict.StrongPositive, "The D7 supports children.");
        var activation = LifeMatterActivationEngine.Activate(
            Promise(PromiseVerdict.Indeterminate, domain),
            [Rule(PlanetName.Jupiter)],
            new GocharaDashaLords(PlanetName.Jupiter, null, null));

        Assert.Equal(VargaSupportState.Supports, activation.VargaSupport);
        Assert.Equal(DashaActivationState.BlockedByNatalPromise, activation.State);
    }

    [Fact]
    public void Transit_alone_is_not_manifestation()
    {
        var activation = LifeMatterActivationEngine.Activate(
            Promise(PromiseVerdict.PositiveConditional),
            [Rule(PlanetName.Jupiter)],
            new GocharaDashaLords(PlanetName.Saturn, null, null));

        var manifestation = LifeMatterActivationEngine.QualifyTransits(
            activation,
            [Transit(PlanetName.Jupiter, PlanetName.Jupiter, foreground: true)]);

        Assert.Equal(DashaActivationState.Inactive, activation.State);
        Assert.Equal(TransitManifestationState.AwaitingDasha, manifestation.State);
        Assert.Empty(manifestation.RelevantTransits);
    }

    [Fact]
    public void Activated_lord_with_foreground_contact_opens_trigger_window()
    {
        var activation = LifeMatterActivationEngine.Activate(
            Promise(PromiseVerdict.PositiveConditional),
            [Rule(PlanetName.Jupiter)],
            new GocharaDashaLords(PlanetName.Jupiter, null, null));

        var manifestation = LifeMatterActivationEngine.QualifyTransits(
            activation,
            [Transit(PlanetName.Saturn, PlanetName.Jupiter, foreground: true)]);

        Assert.Equal(TransitManifestationState.TriggerPresent, manifestation.State);
        Assert.Single(manifestation.RelevantTransits);
    }

    [Fact]
    public void Unrelated_transit_stays_background_during_active_dasha()
    {
        var activation = LifeMatterActivationEngine.Activate(
            Promise(PromiseVerdict.PositiveConditional),
            [Rule(PlanetName.Jupiter)],
            new GocharaDashaLords(PlanetName.Jupiter, null, null));

        var manifestation = LifeMatterActivationEngine.QualifyTransits(
            activation,
            [Transit(PlanetName.Saturn, PlanetName.Mars, foreground: false)]);

        Assert.Equal(TransitManifestationState.Background, manifestation.State);
        Assert.Empty(manifestation.RelevantTransits);
    }

    private static LifeMatterPromise Promise(PromiseVerdict verdict, DomainConfirmation? domain = null)
    {
        var d1 = new D1Foundation(verdict, Confidence.Medium,
            new Dictionary<TestimonyRole, Direction>(), []);
        return new LifeMatterPromise(
            "CHILDREN", d1, domain, [], [], [], [], verdict, Confidence.Medium,
            [], [], "", "", null);
    }

    private static DashaMatterRule Rule(PlanetName planet) =>
        new(1, "D7", "The 5th lord in D-7", "can give a child",
            [new DashaMatterPlanet(planet, "Lord of the 5th")], null, null, "test");

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
