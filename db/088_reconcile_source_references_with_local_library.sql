-- =====================================================================
-- 088 - Reconcile tbl_Dim_Source (the SRC_* citation registry) against the
-- physical library at D:\Vedic Astrology\Vedic Astology Books.
--
-- Three kinds of finding, all fixed here:
--
-- 1. CORRUPTED DATA (real bug, not a books gap). SRC_BPHS_ASHTAKAVARGA's
--    Title was inserted with a mis-decoded em dash ("Ã¢â‚¬â€" mojibake in
--    place of "-") - migration 075's own .sql file has the correct UTF-8
--    byte sequence, so the corruption happened at apply time (sqlcmd run
--    without -f 65001, the same class of bug documented in migrations 086/
--    087). Re-asserted here with the correct text.
--
-- 2. ROWS NOT BACKED BY ANY MIGRATION ON THIS BRANCH (present in the shared
--    dev DB, so a fresh rebuild from this branch's db/ folder alone would
--    silently lose them). Found while cross-checking: SRC_BPHS_GRAHA_SVARUPA,
--    SRC_TB_1_5_1_NAKSHATRAS, SRC_HOROSCOPE_EXPLORER. Traced to git branch
--    `master`, which independently reused migration numbers 071/072/076 for
--    unrelated content (071_seed_bphs_planet_sanskrit_text.sql,
--    072_seed_taittiriya_brahmana_nakshatra_text.sql,
--    076_add_horoscope_explorer_gap_yogas.sql) applied against the same
--    shared local SQL Server instance this branch also builds against - a
--    numbering collision between branches, not a workstream/database
--    migration gap. Captured here via idempotent MERGE so workstream/
--    database's own migration history reproduces them without depending on
--    master. SRC_BPHS_GRAHA_SVARUPA's Author/Title were also mojibake-
--    corrupted in the dev DB (same apply-time bug as #1) - corrected to a
--    proper em dash and "Parāśara" while capturing it.
--
-- 3. MISSING LOCAL-LIBRARY EVIDENCE. Several SRC_* rows cite a classical
--    text this project's local library (D:\Vedic Astrology\Vedic Astology
--    Books) actually holds, but the row never recorded the local path -
--    unlike SRC_RAMAN_GRAHA_BHAVA_BALAS / SRC_JHORA_EXPORT_RAMAKRISHNAN,
--    which already do. Verified each match by opening the file (pdftotext
--    on the first pages) before citing it below, not by filename alone.
--    Appended as "Local: ..." (idempotent - only appends when that marker
--    isn't already present):
--      SRC_PVR_INTEGRATED         - 1_PVR_NarasimhaRao.pdf
--      SRC_BPHS                   - Santhanam's 2-vol. English translation
--      SRC_PHALADEEPIKA           - Mantreswara_s__Phaladeeplka_.pdf (tr. Dr. G. S. Kapoor)
--      SRC_RATH_VARGA             - VargaChakra by S Rath.pdf (confirmed: SJC
--                                    Vyankatesa Sharma Varga Workshop transcript,
--                                    Hyderabad Dec 2002 - a direct match for
--                                    "varga methods", not a guess)
--      SRC_RAMAN_HTJH             - local Vol1/Vol2 PDFs, alongside the
--                                    already-registered OCR extract
--      SRC_RAMAN_300_COMBINATIONS - local DJVU, exact path (Notes previously
--                                    said "complete local DJVU" without one)
--
-- 4. NEW REGISTRATIONS. Two codes are cited in docs/research/domain/
--    rasi-nakshatra.md but were never added to tbl_Dim_Source - closing
--    that gap for both (confirmed, by scanning every SourceRefCode column
--    project-wide, that neither is used as a live SourceRefCode value
--    anywhere - both were doc-only dangling references, so this did not
--    break verify-sources):
--      SRC_BRIHAT_JATAKA_1 - Varahamihira's sign classifications. Text
--        verified readable locally (Brihat Jatak JYOTISH VEDIC ASTROLOGY.pdf,
--        chapter 1 matches the cited sign-classification content);
--        translator not credited in the extracted text.
--      SRC_BPHS_34_45 - BPHS chapters 34-45, nakshatra deity/lord/symbol/
--        range descriptions. No local edition isolates chapters 34-45
--        specifically; the full-text local editions registered under
--        SRC_BPHS (Santhanam) cover the same chapters.
--
-- REQUIRES -f 65001 (see migrations 086/087 - the very bug being fixed
-- here is what happens when that flag is omitted).
-- Apply: sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/088_reconcile_source_references_with_local_library.sql
-- =====================================================================
USE [ikiastrro];
GO

-- --- 1. Fix the mojibake'd em dash in SRC_BPHS_ASHTAKAVARGA.Title ---
UPDATE dbo.tbl_Dim_Source
   SET Title = N'Brihat Parashara Hora Shastra — Ashtakavarga Adhyaya'
 WHERE Code = 'SRC_BPHS_ASHTAKAVARGA'
   AND Title <> N'Brihat Parashara Hora Shastra — Ashtakavarga Adhyaya';
GO

-- --- 2. Capture the three orphaned dev-only rows (idempotent MERGE), fixing
--        SRC_BPHS_GRAHA_SVARUPA's corrupted Title/Author while doing so ---
;WITH orphan (Code, Title, Author, Edition, Tradition, Notes) AS (
    SELECT * FROM (VALUES
        ('SRC_BPHS_GRAHA_SVARUPA', N'BPHS chapter 3 — Graha traits and forms', N'Parāśara (attributed)',
         N'Sanskrit Documents par0110.pdf, 2025 typeset', 'Parasari',
         N'Original Devanagari text; cite printed page and verse. Local English-translation cross-check: BPHS ch. 3 is in Santhanam''s translation (see SRC_BPHS).'),
        ('SRC_TB_1_5_1_NAKSHATRAS', N'Taittiriya Brahmana 1.5.1 — Nakshatra powers', N'Taittiriya recension (traditional)',
         N'Sanskrit Documents taittirIyabrAhmaNamniHsvaraH.pdf, 2026 typeset', 'Vedic',
         N'Accentless Devanagari text; cite printed pages 34-35 and section unit.'),
        ('SRC_HOROSCOPE_EXPLORER', N'Horoscope Explorer comparison output', NULL,
         N'Ramakrishnan P report screenshots, 2026-09-10', 'Comparison',
         N'Parity evidence only. Effects do not establish a classical formation rule.')
    ) v (Code, Title, Author, Edition, Tradition, Notes)
)
MERGE dbo.tbl_Dim_Source AS tgt
USING orphan AS src ON tgt.Code = src.Code
WHEN MATCHED THEN UPDATE SET tgt.Title = src.Title, tgt.Author = src.Author,
    tgt.Edition = src.Edition, tgt.Tradition = src.Tradition, tgt.Notes = src.Notes
WHEN NOT MATCHED THEN INSERT (Code, Title, Author, Edition, Tradition, Notes)
    VALUES (src.Code, src.Title, src.Author, src.Edition, src.Tradition, src.Notes);
GO

-- --- 3. Register SRC_BRIHAT_JATAKA_1 and SRC_BPHS_34_45 (both documented in
--        rasi-nakshatra.md, neither previously registered here) ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Source WHERE Code = 'SRC_BRIHAT_JATAKA_1')
    INSERT dbo.tbl_Dim_Source (Code, Title, Author, Edition, Tradition, Notes)
    VALUES ('SRC_BRIHAT_JATAKA_1', N'Brihat Jataka', N'Varahamihira', NULL, 'classical',
        N'Ch. 1 sign classifications/modalities, polarity, day/night groupings. Cited in docs/research/domain/rasi-nakshatra.md; registered here for the first time (not previously in this table). Local: `D:\Vedic Astrology\Vedic Astology Books\Brihat Jatak JYOTISH VEDIC ASTROLOGY.pdf` (translator not credited in the extracted text).');
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Source WHERE Code = 'SRC_BPHS_34_45')
    INSERT dbo.tbl_Dim_Source (Code, Title, Author, Edition, Tradition, Notes)
    VALUES ('SRC_BPHS_34_45', N'BPHS chapters 34–45 — nakshatra descriptions', N'Parāśara (attrib.)', NULL, 'Parasari',
        N'Deity, lord, symbol, range, and descriptive nakshatra material. Cited in docs/research/domain/rasi-nakshatra.md; registered here for the first time (not previously in this table). Local: no edition isolates ch. 34-45 specifically; the full-text local editions under SRC_BPHS (Santhanam translation) cover these chapters.');
