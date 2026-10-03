using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Dignity;
using Ikiastrro.Core.Engines.Houses;

namespace Ikiastrro.Core.Engines.Matching;

/// <summary>
/// Kuta agreement exactly as Gayatri Devi Vasudev, "The Art of Matching Charts", Ch. VI "Kuta
/// Agreement" (pp.65-83; <c>SRC_VASUDEV_MATCHING_CHARTS</c>) states it: Varna 1, Vashya 2, Dina (Tara)
/// 3, Yoni 4, Grahamaitra 5, Gana 6, Rasi (Bhakoota) 7, Nadi 8 = 36, pass at 18. Rajju, Stree Deergha
/// and Mahendra are tested but carry no numeric value in the source (p.66, p.77).
///
/// Where the source gives no rule the result says so instead of guessing:
///  - Yoni (p.69-70): the book scores 4 for the same animal, 0 for hostile animals and "between 2 and 3"
///    for passable pairs, but lists no hostile-pair table. Different animals are therefore Unscored.
///  - Rasi (p.73-75): the book gives only qualitative verdicts per distance and a maximum of 7; a
///    favourable verdict scores 7 and an unfavourable one 0. That 7/0 mapping is this project's reading.
///  - Vashya (p.67): the table lists "signs - sign"; read as boy's sign in the list of the girl's sign.
///    The book's two examples fix that direction only for the case they show.
/// Vedha, the tenth Dasakoota factor, is not tabulated in the scanned chapter and is not computed.
///
/// Boy = groom, Girl = bride throughout. Pure; no I/O.
/// </summary>
public static class AshtakootaCalculator
{
    public static AshtakootaResult Score(MatchPerson boy, MatchPerson girl)
    {
        var scored = new[]
        {
            Varna(boy, girl), Vashya(boy, girl), Dina(boy, girl), Yoni(boy, girl),
            Grahamaitra(boy, girl), Gana(boy, girl), Rasi(boy, girl), Nadi(boy, girl),
        };
        var additional = new[] { Rajju(boy, girl), StreeDeergha(boy, girl), Mahendra(boy, girl) };

        var min = scored.Sum(k => k.Score ?? 0);
        var max = scored.Sum(k => k.Score ?? k.MaxScore);
        var sameLord = HouseEngine.GetSignLord(boy.MoonSign) == HouseEngine.GetSignLord(girl.MoonSign);
        return new AshtakootaResult(scored, additional, min, max, sameLord);
    }

    // ---- Varna (p.66-67) ---------------------------------------------------------------------

    private enum VarnaRank { Sudra = 1, Vaisya = 2, Kshatriya = 3, Brahmin = 4 }

    private static VarnaRank SignVarna(ZodiacName sign) => sign switch
    {
        ZodiacName.Pisces or ZodiacName.Scorpio or ZodiacName.Cancer => VarnaRank.Brahmin,
        ZodiacName.Aries or ZodiacName.Leo or ZodiacName.Sagittarius => VarnaRank.Kshatriya,
        ZodiacName.Taurus or ZodiacName.Virgo or ZodiacName.Capricornus => VarnaRank.Vaisya,
        _ => VarnaRank.Sudra, // Gemini, Aquarius, Libra
    };

    private static VarnaRank PlanetVarna(string planet) => planet switch
    {
        "Sun" or "Mars" => VarnaRank.Kshatriya,
        "Moon" => VarnaRank.Vaisya,
        "Jupiter" or "Venus" => VarnaRank.Brahmin,
        _ => VarnaRank.Sudra, // Mercury, Saturn
    };

    private static KutaResult Varna(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.66-67";
        var b = SignVarna(boy.MoonSign);
        var g = SignVarna(girl.MoonSign);
        if (g <= b)
            return new("VARNA", "Varna", 1, 1, KutaStatus.Present, $"Girl's Varna ({g}) is not higher than boy's ({b}).", loc);

        // A higher-Varna girl is not approved, but is made up if the ruler of her Moon sign is of a
        // Varna no higher than the ruler of the boy's Moon sign (the book's Aries/Scorpio example).
        var bl = PlanetVarna(HouseEngine.GetSignLord(boy.MoonSign));
        var gl = PlanetVarna(HouseEngine.GetSignLord(girl.MoonSign));
        return gl <= bl
            ? new("VARNA", "Varna", 1, 1, KutaStatus.Present, $"Girl's Varna ({g}) is higher than boy's ({b}), made up because her sign ruler ({gl}) is not higher than his ({bl}).", loc)
            : new("VARNA", "Varna", 0, 1, KutaStatus.Absent, $"Girl's Varna ({g}) is higher than boy's ({b}) and her sign ruler ({gl}) is higher than his ({bl}).", loc);
    }

