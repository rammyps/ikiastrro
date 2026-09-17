/* Seed the research.tbl_Dim_SourceReferencePlanetTransitFromMoon* corpus (migration 112) for
   all nine grahas across the 12 houses counted from natal Moon (Janma Rasi) -- SRC_KP_TRANSIT_
   GOCHARA's sign-by-sign transit results. Sun through Saturn (84 rows) are the source's own
   stated per-house results, paraphrased (not transcribed -- copyright convention, see e.g.
   migration 090's header). Rahu and Ketu (24 rows) are not independently itemized by the
   source; it states an equivalence instead ("Rahu offers the results similar to that of Mars
   in transit. Kethu gives such of those indicated by Saturn.") -- their ClaimText reuses Mars'
   / Saturn's paraphrase respectively, flagged DerivedEquivalence with the derivation recorded
   in RequiredConditionsJson so a consumer can tell it apart from a directly-stated result.

   Two numbering fixes made against the raw OCR extract, each a mid-sequence mis-scanned digit
   that repeated an earlier item number out of place (corrected to the only value that keeps
   each planet's list a clean 1-12 run; the source text itself is otherwise a plain sign-ordered
   list with no separate baseline/qualified branches, unlike tbl_Rule_HouseLordPlacement):
     - Mercury: the item after "7. Gets exhausted..." was OCR'd as "6." -- corrected to 8.
     - Jupiter: the item after "7. Success in enterprise..." was OCR'd as "6." -- corrected to 8.
   Venus item 12 and Saturn item 10 were OCR'd as ", 2." and ", O." respectively -- both restored
   to their sequential number ("12.", "10.") from context alone; no wording was otherwise unclear. */

-- 1) Combination dimension: all nine grahas x all 12 houses-from-Moon.
INSERT research.tbl_Dim_SourceReferencePlanetTransitFromMoon (PlanetId, HouseFromMoon)
SELECT p.Id, h.HouseNumber
FROM research.tbl_Dim_SourceReferencePlanet p
CROSS JOIN research.tbl_Dim_SourceReferenceHouse h
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetTransitFromMoon x
    WHERE x.PlanetId = p.Id AND x.HouseFromMoon = h.HouseNumber
);
GO

-- 2) Source pointer: one row per combo, same citation (matches the per-combo Text-row grain
--    used by research.tbl_Dim_SourceReferenceHouseLordInHouseText).
INSERT research.tbl_Dim_SourceReferencePlanetTransitFromMoonText
    (PlanetTransitFromMoonId, SourceRefCode, WorkTitle, Author, Chapter, VerseOrPage,
     LanguageCode, TextTypeCode, CopyrightStatus, SourceLocator, Notes)
SELECT x.Id, s.SourceRefCode, s.WorkTitle, s.Author, s.Chapter, s.VerseOrPage,
       'en', 'Reference', s.CopyrightStatus, s.SourceLocator, s.Notes
FROM research.tbl_Dim_SourceReferencePlanetTransitFromMoon x
CROSS JOIN (VALUES (
    'SRC_KP_TRANSIT_GOCHARA', N'Krishnamurti Padhdhati', N'K. S. Krishnamurti',
    N'Transit (Gocharaphala Nirnayam)', N'pp. xxii-xxx (approx.; roman-numeral front matter, OCR; exact page per planet section unconfirmed)',
    'SummaryOnly', N'Pasted OCR extract, 2026-09-18; edition/volume and a local file path not yet confirmed',
    N'Reference pointer only; paraphrased in the Claim table, not transcribed.'
)) s (SourceRefCode, WorkTitle, Author, Chapter, VerseOrPage, CopyrightStatus, SourceLocator, Notes)
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetTransitFromMoonText t
    WHERE t.PlanetTransitFromMoonId = x.Id AND t.SourceRefCode = s.SourceRefCode
);
GO

