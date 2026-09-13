/* House-lord-in-house pilot: the Lagna (1st) lord in each of the 12 houses.
   Paraphrased (not transcribed) from SRC_RAMAN_HTJH Vol. I, "Results of Lord of First
   House Occupying Different Houses", printed pp. 21-23 (scan pp. 30-32) -- locator
   RAM-H3 in docs/research/sources.md. Whole-sign houses, Lagna reference point, per the
   project's 2026-09-07 house-system decision (docs/research/domain/house-placement.md).
   BASELINE = the source's unqualified result; WELL_DISPOSED = its own explicitly separate
   "if the lord is well disposed / fortified / strong" branch. The source states these two
   branches itself for 11 of the 12 houses; the 10th has only one branch in the passage.
   This is a reference pointer + paraphrase, not a copy of the copyrighted text. */

-- 1) Combination dimension: Lagna lord (owned house 1) x each occupied house.
INSERT research.tbl_Dim_SourceReferenceHouseLordInHouse (OwnedHouseId, OccupiedHouseId)
SELECT owned.Id, occ.Id
FROM research.tbl_Dim_SourceReferenceHouse owned
CROSS JOIN research.tbl_Dim_SourceReferenceHouse occ
WHERE owned.HouseNumber = 1
  AND NOT EXISTS (
      SELECT 1 FROM research.tbl_Dim_SourceReferenceHouseLordInHouse x
      WHERE x.OwnedHouseId = owned.Id AND x.OccupiedHouseId = occ.Id
        AND x.HouseSystemCode = 'WHOLE_SIGN' AND x.ReferencePointCode = 'LAGNA'
  );

-- 2) Source pointers: Raman (the detailed result) + PVR (whole-sign/method referral, no
--    matching 12-row catalogue of its own -- see house-placement.md "Where coverage does
--    not overlap").
INSERT research.tbl_Dim_SourceReferenceHouseLordInHouseText
    (HouseLordInHouseId, SourceRefCode, WorkTitle, Author, Edition, Chapter, VerseOrPage,
     LanguageCode, TextTypeCode, CopyrightStatus, SourceLocator, Notes)
SELECT x.Id, s.SourceRefCode, s.WorkTitle, s.Author, s.Edition, s.Chapter, s.VerseOrPage,
       'en', 'Reference', s.CopyrightStatus, s.SourceLocator, s.Notes
FROM research.tbl_Dim_SourceReferenceHouseLordInHouse x
JOIN research.tbl_Dim_SourceReferenceHouse owned ON owned.Id = x.OwnedHouseId AND owned.HouseNumber = 1
CROSS JOIN (VALUES
    ('SRC_RAMAN_HTJH', N'How to Judge a Horoscope (vols I-II)', N'B. V. Raman', NULL,
     N'Concerning the First House', N'Printed pp. 21-23; scan pp. 30-32 (Vol. I)',
     'SummaryOnly', N'RAM-H3; docs/research/domain/house-placement.md starter notes',
     N'Reference pointer only; paraphrased in the Claim table, not transcribed. OCR extract: D:\@ClaudeSpace\BookExtracts\how-to-judge-a-horoscope-1.md.'),
    ('SRC_PVR_INTEGRATED', N'Vedic Astrology: An Integrated Approach', N'P. V. R. Narasimha Rao', NULL,
     N'Ch. 13 sec. 13.4.1', N'Printed pp. 169-170',
     'SummaryOnly', N'PVR-H4',
     N'Establishes the whole-sign/Lagna-reference method this project follows and refers readers to Raman for the detailed 12-house catalogue; does not itself state per-house results.')
) s (SourceRefCode, WorkTitle, Author, Edition, Chapter, VerseOrPage, CopyrightStatus, SourceLocator, Notes)
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferenceHouseLordInHouseText t
    WHERE t.HouseLordInHouseId = x.Id AND t.SourceRefCode = s.SourceRefCode
);

