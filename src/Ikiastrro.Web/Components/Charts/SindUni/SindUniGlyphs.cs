using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Web.Components.Charts.SindUni;

/// <summary>
/// Fixed display vocabulary for the SIND-UNI charts (docs/ui/components/spec_SIND-UNI_GridChart.md):
/// sign codes, the sign-nature glyphs, planet codes and glyphs, the three-letter nakshatra codes
/// and the special-lagna colour tokens. English only; the planned language switch replaces these
/// tables as a whole.
/// </summary>
public static class SindUniGlyphs
{
    /// <summary>Appended after a symbol so browsers draw it as text, never as a colour emoji.</summary>
    public const string TextStyle = "︎";

    /// <summary>South Indian layout: fixed (column, row) of each sign in the 4×4 grid.</summary>
    public static readonly IReadOnlyDictionary<ZodiacName, (int Col, int Row)> Position = new Dictionary<ZodiacName, (int, int)>
    {
        [ZodiacName.Pisces] = (1, 1), [ZodiacName.Aries] = (2, 1), [ZodiacName.Taurus] = (3, 1), [ZodiacName.Gemini] = (4, 1),
        [ZodiacName.Aquarius] = (1, 2), [ZodiacName.Cancer] = (4, 2),
        [ZodiacName.Capricornus] = (1, 3), [ZodiacName.Leo] = (4, 3),
        [ZodiacName.Sagittarius] = (1, 4), [ZodiacName.Scorpio] = (2, 4), [ZodiacName.Libra] = (3, 4), [ZodiacName.Virgo] = (4, 4),
    };

    public static string SignCode(ZodiacName s) => s switch
    {
        ZodiacName.Aries => "AR", ZodiacName.Taurus => "TA", ZodiacName.Gemini => "GE", ZodiacName.Cancer => "CN",
        ZodiacName.Leo => "LE", ZodiacName.Virgo => "VI", ZodiacName.Libra => "LI", ZodiacName.Scorpio => "SC",
        ZodiacName.Sagittarius => "SG", ZodiacName.Capricornus => "CP", ZodiacName.Aquarius => "AQ", _ => "PI",
    };

    public static string SignName(ZodiacName s) => s == ZodiacName.Capricornus ? "Capricorn" : s.ToString();

    public static string SignGlyph(ZodiacName s) => ((char)(0x2648 + (int)s)).ToString() + TextStyle;

    public enum Element { Fire, Earth, Air, Water }
    public enum Modality { Movable, Fixed, Dual }

    public static Element ElementOf(ZodiacName s) => (Element)((int)s % 4);
    public static Modality ModalityOf(ZodiacName s) => (Modality)((int)s % 3);
    public static bool IsOdd(ZodiacName s) => (int)s % 2 == 0;

    public static string ElementGlyph(Element e) => e switch
    {
        Element.Fire => "\U0001F702", Element.Earth => "\U0001F703", Element.Air => "\U0001F701", _ => "\U0001F704",
    };

    public static string ModalityGlyph(Modality m) => m switch { Modality.Movable => "↻", Modality.Fixed => "■", _ => "◐" };

    public static string ModalitySanskrit(Modality m) => m switch { Modality.Movable => "chara", Modality.Fixed => "sthira", _ => "dvisvabhava" };

    public static readonly IReadOnlyList<string> PlanetOrder = ["Sun", "Moon", "Mars", "Mercury", "Jupiter", "Venus", "Saturn", "Rahu", "Ketu"];

    public static string PlanetCode(string planet) => planet switch
    {
        "Sun" => "SU", "Moon" => "MO", "Mars" => "MA", "Mercury" => "ME", "Jupiter" => "JU",
        "Venus" => "VE", "Saturn" => "SA", "Rahu" => "RA", "Ketu" => "KE", _ => planet.ToUpperInvariant()[..Math.Min(2, planet.Length)],
    };

