-- =====================================================================
-- 087 - Life-matter reference: bridging PVR's areas-of-life (Table 11),
-- house significations (sec 7.2), Table 12 graha-lagna houses, and
-- naisargika karaka into one practical "how do I read this specific
-- matter" table.
--
-- Source: rammyps's "Area-of-life master table" worksheet (10 categories,
-- ~96 specific matters), cross-checked against the raw PVR text
-- (D:\@ClaudeSpace\BookExtracts\pvr-integrated-approach-raw.txt) and
-- against data this project already seeded from the book:
--   - tbl_Dim_LifeArea               (migration 30, PVR Table 11)
--   - tbl_Rule_HouseReferenceMatter  (migration 32, PVR Table 12 - graha lagna)
--   - tbl_Rule_Naisargika_Karakatwas (migration 086, ch. 8)
--
-- BasisCode on every row records how solid the citation is:
--   PVR_DIRECT          - an exact statement found in the book text
--                          (Venus/marriage cluster pg 83, the Dārakāraka/
--                          Putrakāraka correction also pg 83, Upapada
--                          Lagna for marriage, D-11 "Death and destruction"
--                          verbatim from Table 11).
--   PVR_CROSSVALIDATED   - the (karaka, house) pair independently matches
--                          data already seeded from the book (the
--                          naisargika grid or Table 12), even though this
--                          exact "specific matter" phrasing is the
--                          worksheet's own, not the book's.
--   PROJECT_SYNTHESIS    - no match either way; a reasoned extension of
--                          PVR's stated method (chapter 7.3: "we have to
--                          choose the meanings of houses that are
--                          relevant in that area of life") to a case the
--                          book does not spell out.
--
-- Correction applied before seeding (verified pg 83, verbatim): "We do
-- not take the 7th from DK for spouse, but DK himself shows spouse" -
-- chara karakas (DK, PK, ...) represent the person directly, unlike
-- naisargika karakas where a house IS counted from the karaka. The
-- worksheet's original "houses from DK" / "houses from PK" rows are
-- corrected to "DK/PK himself" accordingly.
--
-- SourceRefCode: SRC_PVR_INTEGRATED for PVR_DIRECT/PVR_CROSSVALIDATED
-- rows; SRC_IKIASTRRO_SYNTHESIS (new tbl_Dim_Source row, this migration)
-- for PROJECT_SYNTHESIS rows - a project-authored extension, not a book
-- citation.
--
-- Idempotent throughout. File is UTF-8 without BOM (diacritics in
-- Nyāya/Mokṣa/Dārakāraka/Putrakāraka) - apply with -f 65001 or sqlcmd
-- mis-decodes them (see migration 086's note).
-- Apply:  sqlcmd -S "localhost\SQLSERVER2025" -E -d ikiastrro -b -f 65001 -i db/087_seed_life_matter_reference.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- --- Batch 1: tbl_Dim_Source - register the project-synthesis source ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Dim_Source WHERE Code = 'SRC_IKIASTRRO_SYNTHESIS')
    INSERT dbo.tbl_Dim_Source (Code, Title, Author, Tradition, Notes)
    VALUES ('SRC_IKIASTRRO_SYNTHESIS', N'ikiastrro project synthesis', N'ikiastrro project (rammyps + Claude)', 'Project',
            N'Project-authored interpretive extension applying a cited source''s own stated method to cases the source does not spell out explicitly. Not a citation to any book''s text - flags rows that need eventual verification against a classical source.');
GO

