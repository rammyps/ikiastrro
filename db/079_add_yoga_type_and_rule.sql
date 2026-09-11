-- =====================================================================
-- 079 — Yoga Type (reference point) + Rule (short calculation form).
--
-- Key Inference round 2 (docs/ui/components/key-inference.md, yoga.md)
-- added two columns to the round-2 Yoga table: Type (which reference
-- point the yoga is judged from — Sun / Moon / Lagna / a combination of
-- points) and Rule (a one-line classical formation, e.g. "7th lord in
-- 5th"). Both were mocked up hard-coded (docs/artifacts/ui/v2-mockup/
-- key-inference-v2.html) pending a DB column — this migration adds them.
--
-- tbl_Rule_Yoga already exists (migration 18) but has been empty since
-- creation: vw_ChartYogaEvaluations reads tbl_Fact_YogaInputEvaluations
-- directly and never joined it. FormationFamilyCode (migration 48) is
-- also unused until now — this migration repurposes it as the Type
-- axis (constrained to SUN/MOON/LAGNA/COMBINATION) and adds one new
-- column, ShortFormationRule, for Rule. Both are source-independent
-- properties of the yoga concept, not of one source's variant row, so
-- one row per (RuleSetId, YogaCode) is enough even though a YogaCode
-- may carry many SourceVariantCode rows in the fact table.
--
-- Coverage: every value below is transcribed from the actual evaluator
-- predicate in src/Ikiastrro.Core/Engines/Yoga/*.cs — never freehand
-- astrological recall — so it matches what the engine actually checks.
-- 223 distinct YogaCodes are evaluated for a chart; 146 have a real
-- coded predicate somewhere and get a row here. The other 77 are
-- deliberately left with no row (Type/Rule read NULL through the view):
--   - 61 are the Raman 201-300 tail (RamanFinalHundredCatalog.cs) —
--     catalogued by name/locator only, predicate not yet implemented.
--   - 14 are RamanYogaBatchEightEvaluator's explicit "Unsupported" set
--     (151-200 numbers with no coded predicate under that YogaCode).
--   - YOGA_VIDYA / YOGA_ARISHTA (HoroscopeExplorerGapYogaEvaluator) are
--     NOT_EVALUATED by design — no natal predicate exists to transcribe.
-- Mirrors the project's "deliberately NULL pending a cited source"
-- convention rather than fabricating a rule with nothing to ground it.
--
-- A few YogaCodes cover more than one classical form under one code
-- (YOGA_DARIDRA, YOGA_DHANA, YOGA_CHAPA, YOGA_DEHASTHOULYA, ...); the
-- Rule text there is a short summary of the family, not one exhaustive
-- enumeration — see docs/ui/components/yoga.md for the caveat.
-- =====================================================================
USE [ikiastrro];
GO
SET QUOTED_IDENTIFIER ON; -- required for the filtered unique index below
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '079_add_yoga_type_and_rule.sql')
BEGIN
    IF COL_LENGTH('dbo.tbl_Rule_Yoga', 'ShortFormationRule') IS NULL
        ALTER TABLE dbo.tbl_Rule_Yoga ADD ShortFormationRule NVARCHAR(300) NULL;

    IF OBJECT_ID('dbo.CK_Rule_Yoga_FormationFamily', 'C') IS NULL
        EXEC(N'ALTER TABLE dbo.tbl_Rule_Yoga ADD CONSTRAINT CK_Rule_Yoga_FormationFamily CHECK
            (FormationFamilyCode IS NULL OR FormationFamilyCode IN (''SUN'',''MOON'',''LAGNA'',''COMBINATION''));');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Rule_Yoga_RuleSet_YogaCode' AND object_id = OBJECT_ID('dbo.tbl_Rule_Yoga'))
        CREATE UNIQUE INDEX UQ_Rule_Yoga_RuleSet_YogaCode ON dbo.tbl_Rule_Yoga (RuleSetId, YogaCode)
            WHERE SourceRefCode IS NULL; -- one canonical (source-independent) definition row per YogaCode
END
GO

-- Separate batch: the seed INSERT below references ShortFormationRule, which the
-- ALTER TABLE above only creates once that batch has fully executed.
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '079_add_yoga_type_and_rule.sql')
BEGIN
    ;WITH YogaDefinitions (YogaCode, FormationFamilyCode, ShortFormationRule) AS
    (
        SELECT * FROM (VALUES
        ('YOGA_ADHI', 'MOON', 'Jupiter, Mercury or Venus in the 6th, 7th or 8th from Moon'),
        ('YOGA_AMALA', 'LAGNA', 'Only benefics occupy the 10th from Lagna or Moon'),
        ('YOGA_AMSAVATARA', 'LAGNA', 'Movable Lagna; Venus, Jupiter & Saturn all in kendras, Saturn exalted'),
        ('YOGA_ANAPHA', 'MOON', 'A planet (not Sun) in the 12th from Moon'),
        ('YOGA_ANDHA', 'COMBINATION', 'Mercury with Moon in the 2nd, or Lagna/2nd lord conjunct Sun with Lagna lord in the 2nd'),
        ('YOGA_ANIVAHUPPU', 'COMBINATION', 'One of 9 grahas alone in its sign; the other 8 split 4-and-4 across its two flanking semicircles'),
        ('YOGA_ARDHA_CHANDRA', 'COMBINATION', 'All 7 classical grahas fill one 7-house Panapara/Apoklima span'),
        ('YOGA_ASATYAVADI', 'COMBINATION', '2nd lord''s dispositor is Mars or Saturn, with a malefic in a kendra/trikona'),
        ('YOGA_AYATNA_DHANA_LABHA', 'COMBINATION', 'Lagna lord and 2nd lord exchange houses'),
        ('YOGA_AYATNA_GRIHA_PRAPTA', 'COMBINATION', 'Lagna lord in Lagna/4th conjunct 7th lord, benefic-influenced (or 9th lord in kendra with 4th lord strong)'),
        ('YOGA_BAHUDRAVYARJANA', 'COMBINATION', 'Lagna, 2nd & 11th lords in a 3-way mutual house exchange'),
        ('YOGA_BANDHUBHISTHYAKTHA', 'COMBINATION', '4th lord joined by a malefic, or in enmity/debilitation'),
        ('YOGA_BANDHU_PUJYA', 'COMBINATION', '4th lord is a natural benefic influenced by another, Mercury in Lagna (or Jupiter linked to the 4th lord)'),
        ('YOGA_BHADRA', 'LAGNA', 'Mercury own/exalted in a kendra'),
        ('YOGA_BHARATHI', 'COMBINATION', 'A 2nd/5th/11th lord''s navamsa dispositor is exalted in D1, sharing the 9th lord''s sign'),
        ('YOGA_BHASKARA', 'COMBINATION', 'Sun 2nd from Mercury, Mercury 11th from Moon, Moon in trine to Jupiter'),
        ('YOGA_BHERI', 'LAGNA', 'Lagna lord, Venus & Jupiter in a mutual kendra chain, 9th lord strong'),
        ('YOGA_BRAHMA', 'COMBINATION', '9th lord-Jupiter & 11th lord-Venus each in mutual kendra, plus Lagna/10th lord-Mercury in kendra'),
        ('YOGA_BUDHA', 'COMBINATION', 'Jupiter in Lagna, Moon in kendra with Rahu 2nd & Sun 3rd from it, Mars conjunct Sun'),
        ('YOGA_BUDHA_ADITYA', 'SUN', 'Sun and Mercury conjunct, more than 10 degrees apart'),
        ('YOGA_CHANDIKA', 'COMBINATION', 'Fixed Lagna; 6th & 9th lords'' navamsa dispositors both join Sun in D1, 6th lord aspects Lagna'),
        ('YOGA_CHANDRA', 'COMBINATION', 'All 7 classical grahas occupy only the 6 odd houses (1,3,5,7,9,11)'),
        ('YOGA_CHANDRA_MANGALA', 'MOON', 'Moon and Mars conjunct (same sign)'),
        ('YOGA_CHAPA', 'COMBINATION', 'Lagna lord exalted with 4th/10th lords in mutual exchange (combination 30); or all 7 grahas fill houses 10-4 (Nabhasa combination 78)'),
        ('YOGA_CHATUSSAGARA', 'LAGNA', 'All 4 kendras (1,4,7,10) occupied'),
        ('YOGA_CHHATRA', 'COMBINATION', 'All 7 classical grahas fill houses 7-1'),
        ('YOGA_DAMNI', 'COMBINATION', 'The 7 classical grahas occupy exactly 6 signs, with no other Nabhasa yoga present'),
        ('YOGA_DANDA', 'COMBINATION', 'All 7 classical grahas fill houses 10-1'),
        ('YOGA_DARIDRA', 'COMBINATION', 'Multiple classical forms - a dusthana lord (6th/8th/11th/12th) afflicted, exchanged or joined with another dusthana lord, aspected by malefics'),
        ('YOGA_DEHAKASHTA', 'LAGNA', 'Lagna lord in the 8th, or joined by a natural malefic'),
        ('YOGA_DEHAPUSHTI', 'LAGNA', 'Lagna lord in a movable sign, influenced by a natural benefic'),
        ('YOGA_DEHASTHOULYA', 'COMBINATION', 'Lagna lord (or Jupiter, or Lagna itself) linked to watery signs; or Lagna lord is Moon/Venus'),
        ('YOGA_DEVENDRA', 'COMBINATION', 'Fixed Lagna; Lagna/11th and 2nd/10th lords in mutual exchange, all four strong'),
        ('YOGA_DHANA', 'COMBINATION', 'Multiple classical forms - the 5th lord conjunct a 5th-house benefic with 11th-house support, or Lagna/2nd-lord strength and exchange'),
        ('YOGA_DURADHARA', 'MOON', 'Planets in both the 2nd and 12th from Moon (Sun excluded)'),
        ('YOGA_DURMUKHA', 'COMBINATION', 'A malefic occupies the 2nd, joined by another malefic or a debilitated 2nd lord'),
        ('YOGA_DURYOGA', 'LAGNA', '10th lord in a dusthana (6th, 8th or 12th)'),
        ('YOGA_DWADASA_SAHODARA', 'COMBINATION', '3rd lord in a kendra; Mars exalted, trine its own dispositor and conjunct Jupiter'),
        ('YOGA_GADA', 'COMBINATION', 'All 7 classical grahas confined to two adjacent kendras'),
        ('YOGA_GAJA', 'COMBINATION', '7th lord in the 11th conjunct Moon, aspected by the 11th lord'),
        ('YOGA_GAJAKESARI', 'MOON', 'Jupiter in a kendra (1,4,7,10) from Moon'),
        ('YOGA_GANDHARVA', 'COMBINATION', '10th lord in 3rd/7th/11th; Lagna lord joined/aspected by Jupiter; Sun strong; Moon in 9th'),
        ('YOGA_GARUDA', 'COMBINATION', 'Day birth, waxing Moon; Moon''s navamsa dispositor exalted in D1'),
        ('YOGA_GAURI', 'COMBINATION', '10th lord''s navamsa dispositor exalted in the D1 10th, sharing the Lagna lord''s sign'),
        ('YOGA_GO', 'LAGNA', 'Jupiter in Moolatrikona conjunct the 2nd lord; Lagna lord exalted'),
        ('YOGA_GOLA', 'COMBINATION', 'Full Moon in the 9th joined by Jupiter & Venus; Mercury in the D9 Lagna'),
        ('YOGA_GOLA_SANKHYA', 'COMBINATION', 'The 7 classical grahas occupy exactly 1 sign, with no other Nabhasa yoga present'),
        ('YOGA_GRIHANASA', 'COMBINATION', '4th lord in the 12th, influenced by a malefic'),
        ('YOGA_HALA', 'COMBINATION', 'All 7 classical grahas confined to one trinal triplet of houses'),
        ('YOGA_HAMSA', 'LAGNA', 'Jupiter own/exalted in a kendra'),
        ('YOGA_HARIHARA_BRAHMA', 'COMBINATION', 'One of three benefic-influence chains anchored on the 2nd, 7th or Lagna lord'),
        ('YOGA_HARSHA', 'LAGNA', '6th lord occupies the 6th (own house)'),
        ('YOGA_INDRA', 'COMBINATION', '5th & 11th lords exchange houses, with Moon in the 5th'),
        ('YOGA_ISHU', 'COMBINATION', 'All 7 classical grahas fill houses 4-7'),
        ('YOGA_JADA', 'COMBINATION', '2nd lord in the 10th joined by a malefic; or Sun and Gulika both occupy the 2nd'),
        ('YOGA_JAYA', 'COMBINATION', '6th lord debilitated while 10th lord is deeply exalted'),
        ('YOGA_KAHALA', 'COMBINATION', '4th & 9th lords in mutual kendra with a strong Lagna lord; or a strong 4th lord shares/aspects the 10th lord''s sign'),
        ('YOGA_KALANIDHI', 'COMBINATION', 'Jupiter in the 2nd or 5th in a dual sign, joined or aspected by Mercury or Venus'),
        ('YOGA_KAMALA', 'COMBINATION', 'All 7 classical grahas confined to the 4 kendras'),
        ('YOGA_KEDARA', 'COMBINATION', 'The 7 classical grahas occupy exactly 4 signs, with no other Nabhasa yoga present'),
        ('YOGA_KEMADRUMA', 'MOON', 'No planet in the 2nd or 12th from Moon (Sun excluded)'),
        ('YOGA_KRISANGA', 'LAGNA', 'Lagna lord (or its dispositor) sits in a dry sign or is ruled by a dry graha'),
        ('YOGA_KULAVARDHANA', 'COMBINATION', 'A natural benefic in the 5th from each of Lagna, Sun and Moon'),
        ('YOGA_KURMA', 'COMBINATION', 'All 4 natural benefics favourably placed in houses 5/6/7 (rasi) or 1/3/11 (navamsa-qualified)'),
        ('YOGA_KUSUMA', 'COMBINATION', 'Jupiter in Lagna, Moon in the 7th, 8th from Sun (or Venus in a fixed kendra with a debilitated Moon and Sun in the 10th)'),
        ('YOGA_KUTA', 'COMBINATION', 'All 7 classical grahas fill houses 4-10'),
        ('YOGA_LAKSHMI', 'LAGNA', 'Lagna lord and 9th lord both strong, 9th lord in a kendra or trikona'),
        ('YOGA_MAHABHAGYA', 'COMBINATION', 'Male+day birth: Sun, Moon & Lagna all in odd signs; female+night: all in even signs'),
        ('YOGA_MAKUTA', 'COMBINATION', '9th lord trine Jupiter; a benefic 9th from Jupiter; Saturn in the 10th'),
        ('YOGA_MALAVYA', 'LAGNA', 'Venus own/exalted in a kendra'),
        ('YOGA_MALIKA_BHAGYA', 'LAGNA', '7 classical grahas fill houses 9-3 contiguously'),
        ('YOGA_MALIKA_DHANA', 'LAGNA', '7 classical grahas fill houses 2-8 contiguously'),
        ('YOGA_MALIKA_KALATRA', 'LAGNA', '7 classical grahas fill houses 7-1 contiguously'),
        ('YOGA_MALIKA_KARMA', 'LAGNA', '7 classical grahas fill houses 10-4 contiguously'),
        ('YOGA_MALIKA_LABHA', 'LAGNA', '7 classical grahas fill houses 11-5 contiguously'),
        ('YOGA_MALIKA_LAGNA', 'LAGNA', '7 classical grahas fill houses 1-7 contiguously'),
        ('YOGA_MALIKA_PUTRA', 'LAGNA', '7 classical grahas fill houses 5-11 contiguously'),
        ('YOGA_MALIKA_RANDHRA', 'LAGNA', '7 classical grahas fill houses 8-2 contiguously'),
        ('YOGA_MALIKA_SATRU', 'LAGNA', '7 classical grahas fill houses 6-12 contiguously'),
        ('YOGA_MALIKA_SUKHA', 'LAGNA', '7 classical grahas fill houses 4-10 contiguously'),
        ('YOGA_MALIKA_VIKRAMA', 'LAGNA', '7 classical grahas fill houses 3-9 contiguously'),
        ('YOGA_MALIKA_VYAYA', 'LAGNA', '7 classical grahas fill houses 12-6 contiguously'),
        ('YOGA_MARUD', 'COMBINATION', 'Venus-Jupiter in trine, Jupiter 5th from Moon, Moon-Sun in mutual kendra'),
        ('YOGA_MATRUGAMI', 'COMBINATION', 'Moon or Venus afflicted by a malefic in a kendra, with a malefic also in the 4th'),
        ('YOGA_MATRUMOOLA_DHANA', 'COMBINATION', '2nd lord and 4th lord conjunct or mutually aspecting'),
        ('YOGA_MATRUNASA', 'COMBINATION', 'Moon joined or aspected by a malefic, or flanked by malefics on both sides'),
        ('YOGA_MATSYA', 'COMBINATION', 'Malefics in the 1st, 9th & 5th with a benefic also in the 5th, and malefics in the 4th & 8th'),
        ('YOGA_MOOKA', 'COMBINATION', '2nd lord in the 8th, conjunct Jupiter'),
        ('YOGA_MRIDANGA', 'COMBINATION', 'Lagna lord strong; an exalted graha''s navamsa dispositor sits in a kendra, well-disposed'),
        ('YOGA_MUSALA', 'COMBINATION', 'All 7 classical grahas occupy only fixed signs'),
        ('YOGA_NALA', 'COMBINATION', 'All 7 classical grahas occupy only dual signs'),
        ('YOGA_NAV', 'COMBINATION', 'All 7 classical grahas fill houses 1-7'),
        ('YOGA_NETRANASA', 'COMBINATION', '10th, 6th & 2nd lords conjunct, with the 10th lord in Lagna'),
        ('YOGA_PARANNABHOJANA', 'COMBINATION', '2nd lord debilitated and afflicted by a debilitated malefic'),
        ('YOGA_PARIJATHA', 'LAGNA', 'Lagna lord''s double dispositor lands in a kendra/trikona, or is strong'),
        ('YOGA_PARVATA', 'LAGNA', 'A benefic in a kendra with the 6th/8th (or 7th/8th) vacant or benefic-only'),
        ('YOGA_PASA', 'COMBINATION', 'The 7 classical grahas occupy exactly 5 signs, with no other Nabhasa yoga present'),
        ('YOGA_PUSHKALA', 'LAGNA', 'Lagna lord with Moon; Moon''s dispositor in a kendra or intimate friend aspecting Lagna, with a strong graha in Lagna'),
        ('YOGA_RAJALAKSHANA', 'LAGNA', 'Jupiter, Venus, Mercury & Moon all in kendras'),
        ('YOGA_RAJJU', 'COMBINATION', 'All 7 classical grahas occupy only movable signs'),
        ('YOGA_RAVI', 'COMBINATION', 'Sun in the 10th; 10th lord and Saturn conjunct in the 3rd'),
        ('YOGA_ROGAGRASTHA', 'LAGNA', 'Lagna lord in Lagna, joined by a 6th/8th/12th lord'),
        ('YOGA_RUCHAKA', 'LAGNA', 'Mars own/exalted in a kendra'),
        ('YOGA_SADA_SANCHARA', 'LAGNA', 'Lagna lord or its dispositor sits in a movable sign'),
        ('YOGA_SAKATA', 'COMBINATION', 'All 7 classical grahas confined to houses 1 & 7'),
        ('YOGA_SAKATA_LUNAR', 'MOON', 'Jupiter in the 6th, 8th or 12th from Moon'),
        ('YOGA_SAKTI', 'COMBINATION', 'All 7 classical grahas fill houses 7-10'),
        ('YOGA_SAMUDRA', 'COMBINATION', 'All 7 classical grahas confined to the 6 even houses'),
        ('YOGA_SANKHA', 'COMBINATION', '5th & 6th lords in mutual kendra distance, with a strong Lagna lord'),
        ('YOGA_SAPTHASANKHYA_SAHODARA', 'COMBINATION', '12th lord conjunct Mars; Moon in the 3rd conjunct Jupiter, undisturbed by Venus'),
        ('YOGA_SARALA', 'LAGNA', '8th lord occupies the 8th (own house)'),
        ('YOGA_SARASWATHI', 'COMBINATION', 'Jupiter, Venus & Mercury all in kendra/trikona/3rd/11th, Jupiter well-disposed'),
        ('YOGA_SAREERA_SOUKHYA', 'LAGNA', 'Lagna lord, Jupiter or Venus occupies a kendra'),
        ('YOGA_SARPA', 'LAGNA', 'All 3 natural malefics occupy kendras'),
        ('YOGA_SARPAGANDA', 'COMBINATION', 'Rahu and Gulika both occupy the 2nd'),
        ('YOGA_SASA', 'LAGNA', 'Saturn own/exalted in a kendra'),
        ('YOGA_SATKATHADI_SRAVANA', 'COMBINATION', '3rd lord and its navamsa dispositor are both natural benefics, with a benefic aspecting the 3rd'),
        ('YOGA_SIVA', 'COMBINATION', '5th lord in the 9th, 9th lord in the 10th, 10th lord in the 5th (mutual cycle)'),
        ('YOGA_SRADDHANNABHUKTHA', 'COMBINATION', 'Saturn rules, joins, or (debilitated) aspects the 2nd house or its lord'),
        ('YOGA_SREENATHA', 'COMBINATION', '7th lord exalted in the 10th, with the 10th & 9th lords conjunct'),
        ('YOGA_SRIK', 'LAGNA', 'All 4 natural benefics occupy kendras'),
        ('YOGA_SRINGHATAKA', 'COMBINATION', 'All 7 classical grahas confined to houses 1, 5 & 9'),
        ('YOGA_SULA', 'COMBINATION', 'The 7 classical grahas occupy exactly 3 signs, with no other Nabhasa yoga present'),
        ('YOGA_SUMUKHA', 'COMBINATION', '2nd lord in a kendra influenced by a benefic, or a benefic occupies the 2nd'),
        ('YOGA_SUNAPHA', 'MOON', 'A planet (not Sun) in the 2nd from Moon'),
        ('YOGA_THRILOCHANA', 'COMBINATION', 'Sun, Moon & Mars in mutual trine'),
        ('YOGA_UBHAYACHARI', 'SUN', 'Planets in both the 2nd and 12th from Sun (Moon excluded)'),
        ('YOGA_UTTAMA_GRIHA', 'COMBINATION', '4th lord in a kendra/trikona, joined by a benefic'),
        ('YOGA_VAJRA', 'COMBINATION', 'Benefics fill the 1st & 7th while malefics fill the 4th & 10th'),
        ('YOGA_VALLAKI', 'COMBINATION', 'The 7 classical grahas occupy exactly 7 signs, with no other Nabhasa yoga present'),
        ('YOGA_VANCHANA_CHORA_BHEETHI', 'COMBINATION', 'A malefic in Lagna with Gulika in the 5th/9th; or a kendra/trikona lord shares Gulika''s sign; or the Lagna lord shares a sign with Rahu/Saturn/Ketu'),
        ('YOGA_VAPEE', 'COMBINATION', 'All 7 classical grahas confined to the 4 panapara or 4 apoklima houses'),
        ('YOGA_VASI', 'SUN', 'A planet (not Moon) in the 12th from Sun'),
        ('YOGA_VASUMATHI', 'COMBINATION', 'A natural benefic in the 3rd, 6th, 10th or 11th from Lagna or Moon'),
        ('YOGA_VESI', 'SUN', 'A planet (not Moon) in the 2nd from Sun'),
        ('YOGA_VICHITRA_SAUDHA_PRAKARA', 'COMBINATION', '4th lord, 10th lord & Saturn conjunct, with Mars also joining the 4th lord'),
        ('YOGA_VIDYUT', 'COMBINATION', '11th lord deeply exalted, conjunct Venus, in a kendra from Lagna'),
        ('YOGA_VIHAGA', 'COMBINATION', 'All 7 classical grahas confined to houses 4 & 10'),
        ('YOGA_VIMALA', 'LAGNA', '12th lord occupies the 12th (own house)'),
        ('YOGA_VIPAREETA_RAJA', 'LAGNA', 'A lord of the 6th, 8th or 12th occupies a dusthana (6th, 8th or 12th)'),
        ('YOGA_VISHNU', 'COMBINATION', '9th lord''s navamsa-dispositor chain, the 10th lord & the 9th lord all share the 2nd-house sign'),
        ('YOGA_YAVA', 'COMBINATION', 'Malefics fill the 1st & 7th while benefics fill the 4th & 10th'),
        ('YOGA_YUDDHATPOORVA_DRIDHACHITTA', 'COMBINATION', '3rd lord exalted and malefic-joined, in a movable sign (D1 or navamsa)'),
        ('YOGA_YUGA', 'COMBINATION', 'The 7 classical grahas occupy exactly 2 signs, with no other Nabhasa yoga present'),
        ('YOGA_YUKTHI_SAMANWITHAVAGMI', 'COMBINATION', '2nd lord in a kendra/trikona joined by a benefic, or exalted and joined by Jupiter'),
        ('YOGA_YUPA', 'COMBINATION', 'All 7 classical grahas fill houses 1-4')
        ) v (YogaCode, FormationFamilyCode, ShortFormationRule)
    )
    INSERT dbo.tbl_Rule_Yoga (RuleSetId, YogaCode, FormationFamilyCode, ShortFormationRule)
    SELECT 1, d.YogaCode, d.FormationFamilyCode, d.ShortFormationRule
    FROM YogaDefinitions d
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.tbl_Rule_Yoga r WHERE r.RuleSetId = 1 AND r.YogaCode = d.YogaCode
    );

    EXEC(N'
    CREATE OR ALTER VIEW dbo.vw_ChartYogaEvaluations
    AS
    SELECT c.BirthDetailId, b.Name, y.ChartResultId, y.RuleSetId,
           y.SourceRefCode, y.SourceVariantCode, y.YogaCode, y.SourceLocator,
           y.Present, y.EvaluationStatus, y.MissingRequirementCodesJson,
           y.SubjectSex, y.IsNightBirth, y.ElongationDegrees,
           y.IsWaxingMoon, y.IsFullMoon, y.LunarPhasePolicyCode,
           y.SunriseMethodCode, y.Notes, y.ComputedAtUtc,
           r.FormationFamilyCode AS YogaTypeCode, r.ShortFormationRule AS YogaRule
    FROM dbo.tbl_Fact_YogaInputEvaluations y
    JOIN dbo.tbl_ChartResults c ON c.Id = y.ChartResultId
    JOIN dbo.tbl_BirthDetails b ON b.Id = c.BirthDetailId
    LEFT JOIN dbo.tbl_Rule_Yoga r ON r.YogaCode = y.YogaCode AND r.RuleSetId = y.RuleSetId AND r.SourceRefCode IS NULL;
    ');

    UPDATE dbo.tbl_Rule_Catalog
        SET Purpose = 'Source-attributed yoga definitions: predicates, qualifications, cancellations, outcomes and exact locators. FormationFamilyCode (Type) + ShortFormationRule (Rule) populated for 146 evaluated YogaCodes as of 079; the rest of the row (RequirementJson/CancellationJson/etc.) remains unpopulated (P4).'
    WHERE RuleTableName = 'tbl_Rule_Yoga';

    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('079_add_yoga_type_and_rule.sql',
        'tbl_Rule_Yoga FormationFamilyCode/ShortFormationRule seeded for 146 YogaCodes; vw_ChartYogaEvaluations exposes YogaTypeCode/YogaRule.');
END
GO

PRINT '079 applied: Yoga Type + Rule are DB-backed.';
GO
