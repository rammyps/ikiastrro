-- =====================================================================
-- 114 - Seed research.tbl_Dim_SourceReferencePlanetInHouse{Text,Claim}
-- with real content: all 108 (9 graha x 12 house) paraphrased
-- interpretations from B.V. Raman, "How to Judge a Horoscope" (Vol I
-- houses 1-6, Vol II houses 7-12), citing SRC_RAMAN_HTJH.
--
-- Corrects migrations 064/065: those seeded citation-pointer Text rows
-- for SRC_BVRAMAN_PLANET_IN_HOUSE, which turned out on inspection to be
-- a DIFFERENT, unregistered Raman book ("Hindu Predictive Astrology," not
-- "How to Judge a Horoscope"). This migration does not touch those old
-- rows - it adds fresh Text + Claim rows citing the correct, registered
-- SRC_RAMAN_HTJH instead, against the same combination dictionary
-- (research.tbl_Dim_SourceReferencePlanetInHouse, unchanged since 064).
--
-- Extracted by two read passes over the already-OCR'd extracts
-- (D:\@ClaudeSpace\BookExtracts\how-to-judge-a-horoscope-1.md and
-- -2-ocr.md), paraphrased (not verbatim - CopyrightStatus=SummaryOnly),
-- page-cited. Vol I is Tier-1 (running-prose) reliability per its own
-- front matter; Vol II is a rawer OCR pass ("prose requires passage
-- verification" per its own front matter) - flagged per-row in Notes,
-- though no passage needed a [verify] flag on this pass.
--
-- StatusCode = 'Proposed' throughout: a first transcription pass, not
-- yet checked against a second author - same honesty db/094's own
-- header states for the house-lord-placement precedent this migration
-- (+115) mirrors.
--
-- Apply:  sqlcmd -S localhost\SQLSERVER2025 -E -d ikiastrro -C -b -f 65001 -i db/114_seed_planet_in_house_claims.sql
-- =====================================================================
USE [ikiastrro];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

;WITH content (HouseNumber, PlanetCode, Page, Note) AS (
    SELECT * FROM (VALUES
    -- House 1 (Vol I, pg 50-52)
    (1,'PLANET_SUN',50,N'Gives a strong moral, righteous, ambitious nature backed by good health and vitality, adding respect and popularity; if joined with Saturn or Mars, brings scars, impure blood, skin eruptions, fevers and eye trouble.'),
    (1,'PLANET_MOON',50,N'Makes one fanciful, romantic, a moderate eater, restless yet easygoing, with a fluctuating fortune and love of travel; combination with Saturn brings mental worry, with Mars menstrual trouble in women, with Rahu hysteria, and with Jupiter an elevated mind.'),
    (1,'PLANET_MARS',50,N'Gives a hot, courageous constitution with self-confidence and enterprise; brings recklessness, scars and accident-proneness, with domestic unhappiness unless offset by other favorable factors.'),
    (1,'PLANET_MERCURY',51,N'Produces a witty, mentally sharp, well-read and adaptable nature especially drawn to occult subjects; good aspect to Venus adds musical talent, while Rahu or Ketu in the sign cause nervous troubles.'),
    (1,'PLANET_JUPITER',51,N'Bestows a magnetic, optimistic, jovial personality with good manners and leadership potential and favors many sons if the 5th house is unafflicted; can bring corpulence and health issues from self-indulgence, and with Rahu tempts one into wrongdoing.'),
    (1,'PLANET_VENUS',51,N'A fortunate placement giving charm, amiability, artistic taste and strong attraction to the opposite sex, often with early marriage; affliction brings marital discord.'),
    (1,'PLANET_SATURN',51,N'Gives a calm, grave, self-disciplined nature inclined to adopt foreign customs and consider others'' welfare, but with a weak, emaciated body, slow but steady progress, and possible early-life misfortune through negligence.'),
    (1,'PLANET_RAHU',52,N'Brings generally poor health needing unconventional treatment, a secretive, eccentric and hypocritical nature drawn to occult matters, and difficulties in marriage.'),
    (1,'PLANET_KETU',52,N'Gives psychic sensitivity but a frail, emaciated body, an unstable, deceitful character with strange appetites and excitability, and an unhappy married life unless offset by favorable factors.'),
    -- House 2 (Vol I, pg 98-99)
    (2,'PLANET_SUN',98,N'Not a very favorable placement - the person suffers losses from offending authorities and has a blemished face, though money is earned through hard, stubborn effort; the type of income depends on the sign involved.'),
    (2,'PLANET_MOON',98,N'Brings a large family and much happiness, with money coming through women though finances fluctuate; older authorities add that such natives can be reserved, unsociable and squint-eyed yet much admired.'),
    (2,'PLANET_MARS',98,N'Makes one quarrelsome and miserly despite strong earning power and accumulated wealth; a good conversationalist who nonetheless keeps bad company and picks quarrels.'),
    (2,'PLANET_MERCURY',99,N'Confers learning in religion and philosophy, gains through speaking, trade and commerce, high intelligence and wealth, along with a charitable and thrifty disposition.'),
    (2,'PLANET_JUPITER',99,N'Produces a poet, great writer, astrologer or scientist with rising success, accumulated fortune, a good spouse and family, and gains through matters ruled by Jupiter; keeps the person from quarreling.'),
    (2,'PLANET_VENUS',99,N'Indicates a large family, easy income through others'' favor, good food and conveyances, an attractive and skillful nature, and a good marriage, with health and wealth both favored.'),
    (2,'PLANET_SATURN',99,N'Unless the sign is Libra, Capricorn or Aquarius, makes earning an uphill struggle with much labor for little gain, harsh speech, unsociability and missed opportunities, plus domestic unhappiness, though gains may come through metals, storage, mining or labor.'),
    (2,'PLANET_RAHU',99,N'Causes a peevish nature, facial blemish and family friction along with danger to eyesight; finances are uncertain unless Jupiter aspects the house, in which case gains come through friends and business.'),
    (2,'PLANET_KETU',99,N'Makes one a poor speaker prone to loss through fraud and deception, with unstable finances, though it can bring success in spiritual, nautical or mystical pursuits.'),
    -- House 3 (Vol I, pg 138-140)
    (3,'PLANET_SUN',138,N'Makes the person courageous, resourceful and successful, though harmful to siblings if afflicted and prone to trouble through correspondence; considered a strong placement overall.'),
    (3,'PLANET_MOON',138,N'Brings frequent changes of occupation, love of travel, an active mind, a fair-complexioned wife and good knowledge, with devotion to children but less concern for spirituality; a waning Moon here makes one cruel, miserable and unscrupulous.'),
    (3,'PLANET_MARS',138,N'Harmful to siblings and prone to accidents while traveling; gives bravery but also recklessness and possible ear trouble, with further affliction suggesting violent or suicidal tendencies (much reduced if the third sign is Capricorn, Aries or Scorpio).'),
    (3,'PLANET_MERCURY',139,N'Gives benevolence toward others though not much personal happiness, a sharp and persistent mind, tact and diplomacy, and success in trade and speculation aided by many siblings and friends; affliction brings nervous breakdown.'),
    (3,'PLANET_JUPITER',139,N'A favorable placement giving an optimistic, philosophical mind and good brothers, though it can make one miserly, unattached to family, and prone to poor health; affliction reduces gratitude and friendships.'),
    (3,'PLANET_VENUS',139,N'Gives good mental qualities but poor health and vitality, a love of music, dancing and the arts, and limited financial success along with good siblings; affliction brings meanness, sensuality and interest in scandal.'),
    (3,'PLANET_SATURN',139,N'Gives bravery, wealth and honor from rulers despite loss of siblings and an eccentric, cruel streak; success comes only after early hardship, with a gloomy mind that improves with age (worse if afflicted).'),
    (3,'PLANET_RAHU',140,N'Gives outward bravery, sudden unexpected news, and generally poor relations with siblings, along with criticism over one''s opinions and ideas.'),
    (3,'PLANET_KETU',140,N'Gives a strong, adventurous but inwardly timid nature troubled by hallucinations.'),
    -- House 4 (Vol I, pg 172-174)
    (4,'PLANET_SUN',172,N'Generally causes unhappiness, mental worry and restlessness, though it promises some inheritance and interest in occult or philosophical study; political success is difficult, especially if Saturn or Mars afflicts the Sun.'),
    (4,'PLANET_MOON',172,N'Gives property, happiness from relatives, cheerfulness and leadership though somewhat quarrelsome; affliction brings early separation from the mother, and fondness for sensual pleasure unless Jupiter aspects.'),
    (4,'PLANET_MARS',173,N'Generally an unfavorable combination causing loss of happiness from mother, relatives and friends and domestic quarrels despite political success; conjunction with Rahu or Ketu can incline one toward suicidal thoughts, and the person owns houses without enjoying them.'),
    (4,'PLANET_MERCURY',173,N'Gives success as an educator or diplomat with boldness in criticizing government, high esteem, a self-made father, good conveyances, artistic taste, love of travel and eloquence.'),
    (4,'PLANET_JUPITER',173,N'Gives a philosophical, learned and happy nature with royal favor, respect and fortune, a peaceful home and strong spiritual growth.'),
    (4,'PLANET_VENUS',173,N'Gives musical accomplishment, refined manners, deep attachment to the mother, many friends, houses and vehicles, religious inclination and fulfillment of desires, favoring domestic harmony.'),
    (4,'PLANET_SATURN',173,N'Causes sickliness in early years and loss of the mother''s happiness, windy or phlegmatic ailments, a lethargic and reclusive nature, lack of inherited property, trouble with houses and vehicles, and strained relations with kin unless well aspected.'),
    (4,'PLANET_RAHU',174,N'Gives foolish behavior, few friends, and vulnerability to fraud or involvement in fraudulent dealings.'),
    (4,'PLANET_KETU',174,N'Causes loss of the mother, property and domestic happiness, and life in foreign lands, with unusual experiences and sudden reversals late in life.'),
    -- House 5 (Vol I, pg 218-220)
    (5,'PLANET_SUN',218,N'Deprives one of children, wealth and happiness, shortens life, brings heart disease and a tendency to wander in forests or mountains, and denotes difficult childbirth.'),
    (5,'PLANET_MOON',219,N'Gives clarity of mind, happiness through children, gains of land and gems, opportunity for state service, honesty, learning and an enemy-free nature, along with a strong bent for speculation; one child becomes notably famous.'),
    (5,'PLANET_MARS',219,N'Brings misery through wife, friends and children, a disturbed, rash and back-biting temperament, colic and misfortune connected with offspring, plus health decline from excess sexual indulgence; can indicate difficult childbirth in a woman''s chart.'),
    (5,'PLANET_MERCURY',219,N'Gives learning, happiness and many children, potential to become an adviser or minister, and high intelligence and knowledge of sacred texts, though excessive sexual indulgence saps vitality.'),
    (5,'PLANET_JUPITER',219,N'Gives deep learning in law and sacred lore, high intelligence, service as a royal adviser, good friends and conveyances, refined manners, many children, piety, and happiness through offspring and friends.'),
    (5,'PLANET_VENUS',219,N'Gives poetic talent, many friends and beautiful children, happiness through offspring, wisdom and wealth and honor from the state; favors daughters and success in speculation.'),
    (5,'PLANET_SATURN',219,N'Gives an unwise, sickly, poor and disliked nature with sorrow through children, unstable fortune, hypocrisy, and quarrels with friends and family.'),
    (5,'PLANET_RAHU',220,N'Brings colic ailments, being misunderstood and friendless, loss of several children, a hard-hearted and unconventional nature, and heart trouble.'),
    (5,'PLANET_KETU',220,N'Brings loss of children and stomach trouble along with unusual emotional experiences, though it inclines one toward spirituality later in life.'),
    -- House 6 (Vol I, pg 259-260)
    (6,'PLANET_SUN',259,N'Makes one a successful, famous politician though health suffers; well-placed it brings administrative skill, wealth and few enemies, while affliction (especially by Saturn) risks prolonged illness or heart trouble unless eased by Jupiter''s aspect.'),
    (6,'PLANET_MOON',259,N'Indicates poor health (Balarishta) in early childhood, with affliction by Mars and Saturn bringing baffling diseases and vengeful enemies; otherwise favors success in subordinate roles, though depending on the sign it may bring bladder stones, timidity toward women, or lung trouble.'),
    (6,'PLANET_MARS',259,N'Gives passion and success as a ruler or leader despite worries from close relatives; affliction brings accidents and trouble through employees, with the specific afflicting planet determining the manner of danger (surgery/injury, suicide, or poisoning).'),
    (6,'PLANET_MERCURY',260,N'Gives a quarrelsome yet respected nature with interrupted education; affliction, especially combined with Mars, Saturn or Rahu, risks nervous or mental breakdown, servant troubles and laziness, though the person remains formidable to enemies.'),
    (6,'PLANET_JUPITER',260,N'Gives an inactive, disrespected nature drawn to black magic and feared by enemies, though generally healthy unless affliction causes health issues from overindulgence.'),
    (6,'PLANET_VENUS',260,N'Gives freedom from enemies and favors from women, though affliction leads to health decline from sexual excess and a licentious nature.'),
    (6,'PLANET_SATURN',260,N'Gives a quarrelsome, stubborn, hearty-eating but courageous and enemy-free nature; affliction brings hardship-related illness and trouble from subordinates, with the specific affliction determining the type of danger, while good aspects bring gains through contracting, mining or masonry.'),
    (6,'PLANET_RAHU',260,N'Gives longevity and wealth but trouble from enemies, supernatural fears and private-parts ailments, with risk of mental derangement if Moon and Saturn join it, plus scandal in personal life.'),
    (6,'PLANET_KETU',260,N'Considered its best house placement, giving fame, authority and freedom from enemies along with intuitive or occult powers, despite a loose moral character.'),
    -- House 7 (Vol II, pg 16-18)
    (7,'PLANET_SUN',16,N'Fair-skinned with thinning hair, he has few friends and struggles socially; marriage is delayed and troubled, he is a fond traveller with loose morals and a liking for foreign things, his wife''s character is questionable, and he risks loss and disgrace through women along with trouble from the government and possible deformity.'),
    (7,'PLANET_MOON',16,N'Passionate and easily roused to jealousy, he may lose his mother young, and though his wife is good-looking he seeks other women; narrow-minded yet sociable, energetic and successful (more so if the Moon is waxing/strong, from a good family), he suffers groin pain and stinginess, and if waning quarrels constantly with enemies.'),
    (7,'PLANET_MARS',17,N'Henpecked and submissive to his wife, his married life is full of clashes and tension (or he has two wives); rash and given to speculation, he is intelligent yet tactless, stubborn, peevish and generally unsuccessful.'),
    (7,'PLANET_MERCURY',17,N'A virtuous, well-dressed and genial man with deep knowledge of law and skill in business, he gains early success and marries a rich woman young; learned in mathematics, astrology and astronomy, religious and diplomatic, though if afflicted he turns cunning and deceitful despite good looks and physique.'),
    (7,'PLANET_JUPITER',17,N'Diplomatic and kind-hearted, he gets a virtuous, good-looking and chaste wife along with a good education and gains through marriage; sensitive to others'' feelings, speculative-minded and a good agriculturist, he undertakes pilgrimages, surpasses his father in qualities, and has good sons.'),
    (7,'PLANET_VENUS',17,N'Quarrelsome, sensuous and passionate with unhealthy habits, he nonetheless has a happy marriage and devoted wife; fond of pleasure and drink, suave and magnetic, he risks loss of virility through disease or excess but succeeds in partnerships with the opposite sex.'),
    (7,'PLANET_SATURN',17,N'Dominated by his wife (who may be unattractive or hunchbacked), he tends to marry more than once or wed a widow, divorcee, or older woman; diplomatic and enterprising, he gains a stable marriage, residence abroad, political success and honours in foreign lands, but suffers colic pains and deafness.'),
    (7,'PLANET_RAHU',18,N'If female, she brings ill-repute to the family; unconventional and heterodox, he has affairs with outcaste or foreign women, his wife suffers womb disorders, and he indulges in rich food and luxury while suffering from diabetes and troubles from ghosts or the supernatural.'),
    (7,'PLANET_KETU',18,N'His marriage is unhappy with a shrewish and sickly wife, being himself passionate, sinful and inclined to lust after widows; he suffers abdominal or uterine cancer (if female) along with humiliation and loss of virility.'),
    -- House 8 (Vol II, pg 92-95)
    (8,'PLANET_SUN',92,N'If exalted in the 8th, he lives long and is a charming, eloquent speaker; if afflicted, he suffers facial and head sores, weak eyes, penury and an uneventful life, though association with the 8th or 11th lord can bring sudden gains through speculation, and progeny is limited and mostly male.'),
    (8,'PLANET_MOON',93,N'Prone to mental aberration, apprehension and psychological complexes, he is capricious, unhealthy and of slender build with weak eyesight, possibly losing his mother in childhood; he gains easily through legacies or inheritance, is fond of fighting and amusement, large-hearted, and suffers excessive perspiration.'),
    (8,'PLANET_MARS',93,N'Short-lived unless mitigated by other factors, he may lose his spouse and have very few children, seeking extra-marital gratification while hating relatives; his domestic life is marred by quarrels and blood-related ailments like piles, though he rules over many people.'),
    (8,'PLANET_MERCURY',93,N'Possessing many good qualities and known for good breeding and courtesy, he inherits and earns much wealth, becomes a learned and famous scholar, and lives long, though with a weak constitution.'),
    (8,'PLANET_JUPITER',94,N'Unhappy but generous-hearted, he lives long yet has difficulty in speech, may commit ignoble deeds while affecting nobility, has liaisons with widows, keeps dirty habits, suffers colitis, and ultimately experiences a painless death.'),
    (8,'PLANET_VENUS',94,N'This placement brings many blessings, much wealth and a comfortable life; the mother may face danger, and early emotional disappointments may push the native toward piety later in life, with exaltation increasing wealth but debilitation combined with Saturn''s affliction bringing subordination and drudgery alongside the mother.'),
    (8,'PLANET_SATURN',94,N'Gives good longevity but many responsibilities that must be borne through sheer perseverance; the native has defective eyes, few children, a paunch, and a leaning toward women outside his caste, is prone to asthma, consumption and lung disorders, and if afflicted becomes dishonest and cruel with children who cause grief.'),
    (8,'PLANET_RAHU',95,N'Brings public censure and humiliation along with many ailments; the native is vicious, quarrelsome and unscrupulous, and certain afflictions involving the Moon can bring mental disorders.'),
    (8,'PLANET_KETU',95,N'If well-aspected, brings much wealth and long life; if afflicted, the native covets others'' wealth and women and suffers diseases of the excretory system as well as ailments from a profligate lifestyle.'),
    -- House 9 (Vol II, pg 193-195)
    (9,'PLANET_SUN',193,N'If afflicted, he may change his faith and shows hostility toward his father with little respect for elders or spiritual teachers; if unafflicted, he is a dutiful son inclined to spiritual pursuits, though combinations with the Moon or Venus bring eye trouble or sickness, health is average, patrimony small, and he remains ambitious and enterprising.'),
    (9,'PLANET_MOON',193,N'Fortunate and prosperous with many sons, friends and kinsmen, he is principled and generous; certain aspects can make him a ruler, though combination with Mars risks harming his mother and with Venus risks an immoral life involving his stepmother; Saturn''s presence brings suffering, but he may found charitable institutions, acquire property, and travel abroad.'),
    (9,'PLANET_MARS',194,N'Wields authority and affluence, has children and happiness, and though not a dutiful son is otherwise generous and admired; conjunction with Jupiter or Mercury brings religious learning, Venus brings two wives and foreign residence plus legal skill, while Saturn''s company brings a wicked, self-seeking, stubborn and impetuous nature with a taste for other women.'),
    (9,'PLANET_MERCURY',194,N'Acquires much education and wealth, becoming a great and famous scholar with an interest in theosophy and metaphysics and a scientific mind; Venus adds fondness for music and pleasure, Jupiter confers wit and wisdom along with invitations to travel and lecture abroad, and relations with the father remain friendly.'),
    (9,'PLANET_JUPITER',195,N'May become an exponent of law or philosophy, gaining much property if benefically aspected and showing fondness for his brothers; the Moon and Mars together can make him a military leader, the Sun and Venus together make him characterless, while Saturn''s benefic aspect pushes him toward austerity and spiritual communion, foreign travel as a preacher, and a conservative, principled nature.'),
    (9,'PLANET_VENUS',195,N'Born fortunate with fame, learning, children, a good wife and general happiness; the Sun''s influence adds polish to his speech but brings physical complaints, Saturn''s influence makes him a respected diplomat or government worker, but the Sun and Moon together can bring quarrels with women and financial loss, and the Sun and Saturn together can bring criminal tendencies, conviction, or a libertine reputation.'),
    (9,'PLANET_SATURN',195,N'Leads a lonely life and may not marry, though renowned for valour in battle; the Sun''s influence brings conflict with father and children plus stomach growths, Mercury''s influence brings dishonesty despite wealth, and he otherwise lives thriftily, somewhat irreligiously, sometimes founding charitable institutions.'),
    (9,'PLANET_RAHU',195,N'Has a nagging, domineering wife and is himself impolite and miserly, suffering emaciation and loose morals; he hates his father and reviles God and religion, yet may still become famous and wealthy.'),
    (9,'PLANET_KETU',195,N'Short-tempered and easily upset over trifles, he is eloquent but uses this to scandalize others, being haughty, arrogant and fond of pomp yet valorous; often hostile toward his parents, he is short-sighted but frugal enough to save much money, and he has a good wife and children.'),
    -- House 10 (Vol II, pg 255-257)
    (10,'PLANET_SUN',255,N'Successful in all undertakings, strong and happy, he gains sons, vehicles, fame, intelligence, money and power along with government employment and ancestral wealth; he is fond of music and possesses personal magnetism, though combinations with Mars bring vice, with Venus a rich wife, and with Saturn sorrow.'),
    (10,'PLANET_MOON',255,N'Religious, wealthy, intelligent and bold, he succeeds in his endeavours, gains ornaments and skill in the arts, and is helpful and virtuous; Jupiter''s influence brings learning in ancient subjects and astrology, Saturn''s aspect brings dispassionate thinking and income through publishing, and he keeps many friends, a long comfortable life, and trusteeship of religious institutions.'),
    (10,'PLANET_MARS',256,N'Given favourable combinations he may become a stern ruler, fond of praise and bold in governance though rash, earning much money; Mercury''s conjunction makes him a skilled scientist favoured by rulers, Jupiter''s makes him a leader of the lower classes, Venus''s makes him a trader abroad, and Saturn''s combination brings daring but childlessness.'),
    (10,'PLANET_MERCURY',256,N'Happy and straightforward, he becomes a scholar in many subjects, successful in his endeavours despite defective eyesight, and skilled in astronomy and mathematics; Venus brings a charming wife and wealth, Jupiter brings unhappiness and childlessness despite prominent government connections, and Saturn condemns him to menial clerical work and penury.'),
    (10,'PLANET_JUPITER',256,N'Becomes a high government official, rich, virtuous, spiritually steadfast, wise and guided by high principles; combined with Venus he is esteemed by government and entrusted with protecting the learned, combined with Rahu he becomes a troublemaker, and aspected by Mars he heads research or educational institutions.'),
    (10,'PLANET_VENUS',257,N'Earns through houses and buildings, is highly influential with many women in his service, and is social, friendly and renowned; combined with Saturn he profits from cosmetics and women''s goods and gains healing skill and trading ability, though his education is disrupted, and he holds divine people in respect.'),
    (10,'PLANET_SATURN',257,N'Becomes a ruler or minister, an agriculturist who is brave, rich and famous, dispassionate and devoted to the downtrodden, serving in a judicial capacity; he visits sacred sites and later becomes ascetic, his career marked by sharp ups and downs, with certain afflictions bringing a tyrannical superior or more than one wife.'),
    (10,'PLANET_RAHU',257,N'Tends to lust after widows, is a skilled artist with a flair for poetry and literature, travels widely and is learned and famous, engaging in business with limited offspring; bold and adventurous, he commits many sins.'),
    (10,'PLANET_KETU',257,N'Strong, bold and well-known, he commits vile deeds and holds impure resolves, facing many obstacles despite great cleverness; if benefically disposed, however, he is happy, religious, well-read in scripture, and fond of visiting pilgrim sites and sacred rivers.'),
    -- House 11 (Vol II, pg 374-375)
    (11,'PLANET_SUN',374,N'Lives long and becomes wealthy, gaining a wife, children and many servants along with royal and governmental favour; he achieves success with little effort and is sagacious and principled.'),
    (11,'PLANET_MOON',374,N'Noble, generous and blessed with riches, wife and children, he is introspective and quiet yet becomes famous through good business profits, acquiring vast lands with the help of women.'),
    (11,'PLANET_MARS',374,N'An eloquent, forceful speaker who is clever and rich though lustful, he acquires landed property and wields considerable influence among the powerful.'),
    (11,'PLANET_MERCURY',375,N'Becomes learned in many sciences with a keen, sharp intellect; wealthy, truthful and happy, he keeps many faithful servants and prospers in engineering ventures.'),
    (11,'PLANET_JUPITER',375,N'Long-lived with a piercing intellect, bold and wealthy though with limited children, he becomes renowned, fond of music, accumulates riches, and keeps many friends.'),
    (11,'PLANET_VENUS',375,N'Of a wandering nature, he makes immense profits and enjoys every comfort and luxury; having a weakness for women and longing for their company, he remains popular with many friends.'),
    (11,'PLANET_SATURN',375,N'Earns by employing many men and women, has few friends but enjoys life and earns through government sources; he lives a long, healthy life and is deeply involved in politics, commanding great respect.'),
    (11,'PLANET_RAHU',375,N'Distinguishes himself in the army or navy, becoming famous, wealthy and learned, though with few children and ear afflictions; he earns much wealth in foreign countries and is bold and adventurous.'),
    (11,'PLANET_KETU',375,N'Has a habit of hoarding and may receive a monetary windfall through speculation such as lottery, horse-racing or the stock market; noble and possessed of many good qualities, he succeeds in his ventures and takes part in charitable works.'),
    -- House 12 (Vol II, pg 431-432)
    (12,'PLANET_SUN',431,N'May turn to an immoral life and vile occupations, achieving little success and feeling neglected by others; he suffers loss of a limb and weak eyesight, though remaining energetic and fathering sons.'),
    (12,'PLANET_MOON',431,N'May suffer some deformity, being narrow-minded, hard-hearted and mischievous, preferring an obscure and solitary life with weak eyesight; if waning and combined with Saturn, sloth and lethargy result.'),
    (12,'PLANET_MARS',431,N'May lose his wife, being selfish and hateful, and suffers diseases from excess bodily heat; prone to being deceived and losing money, with particular combinations bringing skin disorders like leucoderma, danger from fire or malicious people, or bigamy while the first wife still lives.'),
    (12,'PLANET_MERCURY',431,N'Capricious and wayward, he indulges in extra-marital relations and suffers penury, with perverted thinking causing unhappiness; he also has few children.'),
    (12,'PLANET_JUPITER',431,N'May deride religion and turn evil-minded, committing dreadful deeds and living a lascivious life before eventually repenting and reforming; he remains anxious about his vehicles, ornaments and clothes.'),
    (12,'PLANET_VENUS',431,N'Suffers desertion by relatives and a miserable life of longing for comforts without success or of penury, tending toward lying and low company with poor eyesight; if Venus is exalted, the results are instead favourable.'),
    (12,'PLANET_SATURN',432,N'Dull-headed and loses all his money, having squint eyes and a deformed limb; he makes many enemies, suffers trade losses, is pessimistic, and commits sins in secret.'),
    (12,'PLANET_RAHU',432,N'Prosperous and of a helpful nature despite immorality, he suffers eye troubles, and certain combinations indicate his father will die early.'),
    (12,'PLANET_KETU',432,N'Has a restless, wandering mind and leaves his native country, befriending the lower classes; he may lose all his inherited property.')
    ) v (HouseNumber, PlanetCode, Page, Note)
)
INSERT research.tbl_Dim_SourceReferencePlanetInHouseText
    (PlanetInHouseId, SourceRefCode, WorkTitle, Author, Edition, Chapter, VerseOrPage, LanguageCode,
     TextTypeCode, TranslationText, Notes, CopyrightStatus, SourceLocator)
SELECT x.Id, 'SRC_RAMAN_HTJH', N'How to Judge a Horoscope', N'B. V. Raman',
       CASE WHEN c.HouseNumber <= 6 THEN N'Vol. I' ELSE N'Vol. II' END,
       N'House ' + CAST(c.HouseNumber AS NVARCHAR(2)),
       CAST(c.Page AS NVARCHAR(20)), 'en', 'Reference', c.Note,
       CASE WHEN c.HouseNumber <= 6
            THEN N'Tier 1 (running-prose) reliability per the Vol. I extract''s own front matter.'
            ELSE N'Vol. II extract is a rawer OCR pass, flagged "prose requires passage verification" in its own front matter; this paraphrase was judged confidently recoverable on review.' END,
       'SummaryOnly', N'BookExtracts/how-to-judge-a-horoscope-' + CASE WHEN c.HouseNumber <= 6 THEN N'1.md' ELSE N'2-ocr.md' END
FROM content c
JOIN research.tbl_Dim_SourceReferencePlanet p ON p.PlanetCode = c.PlanetCode
JOIN research.tbl_Dim_SourceReferenceHouse h ON h.HouseNumber = c.HouseNumber
JOIN research.tbl_Dim_SourceReferencePlanetInHouse x ON x.PlanetId = p.Id AND x.HouseId = h.Id
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetInHouseText t
    WHERE t.PlanetInHouseId = x.Id AND t.SourceRefCode = 'SRC_RAMAN_HTJH'
);
GO

