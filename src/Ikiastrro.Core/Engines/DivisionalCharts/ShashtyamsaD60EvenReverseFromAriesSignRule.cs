using Ikiastrro.Core.Models;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.DivisionalCharts;

/// <summary>
/// D60 Shashtyamsa counted from Aries, even signs reversed - Jagannatha Hora's D60 as tagged
/// "D-60 (RvAr)" in its export (PyJHora shashtyamsa_chart method 3, "Parasara shashtyamsa even
/// reversal (from Aries)" = parivritti alternate). Odd (1-indexed) signs count the l-th part
/// forward from Aries; even signs count it backward from Pisces - the same odd-forward /
/// even-reversed reading the 60 Shashtiamsa deity names use (ShashtiamsaDeityTable):
///
///   odd 1-indexed sign (r even):  vargaSign = l mod 12
///   even 1-indexed sign (r odd):  vargaSign = (11 - l) mod 12
///
/// l = floor(degreesInSign * 2). Verified against all 68 bodies of the RamakrishnanP
/// "Rasis occupied in all vargas" export (2026-09-23), decision 007.
/// </summary>
public sealed class ShashtyamsaD60EvenReverseFromAriesSignRule : IVargaSignRule
{
    public ZodiacName SignFor(double siderealLongitude)
    {
        var lon = AstroMath.Normalize(siderealLongitude);
        var r = (int)(lon / 30);
        var l = (int)((lon % 30) * 2);
        var idx = r % 2 == 0 ? l : 11 - l;
        return (ZodiacName)((idx % 12 + 12) % 12);
    }
}
