-- =====================================================================
-- 139 - Seed tbl_Rule_LifeMatterSubject and tbl_Rule_LifeMatterFocus.
--
-- Populates the two bridge tables migration 137 created empty. Every row
-- below is a literal transcription of the finalized proposal table in
-- docs/ui/lifematters_claude_research.md ("Proposed Subject and Focus
-- mappings"), closed out per the three resolution decisions recorded in
-- docs/ui/lifematters_plan.md's Approved decisions 9-11:
--
--   9.  Compound/ambiguous PrimaryChartsText (e.g. "D2/D6") resolves
--       Subject to whichever half already has a matching DivisionalSubject,
--       when exactly one does - never guessed when neither or both
--       plausibly match. MARRIAGE_SPOUSE_05 resolves to
--       MARRIAGE_RELATIONSHIPS (stays in its own category's theme).
--       TROUBLE_LOSS_09 (no literal chart code at all) resolves to NO
--       MATCH for Subject; its Focus row still seeds independently.
--  10.  The 32-row PVR_KARAKATWA_GRID batch seeds Focus only, never
--       Subject - single-house natural correspondences with no Varga of
--       their own, so a same-house Subject match would manufacture
--       precision the source doesn't have.
--  11.  CHILDREN_06's derived reference ('9th from 5th') seeds its
--       arithmetically-flattened result, House(LAGNA,1), with the
--       derivation preserved only as a code comment - no schema change
--       for one row.
--
-- 32 of the 96 PVR_LIFE_MATTER rows get no Subject at all: all 11
-- SELF_HEALTH rows and all 8 SPIRITUALITY rows (no DivisionalSubject
-- targets D1/D6/D8/D20/D27/D30), FAMILY_RELATIONSHIPS_06 ("Friends",
-- D1/D9 - theme doesn't fit MARRIAGE_RELATIONSHIPS despite the D9 half
-- technically matching), CAREER_STATUS_10 ("Fame", D5 alone, no D5
-- subject), and 11 of 12 TROUBLE_LOSS rows (only row 3's D2 half maps to
-- WEALTH). That leaves 64 Subject rows.
--
-- Focus seeds for all 96+32 rows; several 96-row-set rows contribute two
-- House rows for a compound HouseFromLagnaText ("9th/12th" etc.), giving
-- 104 Focus rows from that set + 32 from the Karakatwa batch = 136.
-- MARRIAGE_SPOUSE_06 is the sole SpecialPoint row (A12, the corrected
-- Upapada Lagna point from migration 138 - never a 7th-house row).
--
-- SourceRefCode policy: Subject rows are always SRC_IKIASTRRO_SYNTHESIS -
-- correlating a chart to one of the 11 pre-built DivisionalSubject rows
-- is this project's own taxonomy work, not something PVR's text asserts
-- directly. Focus rows for the 96-row set inherit whatever SourceRefCode
-- tbl_Rule_LifeMatterReference already recorded for that same row (most
-- are SRC_PVR_INTEGRATED; a handful the 087 seed itself already flagged
-- SRC_IKIASTRRO_SYNTHESIS, e.g. CHILDREN_06's own derived-reference row -
-- Focus just re-expresses the same HouseFromLagnaText, so its provenance
-- should match). The 32-row Karakatwa batch has no LifeMatterReference
-- row to inherit from; its own KarakaMatter rows are uniformly
-- SRC_PVR_INTEGRATED, carried forward here too.
--
-- Full data source: docs/ui/lifematters_claude_research.md, "Proposed
-- Subject and Focus mappings" (both sub-tables).
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/139_seed_life_matter_subject_focus.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- ---------------------------------------------------------------------
-- Subject rows (64) - tbl_Rule_LifeMatterSubject
-- ---------------------------------------------------------------------
;WITH SubjectSeed(Code, DivisionalSubjectCode) AS (
    SELECT * FROM (VALUES
        ('WEALTH_01','WEALTH'), ('WEALTH_02','WEALTH'), ('WEALTH_03','WEALTH'), ('WEALTH_04','WEALTH'),
        ('WEALTH_05','WEALTH'), ('WEALTH_06','WEALTH'), ('WEALTH_07','WEALTH'), ('WEALTH_08','WEALTH'),

        ('EDUCATION_01','EDUCATION_LEARNING'), ('EDUCATION_02','EDUCATION_LEARNING'),
        ('EDUCATION_03','EDUCATION_LEARNING'), ('EDUCATION_04','EDUCATION_LEARNING'),
        ('EDUCATION_05','EDUCATION_LEARNING'), ('EDUCATION_06','EDUCATION_LEARNING'),
        ('EDUCATION_07','EDUCATION_LEARNING'), ('EDUCATION_08','EDUCATION_LEARNING'),
        ('EDUCATION_09','EDUCATION_LEARNING'), ('EDUCATION_10','EDUCATION_LEARNING'),
        ('EDUCATION_11','EDUCATION_LEARNING'), ('EDUCATION_12','EDUCATION_LEARNING'),
        ('EDUCATION_13','EDUCATION_LEARNING'),

        ('PROPERTY_COMFORTS_01','PROPERTY_RESIDENCE'), ('PROPERTY_COMFORTS_02','PROPERTY_RESIDENCE'),
        ('PROPERTY_COMFORTS_03','PROPERTY_RESIDENCE'), ('PROPERTY_COMFORTS_04','PROPERTY_RESIDENCE'),
        ('PROPERTY_COMFORTS_05','PROPERTY_RESIDENCE'),
        ('PROPERTY_COMFORTS_06','VEHICLES_COMFORTS'), ('PROPERTY_COMFORTS_07','VEHICLES_COMFORTS'),
        ('PROPERTY_COMFORTS_08','VEHICLES_COMFORTS'), ('PROPERTY_COMFORTS_09','VEHICLES_COMFORTS'),
        ('PROPERTY_COMFORTS_10','VEHICLES_COMFORTS'),

        ('FAMILY_RELATIONSHIPS_01','WEALTH'),
        ('FAMILY_RELATIONSHIPS_02','MOTHER_PARENTS'), ('FAMILY_RELATIONSHIPS_03','MOTHER_PARENTS'),
        ('FAMILY_RELATIONSHIPS_04','SIBLINGS_COURAGE'), ('FAMILY_RELATIONSHIPS_05','SIBLINGS_COURAGE'),
        ('FAMILY_RELATIONSHIPS_07','CAREER_STATUS'), ('FAMILY_RELATIONSHIPS_08','CAREER_STATUS'),
        ('FAMILY_RELATIONSHIPS_09','CAREER_STATUS'),

        ('MARRIAGE_SPOUSE_01','MARRIAGE_RELATIONSHIPS'), ('MARRIAGE_SPOUSE_02','MARRIAGE_RELATIONSHIPS'),
        ('MARRIAGE_SPOUSE_03','MARRIAGE_RELATIONSHIPS'), ('MARRIAGE_SPOUSE_04','MARRIAGE_RELATIONSHIPS'),
        ('MARRIAGE_SPOUSE_05','MARRIAGE_RELATIONSHIPS'), ('MARRIAGE_SPOUSE_06','MARRIAGE_RELATIONSHIPS'),
        ('MARRIAGE_SPOUSE_07','MARRIAGE_RELATIONSHIPS'),

        ('CHILDREN_01','CHILDREN_PROGENY'), ('CHILDREN_02','CHILDREN_PROGENY'),
        ('CHILDREN_03','CHILDREN_PROGENY'), ('CHILDREN_04','CHILDREN_PROGENY'),
        ('CHILDREN_05','CHILDREN_PROGENY'), ('CHILDREN_06','CHILDREN_PROGENY'),

        ('CAREER_STATUS_01','CAREER_STATUS'), ('CAREER_STATUS_02','CAREER_STATUS'),
        ('CAREER_STATUS_03','CAREER_STATUS'), ('CAREER_STATUS_04','CAREER_STATUS'),
        ('CAREER_STATUS_05','CAREER_STATUS'), ('CAREER_STATUS_06','CAREER_STATUS'),
        ('CAREER_STATUS_07','CAREER_STATUS'), ('CAREER_STATUS_08','CAREER_STATUS'),
        ('CAREER_STATUS_09','CAREER_STATUS'), ('CAREER_STATUS_11','CAREER_STATUS'),
        ('CAREER_STATUS_12','CAREER_STATUS'),

        ('TROUBLE_LOSS_03','WEALTH')
    ) AS v(Code, DivisionalSubjectCode)
)
INSERT dbo.tbl_Rule_LifeMatterSubject (RuleSetId, LifeMatterId, DivisionalSubjectCode, SourceRefCode)
SELECT 1, lm.Id, s.DivisionalSubjectCode, 'SRC_IKIASTRRO_SYNTHESIS'
FROM SubjectSeed s
JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Code = s.Code
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterSubject x
    WHERE x.RuleSetId = 1 AND x.LifeMatterId = lm.Id AND x.IsActive = 1
);
GO

-- ---------------------------------------------------------------------
-- Focus rows, House kind (135) - tbl_Rule_LifeMatterFocus
-- 96-row set (103 House rows) inherits SourceRefCode from the matching
-- tbl_Rule_LifeMatterReference row; the 32-row Karakatwa batch (no such
-- row to inherit from) uses SRC_PVR_INTEGRATED directly, matching its
-- own tbl_Rule_KarakaMatter rows.
-- ---------------------------------------------------------------------
;WITH FocusHouseSeed(Code, HouseNumber) AS (
    SELECT * FROM (VALUES
        -- SELF_HEALTH (13 rows)
        ('SELF_HEALTH_01',1), ('SELF_HEALTH_02',1), ('SELF_HEALTH_03',6),
        ('SELF_HEALTH_04',8), ('SELF_HEALTH_05',6), ('SELF_HEALTH_05',8),
        ('SELF_HEALTH_06',12), ('SELF_HEALTH_07',1), ('SELF_HEALTH_08',4),
        ('SELF_HEALTH_09',1), ('SELF_HEALTH_10',3), ('SELF_HEALTH_11',1), ('SELF_HEALTH_11',8),

        -- WEALTH (8 rows)
        ('WEALTH_01',1), ('WEALTH_02',2), ('WEALTH_03',2), ('WEALTH_04',5),
        ('WEALTH_05',6), ('WEALTH_06',11), ('WEALTH_07',11), ('WEALTH_08',12),

        -- EDUCATION (13 House rows; row 13 is ARUDHA_LAGNA, seeded separately below)
        ('EDUCATION_01',1), ('EDUCATION_02',4), ('EDUCATION_03',4), ('EDUCATION_04',5),
        ('EDUCATION_05',5), ('EDUCATION_06',5), ('EDUCATION_07',5), ('EDUCATION_08',5),
        ('EDUCATION_09',3), ('EDUCATION_10',9), ('EDUCATION_11',9),
        ('EDUCATION_12',10), ('EDUCATION_12',11),

        -- PROPERTY_COMFORTS (11 rows)
        ('PROPERTY_COMFORTS_01',4), ('PROPERTY_COMFORTS_02',4), ('PROPERTY_COMFORTS_03',9),
        ('PROPERTY_COMFORTS_04',9), ('PROPERTY_COMFORTS_04',12), ('PROPERTY_COMFORTS_05',3),
        ('PROPERTY_COMFORTS_06',4), ('PROPERTY_COMFORTS_07',4), ('PROPERTY_COMFORTS_08',4),
        ('PROPERTY_COMFORTS_09',12), ('PROPERTY_COMFORTS_10',3),

        -- FAMILY_RELATIONSHIPS (9 rows)
        ('FAMILY_RELATIONSHIPS_01',2), ('FAMILY_RELATIONSHIPS_02',4), ('FAMILY_RELATIONSHIPS_03',9),
        ('FAMILY_RELATIONSHIPS_04',3), ('FAMILY_RELATIONSHIPS_05',11), ('FAMILY_RELATIONSHIPS_06',11),
        ('FAMILY_RELATIONSHIPS_07',5), ('FAMILY_RELATIONSHIPS_08',6), ('FAMILY_RELATIONSHIPS_09',9),

        -- MARRIAGE_SPOUSE (6 House rows; row 6 is SpecialPoint, seeded separately below)
        ('MARRIAGE_SPOUSE_01',7), ('MARRIAGE_SPOUSE_02',7), ('MARRIAGE_SPOUSE_03',7),
        ('MARRIAGE_SPOUSE_04',7), ('MARRIAGE_SPOUSE_05',12), ('MARRIAGE_SPOUSE_07',7),

        -- CHILDREN (6 rows; row 6 is the flattened '9th from 5th' -> 1st)
        ('CHILDREN_01',5), ('CHILDREN_02',5), ('CHILDREN_03',7),
        ('CHILDREN_04',9), ('CHILDREN_05',11), ('CHILDREN_06',1),

        -- CAREER_STATUS (13 rows)
        ('CAREER_STATUS_01',1), ('CAREER_STATUS_02',3), ('CAREER_STATUS_03',6),
        ('CAREER_STATUS_04',6), ('CAREER_STATUS_05',7), ('CAREER_STATUS_06',9),
        ('CAREER_STATUS_07',10), ('CAREER_STATUS_08',10), ('CAREER_STATUS_09',5),
        ('CAREER_STATUS_09',10), ('CAREER_STATUS_10',5), ('CAREER_STATUS_11',11), ('CAREER_STATUS_12',12),

        -- SPIRITUALITY (9 rows)
        ('SPIRITUALITY_01',1), ('SPIRITUALITY_02',5), ('SPIRITUALITY_03',9), ('SPIRITUALITY_04',9),
        ('SPIRITUALITY_05',9), ('SPIRITUALITY_06',9), ('SPIRITUALITY_06',12),
        ('SPIRITUALITY_07',12), ('SPIRITUALITY_08',12),

        -- TROUBLE_LOSS (14 rows)
        ('TROUBLE_LOSS_01',6), ('TROUBLE_LOSS_02',6), ('TROUBLE_LOSS_03',6),
        ('TROUBLE_LOSS_04',6), ('TROUBLE_LOSS_04',8), ('TROUBLE_LOSS_05',6), ('TROUBLE_LOSS_05',8),
        ('TROUBLE_LOSS_06',8), ('TROUBLE_LOSS_07',8), ('TROUBLE_LOSS_08',8),
        ('TROUBLE_LOSS_09',12), ('TROUBLE_LOSS_10',12), ('TROUBLE_LOSS_11',8), ('TROUBLE_LOSS_12',8)
    ) AS v(Code, HouseNumber)
)
INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SourceRefCode)
SELECT 1, lm.Id, 'House', 'LAGNA', f.HouseNumber, r.SourceRefCode
FROM FocusHouseSeed f
JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Code = f.Code
JOIN dbo.tbl_Rule_LifeMatterReference r ON r.LifeMatterId = lm.Id AND r.RuleSetId = 1
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus x
    WHERE x.RuleSetId = 1 AND x.LifeMatterId = lm.Id AND x.IsActive = 1
      AND x.FocusKind = 'House' AND x.ReferenceCode = 'LAGNA' AND x.HouseNumber = f.HouseNumber
);
GO

