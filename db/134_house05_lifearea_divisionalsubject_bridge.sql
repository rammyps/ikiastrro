-- =====================================================================
-- 134 - FEAT-HOUSE-05: LifeArea <-> DivisionalSubject reconciliation.
--
-- Migration 109 already built tbl_Dim_InterpretiveFactor + the bridge table
-- tbl_Rule_InterpretiveFactorDetail (LifeAreaId/DivisionalSubjectCode/
-- KarakaRoleId typed columns) and seeded all 11 DivisionalSubject rows'
-- House/Planet/Varga facts -- this was found mid-build, not something this
-- migration needed to (re-)create; the design doc that proposed
-- FEAT-HOUSE-05 (lifearea-varga-charakaraka-synthesis.md, written
-- 2026-09-22) missed that 109 already existed. This migration does only
-- the two pieces 109 explicitly deferred for LifeArea:
--
-- (1) Leg A/B reconciliation policy, decided 2026-09-23 (rammyps): amend
--     Leg A (tbl_Dim_LifeArea.Description) to match Leg B
--     (tbl_Dim_DivisionalSubject)'s narrower framing wherever the same
--     ground overlaps (10 of 20 LifeArea rows -- the 10 chart types
--     tbl_Dim_DivisionalSubject also covers). Still SRC_PVR_INTEGRATED
--     throughout -- narrows existing wording, not a new claim.
-- (2) LifeArea VARGA facts in tbl_Rule_InterpretiveFactorDetail (109's own
--     header: "LifeArea ... details left for a follow-up migration").
--     Derived entirely from the existing tbl_Dim_ChartType.PrimaryLifeAreaId
--     FK -- no manual transcription.
--
-- CHARA_KARAKA_ROLE (Leg D) stays unseeded, same as 109 left it -- no
-- source content to seed from yet (the design doc's VARGA=D9/SIGN_LAGNA
-- example was illustrative, not a real table). Explicit follow-up, not
-- guessed.
--
-- Idempotent throughout.
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/134_house05_lifearea_divisionalsubject_bridge.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: amend Leg A wording to match Leg B's narrower framing ---
UPDATE la
SET la.Description = v.NewDescription
FROM dbo.tbl_Dim_LifeArea la
JOIN dbo.tbl_Dim_ChartType ct ON ct.PrimaryLifeAreaId = la.Id
JOIN (VALUES
    ('D2',  N'Capacity to accumulate, preserve and use money and resources (PVR Table 11, D-2).'),
    ('D3',  N'Siblings, co-born relationships, courage, initiative and sustained effort (PVR Table 11, D-3).'),
    ('D4',  N'Residence, houses, land, property ownership and fortune connected with assets (PVR Table 11, D-4).'),
    ('D7',  N'Children, fertility, progeny and the relationship with children (PVR Table 11, D-7).'),
    ('D9',  N'Spouse, marriage quality, relationship dharma and long-term partnership (PVR Table 11, D-9).'),
    ('D10', N'Profession, authority, achievements, recognition and activity in society (PVR Table 11, D-10).'),
    ('D12', N'Parents, ancestry and inherited family patterns; read with the D1 4th and 9th houses (PVR Table 11, D-12).'),
    ('D16', N'Vehicles, pleasures, comforts and the ability to enjoy material conveniences (PVR Table 11, D-16).'),
    ('D24', N'Formal education, learning, scholarship, examinations and mastery (PVR Table 11, D-24).'),
    ('D60', N'Deep karmic causes and subtle confirmation of major life patterns; birth-time sensitive (PVR Table 11, D-60).')
) v(ChartCode, NewDescription) ON v.ChartCode = ct.Code
WHERE la.Description <> v.NewDescription;
GO

