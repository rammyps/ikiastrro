namespace Ikiastrro.Core.Numerology;

/// <summary>
/// Cheiro's meanings of the "compound" numbers 10–52 (the 52 weeks of the year), transcribed from
/// Cheiro's Book of Numbers (1926, public domain), pp. 80–84. Like the letter-value table in
/// <see cref="CheiroNumerology"/> this is a fixed classical text nobody edits, so it is hardcoded
/// rather than stored as DB reference data. Cheiro stops at 52 ("for all practical purposes there
/// is no necessity to proceed further"), so a compound total above 52 has no entry.
/// Numbers 33–52 mostly just point back to an earlier number — <see cref="Meaning.SameAs"/> holds
/// that pointer and <see cref="Get"/> returns the entry with its pointer resolved.
/// </summary>
public static class CheiroCompoundNumbers
{
    public const int Min = 10;
    public const int Max = 52;

    /// <param name="Number">The compound number.</param>
    /// <param name="Text">Cheiro's text for this number (for a pointer entry, the "same meaning as N" sentence).</param>
    /// <param name="SameAs">The earlier number whose meaning this one shares, or null if it has its own.</param>
    public sealed record Meaning(int Number, string Text, int? SameAs = null);

    private static readonly IReadOnlyDictionary<int, Meaning> Table = Build();

    public static IReadOnlyList<Meaning> All { get; } = Table.Values.OrderBy(m => m.Number).ToList();

    /// <summary>The entry for a compound number, or null outside 10–52.</summary>
    public static Meaning? Get(int number) => Table.GetValueOrDefault(number);

    /// <summary>The entry that actually carries the interpretation: itself, or the number it points to.</summary>
    public static Meaning? Resolve(int number)
    {
        var meaning = Get(number);
        return meaning?.SameAs is { } target ? Get(target) : meaning;
    }