;WITH content (HouseNumber, PlanetCode) AS (
    SELECT DISTINCT HouseNumber, PlanetCode FROM (VALUES
    (1,'PLANET_SUN'),(1,'PLANET_MOON'),(1,'PLANET_MARS'),(1,'PLANET_MERCURY'),(1,'PLANET_JUPITER'),(1,'PLANET_VENUS'),(1,'PLANET_SATURN'),(1,'PLANET_RAHU'),(1,'PLANET_KETU'),
    (2,'PLANET_SUN'),(2,'PLANET_MOON'),(2,'PLANET_MARS'),(2,'PLANET_MERCURY'),(2,'PLANET_JUPITER'),(2,'PLANET_VENUS'),(2,'PLANET_SATURN'),(2,'PLANET_RAHU'),(2,'PLANET_KETU'),
    (3,'PLANET_SUN'),(3,'PLANET_MOON'),(3,'PLANET_MARS'),(3,'PLANET_MERCURY'),(3,'PLANET_JUPITER'),(3,'PLANET_VENUS'),(3,'PLANET_SATURN'),(3,'PLANET_RAHU'),(3,'PLANET_KETU'),
    (4,'PLANET_SUN'),(4,'PLANET_MOON'),(4,'PLANET_MARS'),(4,'PLANET_MERCURY'),(4,'PLANET_JUPITER'),(4,'PLANET_VENUS'),(4,'PLANET_SATURN'),(4,'PLANET_RAHU'),(4,'PLANET_KETU'),
    (5,'PLANET_SUN'),(5,'PLANET_MOON'),(5,'PLANET_MARS'),(5,'PLANET_MERCURY'),(5,'PLANET_JUPITER'),(5,'PLANET_VENUS'),(5,'PLANET_SATURN'),(5,'PLANET_RAHU'),(5,'PLANET_KETU'),
    (6,'PLANET_SUN'),(6,'PLANET_MOON'),(6,'PLANET_MARS'),(6,'PLANET_MERCURY'),(6,'PLANET_JUPITER'),(6,'PLANET_VENUS'),(6,'PLANET_SATURN'),(6,'PLANET_RAHU'),(6,'PLANET_KETU'),
    (7,'PLANET_SUN'),(7,'PLANET_MOON'),(7,'PLANET_MARS'),(7,'PLANET_MERCURY'),(7,'PLANET_JUPITER'),(7,'PLANET_VENUS'),(7,'PLANET_SATURN'),(7,'PLANET_RAHU'),(7,'PLANET_KETU'),
    (8,'PLANET_SUN'),(8,'PLANET_MOON'),(8,'PLANET_MARS'),(8,'PLANET_MERCURY'),(8,'PLANET_JUPITER'),(8,'PLANET_VENUS'),(8,'PLANET_SATURN'),(8,'PLANET_RAHU'),(8,'PLANET_KETU'),
    (9,'PLANET_SUN'),(9,'PLANET_MOON'),(9,'PLANET_MARS'),(9,'PLANET_MERCURY'),(9,'PLANET_JUPITER'),(9,'PLANET_VENUS'),(9,'PLANET_SATURN'),(9,'PLANET_RAHU'),(9,'PLANET_KETU'),
    (10,'PLANET_SUN'),(10,'PLANET_MOON'),(10,'PLANET_MARS'),(10,'PLANET_MERCURY'),(10,'PLANET_JUPITER'),(10,'PLANET_VENUS'),(10,'PLANET_SATURN'),(10,'PLANET_RAHU'),(10,'PLANET_KETU'),
    (11,'PLANET_SUN'),(11,'PLANET_MOON'),(11,'PLANET_MARS'),(11,'PLANET_MERCURY'),(11,'PLANET_JUPITER'),(11,'PLANET_VENUS'),(11,'PLANET_SATURN'),(11,'PLANET_RAHU'),(11,'PLANET_KETU'),
    (12,'PLANET_SUN'),(12,'PLANET_MOON'),(12,'PLANET_MARS'),(12,'PLANET_MERCURY'),(12,'PLANET_JUPITER'),(12,'PLANET_VENUS'),(12,'PLANET_SATURN'),(12,'PLANET_RAHU'),(12,'PLANET_KETU')
    ) v (HouseNumber, PlanetCode)
)
INSERT research.tbl_Dim_SourceReferencePlanetInHouseClaim
    (PlanetInHouseId, ClaimCode, ClaimText, InterpretationText, EvidenceLevelCode, StatusCode, SourceTextId, SourceRefCode)