-- EDUCATION_13: '5th from AL', the sole ARUDHA_LAGNA Focus row.
INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SourceRefCode)
SELECT 1, lm.Id, 'House', 'ARUDHA_LAGNA', 5, r.SourceRefCode
FROM dbo.tbl_Dim_LifeMatter lm
JOIN dbo.tbl_Rule_LifeMatterReference r ON r.LifeMatterId = lm.Id AND r.RuleSetId = 1
WHERE lm.Code = 'EDUCATION_13'
  AND NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus x
    WHERE x.RuleSetId = 1 AND x.LifeMatterId = lm.Id AND x.IsActive = 1
      AND x.FocusKind = 'House' AND x.ReferenceCode = 'ARUDHA_LAGNA' AND x.HouseNumber = 5
  );
GO

-- MARRIAGE_SPOUSE_06: Upapada Lagna, the corrected A12 SpecialPoint row
-- (migration 138) - never a 7th-house row, never the raw alias 'UL'.
INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, SpecialPointCode, SourceRefCode)
SELECT 1, lm.Id, 'SpecialPoint', 'A12', r.SourceRefCode
FROM dbo.tbl_Dim_LifeMatter lm
JOIN dbo.tbl_Rule_LifeMatterReference r ON r.LifeMatterId = lm.Id AND r.RuleSetId = 1
WHERE lm.Code = 'MARRIAGE_SPOUSE_06'
  AND NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus x
    WHERE x.RuleSetId = 1 AND x.LifeMatterId = lm.Id AND x.IsActive = 1
      AND x.FocusKind = 'SpecialPoint' AND x.SpecialPointCode = 'A12'
  );
