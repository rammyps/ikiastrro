/* House-lord-in-house: 2nd through 6th lords, each across all 12 occupied houses (5x12 = 60
   combinations). Paraphrased (not transcribed) from SRC_RAMAN_HTJH Vol. I, "Results of Lord
   of [Nth] House Occupying/Being Situated in Different Houses" -- the chapters "Concerning
   the Second/Third/Fourth/Fifth/Sixth House". Whole-sign houses, Lagna reference point, per
   the project's 2026-09-07 house-system decision. Continues the Lagna-lord pilot (090).
   BASELINE = the source's general/unqualified result; WELL_DISPOSED / AFFLICTED = its own
   explicitly separate qualified branches, where the source gives them (not every house gets
   every branch -- omission means the source did not state a distinct branch there). This is
   a reference pointer + paraphrase, not a copy of the copyrighted text. */

-- 1) Combination dimension: owned houses 2-6 x each occupied house.
INSERT research.tbl_Dim_SourceReferenceHouseLordInHouse (OwnedHouseId, OccupiedHouseId)
SELECT owned.Id, occ.Id
FROM research.tbl_Dim_SourceReferenceHouse owned
CROSS JOIN research.tbl_Dim_SourceReferenceHouse occ
WHERE owned.HouseNumber BETWEEN 2 AND 6
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
JOIN research.tbl_Dim_SourceReferenceHouse owned ON owned.Id = x.OwnedHouseId AND owned.HouseNumber BETWEEN 2 AND 6
CROSS APPLY (VALUES
    ('SRC_RAMAN_HTJH', N'How to Judge a Horoscope (vols I-II)', N'B. V. Raman', NULL,
     CASE owned.HouseNumber
        WHEN 2 THEN N'Concerning the Second House' WHEN 3 THEN N'Concerning the Third House'
        WHEN 4 THEN N'Concerning the Fourth House' WHEN 5 THEN N'The Fifth House'
        WHEN 6 THEN N'Concerning the Sixth House' END,
     CASE owned.HouseNumber
        WHEN 2 THEN N'Vol. I, scan pp. 90-94' WHEN 3 THEN N'Vol. I, scan pp. 133-136'
        WHEN 4 THEN N'Vol. I, scan pp. 169-170' WHEN 5 THEN N'Vol. I, scan pp. 214-217'
        WHEN 6 THEN N'Vol. I, scan pp. 254-257' END,
     'SummaryOnly', N'RAM-H3 family; docs/research/domain/house-placement.md starter notes',
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

-- 3) Claims: paraphrased BASELINE / WELL_DISPOSED / AFFLICTED branches per combination.
;WITH ClaimDefs (OwnedHouseNumber, OccupiedHouseNumber, BranchCode, ClaimText, RequiredConditionsJson) AS
(
    SELECT * FROM (VALUES
    -- ===== 2nd lord =====
    (2,1,'BASELINE', N'The native becomes wealthy but dislikes his own family; poor manners, passionate, subservient, time-serving.', N'{"branch":"baseline"}'),
    (2,1,'WELL_DISPOSED', N'Exalted or otherwise fortified in Lagna (with a 9th-lord/Sun link), earns through effort, intelligence and learning, or inherits wealth -- can reach yogakaraka strength.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (2,1,'AFFLICTED', N'Malefic aspects here bring loss of wealth, poor food or family health, or marital discord.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (2,2,'BASELINE', N'Becomes proud; may marry two or three times depending on 7th-house strength; may end up childless.', N'{"branch":"baseline"}'),
    (2,2,'WELL_DISPOSED', N'Well fortified (benefic-joined or -aspected, favourable constellation), earns considerable fortune through business.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (2,2,'AFFLICTED', N'Malefic aspects bring financial loss and poor food or family health.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (2,3,'BASELINE', N'Brave, intelligent, good-natured but morally depraved; atheistic tendencies and drawn to luxury; becomes miserly later in life.', N'{"branch":"baseline"}'),
    (2,3,'WELL_DISPOSED', N'Well fortified, benefits through sisters and gains skill in music or dance -- though also inclined toward propitiating minor spirits.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (2,4,'BASELINE', N'Spends freely on his own happiness while otherwise highly frugal with money.', N'{"branch":"baseline"}'),
    (2,4,'WELL_DISPOSED', N'Well fortified, prospers as an automobile dealer or agent, agriculturist, landlord or commission agent, and benefits through maternal relations.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (2,4,'AFFLICTED', N'If the 4th lord is afflicted, losses occur through that same maternal-relation channel.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires 4th-lord affliction, a second placement fact, not just this one."}'),
    (2,5,'BASELINE', N'Dislikes family, sensual, ungenerous even toward children, lacking manners.', N'{"branch":"baseline"}'),
    (2,5,'WELL_DISPOSED', N'Well fortified, brings unexpected wealth through lotteries, competitions, or royal favour.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (2,6,'BASELINE', N'Income and expenditure both come through enemies; defects or disease in the anus and thighs.', N'{"branch":"baseline"}'),
    (2,6,'WELL_DISPOSED', N'Well fortified here, wealth accumulates through disreputable means (black-marketing, deceit, manipulating relationships).', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (2,6,'AFFLICTED', N'Afflicted instead, the native is caught up in and prosecuted for such troubles (breach of trust, forgery, perjury).', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (2,7,'BASELINE', N'Inclined toward healing; laxity of morals in both spouses; overspends on sensory gratification.', N'{"branch":"baseline"}'),
    (2,7,'WELL_DISPOSED', N'Strong here with a strong 7th lord, wealth flows from foreign sources and business abroad; a feminine sign or nakshatra brings benefit through women.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires a second placement fact (7th lord strength), not evaluable from this row alone."}'),
    (2,8,'BASELINE', N'Little or no happiness from the spouse; discord with elder brothers; gains landed property.', N'{"branch":"baseline"}'),
    (2,8,'WELL_DISPOSED', N'Strong here, wealth flows in and out -- the source notes this combination rarely lets inherited or accumulated wealth persist regardless.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (2,9,'BASELINE', N'Skilful; poor health in youth, healthy afterwards; accumulates wealth and becomes happy.', N'{"branch":"baseline"}'),
    (2,9,'WELL_DISPOSED', N'Well fortified with the 9th lord in Lagna, brings good inheritance and benefits through varied sources depending on the sign and nakshatra involved.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires the 9th lord''s own placement, a second fact."}'),
    (2,10,'BASELINE', N'Respected by elders and superiors; learned, wealthy, self-made through varied useful pursuits (business, agriculture, philosophical lecturing).', N'{"branch":"baseline"}'),
    (2,10,'AFFLICTED', N'Strong affliction reverses these same income sources into loss.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (2,11,'BASELINE', N'Poor health in childhood; earns considerable wealth but becomes unscrupulous.', N'{"branch":"baseline"}'),
    (2,11,'WELL_DISPOSED', N'Well fortified, earns as a moneylender, banker, or by running a boarding house.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (2,12,'BASELINE', N'Becomes a respectable man, likely a government servant, but denied the happiness of an elder brother; income comes through ecclesiastical sources.', N'{"branch":"baseline"}'),
    (2,12,'AFFLICTED', N'If the 2nd lord is afflicted, that same ecclesiastical-source income is instead lost.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    -- ===== 3rd lord =====
    (3,1,'BASELINE', N'Self-made, vindictive, lean and tall, brave and courageous, but frequently unwell and in service to others.', N'{"branch":"baseline"}'),
    (3,1,'WELL_DISPOSED', N'Well fortified, becomes expert in dance, music or acting and earns a reputation as a performer.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (3,2,'BASELINE', N'An unfavourable position unless offset by other combinations -- unscrupulous, covets others'' spouses and wealth, generally joyless; likely loses younger siblings.', N'{"branch":"baseline"}'),
    (3,3,'BASELINE', N'Brave, surrounded by friends and relatives, blessed with good children, wealthy, happy and content.', N'{"branch":"baseline"}'),
    (3,3,'WELL_DISPOSED', N'Well disposed in the 3rd, 6th or 11th, indicates several younger siblings -- though Mars, Saturn or the Sun here can instead cost a sibling''s life depending on placement.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (3,4,'BASELINE', N'Generally a happy, rich and learned life, though the spouse is cruel-hearted and mean.', N'{"branch":"baseline"}'),
    (3,4,'WELL_DISPOSED', N'Well fortified in the 4th (with weak Lagna/9th lords), siblings survive him; a strong 9th lord adds step-siblings.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires Lagna-lord and 9th-lord strength, separate facts."}'),
    (3,4,'AFFLICTED', N'A weak Mars here costs him his land, forcing him to live in others'' houses.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires Mars-specific strength, not this placement alone."}'),
    (3,5,'BASELINE', N'Little pleasure from children; financially comfortable; domestic friction.', N'{"branch":"baseline"}'),
    (3,5,'WELL_DISPOSED', N'Well disposed in the 5th, greatly benefited by siblings, prospers through large-scale agriculture or adoption into a wealthy family, and does well in government service.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (3,6,'BASELINE', N'Hatred of siblings and relatives and difficulty through them; becomes rich but at the cost of maternal relatives; accepts illegal gains.', N'{"branch":"baseline"}'),
    (3,6,'WELL_DISPOSED', N'Well disposed in the 6th, a younger sibling joins the army or becomes a successful physician; combined with the 6th lord, may produce a sportsman or athlete.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (3,6,'AFFLICTED', N'Both the 3rd and 6th afflicted together bring disease, torment from enemies, and a deceitful nature.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires the 6th lord''s own condition too."}'),
    (3,7,'BASELINE', N'May incur the displeasure of rulers or authorities; many ups and downs, a difficult childhood, an unfortunate union, and danger while travelling.', N'{"branch":"baseline"}'),
    (3,7,'WELL_DISPOSED', N'Well fortified in the 7th, cordial relations between siblings; a strong 7th lord in Lagna has a sibling settle and prosper abroad, aiding the native.', N'{"branch":"well_disposed","evaluableToday":"no","note":"Requires the 7th lord''s own placement/strength too."}'),
    (3,8,'BASELINE', N'Involvement in a criminal matter or false accusation; trouble from a death or bequest; an unfortunate marriage and a rocky career.', N'{"branch":"baseline"}'),
    (3,8,'AFFLICTED', N'In the 8th specifically, a serious, dangerous illness and the loss of a younger sibling.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (3,9,'BASELINE', N'Fortune improves after marriage; an untrustworthy father; long journeys; sudden life changes.', N'{"branch":"baseline"}'),
    (3,9,'WELL_DISPOSED', N'Favourably disposed in the 9th, a sibling inherits ancestral property and the native benefits from that sibling.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (3,9,'AFFLICTED', N'Afflicted here instead brings misunderstandings with the father.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (3,10,'BASELINE', N'A quarrelsome, unfaithful spouse; the native nonetheless becomes rich, happy and intelligent, gaining through career-related travel.', N'{"branch":"baseline"}'),
    (3,10,'WELL_DISPOSED', N'In the 10th, all siblings prosper and are helpful to the native.', N'{"branch":"well_disposed","evaluableToday":"partial"}'),
    (3,11,'BASELINE', N'Not a favourable combination -- effortful earnings, a vindictive nature, an unattractive or emaciated body, dependence on others, and frequent illness.', N'{"branch":"baseline"}'),
    (3,12,'BASELINE', N'Sorrow through relatives, some fortune through marriage, a tendency to seclusion, great ups and downs, an unscrupulous father.', N'{"branch":"baseline"}'),
    (3,12,'AFFLICTED', N'In the 12th specifically, the youngest sibling becomes a tyrant and impoverishes the native.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    -- ===== 4th lord (mostly single-branch in the source) =====
    (4,1,'BASELINE', N'Highly learned but afraid of public speaking; likely loses inherited wealth; family fortune (rich, mediocre or poor) tracks the 4th lord''s own strength here.', N'{"branch":"baseline","note":"Source states results scale generally with the 4th lord''s own strength/affliction, not house-specific unless noted."}'),
    (4,2,'BASELINE', N'Highly fortunate, courageous and happy, with a sarcastic streak; inherits property from the maternal grandfather.', N'{"branch":"baseline"}'),
    (4,3,'BASELINE', N'Sickly but generous and principled, self-made -- though troubled by the machinations of step-siblings and a step-mother.', N'{"branch":"baseline"}'),
    (4,4,'BASELINE', N'Religiously inclined with respect for tradition; rich, respected, happy, and sensual.', N'{"branch":"baseline"}'),
    (4,5,'BASELINE', N'Loved and respected, devoted to Vishnu, self-made wealth; the mother is from a respectable family; acquires vehicles.', N'{"branch":"baseline"}'),
    (4,6,'BASELINE', N'Short-tempered and mean, given to deceit and ill intentions; frequently on the move.', N'{"branch":"baseline"}'),
    (4,7,'BASELINE', N'Generally happy, commanding houses and land; livelihood is earned far from or near the birthplace depending on whether the 7th is a movable or fixed sign.', N'{"branch":"baseline"}'),
    (4,8,'BASELINE', N'A miserable life; the father dies early; either impotence or looseness in sexual conduct; likely loss of land or litigation.', N'{"branch":"baseline"}'),
    (4,9,'BASELINE', N'Generally a fortunate combination, favouring happiness through the father and property.', N'{"branch":"baseline"}'),
    (4,10,'BASELINE', N'Political success, skill as a chemist, triumph over enemies and a strong public personality.', N'{"branch":"baseline"}'),
    (4,10,'AFFLICTED', N'An afflicted 4th lord here instead risks loss of reputation.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (4,11,'BASELINE', N'Self-made and generous but sickly; a fortunate mother (possibly also a step-mother); favours success trading cattle and land.', N'{"branch":"baseline"}'),
    (4,12,'BASELINE', N'Deprived of happiness and property; early death of the mother; poor finances; a generally miserable existence.', N'{"branch":"baseline"}'),
    -- ===== 5th lord (favourable / afflicted, sometimes a third moderate state noted only in JSON) =====
    (5,1,'BASELINE', N'Commands servants and may become a judge, magistrate or minister empowered to punish wrongdoing; earns divine favour; few children; has rivals but is generous to others.', N'{"branch":"baseline"}'),
    (5,1,'AFFLICTED', N'No children; drawn to invoking destructive occult forces, becomes malicious and leads a gang of deceitful people, a stinging gossip.', N'{"branch":"afflicted","evaluableToday":"partial","note":"Source also gives a third, moderately-good, mixed-result state between these two."}'),
    (5,2,'BASELINE', N'Favourably disposed: a beautiful spouse and well-behaved children, gains from government or royalty, becomes learned, a good astrologer.', N'{"branch":"baseline"}'),
    (5,2,'AFFLICTED', N'Weak and afflicted: poverty, loss of money through government displeasure, unable to support the family, domestic strife, becomes a temple priest.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (5,3,'BASELINE', N'Favourably disposed: many good children and siblings.', N'{"branch":"baseline"}'),
    (5,3,'AFFLICTED', N'Unfavourably disposed: loss of children, discord with siblings, ongoing occupational trouble, becomes stingy and given to gossip.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (5,4,'BASELINE', N'Favourably disposed: a few sons (one an agriculturist), a long-lived mother, possibly an adviser to a ruler.', N'{"branch":"baseline","note":"A separate moderate state gives daughters and no sons."}'),
    (5,4,'AFFLICTED', N'Afflicted: death of children.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (5,5,'BASELINE', N'Favourably disposed: several sons, prominence in his own field, or expertise in Mantra-shastra or mathematics, or heads a religious institution, with influential friends.', N'{"branch":"baseline"}'),
    (5,5,'AFFLICTED', N'Afflicted: the opposite results -- children die, unreliable in word, a wavering, cruel disposition.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (5,6,'BASELINE', N'Favourably disposed: the maternal uncle becomes a famous man, though there is enmity with the native''s own son.', N'{"branch":"baseline"}'),
    (5,6,'AFFLICTED', N'Afflicted: no children are born, or one is adopted from the maternal uncle''s line.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (5,7,'BASELINE', N'Favourably disposed: a son lives abroad and attains distinction, wealth and fame, or the native has several children; renowned, learned, prosperous, devoted to his teacher, with a charming personality.', N'{"branch":"baseline"}'),
    (5,7,'AFFLICTED', N'Afflicted: loss of children, including one who dies abroad after achieving name and fame.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (5,8,'BASELINE', N'Loss of paternal property through debt; the family line risks extinction; suffers lung trouble; peevish and unhappy, though not poor.', N'{"branch":"baseline"}'),
    (5,9,'BASELINE', N'Becomes a teacher or preceptor; renovates old temples, wells and gardens; a son gains distinction as an orator or author.', N'{"branch":"baseline"}'),
    (5,9,'AFFLICTED', N'Afflicted: earns divine displeasure and consequent ruin of fortune.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (5,10,'BASELINE', N'Beneficially disposed, forms a Raja Yoga: acquires land, earns royal goodwill, builds temples, performs religious rites; a son attains prominence, possibly in intelligence work if aspected by the Sun.', N'{"branch":"baseline"}'),
    (5,10,'AFFLICTED', N'Afflicted: faces the wrath of rulers and otherwise contrary results.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (5,11,'BASELINE', N'Benefits through sons and success in all undertakings; becomes rich and learned and helps others; has several sons; becomes an author.', N'{"branch":"baseline"}'),
    (5,12,'BASELINE', N'A pronounced quest for ultimate knowledge; leads a life of non-attachment, becomes spiritual, moves between places, and ultimately attains liberation (Moksha).', N'{"branch":"baseline"}'),
    -- ===== 6th lord (fortified / afflicted) =====
    (6,1,'BASELINE', N'Well aspected: may join the army as soldier or commander, or become a war minister or prison official; lives in the maternal uncle''s house.', N'{"branch":"baseline"}'),
    (6,1,'AFFLICTED', N'Weak and afflicted: becomes a robber, thief, or leader of a criminal gang.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (6,2,'BASELINE', N'Even when conjoined with or aspected by benefics, the source describes deep family suffering and sorrow, financial loss through enemies, poor eyesight, uneven teeth, a stammer -- the 6th lord''s own dusthana nature dominates here.', N'{"branch":"baseline","note":"Text states this outcome under a nominally benefic association; preserved as stated rather than reconciled."}'),
    (6,2,'AFFLICTED', N'Weak and ill-disposed: loss of the spouse during a malefic dasha or bhukti; if Venus is weak, celibacy and poverty.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires dasha timing and Venus condition, not this placement alone."}'),
    (6,3,'BASELINE', N'Fortified: enmity with siblings, or the maternal uncle''s friendship with a sibling works against the native, or a sibling suffers frequent ill health.', N'{"branch":"baseline"}'),
    (6,3,'AFFLICTED', N'Weak and afflicted: no younger siblings.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (6,4,'BASELINE', N'Well fortified: lives in a dilapidated building, interrupted education, estrangement from the mother; maternal uncles tend to be farmers.', N'{"branch":"baseline"}'),
    (6,4,'AFFLICTED', N'Weak and afflicted: conflict with the mother, ancestral property burdened by debt, menial work, a troubled household including trouble through servants.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (6,5,'BASELINE', N'Sickly children; the native is adopted by the maternal uncle and becomes fortunate through that.', N'{"branch":"baseline"}'),
    (6,6,'BASELINE', N'More cousins; the maternal uncle becomes renowned.', N'{"branch":"baseline"}'),
    (6,6,'AFFLICTED', N'Conjoined with a weak Lagna lord: an incurable illness and rising enmity with relatives.', N'{"branch":"afflicted","evaluableToday":"no","note":"Requires Lagna-lord strength too."}'),
    (6,7,'BASELINE', N'Typically marries a cousin (maternal or paternal aunt''s/uncle''s daughter); the maternal uncle lives far away or abroad; the spouse''s character is doubtful.', N'{"branch":"baseline"}'),
    (6,7,'AFFLICTED', N'Afflicted: early divorce or the spouse''s death; a hermaphrodite Rasi/Navamsa combination gives a sickly or barren spouse; troubles with disreputable women.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (6,8,'BASELINE', N'Fortified: a middling lifespan (Madhyayu).', N'{"branch":"baseline"}'),
    (6,8,'AFFLICTED', N'Afflicted: heavy debts, loathsome disease, pursuit of women outside the marriage, and taking pleasure in others'' pain.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (6,9,'BASELINE', N'Well fortified: the father becomes a judge; the maternal uncle becomes very fortunate; some discord with the father; gains through cousins or relatives.', N'{"branch":"baseline","note":"A separate moderate state (mason, timber merchant, stone cutter) also given."}'),
    (6,9,'AFFLICTED', N'Afflicted: poverty, sinful acts, misfortune through relatives, ingratitude toward teachers.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (6,10,'BASELINE', N'Fortified: a sinful, destructive nature masked as orthodox piety -- genuinely unscrupulous in religious matters.', N'{"branch":"baseline"}'),
    (6,10,'AFFLICTED', N'Weak: dismissal by formidable enemies, a low life, or reduced to begging.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (6,11,'BASELINE', N'Benefic: the eldest sibling becomes a judge.', N'{"branch":"baseline","note":"A separate ordinary state has the sibling lose that post."}'),
    (6,11,'AFFLICTED', N'Malefic: a poor, wretched life, suffering from convictions.', N'{"branch":"afflicted","evaluableToday":"partial"}'),
    (6,12,'BASELINE', N'Well disposed: still brings difficulty and sorrow through a destructive tendency that harms others.', N'{"branch":"baseline"}'),
    (6,12,'AFFLICTED', N'Afflicted: a miserable, hard, wretched existence.', N'{"branch":"afflicted","evaluableToday":"partial"}')
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
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'091_seed_house_lord_in_house_second_through_sixth.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'091_seed_house_lord_in_house_second_through_sixth.sql', N'Seed the 2nd-6th lord combinations (5x12=60): Raman/PVR source pointers plus paraphrased BASELINE/WELL_DISPOSED/AFFLICTED claims, from Vol. I.');