    public static string PlanetGlyph(string planet) => planet switch
    {
        "Sun" => "☉", "Moon" => "☽", "Mars" => "♂", "Mercury" => "☿", "Jupiter" => "♃",
        "Venus" => "♀", "Saturn" => "♄", "Rahu" => "☊", "Ketu" => "☋", _ => "",
    } + TextStyle;

    /// <summary>Three-letter nakshatra codes by NakshatraId (1 = Ashwini … 27 = Revati).</summary>
    public static readonly IReadOnlyList<(string Code, string Name)> Nakshatras =
    [
        ("ASW", "Ashwini"), ("BHA", "Bharani"), ("KRI", "Krittika"), ("ROH", "Rohini"), ("MRI", "Mrigashira"),
        ("ARD", "Ardra"), ("PUN", "Punarvasu"), ("PUS", "Pushya"), ("ASL", "Ashlesha"), ("MAG", "Magha"),
        ("PPH", "Purva Phalguni"), ("UPH", "Uttara Phalguni"), ("HAS", "Hasta"), ("CHI", "Chitra"), ("SWA", "Swati"),
        ("VIS", "Vishakha"), ("ANU", "Anuradha"), ("JYE", "Jyeshtha"), ("MUL", "Mula"), ("PAS", "Purva Ashadha"),
        ("UAS", "Uttara Ashadha"), ("SRA", "Shravana"), ("DHA", "Dhanishta"), ("SAT", "Shatabhisha"),
        ("PBH", "Purva Bhadrapada"), ("UBH", "Uttara Bhadrapada"), ("REV", "Revati"),
    ];

    public static string NakshatraCode(int nakshatraId) =>
        nakshatraId is >= 1 and <= 27 ? Nakshatras[nakshatraId - 1].Code : "";

    /// <summary>The nine padas a sign holds, in order: (nakshatra id 1–27, pada 1–4).</summary>
    public static IReadOnlyList<(int NakshatraId, int Pada)> PadasIn(ZodiacName s) =>
        Enumerable.Range((int)s * 9, 9).Select(p => (p / 4 + 1, p % 4 + 1)).ToList();

    /// <summary>tbl_Chart_KeyDetails.DignityStatus → the --dignity-* token suffix.</summary>
    public static string DignityToken(string? status) => status switch
    {
        "Exalted" => "exalted",
        "Moolatrikona" => "moolatrikona",
        "Own Sign" or "Own" => "own",
        "Great Friend" => "great-friend",
        "Friend" => "friend",
        "Enemy" => "enemy",
        "Great Enemy" => "great-enemy",
        "Debilitated" => "debilitated",
        _ => "neutral",
    };

    /// <summary>Houses-from dropdown: the planets offered, in the user's order (Moon first).</summary>
    public static readonly IReadOnlyList<string> HouseReferences = ["Moon", "Sun", "Jupiter", "Saturn", "Mars", "Venus", "Mercury"];

    /// <summary>One special lagna the Spl Lagnas view can switch on, with its colour token.</summary>
    public sealed record SpecialLagnaKind(string Code, string Name, string ColorVar, bool OnByDefault);

    public static readonly IReadOnlyList<SpecialLagnaKind> SpecialLagnas =
    [
        new("ASC", "Lagna", "--sl-asc", true),
        new("MOON", "Moon (Chandra lagna)", "--sl-moon", true),
        new("SUN", "Sun (Surya lagna)", "--sl-sun", false),
        new("AL", "Arudha Lagna", "--sl-al", true),
        new("HL", "Hora Lagna", "--sl-hl", true),
        new("GL", "Ghati Lagna", "--sl-gl", true),
        new("BL", "Bhava Lagna", "--sl-bl", false),
        new("SL", "Sree Lagna", "--sl-sl", true),
        new("IL", "Indu Lagna", "--sl-il", false),
        new("PP", "Pranapada", "--sl-pp", false),
        new("PS", "Punya Saham", "--sl-ps", false),
        new("GK", "Gulika · Maandi", "--sl-gk", false),
    ];
}
