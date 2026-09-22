namespace Ikiastrro.Core.Models;

/// <summary>One row of dbo.tbl_Rule_RasiNakshatraCombination (migration 124) — the 36 spatially
/// valid Rāśi×Nakṣatra pairs (not the full 12×27 grid; a nakṣatra either sits entirely inside one
/// rāśi or straddles exactly one boundary). CombinedCharacter/MainSignifications/PotentialBenefits/
/// PotentialDisadvantages/JudgmentNote are SRC_IKIASTRRO_SYNTHESIS (project synthesis, not a
/// verbatim classical table — see the migration's own header comment). LordRelation/AspectingRasis
/// are computed columns, not stored. Read-only.</summary>
public record RasiNakshatraCombinationRow(
    byte RasiId, byte NakshatraId,
    decimal SpanStartDegree, decimal SpanEndDegree,
    string CombinedCharacter, string MainSignifications,
    string PotentialBenefits, string PotentialDisadvantages, string JudgmentNote,
    string? LordRelation, string? AspectingRasis, string? SourceRefCode);
