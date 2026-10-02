using Ikiastrro.Core.Models;
using Ikiastrro.Core.Engines.Astronomy;

namespace Ikiastrro.Core.Engines.DivisionalCharts;

/// <summary>
/// D3 Uma Shambu Drekkana - Jagannatha Hora's default D3 (tagged "D-3 (US)" in its export,
/// "Re-interpreted Parasara Drekkana (Uma-Shambhu)" in its Divisional Chart Calculation Options).
/// The first drekkana of rasi r (0-indexed) falls in r + 4 * ceil(r / 2) — the cycle
/// Ar, Vi, Li, Pi — and odd (1-indexed) signs then run forward, even signs backward:
///
///   start = (r + 4 * ((r + 1) / 2)) mod 12
///   odd 1-indexed sign (r even):  vargaSign = (start + l) mod 12
///   even 1-indexed sign (r odd):  vargaSign = (start - l) mod 12
///
/// l = 0, 1, 2 for 0-10, 10-20, 20-30 deg. Verified against all 67 bodies of the
/// RamakrishnanP "Rasis occupied in all vargas" export (2026-09-23).
/// </summary>
public sealed class DrekkanaD3UmaShambuSignRule : IVargaSignRule
{
    public ZodiacName SignFor(double siderealLongitude)
    {
        var lon = AstroMath.Normalize(siderealLongitude);
        var r = (int)(lon / 30);
        var l = (int)((lon % 30) / 10);
        var start = r + 4 * ((r + 1) / 2);
        var idx = r % 2 == 0 ? start + l : start - l;
        return (ZodiacName)((idx % 12 + 12) % 12);
    }
}
