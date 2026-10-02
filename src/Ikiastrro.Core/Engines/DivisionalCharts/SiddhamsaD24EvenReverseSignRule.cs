using Ikiastrro.Core.Models;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.DivisionalCharts;

/// <summary>
/// D24 Siddhamsa, even signs reversed - Jagannatha Hora's D24 as tagged "D-24 (Rev)" in its
/// export (PyJHora chaturvimsamsa_chart method 2, "Parasara with even sign reversal"). Odd
/// (1-indexed) signs count the l-th part forward from Leo; even signs count backward from Cancer:
///
///   odd 1-indexed sign (r even):  vargaSign = (Leo + l) mod 12
///   even 1-indexed sign (r odd):  vargaSign = (Cancer - l) mod 12
///
/// l = floor(degreesInSign / 1.25). Verified against all 68 bodies of the RamakrishnanP
/// "Rasis occupied in all vargas" export (2026-09-23), decision 007.
/// </summary>
public sealed class SiddhamsaD24EvenReverseSignRule : IVargaSignRule
{
    public ZodiacName SignFor(double siderealLongitude)
    {
        var lon = AstroMath.Normalize(siderealLongitude);
        var r = (int)(lon / 30);
        var l = (int)((lon % 30) / 1.25);
        var idx = r % 2 == 0 ? 4 + l : 3 - l;   // 4 = Leo, 3 = Cancer
        return (ZodiacName)((idx % 12 + 12) % 12);
    }
}
