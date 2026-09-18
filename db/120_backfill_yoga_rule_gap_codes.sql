-- =====================================================================
-- 120 — Backfill tbl_Rule_Yoga for YogaCodes with real coded predicates
-- but no row from 079.
--
-- Correction to 079's header comment: 079 said 146/223 YogaCodes were
-- covered, with a 61-code "uncoded Raman 201-300 tail" and 14 codes in
-- RamanYogaBatchEightEvaluator's "Unsupported" set. Both counts are now
-- stale: RamanFinalHundredCatalog.cs (079's cited source for the "61
-- tail") was fully transcribed into new evaluators
-- (PvrChapter11NumberedYogaEvaluator, RamanRajaYogaEvaluator,
-- RamanFamilyYogaEvaluator, RamanProgenyYogaEvaluator,
-- RamanAfflictionYogaEvaluator, RamanYogaBatchEightEvaluator) on
-- 2026-09-17, AFTER 079 was written; RamanFinalHundredCatalog.cs now has
-- an empty Groups array. RamanYogaBatchEightEvaluator's actual
-- unsupported set is 2 codes (YOGA_SODARANASA/178, YOGA_EKABHAGINI/179),
-- not 14. This migration seeds the codes left over from that gap —
-- every FormationFamilyCode/ShortFormationRule value below transcribed
-- directly from the actual predicate in the named evaluator file, same
-- discipline as 079, never freehand astrological recall. See
-- docs/ui/components/yoga.md for the corrected coverage description
-- (updated in the same change as this migration) and
-- tbl_Rule_Catalog.Purpose (computed via COUNT(*) below, not a
-- hardcoded literal, so it can't go stale the same way again).
--
-- 079 itself is an applied migration and is not edited
-- (db/README.md: "Never edit an applied migration. Add a corrective
-- NN+1 script instead.").
-- =====================================================================
USE [ikiastrro];
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = '120_backfill_yoga_rule_gap_codes.sql')
BEGIN
    ;WITH YogaDefinitions (YogaCode, FormationFamilyCode, ShortFormationRule) AS
    (
        SELECT * FROM (VALUES
        -- from PvrChapter11YogaEvaluator.cs
        ('YOGA_MAALA', 'LAGNA', 'Natural benefics occupy at least 3 of the 4 kendras (1st/4th/7th/10th) from Lagna'),
        ('YOGA_SUBHA', 'LAGNA', 'A natural benefic occupies Lagna, or benefics flank Lagna in both the 2nd and 12th (subha kartari)'),
        ('YOGA_ASUBHA', 'LAGNA', 'A natural malefic occupies Lagna, or malefics flank Lagna in both the 2nd and 12th (paapa kartari)'),
        ('YOGA_GURU_MANGALA', 'COMBINATION', 'Jupiter and Mars conjunct, or in mutual 7th houses from each other'),
        ('YOGA_CHAMARA', 'LAGNA', 'Lagna lord exalted in a kendra with Jupiter''s aspect, or two benefics conjunct in the 7th, 9th or 10th'),
        ('YOGA_KHADGA', 'COMBINATION', '2nd lord in the 9th, 9th lord in the 2nd, and Lagna lord in a kendra or trikona'),
        ('YOGA_LAGNAADHI', 'LAGNA', 'Natural benefics occupy both the 7th and 8th from Lagna, unafflicted by malefic conjunction or aspect'),
        ('YOGA_SAARADA', 'COMBINATION', '10th lord in the 5th, Mercury in a kendra, Sun in Leo, Mercury or Jupiter trine Moon, and Mars in the 11th'),
        ('YOGA_DHARMA_KARMADHIPATI', 'COMBINATION', '9th and 10th lords conjunct, in mutual aspect, or in exchange (parivartana)'),
        -- from PvrChapter11NumberedYogaEvaluator.cs
        ('YOGA_RAJA_ADVANCED', 'COMBINATION', 'Multiple classical forms (18) - chiefly Atmakaraka/Amatyakaraka/Poornakaraka links to the lagna, 5th, 9th or 10th lords, strong placements in Shad Varga or D1/D9/D3, or benefics confined to kendras with malefics confined to the 3rd/6th/11th'),
        ('YOGA_RAJA_SAMBANDHA', 'COMBINATION', 'Multiple classical forms (15) - chiefly Atmakaraka/Amatyakaraka conjunction, strength or aspect with the 9th/10th/11th lords or benefics, or a Lagna/10th-lord exchange'),
        -- from RamanRajaYogaEvaluator.cs
        ('YOGA_RAJA', 'COMBINATION', 'Multiple classical forms (Raman combinations 245-263) - chiefly 3+ planets exalted/own in kendra-trikona, Digbala counts, a debilitated planet rescued by retrograde/bright rays/exalted navamsa, or specific graha-in-house patterns'),
        -- from RamanFamilyYogaEvaluator.cs
        ('YOGA_SAHODAREE_SANGAMA', 'COMBINATION', '7th lord and Venus conjunct in the 4th, afflicted by malefics or in a cruel shashtiamsa'),
        ('YOGA_KAPATA', 'COMBINATION', 'Multiple forms - the 4th house/4th lord afflicted by malefics (Saturn, Mars, Rahu and a malefic 10th lord all in the 4th; or the 4th lord joined by Saturn, Maandi and Rahu, malefic-aspected)'),
        ('YOGA_NISHKAPATA', 'COMBINATION', 'The 4th house/its sign lord is benefic, or Lagna lord joins the 4th with a benefic or reaches Parvatamsa'),
        ('YOGA_MATRU_SATRUTWA', 'COMBINATION', 'Mercury is lord of both Lagna and the 4th, and is joined or aspected by a malefic'),
        ('YOGA_MATRU_SNEHA', 'COMBINATION', 'Lagna lord and 4th lord share a sign, are natural/temporal friends, or are aspected by benefics'),
        ('YOGA_VAHANA', 'COMBINATION', 'Lagna lord joins the 4th, 9th or 11th; or the 4th lord is exalted with its exaltation-sign lord in a kendra or trikona'),
        ('YOGA_ANAPATHYA', 'COMBINATION', 'Jupiter and the lords of Lagna, 5th and 7th are all weak'),
        ('YOGA_SARPASAPA', 'COMBINATION', 'Multiple forms - Rahu in the 5th (a Mars sign or Mars-aspected), or the 5th lord conjunct Rahu with Saturn in the 5th aspected by Moon, or Rahu in Lagna with Mars-linked Jupiter and a dusthana 5th lord'),
        ('YOGA_PITRUSAPA_SUTAKSHAYA', 'SUN', 'Multiple forms - Sun debilitated/hemmed by malefics in the 5th (or its navamsa in Capricorn/Aquarius), or as 5th lord afflicted in a trine, or linked to the 5th lord with malefics in Lagna and the 5th/9th'),
        ('YOGA_MATRUSAPA_SUTAKSHAYA', 'MOON', '8th lord in the 5th, 5th lord in the 8th, and Moon with the 4th lord both in the 6th'),
        ('YOGA_BHRATRUSAPA_SUTAKSHAYA', 'COMBINATION', 'Lagna lord and 5th lord both in the 8th, and the 3rd lord joins Mars and Rahu in the 5th'),
        ('YOGA_PRETASAPA', 'COMBINATION', 'Sun and Saturn in the 5th, a weak Moon in the 7th, Rahu in Lagna, and Jupiter in the 12th'),
        -- from RamanProgenyYogaEvaluator.cs
        ('YOGA_BAHUPUTRA', 'COMBINATION', 'Rahu in the 5th with a navamsa dispositor other than Saturn; or a planet linked to the 7th lord has its navamsa dispositor in the 1st, 2nd or 5th'),
        ('YOGA_DATTAPUTRA', 'COMBINATION', 'Mars and Saturn in the 5th with Lagna lord in a Mercury sign joined/aspected by Mercury; or 7th lord in the 11th, 5th lord joined by a benefic, and Mars or Saturn in the 5th'),
        ('YOGA_APUTRA', 'LAGNA', '5th lord occupies a dusthana (6th, 8th or 12th)'),
        ('YOGA_EKAPUTRA', 'LAGNA', '5th lord occupies a kendra or trikona'),
        ('YOGA_SUPUTRA', 'COMBINATION', 'Jupiter is 5th lord, and Sun is strong or occupies a kendra/trikona'),
        ('YOGA_KALANIRDESAT_PUTRA', 'COMBINATION', 'Jupiter in the 5th with its lord joined by Venus; or Jupiter in the 9th, Venus 9th from Jupiter and conjunct Lagna lord'),
        ('YOGA_KALANIRDESAT_PUTRANASA', 'COMBINATION', 'Rahu in the 5th with its lord malefic-linked and Jupiter debilitated; or malefics occupy both the 5th and the 5th house from Jupiter'),
        ('YOGA_BUDDHIMATURYA', 'COMBINATION', 'Benefics occupy the 5th and the 5th lord is joined or aspected by a benefic'),
        ('YOGA_THEEVRABUDDHI', 'COMBINATION', '5th lord''s navamsa dispositor is itself a benefic, joined or aspected by another benefic'),
        ('YOGA_BUDDHI_JADA', 'COMBINATION', 'Lagna lord joined/aspected by a malefic, Saturn in the 5th, and Lagna lord also aspected by Saturn'),
        ('YOGA_THRIKALAGNANA', 'COMBINATION', 'Jupiter in Mridu shashtiamsha of its own navamsa, or Vaiseshikamsa-strong (Gopuramsa+), and aspected by a benefic'),
        ('YOGA_PUTRA_SUKHA', 'COMBINATION', 'Jupiter and Venus both in the 5th, or Mercury in the 5th, or benefics occupy a benefic-ruled 5th house'),
        ('YOGA_JARA', 'COMBINATION', '10th, 2nd and 7th lords all occupy the 10th house'),
        ('YOGA_JARAJA_PUTRA', 'COMBINATION', 'Strong 5th and 7th lords both joined to the 6th lord, each aspected by a benefic'),
        ('YOGA_BAHU_STREE', 'COMBINATION', 'Lagna lord and 7th lord conjunct or mutually aspecting; or 9th lord in the 7th, 7th lord in the 4th, with Lagna or 11th lord in a kendra'),
        ('YOGA_SATKALATRA', 'COMBINATION', '7th lord or Venus joined or aspected by Jupiter or Mercury'),
        ('YOGA_BHAGA_CHUMBANA', 'COMBINATION', '7th lord in the 4th conjunct Venus, or Lagna lord debilitated in Rasi or Navamsa'),
        ('YOGA_BHAGYA', 'COMBINATION', 'A strong benefic in the 1st, 3rd or 5th house aspects the 9th'),
        ('YOGA_JANANAT_PURVAM_PITRU_MARANA', 'COMBINATION', 'Sun in the 6th, 8th or 12th; 8th lord in the 9th; 12th lord in Lagna; and 6th lord in the 5th'),
        ('YOGA_DHATRUTWA', 'COMBINATION', '9th lord exalted and benefic-aspected, with a benefic also occupying the 9th'),
        ('YOGA_APAKEERTI', 'COMBINATION', 'Sun and Saturn in the 10th, aspected by malefics, or both occupy malefic navamsas'),
        -- from RamanAfflictionYogaEvaluator.cs
        ('YOGA_GALAKARNA', 'COMBINATION', 'Rahu and Mandi both in the 3rd, or Rahu/Mars in the 3rd in a cruel shashtiamsa'),
        ('YOGA_VRANA', 'LAGNA', '6th lord is a malefic occupying Lagna, 8th or 10th'),
        ('YOGA_SISNAVYADHI', 'COMBINATION', 'Mercury in Lagna conjunct both the 6th lord and the 8th lord'),
        ('YOGA_KALATRASHANDA', 'COMBINATION', '7th lord in the 6th conjunct Venus'),
        ('YOGA_KUSHTAROGA', 'COMBINATION', 'Lagna lord in the 4th or 12th conjunct Mars and Mercury; or Jupiter in the 6th conjunct Saturn and Moon'),
        ('YOGA_KSHAYAROGA', 'COMBINATION', 'Rahu in the 6th, Mandi in a kendra, and Lagna lord in the 8th'),
        ('YOGA_BANDHANA', 'COMBINATION', 'Lagna lord and 6th lord both in a kendra/trikona, each conjunct Saturn, Rahu or Ketu'),
        ('YOGA_KARASCHEDA', 'COMBINATION', 'Saturn in the 9th with Jupiter in the 3rd (or 8th/12th, or Moon in 7th/8th conjunct Mars, or Rahu-Saturn-Mercury conjunct in the 10th)'),
        ('YOGA_SIRACHCHEDA', 'COMBINATION', '6th lord conjunct Venus, with Sun or Saturn conjunct Rahu in a cruel shashtiamsa'),
        ('YOGA_DURMARANA', 'COMBINATION', 'Moon aspected by Lagna lord, in the 6th/8th/12th conjunct Saturn, Rahu or Mandi'),
        ('YOGA_YUDDHE_MARANA', 'COMBINATION', '6th or 8th lord conjunct 3rd lord with Rahu/Saturn in a cruel shashtiamsa, or Saturn''s drekkana lord tied to Mars'),
        ('YOGA_SANGHATAKA_MARANA', 'COMBINATION', 'Two malefics in the 8th in a martian rasi/navamsa with one in a cruel shashtiamsa; or Sun, Rahu and Saturn all aspected by the 8th lord and each in evil amsas'),
        ('YOGA_PEENASAROGA', 'COMBINATION', 'Moon in the 6th, Saturn in the 8th, another malefic in the 12th, and Lagna lord''s navamsa dispositor is a malefic'),
        ('YOGA_PITTAROGA', 'COMBINATION', 'Sun in the 6th conjunct one malefic and aspected by another'),
        ('YOGA_VIKALANGA_PATNI', 'COMBINATION', 'Venus and Sun together in the 5th, 7th or 9th'),
        ('YOGA_PUTRA_KALATRA_HEENA', 'COMBINATION', 'Waning Moon in the 5th with Sun, Mars and Saturn filling Lagna, 7th and 12th between them'),
        ('YOGA_BHARYASAHA_VYABHICHARA', 'COMBINATION', 'Venus, Saturn and Mars all conjunct the Moon in the 7th'),
        ('YOGA_VAMSACHEDA', 'COMBINATION', 'Moon in the 10th, Venus in the 7th, and a malefic in the 4th'),
        ('YOGA_GUHYAROGA', 'COMBINATION', 'Moon in navamsa Cancer or Scorpio conjunct a malefic'),
        ('YOGA_ANGAHEENA', 'COMBINATION', 'Moon in the 10th, Mars in the 7th, and Saturn 2nd from the Sun'),
        ('YOGA_SWETAKUSHTA', 'COMBINATION', 'Mars in the 2nd, Saturn in the 12th, Moon in Lagna, and Sun in the 7th'),
        ('YOGA_PISACHA_GRASTHA', 'COMBINATION', 'Rahu in Lagna conjunct Moon, with all three malefics in Lagna, 5th or 9th'),
        ('YOGA_VATHAROGA', 'COMBINATION', 'Jupiter in Lagna and Saturn in the 7th'),
        ('YOGA_MATIBHRAMANA', 'COMBINATION', 'Jupiter in Lagna with Mars in the 7th; or Saturn in Lagna with Mars in the 5th/7th/9th; or Saturn in the 12th with a waning Moon; or Moon-Mercury conjunct in a kendra, influenced by another planet'),
        ('YOGA_KHALWATA', 'LAGNA', 'Ascendant is a malefic sign, Sagittarius or Taurus, aspected by a malefic'),
        ('YOGA_NISHTURABHASHI', 'MOON', 'Moon conjunct Saturn'),
        ('YOGA_RAJABHRASHTA', 'COMBINATION', 'Lords of Arudha Lagna and Arudha 12th (A12) are conjunct'),
        ('YOGA_RAJA_BHANGA', 'COMBINATION', 'Leo Lagna with exalted Saturn in a debilitated navamsa or aspected by a benefic; or Sun at exactly 10 degrees Libra'),
        ('YOGA_GOHANTA', 'COMBINATION', 'A malefic unaspected by any benefic in a kendra, with Jupiter in the 8th'),
        -- from RamanYogaBatchEightEvaluator.cs
        ('YOGA_PARIHASAKA', 'COMBINATION', 'Dispositor of the Navamsa held by the Sun has attained Vaiseshikamsa and occupies the 2nd'),
        ('YOGA_BHOJANA_SOUKHYA', 'COMBINATION', 'A strong 2nd lord has attained Vaiseshikamsa and is aspected by Jupiter or Venus'),
        ('YOGA_ANNADANA', 'COMBINATION', '2nd lord has attained Vaiseshikamsa and is conjunct or aspected by both Jupiter and Mercury'),
        ('YOGA_VAKCHALANA', 'COMBINATION', '2nd lord and its navamsa dispositor are both malefics, with no benefic in or aspecting the 2nd'),
        ('YOGA_VISHAPRAYOGA', 'COMBINATION', 'A malefic in the 2nd aspected by another malefic, its navamsa dispositor also a malefic, and the 2nd lord aspected by yet another malefic'),
        ('YOGA_BHRATRUVRIDDHI', 'COMBINATION', '3rd lord or Mars is strong, and benefic-influenced (or a benefic aspects the 3rd house)'),
        ('YOGA_PARAKRAMA', 'COMBINATION', '3rd lord''s navamsa dispositor is a benefic and benefic-influenced, with Mars'' sign lord also a benefic'),
        ('YOGA_YUDDHA_PRAVEENA', 'COMBINATION', 'The navamsa dispositor two hops on from the 3rd lord holds at least 4 of 6 shadvarga dignities'),
        ('YOGA_YUDDHATPASCHAT_DRIDHA', 'COMBINATION', '3rd lord in a fixed sign and fixed navamsa, in a cruel shashtiamsa, with that sign''s lord debilitated'),
        ('YOGA_MATRU_DEERGHAYUR', 'COMBINATION', 'A benefic in the 4th with the 4th lord exalted and Moon strong; or the 4th lord''s navamsa dispositor is strong and forms a kendra from both Lagna and Moon')
        ) v (YogaCode, FormationFamilyCode, ShortFormationRule)
    )
    INSERT dbo.tbl_Rule_Yoga (RuleSetId, YogaCode, FormationFamilyCode, ShortFormationRule)
    SELECT 1, d.YogaCode, d.FormationFamilyCode, d.ShortFormationRule
    FROM YogaDefinitions d
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.tbl_Rule_Yoga r WHERE r.RuleSetId = 1 AND r.YogaCode = d.YogaCode AND r.SourceRefCode IS NULL
    );

    UPDATE dbo.tbl_Rule_Catalog
        SET Purpose = 'Source-attributed yoga definitions: predicates, qualifications, cancellations, outcomes and exact locators. FormationFamilyCode (Type) + ShortFormationRule (Rule) populated for '
            + CAST((SELECT COUNT(*) FROM dbo.tbl_Rule_Yoga WHERE RuleSetId = 1 AND SourceRefCode IS NULL) AS VARCHAR(10))
            + ' evaluated YogaCodes as of 120; the rest of the row (RequirementJson/CancellationJson/etc.) remains unpopulated (P4).'
    WHERE RuleTableName = 'tbl_Rule_Yoga';

    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES ('120_backfill_yoga_rule_gap_codes.sql',
        'tbl_Rule_Yoga backfilled for YogaCodes previously uncovered by 079; corrects 079''s stale 146/223, 61+14 coverage claim.');
END
GO

PRINT '120 applied: Yoga Rule gap backfilled.';
GO
