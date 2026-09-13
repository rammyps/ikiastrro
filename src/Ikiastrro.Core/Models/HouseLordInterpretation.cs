namespace Ikiastrro.Core.Models;

/// <summary>
/// One row of <c>dbo.vw_ChartHouseLordInterpretation</c> — a classical claim (from
/// <c>tbl_Rule_HouseLordPlacement</c>) matched to one house's actually-computed lord placement
/// (<c>tbl_Chart_HouseLords</c>) for a given chart. A house can have more than one row: a
/// <c>BASELINE</c> claim always, plus <c>WELL_DISPOSED</c> and/or <c>AFFLICTED</c> branches
/// wherever the source states a distinct one. Read-only — this is a view, not a fact table, so
/// there is no InsertAll/Delete here (see db/094_promote_house_lord_placement_claims.sql).
/// </summary>
public class HouseLordInterpretation
{
    public int ChartResultId { get; set; }
    public int OwnedHouseNumber { get; set; }
    public string HouseSign { get; set; } = string.Empty;
    public string LordPlanet { get; set; } = string.Empty;
    public int OccupiedHouseNumber { get; set; }
    public string LordPlacedInSign { get; set; } = string.Empty;
    public string? LordDignityStatus { get; set; }

    public int RuleSetId { get; set; }
    /// <summary>"BASELINE" | "WELL_DISPOSED" | "AFFLICTED".</summary>
    public string BranchCode { get; set; } = string.Empty;
    public string ResultText { get; set; } = string.Empty;
    public string? EvaluableTodayCode { get; set; }
    public string InterpretationStatusCode { get; set; } = string.Empty;
    public string SourceRefCode { get; set; } = string.Empty;
    public string HouseSystemCode { get; set; } = string.Empty;
    public string ReferencePointCode { get; set; } = string.Empty;

    /// <summary>True when this is a WELL_DISPOSED branch and the lord's actual dignity supports it
    /// (Exalted/Moolatrikona/Own Sign/Great Friend). Never filters — every branch is still
    /// returned so the caller decides; see the view's own comment in db/094.</summary>
    public bool IsDignitySupported { get; set; }
}