GO

-- ---------------------------------------------------------------------
-- Focus rows (32) - PVR_KARAKATWA_GRID batch, Focus only (decision 10).
-- HouseNumber comes from tbl_Rule_KarakaMatter (the only place these 32
-- rows' natural-house position lives; tbl_Dim_LifeMatter itself carries
-- no HouseNumber column). SourceRefCode matches those same rows.
-- ---------------------------------------------------------------------
INSERT dbo.tbl_Rule_LifeMatterFocus (RuleSetId, LifeMatterId, FocusKind, ReferenceCode, HouseNumber, SourceRefCode)
SELECT 1, k.LifeMatterId, 'House', 'LAGNA', k.HouseNumber, 'SRC_PVR_INTEGRATED'
FROM (
    SELECT DISTINCT km.LifeMatterId, km.HouseNumber
    FROM dbo.tbl_Rule_KarakaMatter km
    JOIN dbo.tbl_Dim_LifeMatter lm ON lm.Id = km.LifeMatterId
    WHERE lm.SourceGroup = 'PVR_KARAKATWA_GRID' AND km.HouseNumber IS NOT NULL
) k
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.tbl_Rule_LifeMatterFocus x
    WHERE x.RuleSetId = 1 AND x.LifeMatterId = k.LifeMatterId AND x.IsActive = 1
      AND x.FocusKind = 'House' AND x.ReferenceCode = 'LAGNA' AND x.HouseNumber = k.HouseNumber
);
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '139_seed_life_matter_subject_focus.sql',
       'Seeds LifeMatterSubject (64 rows) and LifeMatterFocus (136: 104 from PVR_LIFE_MATTER + 32 Karakatwa), closing the Subject/Focus mapping per lifematters_plan.md decisions 9-11.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '139_seed_life_matter_subject_focus.sql');
GO

DECLARE @subjects INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterSubject WHERE RuleSetId = 1 AND IsActive = 1);
DECLARE @foci INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus WHERE RuleSetId = 1 AND IsActive = 1);
DECLARE @fociHouse INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus WHERE RuleSetId = 1 AND IsActive = 1 AND FocusKind = 'House');
DECLARE @fociSpecial INT = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterFocus WHERE RuleSetId = 1 AND IsActive = 1 AND FocusKind = 'SpecialPoint');
PRINT '139 applied: subject rows=' + CAST(@subjects AS VARCHAR(10)) + ' (expected 64), focus rows=' + CAST(@foci AS VARCHAR(10))
    + ' (expected 136: House=' + CAST(@fociHouse AS VARCHAR(10)) + ' expected 135, SpecialPoint=' + CAST(@fociSpecial AS VARCHAR(10)) + ' expected 1).';
GO