-- --- Batch 2: LifeArea VARGA facts -- fully derived from the existing
-- tbl_Dim_ChartType.PrimaryLifeAreaId FK, no manual transcription. FactorId
-- 4 = VARGA (seeded by migration 109; fixed catalogue, not re-looked-up). ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_InterpretiveFactorDetail WHERE FactorId = 4 AND LifeAreaId IS NOT NULL)
    INSERT dbo.tbl_Rule_InterpretiveFactorDetail
        (RuleSetId, LifeAreaId, FactorId, ChartTypeId, IsPrimary, DisplayOrder, SourceRefCode)
    SELECT rs.Id, la.Id, 4, ct.Id, 1, 1, la.SourceRefCode
    FROM dbo.tbl_Dim_LifeArea la
    JOIN dbo.tbl_Dim_ChartType ct ON ct.PrimaryLifeAreaId = la.Id
    CROSS JOIN dbo.tbl_Rule_Sets rs
    WHERE rs.IsActive = 1;
GO

-- --- Batch 3: tbl_Rule_Catalog note update (rows already registered by 109) ---
UPDATE dbo.tbl_Rule_Catalog
SET Purpose = 'Normalizes House/Planet/Sign/Varga facts for tbl_Dim_LifeArea (all 20 rows, migration 134), tbl_Dim_DivisionalSubject (all 11 rows, migration 109) and CHARA tbl_Dim_KarakaRole rows (not yet seeded) into queryable rows instead of free text.'
WHERE RuleTableName = 'tbl_Rule_InterpretiveFactorDetail';
GO

-- --- Batch 4: ledger + verification summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '134_house05_lifearea_divisionalsubject_bridge.sql',
       'Amended 10 LifeArea.Description rows to match DivisionalSubject; seeded LifeArea VARGA rows into 109''s InterpretiveFactorDetail. FEAT-HOUSE-05.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '134_house05_lifearea_divisionalsubject_bridge.sql');
GO

DECLARE @amended INT = (
    SELECT COUNT(*) FROM dbo.tbl_Dim_LifeArea la
    JOIN dbo.tbl_Dim_ChartType ct ON ct.PrimaryLifeAreaId = la.Id
    JOIN (VALUES
        ('D2',  N'Capacity to accumulate, preserve and use money and resources (PVR Table 11, D-2).'),
        ('D3',  N'Siblings, co-born relationships, courage, initiative and sustained effort (PVR Table 11, D-3).'),
        ('D4',  N'Residence, houses, land, property ownership and fortune connected with assets (PVR Table 11, D-4).'),
        ('D7',  N'Children, fertility, progeny and the relationship with children (PVR Table 11, D-7).'),
        ('D9',  N'Spouse, marriage quality, relationship dharma and long-term partnership (PVR Table 11, D-9).'),
        ('D10', N'Profession, authority, achievements, recognition and activity in society (PVR Table 11, D-10).'),
        ('D12', N'Parents, ancestry and inherited family patterns; read with the D1 4th and 9th houses (PVR Table 11, D-12).'),
        ('D16', N'Vehicles, pleasures, comforts and the ability to enjoy material conveniences (PVR Table 11, D-16).'),
        ('D24', N'Formal education, learning, scholarship, examinations and mastery (PVR Table 11, D-24).'),
        ('D60', N'Deep karmic causes and subtle confirmation of major life patterns; birth-time sensitive (PVR Table 11, D-60).')
    ) v(ChartCode, NewDescription) ON v.ChartCode = ct.Code AND v.NewDescription = la.Description
);
DECLARE @laVarga INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_InterpretiveFactorDetail WHERE FactorId = 4 AND LifeAreaId IS NOT NULL);
DECLARE @dsTotal INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_InterpretiveFactorDetail WHERE DivisionalSubjectCode IS NOT NULL);
PRINT '134 applied: ' + CAST(@amended AS VARCHAR(10)) + ' LifeArea rows amended (expect 10), '
    + CAST(@laVarga AS VARCHAR(10)) + ' LifeArea VARGA rows (expect 21 -- one per ChartType with a PrimaryLifeAreaId; WEALTH has both D2 and D2-US), '
    + CAST(@dsTotal AS VARCHAR(10)) + ' DivisionalSubject detail rows untouched from migration 109 (expect 41)';
GO
