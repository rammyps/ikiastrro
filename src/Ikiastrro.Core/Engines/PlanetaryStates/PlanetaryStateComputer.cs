using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Models;

using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.PlanetaryStates;

/// <summary>
/// Builds the tbl_Fact_PlanetaryState rows for one chart from the ChartKeyDetail list that
/// ChartAnalyzer.Compute already produced (it carries Sign / DegreesInSignDecimal /
/// DignityStatus per planet). ChartResultId is stamped by the caller after the parent ChartResult
/// row exists — same contract as ChartAnalyzer.
///
/// Ascendant is excluded (no avastha for a house-circle point). The age state is emitted only for
/// D1 (needs a continuous within-sign degree); the wakefulness state is emitted for every chart type.
/// </summary>
public static class PlanetaryStateComputer
{
    public static List<PlanetaryStateFact> Compute(
        ChartAnalysisInput input,
        IReadOnlyList<ChartKeyDetail> keyDetails,
        PlanetaryStateRuleSet rules,
        double? janmaGhatis = null)
    {
        var isRasiChart = input.ChartType == "D1";
        var facts = new List<PlanetaryStateFact>();

        // Sayanaadi's chart-wide (not per-planet) inputs: M = Moon's nakshatra, L = Lagna's rasi,
        // G = the ghati running at birth. Resolved once; only used when isRasiChart.
        // kd.Nakshatra stores AstroMath's canonical display name ("Ashwini", "Uttara Phalguni", ...)
        // - NOT the ConstellationName enum member spelling (old VedAstro form, e.g. "Aswini",
        // "Uttara") - see ConstellationName's own doc comment. Look up against the canonical list.
        static int? NakshatraNumber(string? name)
        {
            if (name is null) return null;
            var index = AstroMath.NakshatraCanonicalNames.ToList().IndexOf(name);
            return index < 0 ? null : index + 1;
        }
        var moonNakshatraNumber = isRasiChart
            ? NakshatraNumber(keyDetails.FirstOrDefault(k => k.Planet == "Moon")?.Nakshatra)
            : null;
        var lagnaRasiNumber = isRasiChart ? (int)input.AscendantSign + 1 : (int?)null;
        var ghati = janmaGhatis is { } jg ? PostureStateCalculator.GhatiRunning(jg) : (int?)null;

        foreach (var kd in keyDetails)
        {
            // Ascendant + the special points (AL / A2–A12 / HL / Gulika / Maandi) are not grahas —
            // no avastha for a house-circle or reference point.
            if (kd.Planet == "Ascendant" || kd.PointKind != "Graha")
                continue;

            var fact = new PlanetaryStateFact
            {
                Planet = kd.Planet,
                PlanetId = (byte?)Ikiastrro.Core.Engines.Astronomy.AstroIds.PlanetIdOrNull(kd.Planet),
                RuleSetId = rules.RuleSetId,
            };

            // Wakefulness state — every chart type, from dignity.
            var wake = WakefulnessStateCalculator.For(kd.DignityStatus, rules.WakefulnessByDignity);
            fact.WakefulnessStateId = wake?.AvasthaStateId;

            // Age state — D1 only, from within-sign degree.
            if (isRasiChart && kd.DegreesInSignDecimal is { } degree
                && Enum.TryParse<ZodiacName>(kd.Sign, out var sign))
            {
                var age = AgeStateCalculator.For(sign, degree, rules.AgeBands);
                fact.AgeStateId = age.AvasthaStateId;
                fact.AgeEffectFraction = age.EffectFraction;
            }

            // Posture (Sayanaadi) state — D1 only, needs the within-sign degree (for the
            // navamsa index), the planet's own nakshatra, and the chart-wide M/G/L above.
            if (isRasiChart && kd.DegreesInSignDecimal is { } degreeForNavamsa
                && moonNakshatraNumber is { } m && lagnaRasiNumber is { } l && ghati is { } g
                && NakshatraNumber(kd.Nakshatra) is { } c
                && fact.PlanetId is { } planetIndex)
            {
                var navamsa = PostureStateCalculator.NavamsaIndex(degreeForNavamsa);
                var index = PostureStateCalculator.ComputeIndex(c, planetIndex, navamsa, m, g, l);
                fact.PostureStateId = PostureStateCalculator.For(index, rules.PostureStatesBySequence)?.Id;
            }

            facts.Add(fact);
        }

        return facts;
    }
}
