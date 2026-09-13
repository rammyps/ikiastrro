/* House-lord-in-house: 7th through 12th lords, each across all 12 occupied houses (6x12 = 72
   combinations). Paraphrased (not transcribed) from SRC_RAMAN_HTJH Vol. II, "Results of the
   Lord of the [Nth] House being situated/placed/posited in Different Houses" -- chapters XI
   through XVI. Whole-sign houses, Lagna reference point, per the project's 2026-09-07
   house-system decision. Continues 090 (1st lord) and 091 (2nd-6th lords).
   BASELINE = the source's general/unqualified result; WELL_DISPOSED / AFFLICTED = its own
   explicitly separate qualified branches, where the source gives them. Vol. II frames most
   of these as "the Nth lord joins/combines with the [occupied]-house lord in that house" --
   preserved as the source's own framing rather than simplified to placement alone. This is a
   reference pointer + paraphrase, not a copy of the copyrighted text.

   Known source caveat (see docs/research/domain/house-placement.md "Important qualifications
   exposed by Volume II"): the 9th-lord-in-4th afflicted branch (RAM-II9) contains an internal
   contradiction in the source itself (states no domestic unhappiness, then immediately
   describes misery) -- preserved as unresolved, not corrected. */

-- 1) Combination dimension: owned houses 7-12 x each occupied house.
INSERT research.tbl_Dim_SourceReferenceHouseLordInHouse (OwnedHouseId, OccupiedHouseId)
SELECT owned.Id, occ.Id
FROM research.tbl_Dim_SourceReferenceHouse owned
CROSS JOIN research.tbl_Dim_SourceReferenceHouse occ
WHERE owned.HouseNumber BETWEEN 7 AND 12
  AND NOT EXISTS (
      SELECT 1 FROM research.tbl_Dim_SourceReferenceHouseLordInHouse x
      WHERE x.OwnedHouseId = owned.Id AND x.OccupiedHouseId = occ.Id
        AND x.HouseSystemCode = 'WHOLE_SIGN' AND x.ReferencePointCode = 'LAGNA'
  );

-- 2) Source pointers: Raman (the detailed result) + PVR (whole-sign/method referral).
INSERT research.tbl_Dim_SourceReferenceHouseLordInHouseText
    (HouseLordInHouseId, SourceRefCode, WorkTitle, Author, Edition, Chapter, VerseOrPage,
     LanguageCode, TextTypeCode, CopyrightStatus, SourceLocator, Notes)
SELECT x.Id, s.SourceRefCode, s.WorkTitle, s.Author, s.Edition, s.Chapter, s.VerseOrPage,
       'en', 'Reference', s.CopyrightStatus, s.SourceLocator, s.Notes
FROM research.tbl_Dim_SourceReferenceHouseLordInHouse x
JOIN research.tbl_Dim_SourceReferenceHouse owned ON owned.Id = x.OwnedHouseId AND owned.HouseNumber BETWEEN 7 AND 12
CROSS APPLY (VALUES
    ('SRC_RAMAN_HTJH', N'How to Judge a Horoscope (vols I-II)', N'B. V. Raman',
     N'Fourth Edition, Delhi, 1992, Motilal Banarsidass, ISBN 81-208-0845-2',
     CASE owned.HouseNumber
        WHEN 7 THEN N'Vol. II ch. XI - Concerning the Seventh House'
        WHEN 8 THEN N'Vol. II ch. XII - Concerning the Eighth House'
        WHEN 9 THEN N'Vol. II ch. XIII - Concerning the Ninth House'
        WHEN 10 THEN N'Vol. II ch. XIV - Concerning the Tenth House'
        WHEN 11 THEN N'Vol. II ch. XV - Concerning the Eleventh House'
        WHEN 12 THEN N'Vol. II ch. XVI - Concerning the Twelfth House' END,
     CASE owned.HouseNumber
        WHEN 7 THEN N'RAM-II7; scan pp. 9-12' WHEN 8 THEN N'RAM-II8; scan pp. 78-83'
        WHEN 9 THEN N'RAM-II9; scan pp. 187-190' WHEN 10 THEN N'RAM-II10; scan pp. 246-249'
        WHEN 11 THEN N'RAM-II11; scan pp. 369-372' WHEN 12 THEN N'RAM-II12; scan pp. 418-421' END,
     'SummaryOnly', N'docs/research/sources.md RAM-II7..RAM-II12 locators',
     N'Reference pointer only; paraphrased in the Claim table, not transcribed. OCR extract: D:\@ClaudeSpace\BookExtracts\how-to-judge-a-horoscope-2-ocr.md.'),
    ('SRC_PVR_INTEGRATED', N'Vedic Astrology: An Integrated Approach', N'P. V. R. Narasimha Rao', NULL,
     N'Ch. 13 sec. 13.4.1', N'Printed pp. 169-170',
     'SummaryOnly', N'PVR-H4',
     N'Establishes the whole-sign/Lagna-reference method this project follows and refers readers to Raman for the detailed 12-house catalogue; does not itself state per-house results.')
) s (SourceRefCode, WorkTitle, Author, Edition, Chapter, VerseOrPage, CopyrightStatus, SourceLocator, Notes)
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferenceHouseLordInHouseText t
    WHERE t.HouseLordInHouseId = x.Id AND t.SourceRefCode = s.SourceRefCode
);

