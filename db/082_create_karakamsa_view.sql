-- =====================================================================
-- 082 — Karakamsa (AK in D9) convenience view.
--
-- No new computation or storage: the D9 Chara Karaka label is already
-- stamped onto every chart's graha rows (ChartGenerationService.PersistAnalytics
-- applies CharaKarakaByPlanet to every ChartType, not just D1 — FEAT-KARAKA-01).
-- The Atma Karaka's D9 sign IS the classical "Karakamsa" (PVR sec 7.3.6,
-- tbl_Dim_HouseReference row KARAKAMSA_LAGNA, migration 32); it was simply
-- never surfaced as its own read. This view is that read.
--
-- Verified against the JHora export for 1_Ramakrishnan: AK = Rahu, D9 sign
-- Libra — matches Rammy_Jagannatha.txt's planet table (Rahu row, Navamsa
-- column "Li") exactly.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_ChartKarakamsa
AS
SELECT c.BirthDetailId,
       kd.Planet AS AtmaKarakaPlanet,
       kd.Sign AS KarakamsaSign,
       kd.NirayanaLongitudeDegrees AS AtmaKarakaD9Longitude,
       kd.Nakshatra AS AtmaKarakaD9Nakshatra,
       kd.NakshatraPada AS AtmaKarakaD9Pada
FROM dbo.tbl_Chart_KeyDetails kd
JOIN dbo.tbl_ChartResults c ON c.Id = kd.ChartResultId
WHERE c.ChartType = 'D9' AND kd.CharaKaraka = 'AK';
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '082_create_karakamsa_view.sql',
       'vw_ChartKarakamsa: the D9 sign of AK (Karakamsa Lagna, PVR sec 7.3.6) - a read over existing tbl_Chart_KeyDetails, no new storage.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '082_create_karakamsa_view.sql');
GO

PRINT '082 applied: vw_ChartKarakamsa created.';
GO
