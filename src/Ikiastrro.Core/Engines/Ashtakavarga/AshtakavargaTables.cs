using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.Ashtakavarga;

/// <summary>
/// Parāśari Ashtakavarga reference tables — the verified mirror of the rows seeded in
/// <c>db/075_seed_ashtakavarga_rules_and_benchmark.sql</c> / <c>db/077_*</c>
/// (<c>tbl_Rule_AshtakavargaContribution</c> / <c>tbl_Rule_AshtakavargaReduction</c>).
///
/// The benefic-places matrix is the classical BPHS table; the Moon and Venus rows carry the
/// standard Parāśari corrections (Moon benefic in the 9th from Moon and the 2nd from Jupiter,
/// malefic in the 9th from Mars and the 12th from Jupiter; Venus benefic in the 4th from Mars,
/// malefic in the 5th) — the variant Jagannatha Hora uses. Per-recipient bindu totals
/// 48/49/39/54/56/52/39 → a Sarvāṣṭakavarga grand total of 337. The rāśimāna / grahamāna
/// multipliers and the Ṭrikoṇa / Ekādhipatya rules are likewise the variant that reproduces
/// the Jagannatha Hora export for <c>1_Ramakrishnan</c> exactly (see <c>verify-ashtakavarga</c>).
/// </summary>
public static class AshtakavargaTables
{
    /// <summary>Recipient codes, in bindu-total order (Sun … Saturn). Lagna is a contributor only.</summary>
    public static readonly string[] Recipients =
        { "SUN", "MOON", "MARS", "MERCURY", "JUPITER", "VENUS", "SATURN" };

    /// <summary>Contributor codes — the seven grahas plus the Lagna.</summary>
    public static readonly string[] Contributors =
        { "SUN", "MOON", "MARS", "MERCURY", "JUPITER", "VENUS", "SATURN", "LAGNA" };

    public static string Code(PlanetName p) => p switch
    {
        PlanetName.Sun => "SUN",
        PlanetName.Moon => "MOON",
        PlanetName.Mars => "MARS",
        PlanetName.Mercury => "MERCURY",
        PlanetName.Jupiter => "JUPITER",
        PlanetName.Venus => "VENUS",
        PlanetName.Saturn => "SATURN",
        _ => throw new ArgumentOutOfRangeException(nameof(p), p, "Not one of the seven Ashtakavarga grahas.")
    };

    public static readonly PlanetName[] Grahas =
        { PlanetName.Sun, PlanetName.Moon, PlanetName.Mars, PlanetName.Mercury,
          PlanetName.Jupiter, PlanetName.Venus, PlanetName.Saturn };

