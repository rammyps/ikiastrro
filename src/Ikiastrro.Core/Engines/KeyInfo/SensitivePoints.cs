using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.KeyInfo;

/// <summary>
/// Bhṛgu Bindu and Varṇada lagnas.
/// <list type="bullet">
/// <item>Bhṛgu Bindu: the midpoint going forward from Rahu to the Moon. JHora 1_Ramakrishnan: 10°03′ Virgo.</item>
/// <item>Varṇada (BPHS ch. 6, the rule JHora applies): for the Lagna and the Hora Lagna, count from
/// Aries forward when the sign is odd, from Pisces backward when even. Same parity → add the counts,
/// otherwise take the difference (0 counts as 12). Count that number from Aries forward if the Lagna
/// is odd, from Pisces backward if even. V2–V12 do the same from the 2nd–12th sign of each lagna.
/// JHora keeps the Lagna's degree in the Varṇada sign; so does this. JHora 1_Ramakrishnan: V1 Pisces,
/// V2 Gemini, V3 Scorpio.</item>
/// </list>
/// </summary>
public static class SensitivePoints
{
    public static double BhriguBindu(double rahuLongitude, double moonLongitude) =>
        AstroMath.Normalize(rahuLongitude + AstroMath.Normalize(moonLongitude - rahuLongitude) / 2);

    /// <summary>Varṇada lagna of house <paramref name="house"/> (1 = V1).</summary>
    public static double Varnada(double lagnaLongitude, double horaLagnaLongitude, int house)
    {
        var lagnaSign = ((int)(AstroMath.Normalize(lagnaLongitude) / 30) + house - 1) % 12;
        var horaSign = ((int)(AstroMath.Normalize(horaLagnaLongitude) / 30) + house - 1) % 12;

        var lagnaCount = Count(lagnaSign);
        var horaCount = Count(horaSign);
        var combined = IsOdd(lagnaSign) == IsOdd(horaSign)
            ? (lagnaCount + horaCount) % 12
            : Math.Abs(lagnaCount - horaCount) % 12;
        if (combined == 0) combined = 12;

        var varnadaSign = IsOdd(lagnaSign) ? combined - 1 : 12 - combined;
        var degree = AstroMath.Normalize(lagnaLongitude) % 30;
        return varnadaSign * 30 + degree;
    }

    // Aries, Gemini, … are odd signs (index 0, 2, …).
    private static bool IsOdd(int sign) => sign % 2 == 0;

    // Odd: Aries = 1 forward. Even: Pisces = 1 backward.
    private static int Count(int sign) => IsOdd(sign) ? sign + 1 : 12 - sign;
}
