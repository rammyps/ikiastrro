-- =====================================================================
-- 098 -- Seed tbl_Nakshatras.Gana / YoniAnimal / YoniGender / Nadi.
--
-- These four columns have existed since the nakshatra reference table was first
-- created (db/_archive/021_create_nakshatra_reference_tables.sql) but were left NULL
-- on purpose -- docs/research/domain/reference-data-tables.md documented this as a
-- "legitimate single-answer classical table, not to be reproduced from memory without
-- one cited source" (same caution class as the Rahu/Ketu natural-relationship gap and
-- tbl_SignAttributes.RisingType). Per rammyps's 2026-09-14 call, the source is now
-- named: `SRC_VASUDEV_MATCHING_CHARTS` (Gayatri Devi Vasudev, "The Art of Matching
-- Charts", Chapter VI "Kuta Agreement") -- see docs/research/sources.md.
--
-- Book page citations:
--   Gana        p.71  ("Gana" table, Daiva/Manushya/Rakshasa, 9 rows each)
--   Yoni        p.69  ("Yoni Kuta" table, Male/Female nakshatra columns + animal)
--   Nadi        p.75  ("Nadi" table, Vata/Pitta/Sleshma columns, 9 rows each --
--               Sleshma is a synonym for Kapha, the value this schema's CHECK
--               constraint expects)
--
-- Varna is deliberately NOT touched here. The same book (p.66) confirms Varna Kuta is
-- a Rasi property (Moon-sign), not a nakshatra property -- already correctly seeded on
-- tbl_SignAttributes.Varna_Class. tbl_Nakshatras.Varna stays NULL: for the 9 nakshatras
-- that straddle a sign boundary (tbl_Nakshatras.StraddlesSignBoundary = 1, see migration
-- 096's header) a single nakshatra-level Varna value would be ambiguous in exactly the
-- way RasiId already is, and populating it would just duplicate tbl_SignAttributes with
-- an extra edge case. Tatva is also left NULL -- it belongs to a different, separate
-- classification and this source does not cover it at all (not part of either the
-- 8-factor or 10-factor Kuta scheme the book works through); still an open item.
--
-- Cross-check note: a worked example later in the same chapter (p.82) states
-- "Jyeshta -- Sleshma", which conflicts with the book's own master Nadi table on p.75
-- (Jyeshta is listed under Vata there). Treated as a typo in the worked example, not a
-- second convention -- the master table (p.75) is what this migration seeds from.
-- =====================================================================
USE [ikiastrro];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Source WHERE Code = 'SRC_VASUDEV_MATCHING_CHARTS')
    INSERT dbo.tbl_Dim_Source (Code, Title, Author, Edition, Tradition, Notes)
    VALUES ('SRC_VASUDEV_MATCHING_CHARTS', N'The Art of Matching Charts', N'Gayatri Devi Vasudev', NULL, 'classical',
        N'Ch. VI "Kuta Agreement": Varna (p.66, confirms Rasi-level, cross-checks tbl_SignAttributes.Varna_Class), Yoni Kuta table (p.69), Gana table (p.71), Nadi table (p.75). Registered here for the first time. Local: `D:\Vedic Astrology\Vedic Astology Books\The Art of Matching Charts, autured by Gayatri Devi Vasudev.pdf` (scanned, no text layer -- read via rendered page images).');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Nakshatras WHERE Gana IS NOT NULL)
BEGIN
    ;WITH src (NakshatraName, Gana, YoniAnimal, YoniGender, Nadi) AS
    (
        SELECT * FROM (VALUES
            (N'Ashwini',            N'Deva',     N'Horse',    N'Male',   N'Vata'),
            (N'Bharani',            N'Manushya', N'Elephant', N'Male',   N'Pitta'),
            (N'Krittika',           N'Rakshasa', N'Sheep',    N'Female', N'Kapha'),
            (N'Rohini',             N'Manushya', N'Snake',    N'Male',   N'Kapha'),
            (N'Mrigashira',         N'Deva',     N'Snake',    N'Female', N'Pitta'),
            (N'Ardra',              N'Manushya', N'Dog',      N'Female', N'Vata'),
            (N'Punarvasu',          N'Deva',     N'Cat',      N'Female', N'Vata'),
            (N'Pushya',             N'Deva',     N'Sheep',    N'Male',   N'Pitta'),
            (N'Ashlesha',           N'Rakshasa', N'Cat',      N'Male',   N'Kapha'),
            (N'Magha',              N'Rakshasa', N'Rat',      N'Male',   N'Kapha'),
            (N'Purva Phalguni',     N'Manushya', N'Rat',      N'Female', N'Pitta'),
            (N'Uttara Phalguni',    N'Manushya', N'Cow',      N'Male',   N'Vata'),
            (N'Hasta',              N'Deva',     N'Buffalo',  N'Female', N'Vata'),
            (N'Chitra',             N'Rakshasa', N'Tiger',    N'Female', N'Pitta'),
            (N'Swati',              N'Deva',     N'Buffalo',  N'Male',   N'Kapha'),
            (N'Vishakha',           N'Rakshasa', N'Tiger',    N'Male',   N'Kapha'),
            (N'Anuradha',           N'Deva',     N'Deer',     N'Female', N'Pitta'),
            (N'Jyeshtha',           N'Rakshasa', N'Deer',     N'Male',   N'Vata'),
            (N'Mula',               N'Rakshasa', N'Dog',      N'Male',   N'Vata'),
            (N'Purva Ashadha',      N'Manushya', N'Monkey',   N'Male',   N'Pitta'),
            (N'Uttara Ashadha',     N'Manushya', N'Mongoose', N'Male',   N'Kapha'),
            (N'Shravana',           N'Deva',     N'Monkey',   N'Female', N'Kapha'),
            (N'Dhanishta',          N'Rakshasa', N'Lion',     N'Female', N'Pitta'),
            (N'Shatabhisha',        N'Rakshasa', N'Horse',    N'Female', N'Vata'),
            (N'Purva Bhadrapada',   N'Manushya', N'Lion',     N'Male',   N'Vata'),
            (N'Uttara Bhadrapada',  N'Manushya', N'Cow',      N'Female', N'Pitta'),
            (N'Revati',             N'Deva',     N'Elephant', N'Female', N'Kapha')
        ) AS v (NakshatraName, Gana, YoniAnimal, YoniGender, Nadi)
    )
    UPDATE nak
    SET nak.Gana        = src.Gana,
        nak.YoniAnimal  = src.YoniAnimal,
        nak.YoniGender  = src.YoniGender,
        nak.Nadi        = src.Nadi
    FROM dbo.tbl_Nakshatras nak
    JOIN src ON src.NakshatraName = nak.NakshatraName;

    IF (SELECT COUNT(*) FROM dbo.tbl_Nakshatras WHERE Gana IS NULL OR YoniAnimal IS NULL OR YoniGender IS NULL OR Nadi IS NULL) <> 0
        RAISERROR('098: not all 27 tbl_Nakshatras rows matched the seed set by NakshatraName -- check for a spelling mismatch.', 16, 1);
END
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'098_seed_nakshatra_gana_yoni_nadi.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'098_seed_nakshatra_gana_yoni_nadi.sql', N'Seed tbl_Nakshatras.Gana/YoniAnimal/YoniGender/Nadi from SRC_VASUDEV_MATCHING_CHARTS (Ch. VI, pp.69-75). Varna and Tatva left NULL -- see header.');
GO

PRINT '098 applied: tbl_Nakshatras Gana/Yoni/Nadi populated (27 rows) from SRC_VASUDEV_MATCHING_CHARTS.';
GO
