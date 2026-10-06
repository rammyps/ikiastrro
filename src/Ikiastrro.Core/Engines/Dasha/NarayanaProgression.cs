using Ikiastrro.Core.Engines.Astronomy;
using Ikiastrro.Core.Engines.Houses;
using Ikiastrro.Core.Engines.Karakas;
using Ikiastrro.Core.Pipeline;

namespace Ikiastrro.Core.Engines.Dasha;

/// <summary>Which of PVR's three progression tables applies to the dasa seed.</summary>
public enum NarayanaVariant { Normal, Saturn, Ketu }

/// <summary>
/// The Narayana dasa seed and the order of its twelve rasi dasas. <see cref="Comparison"/> is the Lagna against the
/// 7th with the sec.15.5.2 rule that decided the stronger; <see cref="Seed"/> is that stronger rasi.
/// <see cref="Forward"/> is the direction of counting; <see cref="Sequence"/> is the twelve rasis in dasa order.
/// </summary>
public sealed record NarayanaReading(
    RasiStrengthReading Comparison, ZodiacName Seed, NarayanaVariant Variant, bool Forward, string Pattern,
    IReadOnlyList<ZodiacName> Sequence);

/// <summary>
/// Narayana dasa's dasa seed and progression, P.V.R. Narasimha Rao, <i>Vedic Astrology: An Integrated Approach</i>,
/// sec.18.2.1, printed pp.231-235 (<c>SRC_PVR_INTEGRATED</c>), and the stronger-rasi rules of sec.15.5.2 it uses.
/// <list type="bullet">
/// <item><b>Dasa seed</b>: the stronger of the Lagna and the 7th (Narayana always compares a rasi with the 7th from
/// it, so the natural-strength rule 5 never decides).</item>
/// <item><b>Pattern by the seed's kind</b>: a movable seed counts regularly (1st, 2nd, 3rd, Brahma); a fixed seed
/// takes every 6th (Shiva); a dual seed takes trines from the seed, then from the 10th, the 7th and the 4th
/// (Vishnu).</item>
/// <item><b>Direction</b>: forward when the 9th from the seed is an odd-footed sign (Ar Ta Ge Li Sc Sg), backward
/// when it is even-footed (Cn Le Vi Cp Aq Pi).</item>
/// <item><b>Saturn in the seed</b>: regular and forward. <b>Ketu in the seed</b>: the direction is reversed.</item>
/// </list>
/// Checked against all 36 sequences the book prints (12 seeds, normal, Saturn, Ketu). Where both Saturn and Ketu
/// are in the seed the book does not say; Saturn is applied first. Dasa lengths (years) are not computed here.
/// Pure; no I/O.
/// </summary>
public static class NarayanaProgression
{
    public static NarayanaReading? Read(ChartAnalysisInput chart)
    {
        var signOf = chart.Planets
            .Where(p => p.Planet != "Ascendant" && Enum.TryParse<PlanetName>(p.Planet, out _))
            .ToDictionary(p => Enum.Parse<PlanetName>(p.Planet), p => Enum.Parse<ZodiacName>(p.Sign));
        if (signOf.Count != 9) return null;

        var lagna = chart.AscendantSign;
        var comparison = StrongerRasiComparator.Explain(lagna, HouseEngine.GetHouseSign(lagna, 7), chart);
        var seed = comparison.Stronger;

        var variant = signOf[PlanetName.Saturn] == seed ? NarayanaVariant.Saturn
            : signOf[PlanetName.Ketu] == seed ? NarayanaVariant.Ketu
            : NarayanaVariant.Normal;
        var (sequence, forward, pattern) = Progression(seed, variant);
        return new(comparison, seed, variant, forward, pattern, sequence);
    }

    /// <summary>The twelve dasa rasis from <paramref name="seed"/> for the given variant.</summary>
    public static (IReadOnlyList<ZodiacName> Sequence, bool Forward, string Pattern) Progression(ZodiacName seed, NarayanaVariant variant)
    {
        var ninth = HouseEngine.GetHouseSign(seed, 9);
        var forward = IsOddFooted(ninth);
        string pattern;
        List<int> steps;   // offsets from the seed, in the direction of counting

        switch (variant)
        {
            case NarayanaVariant.Saturn:
                forward = true;
                (steps, pattern) = (Regular(), "regular (Brahma), forward because Saturn is in the seed");
                break;
            default:
                if (variant == NarayanaVariant.Ketu) forward = !forward;
                (steps, pattern) = BaseSteps(seed);
                if (variant == NarayanaVariant.Ketu) pattern += ", direction reversed because Ketu is in the seed";
                break;
        }

        var dir = forward ? 1 : -1;
        var sequence = steps.Select(k => (ZodiacName)((((int)seed + dir * k) % 12 + 12) % 12)).ToList();
        return (sequence, forward, pattern);
    }

    private static (List<int> Steps, string Pattern) BaseSteps(ZodiacName seed) => BaadhakaCalculator.GetModality(seed) switch
    {
        SignModality.Movable => (Regular(), "regular 1st, 2nd, 3rd (Brahma, movable seed)"),
        SignModality.Fixed => (Enumerable.Range(0, 12).Select(i => i * 5 % 12).ToList(), "every 6th (Shiva, fixed seed)"),
        _ => (Trinal(), "trines from the seed, then from the 10th, 7th and 4th (Vishnu, dual seed)"),
    };

    private static List<int> Regular() => Enumerable.Range(0, 12).ToList();

    // 1st, 5th, 9th from the seed, then the same from the 10th (offset 9), the 7th (6) and the 4th (3).
    private static List<int> Trinal() => new[] { 0, 9, 6, 3 }.SelectMany(start => new[] { 0, 4, 8 }.Select(t => (start + t) % 12)).ToList();

    private static bool IsOddFooted(ZodiacName s) =>
        s is ZodiacName.Aries or ZodiacName.Taurus or ZodiacName.Gemini or ZodiacName.Libra or ZodiacName.Scorpio or ZodiacName.Sagittarius;
}
