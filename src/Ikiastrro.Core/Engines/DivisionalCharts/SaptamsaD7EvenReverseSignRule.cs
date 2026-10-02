using Ikiastrro.Core.Models;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.DivisionalCharts;

/// <summary>
/// D7 Saptamsa, even signs reversed - Jagannatha Hora's default D7 (tagged "D-7 (7-1)" in its
/// export). Odd (1-indexed) signs count the l-th part forward from the sign itself, as
/// Parasara; even signs start from the 7th from the sign and count backward:
///
///   odd 1-indexed sign (r even):  vargaSign = (r + l) mod 12
///   even 1-indexed sign (r odd):  vargaSign = (r + 6 - l) mod 12
///
/// l = floor(degreesInSign / (30/7)). Verified against all 67 bodies of the RamakrishnanP
/// "Rasis occupied in all vargas" export (2026-09-23).
/// </summary>
public sealed class SaptamsaD7EvenReverseSignRule : IVargaSignRule
{
    public ZodiacName SignFor(double siderealLongitude)
    {
        var lon = AstroMath.Normalize(siderealLongitude);
        var r = (int)(lon / 30);
        var l = (int)((lon % 30) / (30.0 / 7));
        var idx = r % 2 == 0 ? r + l : r + 6 - l;
        return (ZodiacName)((idx % 12 + 12) % 12);
    }
}
