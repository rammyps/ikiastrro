-- =====================================================================
-- 122 — Pilot tbl_Content_Interpretation rows for YOGA_BUDHA_ADITYA.
--
-- Proves the read+write plumbing (db/080, db/121, InterpretationRepository,
-- InterpretationText.razor, the editable interpretation column) end to end
-- with a real subject before any wider content-authoring pass. Text below
-- is deliberately conservative and keyed to exactly what
-- SourceAttributedYogaEngine.AddBudhaAditya already encodes — nothing
-- beyond it. Flagged PENDING REVIEW: this is a starting point for
-- rammyps to edit (through the UI, once F ships, or by hand), not final
-- copy. Once edited through the UI this migration's rows are superseded
-- by whatever InterpretationRepository.Upsert wrote — this seed is not
-- re-applied over user edits (guarded by WHERE NOT EXISTS, like every
-- other seed in this project).
-- =====================================================================
USE [ikiastrro];
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '122_seed_budha_aditya_interpretation_pilot.sql')
BEGIN
    INSERT dbo.tbl_Content_Interpretation (RuleSetId, SubjectType, SubjectCode, SourceRefCode, StandardText, ShortText)
    SELECT 1, 'YOGA', 'YOGA_BUDHA_ADITYA', 'SRC_RAMAN_300_COMBINATIONS',
           N'PENDING REVIEW - Raman requires Sun and Mercury more than 10 degrees apart (same sign) for this yoga; within 10 degrees, Raman does not treat it as formed.',
           N'PENDING REVIEW - Raman: needs >10 deg separation.'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Content_Interpretation WHERE RuleSetId = 1 AND SubjectType = 'YOGA' AND SubjectCode = 'YOGA_BUDHA_ADITYA' AND SourceRefCode = 'SRC_RAMAN_300_COMBINATIONS');

    INSERT dbo.tbl_Content_Interpretation (RuleSetId, SubjectType, SubjectCode, SourceRefCode, StandardText, ShortText)
    SELECT 1, 'YOGA', 'YOGA_BUDHA_ADITYA', 'SRC_PVR_INTEGRATED',
           N'PENDING REVIEW - PVR treats this yoga as formed whenever Sun and Mercury share a sign, at any separation. When they are 14 degrees or closer, Mercury is combust and PVR notes the yoga''s benefic effect is reduced.',
           N'PENDING REVIEW - PVR: same sign; combust under 14 deg weakens it.'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Content_Interpretation WHERE RuleSetId = 1 AND SubjectType = 'YOGA' AND SubjectCode = 'YOGA_BUDHA_ADITYA' AND SourceRefCode = 'SRC_PVR_INTEGRATED');

    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('122_seed_budha_aditya_interpretation_pilot.sql',
        'Pilot tbl_Content_Interpretation rows for YOGA_BUDHA_ADITYA (Raman, PVR) proving the plumbing; text is PENDING REVIEW, not final.');
END
GO

PRINT '122 applied (pilot content pending review).';
GO
