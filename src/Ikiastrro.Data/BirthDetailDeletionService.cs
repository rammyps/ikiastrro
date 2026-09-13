namespace Ikiastrro.Data;

/// <summary>
/// Deletes a BirthDetails record and every chart artifact derived from it — all chart types (D1, D9,
/// and any future divisional chart) at once, since the analytical tables are shared across chart
/// types and already scoped by BirthDetailId. Only the tbl_BirthDetails row itself is removed; nothing
/// about the tables/columns/repositories is touched.
///
/// FK-safe order: the analytical tables (leaves, reference tbl_ChartResults) -> tbl_ChartResults
/// (references tbl_BirthDetails) -> tbl_BirthDetails. The multi-graha conjunction groups are deleted
/// AFTER tbl_Chart_Conjunctions (its pair rows carry an FK to the groups); members cascade with the
/// group. Sequential, un-transacted calls — same style as
/// every other multi-step write in this project (e.g. the CLI/Web save flow), not wrapped in an
/// explicit SQL transaction.
///
/// Every table with a NO_ACTION FK to tbl_ChartResults must be cleared here, or the final
/// tbl_ChartResults delete throws SqlException 547 and — from the Web delete button — takes the
/// whole Blazor circuit down ("An unhandled error has occurred"). The strength / bhava-bala /
/// vargottama fact tables were added after this service and were missed; they are the same set
/// GenerateAll clears up-front. (tbl_Fact_YogaInputEvaluations FKs with CASCADE, so it needs no
/// explicit delete; tbl_Fact_HouseFromReference, tbl_Fact_KpSubLordChain, tbl_Fact_PlanetAvastha,
/// tbl_Fact_AshtakavargaPinda/BhinnaAshtakavarga(Contribution)/SarvaAshtakavarga are schema-only —
/// no calculator populates them yet, so there is nothing for any person to leave behind; wiring one
/// up must add its delete here too, the same mistake this class's own history warns about.
/// tbl_Dim_AyanamsaBenchmarkCases.BirthDetailId is the one other NO_ACTION reference to
/// tbl_BirthDetails itself (not tbl_ChartResults) — nullable and unlinked via
/// BirthDetailsRepository.UnlinkAyanamsaBenchmarkCases rather than deleted, same as the full-reset
/// script's fix for the same FK.)
/// </summary>
public class BirthDetailDeletionService
{
    private readonly ChartConjunctionsRepository _conjunctionsRepo;
    private readonly ChartMultiGrahaConjunctionRepository _multiGrahaConjunctionsRepo;
    private readonly ChartAspectsRepository _aspectsRepo;
    private readonly ChartKeyDetailsRepository _keyDetailsRepo;
    private readonly ChartHouseLordsRepository _houseLordsRepo;
    private readonly PlanetaryStateRepository _planetaryStateRepo;
    private readonly PlanetaryStrengthRepository _planetaryStrengthRepo;
    private readonly BhavaStrengthRepository _bhavaStrengthRepo;
    private readonly VargottamaRepository _vargottamaRepo;
    private readonly DashaPeriodsRepository _dashaPeriodsRepo;
    private readonly ChartResultsRepository _chartResultsRepo;
    private readonly BirthDetailsRepository _birthDetailsRepo;

    public BirthDetailDeletionService(
        ChartConjunctionsRepository conjunctionsRepo,
        ChartMultiGrahaConjunctionRepository multiGrahaConjunctionsRepo,
        ChartAspectsRepository aspectsRepo,
        ChartKeyDetailsRepository keyDetailsRepo,
        ChartHouseLordsRepository houseLordsRepo,
        PlanetaryStateRepository planetaryStateRepo,
        PlanetaryStrengthRepository planetaryStrengthRepo,
        BhavaStrengthRepository bhavaStrengthRepo,
        VargottamaRepository vargottamaRepo,
        DashaPeriodsRepository dashaPeriodsRepo,
        ChartResultsRepository chartResultsRepo,
        BirthDetailsRepository birthDetailsRepo)
    {
        _conjunctionsRepo = conjunctionsRepo;
        _multiGrahaConjunctionsRepo = multiGrahaConjunctionsRepo;
        _aspectsRepo = aspectsRepo;
        _keyDetailsRepo = keyDetailsRepo;
        _houseLordsRepo = houseLordsRepo;
        _planetaryStateRepo = planetaryStateRepo;
        _planetaryStrengthRepo = planetaryStrengthRepo;
        _bhavaStrengthRepo = bhavaStrengthRepo;
        _vargottamaRepo = vargottamaRepo;
        _dashaPeriodsRepo = dashaPeriodsRepo;
        _chartResultsRepo = chartResultsRepo;
        _birthDetailsRepo = birthDetailsRepo;
    }

    public void DeleteBirthDetail(int birthDetailId)
    {
        _conjunctionsRepo.DeleteByBirthDetailId(birthDetailId);
        _multiGrahaConjunctionsRepo.DeleteByBirthDetailId(birthDetailId);  // after the pair rows (they FK the groups)
        _aspectsRepo.DeleteByBirthDetailId(birthDetailId);
        _keyDetailsRepo.DeleteByBirthDetailId(birthDetailId);
        _houseLordsRepo.DeleteByBirthDetailId(birthDetailId);
        _planetaryStateRepo.DeleteByBirthDetailId(birthDetailId);
        _planetaryStrengthRepo.DeleteByBirthDetailId(birthDetailId);   // FK_Fact_PlanetaryStrength_ChartResult (no cascade)
        _bhavaStrengthRepo.DeleteByBirthDetailId(birthDetailId);       // FK_Fact_BhavaStrength_ChartResult (no cascade)
        _vargottamaRepo.DeleteByBirthDetailId(birthDetailId);          // FK_Fact_Vargottama_ChartResult (no cascade)
        _dashaPeriodsRepo.DeleteByBirthDetailId(birthDetailId);
        _chartResultsRepo.DeleteByBirthDetailId(birthDetailId);
        _birthDetailsRepo.UnlinkAyanamsaBenchmarkCases(birthDetailId);  // release the FK, don't delete the benchmark case
        _birthDetailsRepo.Delete(birthDetailId);
    }
}
