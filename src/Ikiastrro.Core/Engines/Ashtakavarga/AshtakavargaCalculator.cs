using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Ashtakavarga;

/// <summary>
/// Parāśari Ashtakavarga over a D1 chart — the seven Bhinnāṣṭakavargas, the Sarvāṣṭakavarga,
/// Ṭrikoṇa + Ekādhipatya Śodhana, and the Rāśi / Graha / Sodhya Piṇḍa reductions.
///
/// Pure and deterministic: it reads only the natal sign of the seven grahas and the Lagna from
/// the D1 <see cref="ChartAnalysisInput"/>. The bindu matrix and every multiplier live in
/// <see cref="AshtakavargaTables"/>, the verified mirror of the seeded rule rows.
/// </summary>
public static class AshtakavargaCalculator
{
    public static AshtakavargaResult Calculate(ChartAnalysisInput d1)
    {
        var natalSignIndex = NatalSignIndices(d1);
        var occupied = AshtakavargaTables.Grahas
            .Select(g => natalSignIndex[AshtakavargaTables.Code(g)])
            .ToHashSet();

        var bhinna = new List<BhinnaAshtakavarga>(7);
        var contributions = new List<AshtakavargaContributionCell>(7 * 8 * 12);

        foreach (var recipient in AshtakavargaTables.Recipients)
        {
            var bindus = new int[12];
            foreach (var contributor in AshtakavargaTables.Contributors)
            {
                var contributorSign = natalSignIndex[contributor];
                var beneficOffsets = AshtakavargaTables.BeneficPlaces[recipient][contributor];
                for (var sign = 0; sign < 12; sign++)
                {
                    var offset = ((sign - contributorSign) % 12 + 12) % 12 + 1; // 1..12 from the contributor's sign
                    var isBindu = Array.IndexOf(beneficOffsets, offset) >= 0;
                    if (isBindu) bindus[sign]++;
                    contributions.Add(new AshtakavargaContributionCell(
                        recipient, contributor, sign + 1, isBindu, contributorSign + 1));
                }
            }
            bhinna.Add(new BhinnaAshtakavarga(recipient, bindus, bindus.Sum()));
        }

        var sav = new int[12];
        foreach (var b in bhinna)
            for (var s = 0; s < 12; s++)
                sav[s] += b.Bindus[s];

        var reduced = bhinna
            .Select(b => new BhinnaAshtakavarga(
                b.Recipient, EkadhipatyaSodhana(TrikonaSodhana(b.Bindus), occupied), 0))
            .Select(b => b with { Total = b.Bindus.Sum() })
            .ToList();

        var pinda = new List<AshtakavargaPinda>(7);
        foreach (var b in reduced)
        {
            var rasiPinda = 0;
            for (var s = 0; s < 12; s++) rasiPinda += b.Bindus[s] * AshtakavargaTables.Rasimana[s];

            var grahaPinda = 0;
            foreach (var g in AshtakavargaTables.Grahas)
            {
                var code = AshtakavargaTables.Code(g);
                grahaPinda += AshtakavargaTables.Grahamana[code] * b.Bindus[natalSignIndex[code]];
            }

            pinda.Add(new AshtakavargaPinda(b.Recipient, rasiPinda, grahaPinda, rasiPinda + grahaPinda));
        }

        return new AshtakavargaResult(bhinna, reduced, new SarvaAshtakavarga(sav), contributions, pinda);
    }

    /// <summary>Sign index (0 = Aries) of every contributor: the seven grahas plus "LAGNA".</summary>
    private static IReadOnlyDictionary<string, int> NatalSignIndices(ChartAnalysisInput d1)
    {
        var index = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["LAGNA"] = (int)d1.AscendantSign,
        };

        foreach (var g in AshtakavargaTables.Grahas)
        {
            var name = g.ToString();
            var pos = d1.Planets.FirstOrDefault(p =>
                p.PointKind == "Graha" && string.Equals(p.Planet, name, StringComparison.OrdinalIgnoreCase));
            if (pos is null)
                throw new InvalidOperationException($"Ashtakavarga: D1 chart has no '{name}' graha position.");
            index[AshtakavargaTables.Code(g)] = ParseSignIndex(pos.Sign);
        }

        return index;
    }

    private static int ParseSignIndex(string sign)
    {
        if (Enum.TryParse<ZodiacName>(sign, ignoreCase: true, out var z))
            return (int)z;
        throw new InvalidOperationException($"Ashtakavarga: unrecognised sign name '{sign}'.");
    }

    /// <summary>
    /// Ṭrikoṇa Śodhana on one Bhinnāṣṭakavarga. Per trine: any member zero → no change;
    /// all three equal → all zero; otherwise subtract the lowest from all three.
    /// </summary>
    public static int[] TrikonaSodhana(IReadOnlyList<int> bindus)
    {
        var r = bindus.ToArray();
        foreach (var trine in AshtakavargaTables.Trines)
        {
            int a = r[trine[0]], b = r[trine[1]], c = r[trine[2]];
            if (a == 0 || b == 0 || c == 0) continue;
            if (a == b && b == c)
            {
                r[trine[0]] = r[trine[1]] = r[trine[2]] = 0;
            }
            else
            {
                var min = Math.Min(a, Math.Min(b, c));
                r[trine[0]] -= min;
                r[trine[1]] -= min;
                r[trine[2]] -= min;
            }
        }
        return r;
    }

    /// <summary>
    /// Ekādhipatya Śodhana on a post-Ṭrikoṇa Bhinnāṣṭakavarga, using D1 sign occupancy.
    /// Per single-lordship pair: either member zero, or both occupied → no change;
    /// both empty → unequal ⇒ both take the lower value, equal ⇒ both zero;
    /// one occupied ⇒ the empty member: ≤ occupied ⇒ 0, &gt; occupied ⇒ = occupied.
    /// </summary>
    public static int[] EkadhipatyaSodhana(IReadOnlyList<int> bindusAfterTrikona, IReadOnlySet<int> occupiedSigns)
    {
        var r = bindusAfterTrikona.ToArray();
        foreach (var (a, b) in AshtakavargaTables.EkadhipatyaPairs)
        {
            var occA = occupiedSigns.Contains(a);
            var occB = occupiedSigns.Contains(b);
            if (r[a] == 0 || r[b] == 0 || (occA && occB)) continue;

            if (!occA && !occB)
            {
                if (r[a] != r[b]) { var min = Math.Min(r[a], r[b]); r[a] = min; r[b] = min; }
                else { r[a] = 0; r[b] = 0; }
            }
            else if (occA) // b is empty
            {
                r[b] = r[b] <= r[a] ? 0 : r[a];
            }
            else // a is empty
            {
                r[a] = r[a] <= r[b] ? 0 : r[b];
            }
        }
        return r;
    }
}
