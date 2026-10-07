using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>One sphuṭa: its code, name and D1 sidereal longitude.</summary>
public sealed record SphutaRow(string Code, string Name, double Longitude)
{
    public ZodiacName Sign => AstroMath.GetSignAtLongitude(Longitude);
}

/// <summary>
/// The thirteen sphuṭas JHora's "Copy complete calculations" lists after the upagrahas — Prāṇa, Deha, Mṛtyu,
/// Sūkṣma Tri-, Tithi, Yoga, Rāhu Tithi, Kṣetra, Bīja, Tri-, Catus-, Pañca-sphuṭa and the Yogi/Avayogi pair.
/// Each is a sum of whole multiples of a few points mod 360°; Gulika is the upagraha longitude:
/// <list type="bullet">
/// <item>Prāṇa = 5 × Lagna + Gulika · Deha = 8 × Moon + Gulika · Mṛtyu = 7 × Gulika + Sun · Sūkṣma Tri = their sum.</item>
/// <item>Tri = Moon + Lagna + Gulika · Catus = Sun + Tri · Pañca = Rāhu + Catus.</item>
/// <item>Tithi = Moon − Sun · Rāhu Tithi = Rāhu − Sun · Yoga = Sun + Moon · Kṣetra = Moon + Jupiter + Mars · Bīja = Sun + Jupiter + Venus.</item>
/// <item>Yogi = Sun + Moon + 93°20′ · Avayogi = Yogi + 186°40′ (the same points as <see cref="YogiAvayogi"/>).</item>
/// </list>
/// Formulas from PyJHora's <c>sphuta.py</c> (AGPL — formulas only). All thirteen match JHora's export for a second
/// 1_Ramakrishnan chart to under 0.1″. Kuṇḍa is not here: no formula found that fits JHora's five reference charts.
/// </summary>
public static class Sphutas
{
    public static IReadOnlyList<SphutaRow> Compute(
        double lagna, double sun, double moon, double mars, double jupiter, double venus, double rahu, double gulika)
    {
        var prana = AstroMath.Normalize(5 * lagna + gulika);
        var deha = AstroMath.Normalize(8 * moon + gulika);
        var mrityu = AstroMath.Normalize(7 * gulika + sun);
        var tri = AstroMath.Normalize(moon + lagna + gulika);
        var catus = AstroMath.Normalize(sun + tri);
        var yogi = AstroMath.Normalize(sun + moon + 93 + 1 / 3.0);

        return new SphutaRow[]
        {
            new("PRANA", "Prana Sphuta", prana),
            new("DEHA", "Deha Sphuta", deha),
            new("MRITYU", "Mrityu Sphuta", mrityu),
            new("SOOKSHMA_TRI", "Sookshma Tri Sphuta", AstroMath.Normalize(prana + deha + mrityu)),
            new("TITHI", "Tithi Sphuta", AstroMath.Normalize(moon - sun)),
            new("YOGA_SUN_MOON", "Yoga Sphuta (Sun-Moon)", AstroMath.Normalize(sun + moon)),
            new("RAHU_TITHI", "Rahu Tithi Sphuta", AstroMath.Normalize(rahu - sun)),
            new("KSHETRA", "Kshetra Sphuta", AstroMath.Normalize(moon + jupiter + mars)),
            new("BEEJA", "Beeja Sphuta", AstroMath.Normalize(sun + jupiter + venus)),
            new("TRI", "Tri Sphuta", tri),
            new("CHATUS", "Chatus Sphuta", catus),
            new("PANCHA", "Pancha Sphuta", AstroMath.Normalize(rahu + catus)),
            new("YOGI", "Yoga Sphuta", yogi),
            new("AVAYOGI", "Avayoga Sphuta", AstroMath.Normalize(yogi + 186 + 2 / 3.0)),
        };
    }
}