-- 3) Claims: paraphrased BASELINE + WELL_DISPOSED branches per occupied house.
;WITH ClaimDefs (OccupiedHouseNumber, BranchCode, ClaimText, RequiredConditionsJson) AS
(
    SELECT * FROM (VALUES
    (1, 'BASELINE',
     N'Independent, self-reliant character; the native rises through personal effort. The source also flags a plurality-of-partners theme (formal or informal) as part of this baseline reading.',
     N'{"branch":"baseline","conditioned":false,"note":"Unqualified reading; source states strength/aspects/other factors modify the result for every house (see WELL_DISPOSED branch)."}'),
    (1, 'WELL_DISPOSED',
     N'When well disposed in Lagna, the Lagna lord brings recognition and fame within the native''s own community and country.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord well disposed in Lagna","evaluableToday":"partial","evaluableNote":"tbl_Chart_HouseLords.LordDignityStatus (Exalted/Moolatrikona/Own Sign/Great Friend/Friend) approximates dignity; aspect qualification is not yet a computed fact and must read NOT_EVALUATED, not be inferred."}'),
    (2, 'BASELINE',
     N'More financial gain than usual, though the native is tested or troubled by rivals/enemies; good, generous, socially respectable character.',
     N'{"branch":"baseline","conditioned":false}'),
    (2, 'WELL_DISPOSED',
     N'Well disposed here, the native gladly discharges duties toward family, is ambitious, has notably fine eyes, and shows foresight.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord well disposed in the 2nd","evaluableToday":"partial"}'),
    (3, 'BASELINE',
     N'Marked courage, good fortune and social standing, with a recurring plurality-of-partners theme; intelligent and generally happy.',
     N'{"branch":"baseline","conditioned":false}'),
    (3, 'WELL_DISPOSED',
     N'Well disposed, advancement in life comes through siblings; possible fame as a musician or mathematician, depending on the sign and planets involved.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord well disposed in the 3rd","evaluableToday":"partial"}'),
    (4, 'BASELINE',
     N'Happiness from parents, many siblings, materially inclined, well-built and well-mannered.',
     N'{"branch":"baseline","conditioned":false}'),
    (4, 'WELL_DISPOSED',
     N'Favourably disposed, the native gains considerable property (often via the maternal line), wealth, fame and vehicles/comforts.',
     N'{"branch":"well_disposed","conditionText":"4th lord (as Lagna lord) favourably disposed","evaluableToday":"partial"}'),
    (5, 'BASELINE',
     N'The first child may not survive and happiness from children is otherwise limited; short-tempered; a subordinate, service-oriented disposition.',
     N'{"branch":"baseline","conditioned":false}'),
    (5, 'WELL_DISPOSED',
     N'When fortified, the native gains favour with rulers or powerful political figures and may be drawn into trade or diplomatic service, with devotion matching the 5th lord''s own significations.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord fortified in the 5th","evaluableToday":"partial"}'),
    (6, 'BASELINE',
     N'Carries the 3rd-house-lord-in-3rd baseline themes, plus debts that clear specifically during the Lagna lord''s own dasa.',
     N'{"branch":"baseline","conditioned":false,"note":"Compound with the 3rd-lord-in-3rd claim; not yet cross-referenced automatically."}'),
    (6, 'WELL_DISPOSED',
     N'Fortified, this can support a military career up to high command, or leadership in medical/health services -- timing depends on dasa and must be weighed against other influences.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord fortified in the 6th, dasa-timed","evaluableToday":"no","evaluableNote":"Requires dasa-period alignment, not evaluated by a placement fact alone."}'),
    (7, 'BASELINE',
     N'Marital instability (spouse''s early death, or more than one marriage); a later-life turn toward detachment and asceticism; fortune is mixed (rich or poor) depending on other factors; much travel.',
     N'{"branch":"baseline","conditioned":false}'),
    (7, 'WELL_DISPOSED',
     N'Well disposed, this favours extended time abroad -- though the source separately notes a self-indulgent or in-law-dominated variant of the same placement.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord well disposed in the 7th","evaluableToday":"partial"}'),
    (8, 'BASELINE',
     N'Learned, but with gambling tendencies and an interest in the occult; a morally mixed character.',
     N'{"branch":"baseline","conditioned":false}'),
    (8, 'WELL_DISPOSED',
     N'A strong placement instead supports generosity, a wide circle of friends, religious inclination, and an easeful, sudden end.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord strong in the 8th","evaluableToday":"partial"}'),
    (9, 'BASELINE',
     N'Generally fortunate and protective of others, religious (Vishnu devotion where relevant), a good speaker, happy through spouse and children, and wealthy.',
     N'{"branch":"baseline","conditioned":false}'),
    (9, 'WELL_DISPOSED',
     N'Well disposed, the native inherits ancestral/maternal property, and the father becomes famous, philanthropic and devout.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord well disposed in the 9th","evaluableToday":"partial"}'),
    (10, 'BASELINE',
     N'Carries the 4th-house-lord-in-4th baseline themes, plus professional success and honour among eminent people -- often specialising in a field indicated jointly by the Lagna lord and the 10th house.',
     N'{"branch":"baseline","conditioned":false,"note":"Source gives only one branch for this house; no separate well-disposed qualification stated in the cited passage."}'),
    (11, 'BASELINE',
     N'Carries the 2nd-house-lord-in-2nd baseline themes, plus reliable business gains and freedom from financial hardship.',
     N'{"branch":"baseline","conditioned":false}'),
    (11, 'WELL_DISPOSED',
     N'Prosperity here is credited to an elder sibling, with substantial business profit -- subject to the other planets joining the combination.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord well disposed in the 11th","evaluableToday":"partial"}'),
    (12, 'BASELINE',
     N'Carries the 8th-house-lord-in-8th baseline themes, plus repeated losses, pilgrimage, and a lack of success in business ventures.',
     N'{"branch":"baseline","conditioned":false}'),
    (12, 'WELL_DISPOSED',
     N'Favourable disposition redirects inherited wealth toward charity, with an emotionally balanced, public-spirited character.',
     N'{"branch":"well_disposed","conditionText":"Lagna lord favourably disposed in the 12th","evaluableToday":"partial"}')
    ) v (OccupiedHouseNumber, BranchCode, ClaimText, RequiredConditionsJson)
)
INSERT research.tbl_Dim_SourceReferenceHouseLordInHouseClaim
    (HouseLordInHouseId, ClaimCode, BranchCode, ClaimText, RequiredConditionsJson,
     EvidenceLevelCode, StatusCode, SourceTextId, SourceRefCode)