    // ---- Vashya (p.67-68) --------------------------------------------------------------------

    // Key: the girl's sign; value: the boy's signs that are its Vashya (the book's "signs - sign" rows).
    private static readonly IReadOnlyDictionary<ZodiacName, ZodiacName[]> VashyaOf = new Dictionary<ZodiacName, ZodiacName[]>
    {
        [ZodiacName.Aries] = [ZodiacName.Leo, ZodiacName.Scorpio],
        [ZodiacName.Taurus] = [ZodiacName.Cancer, ZodiacName.Libra],
        [ZodiacName.Gemini] = [ZodiacName.Virgo],
        [ZodiacName.Cancer] = [ZodiacName.Scorpio, ZodiacName.Sagittarius],
        [ZodiacName.Leo] = [ZodiacName.Libra],
        [ZodiacName.Virgo] = [ZodiacName.Gemini, ZodiacName.Pisces],
        [ZodiacName.Libra] = [ZodiacName.Capricornus],
        [ZodiacName.Scorpio] = [ZodiacName.Virgo, ZodiacName.Cancer],
        [ZodiacName.Sagittarius] = [ZodiacName.Pisces],
        [ZodiacName.Capricornus] = [ZodiacName.Aquarius, ZodiacName.Aries],
        [ZodiacName.Aquarius] = [ZodiacName.Aries],
        [ZodiacName.Pisces] = [ZodiacName.Capricornus],
    };

    private static KutaResult Vashya(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.67-68";
        if (boy.MoonSign == girl.MoonSign)
            return new("VASHYA", "Vashya", 0, 2, KutaStatus.Absent, "Same sign: Vashya does not obtain.", loc);
        return VashyaOf[girl.MoonSign].Contains(boy.MoonSign)
            ? new("VASHYA", "Vashya", 2, 2, KutaStatus.Present, $"{boy.MoonSign} is in the Vashya list of {girl.MoonSign}.", loc)
            : new("VASHYA", "Vashya", 0, 2, KutaStatus.Absent, $"{boy.MoonSign} is not in the Vashya list of {girl.MoonSign}.", loc);
    }

    // ---- Dina / Tara (p.68) ------------------------------------------------------------------

    /// <summary>Nakshatras counted from the girl's to the boy's, inclusive (same star = 1), 1-27.</summary>
    internal static int CountFromGirl(MatchPerson boy, MatchPerson girl) =>
        ((boy.NakshatraNumber - girl.NakshatraNumber + 27) % 27) + 1;

    private static KutaResult Dina(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.68";
        var count = CountFromGirl(boy, girl);
        var remainder = count % 9;
        return remainder is 2 or 4 or 6 or 8 or 0
            ? new("DINA", "Dina (Tara)", 3, 3, KutaStatus.Present, $"Count {count}, remainder {remainder}: good.", loc)
            : new("DINA", "Dina (Tara)", 0, 3, KutaStatus.Absent, $"Count {count}, remainder {remainder}: not good.", loc);
    }

    // ---- Yoni (p.69-70) ----------------------------------------------------------------------

    private static KutaResult Yoni(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.69-70";
        if (string.Equals(boy.YoniAnimal, girl.YoniAnimal, StringComparison.OrdinalIgnoreCase))
            return new("YONI", "Yoni", 4, 4, KutaStatus.Present, $"Same Yoni ({boy.YoniAnimal}).", loc);
        return new("YONI", "Yoni", null, 4, KutaStatus.Unscored,
            $"{boy.YoniAnimal} and {girl.YoniAnimal} differ; the book scores hostile pairs 0 and passable pairs 2-3 but lists no hostile-pair table.", loc);
    }

    // ---- Grahamaitra (p.70-71) ---------------------------------------------------------------

