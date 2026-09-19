using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Models;

/// <summary>
/// One row of tbl_Fact_Argala (migration 128) — a single occupant of a single argala/virodhargala
/// position for a given target. Built by ArgalaFactBuilder from ArgalaCalculator's output; a
/// position with no occupants produces no row (see ArgalaCalculator for the classical rule).
/// </summary>
public sealed record ChartArgalaFact(
    string TargetKind,          // "House" or "Graha"
    string TargetKey,           // "1".."12" or "Sun".."Ketu"
    int TargetHouseNumber,      // resolved house-from-lagna of the target
    ZodiacName TargetSign,
    string RelationTypeCode,    // "ARGALA" or "VIRODHARGALA"
    int HouseOffset,            // 2/4/5/11 (argala) or 12/10/3/9 (virodhargala)
    bool IsPrimary,
    PlanetName OccupantPlanet,
    bool ExceptionApplied,
    bool CountedAntiZodiacally);