    /// <summary>
    /// <c>BeneficPlaces[recipient][contributor]</c> = the 1-based house offsets, counted from the
    /// contributor's natal sign, that earn the recipient one bindu.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, int[]>> BeneficPlaces =
        new Dictionary<string, IReadOnlyDictionary<string, int[]>>
        {
            ["SUN"] = new Dictionary<string, int[]>
            {
                ["SUN"] = new[] { 1, 2, 4, 7, 8, 9, 10, 11 },
                ["MOON"] = new[] { 3, 6, 10, 11 },
                ["MARS"] = new[] { 1, 2, 4, 7, 8, 9, 10, 11 },
                ["MERCURY"] = new[] { 3, 5, 6, 9, 10, 11, 12 },
                ["JUPITER"] = new[] { 5, 6, 9, 11 },
                ["VENUS"] = new[] { 6, 7, 12 },
                ["SATURN"] = new[] { 1, 2, 4, 7, 8, 9, 10, 11 },
                ["LAGNA"] = new[] { 3, 4, 6, 10, 11, 12 },
            },
            ["MOON"] = new Dictionary<string, int[]>
            {
                ["SUN"] = new[] { 3, 6, 7, 8, 10, 11 },
                ["MOON"] = new[] { 1, 3, 6, 7, 9, 10, 11 },   // + 9 from Moon (Parāśari)
                ["MARS"] = new[] { 2, 3, 5, 6, 10, 11 },       // − 9 from Mars (Parāśari)
                ["MERCURY"] = new[] { 1, 3, 4, 5, 7, 8, 10, 11 },
                ["JUPITER"] = new[] { 1, 2, 4, 7, 8, 10, 11 }, // + 2, − 12 from Jupiter (Parāśari)
                ["VENUS"] = new[] { 3, 4, 5, 7, 9, 10, 11 },
                ["SATURN"] = new[] { 3, 5, 6, 11 },
                ["LAGNA"] = new[] { 3, 6, 10, 11 },
            },
            ["MARS"] = new Dictionary<string, int[]>
            {
                ["SUN"] = new[] { 3, 5, 6, 10, 11 },
                ["MOON"] = new[] { 3, 6, 11 },
                ["MARS"] = new[] { 1, 2, 4, 7, 8, 10, 11 },
                ["MERCURY"] = new[] { 3, 5, 6, 11 },
                ["JUPITER"] = new[] { 6, 10, 11, 12 },
                ["VENUS"] = new[] { 6, 8, 11, 12 },
                ["SATURN"] = new[] { 1, 4, 7, 8, 9, 10, 11 },
                ["LAGNA"] = new[] { 1, 3, 6, 10, 11 },
            },
            ["MERCURY"] = new Dictionary<string, int[]>
            {
                ["SUN"] = new[] { 5, 6, 9, 11, 12 },
                ["MOON"] = new[] { 2, 4, 6, 8, 10, 11 },
                ["MARS"] = new[] { 1, 2, 4, 7, 8, 9, 10, 11 },
                ["MERCURY"] = new[] { 1, 3, 5, 6, 9, 10, 11, 12 },
                ["JUPITER"] = new[] { 6, 8, 11, 12 },
                ["VENUS"] = new[] { 1, 2, 3, 4, 5, 8, 9, 11 },
                ["SATURN"] = new[] { 1, 2, 4, 7, 8, 9, 10, 11 },
                ["LAGNA"] = new[] { 1, 2, 4, 6, 8, 10, 11 },
            },
            ["JUPITER"] = new Dictionary<string, int[]>
            {
                ["SUN"] = new[] { 1, 2, 3, 4, 7, 8, 9, 10, 11 },
                ["MOON"] = new[] { 2, 5, 7, 9, 11 },
                ["MARS"] = new[] { 1, 2, 4, 7, 8, 10, 11 },
                ["MERCURY"] = new[] { 1, 2, 4, 5, 6, 9, 10, 11 },
                ["JUPITER"] = new[] { 1, 2, 3, 4, 7, 8, 10, 11 },
                ["VENUS"] = new[] { 2, 5, 6, 9, 10, 11 },
                ["SATURN"] = new[] { 3, 5, 6, 12 },
                ["LAGNA"] = new[] { 1, 2, 4, 5, 6, 7, 9, 10, 11 },
            },
            ["VENUS"] = new Dictionary<string, int[]>
            {
                ["SUN"] = new[] { 8, 11, 12 },
                ["MOON"] = new[] { 1, 2, 3, 4, 5, 8, 9, 11, 12 },
                ["MARS"] = new[] { 3, 4, 6, 9, 11, 12 },        // + 4, − 5 from Mars (Parāśari)
                ["MERCURY"] = new[] { 3, 5, 6, 9, 11 },
                ["JUPITER"] = new[] { 5, 8, 9, 10, 11 },
                ["VENUS"] = new[] { 1, 2, 3, 4, 5, 8, 9, 10, 11 },
                ["SATURN"] = new[] { 3, 4, 5, 8, 9, 10, 11 },
                ["LAGNA"] = new[] { 1, 2, 3, 4, 5, 8, 9, 11 },
            },
            ["SATURN"] = new Dictionary<string, int[]>
            {
                ["SUN"] = new[] { 1, 2, 4, 7, 8, 10, 11 },
                ["MOON"] = new[] { 3, 6, 11 },
                ["MARS"] = new[] { 3, 5, 6, 10, 11, 12 },
                ["MERCURY"] = new[] { 6, 8, 9, 10, 11, 12 },
                ["JUPITER"] = new[] { 5, 6, 11, 12 },
                ["VENUS"] = new[] { 6, 11, 12 },
                ["SATURN"] = new[] { 3, 5, 6, 11 },
                ["LAGNA"] = new[] { 1, 3, 4, 6, 10, 11 },
            },
        };

    /// <summary>The four Ṭrikoṇa (trine) groups as 0-based sign indices.</summary>
    public static readonly int[][] Trines =
        { new[] { 0, 4, 8 }, new[] { 1, 5, 9 }, new[] { 2, 6, 10 }, new[] { 3, 7, 11 } };

    /// <summary>
    /// The five Ekādhipatya (single-lordship) sign pairs as 0-based sign indices —
    /// (Ar,Sc) Mars, (Ta,Li) Venus, (Ge,Vi) Mercury, (Sg,Pi) Jupiter, (Cp,Aq) Saturn.
    /// Cancer (Moon) and Leo (Sun) are single-lord and excluded.
    /// </summary>
    public static readonly (int A, int B)[] EkadhipatyaPairs =
        { (0, 7), (1, 6), (2, 5), (8, 11), (9, 10) };

    /// <summary>Rāśimāna multipliers, Aries…Pisces — the JHora / Horosoft variant (Virgo = 5).</summary>
    public static readonly int[] Rasimana = { 7, 10, 8, 4, 10, 5, 7, 8, 9, 5, 11, 12 };

    /// <summary>Grahamāna multipliers, Sun…Saturn.</summary>
    public static readonly IReadOnlyDictionary<string, int> Grahamana = new Dictionary<string, int>
    {
        ["SUN"] = 5, ["MOON"] = 5, ["MARS"] = 8, ["MERCURY"] = 5,
        ["JUPITER"] = 10, ["VENUS"] = 7, ["SATURN"] = 5,
    };
}