    private static IReadOnlyDictionary<int, Meaning> Build()
    {
        Meaning[] entries =
        [
            new(10, "Symbolised as the “Wheel of Fortune.” It is a number of honour, of faith and self-confidence, of rise and fall; one's name will be known for good or evil, according to one's desires; it is a fortunate number in the sense that one's plans are likely to be carried out."),
            new(11, "This is an ominous number to occultists. It gives warning of hidden dangers, trial, and treachery from others. It has a symbol of “a Clenched Hand,” and “a Lion Muzzled,” and of a person who will have great difficulties to contend against."),
            new(12, "The symbolism of this number is suffering and anxiety of mind. It is also indicated as “the Sacrifice” or “the Victim” and generally foreshadows one being sacrificed for the plans or intrigues of others."),
            new(13, "This is a number indicating change of plans, place, and such-like, and is not unfortunate, as is generally supposed. In some of the ancient writings it is said, “He who understands the number 13 will be given power and dominion.” It is symbolised by the picture of “a Skeleton” or “Death,” with a scythe reaping down men, in a field of new-grown grass where young faces and heads appear cropping up on every side. It is a number of upheaval and destruction. It is a symbol of “Power” which if wrongly used will wreak destruction upon itself. It is a number of warning of the unknown or unexpected, if it becomes a “compound” number in one's calculations."),
            new(14, "This is a number of movement, combination of people and things, and danger from natural forces, such as tempests, water, air, or fire. This number is fortunate for dealings with money, speculation, and changes in business, but there is always a strong element of risk and danger attached to it, but generally owing to the actions and foolhardiness of others. If this number comes out in calculations of future events the person should be warned to act with caution and prudence."),
            new(15, "This is a number of occult significance, of magic and mystery; but as a rule it does not represent the higher side of occultism, its meaning being that the persons represented by it will use every art of magic they can to carry out their purpose. If associated with a good or fortunate single number, it can be very lucky and powerful, but if associated with one of the peculiar numbers, such as a 4 or an 8, the person it represents will not scruple to use any sort of art, or even “black magic,” to gain what he or she desires. It is peculiarly associated with “good talkers,” often with eloquence, gifts of Music and Art and a dramatic personality, combined with a certain voluptuous temperament and strong personal magnetism. For obtaining money, gifts, and favours from others it is a fortunate number."),
            new(16, "This number has a most peculiar occult symbolism. It is pictured by “a Tower Struck by Lightning from which a man is falling with a Crown on his head.” It is also called “the Shattered Citadel.” It gives warning of some strange fatality awaiting one, also danger of accidents and defeat of one's plans. If it appears as a “compound” number relating to the future, it is a warning sign that should be carefully noted and plans made in advance in the endeavour to avert its fatalistic tendency."),
            new(17, "This is a highly spiritual number, and is expressed in symbolism by the 8-pointed Star of Venus: a symbol of “Peace and Love.” It is also called “the Star of the Magi” and expresses that the person it represents has risen superior in spirit to the trials and difficulties of his life or his career. It is considered a “number of immortality” and that the person's name “lives after him.” It is a fortunate number if it works out in relation to future events, provided it is not associated with the single numbers of fours and eights."),
            new(18, "This number has a difficult symbolism to translate. It is pictured as “a rayed moon from which drops of blood are falling; a wolf and a hungry dog are seen below catching the falling drops of blood in their opened mouths, while still lower a crab is seen hastening to join them.” It is symbolic of materialism striving to destroy the spiritual side of the nature. It generally associates a person with bitter quarrels, even family ones, also with war, social upheavals, revolutions; and in some cases it indicates making money and position through wars or by wars. It is, however, a warning of treachery, deception by others, also danger from the elements, such as storms, danger from water, fires and explosions. When this “compound” number appears in working out dates in advance, such a date should be taken with a great amount of care, caution, and circumspection."),
            new(19, "This number is regarded as fortunate and extremely favourable. It is symbolised as “the Sun” and is called “the Prince of Heaven.” It is a number promising happiness, success, esteem and honour, and promises success in one's plans for the future."),
            new(20, "This number is called “the Awakening”; also “the Judgment.” It is symbolised by the figure of a winged angel sounding a trumpet, while from below a man, a woman, and a child are seen rising from a tomb with their hands clasped in prayer. This number has a peculiar interpretation: the awakening of new purpose, new plans, new ambitions, the call to action, but for some great purpose, cause or duty. It is not a material number and consequently is a doubtful one as far as worldly success is concerned. If used in relation to a future event, it denotes delays, hindrances to one's plans, which can only be conquered through the development of the spiritual side of the nature."),
            new(21, "This number is symbolised by the picture of “the Universe,” and it is also called “the Crown of the Magi.” It is a number of advancement, honours, elevation in life, and general success. It means victory after a long fight, for “the Crown of the Magi” is only gained after long initiation and tests of determination. It is a fortunate number of promise if it appears in any connection with future events."),
            new(22, "This number is symbolised by “a Good Man blinded by the folly of others, with a knapsack on his back full of Errors.” In this picture he appears to offer no defence against a ferocious tiger which is attacking him. It is a warning number of illusion and delusion, a good person who lives in a fool's paradise; a dreamer of dreams who awakens only when surrounded by danger. It is also a number of false judgment owing to the influence of others. As a number in connection with future events its warning and meaning should be carefully noted."),
            new(23, "This number is called “the Royal Star of the Lion.” It is a promise of success, help from superiors and protection from those in high places. In dealing with future events it is a most fortunate number and a promise of success for one's plans."),
            new(24, "This number is also fortunate; it promises the assistance and association of those of rank and position with one's plans; it also denotes gain through love and the opposite sex; it is a favourable number when it comes out in relation to future events."),
            new(25, "This is a number denoting strength gained through experience, and benefits obtained through observation of people and things. It is not deemed exactly “lucky,” as its success is given through strife and trials in the earlier life. It is favourable when it appears in regard to the future."),
            new(26, "This number is full of the gravest warnings for the future. It foreshadows disasters brought about by association with others; ruin, by bad speculations, by partnerships, unions, and bad advice. If it comes out in connection with future events one should carefully consider the path one is treading."),
            new(27, "This is a good number and is symbolised as “the Sceptre.” It is a promise of authority, power, and command. It indicates that reward will come from the productive intellect; that the creative faculties have sown good seeds that will reap a harvest. Persons with this “compound” number at their back should carry out their own ideas and plans. It is a fortunate number if it appears in any connection with future events."),
            new(28, "This number is full of contradictions. It indicates a person of great promise and possibilities who is likely to see all taken away from him unless he carefully provides for the future. It indicates loss through trust in others, opposition and competition in trade, danger of loss through law, and the likelihood of having to begin life's road over and over again. It is not a fortunate number for the indication of future events."),
            new(29, "This number indicates uncertainties, treachery, and deception of others; it foreshadows trials, tribulation, and unexpected dangers, unreliable friends, and grief and deception caused by members of the opposite sex. It gives grave warning if it comes out in anything concerning future events."),
            new(30, "This is a number of thoughtful deduction, retrospection, and mental superiority over one's fellows, but, as it seems to belong completely to the mental plane, the persons it represents are likely to put all material things on one side — not because they have to, but because they wish to do so. For this reason it is neither fortunate nor unfortunate, for either depends on the mental outlook of the person it represents. It can be all powerful, but it is just as often indifferent according to the will or desire of the person."),
            new(31, "This number is very similar to the preceding one, except that the person it represents is even more self-contained, lonely, and isolated from his fellows. It is not a fortunate number from a worldly or material standpoint."),
            new(32, "This number has a magical power like the single 5, or the “compound” numbers 14 and 23. It is usually associated with combinations of people or nations. It is a fortunate number if the person it represents holds to his own judgment and opinions; if not, his plans are likely to become wrecked by the stubbornness and stupidity of others. It is a favourable number if it appears in connection with future events."),
            new(33, "This number has no potency of its own, and consequently has the same meaning as the 24 — which is also a 6 — and the next to it in its own series of “compound” numbers.", SameAs: 24),
            new(34, "Has the same meaning as the number 25, which is the next to it in its own series of “compound” numbers.", SameAs: 25),
            new(35, "Has the same meaning as the number 26, which is the next to it in its own series of “compound” numbers.", SameAs: 26),
            new(36, "Has the same meaning as the number 27, which is the next to it in its own series of “compound” numbers.", SameAs: 27),
            new(37, "This number has a distinct potency of its own. It is a number of good and fortunate friendships in love, and in combinations connected with the opposite sex. It is also good for partnerships of all kinds. It is a fortunate indication if it appears in connection with future events."),
            new(38, "Has the same meaning as the number 29, which is the next to it in its own series of “compound” numbers.", SameAs: 29),
            new(39, "Has the same meaning as the number 30, which is the next to it in its own series of “compound” numbers.", SameAs: 30),
            new(40, "Has the same meaning as the number 31, which is next to it in its own series of “compound” numbers.", SameAs: 31),
            new(41, "Has the same meaning as the number 32, which is next to it in its own series of “compound” numbers.", SameAs: 32),
            new(42, "Has the same meaning as the number 24.", SameAs: 24),
            new(43, "This is an unfortunate number. It is symbolised by the signs of revolution, upheaval, strife, failure, and prevention, and is not a fortunate number if it comes out in calculations relating to future events."),
            new(44, "Has the same meaning as 26.", SameAs: 26),
            new(45, "Has the same meaning as 27.", SameAs: 27),
            new(46, "Has the same meaning as 37.", SameAs: 37),
            new(47, "Has the same meaning as 29.", SameAs: 29),
            new(48, "Has the same meaning as 30.", SameAs: 30),
            new(49, "Has the same meaning as 31.", SameAs: 31),
            new(50, "Has the same meaning as 32.", SameAs: 32),
            new(51, "This number has a very powerful potency of its own. It represents the nature of the warrior; it promises sudden advancement in whatever one undertakes; it is especially favourable for those in military or naval life and for leaders in any cause. At the same time it threatens enemies, danger, and the likelihood of assassination."),
            new(52, "Has the same meaning as 43.", SameAs: 43),
        ];

        return entries.ToDictionary(e => e.Number);
    }
}
