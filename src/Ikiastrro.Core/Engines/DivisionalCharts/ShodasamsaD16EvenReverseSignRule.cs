using Ikiastrro.Core.Models;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.DivisionalCharts;

/// <summary>
/// D16 Shodasamsa, even signs reversed - Jagannatha Hora's D16 as tagged "D-16 (Rev)" in its
/// export. The base is the Parasara one (movable Aries, fixed Leo, dual Sagittarius); odd
/// (1-indexed) signs count the l-th part forward from it, even signs run the same 16 parts in
/// reverse, so the first part falls where the 16th would and the count goes backward:
///
///   base = Aries / Leo / Sagittarius for r mod 3 = 0 / 1 / 2
///   odd 1-indexed sign (r even):  vargaSign = (base + l) mod 12
///   even 1-indexed sign (r odd):  vargaSign = (base + 15 - l) mod 12
///
/// l = floor(degreesInSign / (30/16)). Verified against all 68 bodies of the RamakrishnanP
/// "Rasis occupied in all vargas" export (2026-09-23), decision 007.
/// </summary>
public sealed class ShodasamsaD16EvenReverseSignRule : IVargaSignRule
{
    public ZodiacName SignFor(double siderealLongitude)
    {
        var lon = AstroMath.Normalize(siderealLongitude);
        var r = (int)(lon / 30);
        var l = (int)((lon % 30) / (30.0 / 16));
        var b = (r % 3) switch { 0 => 0, 1 => 4, _ => 8 };
        var idx = r % 2 == 0 ? b + l : b + 15 - l;
        return (ZodiacName)((idx % 12 + 12) % 12);
    }
}
