-- =====================================================================
-- 151 - Special Lagna terminology addendum: Indu Lagna + Pranapada Lagna.
--
-- Same shape as db/29's SPT_BL/HL/GL/SL addendum, continuing its
-- DisplayOrder sequence (901-909 -> 910-913). Two new SpecialPoint rows
-- (SPT_IL, SPT_PP) and two new Concept rows for the calculation families
-- introduced in db/150 (CALC_KALA_SIGN_COUNT, CALC_PRANA_MODALITY_OFFSET),
-- for parity with db/29's CALC_TIME_FROM_SUNRISE / CALC_NAKSHATRA_FRACTION.
--
-- Idempotent: terminology via MERGE (matches db/29).
-- Apply:  sqlcmd -S localhost -E -d ikiastrro -b -i db/151_add_indu_pranapada_terminology.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: terminology addendum - concepts ---
MERGE dbo.tbl_Astro_Terminology AS tgt
USING (VALUES
  ('SpecialPoint','SPT_IL',NULL,'KARAKA',NULL,910),
  ('SpecialPoint','SPT_PP',NULL,'KARAKA',NULL,911),
  ('Concept','CALC_KALA_SIGN_COUNT',NULL,'SPECIALLAGNA',NULL,912),
  ('Concept','CALC_PRANA_MODALITY_OFFSET',NULL,'SPECIALLAGNA',NULL,913)
) AS src (Category, Code, ParentCode, EngineCode, NumericKey, DisplayOrder)
ON tgt.Code = src.Code
WHEN MATCHED THEN UPDATE SET Category = src.Category, ParentCode = src.ParentCode,
    EngineCode = src.EngineCode, NumericKey = src.NumericKey, DisplayOrder = src.DisplayOrder, IsActive = 1
WHEN NOT MATCHED THEN INSERT (Category, Code, ParentCode, EngineCode, NumericKey, DisplayOrder, IsActive)
    VALUES (src.Category, src.Code, src.ParentCode, src.EngineCode, src.NumericKey, src.DisplayOrder, 1);
GO

-- --- Batch 2: terminology addendum - sa + en text ---
MERGE dbo.tbl_Astro_TerminologyText AS tgt
USING (
  SELECT t.TerminologyId, v.LanguageCode, v.Script, v.Name, v.TraditionalName, v.ShortDescription
  FROM (VALUES
   ('SPT_IL','sa','Latn',N'Indu Lagna',N'Indu Lagna',NULL),
   ('SPT_IL','en','Latn',N'Indu Lagna',NULL,N'Special lagna counted from the Moon by the summed Kala value of the 9th-lord-from-Lagna and 9th-lord-from-Moon, reduced mod 12. Shows wealth-yielding capacity; an own/exalted planet in it or 2nd/11th from it forms Koteeswara Yoga.'),
   ('SPT_PP','sa','Latn',N'Pranapada Lagna',N'Pranapada Lagna',NULL),
   ('SPT_PP','en','Latn',N'Pranapada Lagna',NULL,N'Special lagna advancing from the Sun''s sunrise longitude at the classical vighati rate, with a further offset for the modality of the Sun''s sunrise sign. Signifies vitality and life-force; read for health and longevity.'),
   ('CALC_KALA_SIGN_COUNT','sa','Latn',N'Kala sign count',N'Kala sign count',NULL),
   ('CALC_KALA_SIGN_COUNT','en','Latn',N'Kala sign count',NULL,N'Special-lagna calculation family: sum two planets'' Kala/rayi values, reduce mod 12, and count that many signs forward from a reference sign (Indu Lagna).'),
   ('CALC_PRANA_MODALITY_OFFSET','sa','Latn',N'Prana modality offset',N'Prana modality offset',NULL),
   ('CALC_PRANA_MODALITY_OFFSET','en','Latn',N'Prana modality offset',NULL,N'Special-lagna calculation family: a time-rate base value plus a fixed degree offset (0/120/240) depending on the modality of the Sun''s sunrise sign (Pranapada Lagna).')
  ) AS v (Code, LanguageCode, Script, Name, TraditionalName, ShortDescription)
  JOIN dbo.tbl_Astro_Terminology t ON t.Code = v.Code
) AS src
ON tgt.TerminologyId = src.TerminologyId AND tgt.LanguageCode = src.LanguageCode AND tgt.Script = src.Script
WHEN MATCHED THEN UPDATE SET Name = src.Name, TraditionalName = src.TraditionalName, ShortDescription = src.ShortDescription
WHEN NOT MATCHED THEN INSERT (TerminologyId, LanguageCode, Script, Name, TraditionalName, ShortDescription)
    VALUES (src.TerminologyId, src.LanguageCode, src.Script, src.Name, src.TraditionalName, src.ShortDescription);
GO

-- --- Batch 3: ledger + summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '151_add_indu_pranapada_terminology.sql',
       'Terminology addendum: SPT_IL/SPT_PP + CALC_KALA_SIGN_COUNT/CALC_PRANA_MODALITY_OFFSET concepts, sa/en text.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '151_add_indu_pranapada_terminology.sql');
GO

DECLARE @concepts INT = (SELECT COUNT(*) FROM dbo.tbl_Astro_Terminology
    WHERE Code IN ('SPT_IL','SPT_PP','CALC_KALA_SIGN_COUNT','CALC_PRANA_MODALITY_OFFSET'));
DECLARE @notext INT = (SELECT COUNT(*) FROM dbo.tbl_Astro_Terminology t
    WHERE t.Code IN ('SPT_IL','SPT_PP','CALC_KALA_SIGN_COUNT','CALC_PRANA_MODALITY_OFFSET')
      AND (NOT EXISTS (SELECT 1 FROM dbo.tbl_Astro_TerminologyText x WHERE x.TerminologyId = t.TerminologyId AND x.LanguageCode = 'sa')
        OR NOT EXISTS (SELECT 1 FROM dbo.tbl_Astro_TerminologyText x WHERE x.TerminologyId = t.TerminologyId AND x.LanguageCode = 'en')));
PRINT '151 applied: ' + CAST(@concepts AS VARCHAR(10)) + ' terminology concepts (expect 4), '
    + CAST(@notext AS VARCHAR(10)) + ' concepts missing sa or en text (expect 0).';
GO
