-- =====================================================================
-- 135 — Source-grounded planet x Sayanaadi interpretation catalogue.
--
-- Adds one paraphrased interpretation for every combination of the
-- 12 Sayanaadi states and 9 grahas (108 rows). These are interpretive
-- source summaries, not additional computed facts. Conditional clauses
-- are kept separate so consumers do not present them as unconditional.
-- Source: SRC_PVR_INTEGRATED §15.4.4, pp.193-199.
-- =====================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID('dbo.tbl_Rule_PostureStateInterpretation', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_PostureStateInterpretation
    (
        Id                 INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rule_PostureStateInterpretation PRIMARY KEY,
        RuleSetId          TINYINT NOT NULL CONSTRAINT FK_Rule_PostureStateInterpretation_RuleSet
                               REFERENCES dbo.tbl_Rule_Sets(Id),
        PlanetaryStateId   TINYINT NOT NULL CONSTRAINT FK_Rule_PostureStateInterpretation_State
                               REFERENCES dbo.tbl_Dim_PlanetaryState(Id),
        PlanetId           TINYINT NOT NULL CONSTRAINT FK_Rule_PostureStateInterpretation_Planet
                               REFERENCES dbo.tbl_Planets(Id),
        InterpretationText NVARCHAR(500) NOT NULL,
        ConditionNotes     NVARCHAR(500) NULL,
        SourceRefCode      VARCHAR(40) NOT NULL CONSTRAINT FK_Rule_PostureStateInterpretation_Source
                               REFERENCES dbo.tbl_Dim_Source(Code),
        SourceLocator      NVARCHAR(200) NOT NULL,
        CONSTRAINT UQ_Rule_PostureStateInterpretation UNIQUE (RuleSetId, PlanetaryStateId, PlanetId)
    );

    CREATE INDEX IX_Rule_PostureStateInterpretation_StatePlanet
        ON dbo.tbl_Rule_PostureStateInterpretation(PlanetaryStateId, PlanetId);
END
GO

DECLARE @Rows TABLE
(
    StateName NVARCHAR(20), PlanetName VARCHAR(20),
    InterpretationText NVARCHAR(500), ConditionNotes NVARCHAR(500)
);

INSERT @Rows (StateName, PlanetName, InterpretationText, ConditionNotes) VALUES
-- 1 Sayana
(N'Sayana','Sun',N'The source associates this resting mode with reduced vitality and sensitivity around digestion, circulation and the heart.',NULL),
(N'Sayana','Moon',N'The source associates this resting mode with honour and sensuality, but also inertia and weak financial stewardship.',NULL),
(N'Sayana','Mars',N'The source associates this resting mode with physical strain, injuries and inflammatory or skin complaints.',NULL),
(N'Sayana','Mercury',N'The source associates this resting mode with excessive appetite and pleasure-seeking that can weaken judgment or restraint.',N'When Mercury is in the ascendant, the source adds impaired movement and a distinctive eye appearance.'),
(N'Sayana','Jupiter',N'The source retains strength for Jupiter, while associating it with a subdued voice, marked appearance and apprehension about opponents.',NULL),
(N'Sayana','Venus',N'The source retains strength for Venus, while associating it with irritability, material strain, sensual excess and dental concerns.',NULL),
(N'Sayana','Saturn',N'The source associates this resting mode with deprivation or thirst early in life, followed by improved material circumstances later.',NULL),
(N'Sayana','Rahu',N'The source associates this resting mode with repeated difficulty and dissatisfaction.',N'In Aries, Taurus, Gemini or Virgo, the source instead adds potential for wealth.'),
(N'Sayana','Ketu',N'The source associates this resting mode with recurring health difficulties.',N'In Aries, Taurus, Gemini or Virgo, the source instead adds potential for wealth.'),

-- 2 Upavesana
(N'Upavesana','Sun',N'The source associates this seated mode with conflict-proneness, emotional hardness and financial loss.',NULL),
(N'Upavesana','Moon',N'The source associates this seated mode with poor judgment, material strain and conduct that can damage trust.',NULL),
(N'Upavesana','Mars',N'The source gives Mars strength, prominence and wealth here, but warns that force may be expressed without honesty or restraint.',NULL),
(N'Upavesana','Mercury',N'The source makes Mercury highly conditional: character and prosperity depend strongly on placement and association.',N'Ascendant placement is favourable for character; benefic association supports wealth and happiness, while malefic association indicates financial strain.'),
(N'Upavesana','Jupiter',N'The source associates this seated mode with strong speech but conflict with authority or opponents and physical discomfort in the limbs.',NULL),
(N'Upavesana','Venus',N'The source associates this seated mode with prosperity, recognition, happiness and success over opposition.',NULL),
(N'Upavesana','Saturn',N'The source associates this seated mode with self-respect under pressure, danger, punishment or conflict with opponents.',NULL),
(N'Upavesana','Rahu',N'The source combines public or institutional recognition with physical distress and financial difficulty.',NULL),
(N'Upavesana','Ketu',N'The source associates this seated mode with physical distress and threats from opponents, theft or hidden dangers.',NULL),

-- 3 Netrapaani
(N'Netrapaani','Sun',N'The source associates this attentive mode with wisdom, generosity, strength, prosperity and favour from authority.',NULL),
(N'Netrapaani','Moon',N'The source associates this attentive mode with ill health, excessive speech and behaviour that undermines wellbeing.',NULL),
(N'Netrapaani','Mars',N'The source gives Mars administrative or local authority away from the ascendant.',N'In the ascendant, the source instead indicates poverty or scarcity.'),
(N'Netrapaani','Mercury',N'The source associates this attentive mode with honour but weak counsel, learning or dependable support.',N'In the fifth house, it adds strain involving spouse or children; institutional patronage may still bring income.'),
(N'Netrapaani','Jupiter',N'The source associates this attentive mode with reduced health and wealth, strong appetite for entertainment and unconventional company.',NULL),
(N'Netrapaani','Venus',N'The source varies by house: angular placement may bring eye or financial difficulties, while other placements support a substantial home.',N'The difficult clause applies in the ascendant, seventh or tenth house.'),
(N'Netrapaani','Saturn',N'The source associates this attentive mode with intelligence, articulate speech, artistic learning, friendship and support from authority.',NULL),
(N'Netrapaani','Rahu',N'The source associates this attentive mode with eye concerns, financial loss and trouble from hostile or deceptive people.',NULL),
(N'Netrapaani','Ketu',N'The source associates this attentive mode with eye concerns and trouble from hostile people, theft or authority.',NULL),

-- 4 Prakaasana
(N'Prakaasana','Sun',N'The source associates this shining mode with generosity, strength, prosperity, appearance and persuasive public speech.',NULL),
(N'Prakaasana','Moon',N'The source associates this shining mode with reputation, virtue, patronage, prosperity, comfort, devotion and adornment.',NULL),
(N'Prakaasana','Mars',N'The source associates this shining mode with principled action and recognition from authority.',N'In the fifth house it warns of difficulty involving children; conjunction with Rahu there intensifies the risk.'),
(N'Prakaasana','Mercury',N'The source associates this shining mode with generosity, kindness, learning, wise action and capacity to overcome harmful groups.',NULL),
(N'Prakaasana','Jupiter',N'The source associates this shining mode with virtue, happiness, comfort, spiritual travel and splendour.',N'Exaltation is said to amplify fame and prosperity.'),
(N'Prakaasana','Venus',N'The source associates this shining mode with dignity, artistic and literary interest and conduct suited to high position.',N'The favourable description is specifically tied to exaltation or placement in an own or friendly sign.'),
(N'Prakaasana','Saturn',N'The source associates this shining mode with intelligence, prosperity, kindness, entertainment and devotion.',NULL),
(N'Prakaasana','Rahu',N'The source associates this shining mode with prosperity abroad, advancement, public responsibility, virtue and charm.',NULL),
(N'Prakaasana','Ketu',N'The source associates this shining mode with wealth, integrity, enthusiasm, residence abroad and service to authority.',NULL),

-- 5 Gamana
(N'Gamana','Sun',N'The source associates this moving mode with foreign residence, restlessness, fear, anger and material difficulty.',NULL),
(N'Gamana','Moon',N'The source associates this moving mode with anxiety and vulnerability.',N'For a waning Moon it adds harsh conduct and eye concerns; for a waxing Moon it emphasizes fear.'),
(N'Gamana','Mars',N'The source associates this moving mode with wandering, disputes, injuries and inflammatory or skin complaints.',NULL),
(N'Gamana','Mercury',N'The source associates this moving mode with frequent access to institutions or authority and resulting prosperity.',NULL),
(N'Gamana','Jupiter',N'The source associates this moving mode with courage, friendship, scholarship and wealth.',NULL),
(N'Gamana','Venus',N'The source associates this moving mode with separation from family, concern involving the mother and fear of opponents.',NULL),
(N'Gamana','Saturn',N'The source associates this moving mode with wealth, learning, capable children and institutional standing, but also acquisitiveness.',NULL),
(N'Gamana','Rahu',N'The source associates this moving mode with children, learning, prosperity, generosity and recognition from authority.',NULL),
(N'Gamana','Ketu',N'The source associates this moving mode with children, learning, prosperity, generosity, virtue and distinction.',NULL),

-- 6 Aagamana
(N'Aagamana','Sun',N'The source associates this returning mode with travel, social alienation, poor boundaries and conduct that damages trust.',NULL),
(N'Aagamana','Moon',N'The source combines honour with sadness, poor judgment, material strain and physical discomfort in the feet.',NULL),
(N'Aagamana','Mars',N'The source associates this returning mode with good character, valuable possessions, martial capacity and success against opposition.',NULL),
(N'Aagamana','Mercury',N'The source associates this returning mode with frequent institutional or courtly contact and prosperity.',NULL),
(N'Aagamana','Jupiter',N'The source associates this returning mode with a prosperous household, supportive people, relationships and wealth.',NULL),
(N'Aagamana','Venus',N'The source associates this returning mode with wealth, enthusiasm and pilgrimage, alongside physical discomfort in hands or feet.',NULL),
(N'Aagamana','Saturn',N'The source associates this returning mode with separation from family, wandering, confusion and unhappiness.',NULL),
(N'Aagamana','Rahu',N'The source associates this returning mode with irritability, poor judgment, scarcity, possessiveness and uncontrolled desire.',NULL),
(N'Aagamana','Ketu',N'The source associates this returning mode with health and financial difficulties, harmful speech and injury to others.',NULL),

-- 7 Sabhaa
(N'Sabhaa','Sun',N'The source associates this assembly mode with generosity, strength, virtue, affection and possession of land or valuable resources.',NULL),
(N'Sabhaa','Moon',N'The source associates this assembly mode with distinction, good character, pleasure and recognition from authority.',NULL),
(N'Sabhaa','Mars',N'The source associates this assembly mode with learning, honour, wealth and generosity in public or institutional settings.',N'Exaltation supports success and principled courage; trinal or twelfth-house placement carries separate cautions in the source.'),
(N'Sabhaa','Mercury',N'The source associates this assembly mode with wealth, leadership or ministerial ability, good works, devotion and spiritual orientation.',N'Exaltation strengthens the prosperous and constructive indications.'),
(N'Sabhaa','Jupiter',N'The source associates this assembly mode with excellent speech, high learning, distinction and substantial wealth.',NULL),
(N'Sabhaa','Venus',N'The source associates this assembly mode with public distinction, virtue, generosity, prosperity and success over opposition.',NULL),
(N'Sabhaa','Saturn',N'The source associates this assembly mode with judicial or deliberative skill, brilliance and substantial resources.',NULL),
(N'Sabhaa','Rahu',N'The source associates this assembly mode with learning, many abilities, prosperity and happiness, moderated by possessiveness.',NULL),
(N'Sabhaa','Ketu',N'The source associates this assembly mode with forceful speech, pride and specialised ability, but cautions against misdirected or secretive interests.',NULL),

-- 8 Aagama
(N'Aagama','Sun',N'The source associates this acquiring mode with conflict, instability, weakened vitality and conduct that departs from principle.',NULL),
(N'Aagama','Moon',N'The source makes this acquiring mode depend strongly on lunar phase.',N'A waxing Moon supports articulate and principled expression; a waning Moon brings relational and health cautions.'),
(N'Aagama','Mars',N'The source associates this acquiring mode with timid or harmful choices, difficult company and possible ear concerns.',NULL),
(N'Aagama','Mercury',N'The source associates this acquiring mode with income through service and a mixed family narrative in which a daughter brings distinction.',NULL),
(N'Aagama','Jupiter',N'The source associates this acquiring mode with comfort, recognition, learning, friendship, children, transport and a principled path.',NULL),
(N'Aagama','Venus',N'The source associates this acquiring mode with material loss, opposition, health strain and separation involving spouse or children.',NULL),
(N'Aagama','Saturn',N'The source associates this acquiring mode with health difficulties, limited skill expression and lack of institutional support.',NULL),
(N'Aagama','Rahu',N'The source associates this acquiring mode with financial loss, litigation, fear of opponents, separation and manipulative conduct.',NULL),
(N'Aagama','Ketu',N'The source associates this acquiring mode with damaged reputation, conflict among close relations, illness and opposition.',NULL),

-- 9 Bhojana
(N'Bhojana','Sun',N'The source associates this consuming mode with reduced strength, joint or head discomfort, financial loss and unreliable speech.',NULL),
(N'Bhojana','Moon',N'The source makes this consuming mode depend strongly on lunar phase.',N'A waxing Moon supports status, recognition, transport, family and service; a waning Moon makes those results difficult to secure.'),
(N'Bhojana','Mars',N'The source links this consuming mode to enjoyment and provision when Mars is strong, but to dishonourable action when weak.',N'Requires an independent assessment of planetary strength.'),
(N'Bhojana','Mercury',N'The source associates this consuming mode with financial loss through disputes or authority and reduced marital contentment.',NULL),
(N'Bhojana','Jupiter',N'The source associates this consuming mode with excellent nourishment, prosperity and marks of distinction.',NULL),
(N'Bhojana','Venus',N'The source gives a counter-intuitive conditional result: debility is linked with wealth and scholarly respect, while other conditions emphasize deprivation, illness or opposition.',N'Do not simplify this entry without retaining its dignity condition.'),
(N'Bhojana','Saturn',N'The source associates this consuming mode with enjoyment of food alongside weak sight or confusion.',NULL),
(N'Bhojana','Rahu',N'The source associates this consuming mode with scarcity, timidity and reduced happiness through family or nourishment.',NULL),
(N'Bhojana','Ketu',N'The source associates this consuming mode with hunger, illness, wandering and poverty.',NULL),

-- 10 Nriyalipsaa (source/DB spelling retained)
(N'Nriyalipsaa','Sun',N'The source associates this expressive mode with scholarship, poetry, public discussion and recognition from authority.',NULL),
(N'Nriyalipsaa','Moon',N'The source makes this expressive mode depend strongly on lunar phase.',N'A waxing Moon supports strength and knowledge of poetry, music and arts; a waning Moon carries a conduct warning.'),
(N'Nriyalipsaa','Mars',N'The source associates this expressive mode with wealth and valuable possessions gained through powerful connections.',NULL),
(N'Nriyalipsaa','Mercury',N'The source associates this expressive mode with honour, courage, learning, friends, children, transport and valuable resources.',N'Placement in a malefic sign adds a warning about uncontrolled sensuality.'),
(N'Nriyalipsaa','Jupiter',N'The source associates this expressive mode with honour, wealth, religious and esoteric learning, linguistic mastery and respect from learned or powerful people.',NULL),
(N'Nriyalipsaa','Venus',N'The source associates this expressive mode with skill in literature, arts and music, merit and prosperity.',NULL),
(N'Nriyalipsaa','Saturn',N'The source associates this expressive mode with principled conduct, prosperity, courage and recognition from authority.',NULL),
(N'Nriyalipsaa','Rahu',N'The source associates this expressive mode with serious health or eye concerns, fear of opponents and financial loss.',NULL),
(N'Nriyalipsaa','Ketu',N'The source associates this expressive mode with health and eye concerns and conduct that departs from principle.',NULL),

-- 11 Kautuka
(N'Kautuka','Sun',N'The source associates this eager mode with happiness, learning, ritual practice, poetry, attractiveness and access to authority.',NULL),
(N'Kautuka','Moon',N'The source associates this eager mode with status, prosperity and strong sensual expression.',NULL),
(N'Kautuka','Mars',N'The source associates this eager mode with curiosity, friendship and children.',N'Exaltation adds recognition from authority and principled conduct.'),
(N'Kautuka','Mercury',N'The source gives Mercury different expressions by house, ranging from musical skill and good conduct to excessive sensuality.',N'Ascendant: musical skill; seventh/eighth: sensual excess; ninth: constructive conduct and spiritual merit.'),
(N'Kautuka','Jupiter',N'The source associates this eager mode with curiosity, kindness, happiness, recognition, children and reputation.',NULL),
(N'Kautuka','Venus',N'The source associates this eager mode with prosperity, learning, fame and respect in assemblies.',NULL),
(N'Kautuka','Saturn',N'The source associates this eager mode with land, wealth, happiness, artistic or poetic learning and sensual enjoyment.',NULL),
(N'Kautuka','Rahu',N'The source associates this eager mode with wandering, boundary violations and taking what belongs to others.',NULL),
(N'Kautuka','Ketu',N'The source associates this eager mode with wandering, professional loss and uncontrolled sensual or unprincipled conduct.',NULL),

-- 12 Nidraa
(N'Nidraa','Sun',N'The source associates this sleeping mode with lethargy, foreign residence and strain involving partnership.',NULL),
(N'Nidraa','Moon',N'The source makes this sleeping mode depend strongly on Jupiter association.',N'Association with Jupiter supports distinction; without it, the source warns of relational and financial trouble.'),
(N'Nidraa','Mars',N'The source associates this sleeping mode with anger, poor judgment, illness, scarcity and departure from constructive conduct.',NULL),
(N'Nidraa','Mercury',N'The source associates this sleeping mode with disturbed rest, neck discomfort, conflict among close relations and financial loss.',NULL),
(N'Nidraa','Jupiter',N'The source associates this sleeping mode with scarcity, poor judgment and reduced engagement with learning or principled action.',NULL),
(N'Nidraa','Venus',N'The source associates this sleeping mode with dependence on others, criticism, excessive speech and wandering.',NULL),
(N'Nidraa','Saturn',N'The source associates this sleeping mode with wealth, courage, attractive character and success against opponents.',NULL),
(N'Nidraa','Rahu',N'The source associates this sleeping mode with virtue, family happiness, confidence, courage and wealth.',NULL),
(N'Nidraa','Ketu',N'The source associates this sleeping mode with virtue, wealth, agricultural resources and a life oriented toward entertainment.',NULL);

IF (SELECT COUNT(*) FROM @Rows) <> 108
    THROW 51000, 'Expected exactly 108 Sayanaadi interpretation seed rows.', 1;

INSERT dbo.tbl_Rule_PostureStateInterpretation
    (RuleSetId, PlanetaryStateId, PlanetId, InterpretationText, ConditionNotes, SourceRefCode, SourceLocator)
SELECT 1, s.Id, p.Id, r.InterpretationText, r.ConditionNotes,
       'SRC_PVR_INTEGRATED', N'§15.4.4, pp.193-199'
FROM @Rows r
JOIN dbo.tbl_Dim_PlanetaryState s
  ON s.AvasthaSystem = 'Sayanadi' AND s.StateName = r.StateName
JOIN dbo.tbl_Planets p ON p.PlanetName = r.PlanetName
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.tbl_Rule_PostureStateInterpretation x
    WHERE x.RuleSetId = 1 AND x.PlanetaryStateId = s.Id AND x.PlanetId = p.Id
);

IF (SELECT COUNT(*) FROM dbo.tbl_Rule_PostureStateInterpretation WHERE RuleSetId = 1) <> 108
    THROW 51001, 'RuleSet 1 must contain exactly 108 Sayanaadi interpretation rows.', 1;
GO

INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '135_seed_posture_state_interpretations.sql',
       '108 source-grounded planet x Sayanaadi interpretation summaries from SRC_PVR_INTEGRATED sec 15.4.4.'
WHERE NOT EXISTS
(
    SELECT 1 FROM dbo.SchemaMigrations
    WHERE ScriptName = '135_seed_posture_state_interpretations.sql'
);
GO

PRINT '135 applied: 108 planet x Sayanaadi interpretations seeded.';
GO
