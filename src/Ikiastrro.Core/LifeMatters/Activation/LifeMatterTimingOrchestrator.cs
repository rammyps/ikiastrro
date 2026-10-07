using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Engines.Transits;
using Ikiastrro.Core.LifeMatters.Promise;

namespace Ikiastrro.Core.LifeMatters.Activation;

/// <summary>All already-resolved facts needed to time one LifeMatter for one instant.</summary>
public sealed record LifeMatterTimingRequest(
    LifeMatterPromise Promise,
    ResolvedLifeMatterFocus Focus,
    IReadOnlyList<DashaMatterRule> AvailableDashaRules,
    GocharaDashaLords RunningDasha,
    IReadOnlyList<GocharaPlanetQualifier> Transits);

/// <summary>
/// One auditable result preserving every stage of the inference chain. Mapping gaps remain visible
/// and no later stage can silently replace an earlier one.
/// </summary>
public sealed record LifeMatterTimingResult(
    LifeMatterDashaRuleSelection Selection,
    LifeMatterActivation Activation,
    LifeMatterManifestation Manifestation)
{
    public bool HasCompleteDashaMapping => Selection.MissingMappings.Count == 0;
}

/// <summary>
/// Composes the pure LifeMatter engines in their required order:
/// D1 promise → relevant varga → selected dasha rules → running-period activation → transit window.
/// The request contains facts prepared by repositories/adapters; this class performs no I/O.
/// </summary>
public static class LifeMatterTimingOrchestrator
{
    public static LifeMatterTimingResult Evaluate(LifeMatterTimingRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Promise);
        ArgumentNullException.ThrowIfNull(request.Focus);
        ArgumentNullException.ThrowIfNull(request.AvailableDashaRules);
        ArgumentNullException.ThrowIfNull(request.RunningDasha);
        ArgumentNullException.ThrowIfNull(request.Transits);

        var selection = LifeMatterDashaRuleMapper.Select(request.Focus, request.AvailableDashaRules);
        var activation = LifeMatterActivationEngine.Activate(
            request.Promise, selection.Rules, request.RunningDasha);
        var manifestation = LifeMatterActivationEngine.QualifyTransits(activation, request.Transits);

        return new LifeMatterTimingResult(selection, activation, manifestation);
    }
}
