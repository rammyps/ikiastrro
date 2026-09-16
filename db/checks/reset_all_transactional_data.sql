-- Full transactional-data reset: clears every generated/user row and reseeds IDENTITY to 1,
-- while leaving all tbl_Dim_* / tbl_Rule_* reference-seed data and table structures untouched.
--
-- Scope (FK-safe delete order):
--   1. tbl_Fact_*                (per-chart computed facts; children of tbl_ChartResults)
--   2. tbl_Chart_*                (per-chart analytics; children of tbl_ChartResults)
--   3. tbl_ChartResults           (children of tbl_BirthDetails)
--   4. tbl_BirthDetails           (the saved people)
--
-- One necessary side effect: tbl_Dim_AyanamsaBenchmarkCases.BirthDetailId is a nullable FK to
-- tbl_BirthDetails (NO_ACTION) used to link a benchmark case to a real person row. It is NULLed
-- out (not deleted, not any other column touched) wherever it points to a row this script is
-- about to remove, so tbl_BirthDetails can be fully cleared. The benchmark case row itself, and
-- every other tbl_Dim_*/tbl_Rule_* table, is left exactly as it was.
USE [ikiastrro];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

-------------------------------------------------------------------------------------------------
-- 1. tbl_Fact_* (children of tbl_ChartResults; two of them have Fact-to-Fact children too)
-------------------------------------------------------------------------------------------------
DELETE FROM dbo.tbl_Fact_AyanamsaPositionComparisons;
DELETE FROM dbo.tbl_Fact_DashaApplicabilityResults;
DELETE FROM dbo.tbl_Fact_DashaBenchmarkComparisons;
DELETE FROM dbo.tbl_Fact_AyanamsaComparisonRuns;

DELETE FROM dbo.tbl_Fact_YogaValidationResults;
DELETE FROM dbo.tbl_Fact_YogaValidationRuns;   -- also FKs tbl_BirthDetails directly

DELETE FROM dbo.tbl_Fact_Amsabala;
DELETE FROM dbo.tbl_Fact_AshtakavargaPinda;
DELETE FROM dbo.tbl_Fact_BhavaStrength;
DELETE FROM dbo.tbl_Fact_BhavaStrengthComponent;
DELETE FROM dbo.tbl_Fact_BhinnaAshtakavarga;
DELETE FROM dbo.tbl_Fact_BhinnaAshtakavargaContribution;
DELETE FROM dbo.tbl_Fact_HouseFromReference;
-- FKs both tbl_ChartResults and tbl_BirthDetails (db/00_add_avastha_star_schema.sql); guarded
-- because that historical one-off isn't applied on every environment (confirmed absent here).
IF OBJECT_ID('dbo.tbl_Fact_PlanetAvastha', 'U') IS NOT NULL
    DELETE FROM dbo.tbl_Fact_PlanetAvastha;
-- KP sub-lord levels 2-7 (db/095_create_kp_sublord_chain_fact.sql); guarded for the same reason.
IF OBJECT_ID('dbo.tbl_Fact_KpSubLordChain', 'U') IS NOT NULL
    DELETE FROM dbo.tbl_Fact_KpSubLordChain;
DELETE FROM dbo.tbl_Fact_PlanetaryState;
DELETE FROM dbo.tbl_Fact_PlanetaryStrength;
DELETE FROM dbo.tbl_Fact_PlanetaryStrengthComponent;
DELETE FROM dbo.tbl_Fact_SarvaAshtakavarga;
DELETE FROM dbo.tbl_Fact_Vargottama;
DELETE FROM dbo.tbl_Fact_YogaInputEvaluations;

-------------------------------------------------------------------------------------------------
-- 2. tbl_Chart_* (children of tbl_ChartResults)
-------------------------------------------------------------------------------------------------
DELETE FROM dbo.tbl_Chart_Conjunctions;               -- FKs tbl_Chart_MultiGrahaConjunction
DELETE FROM dbo.tbl_Chart_MultiGrahaConjunctionMember; -- FKs tbl_Chart_MultiGrahaConjunction (CASCADE, explicit anyway)
DELETE FROM dbo.tbl_Chart_MultiGrahaConjunction;
DELETE FROM dbo.tbl_Chart_Aspects;
DELETE FROM dbo.tbl_Chart_HouseLords;
DELETE FROM dbo.tbl_Chart_KeyDetails;
DELETE FROM dbo.tbl_Chart_DashaPeriods;               -- self-referencing (ParentDashaPeriodId); whole-table delete is safe
DELETE FROM dbo.tbl_Chart_Panchanga;

-------------------------------------------------------------------------------------------------
-- 3. tbl_ChartResults
-------------------------------------------------------------------------------------------------
DELETE FROM dbo.tbl_ChartResults;

-------------------------------------------------------------------------------------------------
-- 4. tbl_BirthDetails — release the one benchmark-case FK first, then clear the people
-------------------------------------------------------------------------------------------------
UPDATE dbo.tbl_Dim_AyanamsaBenchmarkCases
SET BirthDetailId = NULL
WHERE BirthDetailId IS NOT NULL;

DELETE FROM dbo.tbl_BirthDetails;

COMMIT TRANSACTION;

-------------------------------------------------------------------------------------------------
-- Reseed every touched IDENTITY back to 0 (next insert = 1)
-------------------------------------------------------------------------------------------------
DBCC CHECKIDENT ('dbo.tbl_Fact_AyanamsaPositionComparisons', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_DashaApplicabilityResults', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_DashaBenchmarkComparisons', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_AyanamsaComparisonRuns', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_YogaValidationResults', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_YogaValidationRuns', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_Amsabala', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_AshtakavargaPinda', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_BhavaStrength', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_BhavaStrengthComponent', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_BhinnaAshtakavarga', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_BhinnaAshtakavargaContribution', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_HouseFromReference', RESEED, 0);
IF OBJECT_ID('dbo.tbl_Fact_PlanetAvastha', 'U') IS NOT NULL
    DBCC CHECKIDENT ('dbo.tbl_Fact_PlanetAvastha', RESEED, 0);
IF OBJECT_ID('dbo.tbl_Fact_KpSubLordChain', 'U') IS NOT NULL
    DBCC CHECKIDENT ('dbo.tbl_Fact_KpSubLordChain', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_PlanetaryState', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_PlanetaryStrength', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_PlanetaryStrengthComponent', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_SarvaAshtakavarga', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_Vargottama', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Fact_YogaInputEvaluations', RESEED, 0);

DBCC CHECKIDENT ('dbo.tbl_Chart_Conjunctions', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Chart_MultiGrahaConjunctionMember', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Chart_MultiGrahaConjunction', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Chart_Aspects', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Chart_HouseLords', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Chart_KeyDetails', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Chart_DashaPeriods', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_Chart_Panchanga', RESEED, 0);

DBCC CHECKIDENT ('dbo.tbl_ChartResults', RESEED, 0);
DBCC CHECKIDENT ('dbo.tbl_BirthDetails', RESEED, 0);
GO