-- 3) Claims: paraphrased per-house results, 9 planets x 12 houses = 108 rows.
;WITH ClaimDefs AS (
    SELECT * FROM (VALUES
    -- Sun (~1 month/sign; source caveat: a house's result is not assumed to hold for the
    -- whole month untouched, and must be weighed against other planetary positions)
    ('PLANET_SUN', 1,  N'Health dips and success stalls; possible relocation or change of place.'),
    ('PLANET_SUN', 2,  N'Income drops while expenses rise; risk of deceit, eye trouble, or headaches.'),
    ('PLANET_SUN', 3,  N'Health improves and income rises.'),
    ('PLANET_SUN', 4,  N'Delays and obstacles; health suffers; domestic unhappiness.'),
    ('PLANET_SUN', 5,  N'Opposition from enemies; health suffers; domestic unhappiness.'),
    ('PLANET_SUN', 6,  N'Income rises, health improves, and efforts succeed.'),
    ('PLANET_SUN', 7,  N'Illness, travel, and financial worry.'),
    ('PLANET_SUN', 8,  N'Mental distress, digestive trouble, disputes, difficulties, or risk of injury/accident.'),
    ('PLANET_SUN', 9,  N'Hopes are abandoned; danger and poor health.'),
    ('PLANET_SUN', 10, N'Major success; meetings with influential people who become allies.'),
    ('PLANET_SUN', 11, N'Income grows quickly; business expands, gains and recognition follow, health holds up.'),
    ('PLANET_SUN', 12, N'Hopes are abandoned; deep discouragement and aimless wandering.'),

    -- Moon (~2.25 days/sign)
    ('PLANET_MOON', 1,  N'Restful sleep, good food, and gains of clothing or jewellery.'),
    ('PLANET_MOON', 2,  N'Humiliation; expenses exceed income; obstacles arise.'),
    ('PLANET_MOON', 3,  N'Good health and profitable, successful undertakings.'),
    ('PLANET_MOON', 4,  N'Poor health, restless travel, anxiety and worry.'),
    ('PLANET_MOON', 5,  N'Health suffers, efforts are abandoned, mental strain and low spirits.'),
    ('PLANET_MOON', 6,  N'A pleasant, happy home life with financial gains and good health.'),
    ('PLANET_MOON', 7,  N'An enjoyable period with profits.'),
    ('PLANET_MOON', 8,  N'Poor health, disease, disputes, depression, financial loss and sleeplessness.'),
    ('PLANET_MOON', 9,  N'Fear of enemies.'),
    ('PLANET_MOON', 10, N'Financial gains and successful undertakings.'),
    ('PLANET_MOON', 11, N'Time with friends and relatives; festive gatherings at home.'),
    ('PLANET_MOON', 12, N'Expenses exceed income.'),

    -- Mars (~1.5 months/sign)
    ('PLANET_MARS', 1,  N'Travel, bodily heat or inflammation, fear of enemies, and abandoned hopes.'),
    ('PLANET_MARS', 2,  N'Conflict with superiors and failed endeavours.'),
    ('PLANET_MARS', 3,  N'Friendship with the learned and the eminent brings success.'),
    ('PLANET_MARS', 4,  N'Unpleasant company and physical injury.'),
    ('PLANET_MARS', 5,  N'Poor health for self and children; fear and anxiety; enemies threaten.'),
    ('PLANET_MARS', 6,  N'Major success and gains of gold.'),
    ('PLANET_MARS', 7,  N'Family disputes, wasted money, and either digestive trouble or eye trouble.'),
    ('PLANET_MARS', 8,  N'Travel, irritation, and bodily injury.'),
    ('PLANET_MARS', 9,  N'Delay and difficulty in undertakings; illness and friction with superiors.'),
    ('PLANET_MARS', 10, N'Profit, successful undertakings, and peace of mind.'),
    ('PLANET_MARS', 11, N'A fortunate period.'),
    ('PLANET_MARS', 12, N'Eye trouble, disputes, excess expenses, and damaged reputation.'),

    -- Mercury (~1 month/sign, sometimes 20 days to a bit over a month)
    ('PLANET_MERCURY', 1,  N'Education is disrupted; friction with relatives; caution needed in speech.'),
    ('PLANET_MERCURY', 2,  N'Money comes in; enjoyable food; gains through speaking or writing.'),
    ('PLANET_MERCURY', 3,  N'Conflict with enemies, friction with superiors, and humiliation from opponents.'),
    ('PLANET_MERCURY', 4,  N'Money is earned and life is enjoyed.'),
    ('PLANET_MERCURY', 5,  N'No peace of mind; domestic friction; anxiety.'),
    ('PLANET_MERCURY', 6,  N'Undertakings succeed; money is gained, including through speech.'),
    ('PLANET_MERCURY', 7,  N'Exhaustion; possible separation from close relatives or friends; friction.'),
    ('PLANET_MERCURY', 8,  N'A very happy period marked by success.'),
    ('PLANET_MERCURY', 9,  N'Friction with relatives; health suffers.'),
    ('PLANET_MERCURY', 10, N'Good health is maintained and income increases.'),
    ('PLANET_MERCURY', 11, N'Happiness at home and earnings away from it.'),
    ('PLANET_MERCURY', 12, N'Irritation, friction, disputes, illness, and damaged reputation.'),

    -- Jupiter (~1 year/sign)
    ('PLANET_JUPITER', 1,  N'Relocation, rising expenses, restlessness, and a decline in standing.'),
    ('PLANET_JUPITER', 2,  N'Financial gains and a happy home life.'),
    ('PLANET_JUPITER', 3,  N'Danger; a largely hopeless, difficult period.'),
    ('PLANET_JUPITER', 4,  N'Expenses rise and relatives may pass away.'),
    ('PLANET_JUPITER', 5,  N'Income rises, status and rank improve, and spirits are high.'),
    ('PLANET_JUPITER', 6,  N'Financial loss, a change of employment, and declining health.'),
    ('PLANET_JUPITER', 7,  N'Success in undertakings, but health suffers.'),
    ('PLANET_JUPITER', 8,  N'Poor health for spouse and children, domestic danger, and risk of confinement or aimless wandering.'),
    ('PLANET_JUPITER', 9,  N'New friendships form; charitable acts and success follow.'),
    ('PLANET_JUPITER', 10, N'Delay, friction, financial loss, irritation, and humiliation.'),
    ('PLANET_JUPITER', 11, N'Promotion, rising income, and improved standing.'),
    ('PLANET_JUPITER', 12, N'The spouse travels; expenses exceed income.'),

    -- Venus (~1 month/sign)
    ('PLANET_VENUS', 1,  N'Enjoyable food, restful sleep, and a pleasant life.'),
    ('PLANET_VENUS', 2,  N'Happiness at home; a better harvest or yield; recognition.'),
    ('PLANET_VENUS', 3,  N'Friends offer help, bringing success and profit.'),
    ('PLANET_VENUS', 4,  N'Improvements to the home; helpful relatives and a pleasant journey.'),
    ('PLANET_VENUS', 5,  N'A son thrives and elders bring happiness.'),
    ('PLANET_VENUS', 6,  N'The spouse falls ill and irritation arises.'),
    ('PLANET_VENUS', 7,  N'Domestic unhappiness and friction between partners.'),
    ('PLANET_VENUS', 8,  N'Growth in property, union with a partner, sexual pleasure, and financial gain.'),
    ('PLANET_VENUS', 9,  N'Romance, giving, and charitable undertakings.'),
    ('PLANET_VENUS', 10, N'Obstacles and setbacks in undertakings; irritation and humiliation.'),
    ('PLANET_VENUS', 11, N'Happiness at home and rising income.'),
    ('PLANET_VENUS', 12, N'Spending on jewellery and general improvement.'),

    -- Saturn (~2.5 years/sign)
    ('PLANET_SATURN', 1,  N'Poor health for spouse, children, and close relatives; unsatisfying food; fear of enemies; friction and disputes.'),
    ('PLANET_SATURN', 2,  N'Poor health; loss of pets; restless movement; disputes and financial loss.'),
    ('PLANET_SATURN', 3,  N'Success; pleasant events and functions; recognition and a strong reputation.'),
    ('PLANET_SATURN', 4,  N'Illness; friction with relatives; the health of parents suffers.'),
    ('PLANET_SATURN', 5,  N'Financial loss; false accusations and rumours; disputes.'),
    ('PLANET_SATURN', 6,  N'Enemies are defeated, bringing gain; success in undertakings.'),
    ('PLANET_SATURN', 7,  N'Travel, separation, and expenses.'),
    ('PLANET_SATURN', 8,  N'A close relative passes away; public disapproval of one''s actions; confinement to one place.'),
    ('PLANET_SATURN', 9,  N'Income declines.'),
    ('PLANET_SATURN', 10, N'Illness, irritation, and restless wandering.'),
    ('PLANET_SATURN', 11, N'Financial gains, romantic gain, sympathy, and reward.'),
    ('PLANET_SATURN', 12, N'Danger, accident, mental worry, and heavy expenses.')

    ) v (PlanetCode, HouseFromMoon, ClaimText)
)
INSERT research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim
    (PlanetTransitFromMoonId, ClaimCode, ClaimText, RequiredConditionsJson, EvidenceLevelCode, StatusCode, SourceTextId, SourceRefCode)
