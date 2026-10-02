using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>A body's Mrityu bhāga and Puṣkara standing.</summary>
public sealed record MrityuPushkaraRow(string Body, double FromMrityuBhagaCentre, bool InMrityuBhaga,
    bool InPushkaramsa, bool InPushkaraBhaga, double FromPushkaraBhagaCentre);

/// <summary>
/// Mrityu bhāga and Puṣkara (navāṃśa and bhāga). Each is a single degree per sign; a body "is in" it
/// when it falls inside that degree (≤ 0.5° from its centre), and the distance is to the centre.
/// <list type="bullet">
/// <item>Mrityu bhāga degrees per sign for Sun–Ketu, Māndi and Lagna (BPHS; PyJHora
/// <c>mrityu_bhaga_base_longitudes</c>). JHora matches for 1_Ramakrishnan except the Moon in Scorpio
/// (table 14°, JHora ≈ 23°).</item>
/// <item>Puṣkara navāṃśas: fire signs the 7th and 9th navāṃśa, earth the 3rd and 5th, air the 6th and
/// 8th, water the 1st and 3rd.</item>
/// <item>Puṣkara bhāga per sign from Jātaka Pārijāta — what JHora uses (all rows match).</item>
/// </list>
/// </summary>
public static class MrityuPushkara
{
    // Rows Aries..Pisces; columns Sun, Moon, Mars, Mercury, Jupiter, Venus, Saturn, Rahu, Ketu, Māndi, Lagna.
    private static readonly int[,] MrityuBhaga =
    {
        { 20, 26, 19, 15, 19, 28, 10, 14,  8, 23,  1 },
        {  9, 12, 28, 14, 29, 15,  4, 13, 18, 24,  9 },
        { 12, 13, 25, 13, 12, 11,  7, 12, 20, 11, 22 },
        {  6, 25, 23, 12, 27, 17,  9, 11, 10, 12, 22 },
        {  8, 24, 29,  8,  6, 10, 12, 24, 21, 13, 25 },
        { 24, 11, 28, 18,  4, 13, 16, 23, 22, 14,  2 },
        { 16, 26, 14, 20, 13,  4,  3, 22, 23,  8,  4 },
        { 17, 14, 21, 10, 10,  6, 18, 21, 24, 18, 23 },
        { 22, 13,  2, 21, 17, 27, 28, 10, 11, 20, 18 },
        {  2, 25, 15, 22, 11, 12, 14, 20, 12, 10, 20 },
        {  3,  5, 11,  7, 15, 29, 13, 18, 13, 21, 24 },
        { 23, 12,  6,  5, 28, 19, 15,  8, 14, 22, 10 },
    };

    private static readonly int[] PushkaraBhaga = { 21, 14, 18, 8, 19, 9, 24, 11, 23, 14, 19, 9 };

    /// <summary>Column for a body: a planet name, "Maandi"/"Mandi" or "Lagna"/"Ascendant"; null if none.</summary>
    public static int? Column(string body) => body switch
    {
        "Maandi" or "Mandi" => 9,
        "Lagna" or "Ascendant" => 10,
        _ => Enum.TryParse<PlanetName>(body, out var p) && (int)p <= (int)PlanetName.Ketu ? (int)p : null,
    };

    public static MrityuPushkaraRow? For(string body, double longitude)
    {
        if (Column(body) is not { } column) return null;
        var lon = AstroMath.Normalize(longitude);
        var sign = (int)(lon / 30);
        var degree = lon - sign * 30;

        var mrityu = Math.Abs(degree - (MrityuBhaga[sign, column] - 0.5));
        var pushkara = Math.Abs(degree - (PushkaraBhaga[sign] - 0.5));
        return new MrityuPushkaraRow(body, mrityu, mrityu <= 0.5, InPushkaramsa(sign, degree), pushkara <= 0.5, pushkara);
    }

    /// <summary>The two Puṣkara navāṃśas of a sign, by element (fire, earth, air, water repeat from Aries).</summary>
    public static bool InPushkaramsa(int sign, double degreeInSign)
    {
        var navamsa = (int)(degreeInSign / (30 / 9.0)) + 1;
        return (sign % 4) switch
        {
            0 => navamsa is 7 or 9,
            1 => navamsa is 3 or 5,
            2 => navamsa is 6 or 8,
            _ => navamsa is 1 or 3,
        };
    }
}
