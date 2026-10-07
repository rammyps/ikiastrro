using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dasha;
using Ikiastrro.Core.Engines.Transits;
using Ikiastrro.Core.LifeMatters.Promise;

namespace Ikiastrro.Core.LifeMatters.Activation;

/// <summary>How the matter's relevant divisional chart qualifies the D1 foundation.</summary>
public enum VargaSupportState { NotEvaluated, Supports, Mixed, Obstructs }

/// <summary>Dasha activation is categorical. It is not a probability or strength score.</summary>
public enum DashaActivationState { BlockedByNatalPromise, NotEvaluated, Inactive, Active }

/// <summary>A transit can expose a manifestation window only after natal promise and dasha activation.</summary>
public enum TransitManifestationState { BlockedByNatalPromise, AwaitingDasha, Background, TriggerPresent }

/// <summary>A planet identified by one selected dasha-matter rule.</summary>
public sealed record LifeMatterDashaTarget(
    PlanetName Planet, int RuleNumber, string Varga, string Result, string Why);

/// <summary>A running period lord that matches at least one planet capable of bringing the matter.</summary>
public sealed record LifeMatterDashaMatch(
    string Level, PlanetName Lord, IReadOnlyList<LifeMatterDashaTarget> Targets);

/// <summary>
/// The bridge between a LifeMatter promise and DashaMatters. The caller supplies only the
/// DashaMatter rules selected for this matter; the engine deliberately does no text matching.
/// </summary>
public sealed record LifeMatterActivation(
    string MatterCode,
    PromiseVerdict D1Verdict,
    DomainConfirmation? Varga,
    VargaSupportState VargaSupport,
    IReadOnlyList<LifeMatterDashaTarget> Targets,
    IReadOnlyList<LifeMatterDashaMatch> Matches,
    DashaActivationState State,
    IReadOnlyList<string> Lines);

/// <summary>The transit-stage result. TriggerPresent means a qualified timing contact, not a promised event.</summary>
public sealed record LifeMatterManifestation(
    string MatterCode,
    TransitManifestationState State,
    IReadOnlyList<GocharaPlanetQualifier> RelevantTransits,
    IReadOnlyList<string> Lines);

/// <summary>
/// Orders the inference layers without blending them into a score:
/// D1 promise → relevant varga support → dasha activation → transit manifestation window.
/// Pure; no database or clock access.
/// </summary>
public static class LifeMatterActivationEngine
{
    public static LifeMatterActivation Activate(
        LifeMatterPromise promise,
        IEnumerable<DashaMatterRule> selectedRules,
        GocharaDashaLords runningDasha)
    {
        ArgumentNullException.ThrowIfNull(promise);
        ArgumentNullException.ThrowIfNull(selectedRules);
        ArgumentNullException.ThrowIfNull(runningDasha);

        var targets = selectedRules
            .SelectMany(rule => rule.Planets.Select(planet => new LifeMatterDashaTarget(
                planet.Planet, rule.Number, rule.Varga, rule.Result, planet.Why)))
            .Distinct()
            .ToList();

        var matches = runningDasha.Running()
            .Select(period => new LifeMatterDashaMatch(
                period.Level,
                period.Lord,
                targets.Where(target => target.Planet == period.Lord).ToList()))
            .Where(match => match.Targets.Count > 0)
            .ToList();

        var state = promise.D1.Verdict == PromiseVerdict.Indeterminate
            ? DashaActivationState.BlockedByNatalPromise
            : targets.Count == 0
                ? DashaActivationState.NotEvaluated
                : matches.Count == 0
                    ? DashaActivationState.Inactive
                    : DashaActivationState.Active;

        var lines = new List<string>
        {
            $"D1 promise: {promise.D1.Verdict}.",
            promise.Domain is null
                ? "Relevant varga support: not evaluated."
                : $"Relevant varga {promise.Domain.Chart}: {promise.Domain.Verdict} — {promise.Domain.Inference}",
        };
        lines.Add(state switch
        {
            DashaActivationState.BlockedByNatalPromise => "Dasha activation is not inferred because the D1 promise is indeterminate.",
            DashaActivationState.NotEvaluated => "Dasha activation is not evaluated because no matter-specific dasha targets were supplied.",
            DashaActivationState.Inactive => "No running dasha lord matches a planet selected to bring this matter.",
            _ => $"Active through {string.Join("; ", matches.Select(m => $"{m.Level} {m.Lord}"))}.",
        });

        return new LifeMatterActivation(
            promise.MatterCode, promise.D1.Verdict, promise.Domain, SupportOf(promise.Domain),
            targets, matches, state, lines);
    }

    public static LifeMatterManifestation QualifyTransits(
        LifeMatterActivation activation,
        IEnumerable<GocharaPlanetQualifier> transits)
    {
        ArgumentNullException.ThrowIfNull(activation);
        ArgumentNullException.ThrowIfNull(transits);

        var transitList = transits.ToList();
        var activatedLords = activation.Matches.Select(match => match.Lord).ToHashSet();
        var relevant = transitList.Where(transit =>
            activation.State == DashaActivationState.Active &&
            transit.Foreground &&
            (activatedLords.Contains(transit.Planet) ||
             activatedLords.Contains(transit.HouseLord) ||
             transit.DashaLinks.Any(link => activatedLords.Contains(link.Lord))))
            .ToList();

        var state = activation.State switch
        {
            DashaActivationState.BlockedByNatalPromise => TransitManifestationState.BlockedByNatalPromise,
            not DashaActivationState.Active => TransitManifestationState.AwaitingDasha,
            _ when relevant.Count > 0 => TransitManifestationState.TriggerPresent,
            _ => TransitManifestationState.Background,
        };

        var explanation = state switch
        {
            TransitManifestationState.BlockedByNatalPromise => "Transit cannot create a manifestation claim without an established D1 promise.",
            TransitManifestationState.AwaitingDasha => "Transit remains background because the matter is not active by dasha.",
            TransitManifestationState.Background => "The matter is active by dasha, but no supplied transit contacts an activated lord.",
            _ => "A foreground transit contacts an activated dasha lord; this is a manifestation window, not an event guarantee.",
        };
        return new LifeMatterManifestation(activation.MatterCode, state, relevant, [explanation]);
    }

    private static VargaSupportState SupportOf(DomainConfirmation? domain) => domain?.Verdict switch
    {
        PromiseVerdict.StrongPositive or PromiseVerdict.PositiveConditional => VargaSupportState.Supports,
        PromiseVerdict.Mixed or PromiseVerdict.WeakLimited => VargaSupportState.Mixed,
        PromiseVerdict.Adverse => VargaSupportState.Obstructs,
        _ => VargaSupportState.NotEvaluated,
    };
}