SELECT x.Id,
       CONCAT('TRANSIT_MOON_', REPLACE(c.PlanetCode, 'PLANET_', ''), '_H', RIGHT('0' + CAST(c.HouseFromMoon AS VARCHAR(2)), 2)),
       c.ClaimText, NULL, 'DirectClassical', 'Proposed', t.Id, 'SRC_KP_TRANSIT_GOCHARA'
FROM ClaimDefs c
JOIN research.tbl_Dim_SourceReferencePlanet p ON p.PlanetCode = c.PlanetCode
JOIN research.tbl_Dim_SourceReferencePlanetTransitFromMoon x ON x.PlanetId = p.Id AND x.HouseFromMoon = c.HouseFromMoon
JOIN research.tbl_Dim_SourceReferencePlanetTransitFromMoonText t ON t.PlanetTransitFromMoonId = x.Id AND t.SourceRefCode = 'SRC_KP_TRANSIT_GOCHARA'
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim y
    WHERE y.PlanetTransitFromMoonId = x.Id
      AND y.ClaimCode = CONCAT('TRANSIT_MOON_', REPLACE(c.PlanetCode, 'PLANET_', ''), '_H', RIGHT('0' + CAST(c.HouseFromMoon AS VARCHAR(2)), 2))
      AND y.SourceRefCode = 'SRC_KP_TRANSIT_GOCHARA'
);
GO

