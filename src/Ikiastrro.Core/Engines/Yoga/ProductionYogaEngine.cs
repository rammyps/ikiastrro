using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Yoga;

public sealed record UnifiedYogaEvaluation(
    ContextualYogaResult Result,
    IReadOnlyList<string> MissingRequirementCodes);

/// <summary>
/// Production composition root for every source-attributed yoga evaluator currently implemented.
/// One source variant is one result; authorities and textual alternatives are never collapsed.
/// </summary>
public sealed class ProductionYogaEngine : IYogaEngine
{
    private readonly VerifiedSourceYogaEngine _verified = new();

    public IReadOnlyList<YogaResult> Detect(ChartBundle bundle) => DetectDetailed(bundle)
        .Select(x => new YogaResult(x.Result.YogaCode, Category(x.Result),
            x.Result.Present == true, false, x.Result.Notes))
        .ToList();

    public IReadOnlyList<UnifiedYogaEvaluation> DetectDetailed(ChartBundle bundle)
    {
        var d1 = bundle.Charts.FirstOrDefault(x => x.ChartType.Equals("D1", StringComparison.OrdinalIgnoreCase));
        if (d1 is null) return [];
        var d9 = bundle.Charts.FirstOrDefault(x => x.ChartType.Equals("D9", StringComparison.OrdinalIgnoreCase));

        var rows = new Dictionary<(string Source, string Variant), UnifiedYogaEvaluation>();
        void Add(ContextualYogaResult result, IReadOnlyList<string>? missing = null)
            => rows[(result.SourceRefCode, result.SourceVariantCode)] =
                new(result, missing ?? MissingCodes(result));

        foreach (var x in _verified.Detect(d1))
            Add(new(x.YogaCode, x.Present, "EVALUATED", x.SourceRefCode,
                x.SourceVariantCode, x.SourceLocator, x.Notes));

        foreach (var x in YogaInputEvaluator.Evaluate(bundle.Birth, bundle.Charts, bundle.SunTimes))
            Add(x.Result, x.MissingRequirementCodes);
        foreach (var x in RamanContextYogaEvaluator.Evaluate(d1, d9)) Add(x);
        foreach (var x in MalikaYogaEvaluator.Evaluate(d1))
            Add(new(x.YogaCode, x.Present, "EVALUATED", x.SourceRefCode,
                x.SourceVariantCode, x.SourceLocator, x.DisplayName));
        foreach (var x in RamanYogaBatchFourEvaluator.Evaluate(d1, d9)) Add(x);
        foreach (var x in RamanYogaBatchFiveEvaluator.Evaluate(d1, d9)) Add(x);
        foreach (var x in RamanYogaBatchSixEvaluator.Evaluate(d1, d9,
                     !bundle.SunTimes.IsNightBirth, Phase(bundle)?.IsWaxing, Phase(bundle)?.IsFullMoon)) Add(x);
        foreach (var x in RamanNabhasaBatchEvaluator.Evaluate(d1)) Add(x);
        foreach (var x in RamanNabhasaSecondBatchEvaluator.Evaluate(d1)) Add(x);
        foreach (var x in RamanYogaBatchSevenEvaluator.Evaluate(bundle)) Add(x);
        foreach (var x in RamanDhanaYogaEvaluator.Evaluate(bundle)) Add(x);
        foreach (var x in RamanDaridraYogaEvaluator.Evaluate(d1)) Add(x);
        foreach (var x in RamanYogaBatchEightEvaluator.Evaluate(bundle)) Add(x);
        foreach (var x in RamanFamilyYogaEvaluator.Evaluate(bundle)) Add(x);
        foreach (var x in RamanProgenyYogaEvaluator.Evaluate(bundle)) Add(x);
        foreach (var x in RamanRajaYogaEvaluator.Evaluate(bundle)) Add(x);
        foreach (var x in RamanAfflictionYogaEvaluator.Evaluate(bundle)) Add(x);
        foreach (var x in PvrChapter11YogaEvaluator.Evaluate(d1, d9)) Add(x);
        foreach (var x in PvrChapter11NumberedYogaEvaluator.Evaluate(bundle)) Add(x);
        foreach (var x in HoroscopeExplorerGapYogaEvaluator.Evaluate(d1)) Add(x);
        foreach (var x in RamanFinalHundredCatalog.Entries()) Add(x);

        // Context-aware rows carry stricter missing-input semantics than their batch counterparts.
        foreach (var x in YogaInputEvaluator.Evaluate(bundle.Birth, bundle.Charts, bundle.SunTimes))
            Add(x.Result, x.MissingRequirementCodes);

        return rows.Values
            .OrderBy(x => EntryNumber(x.Result.SourceVariantCode))
            .ThenBy(x => x.Result.SourceRefCode)
            .ThenBy(x => x.Result.SourceVariantCode)
            .ToList();
    }

