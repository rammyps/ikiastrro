-- =====================================================================
-- 124 -- tbl_Rule_RasiNakshatraCombination: the 36 spatially valid
-- Rasi x Nakshatra combinations (not the full 12x27=324 grid -- a
-- nakshatra is 13d20', so it either sits entirely inside one rasi or
-- crosses exactly one rasi boundary; 18 nakshatras fit in one rasi + 9
-- straddle into a second = 18 + 2x9 = 36 valid pairs).
--
-- Source: docs/research/domain/rasi-nakshatra-36-combination-matrix.md
-- (already-reviewed project doc, 2026-09-19). Structural columns
-- (RasiId/NakshatraId/degree span) are DERIVED from tbl_NakshatraPadas
-- (GROUP BY RasiId, NakshatraId), never hand-retyped -- verified against
-- the dev DB to produce exactly 36 groups matching the doc's spans
-- (e.g. Taurus-Krittika = 30.0-40.0 absolute = the doc's "0d-10d within
-- Taurus"). Rasi attributes (element/modality/ruler) and Nakshatra
-- attributes (lord/deity/symbol/Gana/Yoni/Nadi) are NOT duplicated here
-- -- already on tbl_SignAttributes/tbl_Nakshatras, one JOIN away.
--
-- LordRelation and AspectingRasis are non-persisted COMPUTED columns
-- (same pattern as tbl_NakshatraPadas.NakPadaLord, migration 123):
--   - LordRelation: 'Same lord' (sign lord = nakshatra lord), 'Node'
--     (nakshatra lord is Rahu/Ketu -- tbl_Rule_NaturalRelationship has no
--     rows for the nodes, same documented gap as migration 097/105), or
--     the tbl_Rule_NaturalRelationship Friend/Neutral/Enemy lookup
--     (RuleSetId 1) -- directed sign-lord -> nakshatra-lord, matching the
--     doc's own "Lord relation" column. Spot-checked against the doc for
--     all 12 Cancer-and-earlier rows before trusting the rest.
--   - AspectingRasis: comma list of the 3 Rasis this row's RasiId aspects
--     via tbl_Rule_RasiDrishti (migration 107, RuleSetId 1) -- rashi
--     drishti belongs to the sign, so every nakshatra-portion within a
--     sign inherits the same targets (per the doc's own note).
--
-- CombinedCharacter / MainSignifications / PotentialBenefits /
-- PotentialDisadvantages / JudgmentNote are transcribed verbatim from
-- the doc -- NOT fabricated here. Per the doc's own "Source boundaries"
-- section these are project synthesis (concise deductions from the
-- attributed component fields, cross-checked against PVR/BPHS/Brihat
-- Jataka/Raman/Vasudev), not a verbatim classical table -- tagged
-- SourceRefCode = SRC_IKIASTRRO_SYNTHESIS, the same code migration 087
-- registered for exactly this kind of project-authored extension.
--
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -i db/124_rasi_nakshatra_combination.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.fn_GetRasiNakshatraLordRelation', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GetRasiNakshatraLordRelation;
GO

CREATE FUNCTION dbo.fn_GetRasiNakshatraLordRelation (@RasiId TINYINT, @NakshatraId TINYINT)
RETURNS VARCHAR(20)
AS
BEGIN
    DECLARE @RasiLordId TINYINT, @NakLordId TINYINT, @Relation VARCHAR(20);

    SELECT @RasiLordId = RulingPlanetId FROM dbo.tbl_SignAttributes WHERE Id = @RasiId;
    SELECT @NakLordId  = RulingPlanetId FROM dbo.tbl_Nakshatras     WHERE Id = @NakshatraId;

    IF @RasiLordId = @NakLordId
        SET @Relation = 'Same lord';
    ELSE IF @NakLordId IN (8, 9) -- Rahu, Ketu: no tbl_Rule_NaturalRelationship rows for the nodes
        SET @Relation = 'Node';
    ELSE
        SELECT @Relation = RelationshipType
        FROM dbo.tbl_Rule_NaturalRelationship
        WHERE RuleSetId = 1 AND PlanetId = @RasiLordId AND RelatedPlanetId = @NakLordId;

    RETURN @Relation;
END
GO

IF OBJECT_ID('dbo.fn_GetRasiDrishtiTargets', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GetRasiDrishtiTargets;
GO

CREATE FUNCTION dbo.fn_GetRasiDrishtiTargets (@RasiId TINYINT)
RETURNS VARCHAR(100)
AS
BEGIN
    DECLARE @Targets VARCHAR(100);

    SELECT @Targets = STRING_AGG(sa.SignName, ', ') WITHIN GROUP (ORDER BY sa.Id)
    FROM dbo.tbl_Rule_RasiDrishti rd
    JOIN dbo.tbl_SignAttributes sa ON sa.Id = rd.RelatedRasiId
    WHERE rd.RuleSetId = 1 AND rd.RasiId = @RasiId AND rd.IsActive = 1;

    RETURN @Targets;
END
GO

IF OBJECT_ID('dbo.tbl_Rule_RasiNakshatraCombination', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_RasiNakshatraCombination (
        Id                      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_RasiNakshatraCombination PRIMARY KEY,
        RuleSetId               TINYINT       NOT NULL CONSTRAINT FK_Rule_RasiNakshatraCombination_RuleSet   FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        RasiId                  TINYINT       NOT NULL CONSTRAINT FK_Rule_RasiNakshatraCombination_Rasi      FOREIGN KEY REFERENCES dbo.tbl_SignAttributes (Id),
        NakshatraId             TINYINT       NOT NULL CONSTRAINT FK_Rule_RasiNakshatraCombination_Nakshatra FOREIGN KEY REFERENCES dbo.tbl_Nakshatras (Id),
        SpanStartDegree         DECIMAL(9,6)  NOT NULL,
        SpanEndDegree           DECIMAL(9,6)  NOT NULL,
        CombinedCharacter       NVARCHAR(200) NOT NULL,
        MainSignifications      NVARCHAR(300) NOT NULL,
        PotentialBenefits       NVARCHAR(300) NOT NULL,
        PotentialDisadvantages  NVARCHAR(300) NOT NULL,
        JudgmentNote            NVARCHAR(400) NOT NULL,
        MethodCode              VARCHAR(30)   NULL,
        SourceRefCode           VARCHAR(40)   NULL CONSTRAINT CK_Rule_RasiNakshatraCombination_Src CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        IsActive                BIT           NOT NULL CONSTRAINT DF_Rule_RasiNakshatraCombination_IsActive DEFAULT 1,
        CONSTRAINT UQ_Rule_RasiNakshatraCombination UNIQUE (RuleSetId, RasiId, NakshatraId),
        LordRelation    AS (dbo.fn_GetRasiNakshatraLordRelation(RasiId, NakshatraId)),
        AspectingRasis  AS (dbo.fn_GetRasiDrishtiTargets(RasiId))
    );

    CREATE INDEX IX_Rule_RasiNakshatraCombination_Nakshatra ON dbo.tbl_Rule_RasiNakshatraCombination (NakshatraId);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_RasiNakshatraCombination)
BEGIN
    ;WITH span AS
    (
        SELECT RasiId, NakshatraId, MIN(StartDegree) AS SpanStart, MAX(EndDegree) AS SpanEnd
        FROM dbo.tbl_NakshatraPadas
        GROUP BY RasiId, NakshatraId
    ),
    txt (RasiName, NakshatraName, CombinedCharacter, MainSignifications, PotentialBenefits, PotentialDisadvantages, JudgmentNote) AS
    (
        SELECT * FROM (VALUES
        (N'Aries', N'Ashwini',
            N'Rapid, pioneering, instinctive, restorative',
            N'Beginnings, movement, rescue, healing, initiative',
            N'Fast response, courage, recovery, entrepreneurship',
            N'Haste, unfinished action, impatience, abrupt detachment',
            N'Judge Mars, Ketu and the occupied pada; speed needs direction.'),
        (N'Aries', N'Bharani',
            N'Forceful but containing; passionate responsibility',
            N'Bearing burdens, sexuality, creation, restraint, consequences',
            N'Endurance under pressure, creative force, moral courage',
            N'Excess, control struggles, resentment, impulsive desire',
            N'Mars-Venus tension can create or consume; dignity decides the outlet.'),
        (N'Aries', N'Krittika',
            N'Incisive, purifying, competitive, decisive',
            N'Cutting away, leadership, defence, cooking/nourishment',
            N'Clarity, bravery, disciplined purification, executive ability',
            N'Harsh speech, anger, severance, self-righteousness',
            N'Only pada 1 is in Aries; its D9 bridge materially changes expression.'),
        (N'Taurus', N'Krittika',
            N'Controlled heat applied to material form',
            N'Refinement, food, value protection, craft, standards',
            N'Productive discipline, durable creation, practical discrimination',
            N'Pride-versus-comfort conflict, stubborn criticism, possessiveness',
            N'Padas 2-4 shift Krittika from fiery initiation to consolidation.'),
        (N'Taurus', N'Rohini',
            N'Attractive, fertile, receptive, materially generative',
            N'Growth, beauty, land, food, fertility, arts, resources',
            N'Creativity, cultivation, popularity, stability, sensual intelligence',
            N'Attachment, indulgence, jealousy, resistance to change',
            N'Strong benefic symbolism still requires Moon/Venus condition and house context.'),
        (N'Taurus', N'Mrigashira',
            N'Patient search for tangible satisfaction',
            N'Research, courtship, acquisition, paths, sensory inquiry',
            N'Curious craftsmanship, commercial exploration, persistence',
            N'Restlessness beneath stability, acquisitiveness, indecision',
            N'Taurus contains padas 1-2; compare with the more verbal Gemini half.'),
        (N'Gemini', N'Mrigashira',
            N'Mobile, questioning, mentally hunting',
            N'Information search, travel, language, experimentation',
            N'Investigation, adaptability, networking, quick learning',
            N'Scattered attention, suspicion, perpetual dissatisfaction',
            N'Gemini contains padas 3-4; Mars energizes Mercury''s inquiry.'),
        (N'Gemini', N'Ardra',
            N'Intensely analytical, disruptive, cathartic',
            N'Storms, technology, grief, deconstruction, radical learning',
            N'Breakthrough insight, resilience, technical intelligence, truth-seeking',
            N'Volatility, destructive speech, obsession, nervous overload',
            N'Distinguish necessary deconstruction from habitual disruption; judge Rahu.'),
        (N'Gemini', N'Punarvasu',
            N'Reframing, teaching and returning to coherence',
            N'Renewal, homecoming, education, repeat attempts, generosity',
            N'Recovery, broad learning, optimism, communicative wisdom',
            N'Repetition without completion, diffusion, over-promising',
            N'Gemini contains padas 1-3; the Cancer pada becomes more protective.'),
        (N'Cancer', N'Punarvasu',
            N'Protective restoration and emotional renewal',
            N'Return home, caregiving, shelter, faith, replenishment',
            N'Forgiveness, nurturance, emotional resilience, hospitality',
            N'Retreat into familiarity, dependency, cyclical backtracking',
            N'Only pada 4 is in Cancer; Moon and Jupiter become the key dispositor pair.'),
        (N'Cancer', N'Pushya',
            N'Duty-bound nourishment, disciplined care',
            N'Teaching, institutions, food, protection, tradition, service',
            N'Reliability, stewardship, mature support, sustained learning',
            N'Emotional inhibition, over-duty, paternalism, self-denial',
            N'Auspicious symbolism does not cancel Saturnine delay or affliction.'),
        (N'Cancer', N'Ashlesha',
            N'Penetrating, protective, binding, psychologically acute',
            N'Secrets, strategy, medicine, toxins, lineage, entanglement',
            N'Insight, persuasive intelligence, healing knowledge, tenacity',
            N'Manipulation, suspicion, enmeshment, corrosive speech',
            N'Separate perceptiveness from control; inspect Moon-Mercury condition.'),
        (N'Leo', N'Magha',
            N'Ancestral, authoritative, status-conscious',
            N'Lineage, inheritance, office, ritual, legitimacy',
            N'Leadership, loyalty to tradition, custodianship, dignity',
            N'Entitlement, hierarchy fixation, pride, living through ancestry',
            N'Judge Sun, Ketu and the actual strength of ancestral/9th-house factors.'),
        (N'Leo', N'Purva Phalguni',
            N'Creative display, pleasure and social magnetism',
            N'Romance, arts, leisure, union, patronage, enjoyment',
            N'Charisma, creativity, generosity, relationship warmth',
            N'Vanity, indulgence, laziness, dramatic attachment',
            N'Sun-Venus friction makes values and recognition important modifiers.'),
        (N'Leo', N'Uttara Phalguni',
            N'Principled authority expressed through commitments',
            N'Contracts, alliances, patronage, marriage duties, leadership',
            N'Integrity, reliability, organization, lasting support',
            N'Rigidity, paternalism, dominance, over-identification with duty',
            N'Only pada 1 is in Leo; same lord concentrates solar themes.'),
        (N'Virgo', N'Uttara Phalguni',
            N'Practical service to agreements and institutions',
            N'Administration, contracts, counsel, maintenance, alliances',
            N'Competence, ethical service, useful leadership, precision',
            N'Perfectionism, officiousness, anxiety over obligations',
            N'Padas 2-4 operationalize the Leo promise; inspect Mercury-Sun dignity.'),
        (N'Virgo', N'Hasta',
            N'Skilled, adaptive, hands-on and controlling',
            N'Craft, healing hands, writing, trade, making, acquisition',
            N'Dexterity, wit, practical intelligence, manifestation',
            N'Micromanagement, trickery, nervous control, compulsive fixing',
            N'Moon-Mercury tension may alternate feeling and analysis.'),
        (N'Virgo', N'Chitra',
            N'Technical design, refinement and exacting beauty',
            N'Architecture, engineering, repair, ornament, distinction',
            N'Precision, inventive craft, problem-solving, aesthetic skill',
            N'Hypercriticism, competitiveness, image fixation, irritability',
            N'Virgo contains padas 1-2; compare Libra''s relational/aesthetic half.'),
        (N'Libra', N'Chitra',
            N'Bold design expressed through relationship and display',
            N'Beauty, architecture, attraction, social differentiation',
            N'Artistic courage, negotiation with edge, visible craftsmanship',
            N'Rivalry, vanity, unstable attraction, conflict over appearances',
            N'Libra contains padas 3-4; Mars adds heat to Venusian balance.'),
        (N'Libra', N'Swati',
            N'Independent, mobile, commercial, self-forming',
            N'Wind, trade, travel, autonomy, networks, bargaining',
            N'Flexibility, entrepreneurship, diplomacy, cross-cultural reach',
            N'Rootlessness, opportunism, indecision, excessive independence',
            N'Judge Rahu and Venus; flexibility is beneficial only with an anchor.'),
        (N'Libra', N'Vishakha',
            N'Socially strategic and goal-oriented',
            N'Achievement, alliances, competition, branching choices, ceremony',
            N'Focus, persuasion, ambition, coalition-building',
            N'Obsession with outcomes, rivalry, divided loyalties',
            N'Libra contains padas 1-3; the Scorpio pada intensifies the objective.'),
        (N'Scorpio', N'Vishakha',
            N'Concentrated pursuit with ideological conviction',
            N'Penetration, conquest, vows, transformation through goals',
            N'Determination, strategic faith, crisis leadership',
            N'Fanaticism, revenge, inability to release a goal',
            N'Only pada 4 is in Scorpio; Mars-Jupiter friendship supports forceful purpose.'),
        (N'Scorpio', N'Anuradha',
            N'Loyal, disciplined, relational depth',
            N'Friendship, devotion, organization, pilgrimage, cooperation',
            N'Endurance, alliance-building, research, disciplined devotion',
            N'Emotional austerity, loyalty tests, control, delayed trust',
            N'Mitra''s cooperation must be read alongside Scorpio''s guardedness.'),
        (N'Scorpio', N'Jyeshtha',
            N'Protective intelligence, seniority and tactical control',
            N'Rank, guardianship, secrecy, crisis command, responsibility',
            N'Resourcefulness, authority under pressure, strategic speech',
            N'Suspicion, superiority, manipulation, chronic defensiveness',
            N'Mars-Mercury enmity can sharpen strategy but strain communication.'),
        (N'Sagittarius', N'Mula',
            N'Philosophical root-cause inquiry and uprooting',
            N'Origins, research, destruction, medicine, liberation, truth',
            N'Fearless investigation, reform, detachment, recovery from loss',
            N'Nihilism, needless demolition, dogmatism, instability',
            N'Distinguish liberating removal from indiscriminate destruction; judge Ketu.'),
        (N'Sagittarius', N'Purva Ashadha',
            N'Persuasive idealism with emotional and aesthetic force',
            N'Advocacy, purification, travel, inspiration, declaration',
            N'Optimism, influence, creativity, renewal, campaigning ability',
            N'Invincibility complex, excess, moral vanity, avoidance of criticism',
            N'Jupiter-Venus enmity marks competing philosophies, not automatic failure.'),
        (N'Sagittarius', N'Uttara Ashadha',
            N'Principled expansion seeking durable victory',
            N'Ethics, governance, leadership, treaties, long-term achievement',
            N'Integrity, statesmanship, perseverance, public purpose',
            N'Righteousness, inflexibility, delayed gratification, authority conflict',
            N'Only pada 1 is in Sagittarius; ideals dominate over administration.'),
        (N'Capricorn', N'Uttara Ashadha',
            N'Institutional ambition and duty-bound leadership',
            N'Government, responsibility, legacy, systems, earned status',
            N'Endurance, execution, accountability, lasting achievement',
            N'Workaholism, status pressure, cold authority, Sun-Saturn conflict',
            N'Padas 2-4 make the promise concrete; authority and duty must reconcile.'),
        (N'Capricorn', N'Shravana',
            N'Disciplined listening, transmission and preservation',
            N'Learning, oral tradition, reputation, pathways, institutions',
            N'Patient study, communication, cultural memory, organizational skill',
            N'Conformity, anxiety about reputation, emotional reserve, gossip',
            N'Moon-Saturn tension can mature listening or harden emotional habits.'),
        (N'Capricorn', N'Dhanishta',
            N'Ambitious rhythm, coordinated material action',
            N'Wealth, music, teams, engineering, property, timing',
            N'Productivity, leadership in groups, resource coordination',
            N'Harsh competitiveness, status hunger, interpersonal dryness',
            N'Capricorn contains padas 1-2; Saturn-Mars strain can become disciplined force.'),
        (N'Aquarius', N'Dhanishta',
            N'Collective action, technical rhythm and social ambition',
            N'Networks, organizations, performance, innovation, shared resources',
            N'Team mobilization, systems thinking, public accomplishment',
            N'Group conflict, ideological rigidity, detachment from intimacy',
            N'Aquarius contains padas 3-4; group purpose replaces Capricorn''s status focus.'),
        (N'Aquarius', N'Shatabhisha',
            N'Detached diagnosis, boundary-setting and unconventional healing',
            N'Research, medicine, secrecy, networks, regulation, isolation',
            N'Objective analysis, innovation, reform, healing through systems',
            N'Alienation, secrecy, extremism, obsessive diagnosis',
            N'Judge Saturn and Rahu; solitude can be medicinal or isolating.'),
        (N'Aquarius', N'Purva Bhadrapada',
            N'Ideological intensity and radical social vision',
            N'Austerity, vows, reform, endings, philosophy, collective causes',
            N'Commitment, originality, moral courage, transformative teaching',
            N'Extremism, pessimism, double life, scorched-earth rhetoric',
            N'Aquarius contains padas 1-3; the Pisces pada internalizes the vision.'),
        (N'Pisces', N'Purva Bhadrapada',
            N'Mystical intensity and total commitment',
            N'Renunciation, liminality, spiritual fire, endings, sacrifice',
            N'Vision, faith, depth, capacity for profound transition',
            N'Escapism through ideology, martyrdom, emotional extremity',
            N'Only pada 4 is in Pisces; same lord amplifies Jupiterian belief.'),
        (N'Pisces', N'Uttara Bhadrapada',
            N'Deep stillness, containment and mature compassion',
            N'Depth, rest, foundations, sleep, endings, responsibility',
            N'Patience, wisdom, emotional endurance, quiet service',
            N'Inertia, melancholy, withdrawal, carrying hidden burdens',
            N'Saturn stabilizes Pisces but may suppress movement; judge both lords.'),
        (N'Pisces', N'Revati',
            N'Guiding, imaginative, mobile and completion-oriented',
            N'Safe passage, travel, protection, music, accounting, endings',
            N'Compassionate guidance, adaptability, creativity, completion',
            N'Diffusion, avoidance, misplaced trust, weak boundaries',
            N'Jupiter-Mercury enmity describes differing knowledge styles; verify dispositors.')
        ) AS v (RasiName, NakshatraName, CombinedCharacter, MainSignifications, PotentialBenefits, PotentialDisadvantages, JudgmentNote)
    )
    INSERT dbo.tbl_Rule_RasiNakshatraCombination
        (RuleSetId, RasiId, NakshatraId, SpanStartDegree, SpanEndDegree, CombinedCharacter, MainSignifications, PotentialBenefits, PotentialDisadvantages, JudgmentNote, MethodCode, SourceRefCode)
    SELECT
        1, span.RasiId, span.NakshatraId, span.SpanStart, span.SpanEnd,
        txt.CombinedCharacter, txt.MainSignifications, txt.PotentialBenefits, txt.PotentialDisadvantages, txt.JudgmentNote,
        'RASI_NAKSHATRA_COMBINATION', 'SRC_IKIASTRRO_SYNTHESIS'
    FROM span
    JOIN dbo.tbl_SignAttributes sa ON sa.Id = span.RasiId
    JOIN dbo.tbl_Nakshatras nk     ON nk.Id = span.NakshatraId
    JOIN txt ON txt.RasiName = sa.SignName AND txt.NakshatraName = nk.NakshatraName;

    IF (SELECT COUNT(*) FROM dbo.tbl_Rule_RasiNakshatraCombination) <> 36
        RAISERROR('124: expected exactly 36 tbl_Rule_RasiNakshatraCombination rows -- check for a Rasi/Nakshatra name mismatch between the txt VALUES list and tbl_SignAttributes/tbl_Nakshatras.', 16, 1);

    IF EXISTS (SELECT 1 FROM dbo.tbl_Rule_RasiNakshatraCombination WHERE LordRelation IS NULL)
        RAISERROR('124: at least one row failed to resolve LordRelation -- check tbl_Rule_NaturalRelationship coverage.', 16, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_RasiNakshatraCombination')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_RasiNakshatraCombination', 'RELATIONSHIP', 'RASI_NAKSHATRA_COMBINATION',
            'The 36 spatially valid Rasi x Nakshatra combinations (not the full 12x27 grid): degree span (from tbl_NakshatraPadas), computed LordRelation (via tbl_Rule_NaturalRelationship) and AspectingRasis (via tbl_Rule_RasiDrishti), plus project-synthesis character/significations/benefits/disadvantages/judgment note (SRC_IKIASTRRO_SYNTHESIS) from rasi-nakshatra-36-combination-matrix.md.',
            '124_rasi_nakshatra_combination.sql');
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '124_rasi_nakshatra_combination.sql',
       'tbl_Rule_RasiNakshatraCombination created + seeded (36 rows, RuleSetId 1); 2 computed columns (LordRelation, AspectingRasis); 1 tbl_Rule_Catalog row'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '124_rasi_nakshatra_combination.sql');
GO

PRINT '124 applied: tbl_Rule_RasiNakshatraCombination created and seeded (36 rows).';
GO