-- 4) Rahu/Ketu: not independently itemized by the source -- it states an equivalence instead.
--    Reuse Mars'/Saturn's just-seeded ClaimText, flagged DerivedEquivalence.
;WITH Equivalence (FromPlanetCode, ToPlanetCode) AS (
    SELECT * FROM (VALUES ('PLANET_MARS', 'PLANET_RAHU'), ('PLANET_SATURN', 'PLANET_KETU')) v(FromPlanetCode, ToPlanetCode)
)
INSERT research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim
    (PlanetTransitFromMoonId, ClaimCode, ClaimText, RequiredConditionsJson, EvidenceLevelCode, StatusCode, SourceTextId, SourceRefCode)
SELECT xTo.Id,
       CONCAT('TRANSIT_MOON_', REPLACE(eq.ToPlanetCode, 'PLANET_', ''), '_H', RIGHT('0' + CAST(hFrom.HouseFromMoon AS VARCHAR(2)), 2)),
       cFrom.ClaimText,
       CONCAT(N'{"derivedFrom":"', REPLACE(eq.FromPlanetCode, 'PLANET_', ''), N'","equivalenceStatedBySource":true}'),
       'DerivedEquivalence', 'Proposed', tTo.Id, 'SRC_KP_TRANSIT_GOCHARA'
FROM Equivalence eq
JOIN research.tbl_Dim_SourceReferencePlanet pFrom ON pFrom.PlanetCode = eq.FromPlanetCode
JOIN research.tbl_Dim_SourceReferencePlanet pTo ON pTo.PlanetCode = eq.ToPlanetCode
JOIN research.tbl_Dim_SourceReferencePlanetTransitFromMoon hFrom ON hFrom.PlanetId = pFrom.Id
JOIN research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim cFrom
    ON cFrom.PlanetTransitFromMoonId = hFrom.Id AND cFrom.SourceRefCode = 'SRC_KP_TRANSIT_GOCHARA'
JOIN research.tbl_Dim_SourceReferencePlanetTransitFromMoon xTo ON xTo.PlanetId = pTo.Id AND xTo.HouseFromMoon = hFrom.HouseFromMoon
JOIN research.tbl_Dim_SourceReferencePlanetTransitFromMoonText tTo ON tTo.PlanetTransitFromMoonId = xTo.Id AND tTo.SourceRefCode = 'SRC_KP_TRANSIT_GOCHARA'
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim z
    WHERE z.PlanetTransitFromMoonId = xTo.Id
      AND z.ClaimCode = CONCAT('TRANSIT_MOON_', REPLACE(eq.ToPlanetCode, 'PLANET_', ''), '_H', RIGHT('0' + CAST(hFrom.HouseFromMoon AS VARCHAR(2)), 2))
      AND z.SourceRefCode = 'SRC_KP_TRANSIT_GOCHARA'
);
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'113_seed_planet_transit_moon_research_text.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'113_seed_planet_transit_moon_research_text.sql', N'Seed 108 planet-transit-from-Moon research claims (Sun-Saturn direct, Rahu/Ketu derived by source-stated equivalence) from SRC_KP_TRANSIT_GOCHARA.');
GO

DECLARE @rows INT = (SELECT COUNT(*) FROM research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim WHERE SourceRefCode = 'SRC_KP_TRANSIT_GOCHARA');
PRINT '113 applied: research.tbl_Dim_SourceReferencePlanetTransitFromMoonClaim rows=' + CAST(@rows AS VARCHAR(10));
GO