SELECT x.Id,
       'PLANET_IN_HOUSE_' + c.PlanetCode + '_H' + CAST(c.HouseNumber AS VARCHAR(2)),
       t.TranslationText, t.TranslationText, 'SingleSource', 'Proposed', t.Id, 'SRC_RAMAN_HTJH'
FROM content c
JOIN research.tbl_Dim_SourceReferencePlanet p ON p.PlanetCode = c.PlanetCode
JOIN research.tbl_Dim_SourceReferenceHouse h ON h.HouseNumber = c.HouseNumber
JOIN research.tbl_Dim_SourceReferencePlanetInHouse x ON x.PlanetId = p.Id AND x.HouseId = h.Id
JOIN research.tbl_Dim_SourceReferencePlanetInHouseText t ON t.PlanetInHouseId = x.Id AND t.SourceRefCode = 'SRC_RAMAN_HTJH'
WHERE NOT EXISTS (
    SELECT 1 FROM research.tbl_Dim_SourceReferencePlanetInHouseClaim cl
    WHERE cl.PlanetInHouseId = x.Id AND cl.SourceRefCode = 'SRC_RAMAN_HTJH'
);
GO

IF OBJECT_ID(N'dbo.SchemaMigrations', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SchemaMigrations WHERE ScriptName = N'114_seed_planet_in_house_claims.sql')
    INSERT dbo.SchemaMigrations (ScriptName, Note)
    VALUES (N'114_seed_planet_in_house_claims.sql',
        N'Seeds 108 research.*PlanetInHouseText + 108 *PlanetInHouseClaim rows, SRC_RAMAN_HTJH (How to Judge a Horoscope), correcting 064/065''s wrongly-cited source. StatusCode=Proposed throughout.');
GO

DECLARE @textCount INT = (SELECT COUNT(*) FROM research.tbl_Dim_SourceReferencePlanetInHouseText WHERE SourceRefCode = 'SRC_RAMAN_HTJH');
DECLARE @claimCount INT = (SELECT COUNT(*) FROM research.tbl_Dim_SourceReferencePlanetInHouseClaim WHERE SourceRefCode = 'SRC_RAMAN_HTJH');
PRINT '114 applied: ' + CAST(@textCount AS VARCHAR(10)) + ' Text rows (expect 108), '
    + CAST(@claimCount AS VARCHAR(10)) + ' Claim rows (expect 108), SourceRefCode=SRC_RAMAN_HTJH.';
GO