    private static KutaResult Grahamaitra(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.70-71";
        var bl = HouseEngine.GetSignLord(boy.MoonSign);
        var gl = HouseEngine.GetSignLord(girl.MoonSign);
        if (bl == gl)
            return new("GRAHAMAITRA", "Grahamaitra", 5, 5, KutaStatus.Present, $"Both Moon signs are ruled by {bl}.", loc);

        var a = DignityEngine.NaturalAttitude(bl, gl)!;
        var b = DignityEngine.NaturalAttitude(gl, bl)!;
        var friends = (a == "Friend" ? 1 : 0) + (b == "Friend" ? 1 : 0);
        var enemies = (a == "Enemy" ? 1 : 0) + (b == "Enemy" ? 1 : 0);
        var score = (friends, enemies) switch
        {
            (2, _) => 5,      // both friendly
            (1, 0) => 4,      // one friendly, the other neutral
            (0, 0) => 3,      // both neutral
            (1, 1) => 2,      // one friendly, the other an enemy
            _ => 0,           // neutral + enemy, or both enemies
        };
        var reason = $"{bl} regards {gl} as {a.ToLowerInvariant()}; {gl} regards {bl} as {b.ToLowerInvariant()}.";
        return new("GRAHAMAITRA", "Grahamaitra", score, 5, score > 0 ? KutaStatus.Present : KutaStatus.Absent, reason, loc);
    }

    // ---- Gana (p.71-72) ----------------------------------------------------------------------

    private static KutaResult Gana(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.71-72";
        var score = (Norm(boy.Gana), Norm(girl.Gana)) switch
        {
            ("Deva", "Deva") or ("Manushya", "Manushya") or ("Rakshasa", "Rakshasa") => 6,
            ("Deva", "Manushya") or ("Manushya", "Deva") => 4,
            ("Manushya", "Rakshasa") or ("Rakshasa", "Manushya") => 2,
            _ => 0, // Deva with Rakshasa, either way round
        };
        return new("GANA", "Gana", score, 6, score > 0 ? KutaStatus.Present : KutaStatus.Absent,
            $"Boy {boy.Gana}, girl {girl.Gana}.", loc);

        static string Norm(string gana) => gana.Equals("Daiva", StringComparison.OrdinalIgnoreCase) ? "Deva" : gana;
    }

    // ---- Rasi / Bhoo Kuta (p.73-75) ----------------------------------------------------------

    // The 6th-from exception (p.74), (girl's sign, boy's sign): the adverse "loss of progeny" does not apply.
    private static readonly (ZodiacName Girl, ZodiacName Boy)[] SixthException =
    [
        (ZodiacName.Aries, ZodiacName.Virgo), (ZodiacName.Sagittarius, ZodiacName.Taurus),
        (ZodiacName.Libra, ZodiacName.Pisces), (ZodiacName.Aquarius, ZodiacName.Cancer),
        (ZodiacName.Leo, ZodiacName.Capricornus), (ZodiacName.Gemini, ZodiacName.Scorpio),
    ];

    private static bool IsEvenSign(ZodiacName sign) => (int)sign % 2 == 1; // Taurus, Cancer, Virgo, Scorpio, Capricorn, Pisces

    private static KutaResult Rasi(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.73-75";
        // Position of the boy's sign counted from the girl's sign (1 = same sign).
        var d = (((int)boy.MoonSign - (int)girl.MoonSign + 12) % 12) + 1;
        string verdict;
        bool favourable;
        switch (d)
        {
            case 1: favourable = true; verdict = "Same sign (the book scores it 7)."; break;
            case 7: favourable = true; verdict = "Diametrically opposite signs: happy and long married life."; break;
            case 2 when IsEvenSign(boy.MoonSign):
                favourable = true; verdict = "Boy's sign is 2nd from the girl's (fatal in general), but it is an even sign: the book's exception applies."; break;
            case 2: favourable = false; verdict = "Boy's sign is 2nd from the girl's: fatal."; break;
            case 12: favourable = true; verdict = "Girl's sign is 2nd from the boy's: prolongs life."; break;
            case 3: favourable = false; verdict = "Boy's sign is 3rd from the girl's: misery."; break;
            case 11: favourable = true; verdict = "Girl's sign is 3rd from the boy's: happiness."; break;
            case 4: favourable = false; verdict = "Boy's sign is 4th from the girl's: poverty."; break;
            case 10: favourable = true; verdict = "Girl's sign is 4th from the boy's: prosperity."; break;
            case 5: favourable = false; verdict = "Boy's sign is 5th from the girl's: widowhood."; break;
            case 9: favourable = true; verdict = "Girl's sign is 5th from the boy's: long married life."; break;
            case 6 when SixthException.Contains((girl.MoonSign, boy.MoonSign)):
                favourable = true; verdict = "Boy's sign is 6th from the girl's (loss of progeny), but the pair is on the book's exception list."; break;
            case 6: favourable = false; verdict = "Boy's sign is 6th from the girl's: loss of progeny."; break;
            default: favourable = true; verdict = "Girl's sign is 6th from the boy's: birth of children is favoured."; break; // 8
        }
        return favourable
            ? new("RASI", "Rasi (Bhoo Kuta)", 7, 7, KutaStatus.Present, verdict, loc)
            : new("RASI", "Rasi (Bhoo Kuta)", 0, 7, KutaStatus.Absent, verdict, loc);
    }

