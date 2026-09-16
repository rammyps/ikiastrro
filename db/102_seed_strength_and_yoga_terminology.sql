-- =====================================================================
-- 102 — Terminology: Shadbala / Bhava Bala / Ashtakavarga / Amsabala / Yoga.
--
-- tbl_Astro_Terminology's Category CHECK has allowed 'StrengthComponent' and
-- 'Yoga' since it was created (migration 17), but no row of either category
-- was ever seeded — the entire 3. STRENGTH tab (Shadbala 3.1, Bhava Bala 3.2,
-- Ashtakavarga 3.3, Amsabala 3.4) and the Yoga engine had real computed data
-- with no glossary entry at all, and CalculationMethod/TechnicalDefinition
-- have never been populated anywhere in this table before this migration.
--
-- Scope: engine CONCEPTS, not every computed instance. Individual yogas
-- already carry their own ShortFormationRule/CalculationNarrative in
-- tbl_Rule_Yoga (migration 079) and are NOT duplicated here — only the
-- Yoga concept itself and its 4-value Type classification axis
-- (FormationFamilyCode) are registered. Likewise the 44 Amsabala amsa
-- names already live in tbl_Rule_AmsabalaName (migration 099) with their
-- own SourceRefCode and are not duplicated — only the Amsabala concept and
-- its 4 varga-group schemes are registered.
--
-- Every Code below matches a real column value already live in the DB
-- (BalaCode/SubComponentCode in tbl_Fact_PlanetaryStrengthComponent,
-- ComponentCode in tbl_Fact_BhavaStrengthComponent, StepCode in
-- tbl_Rule_AshtakavargaReduction, SchemeCode in tbl_Fact_Amsabala,
-- FormationFamilyCode in tbl_Rule_Yoga) so UI/CLI code can join Terminology
-- straight onto the fact/rule tables without inventing a parallel code
-- space. CalculationMethod text is transcribed from the actual engine
-- (ShadbalaCalculator.cs / BhavaBalaCalculator.cs / AshtakavargaCalculator.cs
-- / AmsabalaCalculator.cs), not freehand classical recall, so it matches
-- what is actually computed and persisted.
--
-- Source attribution: Shadbala + Bhava Bala -> SRC_RAMAN_GRAHA_BHAVA_BALAS
-- (the engine's own FormulaSourceRefCode); Ashtakavarga -> SRC_BPHS_ASHTAKAVARGA
-- (the engine's own SourceRefCode); Amsabala -> SRC_PVR_INTEGRATED sec 6.6
-- (matches tbl_Rule_AmsabalaGroup/_Name); Yoga concept/Type axis -> no single
-- citation (FormationFamilyCode is a project-defined classification, not a
-- book's own taxonomy) so SourceRefCode is left NULL there.
--
-- Three MERGE batches into tbl_Astro_Terminology, tier by tier (parent rows
-- committed before any child row that references them via the self-FK
-- ParentCode -> Code), then one MERGE for the sa/en text. DisplayOrder
-- 1000-1084 — the highest existing value before this migration is 996.
-- Idempotent throughout.
-- Apply:  sqlcmd -S localhost -E -d ikiastrro -b -i db/102_seed_strength_and_yoga_terminology.sql
-- =====================================================================
USE [ikiastrro];
GO

-- --- Tier 0: the five parent concepts ---
MERGE dbo.tbl_Astro_Terminology AS tgt
USING (VALUES
  ('StrengthComponent','SHADBALA',    CONVERT(VARCHAR(40),NULL),'SHADBALA',    CONVERT(INT,NULL),1000),
  ('StrengthComponent','BHAVABALA',   NULL,                     'BHAVABALA',   NULL,              1020),
  ('StrengthComponent','ASHTAKAVARGA',NULL,                     'ASHTAKAVARGA',NULL,              1040),
  ('StrengthComponent','AMSABALA',    NULL,                     'AMSABALA',    NULL,              1060),
  ('Yoga',              'YOGA',       NULL,                     'YOGA',        NULL,              1080)
) AS src (Category, Code, ParentCode, EngineCode, NumericKey, DisplayOrder)
ON tgt.Code = src.Code
WHEN MATCHED THEN UPDATE SET Category = src.Category, ParentCode = src.ParentCode,
    EngineCode = src.EngineCode, NumericKey = src.NumericKey, DisplayOrder = src.DisplayOrder, IsActive = 1
WHEN NOT MATCHED THEN INSERT (Category, Code, ParentCode, EngineCode, NumericKey, DisplayOrder, IsActive)
    VALUES (src.Category, src.Code, src.ParentCode, src.EngineCode, src.NumericKey, src.DisplayOrder, 1);
GO

-- --- Tier 1: the six/three/five/four/four children of each tier-0 concept ---
MERGE dbo.tbl_Astro_Terminology AS tgt
USING (VALUES
  -- Shadbala's six (Cheshta spelled CHESTA_BALA to match the live BalaCode) plus Yuddha
  ('StrengthComponent','STHANA_BALA',           'SHADBALA','SHADBALA',NULL,1001),
  ('StrengthComponent','DIG_BALA',              'SHADBALA','SHADBALA',NULL,1002),
  ('StrengthComponent','KALA_BALA',             'SHADBALA','SHADBALA',NULL,1003),
  ('StrengthComponent','CHESTA_BALA',           'SHADBALA','SHADBALA',NULL,1004),
  ('StrengthComponent','NAISARGIKA_BALA',       'SHADBALA','SHADBALA',NULL,1005),
  ('StrengthComponent','DRIK_BALA',             'SHADBALA','SHADBALA',NULL,1006),
  ('StrengthComponent','YUDDHA_BALA',           'SHADBALA','SHADBALA',NULL,1007),
  -- Bhava Bala's three
  ('StrengthComponent','BHAVADHIPATI_BALA',     'BHAVABALA','BHAVABALA',NULL,1021),
  ('StrengthComponent','BHAVA_DIG_BALA',        'BHAVABALA','BHAVABALA',NULL,1022),
  ('StrengthComponent','BHAVA_DRIK_BALA',       'BHAVABALA','BHAVABALA',NULL,1023),
  -- Ashtakavarga's five
  ('StrengthComponent','BHINNASHTAKAVARGA',     'ASHTAKAVARGA','ASHTAKAVARGA',NULL,1041),
  ('StrengthComponent','SARVASHTAKAVARGA',      'ASHTAKAVARGA','ASHTAKAVARGA',NULL,1042),
  ('StrengthComponent','TRIKONA_SODHANA',       'ASHTAKAVARGA','ASHTAKAVARGA',NULL,1043),
  ('StrengthComponent','EKADHIPATYA_SODHANA',   'ASHTAKAVARGA','ASHTAKAVARGA',NULL,1044),
  ('StrengthComponent','SODHYA_PINDA',          'ASHTAKAVARGA','ASHTAKAVARGA',NULL,1045),
  -- Amsabala's four varga-group schemes
  ('StrengthComponent','AMSABALA_SHADVARGA',    'AMSABALA','AMSABALA',NULL,1061),
  ('StrengthComponent','AMSABALA_SAPTAVARGA',   'AMSABALA','AMSABALA',NULL,1062),
  ('StrengthComponent','AMSABALA_DASAVARGA',    'AMSABALA','AMSABALA',NULL,1063),
  ('StrengthComponent','AMSABALA_SHODASAVARGA', 'AMSABALA','AMSABALA',NULL,1064),
  -- Yoga's four Type/FormationFamilyCode values
  ('Yoga','YOGA_TYPE_LAGNA',                    'YOGA','YOGA',NULL,1081),
  ('Yoga','YOGA_TYPE_MOON',                     'YOGA','YOGA',NULL,1082),
  ('Yoga','YOGA_TYPE_SUN',                      'YOGA','YOGA',NULL,1083),
  ('Yoga','YOGA_TYPE_COMBINATION',              'YOGA','YOGA',NULL,1084)
) AS src (Category, Code, ParentCode, EngineCode, NumericKey, DisplayOrder)
ON tgt.Code = src.Code
WHEN MATCHED THEN UPDATE SET Category = src.Category, ParentCode = src.ParentCode,
    EngineCode = src.EngineCode, NumericKey = src.NumericKey, DisplayOrder = src.DisplayOrder, IsActive = 1
WHEN NOT MATCHED THEN INSERT (Category, Code, ParentCode, EngineCode, NumericKey, DisplayOrder, IsActive)
    VALUES (src.Category, src.Code, src.ParentCode, src.EngineCode, src.NumericKey, src.DisplayOrder, 1);
GO

-- --- Tier 2: Sthana Bala's five sub-components + Kala Bala's five ---
MERGE dbo.tbl_Astro_Terminology AS tgt
USING (VALUES
  ('StrengthComponent','UCHCHA_BALA',               'STHANA_BALA','SHADBALA',NULL,1008),
  ('StrengthComponent','SAPTAVARGAJA_BALA',          'STHANA_BALA','SHADBALA',NULL,1009),
  ('StrengthComponent','OJHA_YUGMA_RASYAMSA_BALA',   'STHANA_BALA','SHADBALA',NULL,1010),
  ('StrengthComponent','KENDRADI_BALA',              'STHANA_BALA','SHADBALA',NULL,1011),
  ('StrengthComponent','DREKKANA_BALA',              'STHANA_BALA','SHADBALA',NULL,1012),
  ('StrengthComponent','NATHONNATA_BALA',            'KALA_BALA',  'SHADBALA',NULL,1013),
  ('StrengthComponent','PAKSHA_BALA',                'KALA_BALA',  'SHADBALA',NULL,1014),
  ('StrengthComponent','TRIBHAGA_BALA',              'KALA_BALA',  'SHADBALA',NULL,1015),
  ('StrengthComponent','DINA_BALA',                  'KALA_BALA',  'SHADBALA',NULL,1016),
  ('StrengthComponent','HORA_BALA',                  'KALA_BALA',  'SHADBALA',NULL,1017)
) AS src (Category, Code, ParentCode, EngineCode, NumericKey, DisplayOrder)
ON tgt.Code = src.Code
WHEN MATCHED THEN UPDATE SET Category = src.Category, ParentCode = src.ParentCode,
    EngineCode = src.EngineCode, NumericKey = src.NumericKey, DisplayOrder = src.DisplayOrder, IsActive = 1
WHEN NOT MATCHED THEN INSERT (Category, Code, ParentCode, EngineCode, NumericKey, DisplayOrder, IsActive)
    VALUES (src.Category, src.Code, src.ParentCode, src.EngineCode, src.NumericKey, src.DisplayOrder, 1);
GO

-- --- Text: sa (romanized name only) + en (full glossary row) for all 38 codes ---
MERGE dbo.tbl_Astro_TerminologyText AS tgt
USING (
  SELECT t.TerminologyId, v.LanguageCode, v.Script, v.Name, v.TraditionalName, v.ShortDescription,
         v.TechnicalDefinition, v.CalculationMethod, v.SourceRefCode
  FROM (VALUES
   ('SHADBALA','sa','Latn',N'Shadbala',N'Shadbala',NULL,NULL,NULL,NULL),
   ('SHADBALA','en','Latn',N'Shadbala',NULL,
     N'Six-fold measure of a planet''s total inherent strength, summed in virupas (BPHS ch. 27).',
     N'The aggregate strength score used to judge which of several candidate planets (contending yoga-karakas, dasha lords) is more capable of giving results; compared against each planet''s minimum requirement and split into Shubha/Ashubha (favourable/unfavourable) territory.',
     N'Sum of the six balas — Sthana + Dig + Kala + Cheshta + Naisargika + Drik — in virupas, plus a Yuddha Bala adjustment when the planet is one of the five tara grahas in a graha yuddha. Also expressed in rupas (virupas / 60) and split into Ishta/Kashta Phala (sqrt of Uchcha Bala x Cheshta Bala, and its complement from 60). ShadbalaCalculator.Calculate.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('STHANA_BALA','sa','Latn',N'Sthana Bala',N'Sthana Bala',NULL,NULL,NULL,NULL),
   ('STHANA_BALA','en','Latn',N'Sthana Bala',NULL,
     N'Positional strength — the largest of the six balas in most charts, combining exaltation degree, divisional dignity, sign parity, kendra placement and drekkana.',
     NULL,
     N'Sum of five sub-components: Uchcha Bala, Saptavargaja Bala, Ojha-Yugma Rasyamsa Bala, Kendradi Bala, Drekkana Bala. ShadbalaCalculator.AddSthana.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('DIG_BALA','sa','Latn',N'Dig Bala',N'Dig Bala',NULL,NULL,NULL,NULL),
   ('DIG_BALA','en','Latn',N'Dig Bala',NULL,
     N'Directional strength — full at the planet''s own strongest house-direction, falling to zero at the opposite house.',
     NULL,
     N'60 x (1 - min(house-distance, 12-house-distance)/6) from the planet''s own directional-strength house: Jupiter/Mercury from Lagna, Moon/Venus from the 4th, Saturn from the 7th, Sun/Mars from the 10th. ShadbalaCalculator.AddDig.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('KALA_BALA','sa','Latn',N'Kala Bala',N'Kala Bala',NULL,NULL,NULL,NULL),
   ('KALA_BALA','en','Latn',N'Kala Bala',NULL,
     N'Temporal strength — day/night, lunar phase, weekday, planetary hour and day/night-third all contribute.',
     NULL,
     N'Sum of five sub-components: Nathonnata Bala, Paksha Bala, Tribhaga Bala, Dina Bala, Hora Bala. ShadbalaCalculator.AddKala.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('CHESTA_BALA','sa','Latn',N'Cheshta Bala',N'Cheshta Bala',NULL,NULL,NULL,NULL),
   ('CHESTA_BALA','en','Latn',N'Cheshta Bala',NULL,
     N'Motional strength from retrograde/direct speed; Sun and Moon are excluded from this test.',
     NULL,
     N'0 for Sun and Moon; for the five star planets, 60 virupas if retrograde (or negative mean speed), else 30 — a coarse two-tier scheme pending a full mean-motion refinement. ShadbalaCalculator.AddCheshta.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('NAISARGIKA_BALA','sa','Latn',N'Naisargika Bala',N'Naisargika Bala',NULL,NULL,NULL,NULL),
   ('NAISARGIKA_BALA','en','Latn',N'Naisargika Bala',NULL,
     N'Fixed natural strength, the same for every chart — Sun strongest, Saturn weakest.',
     NULL,
     N'A constant per planet, independent of the chart: Sun 60.00, Moon 51.43, Venus 42.86, Jupiter 34.29, Mercury 25.71, Mars 17.14, Saturn 8.57 virupas. ShadbalaCalculator.AddNaisargika.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('DRIK_BALA','sa','Latn',N'Drik Bala',N'Drik Bala',NULL,NULL,NULL,NULL),
   ('DRIK_BALA','en','Latn',N'Drik Bala',NULL,
     N'Aspectual strength — net benefic-minus-malefic Sputa Drishti from every other D1 graha.',
     NULL,
     N'Sum over every other D1 graha of its Sputa Drishti (special-aspect-aware strength) onto the planet, signed positive for natural benefics and negative for natural malefics, divided by 4. ShadbalaCalculator.AddDrik.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('YUDDHA_BALA','sa','Latn',N'Yuddha Bala',N'Graha Yuddha',NULL,NULL,NULL,NULL),
   ('YUDDHA_BALA','en','Latn',N'Yuddha Bala',NULL,
     N'Planetary-war adjustment among the five tara grahas when two sit within 1 degree of each other.',
     N'Not one of the six sources; adjusts the six-fold total afterwards. Sun, Moon and the nodes never take part.',
     N'Only among Mars/Mercury/Jupiter/Venus/Saturn: when two are within 1 degree of D1 longitude, the one with the more northern ecliptic latitude wins. Magnitude is deliberately left at 0 virupas — the cited Raman edition''s diameter-based delta formula has no text extract available, so a number is not fabricated. ShadbalaCalculator.ComputeYuddha (tbl_Rule_PlanetaryWar, migration 072).',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('UCHCHA_BALA','sa','Latn',N'Uchcha Bala',N'Uchcha Bala',NULL,NULL,NULL,NULL),
   ('UCHCHA_BALA','en','Latn',N'Uchcha Bala',NULL,
     N'Exaltation-distance strength: how far the planet sits from its own deep-debilitation point.',
     NULL,
     N'Angular distance of the planet''s D1 longitude from its own deep-debilitation point, divided by 3 (0-60 virupas).',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('SAPTAVARGAJA_BALA','sa','Latn',N'Saptavargaja Bala',N'Saptavargaja Bala',NULL,NULL,NULL,NULL),
   ('SAPTAVARGAJA_BALA','en','Latn',N'Saptavargaja Bala',NULL,
     N'Divisional-dignity points summed across the seven-chart saptavarga.',
     N'Distinct from Amsabala (a placement COUNT across a varga group) — this is dignity POINTS per PvrDignityEvaluator, summed across D1/D2/D3/D7/D9/D12/D30.',
     N'Sum of PVR dignity points (PvrDignityEvaluator.Evaluate) for the planet''s placement across the D1, D2, D3, D7, D9, D12 and D30 charts.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('OJHA_YUGMA_RASYAMSA_BALA','sa','Latn',N'Ojha-Yugma Rasyamsa Bala',N'Ojha-Yugma Rasyamsa Bala',NULL,NULL,NULL,NULL),
   ('OJHA_YUGMA_RASYAMSA_BALA','en','Latn',N'Ojha-Yugma Rasyamsa Bala',NULL,
     N'Contribution from whether the planet''s classical gender matches its D1 sign''s odd/even parity.',
     NULL,
     N'30 virupas when a male planet (Sun/Mars/Jupiter/Saturn) or female/neuter planet (Moon/Mercury/Venus) sits in the sign-parity it favours; else 0. ShadbalaCalculator.AddSthana.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('KENDRADI_BALA','sa','Latn',N'Kendradi Bala',N'Kendradi Bala',NULL,NULL,NULL,NULL),
   ('KENDRADI_BALA','en','Latn',N'Kendradi Bala',NULL,
     N'Contribution from house-type: kendra, panaphara, or apoklima from Lagna.',
     NULL,
     N'60 virupas in a kendra (1/4/7/10), 30 in a panaphara (2/5/8/11), 15 in an apoklima (3/6/9/12) house from Lagna.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('DREKKANA_BALA','sa','Latn',N'Drekkana Bala',N'Drekkana Bala',NULL,NULL,NULL,NULL),
   ('DREKKANA_BALA','en','Latn',N'Drekkana Bala',NULL,
     N'Contribution from which third (drekkana) of the sign the planet occupies, by classical gender.',
     NULL,
     N'15 virupas when a male planet (Sun/Mars/Jupiter) sits in a sign''s 1st drekkana (0-10 degrees), a female planet (Moon/Venus) in the 2nd (10-20 degrees), or a neuter planet (Mercury/Saturn) in the 3rd (20-30 degrees); else 0.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('NATHONNATA_BALA','sa','Latn',N'Nathonnata Bala',N'Nathonnata Bala',NULL,NULL,NULL,NULL),
   ('NATHONNATA_BALA','en','Latn',N'Nathonnata Bala',NULL,
     N'Day/night-birth contribution, opposite for day-strong and night-strong planets.',
     NULL,
     N'60 virupas or 0: day-strong planets (Sun/Jupiter/Venus) score 60 for a day birth and 0 for night; the remaining planets score the opposite. ShadbalaCalculator.AddKala.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('PAKSHA_BALA','sa','Latn',N'Paksha Bala',N'Paksha Bala',NULL,NULL,NULL,NULL),
   ('PAKSHA_BALA','en','Latn',N'Paksha Bala',NULL,
     N'Lunar-phase (waxing/waning) contribution; currently scored for the Moon only.',
     NULL,
     N'60 - |180 - Sun-Moon elongation| / 3 for the Moon; 0 for every other planet in the current implementation (calendrical extension pending). ShadbalaCalculator.AddKala.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('TRIBHAGA_BALA','sa','Latn',N'Tribhaga Bala',N'Tribhaga Bala',NULL,NULL,NULL,NULL),
   ('TRIBHAGA_BALA','en','Latn',N'Tribhaga Bala',NULL,
     N'Contribution from which day-third or night-third of the birth day the native was born in.',
     NULL,
     N'60 virupas to the lord of the day-third (Mercury/Sun/Saturn) or night-third (Moon/Venus/Mars) in which birth fell, split by elapsed time since sunrise/sunset; Jupiter is classically exempt and always scores 60. ShadbalaCalculator.TribhagaValue.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('DINA_BALA','sa','Latn',N'Dina Bala',N'Vara Bala',NULL,NULL,NULL,NULL),
   ('DINA_BALA','en','Latn',N'Dina Bala',N'Vara Bala',
     N'(Vara Bala) Full strength to the lord of the birth''s Vedic weekday, else none.',
     NULL,
     N'45 virupas to the lord of the birth weekday (sunrise-to-sunrise Vedic weekday), else 0. ShadbalaCalculator.AddKala.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('HORA_BALA','sa','Latn',N'Hora Bala',N'Hora Bala',NULL,NULL,NULL,NULL),
   ('HORA_BALA','en','Latn',N'Hora Bala',NULL,
     N'Full strength to the lord of the planetary hour running at birth, else none.',
     NULL,
     N'60 virupas to the lord of the planetary hour running at the birth moment (from PanchangaCalculator''s Hora Lord), else 0. ShadbalaCalculator.AddKala.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('BHAVABALA','sa','Latn',N'Bhava Bala',N'Bhava Bala',NULL,NULL,NULL,NULL),
   ('BHAVABALA','en','Latn',N'Bhava Bala',NULL,
     N'Three-part measure of a house''s own strength, in virupas.',
     N'Computed for all 12 houses from the D1 chart and each planet''s already-computed Shadbala; also expressed in rupas (virupas / 60).',
     N'Sum of Bhavadhipati Bala + Bhava Dig Bala + Bhava Drik Bala for each of the 12 houses. BhavaBalaCalculator.Calculate.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('BHAVADHIPATI_BALA','sa','Latn',N'Bhavadhipati Bala',N'Bhavadhipati Bala',NULL,NULL,NULL,NULL),
   ('BHAVADHIPATI_BALA','en','Latn',N'Bhavadhipati Bala',NULL,
     N'The house''s own sign lord''s total Shadbala, carried into the house''s strength.',
     NULL,
     N'The house''s D1 sign lord''s total ShadbalaVirupas, read straight from the planetary-strength results. BhavaBalaCalculator.Calculate.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('BHAVA_DIG_BALA','sa','Latn',N'Bhava Dig Bala',N'Bhava Dig Bala',NULL,NULL,NULL,NULL),
   ('BHAVA_DIG_BALA','en','Latn',N'Bhava Dig Bala',NULL,
     N'Directional strength of the house itself, from its sign''s classical nature.',
     NULL,
     N'0-60 virupas from a fixed directional table keyed by the house sign''s classical nature (Nara/Jalachara/Chatushpada/Keeta), read at the house''s own position (1-12) in that table. BhavaBalaCalculator.Calculate.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('BHAVA_DRIK_BALA','sa','Latn',N'Bhava Drik Bala',N'Bhava Drik Bala',NULL,NULL,NULL,NULL),
   ('BHAVA_DRIK_BALA','en','Latn',N'Bhava Drik Bala',NULL,
     N'Net benefic-minus-malefic aspect strength onto the house''s own midpoint.',
     NULL,
     N'Benefic minus malefic Sputa Drishti from every D1 graha (Rahu/Ketu excluded) onto the house''s 15-degree whole-sign midpoint, divided by 4 and capped at 20 virupas. BhavaBalaCalculator.HouseDrik.',
     'SRC_RAMAN_GRAHA_BHAVA_BALAS'),

   ('ASHTAKAVARGA','sa','Latn',N'Ashtakavarga',N'Ashtakavarga',NULL,NULL,NULL,NULL),
   ('ASHTAKAVARGA','en','Latn',N'Ashtakavarga',NULL,
     N'Parasari benefic-point (bindu) system: 8 contributors casting points into specific signs for each of 8 recipients.',
     N'Pure and deterministic from D1 alone — reads only the natal sign of the 7 planets and the Lagna. The bindu matrix and every multiplier live in a fixed, verified table.',
     N'For each of the 7 planets + Lagna as recipient, each of the 8 contributors (7 planets + Lagna) casts a bindu into every sign that is a classically benefic count from the contributor''s own natal sign, per a fixed 8x8 offset table. AshtakavargaCalculator.Calculate.',
     'SRC_BPHS_ASHTAKAVARGA'),

   ('BHINNASHTAKAVARGA','sa','Latn',N'Bhinnashtakavarga',N'Bhinnashtakavarga',NULL,NULL,NULL,NULL),
   ('BHINNASHTAKAVARGA','en','Latn',N'Bhinnashtakavarga',NULL,
     N'One recipient planet''s own 12-sign bindu profile, before reduction.',
     NULL,
     N'For each of the 8 contributors, +1 bindu to every sign that is a classically benefic count from the contributor''s own natal sign; summed per sign, unreduced. AshtakavargaCalculator.Calculate / tbl_Fact_BhinnaAshtakavarga.',
     'SRC_BPHS_ASHTAKAVARGA'),

   ('SARVASHTAKAVARGA','sa','Latn',N'Sarvashtakavarga',N'Sarvashtakavarga',NULL,NULL,NULL,NULL),
   ('SARVASHTAKAVARGA','en','Latn',N'Sarvashtakavarga',NULL,
     N'The combined bindu total per sign, summed across all 7 Bhinnashtakavargas.',
     NULL,
     N'Sign-wise sum of the 7 recipients'' unreduced Bhinnashtakavarga bindu counts. AshtakavargaCalculator.Calculate / tbl_Fact_SarvaAshtakavarga.',
     'SRC_BPHS_ASHTAKAVARGA'),

   ('TRIKONA_SODHANA','sa','Latn',N'Trikona Shodhana',N'Trikona Shodhana',NULL,NULL,NULL,NULL),
   ('TRIKONA_SODHANA','en','Latn',N'Trikona Shodhana',NULL,
     N'Trine-based reduction step applied to a Bhinnashtakavarga before it is used for predictive strength.',
     NULL,
     N'Per 1-5-9 trine: if all three members are equal (and nonzero), all three go to 0; otherwise the lowest of the three is subtracted from all three; skipped entirely if any member is already 0. AshtakavargaCalculator.TrikonaSodhana.',
     'SRC_BPHS_ASHTAKAVARGA'),

   ('EKADHIPATYA_SODHANA','sa','Latn',N'Ekadhipatya Shodhana',N'Ekadhipatya Shodhana',NULL,NULL,NULL,NULL),
   ('EKADHIPATYA_SODHANA','en','Latn',N'Ekadhipatya Shodhana',NULL,
     N'Single-lordship-pair reduction step applied after Trikona Shodhana, using D1 sign occupancy.',
     NULL,
     N'Per sign-pair sharing one lord: skipped if either bindu count is 0 or both signs are occupied; if neither is occupied, unequal counts both fall to the lower value (equal counts both go to 0); if exactly one is occupied, the empty sign''s count falls to the occupied sign''s count when higher, else to 0. AshtakavargaCalculator.EkadhipatyaSodhana.',
     'SRC_BPHS_ASHTAKAVARGA'),

   ('SODHYA_PINDA','sa','Latn',N'Shodhya Pinda',N'Shodhya Pinda',NULL,NULL,NULL,NULL),
   ('SODHYA_PINDA','en','Latn',N'Shodhya Pinda',NULL,
     N'The fully-reduced bindu count converted to a weighted strength score (Rasi Pinda + Graha Pinda).',
     NULL,
     N'Rasi Pinda: each sign''s post-reduction bindu count x that sign''s Rasimana multiplier, summed. Graha Pinda: each graha''s Grahamana multiplier x the reduced bindu count in that graha''s own natal sign. AshtakavargaCalculator.Calculate / tbl_Fact_AshtakavargaPinda.',
     'SRC_BPHS_ASHTAKAVARGA'),

   ('AMSABALA','sa','Latn',N'Amsabala',N'Amsabala',NULL,NULL,NULL,NULL),
   ('AMSABALA','en','Latn',N'Amsabala',NULL,
     N'PVR''s varga-grouping dignity count: how many charts in a named group show a planet own/moolatrikona/exalted.',
     N'Distinct from Saptavargaja Bala (dignity POINTS, not a placement count) and from the still-unbuilt Vimsopaka Bala — this varga-grouping layer is the prerequisite Vimsopaka Bala builds on once PVR''s numeric per-varga weights are sourced.',
     N'Across each of 4 named varga groups (Shadvarga/Saptavarga/Dasavarga/Shodasavarga), count the divisional charts in which the planet sits in its own sign, moolatrikona, or exaltation sign, then name the resulting "amsa" from that count via tbl_Rule_AmsabalaName. A count below 2 has no named amsa (PVR''s own tables start at 2), not a data gap. AmsabalaCalculator.Calculate (PVR Integrated Approach sec 6.6).',
     'SRC_PVR_INTEGRATED'),

   ('AMSABALA_SHADVARGA','sa','Latn',N'Amsabala (Shadvarga)',N'Shadvarga',NULL,NULL,NULL,NULL),
   ('AMSABALA_SHADVARGA','en','Latn',N'Amsabala (Shadvarga)',NULL,
     N'The 6-chart Amsabala group.',
     NULL,
     N'D1, D2, D3, D9, D12, D30. A planet needs 2+ good (own/moolatrikona/exalted) placements across these 6 charts to receive a named amsa.',
     'SRC_PVR_INTEGRATED'),

   ('AMSABALA_SAPTAVARGA','sa','Latn',N'Amsabala (Saptavarga)',N'Saptavarga',NULL,NULL,NULL,NULL),
   ('AMSABALA_SAPTAVARGA','en','Latn',N'Amsabala (Saptavarga)',NULL,
     N'The 7-chart Amsabala group.',
     NULL,
     N'D1, D2, D3, D7, D9, D12, D30 — the Shadvarga group plus D7 (Saptamsa).',
     'SRC_PVR_INTEGRATED'),

   ('AMSABALA_DASAVARGA','sa','Latn',N'Amsabala (Dasavarga)',N'Dasavarga',NULL,NULL,NULL,NULL),
   ('AMSABALA_DASAVARGA','en','Latn',N'Amsabala (Dasavarga)',NULL,
     N'The 10-chart Amsabala group.',
     NULL,
     N'D1, D2, D3, D7, D9, D10, D12, D16, D30, D60 — the Saptavarga group plus D10, D16 and D60.',
     'SRC_PVR_INTEGRATED'),

   ('AMSABALA_SHODASAVARGA','sa','Latn',N'Amsabala (Shodasavarga)',N'Shodasavarga',NULL,NULL,NULL,NULL),
   ('AMSABALA_SHODASAVARGA','en','Latn',N'Amsabala (Shodasavarga)',NULL,
     N'The 16-chart Amsabala group — every varga PVR uses for dignity.',
     NULL,
     N'D1, D2, D3, D4, D7, D9, D10, D12, D16, D20, D24, D27, D30, D40, D45, D60.',
     'SRC_PVR_INTEGRATED'),

   ('YOGA','sa','Latn',N'Yoga',N'Yoga',NULL,NULL,NULL,NULL),
   ('YOGA','en','Latn',N'Yoga',NULL,
     N'A specific planetary combination classical texts associate with a named life-effect.',
     N'223 distinct yoga codes are evaluated against a chart; this entry documents the concept and its Type classification axis, not each individual yoga — those carry their own ShortFormationRule/CalculationNarrative in tbl_Rule_Yoga (migration 079), sourced per-yoga via tbl_Rule_Yoga.SourceRefCode.',
     N'Each yoga is independently evaluated against its own coded predicate (src/Ikiastrro.Core/Engines/Yoga) and persisted per chart in tbl_Fact_YogaInputEvaluations, exposed via vw_ChartYogaEvaluations.',
     NULL),

   ('YOGA_TYPE_LAGNA','sa','Latn',N'Lagna-based Yoga',N'Lagna',NULL,NULL,NULL,NULL),
   ('YOGA_TYPE_LAGNA','en','Latn',N'Lagna-based Yoga',NULL,
     N'Yoga classification: judged relative to the Lagna.',
     NULL,
     N'The yoga''s formation is judged relative to the Lagna (ascendant) — e.g. a planet exalted or own in a kendra from Lagna. Stored as tbl_Rule_Yoga.FormationFamilyCode = ''LAGNA''.',
     NULL),

   ('YOGA_TYPE_MOON','sa','Latn',N'Moon-based Yoga',N'Chandra',NULL,NULL,NULL,NULL),
   ('YOGA_TYPE_MOON','en','Latn',N'Moon-based Yoga',NULL,
     N'Yoga classification: judged relative to the Moon.',
     NULL,
     N'The yoga''s formation is judged relative to the Moon — e.g. planets in the 2nd/12th from Moon (Sunapha/Anapha). Stored as FormationFamilyCode = ''MOON''.',
     NULL),

   ('YOGA_TYPE_SUN','sa','Latn',N'Sun-based Yoga',N'Surya',NULL,NULL,NULL,NULL),
   ('YOGA_TYPE_SUN','en','Latn',N'Sun-based Yoga',NULL,
     N'Yoga classification: judged relative to the Sun.',
     NULL,
     N'The yoga''s formation is judged relative to the Sun — e.g. planets in the 2nd/12th from Sun (Vesi/Vasi). Stored as FormationFamilyCode = ''SUN''.',
     NULL),

   ('YOGA_TYPE_COMBINATION','sa','Latn',N'Combination Yoga',N'Combination',NULL,NULL,NULL,NULL),
   ('YOGA_TYPE_COMBINATION','en','Latn',N'Combination Yoga',NULL,
     N'Yoga classification: needs a multi-point combination beyond a single Lagna/Moon/Sun reference.',
     NULL,
     N'The yoga''s formation requires a specific multi-point combination not reducible to a single Lagna/Moon/Sun reference — most Raja and Dhana yogas, and every Nabhasa yoga. Stored as FormationFamilyCode = ''COMBINATION''.',
     NULL)

  ) AS v (Code, LanguageCode, Script, Name, TraditionalName, ShortDescription, TechnicalDefinition, CalculationMethod, SourceRefCode)
  JOIN dbo.tbl_Astro_Terminology t ON t.Code = v.Code
) AS src
ON tgt.TerminologyId = src.TerminologyId AND tgt.LanguageCode = src.LanguageCode AND tgt.Script = src.Script
WHEN MATCHED THEN UPDATE SET Name = src.Name, TraditionalName = src.TraditionalName, ShortDescription = src.ShortDescription,
    TechnicalDefinition = src.TechnicalDefinition, CalculationMethod = src.CalculationMethod, SourceRefCode = src.SourceRefCode
WHEN NOT MATCHED THEN INSERT (TerminologyId, LanguageCode, Script, Name, TraditionalName, ShortDescription, TechnicalDefinition, CalculationMethod, SourceRefCode)
    VALUES (src.TerminologyId, src.LanguageCode, src.Script, src.Name, src.TraditionalName, src.ShortDescription, src.TechnicalDefinition, src.CalculationMethod, src.SourceRefCode);
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '102_seed_strength_and_yoga_terminology.sql',
       'Terminology: 38 StrengthComponent/Yoga concepts (Shadbala 18, Bhava Bala 4, Ashtakavarga 6, Amsabala 5, Yoga 5) + 76 sa/en text rows, CalculationMethod populated throughout.'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '102_seed_strength_and_yoga_terminology.sql');
GO

DECLARE @concepts INT = (SELECT COUNT(*) FROM dbo.tbl_Astro_Terminology WHERE Category IN ('StrengthComponent','Yoga'));
DECLARE @notext   INT = (SELECT COUNT(*) FROM dbo.tbl_Astro_Terminology t
    WHERE t.Category IN ('StrengthComponent','Yoga')
      AND (NOT EXISTS (SELECT 1 FROM dbo.tbl_Astro_TerminologyText x WHERE x.TerminologyId = t.TerminologyId AND x.LanguageCode = 'sa')
        OR NOT EXISTS (SELECT 1 FROM dbo.tbl_Astro_TerminologyText x WHERE x.TerminologyId = t.TerminologyId AND x.LanguageCode = 'en')));
DECLARE @nocalc   INT = (SELECT COUNT(*) FROM dbo.tbl_Astro_Terminology t
    JOIN dbo.tbl_Astro_TerminologyText x ON x.TerminologyId = t.TerminologyId AND x.LanguageCode = 'en'
    WHERE t.Category IN ('StrengthComponent','Yoga') AND x.CalculationMethod IS NULL);
DECLARE @orphan   INT = (SELECT COUNT(*) FROM dbo.tbl_Astro_Terminology t
    WHERE t.Category IN ('StrengthComponent','Yoga') AND t.ParentCode IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.tbl_Astro_Terminology p WHERE p.Code = t.ParentCode));
PRINT '102 applied: ' + CAST(@concepts AS VARCHAR(10)) + ' StrengthComponent/Yoga concepts (expect 38), '
    + CAST(@notext AS VARCHAR(10)) + ' missing sa or en text (expect 0), '
    + CAST(@nocalc AS VARCHAR(10)) + ' en rows missing CalculationMethod (expect 0), '
    + CAST(@orphan AS VARCHAR(10)) + ' orphan ParentCode (expect 0).';
GO
