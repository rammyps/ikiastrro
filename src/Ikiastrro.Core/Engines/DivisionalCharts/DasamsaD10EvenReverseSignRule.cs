using Ikiastrro.Core.Models;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.DivisionalCharts;

/// <summary>
/// D10 Dasamsa, even signs reversed - Jagannatha Hora's D10 as tagged "D-10 (5-8)" in its export
/// (PyJHora dasamsa_chart method 3, "start from reverse 9th and go backward"). Odd (1-indexed)
/// signs count the l-th part forward from the sign itself, as Parasara; even signs start from the
/// 9th counted backward (= the 5th forward) and count backward:
///
///   odd 1-indexed sign (r even):  vargaSign = (r + l) mod 12
///   even 1-indexed sign (r odd):  vargaSign = (r - 8 - l) mod 12
///
/// l = floor(degreesInSign / 3). Verified against all 68 bodies of the RamakrishnanP
/// "Rasis occupied in all vargas" export (2026-09-23), decision 007.
/// </summary>
public sealed class DasamsaD10EvenReverseSignRule : IVargaSignRule
{
    public ZodiacName SignFor(double siderealLongitude)
    {
        var lon = AstroMath.Normalize(siderealLongitude);
        var r = (int)(lon / 30);
        var l = (int)((lon % 30) / 3.0);
        var idx = r % 2 == 0 ? r + l : r - 8 - l;
        return (ZodiacName)((idx % 12 + 12) % 12);
    }
}