SELECT x.Id,
       CONCAT('HLP_H01_H', RIGHT('0' + CAST(c.OccupiedHouseNumber AS VARCHAR(2)), 2), '_', c.BranchCode),
       c.BranchCode, c.ClaimText, c.RequiredConditionsJson,
       'DirectClassical', 'Proposed', t.Id, 'SRC_RAMAN_HTJH'
FROM ClaimDefs c
JOIN research.tbl_Dim_SourceReferenceHouse occ ON occ.HouseNumber = c.OccupiedHouseNumber
JOIN research.tbl_Dim_SourceReferenceHouse owned ON owned.HouseNumber = 1
JOIN research.tbl_Dim_SourceReferenceHouseLordInHouse x
    ON x.OwnedHouseId = owned.Id AND x.OccupiedHouseId = occ.Id
JOIN research.tbl_Dim_SourceReferenceHouseLordInHouseText t
    ON t.HouseLordInHouseId = x.Id AND t.SourceRefCode = 'SRC_RAMAN_HTJH'
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferenceHouseLordInHouseClaim y
    WHERE y.HouseLordInHouseId = x.Id
      AND y.ClaimCode = CONCAT('HLP_H01_H', RIGHT('0' + CAST(c.OccupiedHouseNumber AS VARCHAR(2)), 2), '_', c.BranchCode)
      AND y.SourceRefCode = 'SRC_RAMAN_HTJH'
);

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'090_seed_house_lord_in_house_pilot_lagna.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'090_seed_house_lord_in_house_pilot_lagna.sql', N'Seed the Lagna-lord pilot (1x12 combinations): Raman/PVR source pointers plus paraphrased BASELINE/WELL_DISPOSED claims, per docs/research/domain/house-placement.md''s planned first-house pilot.');