    /// <summary>The divisional charts JHora's yoga rules name (D-1, D-9, D-2, D-3, D-12, D-30 — decided
    /// 2026-10-07, docs/cli/yoga-per-varga-design.md). Only these are evaluated as a yoga reference chart.</summary>
    public static readonly IReadOnlyList<string> VargaReferenceCharts = ["D1", "D2", "D3", "D9", "D12", "D30"];

    /// <summary>
    /// Yogas as JHora's Yogas list shows them for one divisional chart: the single-chart geometry evaluators
    /// (Verified source engine, Malika, both Nabhasa batches, PVR ch.11 named yogas) re-run with
    /// <paramref name="chartType"/> as the reference chart. Everything that needs birth-level inputs
    /// (karakas, special lagnas, day/night, lunar phase, strength thresholds) or several charts at once stays
    /// D1-only in <see cref="DetectDetailed"/>; this is a confirmation layer, never a source-attributed D1 row.
    /// Rows that cannot be computed from the one chart are omitted rather than reported NOT_EVALUATED.
    /// </summary>
    public IReadOnlyList<UnifiedYogaEvaluation> DetectForVarga(ChartBundle bundle, string chartType)
    {
        if (!VargaReferenceCharts.Contains(chartType, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentOutOfRangeException(nameof(chartType), chartType, "Not a yoga reference chart.");
        var chart = bundle.Charts.FirstOrDefault(x => x.ChartType.Equals(chartType, StringComparison.OrdinalIgnoreCase));
        if (chart is null) return [];

        var rows = new Dictionary<(string Source, string Variant), UnifiedYogaEvaluation>();
        void Add(ContextualYogaResult result)
        {
            if (result.EvaluationStatus == "EVALUATED")
                rows[(result.SourceRefCode, result.SourceVariantCode)] = new(result, []);
        }

        foreach (var x in _verified.Detect(chart))
            Add(new(x.YogaCode, x.Present, "EVALUATED", x.SourceRefCode, x.SourceVariantCode, x.SourceLocator, x.Notes));
        foreach (var x in MalikaYogaEvaluator.Evaluate(chart))
            Add(new(x.YogaCode, x.Present, "EVALUATED", x.SourceRefCode, x.SourceVariantCode, x.SourceLocator, x.DisplayName));
        foreach (var x in RamanNabhasaBatchEvaluator.Evaluate(chart)) Add(x);
        foreach (var x in RamanNabhasaSecondBatchEvaluator.Evaluate(chart)) Add(x);
        foreach (var x in PvrChapter11YogaEvaluator.Evaluate(chart)) Add(x);

        return rows.Values
            .OrderBy(x => x.Result.SourceRefCode).ThenBy(x => x.Result.SourceVariantCode).ToList();
    }

    private static LunarPhase? Phase(ChartBundle bundle)
    {
        var d1 = bundle.Charts.FirstOrDefault(x => x.ChartType.Equals("D1", StringComparison.OrdinalIgnoreCase));
        double? Longitude(string planet) => d1?.Planets.FirstOrDefault(x => x.Planet == planet)?.NirayanaLongitudeDegrees;
        return LunarPhase.Calculate(Longitude("Sun"), Longitude("Moon"));
    }

    private static IReadOnlyList<string> MissingCodes(ContextualYogaResult result)
    {
        if (result.EvaluationStatus != "NOT_EVALUATED") return [];
        var note = result.Notes ?? string.Empty;
        if (note.Contains("D9", StringComparison.OrdinalIgnoreCase)) return ["CHART_D9"];
        if (note.Contains("Vaiseshikamsa", StringComparison.OrdinalIgnoreCase)) return ["VAISESHIKAMSA"];
        if (note.Contains("visual source adjudication", StringComparison.OrdinalIgnoreCase)) return ["SOURCE_ADJUDICATION"];
        if (note.Contains("Shadbala threshold", StringComparison.OrdinalIgnoreCase)) return ["SHADBALA_THRESHOLD"];
        if (note.Contains("strength", StringComparison.OrdinalIgnoreCase)) return ["YOGA_STRENGTH_POLICY"];
        return ["REQUIRED_CALCULATION_NOT_IMPLEMENTED"];
    }

    private static int EntryNumber(string variant)
    {
        if (!variant.StartsWith("RAMAN_300_", StringComparison.Ordinal)) return int.MaxValue;
        var digits = new string(variant[10..].TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var number) ? number : int.MaxValue;
    }

    private static string Category(ContextualYogaResult result)
        => result.SourceRefCode == "SRC_PVR_INTEGRATED" ? "PVR" : "Raman";
}