GO

-- --- 4. Append confirmed local-library paths to rows that lacked them
--        (guarded on the "Local:" marker so this is safe to re-run) ---
UPDATE dbo.tbl_Dim_Source SET Notes = Notes + N' Local: `D:\Vedic Astrology\Vedic Astology Books\1_PVR_NarasimhaRao.pdf`; raw text extract `D:\@ClaudeSpace\BookExtracts\pvr-integrated-approach-raw.txt`.'
 WHERE Code = 'SRC_PVR_INTEGRATED' AND Notes NOT LIKE N'%Local:%';

UPDATE dbo.tbl_Dim_Source SET Notes = Notes + N' Local: R. Santhanam''s English translation (Ranjan Publications, New Delhi), 2 vols. - `D:\Vedic Astrology\Vedic Astology Books\Brihad Parasara Hora Shastra - Santhanam''s\BPHS-Santhanam-Vol-1.pdf` / `...-Vol-2.pdf`.'
 WHERE Code = 'SRC_BPHS' AND Notes NOT LIKE N'%Local:%';

UPDATE dbo.tbl_Dim_Source SET Notes = Notes + N' Local: Eng. tr. Dr. G. S. Kapoor - `D:\Vedic Astrology\Vedic Astology Books\Mantreswara_s__Phaladeeplka_.pdf`.'
 WHERE Code = 'SRC_PHALADEEPIKA' AND Notes NOT LIKE N'%Local:%';

