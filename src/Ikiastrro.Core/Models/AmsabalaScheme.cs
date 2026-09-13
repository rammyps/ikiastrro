namespace Ikiastrro.Core.Models;

/// <summary>
/// One row of dbo.tbl_Rule_AmsabalaGroup - PVR Sec. 6.6: one varga chart type's membership
/// in a named amsabala scheme (SHADVARGA/SAPTAVARGA/DASAVARGA/SHODASAVARGA).
/// </summary>
public record AmsabalaGroupMember(string SchemeCode, string ChartType);

/// <summary>
/// One row of dbo.tbl_Rule_AmsabalaName - PVR Sec. 6.6: the amsa name for a given count of
/// "good" (own/moolatrikona/exalted) placements within a scheme.
/// </summary>
public record AmsabalaNameEntry(string SchemeCode, int GoodCount, string AmsaName);