-- --- Batch 2: tbl_Rule_LifeMatterReference ---
IF OBJECT_ID('dbo.tbl_Rule_LifeMatterReference', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_Rule_LifeMatterReference (
        Id                   INT IDENTITY(1,1) NOT NULL
                                 CONSTRAINT PK_Rule_LifeMatterReference PRIMARY KEY,
        RuleSetId            TINYINT      NOT NULL
                                 CONSTRAINT FK_Rule_LifeMatterReference_RuleSet FOREIGN KEY REFERENCES dbo.tbl_Rule_Sets (Id),
        CategoryCode         VARCHAR(30)  NOT NULL,
        CategoryName         NVARCHAR(60) NOT NULL,
        DisplayOrder         TINYINT      NOT NULL,
        MatterText           NVARCHAR(120) NOT NULL,
        PrimaryChartsText    VARCHAR(30)  NOT NULL,    -- e.g. 'D9', 'D6/D8' - preserved as given, compound charts not split
        PrimaryLifeAreaId    TINYINT      NULL
                                 CONSTRAINT FK_Rule_LifeMatterReference_LifeArea FOREIGN KEY REFERENCES dbo.tbl_Dim_LifeArea (Id),  -- NULL when PrimaryChartsText is compound/unmapped
        HouseFromLagnaText   VARCHAR(30)  NOT NULL,    -- e.g. '7th', '6th/8th', '5th from AL', 'Upapada Lagna'
        KarakaText           NVARCHAR(80) NOT NULL,    -- e.g. 'Venus', 'Mars and Rahu', 'Dārakāraka'
        NaisargikaGrahaId    TINYINT      NULL
                                 CONSTRAINT FK_Rule_LifeMatterReference_Graha FOREIGN KEY REFERENCES dbo.tbl_Planets (Id),  -- NULL when KarakaText is compound or a chara karaka
        CharaKarakaCode      VARCHAR(4)   NULL,        -- AK/AmK/BK/MK/PiK/PK/GK/DK, when KarakaText names a chara karaka instead of a fixed graha
        HouseFromKarakaText  NVARCHAR(160) NOT NULL,   -- e.g. '7th from Venus'; for chara karakas, 'DK himself shows spouse' (see header note)
        BasisCode            VARCHAR(20)  NOT NULL,    -- PVR_DIRECT / PVR_CROSSVALIDATED / PROJECT_SYNTHESIS
        CalculationNarrative NVARCHAR(400) NULL,       -- citation or rationale
        SourceRefCode        VARCHAR(40)  NULL,
        IsActive             BIT          NOT NULL CONSTRAINT DF_Rule_LifeMatterReference_IsActive DEFAULT 1,
        CONSTRAINT CK_Rule_LifeMatterReference_Category CHECK (CategoryCode IN (
            'SELF_HEALTH','WEALTH','EDUCATION','PROPERTY_COMFORTS','FAMILY_RELATIONSHIPS',
            'MARRIAGE_SPOUSE','CHILDREN','CAREER_STATUS','SPIRITUALITY','TROUBLE_LOSS')),
        CONSTRAINT CK_Rule_LifeMatterReference_Basis CHECK (BasisCode IN ('PVR_DIRECT','PVR_CROSSVALIDATED','PROJECT_SYNTHESIS')),
        CONSTRAINT CK_Rule_LifeMatterReference_Chara CHECK (CharaKarakaCode IS NULL OR CharaKarakaCode IN ('AK','AmK','BK','MK','PiK','PK','GK','DK')),
        CONSTRAINT CK_Rule_LifeMatterReference_Src   CHECK (SourceRefCode IS NULL OR SourceRefCode LIKE 'SRC[_]%'),
        CONSTRAINT UQ_Rule_LifeMatterReference UNIQUE (RuleSetId, CategoryCode, MatterText)
    );
    CREATE NONCLUSTERED INDEX IX_Rule_LifeMatterReference_LifeArea
        ON dbo.tbl_Rule_LifeMatterReference (PrimaryLifeAreaId);
    CREATE NONCLUSTERED INDEX IX_Rule_LifeMatterReference_Graha
        ON dbo.tbl_Rule_LifeMatterReference (NaisargikaGrahaId);
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_LifeMatterReference)
BEGIN
    ;WITH m (CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText, AreaCode,
             HouseFromLagnaText, KarakaText, SinglePlanet, CharaKaraka, HouseFromKarakaText,
             BasisCode, Narrative) AS (
        SELECT * FROM (VALUES

        -- Self, health and psychology
        ('SELF_HEALTH', N'Self, health and psychology', 1, N'Physical self and constitution', 'D1', 'PHYSICAL_EXISTENCE', '1st', N'Sun', 'Sun', NULL, N'1st from Sun', 'PVR_CROSSVALIDATED', CONVERT(NVARCHAR(400),NULL)),
        ('SELF_HEALTH', N'Self, health and psychology', 2, N'General health', 'D1', 'PHYSICAL_EXISTENCE', '1st', N'Sun', 'Sun', NULL, N'1st from Sun', 'PVR_CROSSVALIDATED', NULL),
        ('SELF_HEALTH', N'Self, health and psychology', 3, N'Illness', 'D6', 'HEALTH_TROUBLES', '6th', N'Mars', 'Mars', NULL, N'6th from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('SELF_HEALTH', N'Self, health and psychology', 4, N'Chronic illness / longevity vulnerability', 'D6/D8', NULL, '8th', N'Saturn', 'Saturn', NULL, N'8th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('SELF_HEALTH', N'Self, health and psychology', 5, N'Accidents', 'D6/D8', NULL, '6th/8th', N'Mars and Rahu', NULL, NULL, N'6th from Mars/Rahu', 'PVR_CROSSVALIDATED', N'Rahu/6th also matches its own naisargika karakatwa (accidents).'),
        ('SELF_HEALTH', N'Self, health and psychology', 6, N'Hospitalization', 'D6/D30', NULL, '12th', N'Saturn', 'Saturn', NULL, N'12th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('SELF_HEALTH', N'Self, health and psychology', 7, N'Mind', 'D1', 'PHYSICAL_EXISTENCE', '1st', N'Moon', 'Moon', NULL, N'Moon itself / 1st from Moon', 'PVR_CROSSVALIDATED', NULL),
        ('SELF_HEALTH', N'Self, health and psychology', 8, N'Peace of mind', 'D16, confirmed in D1', NULL, '4th', N'Moon', 'Moon', NULL, N'4th from Moon', 'PVR_CROSSVALIDATED', N'Matches naisargika karakatwa: Moon/4th (mother and peace of mind).'),
        ('SELF_HEALTH', N'Self, health and psychology', 9, N'Inherent strengths and weaknesses', 'D27', 'INNATE_NATURE', '1st', N'Lagna lord', NULL, NULL, N'1st from Lagna lord', 'PROJECT_SYNTHESIS', N'Lagna lord is chart-dependent, not a fixed naisargika/chara karaka; no book table entry to cross-check.'),
        ('SELF_HEALTH', N'Self, health and psychology', 10, N'Courage and persistence', 'D27, confirmed in D1', 'INNATE_NATURE', '3rd', N'Mars', 'Mars', NULL, N'3rd from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('SELF_HEALTH', N'Self, health and psychology', 11, N'Subconscious disturbances', 'D30', NULL, '1st/8th', N'Moon/Saturn', NULL, NULL, N'From the relevant karaka', 'PROJECT_SYNTHESIS', N'Compound karaka/house pair, not individually checkable against a book table.'),

        -- Wealth and financial matters
        ('WEALTH', N'Wealth and financial matters', 1, N'Overall financial condition', 'D2', 'WEALTH', '1st', N'Jupiter', 'Jupiter', NULL, N'1st from Jupiter', 'PROJECT_SYNTHESIS', N'Jupiter/1st is not in the naisargika grid or Table 12; a divisional-chart-lagna generalisation (chart''s own 1st house = general vitality of that area of life).'),
        ('WEALTH', N'Wealth and financial matters', 2, N'Accumulated wealth', 'D2', 'WEALTH', '2nd', N'Jupiter', 'Jupiter', NULL, N'2nd from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('WEALTH', N'Wealth and financial matters', 3, N'Family resources', 'D2', 'WEALTH', '2nd', N'Jupiter', 'Jupiter', NULL, N'2nd from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('WEALTH', N'Wealth and financial matters', 4, N'Speculation', 'D2', 'WEALTH', '5th', N'Mars', 'Mars', NULL, N'5th from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('WEALTH', N'Wealth and financial matters', 5, N'Loans and debt', 'D2/D6', NULL, '6th', N'Mars', 'Mars', NULL, N'6th from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('WEALTH', N'Wealth and financial matters', 6, N'Gains and income', 'D2', 'WEALTH', '11th', N'Jupiter', 'Jupiter', NULL, N'11th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('WEALTH', N'Wealth and financial matters', 7, N'Credits / receivables', 'D2', 'WEALTH', '11th', N'Mercury', 'Mercury', NULL, N'11th from Mercury', 'PVR_CROSSVALIDATED', NULL),
        ('WEALTH', N'Wealth and financial matters', 8, N'Expenditure and financial loss', 'D2', 'WEALTH', '12th', N'Saturn', 'Saturn', NULL, N'12th from Saturn', 'PVR_CROSSVALIDATED', NULL),

        -- Education and knowledge
        ('EDUCATION', N'Education and knowledge', 1, N'Overall learning', 'D24', 'EDUCATION', '1st', N'Mercury', 'Mercury', NULL, N'1st from Mercury', 'PROJECT_SYNTHESIS', NULL),
        ('EDUCATION', N'Education and knowledge', 2, N'Basic / formal education', 'D24', 'EDUCATION', '4th', N'Mercury', 'Mercury', NULL, N'4th from Mercury', 'PVR_CROSSVALIDATED', NULL),
        ('EDUCATION', N'Education and knowledge', 3, N'Traditional learning', 'D24', 'EDUCATION', '4th', N'Jupiter', 'Jupiter', NULL, N'4th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('EDUCATION', N'Education and knowledge', 4, N'Memory', 'D24', 'EDUCATION', '5th', N'Mercury', 'Mercury', NULL, N'5th from Mercury', 'PVR_CROSSVALIDATED', N'PVR (sec 7.3 discussion of paaka lagna): "the 5th house from paaka lagna shows memory the best" ties memory to the 5th-from-a-reference method.'),
        ('EDUCATION', N'Education and knowledge', 5, N'Intelligence', 'D24', 'EDUCATION', '5th', N'Jupiter', 'Jupiter', NULL, N'5th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('EDUCATION', N'Education and knowledge', 6, N'Scholarship', 'D24', 'EDUCATION', '5th', N'Mercury/Jupiter', NULL, NULL, N'5th from Mercury/Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('EDUCATION', N'Education and knowledge', 7, N'Nyāya or logical scholarship', 'D24', 'EDUCATION', '5th', N'Mars', 'Mars', NULL, N'5th from Mars', 'PVR_CROSSVALIDATED', N'Matches naisargika karakatwa verbatim: Mars/5th (Nyāya scholarship and speculation).'),
        ('EDUCATION', N'Education and knowledge', 8, N'Students', 'D24', 'EDUCATION', '5th', N'Mercury', 'Mercury', NULL, N'5th from Mercury', 'PVR_CROSSVALIDATED', NULL),
        ('EDUCATION', N'Education and knowledge', 9, N'Writing and communication', 'D24', 'EDUCATION', '3rd', N'Mars/Mercury', NULL, NULL, N'3rd from Mars; 2nd from Mercury for speech', 'PVR_CROSSVALIDATED', NULL),
        ('EDUCATION', N'Education and knowledge', 10, N'Teachers and gurus', 'D24', 'EDUCATION', '9th', N'Jupiter', 'Jupiter', NULL, N'9th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('EDUCATION', N'Education and knowledge', 11, N'Higher education', 'D24', 'EDUCATION', '9th', N'Jupiter', 'Jupiter', NULL, N'9th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('EDUCATION', N'Education and knowledge', 12, N'Educational achievement', 'D24', 'EDUCATION', '10th/11th', N'Mercury', 'Mercury', NULL, N'10th/11th from Mercury', 'PVR_CROSSVALIDATED', NULL),
        ('EDUCATION', N'Education and knowledge', 13, N'Academic recognition', 'D24', 'EDUCATION', '5th from AL', N'Sun/Mercury', NULL, NULL, N'5th from Sun or Mercury', 'PVR_DIRECT', N'Distinguishes perceived achievement (5th from Sun/AL) from actual ability (5th from Mercury/Jupiter/Lagna), per PVR''s discussion of paaka lagna/self references.'),

        -- Property, residence, vehicles and comforts
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 1, N'House and immovable property', 'D4', 'PROPERTY_FORTUNE', '4th', N'Mars', 'Mars', NULL, N'4th from Mars', 'PVR_CROSSVALIDATED', N'PVR''s own worked example (sec 7.3, pg 69-70): house and immovable property is read from D-4.'),
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 2, N'General residence', 'D4', 'PROPERTY_FORTUNE', '4th', N'Mars', 'Mars', NULL, N'4th from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 3, N'Fortune connected with property', 'D4', 'PROPERTY_FORTUNE', '9th', N'Jupiter', 'Jupiter', NULL, N'9th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 4, N'Foreign residence', 'D4', 'PROPERTY_FORTUNE', '9th/12th', N'Rahu/Ketu', NULL, NULL, N'9th from Rahu/Ketu', 'PVR_CROSSVALIDATED', NULL),
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 5, N'Expenditure on property', 'D4', 'PROPERTY_FORTUNE', '3rd', N'Saturn/Mars', NULL, NULL, N'12th from property reference', 'PVR_CROSSVALIDATED', N'Matches PVR''s stated derived-house technique: 12th from a house = expenditure on that house''s matters (cf. H03_VEHICLE_HOUSE_EXPENSE, "12th from the 4th").'),
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 6, N'Vehicles', 'D16', 'VEHICLES_COMFORTS', '4th', N'Venus', 'Venus', NULL, N'4th from Venus', 'PVR_CROSSVALIDATED', N'PVR''s own worked example: vehicle is read from D-16.'),
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 7, N'Pleasure from vehicles', 'D16', 'VEHICLES_COMFORTS', '4th', N'Venus', 'Venus', NULL, N'4th from Venus', 'PVR_CROSSVALIDATED', NULL),
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 8, N'General comforts', 'D16', 'VEHICLES_COMFORTS', '4th', N'Moon/Venus', NULL, NULL, N'4th from Moon/Venus', 'PVR_CROSSVALIDATED', NULL),
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 9, N'Loss of comfort', 'D16', 'VEHICLES_COMFORTS', '12th', N'Saturn', 'Saturn', NULL, N'12th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('PROPERTY_COMFORTS', N'Property, residence, vehicles and comforts', 10, N'Expenditure on vehicles', 'D16', 'VEHICLES_COMFORTS', '3rd', N'Venus/Saturn', NULL, NULL, N'12th from the vehicle house', 'PVR_CROSSVALIDATED', N'Same 12th-from-X expenditure technique as property; 3rd = 12th from 4th.'),

        -- Family and relationships
        ('FAMILY_RELATIONSHIPS', N'Family and relationships', 1, N'Family', 'D2', 'WEALTH', '2nd', N'Jupiter', 'Jupiter', NULL, N'2nd from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('FAMILY_RELATIONSHIPS', N'Family and relationships', 2, N'Mother', 'D12', 'PARENTS', '4th', N'Moon', 'Moon', NULL, N'4th from Moon', 'PVR_CROSSVALIDATED', N'Matches PVR Table 12 (sec 7.3.9): Chandra Lagna reads house 4 (tbl_Rule_HouseReferenceMatter).'),
        ('FAMILY_RELATIONSHIPS', N'Family and relationships', 3, N'Father', 'D12', 'PARENTS', '9th', N'Sun', 'Sun', NULL, N'9th from Sun', 'PVR_CROSSVALIDATED', N'Matches PVR Table 12: Surya Lagna reads house 9.'),
        ('FAMILY_RELATIONSHIPS', N'Family and relationships', 4, N'Younger siblings', 'D3', 'SIBLINGS', '3rd', N'Mars', 'Mars', NULL, N'3rd from Mars', 'PVR_CROSSVALIDATED', N'Matches PVR Table 12: Mars-lagna reads house 3.'),
        ('FAMILY_RELATIONSHIPS', N'Family and relationships', 5, N'Elder siblings', 'D3', 'SIBLINGS', '11th', N'Jupiter', 'Jupiter', NULL, N'11th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('FAMILY_RELATIONSHIPS', N'Family and relationships', 6, N'Friends', 'D1/D9', NULL, '11th', N'Moon', 'Moon', NULL, N'11th from Moon', 'PVR_CROSSVALIDATED', N'Matches PVR Table 12: Chandra Lagna also reads house 11.'),
        ('FAMILY_RELATIONSHIPS', N'Family and relationships', 7, N'Followers', 'D5/D10', NULL, '5th', N'Saturn', 'Saturn', NULL, N'5th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('FAMILY_RELATIONSHIPS', N'Family and relationships', 8, N'Servants / subordinates', 'D10', 'CAREER', '6th', N'Saturn', 'Saturn', NULL, N'6th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('FAMILY_RELATIONSHIPS', N'Family and relationships', 9, N'Boss or authority figure', 'D10', 'CAREER', '9th', N'Sun', 'Sun', NULL, N'9th from Sun', 'PVR_CROSSVALIDATED', NULL),

        -- Marriage and spouse
        ('MARRIAGE_SPOUSE', N'Marriage and spouse', 1, N'Marriage', 'D9', 'MARRIAGE_SPOUSE', '7th', N'Venus', 'Venus', NULL, N'7th from Venus', 'PVR_DIRECT', N'PVR pg 83, verbatim: "the 7th from Venus (and not Venus himself) shows husband", used for both male and female charts.'),
        ('MARRIAGE_SPOUSE', N'Marriage and spouse', 2, N'Spouse', 'D9', 'MARRIAGE_SPOUSE', '7th', N'Venus', 'Venus', NULL, N'7th from Venus', 'PVR_DIRECT', N'Same pg 83 citation as Marriage.'),
        ('MARRIAGE_SPOUSE', N'Marriage and spouse', 3, N'Marital happiness', 'D9', 'MARRIAGE_SPOUSE', '7th', N'Venus', 'Venus', NULL, N'7th from Venus', 'PVR_DIRECT', N'Same pg 83 citation.'),
        ('MARRIAGE_SPOUSE', N'Marriage and spouse', 4, N'Interaction with others', 'D9', 'MARRIAGE_SPOUSE', '7th', N'Venus', 'Venus', NULL, N'7th from Venus', 'PVR_DIRECT', N'D-9''s own Table 11 description explicitly includes "interaction with other people".'),
        ('MARRIAGE_SPOUSE', N'Marriage and spouse', 5, N'Sexual / bed pleasures', 'D9/D16', NULL, '12th', N'Venus', 'Venus', NULL, N'12th from Venus', 'PVR_CROSSVALIDATED', NULL),
        ('MARRIAGE_SPOUSE', N'Marriage and spouse', 6, N'Public manifestation of marriage', 'D9/D1', NULL, 'Upapada Lagna', N'Venus', 'Venus', NULL, N'Connections with UL', 'PVR_DIRECT', N'Upapada Lagna (arudha of the 7th) is PVR''s own stated reference for the visible/social side of marriage, used throughout the book for marriage timing.'),
        ('MARRIAGE_SPOUSE', N'Marriage and spouse', 7, N'Individual spouse-role', 'D9', 'MARRIAGE_SPOUSE', '7th', N'Dārakāraka', NULL, 'DK', N'DK himself shows spouse (not a house counted from DK)', 'PVR_DIRECT', N'CORRECTED per PVR pg 83, verbatim: "We do not take the 7th from DK for spouse, but DK himself shows spouse" - chara karakas represent the person directly, unlike naisargika karakas. Worksheet originally said "houses from DK".'),

        -- Children
        ('CHILDREN', N'Children', 1, N'Children / progeny', 'D7', 'CHILDREN', '5th', N'Jupiter', 'Jupiter', NULL, N'5th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('CHILDREN', N'Children', 2, N'Particular child-role', 'D7', 'CHILDREN', '5th', N'Putrakāraka', NULL, 'PK', N'PK himself shows the child (not a house counted from PK)', 'PVR_DIRECT', N'CORRECTED: generalises PVR''s explicit pg 83 rule for chara karakas (stated for DK, "similar to sthira karakas") - the person is shown by the karaka itself. Worksheet originally said "examine PK and houses from it".'),
        ('CHILDREN', N'Children', 3, N'Conception / union', 'D7', 'CHILDREN', '7th', N'Venus', 'Venus', NULL, N'7th from Venus', 'PVR_CROSSVALIDATED', NULL),
        ('CHILDREN', N'Children', 4, N'Fortune of children', 'D7', 'CHILDREN', '9th', N'Jupiter', 'Jupiter', NULL, N'9th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('CHILDREN', N'Children', 5, N'Gains or fulfilment through children', 'D7', 'CHILDREN', '11th', N'Jupiter', 'Jupiter', NULL, N'11th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('CHILDREN', N'Children', 6, N'Grandchildren', 'D7', 'CHILDREN', '9th from 5th', N'Jupiter', 'Jupiter', NULL, N'Derive from child''s house', 'PROJECT_SYNTHESIS', N'Applies PVR''s "houses from houses" derived-reference technique (sec 7.2) rather than a book-stated grandchildren rule specifically.'),

        -- Career, status and authority
        ('CAREER_STATUS', N'Career, status and authority', 1, N'Overall career direction', 'D10', 'CAREER', '1st', N'Sun/Mercury', NULL, NULL, N'1st from relevant karaka', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 2, N'Initiative at work', 'D10', 'CAREER', '3rd', N'Mars', 'Mars', NULL, N'3rd from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 3, N'Employment and service', 'D10', 'CAREER', '6th', N'Saturn', 'Saturn', NULL, N'6th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 4, N'Professional competition', 'D10', 'CAREER', '6th', N'Mars', 'Mars', NULL, N'6th from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 5, N'Business partnership', 'D10', 'CAREER', '7th', N'Venus', 'Venus', NULL, N'7th from Venus', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 6, N'Boss and mentor', 'D10', 'CAREER', '9th', N'Sun/Jupiter', NULL, NULL, N'9th from Sun/Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 7, N'Career and actions', 'D10', 'CAREER', '10th', N'Sun/Mercury', NULL, NULL, N'10th from Sun/Mercury', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 8, N'Achievements and honours', 'D10', 'CAREER', '10th', N'Mercury', 'Mercury', NULL, N'10th from Mercury', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 9, N'Authority and power', 'D5/D10', NULL, '5th/10th', N'Sun', 'Sun', NULL, N'5th/10th from Sun', 'PVR_CROSSVALIDATED', N'PVR also names Ghati Lagna for this, verbatim: "Ghati lagna shows self, from the point of view of power, authority and fame."'),
        ('CAREER_STATUS', N'Career, status and authority', 10, N'Fame', 'D5', 'FAME_POWER', '5th', N'Sun', 'Sun', NULL, N'5th from Sun', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 11, N'Professional gains', 'D10', 'CAREER', '11th', N'Jupiter', 'Jupiter', NULL, N'11th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('CAREER_STATUS', N'Career, status and authority', 12, N'Professional loss / retirement', 'D10', 'CAREER', '12th', N'Saturn', 'Saturn', NULL, N'12th from Saturn', 'PVR_CROSSVALIDATED', NULL),

        -- Spirituality and religion
        ('SPIRITUALITY', N'Spirituality and religion', 1, N'Overall spiritual life', 'D20', 'RELIGION_SPIRITUALITY', '1st', N'Jupiter/Ketu', NULL, NULL, N'From the appropriate karaka', 'PROJECT_SYNTHESIS', NULL),
        ('SPIRITUALITY', N'Spirituality and religion', 2, N'Devotion and mantra', 'D20', 'RELIGION_SPIRITUALITY', '5th', N'Jupiter', 'Jupiter', NULL, N'5th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('SPIRITUALITY', N'Spirituality and religion', 3, N'Teacher / guru', 'D20', 'RELIGION_SPIRITUALITY', '9th', N'Jupiter', 'Jupiter', NULL, N'9th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('SPIRITUALITY', N'Spirituality and religion', 4, N'Religion and fortune', 'D20', 'RELIGION_SPIRITUALITY', '9th', N'Jupiter', 'Jupiter', NULL, N'9th from Jupiter', 'PVR_CROSSVALIDATED', NULL),
        ('SPIRITUALITY', N'Spirituality and religion', 5, N'Pilgrimage', 'D20/D4', NULL, '9th', N'Rahu/Ketu', NULL, NULL, N'9th from Rahu/Ketu', 'PVR_CROSSVALIDATED', N'Matches naisargika karakatwa: Rahu/9th and Ketu/9th (pilgrimage and foreign travel).'),
        ('SPIRITUALITY', N'Spirituality and religion', 6, N'Foreign spiritual journey', 'D20/D4', NULL, '9th/12th', N'Rahu/Ketu', NULL, NULL, N'9th/12th from nodes', 'PVR_CROSSVALIDATED', NULL),
        ('SPIRITUALITY', N'Spirituality and religion', 7, N'Renunciation', 'D20', 'RELIGION_SPIRITUALITY', '12th', N'Saturn/Ketu', NULL, NULL, N'12th from Saturn/Ketu', 'PVR_CROSSVALIDATED', NULL),
        ('SPIRITUALITY', N'Spirituality and religion', 8, N'Mokṣa', 'D20', 'RELIGION_SPIRITUALITY', '12th', N'Ketu', 'Ketu', NULL, N'12th from Ketu', 'PVR_CROSSVALIDATED', N'Matches naisargika karakatwa: Ketu/12th (Moksha).'),

        -- Trouble, conflict and loss
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 1, N'Enemies', 'D6/D8', NULL, '6th', N'Mars', 'Mars', NULL, N'6th from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 2, N'Disease', 'D6/D30', NULL, '6th', N'Mars', 'Mars', NULL, N'6th from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 3, N'Debt', 'D2/D6', NULL, '6th', N'Mars', 'Mars', NULL, N'6th from Mars', 'PVR_CROSSVALIDATED', NULL),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 4, N'Accidents', 'D6/D8', NULL, '6th/8th', N'Mars/Rahu', NULL, NULL, N'6th from Mars/Rahu', 'PVR_CROSSVALIDATED', NULL),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 5, N'Litigation', 'D8', NULL, '6th/8th', N'Mars/Rahu', NULL, NULL, N'Corresponding houses', 'PROJECT_SYNTHESIS', N'No fixed house-from-karaka given; left deliberately vague in the source worksheet.'),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 6, N'Sudden trouble', 'D8', 'SUDDEN_TROUBLES', '8th', N'Saturn/Rahu', NULL, NULL, N'8th from Saturn/Rahu', 'PVR_CROSSVALIDATED', NULL),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 7, N'Longevity', 'D1/D8', NULL, '8th', N'Saturn', 'Saturn', NULL, N'8th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 8, N'General troubles', 'D8/D30', NULL, '8th', N'Saturn', 'Saturn', NULL, N'8th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 9, N'Loss', 'Relevant varga', NULL, '12th', N'Saturn', 'Saturn', NULL, N'12th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 10, N'Hospitalization / confinement', 'D6/D30', NULL, '12th', N'Saturn', 'Saturn', NULL, N'12th from Saturn', 'PVR_CROSSVALIDATED', NULL),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 11, N'Destruction / death', 'D11', 'DEATH_DESTRUCTION', '8th', N'Saturn', 'Saturn', NULL, N'8th from Saturn', 'PVR_DIRECT', N'D-11''s Table 11 description is verbatim "Death and destruction".'),
        ('TROUBLE_LOSS', N'Trouble, conflict and loss', 12, N'Occult knowledge', 'D8/D20', NULL, '8th', N'Rahu/Ketu', NULL, NULL, N'8th from Rahu/Ketu', 'PVR_CROSSVALIDATED', N'Matches naisargika karakatwa: Rahu/8th and Ketu/8th (occult knowledge).')

        ) v (CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText, AreaCode,
             HouseFromLagnaText, KarakaText, SinglePlanet, CharaKaraka, HouseFromKarakaText,
             BasisCode, Narrative)
    )
    INSERT dbo.tbl_Rule_LifeMatterReference
        (RuleSetId, CategoryCode, CategoryName, DisplayOrder, MatterText, PrimaryChartsText,
         PrimaryLifeAreaId, HouseFromLagnaText, KarakaText, NaisargikaGrahaId, CharaKarakaCode,
         HouseFromKarakaText, BasisCode, CalculationNarrative, SourceRefCode, IsActive)
    SELECT 1, m.CategoryCode, m.CategoryName, m.DisplayOrder, m.MatterText, m.PrimaryChartsText,
           la.Id, m.HouseFromLagnaText, m.KarakaText, p.Id, m.CharaKaraka,
           m.HouseFromKarakaText, m.BasisCode, m.Narrative,
           CASE WHEN m.BasisCode = 'PROJECT_SYNTHESIS' THEN 'SRC_IKIASTRRO_SYNTHESIS' ELSE 'SRC_PVR_INTEGRATED' END,
           1
    FROM m
    LEFT JOIN dbo.tbl_Dim_LifeArea la ON la.AreaCode = m.AreaCode
    LEFT JOIN dbo.tbl_Planets p ON p.PlanetName = m.SinglePlanet;
END
GO

-- --- Batch 3: tbl_Rule_Catalog row ---
IF NOT EXISTS (SELECT 1 FROM dbo.tbl_Rule_Catalog WHERE RuleTableName = 'tbl_Rule_LifeMatterReference')
    INSERT dbo.tbl_Rule_Catalog (RuleTableName, EngineCode, MethodCodes, Purpose, IntroducedIn)
    VALUES ('tbl_Rule_LifeMatterReference', 'HOUSE', 'MAP_LOOKUP',
            'Bridges PVR areas-of-life (Table 11), house significations (sec 7.2), Table 12 graha-lagna houses, and naisargika/chara karaka into one practical reference: for each of 96 specific interpretive matters (10 categories), which divisional chart, house, karaka, and house-from-karaka to read. BasisCode marks PVR_DIRECT / PVR_CROSSVALIDATED / PROJECT_SYNTHESIS provenance per row.',
            '087_seed_life_matter_reference.sql');
GO

-- --- Batch 4: ledger + summary ---
INSERT dbo.SchemaMigrations (ScriptName, Note)
SELECT '087_seed_life_matter_reference.sql',
       'tbl_Rule_LifeMatterReference (96 rows, 10 categories) bridging Table 11 + sec 7.2 + Table 12 + naisargika karaka; SRC_IKIASTRRO_SYNTHESIS registered in tbl_Dim_Source; 1 tbl_Rule_Catalog row'
WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '087_seed_life_matter_reference.sql');
GO

DECLARE @rows INT       = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference);
DECLARE @direct INT     = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE BasisCode = 'PVR_DIRECT');
DECLARE @crossval INT   = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE BasisCode = 'PVR_CROSSVALIDATED');
DECLARE @synth INT      = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE BasisCode = 'PROJECT_SYNTHESIS');
DECLARE @areaNull INT   = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE PrimaryLifeAreaId IS NULL);
DECLARE @grahaNull INT  = (SELECT COUNT(*) FROM dbo.tbl_Rule_LifeMatterReference WHERE NaisargikaGrahaId IS NULL AND CharaKarakaCode IS NULL);
PRINT '087 applied: ' + CAST(@rows AS VARCHAR(10)) + ' rows (expect 96); basis PVR_DIRECT=' + CAST(@direct AS VARCHAR(10))
    + ' PVR_CROSSVALIDATED=' + CAST(@crossval AS VARCHAR(10)) + ' PROJECT_SYNTHESIS=' + CAST(@synth AS VARCHAR(10))
    + '; ' + CAST(@areaNull AS VARCHAR(10)) + ' rows with compound/unmapped chart (LifeAreaId NULL), '
    + CAST(@grahaNull AS VARCHAR(10)) + ' rows with compound/no single karaka (both graha+chara-karaka NULL).';
GO