-- 3) Claims: paraphrased BASELINE / WELL_DISPOSED / AFFLICTED branches per combination.
;WITH ClaimDefs (OwnedHouseNumber, OccupiedHouseNumber, BranchCode, ClaimText, RequiredConditionsJson) AS
(
    SELECT * FROM (VALUES
    -- ===== 7th lord =====
    (7,1,'BASELINE', N'Likely marries someone known since childhood or raised in the same household; a stable, mature spouse; the native is intelligent and level-headed.', N'{"branch":"baseline"}'),
    (7,1,'AFFLICTED', N'Afflictions to the 7th lord bring constant travel; if the 7th lord and Venus are both afflicted, a tendency toward illicit relationships.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires Venus condition too."}'),
    (7,2,'BASELINE', N'Wealth comes through women or through marriage.', N'{"branch":"baseline"}'),
    (7,2,'AFFLICTED', N'Afflicted: money through disreputable means (including trading on women, even the spouse), begging for funeral-offering food, more than one marriage if the 2nd house is a dual sign, a wavering and sensually inclined mind; a maraka dasha of the 7th lord risks death.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (7,3,'BASELINE', N'Fortunate siblings, possibly living abroad; female offspring survive.', N'{"branch":"baseline"}'),
    (7,3,'AFFLICTED', N'Afflicted: adultery with a sibling''s spouse, and misfortune to siblings generally.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (7,4,'BASELINE', N'A fortunate, happy spouse with many children and comforts; high academic achievement; owns several vehicles.', N'{"branch":"baseline"}'),
    (7,4,'AFFLICTED', N'Afflicted: domestic disharmony through an immature, mean-spirited spouse, endless vehicle-related trouble; severe nodal or malefic affliction casts doubt on the spouse''s character.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (7,5,'BASELINE', N'An early marriage into an affluent family; a mature spouse who benefits the native.', N'{"branch":"baseline"}'),
    (7,5,'AFFLICTED', N'A weak 7th lord risks childlessness; severe affliction risks children through the spouse''s infidelity, or only female issue if both afflictions and benefic influence are mixed; trouble with superiors from foreign sources.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (7,6,'BASELINE', N'Possibly two marriages with both partners living; may marry a cousin.', N'{"branch":"baseline"}'),
    (7,6,'AFFLICTED', N'Badly afflicted with an ill-disposed Venus: impotence or other illness, a sickly and jealous spouse; a well-placed Venus with an afflicted 7th lord instead brings piles; a weak but unafflicted Venus risks deserting or losing the spouse through an indiscreet act.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires Venus condition too."}'),
    (7,7,'BASELINE', N'Well placed: a charming, magnetic personality; the spouse comes from a reputable, socially prominent family.', N'{"branch":"baseline"}'),
    (7,7,'AFFLICTED', N'Weak and afflicted: a lonely life without marriage or friends, and loss through failed marriage negotiations.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (7,8,'BASELINE', N'Well placed: marriage to a relative or to a wealthy partner.', N'{"branch":"baseline"}'),
    (7,8,'AFFLICTED', N'Afflicted: early death of the partner, or the native dies in a distant land; a sickly, ill-tempered spouse leading to estrangement.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (7,9,'BASELINE', N'Fortified: the father may live abroad while the native prospers in foreign lands; an accomplished spouse who supports a righteous life.', N'{"branch":"baseline"}'),
    (7,9,'AFFLICTED', N'Afflicted: early death of the father; the spouse may pull the native off the righteous path, risking wasted wealth and penury.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (7,10,'BASELINE', N'Prosperity in a profession abroad, or a career involving constant travel; a devoted spouse who may be employed and contribute to income or career.', N'{"branch":"baseline"}'),
    (7,10,'AFFLICTED', N'Afflicted: an avaricious, over-ambitious but under-capable spouse, harming the native''s career.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (7,11,'BASELINE', N'Possibly more than one marriage or association with several women; a beneficially disposed lord brings a wealthy spouse.', N'{"branch":"baseline"}'),
    (7,11,'AFFLICTED', N'Afflicted: more than one marriage, though one spouse outlives the native.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (7,12,'BASELINE', N'Possibly more than one marriage, including a clandestine second marriage while the first spouse still lives.', N'{"branch":"baseline"}'),
    (7,12,'AFFLICTED', N'Severe affliction: the spouse dies or separates soon after marriage with no remarriage, possibly death while travelling or abroad; if both the karaka Venus and the 7th lord are weak, the native may never marry, and any spouse comes from a servant''s family, with a close-fisted, generally poor native.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires Venus condition too."}'),
    -- ===== 8th lord (framed by the source as "joins the Nth lord in the Nth house") =====
    (8,1,'BASELINE', N'Combined with the Lagna lord in the Ascendant: penury and heavy debt, persistent misfortune.', N'{"branch":"baseline","note":"Weak 8th lord, or in 6th/8th/12th from Navamsa Lagna, reduces intensity per source."}'),
    (8,1,'AFFLICTED', N'Severely afflicted: bodily complaints, disease and disfigurement, a weak constitution, and trouble from superiors or government.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (8,2,'BASELINE', N'Joined with the 2nd lord in the 2nd: assorted troubles, eye and tooth problems, unpalatable food, domestic discontent risking estrangement.', N'{"branch":"baseline","note":"Severity scales with longevity/dignity per source; 6th/8th/12th-from-Navamsa placement reduces intensity."}'),
    (8,3,'BASELINE', N'Combined with the 3rd lord in the 3rd: ear trouble or deafness, discord with siblings, fear and mental anguish (even hallucination), debt trouble.', N'{"branch":"baseline"}'),
    (8,3,'AFFLICTED', N'Malefic-afflicted: unbearable suffering.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (8,3,'WELL_DISPOSED', N'Combined instead with the 6th or 12th lord: benefic results, a monetary windfall through writing or through a sibling.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires the 6th/12th lord''s own placement too."}'),
    (8,4,'BASELINE', N'Joined with the 4th lord in the 4th: shattered peace of mind, domestic and financial trouble, risk to the mother''s health, house/land/vehicle problems.', N'{"branch":"baseline"}'),
    (8,4,'AFFLICTED', N'Heavy affliction: loss of land or property (possibly forced abroad into further trouble), loss of vehicles or pets, professional reverses.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (8,5,'BASELINE', N'Joined with the 5th lord in the 5th: trouble for the native''s children (even criminal trouble), discord with the father, a child''s illness.', N'{"branch":"baseline"}'),
    (8,5,'AFFLICTED', N'Heavy affliction: a child''s death or serious illness/mental impairment, the native''s own poor health; fortified in a kendra or trikona instead intensifies the evil here.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (8,6,'BASELINE', N'Joined with the 6th lord in the 6th: forms a Raja Yoga bringing material affluence, fame and fulfilled desires, though ill-health is likely since the 6th is the disease house.', N'{"branch":"baseline"}'),
    (8,6,'AFFLICTED', N'Afflicted: loss of money through theft, trouble with courts or police, hardship to the maternal uncle -- worse if the 8th lord sits in a kendra or trikona.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (8,6,'WELL_DISPOSED', N'A fortified 6th lord instead lets the native overcome such troubles and prevail over enemies.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires the 6th lord''s own strength too."}'),
    (8,7,'BASELINE', N'Joined with the 7th lord in the 7th: shortened longevity, the spouse''s poor health.', N'{"branch":"baseline"}'),
    (8,7,'AFFLICTED', N'Afflicted: the native''s own illness, trouble abroad.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (8,7,'WELL_DISPOSED', N'A strong 7th and 8th lord instead brings distinguished diplomatic missions abroad.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires the 7th lord''s own strength too."}'),
    (8,8,'BASELINE', N'In its own house, in strength: a long, happy life with land, vehicles, power and position earned through past merit.', N'{"branch":"baseline"}'),
    (8,8,'AFFLICTED', N'Weak: no serious trouble but little fortune either, and the father may face a crisis; afflicted outright: failure in undertakings through wrongdoing.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (8,9,'BASELINE', N'Joined with the 9th lord in the 9th with malefics: loss of the father''s property and discord with him; an afflicted Sun risks the father''s death during the 9th lord''s period.', N'{"branch":"baseline"}'),
    (8,9,'WELL_DISPOSED', N'Joined with benefics instead: the native acquires the father''s property and enjoys harmonious relations.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires benefic-vs-malefic association, not this placement alone."}'),
    (8,9,'AFFLICTED', N'A weak 9th lord: hardship, misery, and desertion by friends and relatives, criticism from superiors.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (8,10,'BASELINE', N'Joined with the 10th lord in the 10th: slow career advancement, obstacles, being superseded by subordinates, possible resort to deceit and legal trouble; a further afflicted 2nd lord adds reputational damage from unpayable debt.', N'{"branch":"baseline"}'),
    (8,10,'WELL_DISPOSED', N'Can also bring unexpected gains from the death of a superior or elder.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (8,11,'BASELINE', N'Joined with the 11th lord in the 11th: trouble for close friends, hardship for the elder sibling, strained relations with him, business losses and debt.', N'{"branch":"baseline"}'),
    (8,11,'WELL_DISPOSED', N'Benefic influence still brings trouble but the native gets help from friends and the elder sibling to overcome it.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires benefic influence on the combination, a separate fact."}'),
    (8,12,'BASELINE', N'Joined with the 12th lord in the 12th: forms a Raja Yoga.', N'{"branch":"baseline"}'),
    (8,12,'AFFLICTED', N'If benefics instead join the 8th lord here, the source describes unfavourable results -- treachery from friends causing several problems; preserved as stated, an inversion of the usual benefic/malefic expectation.', N'{"branch":"afflicted","evaluableToday":"no","note":"Source explicitly inverts the usual benefic-favourable pattern here; do not normalise away."}'),
    -- ===== 9th lord =====
    (9,1,'BASELINE', N'The native becomes self-made, earning substantially through his own efforts.', N'{"branch":"baseline"}'),
    (9,1,'WELL_DISPOSED', N'Combined with the Lagna lord in the first house and linked with or aspected by a benefic: notably fortunate, wealthy and happy.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires the Lagna lord''s own placement too."}'),
    (9,2,'BASELINE', N'Beneficially placed: the father is rich and influential, and the native acquires wealth from him.', N'{"branch":"baseline"}'),
    (9,2,'AFFLICTED', N'Malefic influence: ruin or destruction of paternal property.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (9,3,'BASELINE', N'Fortune through writing, speech and oratory; the father is of moderate means while the native advances through siblings.', N'{"branch":"baseline"}'),
    (9,3,'AFFLICTED', N'Malefic affliction: trouble through irrational or even obscene writing, possibly forcing the sale of paternal property.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (9,4,'BASELINE', N'Extensive land and fine houses, or income through estate/land dealings; a rich, fortunate mother; inherits the father''s immovable property.', N'{"branch":"baseline"}'),
    (9,4,'AFFLICTED', N'An unresolved source inconsistency: the text states no domestic unhappiness follows, then immediately describes miseries from a hard-hearted father or parental discord -- preserved as unresolved, not reconciled. Rahu''s affliction specifically risks the mother''s divorce or separation.', N'{"branch":"afflicted","evaluableToday":"no","note":"RAM-II9 source-internal contradiction; see docs/research/domain/house-placement.md. Do not silently pick a side."}'),
    (9,5,'BASELINE', N'A prosperous, famous father; fortunate sons who achieve success and distinction.', N'{"branch":"baseline"}'),
    (9,6,'BASELINE', N'A sickly father with chronic disease.', N'{"branch":"baseline"}'),
    (9,6,'WELL_DISPOSED', N'Benefics flanking the 6th here bring wealth through the successful resolution of the father''s legal troubles (compensation, costs).', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires benefics flanking the 6th house, a separate fact."}'),
    (9,6,'AFFLICTED', N'Malefic affliction: fortune is frustrated by litigation involving the father or his debts.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (9,7,'BASELINE', N'Prosperity abroad for both native and father; a noble, fortunate spouse.', N'{"branch":"baseline"}'),
    (9,7,'AFFLICTED', N'Inauspicious yogas afflicting the 9th lord: the father may die abroad.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires identifying the specific ashubha yogas involved."}'),
    (9,8,'BASELINE', N'Possible early loss of the father.', N'{"branch":"baseline"}'),
    (9,8,'AFFLICTED', N'Malefic affliction of the 8th house in this configuration: severe poverty and heavy responsibility from the father''s death.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (9,8,'WELL_DISPOSED', N'Benefic influence on the 9th lord instead brings substantial paternal inheritance; affliction can separately risk abandoning tradition or damaging family-founded religious institutions.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (9,9,'BASELINE', N'A long-lived, prosperous father; the native is religious, charitable, and travels abroad to earn money and distinction.', N'{"branch":"baseline"}'),
    (9,9,'AFFLICTED', N'Malefic affliction, or the 9th lord in the 6th/8th/12th from Navamsa Lagna: the father dies early.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (9,10,'BASELINE', N'Great fame and power, generosity, positions of authority, considerable wealth, and a righteous, law-abiding livelihood.', N'{"branch":"baseline"}'),
    (9,11,'BASELINE', N'Exceptional wealth, powerful and influential friends, a well-known and well-placed father.', N'{"branch":"baseline"}'),
    (9,11,'AFFLICTED', N'Afflicted: unfaithful friends destroy the native''s wealth through selfish scheming and fraud.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (9,12,'BASELINE', N'A poor background, hard work without guaranteed success, a religious and noble but perpetually wanting disposition, and possible early loss of the father leaving the native penniless.', N'{"branch":"baseline"}'),
    -- ===== 10th lord =====
    (10,1,'BASELINE', N'Rises through sheer perseverance, self-employed or pursuing an independent profession.', N'{"branch":"baseline"}'),
    (10,1,'WELL_DISPOSED', N'Combined with the Lagna lord in the first house: notable fame, a pioneering figure who founds public institutions and engages in social projects.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires the Lagna lord''s own placement too."}'),
    (10,2,'BASELINE', N'Fortunate, prospers well, and makes substantial money -- potentially by developing the family trade.', N'{"branch":"baseline"}'),
    (10,2,'AFFLICTED', N'Malefic affliction: business losses, possibly winding up the family business (though catering or restaurant ventures can still prosper).', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (10,3,'BASELINE', N'Constant short-distance travel.', N'{"branch":"baseline"}'),
    (10,3,'WELL_DISPOSED', N'Well placed: becomes a celebrated speaker or writer, aided in career by siblings.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (10,3,'AFFLICTED', N'In the 6th, 8th or 12th from Navamsa Lagna, or an unfriendly constellation here: slow, obstacle-ridden career advancement, worsened by an afflicted 3rd lord causing sibling rivalry.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires Navamsa-Lagna-relative placement, not yet a computed fact."}'),
    (10,4,'BASELINE', N'Fortunate and widely learned, admired for both learning and generosity.', N'{"branch":"baseline"}'),
    (10,4,'WELL_DISPOSED', N'Strong here: respected wherever he goes, royal favour, agricultural or property dealings; combined favourably with the 4th and 9th lords, can bring high political authority (president or head of government).', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (10,4,'AFFLICTED', N'Depressed, eclipsed, in an inimical sign, or malefic-afflicted: loss of land and a life of servitude -- the same applies if it conjoins the 8th lord here in a malefic Shashtyamsa.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (10,5,'BASELINE', N'Shines as a broker, engaging in speculation and similar business.', N'{"branch":"baseline"}'),
    (10,5,'WELL_DISPOSED', N'Joined by benefics here: a simple, pious life devoted to prayer, possibly heading an orphanage or similar institution (if placed 6th/8th/12th from Navamsa).', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires benefic association, a separate fact."}'),
    (10,6,'BASELINE', N'An occupation tied to the judiciary, prisons, or hospitals.', N'{"branch":"baseline"}'),
    (10,6,'WELL_DISPOSED', N'Aspected by benefics: a position of authority held in high esteem.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires benefic aspect, a separate fact."}'),
    (10,6,'AFFLICTED', N'Aspected by Saturn: a lifetime in a low-paying job; with Rahu or afflicted malefics: career disgrace, even imprisonment.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires identifying the specific aspecting planet."}'),
    (10,7,'BASELINE', N'A mature spouse who assists in the native''s work; foreign travel on diplomatic missions; skilled at negotiation; profit through partnerships.', N'{"branch":"baseline"}'),
    (10,7,'AFFLICTED', N'Malefic affliction: debased sexual conduct and vice.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (10,8,'BASELINE', N'Multiple career breaks.', N'{"branch":"baseline"}'),
    (10,8,'WELL_DISPOSED', N'Fortified: a high office, though only briefly.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (10,8,'AFFLICTED', N'Malefic affliction: criminal tendencies and offences; Jupiter''s influence here instead makes a mystic or spiritual teacher, while Saturn''s makes an undertaker or graveyard/cremation-ground worker.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires identifying which planet influences the 10th lord here."}'),
    (10,9,'BASELINE', N'A spiritual stalwart, a guiding light for seekers if Jupiter aspects the lord here; mixed benefic/malefic aspect still brings general fortune and a hereditary or teaching/healing profession, with strong paternal influence and charitable acts.', N'{"branch":"baseline","evaluableToday":"no","note":"Requires identifying aspecting planets on the 10th lord."}'),
    (10,10,'BASELINE', N'Strongly disposed in its own house: high professional success, respect and honour.', N'{"branch":"baseline","note":"A separate edge case: three or more planets conjoining the 10th lord here produces an ascetic instead."}'),
    (10,10,'AFFLICTED', N'Weak and afflicted: no self-respect, a lifelong dependent, fickle-minded; in the 6th/8th/12th from Navamsa: a routine, ordinary career.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (10,11,'BASELINE', N'Immense wealth, fortune in every respect, charitable activity, employer to many, widely respected and well-liked.', N'{"branch":"baseline"}'),
    (10,11,'AFFLICTED', N'Affliction to the 11th house: friends turn to enemies, bringing hardship and worry.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (10,12,'BASELINE', N'Work in a far-off place, lacking comforts and facing many difficulties.', N'{"branch":"baseline"}'),
    (10,12,'WELL_DISPOSED', N'Beneficially disposed: becomes a spiritual seeker.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (10,12,'AFFLICTED', N'Malefic affliction: separation from family and unsuccessful wandering, even smuggling or other illicit activity -- Rahu''s affliction specifically risks criminality and sorrow to the family.', N'{"branch":"afflicted","evaluableToday":"no","note":"Rahu-specific variant requires identifying the afflicting planet."}'),
    -- ===== 11th lord =====
    (11,1,'BASELINE', N'Born into a wealthy family and earns substantial wealth; the family''s relative wealth (very rich, fairly rich, or well-to-do) tracks the 11th lord''s own strength here; loses an elder sibling early in life.', N'{"branch":"baseline"}'),
    (11,2,'BASELINE', N'Lives with elder siblings.', N'{"branch":"baseline"}'),
    (11,2,'WELL_DISPOSED', N'Benefics present: harmonious relations; earns through commercial and banking business; business with friends brings good profit.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (11,2,'AFFLICTED', N'Malefics present: domestic friction (though still sharing a residence); malefic involvement in business-with-friends risks heavy losses.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (11,3,'BASELINE', N'Becomes a concert singer or musician and earns thereby; gains through siblings; has many friends and helpful neighbours.', N'{"branch":"baseline"}'),
    (11,3,'AFFLICTED', N'Afflictions reverse these results.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (11,4,'BASELINE', N'Profits through land, rentals and agricultural produce; a cultured, distinguished mother; renown for learning across subjects; a comfortable life with a devoted spouse.', N'{"branch":"baseline"}'),
    (11,5,'BASELINE', N'Many children who do well in life; gains through speculation.', N'{"branch":"baseline"}'),
    (11,5,'AFFLICTED', N'Afflicted: a gambler given to foolish ventures.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (11,5,'WELL_DISPOSED', N'Beneficially disposed: pious, observing vows that enhance prosperity.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (11,6,'BASELINE', N'Gains through maternal relatives, litigation, or running nursing homes.', N'{"branch":"baseline"}'),
    (11,6,'AFFLICTED', N'Afflicted in the 6th: a tendency to set people against each other and involve himself in others'' disputes and antisocial activity; further malefic affliction brings losses from the same sources.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (11,7,'BASELINE', N'Marries more than once; prospers in foreign countries.', N'{"branch":"baseline"}'),
    (11,7,'AFFLICTED', N'Afflictions: liaisons with disreputable women and involvement in immoral trade.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (11,7,'WELL_DISPOSED', N'Fortified instead: marries only once, to a rich and influential spouse.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (11,8,'BASELINE', N'Though born to wealth, suffers many setbacks and loses much of it, including to thieves and swindlers.', N'{"branch":"baseline"}'),
    (11,8,'AFFLICTED', N'A malefic constellation here: reduced to begging for a livelihood.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires the specific occupied nakshatra, not yet a compared fact."}'),
    (11,9,'BASELINE', N'Inherits substantial paternal fortune, very fortunate, owns many houses and every luxury; religious-minded, distributes religious literature, and is charitable.', N'{"branch":"baseline"}'),
    (11,10,'BASELINE', N'Prospers well in business, aided by the elder sibling; may earn prize recognition for original work in his field.', N'{"branch":"baseline","note":"Fair vs. foul means of earning depend on the lord''s benefic/malefic nature."}'),
    (11,11,'BASELINE', N'Many friends and elder siblings who help throughout life; a happy life with spouse, home, children and comfort.', N'{"branch":"baseline"}'),
    (11,12,'BASELINE', N'Business losses; an ailing elder sibling with heavy associated expense, possibly lost to death; frequent fines or penalties and heavy domestic responsibility.', N'{"branch":"baseline"}'),
    -- ===== 12th lord =====
    (12,1,'BASELINE', N'A weak constitution and feeble-mindedness, though handsome and sweet-spoken; a common sign here inclines to frequent travel.', N'{"branch":"baseline"}'),
    (12,1,'WELL_DISPOSED', N'The 6th lord joining the 12th lord in Lagna brings longevity, unless the 8th house is afflicted (then short-lived); also suggests imprisonment or living abroad.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires the 6th lord''s own placement too."}'),
    (12,1,'AFFLICTED', N'Lagna and 12th lord exchanging signs: miserliness, universal dislike, and lack of intelligence.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires detecting a Lagna/12th-lord sign exchange specifically."}'),
    (12,2,'BASELINE', N'Financial loss, possible debt and disreputable activity, irregular meals, poor eyesight, and disharmony at home.', N'{"branch":"baseline"}'),
    (12,2,'WELL_DISPOSED', N'A benefic, dignified 12th lord here greatly reduces these ills and brings financial stability and tactful speech.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (12,2,'AFFLICTED', N'An ill-disposed 12th lord instead brings gossip and quarrelling.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (12,3,'BASELINE', N'Timid and quiet; loss of a sibling; shabby dress; unsuccessful as a writer; low-earning, ordinary work.', N'{"branch":"baseline","note":"A special case: joined with the 2nd lord here and aspected by Jupiter or the 9th lord gives more than one spouse."}'),
    (12,3,'AFFLICTED', N'Malefic affliction: ear ailments and heavy spending on younger siblings.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (12,4,'BASELINE', N'Early death of the mother, mental restlessness and needless worry, enmity of relatives, living abroad, landlord harassment, and an ordinary residence.', N'{"branch":"baseline"}'),
    (12,4,'WELL_DISPOSED', N'A well-placed 12th lord mitigates these adverse indications considerably; a strong Venus adds a troublesome but owned vehicle.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (12,5,'BASELINE', N'Difficulty having children, or unhappiness from them; a religious turn including pilgrimage; weak-mindedness and a sense of misery; poor results in agriculture (pest- or disease-prone crops).', N'{"branch":"baseline"}'),
    (12,6,'BASELINE', N'A happy, prosperous, long life with many comforts, good health and physique, and victory over enemies -- though litigation may arise (resolving in the native''s favour).', N'{"branch":"baseline"}'),
    (12,6,'AFFLICTED', N'Malefic affliction: an unscrupulous, sinful, ill-tempered nature, hatred of the mother, unhappiness from his own children, and ruin through womanising.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (12,7,'BASELINE', N'A spouse from a poor family; an unhappy marriage possibly ending in separation, followed by a turn toward asceticism; weak health with phlegmatic trouble; lacking learning or property.', N'{"branch":"baseline"}'),
    (12,8,'BASELINE', N'Rich and celebrated, a luxurious life with many servants; gains through death or legacy; drawn to occult subjects and devoted to Vishnu; righteous, famous, and an eloquent, well-regarded speaker.', N'{"branch":"baseline"}'),
    (12,9,'BASELINE', N'Residence and prosperity abroad, acquiring property in foreign lands; honest, generous and large-hearted, though not especially spiritually inclined; some estrangement from spouse, friends and teacher, with an interest in physical culture; loses the father early.', N'{"branch":"baseline"}'),
    (12,10,'BASELINE', N'Hard-working, with tedious travel required for his occupation -- potentially as a jailer, doctor, or cemetery worker; profits from agricultural pursuits; little happiness or comfort from his sons.', N'{"branch":"baseline"}'),
    (12,11,'BASELINE', N'Engages in business without much profit; few friends but many enemies; troubled and drained financially by extravagant, sometimes invalid, siblings.', N'{"branch":"baseline"}'),
    (12,11,'WELL_DISPOSED', N'Earns well trading in pearls, rubies and other precious stones.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (12,12,'BASELINE', N'Spends heavily on religious and righteous causes; good eyesight, enjoys comforts, engaged in agriculture.', N'{"branch":"baseline"}'),
    (12,12,'AFFLICTED', N'Malefic affliction: restlessness and constant wandering.', N'{"branch":"afflicted","evaluableToday":"partial"}')
    ) v (OwnedHouseNumber, OccupiedHouseNumber, BranchCode, ClaimText, RequiredConditionsJson)
)
INSERT research.tbl_Dim_SourceReferenceHouseLordInHouseClaim
    (HouseLordInHouseId, ClaimCode, BranchCode, ClaimText, RequiredConditionsJson,
     EvidenceLevelCode, StatusCode, SourceTextId, SourceRefCode)
SELECT x.Id,
       CONCAT('HLP_H', RIGHT('0' + CAST(c.OwnedHouseNumber AS VARCHAR(2)), 2),
              '_H', RIGHT('0' + CAST(c.OccupiedHouseNumber AS VARCHAR(2)), 2), '_', c.BranchCode),
       c.BranchCode, c.ClaimText, c.RequiredConditionsJson,
       'DirectClassical', 'Proposed', t.Id, 'SRC_RAMAN_HTJH'
FROM ClaimDefs c
JOIN research.tbl_Dim_SourceReferenceHouse occ ON occ.HouseNumber = c.OccupiedHouseNumber
JOIN research.tbl_Dim_SourceReferenceHouse owned ON owned.HouseNumber = c.OwnedHouseNumber
JOIN research.tbl_Dim_SourceReferenceHouseLordInHouse x
    ON x.OwnedHouseId = owned.Id AND x.OccupiedHouseId = occ.Id
JOIN research.tbl_Dim_SourceReferenceHouseLordInHouseText t
    ON t.HouseLordInHouseId = x.Id AND t.SourceRefCode = 'SRC_RAMAN_HTJH'
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferenceHouseLordInHouseClaim y
    WHERE y.HouseLordInHouseId = x.Id
      AND y.ClaimCode = CONCAT('HLP_H', RIGHT('0' + CAST(c.OwnedHouseNumber AS VARCHAR(2)), 2),
              '_H', RIGHT('0' + CAST(c.OccupiedHouseNumber AS VARCHAR(2)), 2), '_', c.BranchCode)
      AND y.SourceRefCode = 'SRC_RAMAN_HTJH'
);

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'092_seed_house_lord_in_house_seventh_through_twelfth.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'092_seed_house_lord_in_house_seventh_through_twelfth.sql', N'Seed the 7th-12th lord combinations (6x12=72): Raman/PVR source pointers plus paraphrased BASELINE/WELL_DISPOSED/AFFLICTED claims, from Vol. II. Completes the 12x12 house-lord-placement research grid.');