    // ---- Nadi (p.75) -------------------------------------------------------------------------

    private static KutaResult Nadi(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.75";
        return string.Equals(boy.Nadi, girl.Nadi, StringComparison.OrdinalIgnoreCase)
            ? new("NADI", "Nadi", 0, 8, KutaStatus.Absent, $"Both {boy.Nadi}: not generally approved.", loc)
            : new("NADI", "Nadi", 8, 8, KutaStatus.Present, $"Boy {boy.Nadi}, girl {girl.Nadi}: different Nadis.", loc);
    }

    // ---- Rajju (p.76-77), Stree Deergha, Mahendra (p.77): tested, not scored -------------------

    private enum RajjuGroup { Pada, Ooru, Nabhi, Kanta, Sira }

    // Nakshatra number -> Rajju group (book table p.76).
    private static readonly IReadOnlyDictionary<int, RajjuGroup> RajjuOf = BuildRajju();

    private static Dictionary<int, RajjuGroup> BuildRajju()
    {
        var map = new Dictionary<int, RajjuGroup>();
        void Add(RajjuGroup g, params int[] nakshatras) { foreach (var n in nakshatras) map[n] = g; }
        Add(RajjuGroup.Pada, 1, 9, 10, 18, 19, 27);   // Ashwini, Ashlesha, Magha, Jyeshtha, Moola, Revati
        Add(RajjuGroup.Ooru, 2, 8, 11, 17, 20, 26);   // Bharani, Pushya, Purva Phalguni, Anuradha, Purvashada, Uttarabhadra
        Add(RajjuGroup.Nabhi, 3, 7, 12, 16, 21, 25);  // Krittika, Punarvasu, Uttara Phalguni, Visakha, Uttarashada, Purvabhadra
        Add(RajjuGroup.Kanta, 4, 6, 13, 15, 22, 24);  // Rohini, Ardra, Hasta, Swati, Sravana, Satabhisha
        Add(RajjuGroup.Sira, 5, 14, 23);              // Mrigasira, Chitra, Dhanishta
        return map;
    }

    private static KutaResult Rajju(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.76-77";
        var b = RajjuOf[boy.NakshatraNumber];
        var g = RajjuOf[girl.NakshatraNumber];
        return b != g
            ? new("RAJJU", "Rajju", null, 0, KutaStatus.Present, $"Different Rajju groups ({b} / {g}): favourable.", loc)
            : new("RAJJU", "Rajju", null, 0, KutaStatus.Absent, $"Same Rajju group ({b}).", loc);
    }

    private static KutaResult StreeDeergha(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.77";
        var count = CountFromGirl(boy, girl);
        return count > 9
            ? new("STREE_DEERGHA", "Stree Deergha", null, 0, KutaStatus.Present, $"Boy's star is {count} from the girl's: beyond the 9th.", loc)
            : new("STREE_DEERGHA", "Stree Deergha", null, 0, KutaStatus.Absent, $"Boy's star is {count} from the girl's: within the 9th.", loc);
    }

    private static readonly int[] MahendraCounts = [4, 7, 10, 12, 16, 19, 22, 25];

    private static KutaResult Mahendra(MatchPerson boy, MatchPerson girl)
    {
        const string loc = "p.77";
        var count = CountFromGirl(boy, girl);
        return MahendraCounts.Contains(count)
            ? new("MAHENDRA", "Mahendra", null, 0, KutaStatus.Present, $"Boy's star is the {count}th from the girl's.", loc)
            : new("MAHENDRA", "Mahendra", null, 0, KutaStatus.Absent, $"Boy's star is {count} from the girl's: not one of 4, 7, 10, 12, 16, 19, 22, 25.", loc);
    }
}
