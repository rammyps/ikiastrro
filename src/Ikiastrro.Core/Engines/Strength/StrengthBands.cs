namespace Ikiastrro.Core.Engines.Strength;

/// <summary>Strong / Moderate / Weak reading of one strength statistic; None when there is no value.</summary>
public enum StrengthTier { Strong, Moderate, Weak, None }

/// <summary>One statistic's two cut-offs, both inclusive lower bounds: at or above
/// <see cref="StrongFrom"/> is Strong, at or above <see cref="ModerateFrom"/> is Moderate,
/// anything lower is Weak. Each boundary carries its own <c>SRC_*</c> code, since a scale can
/// mix a cited boundary with a project heuristic (Shadbala does).</summary>
public sealed record StrengthBandScale(
    string ScaleCode,
    decimal StrongFrom, string StrongSourceRefCode,
    decimal ModerateFrom, string ModerateSourceRefCode)
{
    public StrengthTier Classify(decimal? value) => value switch
    {
        null => StrengthTier.None,
        _ when value >= StrongFrom => StrengthTier.Strong,
        _ when value >= ModerateFrom => StrengthTier.Moderate,
        _ => StrengthTier.Weak,
    };
}

/// <summary>
/// The app's one set of strength cut-offs, read by Key Inference step 3 and Life Matters alike so
/// the same planet or house never gets two different labels. Mirrors <c>tbl_Rule_StrengthBand</c>
/// (migration 154) — CLI <c>verify-strength</c> checks the two agree — the same "verified mirror"
/// pattern as <c>AstroMath.DeepExaltationPoints</c>.
/// </summary>
public static class StrengthBands
{
    /// <summary>Ṣaḍbala as % of the planet's required minimum. 100% is Parāśara's own line
    /// (a planet at or above its required Ṣaḍbala is strong); the 80% Moderate floor is ours.</summary>
    public static readonly StrengthBandScale ShadbalaPercentOfMinimum =
        new("SHADBALA_PCT_OF_MIN", 100m, "SRC_BPHS_27", 80m, "SRC_IKIASTRRO_SYNTHESIS");

    /// <summary>Bhava Bala in Rūpas. No classical cut-off on file — both boundaries are ours.</summary>
    public static readonly StrengthBandScale BhavaBalaRupas =
        new("BHAVA_BALA_RUPAS", 7m, "SRC_IKIASTRRO_SYNTHESIS", 5m, "SRC_IKIASTRRO_SYNTHESIS");

    /// <summary>Sarvāṣṭakavarga bindus in a sign: above 30 favourable, below 25 unfavourable
    /// (docs/research/domain/transit-events.md) — as whole-bindu lower bounds, 31 and 25.</summary>
    public static readonly StrengthBandScale SarvaAshtakavargaBindus =
        new("SAV_BINDUS", 31m, "SRC_PVR_INTEGRATED", 25m, "SRC_PVR_INTEGRATED");

    public static IReadOnlyList<StrengthBandScale> All { get; } =
        [ShadbalaPercentOfMinimum, BhavaBalaRupas, SarvaAshtakavargaBindus];
}
