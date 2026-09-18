-- =====================================================================
-- 118 -- Add tbl_Nakshatras.TamilName; seed the 15 nakshatras with a
-- distinct Tamil panchangam name.
--
-- Only 15 of the 27 nakshatras get a value here -- the other 12
-- (Ashwini, Bharani, Rohini, Pushya, Ashlesha, Magha, Hasta, Chitra,
-- Swati, Vishakha, Mula, Revati) use the same name in Tamil as the
-- Sanskrit name already stored in NakshatraName, so TamilName stays
-- NULL for those rather than duplicating it.
--
-- No single cited source for this list (rammyps, 2026-09-18) -- unlike
-- Gana/YoniAnimal/YoniGender/Nadi (migration 098), which are pinned to
-- SRC_VASUDEV_MATCHING_CHARTS. Treat TamilName as informal/display data,
-- not a sourced classical attribute.
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('dbo.tbl_Nakshatras', 'TamilName') IS NULL
    ALTER TABLE dbo.tbl_Nakshatras ADD TamilName NVARCHAR(40) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Nakshatras WHERE TamilName IS NOT NULL)
BEGIN
    ;WITH src (NakshatraName, TamilName) AS
    (
        SELECT * FROM (VALUES
            (N'Krittika',           N'Karthikai'),
            (N'Mrigashira',         N'Mrigashirisha'),
            (N'Ardra',              N'Thiruvadhirai'),
            (N'Punarvasu',          N'Punarpusam'),
            (N'Purva Phalguni',     N'Pubba'),
            (N'Uttara Phalguni',    N'Uthram'),
            (N'Anuradha',           N'Anusham'),
            (N'Jyeshtha',           N'Kettai'),
            (N'Purva Ashadha',      N'Pooradam'),
            (N'Uttara Ashadha',     N'Uthradam'),
            (N'Shravana',           N'Thiruvonam'),
            (N'Dhanishta',          N'Avittam'),
            (N'Shatabhisha',        N'Sathayam'),
            (N'Purva Bhadrapada',   N'Poorattadhi'),
            (N'Uttara Bhadrapada',  N'Uthrattadhi')
        ) AS v (NakshatraName, TamilName)
    )
    UPDATE nak
    SET nak.TamilName = src.TamilName
    FROM dbo.tbl_Nakshatras nak
    JOIN src ON src.NakshatraName = nak.NakshatraName;

    IF (SELECT COUNT(*) FROM dbo.tbl_Nakshatras WHERE TamilName IS NOT NULL) <> 15
        RAISERROR('118: expected exactly 15 tbl_Nakshatras rows with TamilName set -- check for a NakshatraName spelling mismatch.', 16, 1);
END
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'118_add_nakshatra_tamil_names.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'118_add_nakshatra_tamil_names.sql', N'Add tbl_Nakshatras.TamilName; seed 15 of 27 nakshatras with a distinct Tamil name (other 12 share the Sanskrit name). No cited source -- informal data, unlike the sourced migration 098 seed.');
GO

PRINT '118 applied: tbl_Nakshatras.TamilName added and populated (15 of 27 rows).';
GO