UPDATE dbo.tbl_Dim_Source SET Notes = Notes + N' Local: Sri Jagannath Center Vyankatesa Sharma Varga Workshop transcript (Hyderabad, Dec 2002), ed. Sanjay Rath - `D:\Vedic Astrology\Vedic Astology Books\VargaChakra by S Rath.pdf`.'
 WHERE Code = 'SRC_RATH_VARGA' AND Notes NOT LIKE N'%Local:%';

UPDATE dbo.tbl_Dim_Source SET Notes = Notes + N' Local PDFs (alongside the OCR extract above): `D:\Vedic Astrology\Vedic Astology Books\How to judge a horoscope\BVRaman-_How_to_Judge_Horoscope_Vol1.pdf` / `BVRaman_-_How_to_Judge_Horoscope_Vol2.pdf`.'
 WHERE Code = 'SRC_RAMAN_HTJH' AND Notes NOT LIKE N'%Local PDFs%';

UPDATE dbo.tbl_Dim_Source SET Notes = Notes + N' Local: `D:\Vedic Astrology\Vedic Astology Books\B. V. Raman\300 Important Combinations.djvu`.'
 WHERE Code = 'SRC_RAMAN_300_COMBINATIONS' AND Notes NOT LIKE N'%Local:%';
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '088_reconcile_source_references_with_local_library.sql',
       'tbl_Dim_Source: fixed 2 mojibake rows, captured 3 cross-branch rows, registered SRC_BRIHAT_JATAKA_1 + SRC_BPHS_34_45, added confirmed local-library paths to 6 rows'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '088_reconcile_source_references_with_local_library.sql');
GO

DECLARE @n INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_Source);
DECLARE @withLocal INT = (SELECT COUNT(*) FROM dbo.tbl_Dim_Source WHERE Notes LIKE N'%Local%');
PRINT '088 applied: tbl_Dim_Source has ' + CAST(@n AS VARCHAR(10)) + ' rows, ' + CAST(@withLocal AS VARCHAR(10)) + ' with a recorded local-library path.';
GO
